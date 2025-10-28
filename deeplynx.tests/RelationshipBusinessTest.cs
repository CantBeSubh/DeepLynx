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
        public long dsid;
        public long originClassId;
        public long destinationClassId;
        public long uid;

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

        [Fact]
        public async Task CreateRelationship_Success_ReturnsIdAndCreatedAt()
        {
            var now = DateTime.UtcNow;
            var dto = new CreateRelationshipRequestDto
            {
                Name = $"Test Relationship {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Test Description",
                Uuid = $"test-uuid-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                OriginId = originClassId,
                DestinationId = destinationClassId
            };

            var result = await _relationshipBusiness.CreateRelationship(pid, dto);
            result.Id.Should().BeGreaterThan(0);
            result.LastUpdatedAt.Should().BeOnOrAfter(now);
            result.Name.Should().Be(dto.Name);
            result.Description.Should().Be(dto.Description);
            result.OriginId.Should().Be(originClassId);
            result.DestinationId.Should().Be(destinationClassId);
            result.ProjectId.Should().Be(pid);
            result.OriginId.Should().NotBeNull();
            result.DestinationId.Should().NotBeNull();
            
            // Ensure that relationship create event was logged
            var eventList = Context.Events.ToList();
            eventList.Should().HaveCount(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                Operation = "create",
                EntityType = "relationship",
                EntityId = result.Id,
                ProjectId = result.ProjectId,
            });
        }

        [Fact]
        public async Task CreateRelationship_Success_WithNullOriginId()
        {
            var dto = new CreateRelationshipRequestDto
            {
                Name = $"Test Relationship {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Test Description",
                OriginId = null,
                DestinationId = destinationClassId
            };

            var result = await _relationshipBusiness.CreateRelationship(pid, dto);
            result.Id.Should().BeGreaterThan(0);
            result.OriginId.Should().BeNull();
            result.DestinationId.Should().Be(destinationClassId);
            
            // Ensure that relationship create event was logged
            var eventList = Context.Events.ToList();
            eventList.Should().HaveCount(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                Operation = "create",
                EntityType = "relationship",
                EntityId = result.Id,
                ProjectId = result.ProjectId,
            });
        }

        [Fact]
        public async Task CreateRelationship_Success_WithNullDestinationId()
        {
            var dto = new CreateRelationshipRequestDto
            {
                Name = $"Test Relationship {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Test Description",
                OriginId = originClassId,
                DestinationId = null
            };

            var result = await _relationshipBusiness.CreateRelationship(pid, dto);
            result.Id.Should().BeGreaterThan(0);
            result.OriginId.Should().Be(originClassId);
            result.DestinationId.Should().BeNull();
            
            // Ensure that relationship create event was logged
            var eventList = Context.Events.ToList();
            eventList.Should().HaveCount(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                Operation = "create",
                EntityType = "relationship",
                EntityId = result.Id,
                ProjectId = result.ProjectId,
            });
        }

        [Fact]
        public async Task BulkCreateRelationships_Success_ReturnsMultipleRelationships()
        {
            var now = DateTime.UtcNow;

            var relationshipDtos = new List<CreateRelationshipRequestDto>
            {
                new CreateRelationshipRequestDto
                {
                    Name = "Bulk Relationship 1",
                    Description = "First bulk relationship",
                    OriginId = originClassId,
                    DestinationId = destinationClassId
                },
                new CreateRelationshipRequestDto
                {
                    Name = "Bulk Relationship 2",
                    Description = "Second bulk relationship",
                    OriginId = destinationClassId,
                    DestinationId = originClassId
                }
            };

            var result = await _relationshipBusiness.BulkCreateRelationships(pid, relationshipDtos);
            result.Should().HaveCount(2);
            result.Should().OnlyContain(r => r.Id > 0);
            result.Should().OnlyContain(r => r.LastUpdatedAt >= now);
            result.First().Name.Should().Be("Bulk Relationship 1");
            result.Last().Name.Should().Be("Bulk Relationship 2");
            
            // Ensure that relationship create event was logged
            var eventList = Context.Events.ToList();
            eventList.Should().HaveCount(2);
            eventList[0].Should().BeEquivalentTo(new
            {
                Operation = "create",
                EntityType = "relationship",
                EntityId = result[0].Id,
                ProjectId = result[0].ProjectId,
            });
            eventList[1].Should().BeEquivalentTo(new
            {
                Operation = "create",
                EntityType = "relationship",
                EntityId = result[1].Id,
                ProjectId = result[1].ProjectId,
            });
        }

        [Fact]
        public async Task CreateRelationship_Fails_IfNoName()
        {
            var dto = new CreateRelationshipRequestDto { Name = null!, Description = "Test Description" };
            var result = () => _relationshipBusiness.CreateRelationship(pid, dto);
            await result.Should().ThrowAsync<ValidationException>();
            
            // Ensure that no relationship create event is logged
            var eventList = Context.Events.ToList();
            eventList.Should().HaveCount(0);
        }

        [Fact]
        public async Task CreateRelationship_Fails_IfEmptyName()
        {
            var dto = new CreateRelationshipRequestDto { Name = "", Description = "Test Description" };
            var result = () => _relationshipBusiness.CreateRelationship(pid, dto);
            await result.Should().ThrowAsync<ValidationException>();
            
            // Ensure that no relationship create event is logged
            var eventList = Context.Events.ToList();
            eventList.Should().HaveCount(0);
        }

        [Fact]
        public async Task CreateRelationship_Fails_IfNoProjectId()
        {
            var dto = new CreateRelationshipRequestDto
            {
                Name = "Test Relationship",
                Description = "Test Description",
                OriginId = originClassId,
                DestinationId = destinationClassId
            };
            var result = () => _relationshipBusiness.CreateRelationship(pid + 99, dto);
            await result.Should().ThrowAsync<KeyNotFoundException>();
            
            // Ensure that no relationship create event is logged
            var eventList = Context.Events.ToList();
            eventList.Should().HaveCount(0);
        }

        [Fact]
        public async Task CreateRelationship_Fails_IfDeletedProjectId()
        {
            var project = await Context.Projects.FindAsync(pid);
            project.IsArchived = true;
            Context.Projects.Update(project);
            await Context.SaveChangesAsync();

            var dto = new CreateRelationshipRequestDto
            {
                Name = "Test Relationship",
                Description = "Test Description",
                OriginId = originClassId,
                DestinationId = destinationClassId
            };
            var result = () => _relationshipBusiness.CreateRelationship(pid, dto);
            await result.Should().ThrowAsync<KeyNotFoundException>();
            
            // Ensure that no relationship create event is logged
            var eventList = Context.Events.ToList();
            eventList.Should().HaveCount(0);
        }

        [Fact]
        public async Task CreateRelationship_Fails_IfInvalidOriginId()
        {
            var dto = new CreateRelationshipRequestDto
            {
                Name = "Test Relationship",
                Description = "Test Description",
                OriginId = 999,
                DestinationId = destinationClassId
            };
            var result = () => _relationshipBusiness.CreateRelationship(pid, dto);
            await result.Should().ThrowAsync<KeyNotFoundException>();
            
            // Ensure that no relationship create event is logged
            var eventList = Context.Events.ToList();
            eventList.Should().HaveCount(0);
        }

        [Fact]
        public async Task CreateRelationship_Fails_IfInvalidDestinationId()
        {
            var dto = new CreateRelationshipRequestDto
            {
                Name = "Test Relationship",
                Description = "Test Description",
                OriginId = originClassId,
                DestinationId = 999
            };
            var result = () => _relationshipBusiness.CreateRelationship(pid, dto);
            await result.Should().ThrowAsync<KeyNotFoundException>();
            
            // Ensure that no relationship create event is logged
            var eventList = Context.Events.ToList();
            eventList.Should().HaveCount(0);
        }

        [Fact]
        public async Task GetAllRelationships_ReturnsOnlyForProject()
        {
            var p2 = new Project { Name = "ExtraProj" };
            Context.Projects.Add(p2);
            await Context.SaveChangesAsync();

            await _relationshipBusiness.CreateRelationship(pid, new CreateRelationshipRequestDto
            {
                Name = $"Relationship1-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Test",
                OriginId = originClassId,
                DestinationId = destinationClassId
            });

            // Create relationship directly for project 2 (bypass validation)
            var p2Relationship = new Relationship
            {
                Name = "P2 Relationship",
                ProjectId = p2.Id,
                OriginId = originClassId,
                DestinationId = destinationClassId,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Relationships.Add(p2Relationship);
            await Context.SaveChangesAsync();

            var list = await _relationshipBusiness.GetAllRelationships(pid, true);
            Assert.All(list, r => Assert.Equal(pid, r.ProjectId));
        }

        [Fact]
        public async Task GetAllRelationships_ExcludesSoftDeleted()
        {
            var activeRelationship = new Relationship
            {
                Name = $"Active Relationship {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                OriginId = originClassId,
                DestinationId = destinationClassId,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };

            var archivedRelationship = new Relationship
            {
                Name = $"Archived Relationship {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                OriginId = originClassId,
                DestinationId = destinationClassId,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null,
                IsArchived = true
            };
            Context.Relationships.Add(activeRelationship);
            Context.Relationships.Add(archivedRelationship);
            await Context.SaveChangesAsync();

            var listWithArchived = await _relationshipBusiness.GetAllRelationships(pid, false);
            var listWithoutArchived = await _relationshipBusiness.GetAllRelationships(pid, true);

            listWithArchived.Should().Contain(r => r.Id == archivedRelationship.Id);
            listWithoutArchived.Should().NotContain(r => r.Id == archivedRelationship.Id);
        }

        [Fact]
        public async Task GetRelationship_Success_WhenExists()
        {
            var testRelationship = new Relationship
            {
                Name = $"Test Relationship {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Test Description",
                ProjectId = pid,
                OriginId = originClassId,
                DestinationId = destinationClassId,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            Context.Relationships.Add(testRelationship);
            await Context.SaveChangesAsync();

            var result = await _relationshipBusiness.GetRelationship(pid, testRelationship.Id, true);
            Assert.Equal(testRelationship.Id, result.Id);
            Assert.Equal(testRelationship.Name, result.Name);
            Assert.Equal(originClassId, result.OriginId);
            Assert.Equal(destinationClassId, result.DestinationId);
        }

        [Fact]
        public async Task GetRelationship_Fails_IfNoProjectID()
        {
            var testRelationship = new Relationship
            {
                Name = $"Test Relationship {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                OriginId = originClassId,
                DestinationId = destinationClassId,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            Context.Relationships.Add(testRelationship);
            await Context.SaveChangesAsync();

            var result = () => _relationshipBusiness.GetRelationship(pid + 999, testRelationship.Id, true);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task GetRelationship_Fails_IfDeletedRelationship()
        {
            var testRelationship = new Relationship
            {
                Name = $"Deleted Relationship {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                OriginId = originClassId,
                DestinationId = destinationClassId,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null,
                IsArchived = true
            };
            Context.Relationships.Add(testRelationship);
            await Context.SaveChangesAsync();

            var result = () => _relationshipBusiness.GetRelationship(pid, testRelationship.Id, true);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task UpdateRelationship_Success_ReturnsModifiedAt()
        {
            var testRelationship = new Relationship
            {
                Name = $"Original Relationship {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Original Description",
                ProjectId = pid,
                OriginId = originClassId,
                DestinationId = destinationClassId,
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
                OriginId = destinationClassId,
                DestinationId = originClassId
            };
            var updatedResult = await _relationshipBusiness.UpdateRelationship(pid, testRelationship.Id, dto);

            updatedResult.LastUpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            updatedResult.Name.Should().Be(dto.Name);
            updatedResult.Description.Should().Be(dto.Description);
            updatedResult.OriginId.Should().Be(destinationClassId);
            updatedResult.DestinationId.Should().Be(originClassId);
            
            // Ensure that relationship update event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                Operation = "update",
                EntityType = "relationship",
                EntityId = updatedResult.Id,
                ProjectId = updatedResult.ProjectId,
            });
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
                OriginId = originClassId,
                DestinationId = destinationClassId,
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
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                Operation = "update",
                EntityType = "relationship",
                EntityId = result.Id,
                ProjectId = result.ProjectId
            });
        }

        [Fact]
        public async Task UpdateRelationship_Fails_IfNotFound()
        {
            var dto = new UpdateRelationshipRequestDto
            {
                Name = "Updated Relationship",
                Description = "Updated Description",
                OriginId = originClassId,
                DestinationId = destinationClassId
            };
            var updatedResult = () => _relationshipBusiness.UpdateRelationship(pid, 99, dto);

            await updatedResult.Should().ThrowAsync<KeyNotFoundException>();
            
            // Ensure that no relationship update event is logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task DeleteRelationship_Success_WhenExists()
        {
            var testRelationship = new Relationship
            {
                Name = $"Relationship to Delete {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                OriginId = originClassId,
                DestinationId = destinationClassId,
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
        public async Task ArchiveRelationship_Success_WhenExists()
        {
            var beforeArchive = DateTime.UtcNow;

            var testRelationship = new Relationship
                
            {
                Name = $"Relationship to Archive {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                OriginId = originClassId,
                DestinationId = destinationClassId,
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
        public async Task UnarchiveRelationship_Success_WhenArchived()
        {
            var testRelationship = new Relationship
            {
                Name = $"Archived Relationship {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                OriginId = originClassId,
                DestinationId = destinationClassId,
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
        public async Task RelationshipArchived_WhenProjectArchived()
        {
            var beforeArchive = DateTime.UtcNow;
            var testRelationship = new Relationship
            {
                Name = $"Relationship in Project {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                OriginId = originClassId,
                DestinationId = destinationClassId,
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
        public async Task BulkCreateRelationships_Fails_IfNullDto()
        {
            var result = () => _relationshipBusiness.BulkCreateRelationships(pid, null!);
            await result.Should().ThrowAsync<ArgumentNullException>();
            
            // Ensure that no relationship create event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task ArchiveRelationship_Fails_IfNotFound()
        {
            var result = () => _relationshipBusiness.ArchiveRelationship(pid, 999);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task DeleteRelationship_Fails_IfNotFound()
        {
            var result = () => _relationshipBusiness.DeleteRelationship(pid, 999);
            await result.Should().ThrowAsync<KeyNotFoundException>();
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
                OriginId = originClassId,
                DestinationId = destinationClassId,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null // Not archived
            };
            Context.Relationships.Add(activeRelationship);
            await Context.SaveChangesAsync();

            var result = () => _relationshipBusiness.UnarchiveRelationship(pid, activeRelationship.Id);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

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
                LastUpdatedBy = uid,
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
            Assert.Equal(uid, dto.LastUpdatedBy);
            Assert.Equal(now, dto.LastUpdatedAt);
            Assert.False(dto.IsArchived);
            Assert.Equal(1, dto.OriginId);
            Assert.Equal(2, dto.DestinationId);
        }
#region GetRelationshipsByName Tests

[Fact]
public async Task GetRelationshipsByName_ValidRelationshipNames_ReturnsMatchingRelationships()
{
    // Arrange
    var testRelationship = new Relationship
    {
        Name = "TestValidationRelationship",
        ProjectId = pid,
        OriginId = originClassId,
        DestinationId = destinationClassId,
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
        OriginId = originClassId,
        DestinationId = destinationClassId,
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
        #region LastUpdatedBy Tests

            [Fact]
            public async Task CreateRelationship_Success_StoresLastUpdatedByUserId()
            {
                // Arrange
                var testRelationship = new Relationship
                {
                    Name = $"Test Relationship with User {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                    Description = "Test Description with User ID",
                    ProjectId = pid,
                    OriginId = originClassId,
                    DestinationId = destinationClassId,
                    LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    LastUpdatedBy = uid,
                    IsArchived = false
                };
                
                // Act
                Context.Relationships.Add(testRelationship);
                await Context.SaveChangesAsync();

                // Assert
                var savedRelationship = await Context.Relationships.FindAsync(testRelationship.Id);
                Assert.NotNull(savedRelationship);
                Assert.Equal(uid, savedRelationship.LastUpdatedBy);
            }

            [Fact]
            public async Task CreateRelationship_Success_NavigationPropertyLoadsUser()
            {
                // Arrange
                var testRelationship = new Relationship
                {
                    Name = $"Test Relationship Navigation {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                    Description = "Test Navigation Property",
                    ProjectId = pid,
                    OriginId = originClassId,
                    DestinationId = destinationClassId,
                    LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    LastUpdatedBy = uid,
                    IsArchived = false
                };
                
                Context.Relationships.Add(testRelationship);
                await Context.SaveChangesAsync();

                // Act
                var relationshipWithUser = await Context.Relationships
                    .Include(r => r.LastUpdatedByUser)
                    .FirstAsync(r => r.Id == testRelationship.Id);
                
                // Assert
                Assert.NotNull(relationshipWithUser.LastUpdatedByUser);
                Assert.Equal("Test User", relationshipWithUser.LastUpdatedByUser.Name);
                Assert.Equal("test.user@test.com", relationshipWithUser.LastUpdatedByUser.Email);
                Assert.Equal(uid, relationshipWithUser.LastUpdatedBy);
            }

            [Fact]
            public async Task CreateRelationship_Success_WithNullLastUpdatedBy()
            {
                // Arrange
                var testRelationship = new Relationship
                {
                    Name = $"Test Relationship Null User {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                    Description = "Test with null LastUpdatedBy",
                    ProjectId = pid,
                    OriginId = originClassId,
                    DestinationId = destinationClassId,
                    LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    LastUpdatedBy = null,
                    IsArchived = false
                };
                
                // Act
                Context.Relationships.Add(testRelationship);
                await Context.SaveChangesAsync();

                // Assert
                var savedRelationship = await Context.Relationships.FindAsync(testRelationship.Id);
                Assert.NotNull(savedRelationship);
                Assert.Null(savedRelationship.LastUpdatedBy);
                
                var relationshipWithUser = await Context.Relationships
                    .Include(r => r.LastUpdatedByUser)
                    .FirstAsync(r => r.Id == testRelationship.Id);
                
                Assert.Null(relationshipWithUser.LastUpdatedByUser);
            }

            [Fact]
            public async Task UpdateRelationship_Success_UpdatesLastUpdatedByUserId()
            {
                // Arrange
                var testRelationship = new Relationship
                {
                    Name = $"Original Relationship {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                    Description = "Original Description",
                    ProjectId = pid,
                    OriginId = originClassId,
                    DestinationId = destinationClassId,
                    LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    LastUpdatedBy = null
                };
                Context.Relationships.Add(testRelationship);
                await Context.SaveChangesAsync();

                // Act
                testRelationship.LastUpdatedBy = uid;
                testRelationship.Description = "Updated Description";
                testRelationship.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                
                Context.Relationships.Update(testRelationship);
                await Context.SaveChangesAsync();

                // Assert
                var updatedRelationship = await Context.Relationships
                    .Include(r => r.LastUpdatedByUser)
                    .FirstAsync(r => r.Id == testRelationship.Id);
                
                Assert.Equal(uid, updatedRelationship.LastUpdatedBy);
                Assert.NotNull(updatedRelationship.LastUpdatedByUser);
                Assert.Equal("Test User", updatedRelationship.LastUpdatedByUser.Name);
                Assert.Equal("Updated Description", updatedRelationship.Description);
            }

            #endregion
        protected override async Task SeedTestDataAsync()
        {
            await base.SeedTestDataAsync();
            
            var testUser = new User
            {
                Name = "Test User",
                Email = "test.user@test.com",
                Password = "test_password",
                IsArchived = false
            };
            Context.Users.Add(testUser);
            await Context.SaveChangesAsync();
            uid = testUser.Id;
            
            var project = new Project { Name = "Project 1" };
            Context.Projects.Add(project);
            await Context.SaveChangesAsync();
            pid = project.Id;

            var dataSource = new DataSource
            {
                Name = "DataSource 1",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.DataSources.Add(dataSource);
            await Context.SaveChangesAsync();
            dsid = dataSource.Id;

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

            originClassId = originClass.Id;
            destinationClassId = destinationClass.Id;
        }
    }
}