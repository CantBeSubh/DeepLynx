using System.ComponentModel.DataAnnotations;
using deeplynx.business; using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models; using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Action = deeplynx.datalayer.Models.Action;
using Moq;


namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class SubscriptionBusinessTests : IntegrationTestBase
    {
        private SubscriptionBusiness _subscriptionBusiness = null!;
        public long mockActionId;
        public long mockDataSourceId;
  
        private readonly DateTime now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        public long pid;
        public long uid;
        public SubscriptionBusinessTests(TestSuiteFixture fixture) : base(fixture) { }
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _subscriptionBusiness = new SubscriptionBusiness(Context, _cacheBusiness);
        }

        # region GetTests
  
        [Fact]
        public async Task GetAllSubscriptions_Success_ReturnsSubscriptions()
        {
            var subscription = new Subscription
            {
                UserId = uid,
                ProjectId = pid,
                ActionId = mockActionId,
                Operation = "create",
                DataSourceId = mockDataSourceId,
                EntityType = "record",
                EntityId = 1
            };
            Context.Subscriptions.Add(subscription);
            await Context.SaveChangesAsync();
            var result = await _subscriptionBusiness.GetAllSubscriptions(uid, pid, false);
            result.Should().HaveCount(1);
            result.First().Should().BeEquivalentTo(new
            {
                UserId = uid,
                ProjectId = pid,
                ActionId = mockActionId,
                Operation = "create",
                DataSourceId = mockDataSourceId,
                EntityType = "record",
                EntityId = 1
            });
        }
        [Fact]
        public async Task GetSubscription_Success_ReturnsSubscription()
        {
            var subscription = new Subscription
            {
                UserId = uid,
                ProjectId = pid,
                ActionId = mockActionId,
                Operation = "delete",
                DataSourceId = null,
                EntityType = "record",
                EntityId = 1
            };
            Context.Subscriptions.Add(subscription);
            await Context.SaveChangesAsync();
            var result = await _subscriptionBusiness.GetSubscription(uid, pid, subscription.Id, false);
            result.Should().BeEquivalentTo(new
            {
                UserId = uid,
                ProjectId = pid,
                ActionId = mockActionId,
                Operation = "delete",
                EntityType = "record",
                EntityId = 1,
            });
        }
        
        #endregion
        #region CreateTests
        [Fact]
        public async Task BulkCreateSubscriptions_Success_CreatesSubscriptions()
        {
            var dtos = new List<CreateSubscriptionRequestDto>
            {
                new()
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = mockActionId,
                    Operation = "delete",
                    DataSourceId = mockDataSourceId,
                    EntityType = "record",
                    EntityId = 1,
                },
                new()
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = mockActionId,
                    Operation = "update",
                    DataSourceId = mockDataSourceId,
                    EntityType = "record",
                    EntityId = 3,
                },
            };
            var result = await _subscriptionBusiness.BulkCreateSubscriptions(uid, pid, dtos);
            result.Should().HaveCount(2);
            result.First().Should().BeEquivalentTo(new
            {
                UserId = uid,
                ProjectId = pid,
                ActionId = mockActionId,
                Operation = "delete",
                EntityType = "record",
                EntityId = 1,
            });
            result.Last().Should().BeEquivalentTo(new
            {
                UserId = uid,
                ProjectId = pid,
                ActionId = mockActionId,
                Operation = "update",
                DataSourceId = mockDataSourceId,
                EntityType = "record",
                EntityId = 3,
            });
        }
        
        [Fact]
        public async Task BulkCreateSubscriptions_Fails_IfNoUserID()
        {
            var dtos = new List<CreateSubscriptionRequestDto>
            {
                new()
                {
                    ProjectId = pid,
                    ActionId = mockActionId,
                    Operation = "delete",
                    DataSourceId = mockDataSourceId,
                    EntityType = "record",
                    EntityId = 1,
                }
            };
        
            var result = () => _subscriptionBusiness.BulkCreateSubscriptions(uid, pid, dtos);
            await result.Should().ThrowAsync<ValidationException>();
            
            var SubscriptionList = Context.Subscriptions.ToList();
            SubscriptionList.Should().HaveCount(0);
        }
        
        [Fact]
        public async Task BulkCreateSubscriptions_Fails_IfNoProjectID()
        {
            var dtos = new List<CreateSubscriptionRequestDto>
            {
                new()
                {
                    UserId = uid,
                    ActionId = mockActionId,
                    Operation = "delete",
                    DataSourceId = mockDataSourceId,
                    EntityType = "record",
                    EntityId = 1,
                }
            };
            var result = () => _subscriptionBusiness.BulkCreateSubscriptions(uid, pid, dtos);
            result.Should().ThrowAsync<ValidationException>();
            
            var SubscriptionList = Context.Subscriptions.ToList();
            SubscriptionList.Should().HaveCount(0);
        }
        
        [Fact]
        public async Task BulkCreateSubscriptions_Fails_IfNoActionID()
        {
            var dtos = new List<CreateSubscriptionRequestDto>
            {
                new()
                {
                    UserId = uid,
                    ProjectId = pid,
                    Operation = "delete",
                    DataSourceId = mockDataSourceId,
                    EntityType = "record",
                    EntityId = 1,
                }
            };
        
            var result = () => _subscriptionBusiness.BulkCreateSubscriptions(uid, pid, dtos);
            await result.Should().ThrowAsync<ValidationException>();
            
            var subscriptionList = Context.Subscriptions.ToList();
            subscriptionList.Count.Should().Be(0);
        }
        
        #endregion
        #region UpdateTests
        [Fact]
        public async Task BulkUpdateSubscriptions_Success_UpdatesSubscriptions()
        {
            var subscriptions = new List<Subscription>
            {
                new Subscription
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = mockActionId,
                    Operation = "create",
                    DataSourceId = mockDataSourceId,
                    EntityType = "record",
                    EntityId = 1
                },
                new Subscription
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = mockActionId,
                    Operation = "update",
                    DataSourceId = mockDataSourceId,
                    EntityType = "record",
                    EntityId = 1
                },
                new Subscription
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = mockActionId,
                    Operation = "delete",
                    DataSourceId = mockDataSourceId,
                    EntityType = "record",
                    EntityId = 1
                }
            };
            Context.Subscriptions.AddRange(subscriptions);
            await Context.SaveChangesAsync();
            var dtos = new List<UpdateSubscriptionRequestDto>
            {
                new()
                {
                    Id = subscriptions[0].Id,
                    ActionId = mockActionId,
                    Operation = "delete",
                    DataSourceId = mockDataSourceId,
                    EntityType = "data_source",
                    EntityId = 2
                },
                new()
                {
                    Id = subscriptions[1].Id,
                    ActionId = mockActionId,
                    Operation = "delete",
                    DataSourceId = mockDataSourceId,
                    EntityType = "edge",
                    EntityId = 2
                }
            };
            await _subscriptionBusiness.BulkUpdateSubscriptions(uid, pid, dtos);
            
            var subscriptionList = Context.Subscriptions
                .Where(s => s.Operation == "delete")
                .ToList();
            
            subscriptionList.Should().HaveCount(3);
        }
        
        #endregion
        [Fact]
        public async Task BulkDeleteSubscriptions_Success_DeletesSubscriptions()
        {
            var subscriptions = new List<Subscription>
            {
                new Subscription
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = mockActionId,
                    Operation = "create",
                    DataSourceId = mockDataSourceId,
                    EntityType = "record",
                    EntityId = 1
                },
                new Subscription
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = mockActionId,
                    Operation = "update",
                    DataSourceId = mockDataSourceId,
                    EntityType = "record",
                    EntityId = 1
                },
                new Subscription
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = mockActionId,
                    Operation = "delete",
                    DataSourceId = mockDataSourceId,
                    EntityType = "record",
                    EntityId = 1
                }
            };
            Context.Subscriptions.AddRange(subscriptions);
            await Context.SaveChangesAsync();
            // Delete 2/3 Subscriptions to ensure deletion is done selectively by ID
            var result = () => _subscriptionBusiness.BulkDeleteSubscriptions(uid, pid, new List<long>
            {
                subscriptions[0].Id, 
                subscriptions[1].Id, 
                5 // include a non-existing subscriptionID
            });
            // Ensure Exception about non-maching ID
            await result.Should().ThrowAsync<InvalidOperationException>();
            
            var subscriptionList = Context.Subscriptions.ToList();
            subscriptionList.Should().HaveCount(1);
        }
        [Fact]
        public async Task BulkArchiveSubscriptions_Success_ArchivesSubscriptions()
        {
            var subscriptions = new List<Subscription>
            {
                new Subscription
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = mockActionId,
                    Operation = "create",
                    DataSourceId = mockDataSourceId,
                    EntityType = "record",
                    EntityId = 1
                },
                new Subscription
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = mockActionId,
                    Operation = "update",
                    DataSourceId = mockDataSourceId,
                    EntityType = "record",
                    EntityId = 1
                },
                new Subscription
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = mockActionId,
                    Operation = "delete",
                    DataSourceId = mockDataSourceId,
                    EntityType = "record",
                    EntityId = 1
                }
            };
            Context.Subscriptions.AddRange(subscriptions);
            await Context.SaveChangesAsync();
            
            // Archive 2/3 subscriptions to ensure it selectively archives by ID
            var result =
                await _subscriptionBusiness.BulkArchiveSubscriptions(uid, pid,
                    new List<long> { subscriptions[0].Id, subscriptions[1].Id });
            result.Should().BeTrue();
            var subscriptionList = Context.Subscriptions
                .Where(s => s.IsArchived)
                .ToList();
            
            subscriptionList.Should().HaveCount(2);
        }
        [Fact]
        public async Task BulkUnarchiveSubscriptions_Success_UnarchivesSpecificSubscriptions()
        {
            var subscriptions = new List<Subscription>
            {
                new Subscription
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = mockActionId,
                    Operation = "create",
                    DataSourceId = mockDataSourceId,
                    EntityType = "record",
                    EntityId = 1,
                    IsArchived = true
                },
                new Subscription
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = mockActionId,
                    Operation = "update",
                    DataSourceId = mockDataSourceId,
                    EntityType = "record",
                    EntityId = 1,
                    IsArchived = true
                },
                new Subscription
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = mockActionId,
                    Operation = "delete",
                    DataSourceId = mockDataSourceId,
                    EntityType = "record",
                    EntityId = 1,
                    IsArchived = true
                }
            };
            Context.Subscriptions.AddRange(subscriptions);
            await Context.SaveChangesAsync();
            
            // Unarchive 2/3 subscriptions to ensure it selectively Unarchives by ID
            var result = () => _subscriptionBusiness.BulkUnarchiveSubscriptions(uid, pid, new List<long>
                {
                    subscriptions[0].Id, 
                    subscriptions[1].Id,
                    5 // Include non-existing ID
                });
            await result.Should().ThrowAsync<InvalidOperationException>();
            
            var subscriptionList = Context.Subscriptions
                .Where(s => !s.IsArchived)
                .ToList();
            
            subscriptionList.Should().HaveCount(2);
        }
        #region SubscriptionResponseDto Tests

[Fact]
public void SubscriptionResponseDto_AllProperties_CanBeSetAndRetrieved()
{
    // Arrange
    var now = DateTime.UtcNow;
    var dto = new SubscriptionResponseDto
    {
        Id = 1,
        UserId = uid,
        ProjectId = pid,
        ActionId = 100,
        Operation = "create",
        DataSourceId = 200,
        EntityType = "record",
        EntityId = 300,
        LastUpdatedAt = now,
        LastUpdatedBy = uid,
        IsArchived = false
    };

    // Assert
    Assert.Equal(1, dto.Id);
    Assert.Equal(uid, dto.UserId);
    Assert.Equal(pid, dto.ProjectId);
    Assert.Equal(100, dto.ActionId);
    Assert.Equal("create", dto.Operation);
    Assert.Equal(200, dto.DataSourceId);
    Assert.Equal("record", dto.EntityType);
    Assert.Equal(300, dto.EntityId);
    Assert.Equal(now, dto.LastUpdatedAt);
    Assert.Equal(uid, dto.LastUpdatedBy);
    Assert.False(dto.IsArchived);
}

#endregion

    #region LastUpdatedBy Tests

    [Fact]
    public async Task CreateSubscription_Success_StoresLastUpdatedByUserId()
    {
        // Arrange
        var testSubscription = new Subscription
        {
            UserId = uid,
            ProjectId = pid,
            ActionId = mockActionId,
            Operation = "create",
            DataSourceId = mockDataSourceId,
            EntityType = "record",
            EntityId = 100,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = uid
        };
        
        // Act
        Context.Subscriptions.Add(testSubscription);
        await Context.SaveChangesAsync();

        // Assert
        var savedSubscription = await Context.Subscriptions.FindAsync(testSubscription.Id);
        Assert.NotNull(savedSubscription);
        Assert.Equal(uid, savedSubscription.LastUpdatedBy);
    }

    [Fact]
    public async Task CreateSubscription_Success_NavigationPropertyLoadsUser()
    {
        // Arrange
        var testSubscription = new Subscription
        {
            UserId = uid,
            ProjectId = pid,
            ActionId = mockActionId,
            Operation = "update",
            DataSourceId = mockDataSourceId,
            EntityType = "record",
            EntityId = 101,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = uid
        };
        
        Context.Subscriptions.Add(testSubscription);
        await Context.SaveChangesAsync();

        // Act
        var subscriptionWithUser = await Context.Subscriptions
            .Include(s => s.LastUpdatedByUser)
            .FirstAsync(s => s.Id == testSubscription.Id);
        
        // Assert
        Assert.NotNull(subscriptionWithUser.LastUpdatedByUser);
        Assert.Equal("test_user", subscriptionWithUser.LastUpdatedByUser.Name);
        Assert.Equal("Fake@gmail.com", subscriptionWithUser.LastUpdatedByUser.Email);
        Assert.Equal(uid, subscriptionWithUser.LastUpdatedBy);
    }

    [Fact]
    public async Task CreateSubscription_Success_WithNullLastUpdatedBy()
    {
        // Arrange
        var testSubscription = new Subscription
        {
            UserId = uid,
            ProjectId = pid,
            ActionId = mockActionId,
            Operation = "delete",
            DataSourceId = mockDataSourceId,
            EntityType = "record",
            EntityId = 102,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };
        
        // Act
        Context.Subscriptions.Add(testSubscription);
        await Context.SaveChangesAsync();

        // Assert
        var savedSubscription = await Context.Subscriptions.FindAsync(testSubscription.Id);
        Assert.NotNull(savedSubscription);
        Assert.Null(savedSubscription.LastUpdatedBy);
        
        var subscriptionWithUser = await Context.Subscriptions
            .Include(s => s.LastUpdatedByUser)
            .FirstAsync(s => s.Id == testSubscription.Id);
        
        Assert.Null(subscriptionWithUser.LastUpdatedByUser);
    }

    [Fact]
    public async Task UpdateSubscription_Success_UpdatesLastUpdatedByUserId()
    {
        // Arrange
        var testSubscription = new Subscription
        {
            UserId = uid,
            ProjectId = pid,
            ActionId = mockActionId,
            Operation = "create",
            DataSourceId = mockDataSourceId,
            EntityType = "record",
            EntityId = 103,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };
        Context.Subscriptions.Add(testSubscription);
        await Context.SaveChangesAsync();

        // Act
        testSubscription.LastUpdatedBy = uid;
        testSubscription.Operation = "update";
        testSubscription.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        
        Context.Subscriptions.Update(testSubscription);
        await Context.SaveChangesAsync();

        // Assert
        var updatedSubscription = await Context.Subscriptions
            .Include(s => s.LastUpdatedByUser)
            .FirstAsync(s => s.Id == testSubscription.Id);
        
        Assert.Equal(uid, updatedSubscription.LastUpdatedBy);
        Assert.NotNull(updatedSubscription.LastUpdatedByUser);
        Assert.Equal("test_user", updatedSubscription.LastUpdatedByUser.Name);
        Assert.Equal("update", updatedSubscription.Operation);
    }

    #endregion

        protected override async Task SeedTestDataAsync()
        {
            
            await base.SeedTestDataAsync();
            
            var user = new User 
            { 
                Name = "test_user", 
                Email = "Fake@gmail.com",
                Password = "test_password",
                IsArchived = false
            };
            Context.Users.Add(user);
            await Context.SaveChangesAsync();
            uid = user.Id;
            
            var project = new Project { Name = "Project 1" };
            Context.Projects.Add(project);
            await Context.SaveChangesAsync();
            pid = project.Id;
            var action = new Action
            {
                Name = "Action1",
                ProjectId = pid,
                LastUpdatedBy =uid,
                LastUpdatedAt = now
            };
            Context.Actions.Add(action);
            await Context.SaveChangesAsync();
            mockActionId = action.Id;
            var dataSource = new DataSource
            {
                Name = "DataSource2",
                ProjectId = pid,
                LastUpdatedBy = uid ,
                LastUpdatedAt = now
            };
            Context.DataSources.Add(dataSource);
            await Context.SaveChangesAsync();
            mockDataSourceId = dataSource.Id;
            Context.Users.Add(user);
            await Context.SaveChangesAsync();
            uid = user.Id;
        }
    }
}