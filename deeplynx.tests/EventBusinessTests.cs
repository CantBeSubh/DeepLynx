using System.Threading.Tasks;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.helpers.Hubs;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;

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
        private long mockActionId;
        private long mockDataSourceId;
        private long mockDataSource2Id;

        public EventBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _mockHubContext = new Mock<IHubContext<EventNotificationHub>>();
            _mockNotificationLogger = new Mock<ILogger<NotificationBusiness>>();
            _notificationBusiness = new NotificationBusiness(Context, _mockNotificationLogger.Object, _mockHubContext.Object);
            _eventBusiness = new EventBusiness(Context, _cacheBusiness, _notificationBusiness);
        }
        
        #region GetEvents Tests
        
        [Fact]
        public async Task GetEventsByUserProjectSubscriptions_Success_NoFilters()
        {
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
            
            var result = await _eventBusiness.GetAllEventsByUserProjectSubscriptions(mockUserId, pid);
            Assert.Equal(6, result.Count);
            Assert.All(result, e => Assert.Equal(pid, e.ProjectId));
        }

        [Fact]
        public async Task GetEventsByUserProjectSubscriptions_Success_MatchingSubscriptionsByEntity()
        {
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

            var result = await _eventBusiness.GetAllEventsByUserProjectSubscriptions(mockUserId, pid);
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

            var result = await _eventBusiness.GetAllEventsByUserProjectSubscriptions(mockUserId, pid);
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
            
            // Collection of subscriptions with incorrect UserId and ProjectId pair
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
            
            var result = await _eventBusiness.GetAllEventsByUserProjectSubscriptions(mockUser2Id, pid2);
            Assert.Empty(result);
        }
        
        #endregion
        
        #region CreateEvent Tests
        
        [Fact]
        public async Task CreateEvent_Success_ReturnsIdAndCreatedAt()
        {
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

            var result = await _eventBusiness.CreateEvent(dto);

            Assert.NotNull(result);
            Assert.NotEqual(0, result.Id);
            Assert.Equal(dto.Operation, result.Operation);
            Assert.Equal(dto.EntityType, result.EntityType);
            Assert.Equal(dto.EntityId, result.EntityId);
            Assert.Equal(dto.DataSourceId, result.DataSourceId);
            Assert.Equal(dto.Properties, result.Properties);
            Assert.Equal(dto.LastUpdatedBy, result.LastUpdatedBy);
            Assert.True(result.LastUpdatedAt >= now);
        }
        
        [Fact]
        public async Task CreateEvent_Fails_IfBadEntityType()
        {
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
            
            await Assert.ThrowsAsync<ArgumentException>(() => _eventBusiness.CreateEvent(dto));
        }
        
        [Fact]
        public async Task CreateEvent_Fails_IfBadOperationType()
        {
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
            await Assert.ThrowsAsync<ArgumentException>(() => _eventBusiness.CreateEvent(dto));
        }
        
        #endregion
        
        #region BulkCreateEvents Tests
        
        [Fact]
        public async Task BulkCreateEvents_Success_ReturnsIdAndCreatedAt()
        {

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

            var results = await _eventBusiness.BulkCreateEvents(pid, events);
            Assert.NotNull(results);

            var actualEvent0 = results[0];
            Assert.Equal("create", actualEvent0.Operation);
            Assert.Equal("metadata", actualEvent0.EntityType);
            Assert.Equal(1, actualEvent0.EntityId);
            Assert.Equal(pid, actualEvent0.ProjectId);
            Assert.Equal("{}", actualEvent0.Properties);
            Assert.Equal("user123", actualEvent0.LastUpdatedBy);

            var actualEvent1 = results[1];
            Assert.Equal("create", actualEvent1.Operation);
            Assert.Equal("metadata", actualEvent1.EntityType);
            Assert.Equal(2, actualEvent1.EntityId);
            Assert.Equal(pid, actualEvent1.ProjectId);
            Assert.Equal("{}", actualEvent1.Properties);
            Assert.Equal("user123", actualEvent1.LastUpdatedBy);
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
            };
            Context.Users.AddRange(users);
            await Context.SaveChangesAsync();
            mockUserId = users[0].Id;
            mockUser2Id = users[1].Id;

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
                    LastUpdatedBy = "user123",
                    LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                },
                new DataSource
                {
                    Name = "DataSource2",
                    ProjectId = pid2,
                    LastUpdatedBy = "user123",
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
                    LastUpdatedBy = "user1",
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
                    LastUpdatedBy = "user2",
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
                    LastUpdatedBy = "user2",
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
                    LastUpdatedBy = "user1",
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
                    LastUpdatedBy = "user1",
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
                    LastUpdatedBy = "user2",
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
                    LastUpdatedBy = "user3",
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
                    LastUpdatedBy = "user4",
                    LastUpdatedAt = now
                }
            };

            Context.Events.AddRange(events);
            await Context.SaveChangesAsync();
        }
    }
}