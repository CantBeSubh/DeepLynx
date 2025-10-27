using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.helpers.Context;
using deeplynx.helpers.Hubs;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class EventBusinessTests : IntegrationTestBase
    {
        private EventBusiness _eventBusiness = null!;
        private INotificationBusiness _notificationBusiness = null!;
        private Mock<ILogger<NotificationBusiness>> _mockNotificationLogger = null!;
        private Mock<IHubContext<EventNotificationHub>> _mockHubContext = null!;
        private long pid;
        private long pid2;
        private DateTime now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        private long mockUserId;
        private long mockUser2Id;
        private long mockUser3Id;
        private long mockUser4Id;
        private long mockActionId;
        private long mockDataSourceId;
        private long mockDataSource2Id;

        public EventBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _mockHubContext = new Mock<IHubContext<EventNotificationHub>>();
            _mockNotificationLogger = new Mock<ILogger<NotificationBusiness>>();
            _notificationBusiness =
                new NotificationBusiness(Context, _mockNotificationLogger.Object, _mockHubContext.Object);
            _eventBusiness = new EventBusiness(Context, _cacheBusiness, _notificationBusiness);
        }

        #region GetAllEvents

        [Fact]
        public async Task GetAllEvents_Success_NoFilters()
        {
            // Act
            var result = await _eventBusiness.GetAllEvents(null, null);

            // Assert
            Assert.Equal(8, result.Count); // All events from both projects
        }

        [Fact]
        public async Task GetAllEvents_Success_FilterByProjectId()
        {
            // Act
            var result = await _eventBusiness.GetAllEvents(pid, null);

            // Assert
            Assert.Equal(6, result.Count);
            Assert.All(result, e => Assert.Equal(pid, e.ProjectId));
        }

        [Fact]
        public async Task GetAllEvents_Success_FilterByProjectId2()
        {
            // Act
            var result = await _eventBusiness.GetAllEvents(pid2, null);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, e => Assert.Equal(pid2, e.ProjectId));
        }

        [Fact]
        public async Task GetAllEventsByUserProjectMembership_Success()
        {
            // Arrange - Add project membership for mockUserId to pid
            var projectMember = new ProjectMember
            {
                ProjectId = pid,
                UserId = mockUserId,
                RoleId = null
            };
            Context.ProjectMembers.Add(projectMember);
            await Context.SaveChangesAsync();

            // Set the user context to simulate authenticated user
            UserContextStorage.UserId = mockUserId;
            UserContextStorage.Email = "test@gmail.com";

            try
            {
                // Act
                var result = await _eventBusiness.GetAllEventsByUser();

                // Assert
                Assert.Equal(6, result.Count);
                Assert.All(result, e => Assert.Equal(pid, e.ProjectId));
            }
            finally
            {
                // Cleanup - clear the user context
                UserContextStorage.UserId = 0;
                UserContextStorage.Email = null;
            }
        }

        [Fact]
        public async Task GetAllEventsByUserProjectMembership_Success_MultipleProjects()
        {
            // Arrange - Add project membership for mockUserId to both projects
            var projectMembers = new List<ProjectMember>
            {
                new ProjectMember
                {
                    ProjectId = pid,
                    UserId = mockUserId,
                    RoleId = null
                },
                new ProjectMember
                {
                    ProjectId = pid2,
                    UserId = mockUserId,
                    RoleId = null
                }
            };
            Context.ProjectMembers.AddRange(projectMembers);
            await Context.SaveChangesAsync();

            // Set the user context
            UserContextStorage.UserId = mockUserId;
            UserContextStorage.Email = "test@gmail.com";

            try
            {
                // Act
                var result = await _eventBusiness.GetAllEventsByUser();

                // Assert
                Assert.Equal(8, result.Count); // All events from both projects
            }
            finally
            {
                // Cleanup
                UserContextStorage.UserId = 0;
                UserContextStorage.Email = null;
            }
        }

        [Fact]
        public async Task GetAllEventsByUserProjectMembership_ReturnsEmpty_WhenUserNotAuthenticated()
        {
            // Arrange - User context is not set (userId = 0)
            UserContextStorage.UserId = 0;
            UserContextStorage.Email = null;

            try
            {
                // Act
                var result = await _eventBusiness.GetAllEventsByUser();

                // Assert
                Assert.Empty(result);
            }
            finally
            {
                // Cleanup
                UserContextStorage.UserId = 0;
                UserContextStorage.Email = null;
            }
        }

        [Fact]
        public async Task GetAllEventsByUserProjectMembership_ReturnsEmpty_WhenUserHasNoProjectMemberships()
        {
            // Arrange - User is authenticated but has no project memberships
            UserContextStorage.UserId = mockUserId;
            UserContextStorage.Email = "test@gmail.com";

            try
            {
                // Act
                var result = await _eventBusiness.GetAllEventsByUser();

                // Assert
                Assert.Empty(result);
            }
            finally
            {
                // Cleanup
                UserContextStorage.UserId = 0;
                UserContextStorage.Email = null;
            }
        }

        # endregion

        #region GetEvents Tests

        [Fact]
        public async Task GetEventsByUserProjectSubscriptions_Success_NoFilters()
        {
            // Arrange
            var subscription = new Subscription
            {
                UserId = mockUserId,
                ProjectId = pid,
                EntityId = null,
                EntityType = null,
                DataSourceId = null,
                Operation = null,
                ActionId = mockActionId
            };
            Context.Subscriptions.Add(subscription);
            await Context.SaveChangesAsync();

            // Act
            var result = await _eventBusiness.GetAllEventsByUserProjectSubscriptions(mockUserId, pid);

            // Assert
            Assert.Equal(6, result.Count);
            Assert.All(result, e => Assert.Equal(pid, e.ProjectId));
        }

        [Fact]
        public async Task GetEventsByUserProjectSubscriptions_Success_MatchingSubscriptionsByEntity()
        {
            // Arrange
            var subscription = new Subscription
            {
                // Get Event with specific Entity
                UserId = mockUserId,
                ProjectId = pid,
                EntityId = 1,
                EntityType = "edge",
                DataSourceId = mockDataSourceId,
                Operation = "create",
                ActionId = mockActionId
            };

            Context.Subscriptions.Add(subscription);
            await Context.SaveChangesAsync();

            // Act
            var result = await _eventBusiness.GetAllEventsByUserProjectSubscriptions(mockUserId, pid);

            // Assert
            Assert.Single(result);

            var actualEvent = result[0];

            Assert.Equal(pid, actualEvent.ProjectId);
            Assert.Equal("create", actualEvent.Operation);
            Assert.Equal("edge", actualEvent.EntityType);
            Assert.Equal(1, actualEvent.EntityId);
            Assert.Equal(mockDataSourceId, actualEvent.DataSourceId);
        }

        [Fact]
        public async Task GetEventsByUserProjectSubscriptions_Success_MatchingSubscriptionsByOperation()
        {
            // Arrange
            var subscriptions = new List<Subscription>
            {
                new Subscription
                {
                    UserId = mockUserId,
                    ProjectId = pid,
                    EntityId = null,
                    EntityType = null,
                    DataSourceId = null,
                    Operation = "create",
                    ActionId = mockActionId
                },
                new Subscription
                {
                    UserId = mockUserId,
                    ProjectId = pid,
                    EntityId = null,
                    EntityType = null,
                    DataSourceId = null,
                    Operation = "update",
                    ActionId = mockActionId
                },
                new Subscription
                {
                    UserId = mockUserId,
                    ProjectId = pid,
                    EntityId = null,
                    EntityType = null,
                    DataSourceId = null,
                    Operation = "delete",
                    ActionId = mockActionId
                },
            };
            Context.Subscriptions.AddRange(subscriptions);
            await Context.SaveChangesAsync();

            // Act
            var result = await _eventBusiness.GetAllEventsByUserProjectSubscriptions(mockUserId, pid);

            // Assert
            Assert.Equal(6, result.Count);

            var actualEvent0 = result[0];
            Assert.Equal(pid, actualEvent0.ProjectId);
            Assert.Equal("create", actualEvent0.Operation);
            Assert.Equal("edge", actualEvent0.EntityType);
            Assert.Equal(1, actualEvent0.EntityId);
            Assert.Equal(mockDataSourceId, actualEvent0.DataSourceId);

            var actualEvent1 = result[1];
            Assert.Equal(pid, actualEvent1.ProjectId);
            Assert.Equal("create", actualEvent1.Operation);
            Assert.Equal("edge", actualEvent1.EntityType);
            Assert.Equal(2, actualEvent1.EntityId);
            Assert.Equal(mockDataSourceId, actualEvent1.DataSourceId);

            var actualEvent2 = result[2];
            Assert.Equal(pid, actualEvent2.ProjectId);
            Assert.Equal("delete", actualEvent2.Operation);
            Assert.Equal("class", actualEvent2.EntityType);
            Assert.Equal(3, actualEvent2.EntityId);
            Assert.Equal(mockDataSourceId, actualEvent2.DataSourceId);

            var actualEvent3 = result[3];
            Assert.Equal(pid, actualEvent3.ProjectId);
            Assert.Equal("delete", actualEvent3.Operation);
            Assert.Equal("class", actualEvent3.EntityType);
            Assert.Equal(4, actualEvent3.EntityId);
            Assert.Equal(mockDataSourceId, actualEvent3.DataSourceId);

            var actualEvent4 = result[4];
            Assert.Equal(pid, actualEvent4.ProjectId);
            Assert.Equal("delete", actualEvent4.Operation);
            Assert.Equal("edge", actualEvent4.EntityType);
            Assert.Equal(2, actualEvent4.EntityId);
            Assert.Equal(mockDataSourceId, actualEvent4.DataSourceId);

            var actualEvent5 = result[5];
            Assert.Equal(pid, actualEvent5.ProjectId);
            Assert.Equal("update", actualEvent5.Operation);
            Assert.Equal("edge", actualEvent5.EntityType);
            Assert.Equal(5, actualEvent5.EntityId);
            Assert.Equal(mockDataSourceId, actualEvent5.DataSourceId);
        }

        [Fact]
        public async Task GetEventsByUserProjectSubscriptions_Fails_NonMatchingSubscriptions()
        {
            // Arrange
            var subscriptions = new List<Subscription>
            {
                new Subscription
                {
                    UserId = mockUser2Id,
                    ProjectId = pid,
                    EntityId = null,
                    EntityType = null,
                    DataSourceId = null,
                    Operation = "delete",
                    ActionId = mockActionId
                },
                new Subscription
                {
                    UserId = mockUserId,
                    ProjectId = pid,
                    EntityId = null,
                    EntityType = null,
                    DataSourceId = null,
                    Operation = "delete",
                    ActionId = mockActionId
                },
                new Subscription
                {
                    UserId = mockUser2Id,
                    ProjectId = pid2,
                    EntityId = 0,
                    EntityType = "class",
                    DataSourceId = mockDataSource2Id,
                    Operation = "create",
                    ActionId = mockActionId
                },
                new Subscription
                {
                    UserId = mockUser2Id,
                    ProjectId = pid2,
                    EntityId = null,
                    EntityType = null,
                    DataSourceId = null,
                    Operation = "create",
                    ActionId = mockActionId
                },
                new Subscription
                {
                    UserId = mockUser2Id,
                    ProjectId = pid2,
                    EntityId = null,
                    EntityType = null,
                    DataSourceId = mockDataSource2Id,
                    Operation = null,
                    ActionId = mockActionId
                },
            };
            Context.Subscriptions.AddRange(subscriptions);
            await Context.SaveChangesAsync();

            // Act
            var result = await _eventBusiness.GetAllEventsByUserProjectSubscriptions(mockUser2Id, pid2);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region CreateEvent Tests

        [Fact]
        public async Task CreateEvent_Success_ReturnsIdAndCreatedAt()
        {
            // Arrange
            var dto = new CreateEventRequestDto
            {
                Operation = "create",
                EntityType = "metadata",
                EntityId = 1,
                ProjectId = pid,
                DataSourceId = null,
                Properties = "{}",
                LastUpdatedBy = "user123"
            };

            // Act
            var result = await _eventBusiness.CreateEvent(dto);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(0, result.Id);
            Assert.Equal(dto.Operation, result.Operation);
            Assert.Equal(dto.EntityType, result.EntityType);
            Assert.Equal(dto.EntityId, result.EntityId);
            Assert.Equal(dto.DataSourceId, result.DataSourceId);
            Assert.Equal(dto.Properties, result.Properties);
            Assert.True(result.LastUpdatedAt >= now);
        }

        [Fact]
        public async Task CreateEvent_Fails_IfBadEntityType()
        {
            // Arrange
            var dto = new CreateEventRequestDto
            {
                Operation = "create",
                EntityType = "BadType",
                EntityId = 1,
                ProjectId = pid,
                DataSourceId = null,
                Properties = "{}",
                LastUpdatedBy = "user123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _eventBusiness.CreateEvent(dto));
        }

        [Fact]
        public async Task CreateEvent_Fails_IfBadOperationType()
        {
            // Arrange
            var dto = new CreateEventRequestDto
            {
                Operation = "BadType",
                EntityType = "metadata",
                EntityId = 1,
                ProjectId = pid,
                DataSourceId = null,
                Properties = "{}",
                LastUpdatedBy = "user123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _eventBusiness.CreateEvent(dto));
        }

        #endregion

        #region BulkCreateEvents Tests

        [Fact]
        public async Task BulkCreateEvents_Success_ReturnsIdAndCreatedAt()
        {
            // Arrange
            var events = new List<CreateEventRequestDto> { };

            var dto1 = new CreateEventRequestDto
            {
                Operation = "create",
                EntityType = "metadata",
                EntityId = 1,
                ProjectId = pid,
                DataSourceId = null,
                Properties = "{}",
                LastUpdatedBy = "user123"
            };

            var dto2 = new CreateEventRequestDto
            {
                Operation = "create",
                EntityType = "metadata",
                EntityId = 2,
                ProjectId = pid,
                DataSourceId = null,
                Properties = "{}",
                LastUpdatedBy = "user123"
            };

            events.Add(dto1);
            events.Add(dto2);

            // Act
            var results = await _eventBusiness.BulkCreateEvents(pid, events);

            // Assert
            Assert.NotNull(results);

            var actualEvent0 = results[0];
            Assert.Equal("create", actualEvent0.Operation);
            Assert.Equal("metadata", actualEvent0.EntityType);
            Assert.Equal(1, actualEvent0.EntityId);
            Assert.Equal(pid, actualEvent0.ProjectId);
            Assert.Equal("{}", actualEvent0.Properties);

            var actualEvent1 = results[1];
            Assert.Equal("create", actualEvent1.Operation);
            Assert.Equal("metadata", actualEvent1.EntityType);
            Assert.Equal(2, actualEvent1.EntityId);
            Assert.Equal(pid, actualEvent1.ProjectId);
            Assert.Equal("{}", actualEvent1.Properties);
        }

        #endregion

             #region LastUpdatedBy Tests
        
        [Fact]
        public async Task CreateEvent_Success_StoresLastUpdatedByUserId()
        {
            // Arrange
            var testEvent = new Event
            {
                ProjectId = pid,
                Operation = "create",
                EntityType = "test",
                EntityId = 999,
                DataSourceId = mockDataSourceId,
                Properties = "{}",
                LastUpdatedBy = mockUserId,
                LastUpdatedAt = now
            };
            
            // Act
            Context.Events.Add(testEvent);
            await Context.SaveChangesAsync();

            // Assert
            var savedEvent = await Context.Events.FindAsync(testEvent.Id);
            Assert.NotNull(savedEvent);
            Assert.Equal(mockUserId, savedEvent.LastUpdatedBy);
        }

        [Fact]
        public async Task CreateEvent_Success_NavigationPropertyLoadsUser()
        {
            // Arrange
            var testEvent = new Event
            {
                ProjectId = pid,
                Operation = "create",
                EntityType = "test",
                EntityId = 998,
                DataSourceId = mockDataSourceId,
                Properties = "{}",
                LastUpdatedBy = mockUserId,
                LastUpdatedAt = now
            };
            
            Context.Events.Add(testEvent);
            await Context.SaveChangesAsync();

            // Act
            var eventWithUser = await Context.Events
                .Include(e => e.LastUpdatedByUser)
                .FirstAsync(e => e.Id == testEvent.Id);
            
            // Assert
            Assert.NotNull(eventWithUser.LastUpdatedByUser);
            Assert.Equal("user1", eventWithUser.LastUpdatedByUser.Name);
            Assert.Equal("test@gmail.com", eventWithUser.LastUpdatedByUser.Email);
            Assert.Equal(mockUserId, eventWithUser.LastUpdatedBy);
        }

        [Fact]
        public async Task CreateEvent_Success_WithNullLastUpdatedBy()
        {
            // Arrange
            var testEvent = new Event
            {
                ProjectId = pid,
                Operation = "create",
                EntityType = "test",
                EntityId = 997,
                DataSourceId = mockDataSourceId,
                Properties = "{}",
                LastUpdatedBy = null,
                LastUpdatedAt = now
            };
            
            // Act
            Context.Events.Add(testEvent);
            await Context.SaveChangesAsync();

            // Assert
            var savedEvent = await Context.Events.FindAsync(testEvent.Id);
            Assert.NotNull(savedEvent);
            Assert.Null(savedEvent.LastUpdatedBy);
            
            var eventWithUser = await Context.Events
                .Include(e => e.LastUpdatedByUser)
                .FirstAsync(e => e.Id == testEvent.Id);
            
            Assert.Null(eventWithUser.LastUpdatedByUser);
        }

        [Fact]
        public async Task UpdateEvent_Success_UpdatesLastUpdatedByUserId()
        {
            // Arrange
            var testEvent = new Event
            {
                ProjectId = pid,
                Operation = "create",
                EntityType = "test",
                EntityId = 996,
                DataSourceId = mockDataSourceId,
                Properties = "{}",
                LastUpdatedBy = null,
                LastUpdatedAt = now
            };
            Context.Events.Add(testEvent);
            await Context.SaveChangesAsync();

            // Act
            testEvent.LastUpdatedBy = mockUser2Id;
            testEvent.Operation = "update";
            testEvent.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            
            Context.Events.Update(testEvent);
            await Context.SaveChangesAsync();

            // Assert
            var updatedEvent = await Context.Events
                .Include(e => e.LastUpdatedByUser)
                .FirstAsync(e => e.Id == testEvent.Id);
            
            Assert.Equal(mockUser2Id, updatedEvent.LastUpdatedBy);
            Assert.NotNull(updatedEvent.LastUpdatedByUser);
            Assert.Equal("user2", updatedEvent.LastUpdatedByUser.Name);
            Assert.Equal("update", updatedEvent.Operation);
        }
        
        #endregion
        protected override async Task SeedTestDataAsync()
        {
            await base.SeedTestDataAsync();

            var projects = new List<Project>
            {
                new Project { Name = "Project 1" },
                new Project { Name = "Project 2" },
            };
            Context.Projects.AddRange(projects);
            await Context.SaveChangesAsync();
            pid = projects[0].Id;
            pid2 = projects[1].Id;

            var users = new List<User>
            {
                new User { Name = "user1", Email = "test@gmail.com" },
                new User { Name = "user2", Email = "test2@gmail.com" },
                new User { Name = "user3", Email = "test3@gmail.com" },
                new User { Name = "user4", Email = "test4@gmail.com" },
            };
            Context.Users.AddRange(users);
            await Context.SaveChangesAsync();
            mockUserId = users[0].Id;
            mockUser2Id = users[1].Id;
            mockUser3Id = users[2].Id;
            mockUser4Id = users[3].Id;

            var action = new deeplynx.datalayer.Models.Action
            {
                Name = "Action1",
                ProjectId = pid,
                LastUpdatedBy = "user123",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Actions.Add(action);
            await Context.SaveChangesAsync();
            mockActionId = action.Id;

            var dataSources = new List<DataSource>
            {
                new DataSource
                {
                    Name = "DataSource1",
                    ProjectId = pid,
                    LastUpdatedBy = mockUserId,
                    LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                },
                new DataSource
                {
                    Name = "DataSource2",
                    ProjectId = pid2,
                    LastUpdatedBy = mockUserId,
                    LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                }
            };

            Context.DataSources.AddRange(dataSources);
            await Context.SaveChangesAsync();
            mockDataSourceId = dataSources[0].Id;
            mockDataSource2Id = dataSources[1].Id;

            var events = new List<Event>
            {
                // Events with 1st Project
                new Event
                {
                    ProjectId = pid,
                    Operation = "create",
                    EntityType = "edge",
                    EntityId = 1,
                    DataSourceId = mockDataSourceId,
                    Properties = "{}",
                    LastUpdatedBy = mockUserId,
                    LastUpdatedAt = now
                },
                new Event
                {
                    ProjectId = pid,
                    Operation = "create",
                    EntityType = "edge",
                    EntityId = 2,
                    DataSourceId = mockDataSourceId,
                    Properties = "{}",
                    LastUpdatedBy = mockUser2Id,
                    LastUpdatedAt = now
                },
                new Event
                {
                    ProjectId = pid,
                    Operation = "delete",
                    EntityType = "class",
                    EntityId = 3,
                    DataSourceId = mockDataSourceId,
                    Properties = "{}",
                    LastUpdatedBy = mockUser2Id,
                    LastUpdatedAt = now
                },
                new Event
                {
                    ProjectId = pid,
                    Operation = "delete",
                    EntityType = "class",
                    EntityId = 4,
                    DataSourceId = mockDataSourceId,
                    Properties = "{}",
                    LastUpdatedBy = mockUserId,
                    LastUpdatedAt = now
                },
                new Event
                {
                    ProjectId = pid,
                    Operation = "delete",
                    EntityType = "edge",
                    EntityId = 2,
                    DataSourceId = mockDataSourceId,
                    Properties = "{}",
                    LastUpdatedBy = mockUserId,
                    LastUpdatedAt = now
                },
                new Event
                {
                    ProjectId = pid,
                    Operation = "update",
                    EntityType = "edge",
                    EntityId = 5,
                    DataSourceId = mockDataSourceId,
                    Properties = "{}",
                    LastUpdatedBy = mockUser2Id,
                    LastUpdatedAt = now
                },
                // Events with 2nd Project
                new Event
                {
                    ProjectId = pid2,
                    Operation = "update",
                    EntityType = "edge",
                    EntityId = 3,
                    DataSourceId = null,
                    Properties = "{}",
                    LastUpdatedBy = mockUser3Id,
                    LastUpdatedAt = now
                },
                new Event
                {
                    ProjectId = pid2,
                    Operation = "delete",
                    EntityType = "edge",
                    EntityId = 4,
                    DataSourceId = mockDataSourceId,
                    Properties = "{}",
                    LastUpdatedBy = mockUser4Id,
                    LastUpdatedAt = now
                }
            };

            Context.Events.AddRange(events);
            await Context.SaveChangesAsync();
        }
    }
}