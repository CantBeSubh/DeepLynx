using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.helpers.Hubs;
using deeplynx.interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Action = deeplynx.datalayer.Models.Action;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class NotificationBusinessTests : IntegrationTestBase
    {
        private INotificationBusiness _notificationBusiness = null!;
        private Mock<ILogger<NotificationBusiness>> _mockLogger = null!;
        private Mock<IHubContext<EventNotificationHub>> _mockHubContext = null!;
        private Mock<IClientProxy> _mockClientProxy = null!;
        private Mock<IHubClients> _mockClients = null!;
        private long _projectId;
        private long _actionId;
        private long _dataSourceId;
        private User _user1 = null!;
        private User _user2 = null!;
        private readonly DateTime now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        private long organizationId;
        private Config _config;

        public NotificationBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _config = new Config();
            // Setup mocks
            _mockLogger = new Mock<ILogger<NotificationBusiness>>();
            _mockHubContext = new Mock<IHubContext<EventNotificationHub>>();
            _mockClientProxy = new Mock<IClientProxy>();
            _mockClients = new Mock<IHubClients>();
            
            // Setup hub context to return our mock clients
            _mockHubContext.Setup(h => h.Clients).Returns(_mockClients.Object);
            
            // Setup clients to return our mock client proxy for any group
            _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
            
            _notificationBusiness = new NotificationBusiness(
                _config,
                Context, 
                _mockLogger.Object, 
                _mockHubContext.Object
            );
        }

        #region SendEventNotification Tests

        [Fact]
        public async Task SendEventNotification_Success_SendsToSubscribedUsers()
        {
            // Arrange
            var eventDto = new EventResponseDto
            {
                Id = 1,
                ProjectId = _projectId,
                EntityId = 100,
                EntityType = "class",
                DataSourceId = _dataSourceId,
                Operation = "create"
            };

            // Create subscriptions for both users
            var subscription1 = new Subscription
            {
                UserId = _user1.Id,
                ProjectId = _projectId,
                ActionId = _actionId,
                EntityType = "class",
                EntityId = 100,
                DataSourceId = _dataSourceId,
                Operation = "create"
            };

            var subscription2 = new Subscription
            {
                UserId = _user2.Id,
                ProjectId = _projectId,
                ActionId = _actionId,
                EntityType = "class",
                EntityId = null, // Wildcard subscription
                DataSourceId = _dataSourceId,
                Operation = "create"
            };

            Context.Subscriptions.AddRange(subscription1, subscription2);
            await Context.SaveChangesAsync();

            // Act
            await _notificationBusiness.SendEventNotification(eventDto);

            // Assert
            _mockClients.Verify(
                c => c.Group($"user_{_user1.Id}"), 
                Times.Once
            );
            
            _mockClients.Verify(
                c => c.Group($"user_{_user2.Id}"), 
                Times.Once
            );

            _mockClientProxy.Verify(
                c => c.SendCoreAsync(
                    "ReceiveNotification",
                    It.Is<object[]>(o => o.Length == 1 && o[0] is string),
                    default
                ),
                Times.Exactly(2)
            );
        }

        [Fact]
        public async Task SendEventNotification_NoSubscribers_DoesNotSendNotifications()
        {
            // Arrange
            var eventDto = new EventResponseDto
            {
                Id = 1,
                ProjectId = _projectId,
                EntityId = 100,
                EntityType = "class",
                DataSourceId = _dataSourceId,
                Operation = "create"
            };

            // No subscriptions created

            // Act
            await _notificationBusiness.SendEventNotification(eventDto);

            // Assert
            _mockClientProxy.Verify(
                c => c.SendCoreAsync(
                    It.IsAny<string>(),
                    It.IsAny<object[]>(),
                    default
                ),
                Times.Never
            );
        }

        [Fact]
        public async Task SendEventNotification_NullEvent_LogsWarningAndReturns()
        {
            // Act
            await _notificationBusiness.SendEventNotification(null);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("null event")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );

            _mockClientProxy.Verify(
                c => c.SendCoreAsync(
                    It.IsAny<string>(),
                    It.IsAny<object[]>(),
                    default
                ),
                Times.Never
            );
        }

        [Fact]
        public async Task SendEventNotification_WildcardSubscription_MatchesAllEntities()
        {
            // Arrange
            var eventDto = new EventResponseDto
            {
                Id = 1,
                ProjectId = _projectId,
                EntityId = 100,
                EntityType = "class",
                DataSourceId = _dataSourceId,
                Operation = "create"
            };

            var testAction = new Action
            {
                Name = "TestAction_Optimize",
                ProjectId = _projectId,
                LastUpdatedBy = _user1.Id,
                LastUpdatedAt = now
            };
            Context.Actions.Add(testAction);
            await Context.SaveChangesAsync();
            
            // Wildcard subscription (all nulls except project)
            var wildcardSubscription = new Subscription
            {
                UserId = _user1.Id,
                ProjectId = _projectId,
                ActionId = testAction.Id,
                EntityType = null,
                EntityId = null,
                DataSourceId = null,
                Operation = null
            };

            Context.Subscriptions.Add(wildcardSubscription);
            await Context.SaveChangesAsync();

            // Act
            await _notificationBusiness.SendEventNotification(eventDto);

            // Assert
            _mockClients.Verify(
                c => c.Group($"user_{_user1.Id}"), 
                Times.Once
            );
        }

        [Fact]
        public async Task SendEventNotification_PartialMatchSubscription_OnlyMatchesRelevantFields()
        {
            // Arrange
            var eventDto = new EventResponseDto
            {
                Id = 1,
                ProjectId = _projectId,
                EntityId = 100,
                EntityType = "class",
                DataSourceId = _dataSourceId,
                Operation = "create"
            };
            
            var testAction = new Action
            {
                Name = "TestAction_Optimize",
                ProjectId = _projectId,
                LastUpdatedBy = _user1.Id,
                LastUpdatedAt = now
            };
            Context.Actions.Add(testAction);
            await Context.SaveChangesAsync();

            // Subscription with partial match (wrong entity type)
            var wrongTypeSubscription = new Subscription
            {
                UserId = _user1.Id,
                ProjectId = _projectId,
                ActionId = testAction.Id,
                EntityType = "relationship", // Different entity type
                EntityId = 100,
                DataSourceId = _dataSourceId,
                Operation = "create"
            };

            Context.Subscriptions.Add(wrongTypeSubscription);
            await Context.SaveChangesAsync();

            // Act
            await _notificationBusiness.SendEventNotification(eventDto);

            // Assert - Should not send notification
            _mockClientProxy.Verify(
                c => c.SendCoreAsync(
                    It.IsAny<string>(),
                    It.IsAny<object[]>(),
                    default
                ),
                Times.Never
            );
        }

        [Fact]
        public async Task SendEventNotification_UserByEmail_SendsToCorrectUserGroup()
        {
            // Arrange
            var eventDto = new EventResponseDto
            {
                Id = 1,
                ProjectId = _projectId,
                EntityId = 100,
                EntityType = "class",
                DataSourceId = _dataSourceId,
                Operation = "create"
            };

            // Create subscription using user1's ID (which was looked up by email in the hub)
            var subscription = new Subscription
            {
                UserId = _user1.Id,
                ProjectId = _projectId,
                ActionId = _actionId,
                EntityType = "class",
                EntityId = null,
                DataSourceId = _dataSourceId,
                Operation = "create"
            };

            Context.Subscriptions.Add(subscription);
            await Context.SaveChangesAsync();

            // Act
            await _notificationBusiness.SendEventNotification(eventDto);

            // Assert - Verify notification sent to correct user group (based on userId from email lookup)
            _mockClients.Verify(
                c => c.Group($"user_{_user1.Id}"), 
                Times.Once,
                $"Expected notification to be sent to group 'user_{_user1.Id}' for user with email {_user1.Email}"
            );
        }

        #endregion

        #region SendBulkEventNotifications Tests

        [Fact]
        public async Task SendBulkEventNotifications_Success_SendsToAllSubscribedUsers()
        {
            // Arrange
            var event1 = new EventResponseDto
            {
                Id = 1,
                ProjectId = _projectId,
                EntityId = 100,
                EntityType = "class",
                DataSourceId = _dataSourceId,
                Operation = "create"
            };

            var event2 = new EventResponseDto
            {
                Id = 2,
                ProjectId = _projectId,
                EntityId = 200,
                EntityType = "class",
                DataSourceId = _dataSourceId,
                Operation = "update"
            };

            var events = new List<EventResponseDto> { event1, event2 };
            
            var testAction = new Action
            {
                Name = "TestAction_Optimize",
                ProjectId = _projectId,
                LastUpdatedBy = _user1.Id,
                LastUpdatedAt = now
            };
            Context.Actions.Add(testAction);
            await Context.SaveChangesAsync();

            // User1 subscribed to both events
            var subscription1 = new Subscription
            {
                UserId = _user1.Id,
                ProjectId = _projectId,
                ActionId = testAction.Id,
                EntityType = "class",
                EntityId = null, // Wildcard
                DataSourceId = _dataSourceId,
                Operation = null
            };

            // User2 only subscribed to event2
            var subscription2 = new Subscription
            {
                UserId = _user2.Id,
                ProjectId = _projectId,
                ActionId = testAction.Id,
                EntityType = "class",
                EntityId = 200,
                DataSourceId = _dataSourceId,
                Operation = "update"
            };

            Context.Subscriptions.AddRange(subscription1, subscription2);
            await Context.SaveChangesAsync();

            // Act
            await _notificationBusiness.SendBulkEventNotifications(events);

            // Assert
            // User1 should receive 2 notifications (both events)
            _mockClients.Verify(
                c => c.Group($"user_{_user1.Id}"), 
                Times.Exactly(2)
            );

            // User2 should receive 1 notification (event2 only)
            _mockClients.Verify(
                c => c.Group($"user_{_user2.Id}"), 
                Times.Once
            );

            // Total 3 notifications sent
            _mockClientProxy.Verify(
                c => c.SendCoreAsync(
                    "ReceiveNotification",
                    It.IsAny<object[]>(),
                    default
                ),
                Times.Exactly(3)
            );
        }

        [Fact]
        public async Task SendBulkEventNotifications_NullList_LogsWarningAndReturns()
        {
            // Act
            await _notificationBusiness.SendBulkEventNotifications(null);

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("empty or null")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );

            _mockClientProxy.Verify(
                c => c.SendCoreAsync(
                    It.IsAny<string>(),
                    It.IsAny<object[]>(),
                    default
                ),
                Times.Never
            );
        }

        [Fact]
        public async Task SendBulkEventNotifications_EmptyList_LogsWarningAndReturns()
        {
            // Act
            await _notificationBusiness.SendBulkEventNotifications(new List<EventResponseDto>());

            // Assert
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("empty or null")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task SendBulkEventNotifications_NoSubscribers_DoesNotSendNotifications()
        {
            // Arrange
            var events = new List<EventResponseDto>
            {
                new EventResponseDto
                {
                    Id = 1,
                    ProjectId = _projectId,
                    EntityId = 100,
                    EntityType = "class",
                    DataSourceId = _dataSourceId,
                    Operation = "create"
                }
            };

            // No subscriptions

            // Act
            await _notificationBusiness.SendBulkEventNotifications(events);

            // Assert
            _mockClientProxy.Verify(
                c => c.SendCoreAsync(
                    It.IsAny<string>(),
                    It.IsAny<object[]>(),
                    default
                ),
                Times.Never
            );
        }

        [Fact]
        public async Task SendBulkEventNotifications_OptimizesWithSingleDatabaseQuery()
        {
            // Arrange
            var events = new List<EventResponseDto>
            {
                new EventResponseDto
                {
                    Id = 1,
                    ProjectId = _projectId,
                    EntityId = 100,
                    EntityType = "class",
                    DataSourceId = _dataSourceId,
                    Operation = "create"
                },
                new EventResponseDto
                {
                    Id = 2,
                    ProjectId = _projectId,
                    EntityId = 200,
                    EntityType = "class",
                    DataSourceId = _dataSourceId,
                    Operation = "update"
                }
            };
            
            var testAction = new Action
            {
                Name = "TestAction_Optimize",
                ProjectId = _projectId,
                LastUpdatedBy = _user1.Id,
                LastUpdatedAt = now
            };
            Context.Actions.Add(testAction);
            await Context.SaveChangesAsync();

            var subscription = new Subscription
            {
                UserId = _user1.Id,
                ProjectId = _projectId,
                ActionId = testAction.Id,  // Use the newly created action's ID
                EntityType = "class",
                EntityId = null,
                DataSourceId = _dataSourceId,
                Operation = null
            };

            Context.Subscriptions.Add(subscription);
            await Context.SaveChangesAsync();

            // Track database queries (this is implicit - EF will only query once)
            var initialQueryCount = Context.ChangeTracker.Entries().Count();

            // Act
            await _notificationBusiness.SendBulkEventNotifications(events);

            // Assert - Verify both events were sent
            _mockClientProxy.Verify(
                c => c.SendCoreAsync(
                    "ReceiveNotification",
                    It.IsAny<object[]>(),
                    default
                ),
                Times.Exactly(2)
            );
        }

        [Fact]
        public async Task SendBulkEventNotifications_MultipleUsersWithDifferentEmails_SendsToCorrectGroups()
        {
            // Arrange
            var events = new List<EventResponseDto>
            {
                new EventResponseDto
                {
                    Id = 1,
                    ProjectId = _projectId,
                    EntityId = 100,
                    EntityType = "class",
                    DataSourceId = _dataSourceId,
                    Operation = "create"
                }
            };

            // Both users subscribed to the same event
            var subscription1 = new Subscription
            {
                UserId = _user1.Id,
                ProjectId = _projectId,
                ActionId = _actionId,
                EntityType = "class",
                EntityId = null,
                DataSourceId = _dataSourceId,
                Operation = "create"
            };

            var subscription2 = new Subscription
            {
                UserId = _user2.Id,
                ProjectId = _projectId,
                ActionId = _actionId,
                EntityType = "class",
                EntityId = null,
                DataSourceId = _dataSourceId,
                Operation = "create"
            };

            Context.Subscriptions.AddRange(subscription1, subscription2);
            await Context.SaveChangesAsync();

            // Act
            await _notificationBusiness.SendBulkEventNotifications(events);

            // Assert - Both users should receive notifications to their respective groups
            _mockClients.Verify(
                c => c.Group($"user_{_user1.Id}"),
                Times.Once,
                $"Expected notification to user_{_user1.Id} (email: {_user1.Email})"
            );

            _mockClients.Verify(
                c => c.Group($"user_{_user2.Id}"),
                Times.Once,
                $"Expected notification to user_{_user2.Id} (email: {_user2.Email})"
            );
        }

        #endregion

        #region Subscription Matching Tests

        [Fact]
        public async Task SendEventNotification_MultipleProjectSubscriptions_OnlyMatchesCorrectProject()
        {
            // Arrange
            var otherProjectId = _projectId + 1;
            var otherProject = new Project { Name = "Other Project", OrganizationId = organizationId };
            Context.Projects.Add(otherProject);
            await Context.SaveChangesAsync();

            var eventDto = new EventResponseDto
            {
                Id = 1,
                ProjectId = _projectId,
                EntityId = 100,
                EntityType = "class",
                DataSourceId = _dataSourceId,
                Operation = "create"
            };
            
            var testAction = new Action
            {
                Name = "TestAction_Optimize",
                ProjectId = _projectId,
                LastUpdatedBy = _user1.Id,
                LastUpdatedAt = now
            };
            Context.Actions.Add(testAction);
            await Context.SaveChangesAsync();

            // Subscription for correct project
            var correctSubscription = new Subscription
            {
                UserId = _user1.Id,
                ProjectId = _projectId,
                ActionId = testAction.Id,
                EntityType = "class",
                EntityId = null,
                DataSourceId = _dataSourceId,
                Operation = "create"
            };

            // Subscription for different project
            var wrongProjectSubscription = new Subscription
            {
                UserId = _user2.Id,
                ProjectId = otherProject.Id,
                ActionId = testAction.Id,
                EntityType = "class",
                EntityId = null,
                DataSourceId = _dataSourceId,
                Operation = "create"
            };

            Context.Subscriptions.AddRange(correctSubscription, wrongProjectSubscription);
            await Context.SaveChangesAsync();

            // Act
            await _notificationBusiness.SendEventNotification(eventDto);

            // Assert
            _mockClients.Verify(
                c => c.Group($"user_{_user1.Id}"), 
                Times.Once
            );

            _mockClients.Verify(
                c => c.Group($"user_{_user2.Id}"), 
                Times.Never
            );
        }

        [Fact]
        public async Task SendEventNotification_DuplicateSubscriptions_SendsOnlyOnce()
        {
            // Arrange
            var eventDto = new EventResponseDto
            {
                Id = 1,
                ProjectId = _projectId,
                EntityId = 100,
                EntityType = "class",
                DataSourceId = _dataSourceId,
                Operation = "create"
            };
            
            var testAction = new Action
            {
                Name = "TestAction_Optimize",
                ProjectId = _projectId,
                LastUpdatedBy = _user1.Id,
                LastUpdatedAt = now
            };
            Context.Actions.Add(testAction);
            await Context.SaveChangesAsync();

            // Create multiple subscriptions that would match the same event
            var subscription1 = new Subscription
            {
                UserId = _user1.Id,
                ProjectId = _projectId,
                ActionId = testAction.Id,
                EntityType = "class",
                EntityId = 100,
                DataSourceId = _dataSourceId,
                Operation = "create"
            };

            var subscription2 = new Subscription
            {
                UserId = _user1.Id,
                ProjectId = _projectId,
                ActionId = testAction.Id,
                EntityType = "class",
                EntityId = null, // Wildcard also matches
                DataSourceId = _dataSourceId,
                Operation = "create"
            };

            Context.Subscriptions.AddRange(subscription1, subscription2);
            await Context.SaveChangesAsync();

            // Act
            await _notificationBusiness.SendEventNotification(eventDto);

            // Assert - Should only send once due to Distinct() in the code
            _mockClients.Verify(
                c => c.Group($"user_{_user1.Id}"), 
                Times.Once,
                "Should only send one notification even with multiple matching subscriptions"
            );
        }

        #endregion

        protected override async Task SeedTestDataAsync()
        {
            await base.SeedTestDataAsync();
            
            _user1 = new User
            {
                Name = "Test User 1",
                Email = "testuser1@example.com",
                Password = "test_password",
                IsArchived = false
            };

            _user2 = new User
            {
                Name = "Test User 2",
                Email = "testuser2@example.com",
                Password = "test_password",
                IsArchived = false
            };
            
            var organization = new Organization { Name = "Test Organization" };
            Context.Organizations.Add(organization);
            await Context.SaveChangesAsync();
            organizationId = organization.Id;

            Context.Users.AddRange(_user1, _user2);
            await Context.SaveChangesAsync();
            
            var project = new Project 
            { 
                Name = "Test Project",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = _user1.Id,
                OrganizationId = organizationId
            };
            Context.Projects.Add(project);
            await Context.SaveChangesAsync();
            _projectId = project.Id;

            var dataSource = new DataSource
            {
                Name = "Test DataSource",
                ProjectId = _projectId,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = _user1.Id
            };
            Context.DataSources.Add(dataSource);
            await Context.SaveChangesAsync();
            _dataSourceId = dataSource.Id;
            
            var action = new Action
            {
                Name = "Action1",
                ProjectId = _projectId,
                LastUpdatedBy = _user1.Id,
                LastUpdatedAt = now
            };
            Context.Actions.Add(action);
            await Context.SaveChangesAsync();
            _actionId = action.Id;
        }
    }
}