using System.ComponentModel.DataAnnotations;
using deeplynx.business; using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using Action = deeplynx.datalayer.Models.Action;
using Moq;


namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class SubscriptionBusinessTests : IntegrationTestBase
    {
        private SubscriptionBusiness _subscriptionBusiness = null!;
        
        public long aid;
        public long did;
        public long pid;
        public long uid;
        private readonly DateTime now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        
        public SubscriptionBusinessTests(TestSuiteFixture fixture) : base(fixture) { }
        
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _subscriptionBusiness = new SubscriptionBusiness(Context, _cacheBusiness);
        }

        #region GetAllSubscriptions Tests
  
        [Fact]
        public async Task GetAllSubscriptions_Success_ReturnsSubscriptions()
        {
            // Arrange
            var subscription = new Subscription
            {
                UserId = uid,
                ProjectId = pid,
                ActionId = aid,
                Operation = "create",
                DataSourceId = did,
                EntityType = "record",
                EntityId = 1
            };
            Context.Subscriptions.Add(subscription);
            await Context.SaveChangesAsync();
    
            // Act
            var result = await _subscriptionBusiness.GetAllSubscriptions(uid, pid, false);
    
            // Assert
            Assert.Single(result);
    
            var actualSubscription = result.First();
            
            Assert.Equal(uid, actualSubscription.UserId);
            Assert.Equal(pid, actualSubscription.ProjectId);
            Assert.Equal(aid, actualSubscription.ActionId);
            Assert.Equal("create", actualSubscription.Operation);
            Assert.Equal(did, actualSubscription.DataSourceId);
            Assert.Equal("record", actualSubscription.EntityType);
            Assert.Equal(1, actualSubscription.EntityId);
        }
        
        [Fact]
        public async Task GetSubscription_Success_ReturnsSubscription()
        {
            // Arrange
            var subscription = new Subscription
            {
                UserId = uid,
                ProjectId = pid,
                ActionId = aid,
                Operation = "delete",
                DataSourceId = null,
                EntityType = "record",
                EntityId = 1
            };
            Context.Subscriptions.Add(subscription);
            await Context.SaveChangesAsync();
    
            // Act
            var result = await _subscriptionBusiness.GetSubscription(uid, pid, subscription.Id, false);
    
            // Assert
            Assert.NotNull(result);
            Assert.Equal(uid, result.UserId);
            Assert.Equal(pid, result.ProjectId);
            Assert.Equal(aid, result.ActionId);
            Assert.Equal("delete", result.Operation);
            Assert.Equal("record", result.EntityType);
            Assert.Equal(1, result.EntityId);
        }
        
        #endregion
        
        #region BulkCreateSubscriptions Tests
        
        [Fact]
        public async Task BulkCreateSubscriptions_Success_CreatesSubscriptions()
        {
            // Arrange
            var dtos = new List<CreateSubscriptionRequestDto>
            {
                new()
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = aid,
                    Operation = "delete",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 1,
                },
                new()
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = aid,
                    Operation = "update",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 3,
                },
            };
    
            // Act
            var result = await _subscriptionBusiness.BulkCreateSubscriptions(uid, pid, dtos);
    
            // Assert
            Assert.Equal(2, result.Count);
    
            var firstSubscription = result.First();
            Assert.Equal(uid, firstSubscription.UserId);
            Assert.Equal(pid, firstSubscription.ProjectId);
            Assert.Equal(aid, firstSubscription.ActionId);
            Assert.Equal("delete", firstSubscription.Operation);
            Assert.Equal("record", firstSubscription.EntityType);
            Assert.Equal(1, firstSubscription.EntityId);
    
            var lastSubscription = result.Last();
            Assert.Equal(uid, lastSubscription.UserId);
            Assert.Equal(pid, lastSubscription.ProjectId);
            Assert.Equal(aid, lastSubscription.ActionId);
            Assert.Equal("update", lastSubscription.Operation);
            Assert.Equal(did, lastSubscription.DataSourceId);
            Assert.Equal("record", lastSubscription.EntityType);
            Assert.Equal(3, lastSubscription.EntityId);
        }
        
        [Fact]
        public async Task BulkCreateSubscriptions_Fails_IfNoUserID()
        {
            // Arrange
            var dtos = new List<CreateSubscriptionRequestDto>
            {
                new()
                {
                    ProjectId = pid,
                    ActionId = aid,
                    Operation = "delete",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 1,
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _subscriptionBusiness.BulkCreateSubscriptions(uid, pid, dtos));
    
            var subscriptionList = await Context.Subscriptions.ToListAsync();
            Assert.Empty(subscriptionList);
        }
        
        [Fact]
        public async Task BulkCreateSubscriptions_Fails_IfNoProjectID()
        {
            // Arrange
            var dtos = new List<CreateSubscriptionRequestDto>
            {
                new()
                {
                    UserId = uid,
                    ActionId = aid,
                    Operation = "delete",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 1,
                }
            };
    
            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _subscriptionBusiness.BulkCreateSubscriptions(uid, pid, dtos));
    
            var subscriptionList = await Context.Subscriptions.ToListAsync();
            Assert.Empty(subscriptionList);
        }
        
        [Fact]
        public async Task BulkCreateSubscriptions_Fails_IfNoActionID()
        {
            // Arrange
            var dtos = new List<CreateSubscriptionRequestDto>
            {
                new()
                {
                    UserId = uid,
                    ProjectId = pid,
                    Operation = "delete",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 1,
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _subscriptionBusiness.BulkCreateSubscriptions(uid, pid, dtos));
    
            var subscriptionList = await Context.Subscriptions.ToListAsync();
            Assert.Empty(subscriptionList);
        }
        
        #endregion
        
        #region BulkUpdateSubscriptions Tests
        
        [Fact]
        public async Task BulkUpdateSubscriptions_Success_UpdatesSubscriptions()
        {
            // Arrange
            var subscriptions = new List<Subscription>
            {
                new()
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = aid,
                    Operation = "create",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 1
                },
                new()
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = aid,
                    Operation = "update",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 1
                },
                new()
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = aid,
                    Operation = "delete",
                    DataSourceId = did,
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
                    ActionId = aid,
                    Operation = "delete",
                    DataSourceId = did,
                    EntityType = "data_source",
                    EntityId = 2
                },
                new()
                {
                    Id = subscriptions[1].Id,
                    ActionId = aid,
                    Operation = "delete",
                    DataSourceId = did,
                    EntityType = "edge",
                    EntityId = 2
                }
            };
    
            // Act
            await _subscriptionBusiness.BulkUpdateSubscriptions(uid, pid, dtos);
    
            // Assert
            var subscriptionList = await Context.Subscriptions
                .Where(s => s.Operation == "delete")
                .ToListAsync();
    
            Assert.Equal(3, subscriptionList.Count);
        }
        
        #endregion
        
        #region BulkDeleteSubSubscriptions Tests
        
        [Fact]
        public async Task BulkDeleteSubscriptions_Success_DeletesSubscriptions()
        {
            // Arrange
            var subscriptions = new List<Subscription>
            {
                new()
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = aid,
                    Operation = "create",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 1
                },
                new()
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = aid,
                    Operation = "update",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 1
                },
                new()
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = aid,
                    Operation = "delete",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 1
                }
            };
            Context.Subscriptions.AddRange(subscriptions);
            await Context.SaveChangesAsync();
    
            // Delete 2/3 Subscriptions to ensure deletion is done selectively by ID
            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _subscriptionBusiness.BulkDeleteSubscriptions(uid, pid, new List<long>
                {
                    subscriptions[0].Id, 
                    subscriptions[1].Id, 
                    5 // Include a non-existing subscriptionID
                }));
            
            Assert.Contains("Some subscriptions were not deleted because they do not exist.", exception.Message);
    
            var subscriptionList = await Context.Subscriptions.ToListAsync();
            Assert.Single(subscriptionList);
        }
        
        #endregion
        
        #region BulkArchiveSubscriptions Tests
        
        [Fact]
        public async Task BulkArchiveSubscriptions_Success_ArchivesSubscriptions()
        {
            // Arrange
            var subscriptions = new List<Subscription>
            {
                new()
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = aid,
                    Operation = "create",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 1
                },
                new()
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = aid,
                    Operation = "update",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 1
                },
                new()
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = aid,
                    Operation = "delete",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 1
                }
            };
            Context.Subscriptions.AddRange(subscriptions);
            await Context.SaveChangesAsync();
    
            // Archive 2/3 subscriptions to ensure it selectively archives by ID
            // Act
            var result = await _subscriptionBusiness.BulkArchiveSubscriptions(uid, pid,
                new List<long> { subscriptions[0].Id, subscriptions[1].Id });
    
            // Assert
            Assert.True(result);
    
            var subscriptionList = await Context.Subscriptions
                .Where(s => s.IsArchived)
                .ToListAsync();
    
            Assert.Equal(2, subscriptionList.Count);
        }
        
        #endregion
        
        #region BulkUnarchiveSubscriptions Tests
        
        [Fact]
        public async Task BulkUnarchiveSubscriptions_Success_UnarchivesSpecificSubscriptions()
        {
            // Arrange
            var subscriptions = new List<Subscription>
            {
                new()
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = aid,
                    Operation = "create",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 1,
                    IsArchived = true
                },
                new()
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = aid,
                    Operation = "update",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 1,
                    IsArchived = true
                },
                new()
                {
                    UserId = uid,
                    ProjectId = pid,
                    ActionId = aid,
                    Operation = "delete",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 1,
                    IsArchived = true
                }
            };
            Context.Subscriptions.AddRange(subscriptions);
            await Context.SaveChangesAsync();
    
            // Unarchive 2/3 subscriptions to ensure it selectively Unarchives by ID
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _subscriptionBusiness.BulkUnarchiveSubscriptions(uid, pid, new List<long>
                {
                    subscriptions[0].Id, 
                    subscriptions[1].Id,
                    5 // Include non-existing ID
                }));
    
            var subscriptionList = await Context.Subscriptions
                .Where(s => !s.IsArchived)
                .ToListAsync();
    
            Assert.Equal(2, subscriptionList.Count);
        }
        
        #endregion
        
        protected override async Task SeedTestDataAsync()
        {
            await base.SeedTestDataAsync();
            
            // Add project
            var project = new Project { Name = "Project 1" };
            Context.Projects.Add(project);
            await Context.SaveChangesAsync();
            pid = project.Id;
            
            // Add action
            var action = new Action
            {
                Name = "Action1",
                ProjectId = pid,
                LastUpdatedBy = "user123",
                LastUpdatedAt = now
            };
            Context.Actions.Add(action);
            await Context.SaveChangesAsync();
            aid = action.Id;
            
            // Add datasource
            var dataSource = new DataSource
            {
                Name = "DataSource2",
                ProjectId = pid,
                LastUpdatedBy = "user123",
                LastUpdatedAt = now
            };
            Context.DataSources.Add(dataSource);
            await Context.SaveChangesAsync();
            did = dataSource.Id;
            
            // Add user
            var user = new User { Name = "test_user", Email = "Fake@gmail.com" };
            Context.Users.Add(user);
            await Context.SaveChangesAsync();
            uid = user.Id;
        }
    }
}