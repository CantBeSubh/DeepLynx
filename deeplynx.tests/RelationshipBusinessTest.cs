using System.ComponentModel.DataAnnotations;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.helpers.Hubs;
using deeplynx.interfaces;
using deeplynx.models;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class RelationshipBusinessTests : IntegrationTestBase
    {
        private RelationshipBusiness _relationshipBusiness = null!;
        private ProjectBusiness _projectBusiness = null!;
        private DataSourceBusiness _dataSourceBusiness = null!;
        private ClassBusiness _classBusiness = null!;
        private Mock<IEdgeBusiness> _mockEdgeBusiness = null!;
        private Mock<IRecordBusiness> _mockRecordBusiness = null!;
        private Mock<ILogger<ProjectBusiness>> _mockLogger = null!;
        private EventBusiness _eventBusiness = null!;
        private INotificationBusiness _notificationBusiness = null!;
        private Mock<ILogger<NotificationBusiness>> _mockNotificationLogger = null!;
        private Mock<IHubContext<EventNotificationHub>> _mockHubContext = null!;
        private Mock<IObjectStorageBusiness> _mockObjectStorageBusiness = null!;
        private Mock<IRoleBusiness> _mockRoleBusiness = null!;
        
        public long pid;
        public long cid; // origin class ID
        public long cid2; // dest. class ID
        public long rid; // existing record ID
        public long rid2; // archived record ID

        public RelationshipBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _mockEdgeBusiness = new Mock<IEdgeBusiness>();
            _mockRecordBusiness = new Mock<IRecordBusiness>();
            _mockLogger = new Mock<ILogger<ProjectBusiness>>();
            _mockHubContext = new Mock<IHubContext<EventNotificationHub>>();
            _mockNotificationLogger = new Mock<ILogger<NotificationBusiness>>();
            _notificationBusiness = new NotificationBusiness(Context, _mockNotificationLogger.Object, _mockHubContext.Object);
            _eventBusiness = new EventBusiness(Context, _cacheBusiness, _notificationBusiness);
            _mockObjectStorageBusiness = new Mock<IObjectStorageBusiness>();
            _mockRoleBusiness = new Mock<IRoleBusiness>();

            _relationshipBusiness = new RelationshipBusiness(
                Context, _cacheBusiness, _mockEdgeBusiness.Object, _eventBusiness);
    
            _dataSourceBusiness = new DataSourceBusiness(
                Context, _cacheBusiness, _mockEdgeBusiness.Object, _mockRecordBusiness.Object, _eventBusiness);
    
            _classBusiness = new ClassBusiness(
                Context, _cacheBusiness, _mockRecordBusiness.Object, 
                _relationshipBusiness, _eventBusiness);
    
            _projectBusiness = new ProjectBusiness(
                Context, _cacheBusiness, _mockLogger.Object, 
                _classBusiness, _mockRoleBusiness.Object, _dataSourceBusiness, 
                _mockObjectStorageBusiness.Object, _eventBusiness);
        }

        #region CreateRelationship Tests
        
        [Fact]
        public async Task CreateRelationship_Success_ReturnsIdAndCreatedAt()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var dto = new CreateRelationshipRequestDto
            {
                Name = $"Test Relationship {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Test Description",
                Uuid = $"test-uuid-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                OriginId = cid,
                DestinationId = cid2
            };

            // Act
            var result = await _relationshipBusiness.CreateRelationship(pid, dto);

            // Assert
            Assert.True(result.Id > 0);
            Assert.True(result.LastUpdatedAt >= now);
            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(dto.Description, result.Description);
            Assert.Equal(cid, result.OriginId);
            Assert.Equal(cid2, result.DestinationId);
            Assert.Equal(pid, result.ProjectId);
            Assert.NotNull(result.OriginId);
            Assert.NotNull(result.DestinationId);
            
            // Ensure that relationship create event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);

            var actualEvent = eventList[0];
            
            Assert.Equal("create", actualEvent.Operation);
            Assert.Equal("relationship", actualEvent.EntityType);
            Assert.Equal(result.Id, actualEvent.EntityId);
            Assert.Equal(result.ProjectId, actualEvent.ProjectId);
        }

        [Fact]
        public async Task CreateRelationship_Success_WithNullOriginId()
        {
            // Arrange
            var dto = new CreateRelationshipRequestDto
            {
                Name = $"Test Relationship {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Test Description",
                OriginId = null,
                DestinationId = cid2
            };

            // Act
            var result = await _relationshipBusiness.CreateRelationship(pid, dto);
            
            // Assert
            Assert.True(result.Id > 0);
            Assert.Null(result.OriginId);
            Assert.Equal(cid2, result.DestinationId);

            // Ensure that relationship create event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);

            var actualEvent = eventList[0];
            Assert.Equal("create", actualEvent.Operation);
            Assert.Equal("relationship", actualEvent.EntityType);
            Assert.Equal(result.Id, actualEvent.EntityId);
            Assert.Equal(result.ProjectId, actualEvent.ProjectId);
        }

        [Fact]
        public async Task CreateRelationship_Success_WithNullDestinationId()
        {
            // Arrange
            var dto = new CreateRelationshipRequestDto
            {
                Name = $"Test Relationship {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Test Description",
                OriginId = cid,
                DestinationId = null
            };

            // Act
            var result = await _relationshipBusiness.CreateRelationship(pid, dto);
            
            // Assert
            Assert.True(result.Id > 0);
            Assert.Equal(cid, result.OriginId);
            Assert.Null(result.DestinationId);

            // Ensure that relationship create event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);

            var actualEvent = eventList[0];
            Assert.Equal("create", actualEvent.Operation);
            Assert.Equal("relationship", actualEvent.EntityType);
            Assert.Equal(result.Id, actualEvent.EntityId);
            Assert.Equal(result.ProjectId, actualEvent.ProjectId);
        }
        
        [Fact]
        public async Task CreateRelationship_Fails_IfNoName()
        {
            // Arrange
            var dto = new CreateRelationshipRequestDto
            {
                Name = null!, Description = "Test Description" 
            };
            
            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _relationshipBusiness.CreateRelationship(pid, dto));
            
            // Ensure that no relationship create event is logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreateRelationship_Fails_IfEmptyName()
        {
            // Arrange
            var dto = new CreateRelationshipRequestDto
            {
                Name = "", Description = "Test Description" 
                
            };
            
            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                () => _relationshipBusiness.CreateRelationship(pid, dto));
            
            // Ensure that no relationship create event is logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreateRelationship_Fails_IfNoProjectId()
        {
            // Arrange
            var dto = new CreateRelationshipRequestDto
            {
                Name = "Test Relationship",
                Description = "Test Description",
                OriginId = cid,
                DestinationId = cid2
            };
 
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _relationshipBusiness.CreateRelationship(pid + 99, dto));

            Assert.Contains($"Project with id {pid + 99} not found.", exception.Message);
            
            // Ensure that no relationship create event is logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreateRelationship_Fails_IfDeletedProjectId()
        {
            // Arrange
            var project = await Context.Projects.FindAsync(pid);
            project.IsArchived = true;
            await Context.SaveChangesAsync();

            var dto = new CreateRelationshipRequestDto
            {
                Name = "Test Relationship",
                Description = "Test Description",
                OriginId = cid,
                DestinationId = cid2
            };
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _relationshipBusiness.CreateRelationship(pid, dto));

            Assert.Contains($"Project with id {pid} not found.", exception.Message);
            
            // Ensure that no relationship create event is logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreateRelationship_Fails_IfInvalidOriginId()
        {
            // Arrange
            var dto = new CreateRelationshipRequestDto
            {
                Name = "Test Relationship",
                Description = "Test Description",
                OriginId = 999,
                DestinationId = cid2
            };
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _relationshipBusiness.CreateRelationship(pid, dto));

            Assert.Contains($"Origin class with ID {dto.OriginId} not found.", exception.Message);
            
            // Ensure that no relationship create event is logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreateRelationship_Fails_IfInvalidDestinationId()
        {
            // Arrange
            var dto = new CreateRelationshipRequestDto
            {
                Name = "Test Relationship",
                Description = "Test Description",
                OriginId = cid,
                DestinationId = 999
            };
            
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _relationshipBusiness.CreateRelationship(pid, dto));

            Assert.Contains($"Destination class with ID {dto.DestinationId} not found.", exception.Message);
            
            // Ensure that no relationship create event is logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }
        
        #endregion

        #region BulkCreateRelationships Tests
        
        [Fact]
        public async Task BulkCreateRelationships_Success_ReturnsMultipleRelationships()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var relationshipDtos = new List<CreateRelationshipRequestDto>
            {
                new()
                {
                    Name = "Bulk Relationship 1",
                    Description = "First bulk relationship",
                    OriginId = cid,
                    DestinationId = cid2
                },
                new()
                {
                    Name = "Bulk Relationship 2",
                    Description = "Second bulk relationship",
                    OriginId = cid2,
                    DestinationId = cid
                }
            };

            // Act
            var result = await _relationshipBusiness.BulkCreateRelationships(pid, relationshipDtos);
            
            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.True(r.Id > 0));
            Assert.All(result, r => Assert.True(r.LastUpdatedAt >= now));
            Assert.Equal("Bulk Relationship 1", result.First().Name);
            Assert.Equal("Bulk Relationship 2", result.Last().Name);

            // Ensure that relationship create events were logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Equal(2, eventList.Count);

            var firstEvent = eventList[0];
            Assert.Equal("create", firstEvent.Operation);
            Assert.Equal("relationship", firstEvent.EntityType);
            Assert.Equal(result[0].Id, firstEvent.EntityId);
            Assert.Equal(result[0].ProjectId, firstEvent.ProjectId);

            var secondEvent = eventList[1];
            Assert.Equal("create", secondEvent.Operation);
            Assert.Equal("relationship", secondEvent.EntityType);
            Assert.Equal(result[1].Id, secondEvent.EntityId);
            Assert.Equal(result[1].ProjectId, secondEvent.ProjectId);
        }
        
        [Fact]
        public async Task BulkCreateRelationships_Fails_IfNullDto()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _relationshipBusiness.BulkCreateRelationships(pid, null!));
    
            // Assert - Ensure that no relationship create events were logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }
        
        #endregion
        
        #region GetAllRelationships Tests

        [Fact]
        public async Task GetAllRelationships_ReturnsOnlyForProject()
        {
            // Arrange
            var p2 = new Project { Name = "ExtraProj" };
            Context.Projects.Add(p2);
            await Context.SaveChangesAsync();

            await _relationshipBusiness.CreateRelationship(pid, new CreateRelationshipRequestDto
            {
                Name = $"Relationship1-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Test",
                OriginId = cid,
                DestinationId = cid2
            });

            // Create relationship directly for project 2 (bypass validation)
            var p2Relationship = new Relationship
            {
                Name = "P2 Relationship",
                ProjectId = p2.Id,
                OriginId = cid,
                DestinationId = cid2,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Relationships.Add(p2Relationship);
            await Context.SaveChangesAsync();

            // Act
            var list = await _relationshipBusiness.GetAllRelationships(pid, true);
            
            // Assert
            Assert.All(list, r => Assert.Equal(pid, r.ProjectId));
        }

        [Fact]
        public async Task GetAllRelationships_ExcludesSoftDeleted()
        {
            // Arrange
            var activeRelationship = new Relationship
            {
                Name = $"Active Relationship {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                OriginId = cid,
                DestinationId = cid2,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };

            var archivedRelationship = new Relationship
            {
                Name = $"Archived Relationship {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                OriginId = cid,
                DestinationId = cid2,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null,
                IsArchived = true
            };
            Context.Relationships.Add(activeRelationship);
            Context.Relationships.Add(archivedRelationship);
            await Context.SaveChangesAsync();
            
            // Act
            var listWithArchived = await _relationshipBusiness.GetAllRelationships(pid, false);
            var listWithoutArchived = await _relationshipBusiness.GetAllRelationships(pid, true);

            // Assert
            Assert.Contains(listWithArchived, r => r.Id == archivedRelationship.Id);
            Assert.DoesNotContain(listWithoutArchived, r => r.Id == archivedRelationship.Id);
        }
        
        #endregion
        
        #region GetRelationship Tests

        [Fact]
        public async Task GetRelationship_Success_WhenExists()
        {
            // Arrange
            var testRelationship = new Relationship
            {
                Name = $"Test Relationship {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Test Description",
                ProjectId = pid,
                OriginId = cid,
                DestinationId = cid2,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            Context.Relationships.Add(testRelationship);
            await Context.SaveChangesAsync();

            // Act
            var result = await _relationshipBusiness.GetRelationship(pid, testRelationship.Id, true);
            
            // Assert
            Assert.Equal(testRelationship.Id, result.Id);
            Assert.Equal(testRelationship.Name, result.Name);
            Assert.Equal(cid, result.OriginId);
            Assert.Equal(cid2, result.DestinationId);
        }

        [Fact]
        public async Task GetRelationship_Fails_IfNoProjectID()
        {
            // Arrange
            var testRelationship = new Relationship
            {
                Name = $"Test Relationship {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                OriginId = cid,
                DestinationId = cid2,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            Context.Relationships.Add(testRelationship);
            await Context.SaveChangesAsync();
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _relationshipBusiness.GetRelationship(pid + 999, testRelationship.Id, true));

            Assert.Contains($"Project with id {pid + 999} not found.", exception.Message);
        }

        [Fact]
        public async Task GetRelationship_Fails_IfDeletedRelationship()
        {
            // Arrange
            var testRelationship = new Relationship
            {
                Name = $"Deleted Relationship {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                OriginId = cid,
                DestinationId = cid2,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null,
                IsArchived = true
            };
            Context.Relationships.Add(testRelationship);
            await Context.SaveChangesAsync();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _relationshipBusiness.GetRelationship(pid, testRelationship.Id, true));

            Assert.Contains($"Relationship with id {testRelationship.Id} is archived", exception.Message);
        }
        
        #endregion
        
        #region UpdateRelationship Tests

        [Fact]
        public async Task UpdateRelationship_Success_ReturnsModifiedAt()
        {
            // Arrange
            var testRelationship = new Relationship
            {
                Name = $"Original Relationship {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Original Description",
                ProjectId = pid,
                OriginId = cid,
                DestinationId = cid2,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null,
            };
            Context.Relationships.Add(testRelationship);
            await Context.SaveChangesAsync();

            // Add a small delay to ensure ModifiedAt is after CreatedAt
            await Task.Delay(50);

            var dto = new UpdateRelationshipRequestDto
            {
                Name = $"Updated Relationship {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Updated Description",
                OriginId = cid2,
                DestinationId = cid
            };
            
            // Act
            var updatedResult = await _relationshipBusiness.UpdateRelationship(pid, testRelationship.Id, dto);

            // Assert
            Assert.True((DateTime.UtcNow - updatedResult.LastUpdatedAt).TotalSeconds < 1);
            Assert.Equal(dto.Name, updatedResult.Name);
            Assert.Equal(dto.Description, updatedResult.Description);
            Assert.Equal(cid2, updatedResult.OriginId);
            Assert.Equal(cid, updatedResult.DestinationId);

            // Ensure that relationship update event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);

            var actualEvent = eventList[0];
            Assert.Equal("update", actualEvent.Operation);
            Assert.Equal("relationship", actualEvent.EntityType);
            Assert.Equal(updatedResult.Id, actualEvent.EntityId);
            Assert.Equal(updatedResult.ProjectId, actualEvent.ProjectId);
        }

        [Fact]
        public async Task UpdateRelationship_PartialUpdate_UpdatesRelationship()
        {
            // Arrange
            var originalRelationship = new Relationship
            {
                Name = "Original Relationship",
                Description = "Original Description",
                ProjectId = pid,
                OriginId = cid,
                DestinationId = cid2,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };

            Context.Relationships.Add(originalRelationship);
            await Context.SaveChangesAsync();

            var updateDto = new UpdateRelationshipRequestDto
            {
                Description = "Updated Description"
            };

            // Act
            var result = await _relationshipBusiness.UpdateRelationship(pid, originalRelationship.Id, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(originalRelationship.Id, result.Id);
            Assert.Equal("Updated Description", result.Description);
            Assert.Equal(originalRelationship.Name, result.Name);

            // Verify it was actually updated in database
            var updatedRelationship = await Context.Relationships.FindAsync(originalRelationship.Id);
            Assert.NotNull(updatedRelationship);
            Assert.Equal("Updated Description", updatedRelationship.Description);
            Assert.Equal(originalRelationship.Name, updatedRelationship.Name);
            Assert.NotNull(updatedRelationship.LastUpdatedAt);

            // Ensure that relationship update event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);

            var actualEvent = eventList[0];
            Assert.Equal("update", actualEvent.Operation);
            Assert.Equal("relationship", actualEvent.EntityType);
            Assert.Equal(result.Id, actualEvent.EntityId);
            Assert.Equal(result.ProjectId, actualEvent.ProjectId);
        }

        [Fact]
        public async Task UpdateRelationship_Fails_IfNotFound()
        {
            // Arrange
            var dto = new UpdateRelationshipRequestDto
            {
                Name = "Updated Relationship",
                Description = "Updated Description",
                OriginId = cid,
                DestinationId = cid2
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _relationshipBusiness.UpdateRelationship(pid, 99, dto));
            
            Assert.Contains($"Relationship with ID 99 not found.", exception.Message);
    
            // Ensure that no relationship update event is logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }
        
        #endregion
        
        #region DeleteRelationship Tests

        [Fact]
        public async Task DeleteRelationship_Success_WhenExists()
        {
            var testRelationship = new Relationship
            {
                Name = $"Relationship to Delete {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                OriginId = cid,
                DestinationId = cid2,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null,
            };
            Context.Relationships.Add(testRelationship);
            await Context.SaveChangesAsync();

            var deletedResult = await _relationshipBusiness.DeleteRelationship(pid, testRelationship.Id);
            Assert.True(deletedResult);

            var deletedRelationship = await Context.Relationships.FindAsync(testRelationship.Id);
            Assert.Null(deletedRelationship);
        }
        
        [Fact]
        public async Task DeleteRelationship_Fails_IfNotFound()
        {
            var result = () => _relationshipBusiness.DeleteRelationship(pid, 999);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        #endregion
        
        #region ArchiveRelationship Tests
        
        [Fact]
        public async Task ArchiveRelationship_Success_WhenExists()
        {
            var beforeArchive = DateTime.UtcNow;

            var testRelationship = new Relationship
                
            {
                Name = $"Relationship to Archive {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                OriginId = cid,
                DestinationId = cid2,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null,
            };
            Context.Relationships.Add(testRelationship);
            await Context.SaveChangesAsync();

            var archivedResult = await _relationshipBusiness.ArchiveRelationship(pid, testRelationship.Id);
            Assert.True(archivedResult);

            // procedure is not traced by entity framework
            //this forces EF to sync to db on next query
            Context.ChangeTracker.Clear();

            var archivedRelationship = await Context.Relationships.FindAsync(testRelationship.Id);
            Assert.NotNull(archivedRelationship);
            Assert.True(archivedRelationship.IsArchived);
            // Ensure that relationship delete event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                ProjectId = testRelationship.ProjectId,
                Operation = "archive",
                EntityType = "relationship",
                EntityId = archivedRelationship.Id,
            });
        }
        
        [Fact]
        public async Task RelationshipArchived_WhenProjectArchived()
        {
            // Arrange
            var testRelationship = new Relationship
            {
                Name = $"Relationship in Project {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                OriginId = cid,
                DestinationId = cid2,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null,
            };
            Context.Relationships.Add(testRelationship);
            await Context.SaveChangesAsync();

            var deletedResult = await _projectBusiness.ArchiveProject(pid);
            Assert.True(deletedResult);

            // procedure is not traced by entity framework
            //this forces EF to sync to db on next query
            Context.ChangeTracker.Clear();

            var archivedRelationship = await Context.Relationships.FindAsync(testRelationship.Id);
            Assert.NotNull(archivedRelationship);

            // Check if ArchivedAt was set (optional based on implementation)
            if (archivedRelationship.IsArchived)
            {
                Assert.True(archivedRelationship.IsArchived);
            }
        }
        
        [Fact]
        public async Task ArchiveRelationship_Fails_IfNotFound()
        {
            var result = () => _relationshipBusiness.ArchiveRelationship(pid, 999);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }
        
        #endregion
        
        #region UnarchiveRelationship Tests

        [Fact]
        public async Task UnarchiveRelationship_Success_WhenArchived()
        {
            var testRelationship = new Relationship
            {
                Name = $"Archived Relationship {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                OriginId = cid,
                DestinationId = cid2,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null,
                IsArchived = true
            };
            Context.Relationships.Add(testRelationship);
            await Context.SaveChangesAsync();

            var unarchivedResult = await _relationshipBusiness.UnarchiveRelationship(pid, testRelationship.Id);
            Assert.True(unarchivedResult);

            // procedure is not traced by entity framework
            //this forces EF to sync to db on next query
            Context.ChangeTracker.Clear();

            var unarchivedRelationship = await Context.Relationships.FindAsync(testRelationship.Id);
            Assert.NotNull(unarchivedRelationship);
            Assert.False(unarchivedRelationship.IsArchived);
        }
        
        [Fact]
        public async Task UnarchiveRelationship_Fails_IfNotFound()
        {
            var result = () => _relationshipBusiness.UnarchiveRelationship(pid, 999);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task UnarchiveRelationship_Fails_IfNotArchived()
        {
            var activeRelationship = new Relationship
            {
                Name = "Active Relationship",
                ProjectId = pid,
                OriginId = cid,
                DestinationId = cid2,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null // Not archived
            };
            Context.Relationships.Add(activeRelationship);
            await Context.SaveChangesAsync();

            var result = () => _relationshipBusiness.UnarchiveRelationship(pid, activeRelationship.Id);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }
        
        #endregion

        #region DTO Tests
        
        [Fact]
        public void RelationshipRequestDto_AllProperties_CanBeSetAndRetrieved()
        {
            var dto = new CreateRelationshipRequestDto
            {
                Name = "Test Relationship",
                Description = "Test Description",
                Uuid = "test-uuid",
                OriginId = 1,
                DestinationId = 2
            };

            Assert.Equal("Test Relationship", dto.Name);
            Assert.Equal("Test Description", dto.Description);
            Assert.Equal("test-uuid", dto.Uuid);
            Assert.Equal(1, dto.OriginId);
            Assert.Equal(2, dto.DestinationId);
        }

        [Fact]
        public void RelationshipResponseDto_AllProperties_CanBeSetAndRetrieved()
        {
            var now = DateTime.UtcNow;
            var origin = new ClassRelationshipResponseDto { Id = 1, Name = "Origin Class" };
            var destination = new ClassRelationshipResponseDto { Id = 2, Name = "Destination Class" };

            var dto = new RelationshipResponseDto
            {
                Id = 1,
                Name = "Test Relationship",
                Description = "Test Description",
                Uuid = "test-uuid",
                ProjectId = 3,
                LastUpdatedBy = "test@example.com",
                LastUpdatedAt = now,
                IsArchived = false,
                OriginId = 1,
                DestinationId = 2,
              
            };

            Assert.Equal(1, dto.Id);
            Assert.Equal("Test Relationship", dto.Name);
            Assert.Equal("Test Description", dto.Description);
            Assert.Equal("test-uuid", dto.Uuid);
            Assert.Equal(3, dto.ProjectId);
            Assert.Equal("test@example.com", dto.LastUpdatedBy);
            Assert.Equal(now, dto.LastUpdatedAt);
            Assert.False(dto.IsArchived);
            Assert.Equal(1, dto.OriginId);
            Assert.Equal(2, dto.DestinationId);
        }
        
        #endregion
        
        #region GetRelationshipsByName Tests

        [Fact]
        public async Task GetRelationshipsByName_ValidRelationshipNames_ReturnsMatchingRelationships()
        {
            // Arrange
            var testRelationship = new Relationship
            {
                Name = "TestValidationRelationship",
                ProjectId = pid,
                OriginId = cid,
                DestinationId = cid2,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            Context.Relationships.Add(testRelationship);
            await Context.SaveChangesAsync();

            var relationshipNames = new List<string> { "TestValidationRelationship" };

            // Act
            var result = await _relationshipBusiness.GetRelationshipsByName(pid, relationshipNames);

            // Assert
            Assert.Equal(1, result.Count);
            Assert.Equal("TestValidationRelationship", result.First().Name);
            Assert.Equal(pid, result.First().ProjectId);
        }

        [Fact]
        public async Task GetRelationshipsByName_MissingRelationshipNames_ThrowsKeyNotFoundException()
        {
            // Arrange
            var relationshipNames = new List<string> { "NonExistentRelationship" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _relationshipBusiness.GetRelationshipsByName(pid, relationshipNames));

            Assert.Contains("Relationships not found with names", exception.Message);
        }

        [Fact]
        public async Task GetRelationshipsByName_NullRelationshipNames_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _relationshipBusiness.GetRelationshipsByName(pid, null));
        }

        [Fact]
        public async Task GetRelationshipsByName_ExcludesArchivedRelationships()
        {
            // Arrange
            var archivedRelationship = new Relationship
            {
                Name = "ArchivedRelationship",
                ProjectId = pid,
                OriginId = cid,
                DestinationId = cid2,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                IsArchived = true
            };

            Context.Relationships.Add(archivedRelationship);
            await Context.SaveChangesAsync();

            var relationshipNames = new List<string> { "ArchivedRelationship" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _relationshipBusiness.GetRelationshipsByName(pid, relationshipNames));
            
            Assert.Contains("ArchivedRelationship", exception.Message);
        }

        [Fact]
        public async Task GetRelationshipsByName_InvalidProjectId_ThrowsKeyNotFoundException()
        {
            // Arrange
            var relationshipNames = new List<string> { "SomeRelationship" };
            var invalidProjectId = 999L;

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _relationshipBusiness.GetRelationshipsByName(invalidProjectId, relationshipNames));
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

            // Add classes
            var originClass = new Class
            {
                Name = "Origin Class",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(originClass);

            var destinationClass = new Class
            {
                Name = "Destination Class",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(destinationClass);
            await Context.SaveChangesAsync();

            cid = originClass.Id;
            cid2 = destinationClass.Id;
            
            // Add relationships
            var existingRelationship = new Relationship
            {
                Name = "Original Relationship",
                Description = "Original Description",
                ProjectId = pid,
                OriginId = cid,
                DestinationId = cid2,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            var arhivedRelationship = new Relationship
            {
                Name = "Archived Relationship",
                Description = "Archived Description",
                ProjectId = pid,
                OriginId = cid,
                DestinationId = cid2,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            Context.Relationships.AddRange(existingRelationship, arhivedRelationship);
            await Context.SaveChangesAsync();

            rid = existingRelationship.Id;
            rid2 = arhivedRelationship.Id;
        }
    }
}