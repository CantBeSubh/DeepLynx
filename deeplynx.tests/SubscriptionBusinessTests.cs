using System.ComponentModel.DataAnnotations;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using Action = deeplynx.datalayer.Models.Action;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class SubscriptionBusinessTests : IntegrationTestBase
    {
        private SubscriptionBusiness _subscriptionBusiness = null!;
        
        public long aid;    // Action ID
        public long did;    // Datasource ID
        public long pid;    // Project ID
        public long oid;    // Organization ID
        public long uid;    // User ID
        private readonly DateTime now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        
        public SubscriptionBusinessTests(TestSuiteFixture fixture) : base(fixture) { }
        
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _subscriptionBusiness = new SubscriptionBusiness(Context, _cacheBusiness);
            await _cacheBusiness.SetAsync("projects", (List<ProjectResponseDto>?)null, TimeSpan.FromSeconds(1));
        }

        #region GetAllSubscriptions Tests
  
        [Fact]
        public async Task GetAllSubscriptions_Success_OrganizationLevel()
        {
            // Arrange
            var subscription = new Subscription
            {
                UserId = uid,
                OrganizationId = oid,
                ProjectId = null,
                ActionId = aid,
                Operation = "create",
                DataSourceId = did,
                EntityType = "record",
                EntityId = 1
            };
            Context.Subscriptions.Add(subscription);
            await Context.SaveChangesAsync();
    
            // Act
            var result = await _subscriptionBusiness.GetAllSubscriptions(uid, oid, false, null);
    
            // Assert
            Assert.Single(result);
    
            var actualSubscription = result.First();
            Assert.Equal(uid, actualSubscription.UserId);
            Assert.Equal(oid, actualSubscription.OrganizationId);
            Assert.Null(actualSubscription.ProjectId);
            Assert.Equal(aid, actualSubscription.ActionId);
            Assert.Equal("create", actualSubscription.Operation);
            Assert.Equal(did, actualSubscription.DataSourceId);
            Assert.Equal("record", actualSubscription.EntityType);
            Assert.Equal(1, actualSubscription.EntityId);
        }

        [Fact]
        public async Task GetAllSubscriptions_Success_ProjectLevel()
        {
            // Arrange
            var subscription = new Subscription
            {
                UserId = uid,
                OrganizationId = oid,
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
            var result = await _subscriptionBusiness.GetAllSubscriptions(uid, oid, false, pid);
    
            // Assert
            Assert.Single(result);
    
            var actualSubscription = result.First();
            Assert.Equal(uid, actualSubscription.UserId);
            Assert.Equal(oid, actualSubscription.OrganizationId);
            Assert.Equal(pid, actualSubscription.ProjectId);
            Assert.Equal(aid, actualSubscription.ActionId);
            Assert.Equal("create", actualSubscription.Operation);
            Assert.Equal(did, actualSubscription.DataSourceId);
            Assert.Equal("record", actualSubscription.EntityType);
            Assert.Equal(1, actualSubscription.EntityId);
        }

        [Fact]
        public async Task GetAllSubscriptions_FiltersCorrectly_ByScope()
        {
            // Arrange - Create both organization and project level subscriptions
            var orgSubscription = new Subscription
            {
                UserId = uid,
                OrganizationId = oid,
                ProjectId = null,
                ActionId = aid,
                Operation = "create",
                DataSourceId = did,
                EntityType = "record",
                EntityId = 1
            };
            var projectSubscription = new Subscription
            {
                UserId = uid,
                OrganizationId = oid,
                ProjectId = pid,
                ActionId = aid,
                Operation = "delete",
                DataSourceId = did,
                EntityType = "edge",
                EntityId = 2
            };
            Context.Subscriptions.AddRange(orgSubscription, projectSubscription);
            await Context.SaveChangesAsync();
    
            // Act - Get organization level subscriptions
            var orgResult = await _subscriptionBusiness.GetAllSubscriptions(uid, oid, false, null);
            
            // Assert
            Assert.Single(orgResult);
            Assert.Null(orgResult.First().ProjectId);
            
            // Act - Get project level subscriptions
            var projectResult = await _subscriptionBusiness.GetAllSubscriptions(uid, oid, false, pid);
            
            // Assert
            Assert.Single(projectResult);
            Assert.Equal(pid, projectResult.First().ProjectId);
        }
        
        [Fact]
        public async Task GetSubscription_Success_OrganizationLevel()
        {
            // Arrange
            var subscription = new Subscription
            {
                UserId = uid,
                OrganizationId = oid,
                ProjectId = null,
                ActionId = aid,
                Operation = "delete",
                DataSourceId = null,
                EntityType = "record",
                EntityId = 1
            };
            Context.Subscriptions.Add(subscription);
            await Context.SaveChangesAsync();
    
            // Act
            var result = await _subscriptionBusiness.GetSubscription(uid, subscription.Id, oid, false, null);
    
            // Assert
            Assert.NotNull(result);
            Assert.Equal(uid, result.UserId);
            Assert.Equal(oid, result.OrganizationId);
            Assert.Null(result.ProjectId);
            Assert.Equal(aid, result.ActionId);
            Assert.Equal("delete", result.Operation);
            Assert.Equal("record", result.EntityType);
            Assert.Equal(1, result.EntityId);
        }

        [Fact]
        public async Task GetSubscription_Success_ProjectLevel()
        {
            // Arrange
            var subscription = new Subscription
            {
                UserId = uid,
                OrganizationId = oid,
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
            var result = await _subscriptionBusiness.GetSubscription(uid, subscription.Id, oid, false, pid);
    
            // Assert
            Assert.NotNull(result);
            Assert.Equal(uid, result.UserId);
            Assert.Equal(oid, result.OrganizationId);
            Assert.Equal(pid, result.ProjectId);
            Assert.Equal(aid, result.ActionId);
            Assert.Equal("delete", result.Operation);
            Assert.Equal("record", result.EntityType);
            Assert.Equal(1, result.EntityId);
        }

        [Fact]
        public async Task GetSubscription_ThrowsUnauthorized_WhenScopeMismatch()
        {
            // Arrange - Create a project-level subscription
            var subscription = new Subscription
            {
                UserId = uid,
                OrganizationId = oid,
                ProjectId = pid,
                ActionId = aid,
                Operation = "delete",
                DataSourceId = null,
                EntityType = "record",
                EntityId = 1
            };
            Context.Subscriptions.Add(subscription);
            await Context.SaveChangesAsync();
    
            // Act & Assert - Try to access as organization-level
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _subscriptionBusiness.GetSubscription(uid, subscription.Id, oid, false, null));
        }
        
        #endregion
        
        #region BulkCreateSubscriptions Tests
        
        [Fact]
        public async Task BulkCreateSubscriptions_Success_OrganizationLevel()
        {
            // Arrange
            var dtos = new List<CreateSubscriptionRequestDto>
            {
                new()
                {
                    ActionId = aid,
                    Operation = "delete",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 1,
                },
                new()
                {
                    ActionId = aid,
                    Operation = "update",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 3,
                },
            };
    
            // Act
            var result = await _subscriptionBusiness.BulkCreateSubscriptions(uid, oid, dtos, null);
    
            // Assert
            Assert.Equal(2, result.Count);
    
            var firstSubscription = result.First();
            Assert.Equal(uid, firstSubscription.UserId);
            Assert.Equal(oid, firstSubscription.OrganizationId);
            Assert.Null(firstSubscription.ProjectId);
            Assert.Equal(aid, firstSubscription.ActionId);
            Assert.Equal("delete", firstSubscription.Operation);
            Assert.Equal("record", firstSubscription.EntityType);
            Assert.Equal(1, firstSubscription.EntityId);
    
            var lastSubscription = result.Last();
            Assert.Equal(uid, lastSubscription.UserId);
            Assert.Equal(oid, lastSubscription.OrganizationId);
            Assert.Null(lastSubscription.ProjectId);
            Assert.Equal(aid, lastSubscription.ActionId);
            Assert.Equal("update", lastSubscription.Operation);
            Assert.Equal(did, lastSubscription.DataSourceId);
            Assert.Equal("record", lastSubscription.EntityType);
            Assert.Equal(3, lastSubscription.EntityId);
        }

        [Fact]
        public async Task BulkCreateSubscriptions_Success_ProjectLevel()
        {
            // Arrange
            var dtos = new List<CreateSubscriptionRequestDto>
            {
                new()
                {
                    ActionId = aid,
                    Operation = "delete",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 1,
                },
                new()
                {
                    ActionId = aid,
                    Operation = "update",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 3,
                },
            };
    
            // Act
            var result = await _subscriptionBusiness.BulkCreateSubscriptions(uid, oid, dtos, pid);
    
            // Assert
            Assert.Equal(2, result.Count);
    
            var firstSubscription = result.First();
            Assert.Equal(uid, firstSubscription.UserId);
            Assert.Equal(oid, firstSubscription.OrganizationId);
            Assert.Equal(pid, firstSubscription.ProjectId);
            Assert.Equal(aid, firstSubscription.ActionId);
            Assert.Equal("delete", firstSubscription.Operation);
            Assert.Equal("record", firstSubscription.EntityType);
            Assert.Equal(1, firstSubscription.EntityId);
    
            var lastSubscription = result.Last();
            Assert.Equal(uid, lastSubscription.UserId);
            Assert.Equal(oid, lastSubscription.OrganizationId);
            Assert.Equal(pid, lastSubscription.ProjectId);
            Assert.Equal(aid, lastSubscription.ActionId);
            Assert.Equal("update", lastSubscription.Operation);
            Assert.Equal(did, lastSubscription.DataSourceId);
            Assert.Equal("record", lastSubscription.EntityType);
            Assert.Equal(3, lastSubscription.EntityId);
        }

        [Fact]
        public async Task BulkCreateSubscriptions_HandlesConflicts_OrganizationLevel()
        {
            // Arrange - Create existing subscription
            var existing = new Subscription
            {
                UserId = uid,
                OrganizationId = oid,
                ProjectId = null,
                ActionId = aid,
                Operation = "delete",
                DataSourceId = did,
                EntityType = "record",
                EntityId = 1
            };
            Context.Subscriptions.Add(existing);
            await Context.SaveChangesAsync();

            var dtos = new List<CreateSubscriptionRequestDto>
            {
                new()
                {
                    ActionId = aid,
                    Operation = "delete",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 1, // Duplicate
                },
                new()
                {
                    ActionId = aid,
                    Operation = "update",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 3, // New
                },
            };
    
            // Act
            var result = await _subscriptionBusiness.BulkCreateSubscriptions(uid, oid, dtos, null);
    
            // Assert - Only the new one should be created due to ON CONFLICT DO NOTHING
            Assert.Single(result);
            Assert.Equal(3, result.First().EntityId);
        }
        
        #endregion
        
        #region BulkUpdateSubscriptions Tests
        
        [Fact]
        public async Task BulkUpdateSubscriptions_Success_OrganizationLevel()
        {
            // Arrange
            var subscriptions = new List<Subscription>
            {
                new()
                {
                    UserId = uid,
                    OrganizationId = oid,
                    ProjectId = null,
                    ActionId = aid,
                    Operation = "create",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 1
                },
                new()
                {
                    UserId = uid,
                    OrganizationId = oid,
                    ProjectId = null,
                    ActionId = aid,
                    Operation = "update",
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
            var result = await _subscriptionBusiness.BulkUpdateSubscriptions(uid, oid, dtos, null);
    
            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, s => 
            {
                Assert.Equal("delete", s.Operation);
                Assert.Equal(oid, s.OrganizationId);
                Assert.Null(s.ProjectId);
            });
        }

        [Fact]
        public async Task BulkUpdateSubscriptions_Success_ProjectLevel()
        {
            // Arrange
            var subscriptions = new List<Subscription>
            {
                new()
                {
                    UserId = uid,
                    OrganizationId = oid,
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
                    OrganizationId = oid,
                    ProjectId = pid,
                    ActionId = aid,
                    Operation = "update",
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
            var result = await _subscriptionBusiness.BulkUpdateSubscriptions(uid, oid, dtos, pid);
    
            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, s => 
            {
                Assert.Equal("delete", s.Operation);
                Assert.Equal(oid, s.OrganizationId);
                Assert.Equal(pid, s.ProjectId);
            });
        }

        [Fact]
        public async Task BulkUpdateSubscriptions_Fails_WhenScopeMismatch()
        {
            // Arrange - Create organization-level subscription
            var subscription = new Subscription
            {
                UserId = uid,
                OrganizationId = oid,
                ProjectId = null,
                ActionId = aid,
                Operation = "create",
                DataSourceId = did,
                EntityType = "record",
                EntityId = 1
            };
            Context.Subscriptions.Add(subscription);
            await Context.SaveChangesAsync();
    
            var dtos = new List<UpdateSubscriptionRequestDto>
            {
                new()
                {
                    Id = subscription.Id,
                    ActionId = aid,
                    Operation = "delete",
                    DataSourceId = did,
                    EntityType = "edge",
                    EntityId = 2
                }
            };
    
            // Act & Assert - Try to update as project-level
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _subscriptionBusiness.BulkUpdateSubscriptions(uid, oid, dtos, pid));
        }
        
        #endregion
        
        #region BulkDeleteSubscriptions Tests
        
        [Fact]
        public async Task BulkDeleteSubscriptions_Success_OrganizationLevel()
        {
            // Arrange
            var subscriptions = new List<Subscription>
            {
                new()
                {
                    UserId = uid,
                    OrganizationId = oid,
                    ProjectId = null,
                    ActionId = aid,
                    Operation = "create",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 1
                },
                new()
                {
                    UserId = uid,
                    OrganizationId = oid,
                    ProjectId = null,
                    ActionId = aid,
                    Operation = "update",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 1
                },
                new()
                {
                    UserId = uid,
                    OrganizationId = oid,
                    ProjectId = null,
                    ActionId = aid,
                    Operation = "delete",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 1
                }
            };
            Context.Subscriptions.AddRange(subscriptions);
            await Context.SaveChangesAsync();
    
            // Act & Assert - Delete 2/3 with one non-existing ID
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _subscriptionBusiness.BulkDeleteSubscriptions(uid, oid, new List<long>
                {
                    subscriptions[0].Id, 
                    subscriptions[1].Id, 
                    99999
                }, null));
            
            Assert.Contains("Some subscriptions were not deleted because they do not exist.", exception.Message);
    
            var subscriptionList = await Context.Subscriptions
                .Where(s => s.OrganizationId == oid && s.ProjectId == null)
                .ToListAsync();
            Assert.Single(subscriptionList);
        }

        [Fact]
        public async Task BulkDeleteSubscriptions_Success_ProjectLevel()
        {
            // Arrange
            var subscriptions = new List<Subscription>
            {
                new()
                {
                    UserId = uid,
                    OrganizationId = oid,
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
                    OrganizationId = oid,
                    ProjectId = pid,
                    ActionId = aid,
                    Operation = "update",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 2
                }
            };
            Context.Subscriptions.AddRange(subscriptions);
            await Context.SaveChangesAsync();
    
            // Act
            var result = await _subscriptionBusiness.BulkDeleteSubscriptions(uid, oid, 
                new List<long> { subscriptions[0].Id, subscriptions[1].Id }, pid);
            
            // Assert
            Assert.True(result);
    
            var subscriptionList = await Context.Subscriptions
                .Where(s => s.ProjectId == pid)
                .ToListAsync();
            Assert.Empty(subscriptionList);
        }

        [Fact]
        public async Task BulkDeleteSubscriptions_DoesNotDelete_WrongScope()
        {
            // Arrange - Create both org and project level subscriptions
            var orgSubscription = new Subscription
            {
                UserId = uid,
                OrganizationId = oid,
                ProjectId = null,
                ActionId = aid,
                Operation = "create",
                DataSourceId = did,
                EntityType = "record",
                EntityId = 1
            };
            var projectSubscription = new Subscription
            {
                UserId = uid,
                OrganizationId = oid,
                ProjectId = pid,
                ActionId = aid,
                Operation = "delete",
                DataSourceId = did,
                EntityType = "record",
                EntityId = 2
            };
            Context.Subscriptions.AddRange(orgSubscription, projectSubscription);
            await Context.SaveChangesAsync();
    
            // Act & Assert - Try to delete org subscription using project scope
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _subscriptionBusiness.BulkDeleteSubscriptions(uid, oid, 
                    new List<long> { orgSubscription.Id }, pid));
            
            // Verify org subscription still exists
            var orgSub = await Context.Subscriptions.FindAsync(orgSubscription.Id);
            Assert.NotNull(orgSub);
        }
        
        #endregion
        
        #region BulkArchiveSubscriptions Tests
        
        [Fact]
        public async Task BulkArchiveSubscriptions_Success_OrganizationLevel()
        {
            // Arrange
            var subscriptions = new List<Subscription>
            {
                new()
                {
                    UserId = uid,
                    OrganizationId = oid,
                    ProjectId = null,
                    ActionId = aid,
                    Operation = "create",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 1,
                    LastUpdatedBy = uid, 
                    LastUpdatedAt = now  
                },
                new()
                {
                    UserId = uid,
                    OrganizationId = oid,
                    ProjectId = null,
                    ActionId = aid,
                    Operation = "update",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 1,
                    LastUpdatedBy = uid, 
                    LastUpdatedAt = now  
                }
            };
            Context.Subscriptions.AddRange(subscriptions);
            await Context.SaveChangesAsync();
    
            // Act
            var result = await _subscriptionBusiness.BulkArchiveSubscriptions(uid, oid,
                new List<long> { subscriptions[0].Id, subscriptions[1].Id }, null);
    
            // Assert
            Assert.True(result);
    
            var subscriptionList = await Context.Subscriptions
                .Where(s => s.IsArchived && s.OrganizationId == oid && s.ProjectId == null)
                .ToListAsync();
    
            Assert.Equal(2, subscriptionList.Count);
        }

        [Fact]
        public async Task BulkArchiveSubscriptions_Success_ProjectLevel()
        {
            // Arrange
            var subscriptions = new List<Subscription>
            {
                new()
                {
                    UserId = uid,
                    OrganizationId = oid,
                    ProjectId = pid,
                    ActionId = aid,
                    Operation = "create",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 1,
                    LastUpdatedBy = uid, 
                    LastUpdatedAt = now  
                },
                new()
                {
                    UserId = uid,
                    OrganizationId = oid,
                    ProjectId = pid,
                    ActionId = aid,
                    Operation = "update",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 2,
                    LastUpdatedBy = uid, 
                    LastUpdatedAt = now  
                }
            };
            Context.Subscriptions.AddRange(subscriptions);
            await Context.SaveChangesAsync();
    
            // Act
            var result = await _subscriptionBusiness.BulkArchiveSubscriptions(uid, oid,
                new List<long> { subscriptions[0].Id, subscriptions[1].Id }, pid);
    
            // Assert
            Assert.True(result);
    
            var subscriptionList = await Context.Subscriptions
                .Where(s => s.IsArchived && s.ProjectId == pid)
                .ToListAsync();
    
            Assert.Equal(2, subscriptionList.Count);
        }
        
        #endregion
        
        #region BulkUnarchiveSubscriptions Tests
        
        [Fact]
        public async Task BulkUnarchiveSubscriptions_Success_OrganizationLevel()
        {
            // Arrange
            var subscriptions = new List<Subscription>
            {
                new()
                {
                    UserId = uid,
                    OrganizationId = oid,
                    ProjectId = null,
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
                    OrganizationId = oid,
                    ProjectId = null,
                    ActionId = aid,
                    Operation = "update",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 2,
                    IsArchived = true
                }
            };
            Context.Subscriptions.AddRange(subscriptions);
            await Context.SaveChangesAsync();
    
            // Act
            var result = await _subscriptionBusiness.BulkUnarchiveSubscriptions(uid, oid, 
                new List<long> { subscriptions[0].Id, subscriptions[1].Id }, null);
    
            // Assert
            Assert.True(result);
    
            var subscriptionList = await Context.Subscriptions
                .Where(s => !s.IsArchived && s.OrganizationId == oid && s.ProjectId == null)
                .ToListAsync();
    
            Assert.Equal(2, subscriptionList.Count);
        }

        [Fact]
        public async Task BulkUnarchiveSubscriptions_Success_ProjectLevel()
        {
            // Arrange
            var subscriptions = new List<Subscription>
            {
                new()
                {
                    UserId = uid,
                    OrganizationId = oid,
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
                    OrganizationId = oid,
                    ProjectId = pid,
                    ActionId = aid,
                    Operation = "update",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 2,
                    IsArchived = true
                }
            };
            Context.Subscriptions.AddRange(subscriptions);
            await Context.SaveChangesAsync();
    
            // Act
            var result = await _subscriptionBusiness.BulkUnarchiveSubscriptions(uid, oid,
                new List<long> { subscriptions[0].Id, subscriptions[1].Id }, pid);
    
            // Assert
            Assert.True(result);
    
            var subscriptionList = await Context.Subscriptions
                .Where(s => !s.IsArchived && s.ProjectId == pid)
                .ToListAsync();
    
            Assert.Equal(2, subscriptionList.Count);
        }

        [Fact]
        public async Task BulkUnarchiveSubscriptions_Fails_WithNonExistingId()
        {
            // Arrange
            var subscriptions = new List<Subscription>
            {
                new()
                {
                    UserId = uid,
                    OrganizationId = oid,
                    ProjectId = null,
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
                    OrganizationId = oid,
                    ProjectId = null,
                    ActionId = aid,
                    Operation = "update",
                    DataSourceId = did,
                    EntityType = "record",
                    EntityId = 2,
                    IsArchived = true
                }
            };
            Context.Subscriptions.AddRange(subscriptions);
            await Context.SaveChangesAsync();
    
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _subscriptionBusiness.BulkUnarchiveSubscriptions(uid, oid, new List<long>
                {
                    subscriptions[0].Id, 
                    subscriptions[1].Id,
                    99999 // Non-existing ID
                }, null));
    
            var subscriptionList = await Context.Subscriptions
                .Where(s => !s.IsArchived)
                .ToListAsync();
    
            Assert.Equal(2, subscriptionList.Count);
        }
        
        #endregion
        
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
                OrganizationId = oid,
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
            Assert.Equal(oid, dto.OrganizationId);
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
                OrganizationId = oid,
                ProjectId = pid,
                ActionId = aid,
                Operation = "create",
                DataSourceId = did,
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
                OrganizationId = oid,
                ProjectId = pid,
                ActionId = aid,
                Operation = "update",
                DataSourceId = did,
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
                OrganizationId = oid,
                ProjectId = pid,
                ActionId = aid,
                Operation = "delete",
                DataSourceId = did,
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
                OrganizationId = oid,
                ProjectId = pid,
                ActionId = aid,
                Operation = "create",
                DataSourceId = did,
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
            
            var organization = new Organization { Name = "Test Organization" };
            Context.Organizations.Add(organization);
            await Context.SaveChangesAsync();
            oid = organization.Id;
            
            var project = new Project {
                Name = "Project 1", 
                LastUpdatedBy = uid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                OrganizationId = oid
            };
            Context.Projects.Add(project);
            await Context.SaveChangesAsync();
            pid = project.Id;
            
            // Add action
            var action = new Action
            {
                Name = "Action1",
                ProjectId = pid,
                LastUpdatedBy = uid,
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
                LastUpdatedBy = uid,
                LastUpdatedAt = now
            };
            Context.DataSources.Add(dataSource);
            await Context.SaveChangesAsync();
            did = dataSource.Id;
        }
    }
}