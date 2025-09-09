using System;
using System.Threading.Tasks;
using deeplynx.business;
using FluentAssertions;
using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class QueryBusinessTests : IntegrationTestBase
    {
        private QueryBusiness _queryBusiness = null!;
        private long pid;
        private long pid2;
        private DateTime now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        private long mockUserId;
        private long mockUser2Id;
        private long mockActionId;
        private long mockDataSourceId;
        private long mockDataSource2Id;

        public QueryBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _queryBusiness = new QueryBusiness(Context);
        }

        // [Fact]
        // public async Task GetEventsByUserProjectSubscriptions_Success_NoFilters()
        // {
        //     var subscription = new Subscription
        //     {
        //         UserId = mockUserId,
        //         ProjectId = pid,
        //         EntityId = null,
        //         EntityType = null,
        //         DataSourceId = null,
        //         Operation = null,
        //         ActionId = mockActionId
        //     };
        //     Context.Subscriptions.Add(subscription);
        //     await Context.SaveChangesAsync();
        //     
        //     var result = await _queryBusiness.GetAllEventsByUserProjectSubscriptions(mockUserId, pid);
        //     result.Should().HaveCount(6);
        //     result.Should().OnlyContain(e => e.ProjectId == pid);
        // }

        // [Fact]
        // public async Task GetEventsByUserProjectSubscriptions_Success_MatchingSubscriptionsByEntity()
        // {
        //     var subscription = new Subscription
        //     {
        //         // Get Event with specific Entity
        //         UserId = mockUserId,
        //         ProjectId = pid,
        //         EntityId = 1,
        //         EntityType = "edge",
        //         DataSourceId = mockDataSourceId,
        //         Operation = "create",
        //         ActionId = mockActionId
        //     };
        //     
        //     Context.Subscriptions.Add(subscription);
        //     await Context.SaveChangesAsync();
        //
        //     var result = await _eventBusiness.GetAllEventsByUserProjectSubscriptions(mockUserId, pid);
        //     result.Should().HaveCount(1);
        //     result.First().Should().BeEquivalentTo(new {
        //         ProjectId = pid,
        //         Operation = "create",
        //         EntityType = "edge",
        //         EntityId = 1,
        //         DataSourceId = mockDataSourceId
        //     });
        // }
        //
        // [Fact]
        // public async Task GetEventsByUserProjectSubscriptions_Success_MatchingSubscriptionsByOperation()
        // {
        //     var subscriptions = new List<Subscription>
        //     {
        //         new Subscription
        //         {
        //             UserId = mockUserId,
        //             ProjectId = pid,
        //             EntityId = null,
        //             EntityType = null,
        //             DataSourceId = null,
        //             Operation = "create",
        //             ActionId = mockActionId
        //         },
        //         new Subscription
        //         {
        //             UserId = mockUserId,
        //             ProjectId = pid,
        //             EntityId = null,
        //             EntityType = null,
        //             DataSourceId = null,
        //             Operation = "update",
        //             ActionId = mockActionId
        //         },
        //         new Subscription
        //         {
        //             UserId = mockUserId,
        //             ProjectId = pid,
        //             EntityId = null,
        //             EntityType = null,
        //             DataSourceId = null,
        //             Operation = "delete",
        //             ActionId = mockActionId
        //         },
        //     };
        //     Context.Subscriptions.AddRange(subscriptions);
        //     await Context.SaveChangesAsync();
        //
        //     var result = await _eventBusiness.GetAllEventsByUserProjectSubscriptions(mockUserId, pid);
        //     result.Should().HaveCount(6);
        //     result[0].Should().BeEquivalentTo(new {
        //         ProjectId = pid,
        //         Operation = "create",
        //         EntityType = "edge",
        //         EntityId = 1,
        //         DataSourceId = mockDataSourceId
        //     });
        //     result[1].Should().BeEquivalentTo(new {
        //         ProjectId = pid,
        //         Operation = "create",
        //         EntityType = "edge",
        //         EntityId = 2,
        //         DataSourceId = mockDataSourceId
        //     });
        //     result[2].Should().BeEquivalentTo(new {
        //         ProjectId = pid,
        //         Operation = "delete",
        //         EntityType = "class",
        //         EntityId = 3,
        //         DataSourceId = mockDataSourceId
        //     });
        //     result[3].Should().BeEquivalentTo(new {
        //         ProjectId = pid,
        //         Operation = "delete",
        //         EntityType = "class",
        //         EntityId = 4,
        //         DataSourceId = mockDataSourceId
        //     });
        //     result[4].Should().BeEquivalentTo(new {
        //         ProjectId = pid,
        //         Operation = "delete",
        //         EntityType = "edge",
        //         EntityId = 2,
        //         DataSourceId = mockDataSourceId
        //     });
        //     result[5].Should().BeEquivalentTo(new {
        //         ProjectId = pid,
        //         Operation = "update",
        //         EntityType = "edge",
        //         EntityId = 5,
        //         DataSourceId = mockDataSourceId
        //     });
        // }
        //
        // [Fact]
        // public async Task GetEventsByUserProjectSubscriptions_Fails_NonMatchingSubscriptions()
        // {
        //     var subscriptions = new List<Subscription>
        //     {
        //         new Subscription
        //         {
        //             UserId = mockUser2Id,
        //             ProjectId = pid,
        //             EntityId = null,
        //             EntityType = null,
        //             DataSourceId = null,
        //             Operation = "delete",
        //             ActionId = mockActionId
        //         },
        //         new Subscription
        //         {
        //             UserId = mockUserId,
        //             ProjectId = pid,
        //             EntityId = null,
        //             EntityType = null,
        //             DataSourceId = null,
        //             Operation = "delete",
        //             ActionId = mockActionId
        //         },
        //         new Subscription
        //         {
        //             UserId = mockUser2Id,
        //             ProjectId = pid2,
        //             EntityId = 0,
        //             EntityType = "class",
        //             DataSourceId = mockDataSource2Id,
        //             Operation = "create",
        //             ActionId = mockActionId
        //         },
        //         new Subscription
        //         {
        //             UserId = mockUser2Id,
        //             ProjectId = pid2,
        //             EntityId = null,
        //             EntityType = null,
        //             DataSourceId = null,
        //             Operation = "create",
        //             ActionId = mockActionId
        //         },
        //         new Subscription
        //         {
        //             UserId = mockUser2Id,
        //             ProjectId = pid2,
        //             EntityId = null,
        //             EntityType = null,
        //             DataSourceId = mockDataSource2Id,
        //             Operation = null,
        //             ActionId = mockActionId
        //         },
        //     };
        //     Context.Subscriptions.AddRange(subscriptions);
        //     await Context.SaveChangesAsync();
        //     
        //     var result = await _eventBusiness.GetAllEventsByUserProjectSubscriptions(mockUser2Id, pid2);
        //     result.Should().HaveCount(0);
        // }
        //
        // [Fact]
        // public async Task CreateEvent_Success_ReturnsIdAndCreatedAt()
        // {
        //     var dto = new CreateEventRequestDto
        //     {
        //         Operation = "create",
        //         EntityType = "metadata",
        //         EntityId = 1,
        //         ProjectId = pid,
        //         DataSourceId = null,
        //         Properties = "{}",
        //         CreatedBy = "user123"
        //     };
        //
        //     var result = await _eventBusiness.CreateEvent(dto);
        //
        //     result.Should().NotBeNull();
        //     result.Id.Should().NotBe(0);
        //     result.Operation.Should().Be(dto.Operation);
        //     result.EntityType.Should().Be(dto.EntityType);
        //     result.EntityId.Should().Be(dto.EntityId);
        //     result.DataSourceId.Should().Be(dto.DataSourceId);
        //     result.Properties.Should().Be(dto.Properties);
        //     result.CreatedBy.Should().Be(dto.CreatedBy);
        //     result.CreatedAt.Should().BeOnOrAfter(now);
        // }
        //
        // [Fact]
        // public async Task CreateEvent_Fails_IfBadEntityType()
        // {
        //     var dto = new CreateEventRequestDto
        //     {
        //         Operation = "create",
        //         EntityType = "BadType",
        //         EntityId = 1,
        //         ProjectId = pid,
        //         DataSourceId = null,
        //         Properties = "{}",
        //         CreatedBy = "user123"
        //     };
        //     
        //     var result = () => _eventBusiness.CreateEvent(dto);
        //     await result.Should().ThrowAsync<ArgumentException>();
        // }
        //
        // [Fact]
        // public async Task CreateEvent_Fails_IfBadOperationType()
        // {
        //     var dto = new CreateEventRequestDto
        //     {
        //         Operation = "BadType",
        //         EntityType = "metadata",
        //         EntityId = 1,
        //         ProjectId = pid,
        //         DataSourceId = null,
        //         Properties = "{}",
        //         CreatedBy = "user123"
        //     };
        //
        //     var result = () => _eventBusiness.CreateEvent(dto);
        //     await result.Should().ThrowAsync<ArgumentException>();
        // }
        //
        //
        // protected override async Task SeedTestDataAsync()
        // {
        //     await base.SeedTestDataAsync();
        //     
        //     var projects = new List<Project>
        //     {
        //         new Project { Name = "Project 1" },
        //         new Project { Name = "Project 2" },
        //     };
        //     Context.Projects.AddRange(projects);
        //     await Context.SaveChangesAsync();
        //     pid = projects[0].Id;
        //     pid2 = projects[1].Id;
        //
        //     var users = new List<User>
        //     {
        //         new User { Name = "user1", Email = "test@gmail.com" },
        //         new User { Name = "user2", Email = "test@gmail.com" },
        //     };
        //     Context.Users.AddRange(users);
        //     await Context.SaveChangesAsync();
        //     mockUserId = users[0].Id;
        //     mockUser2Id = users[1].Id;
        //
        //     var action = new deeplynx.datalayer.Models.Action
        //     {
        //         Name = "Action1",
        //         ProjectId = pid,
        //         CreatedBy = "user123",
        //         CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        //     };
        //     Context.Actions.Add(action);
        //     await Context.SaveChangesAsync();
        //     mockActionId = action.Id;
        //
        //     var dataSources = new List<DataSource>
        //     {
        //         new DataSource
        //         {
        //             Name = "DataSource1",
        //             ProjectId = pid,
        //             CreatedBy = "user123",
        //             CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        //         },
        //         new DataSource
        //         {
        //             Name = "DataSource2",
        //             ProjectId = pid2,
        //             CreatedBy = "user123",
        //             CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        //         }
        //     };
        //     
        //     Context.DataSources.AddRange(dataSources);
        //     await Context.SaveChangesAsync();
        //     mockDataSourceId = dataSources[0].Id;
        //     mockDataSource2Id = dataSources[1].Id;
        //     
        //     var events = new List<Event>
        //     {
        //         // Events with 1st Project
        //         new Event
        //         {
        //             ProjectId = pid,
        //             Operation = "create",
        //             EntityType = "edge",
        //             EntityId = 1,
        //             DataSourceId = mockDataSourceId,
        //             Properties = "{}",
        //             CreatedBy = "user1",
        //             CreatedAt = now
        //         },
        //         new Event
        //         {
        //             ProjectId = pid,
        //             Operation = "create",
        //             EntityType = "edge",
        //             EntityId = 2,
        //             DataSourceId = mockDataSourceId,
        //             Properties = "{}",
        //             CreatedBy = "user2",
        //             CreatedAt = now
        //         },
        //         new Event
        //         {
        //             ProjectId = pid,
        //             Operation = "delete",
        //             EntityType = "class",
        //             EntityId = 3,
        //             DataSourceId = mockDataSourceId,
        //             Properties = "{}",
        //             CreatedBy = "user2",
        //             CreatedAt = now
        //         },
        //         new Event
        //         {
        //             ProjectId = pid,
        //             Operation = "delete",
        //             EntityType = "class",
        //             EntityId = 4,
        //             DataSourceId = mockDataSourceId,
        //             Properties = "{}",
        //             CreatedBy = "user2",
        //             CreatedAt = now
        //         },
        //         new Event
        //         {
        //             ProjectId = pid,
        //             Operation = "delete",
        //             EntityType = "edge",
        //             EntityId = 2,
        //             DataSourceId = mockDataSourceId,
        //             Properties = "{}",
        //             CreatedBy = "user2",
        //             CreatedAt = now
        //         },
        //         new Event
        //         {
        //             ProjectId = pid,
        //             Operation = "update",
        //             EntityType = "edge",
        //             EntityId = 5,
        //             DataSourceId = mockDataSourceId,
        //             Properties = "{}",
        //             CreatedBy = "user2",
        //             CreatedAt = now
        //         },
        //         // Events with 2nd Project
        //         new Event
        //         {
        //             ProjectId = pid2,
        //             Operation = "update",
        //             EntityType = "edge",
        //             EntityId = 3,
        //             DataSourceId = null,
        //             Properties = "{}",
        //             CreatedBy = "user3",
        //             CreatedAt = now
        //         },
        //         new Event
        //         {
        //             ProjectId = pid2,
        //             Operation = "delete",
        //             EntityType = "edge",
        //             EntityId = 4,
        //             DataSourceId = mockDataSourceId,
        //             Properties = "{}",
        //             CreatedBy = "user4",
        //             CreatedAt = now
        //         }
        //     };
        //
        //     Context.Events.AddRange(events);
        //     await Context.SaveChangesAsync();
        // }
    }
}