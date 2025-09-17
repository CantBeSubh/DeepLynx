using System.Text.Json.Nodes;
using System.ComponentModel.DataAnnotations;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.Extensions.Logging;
using Record = deeplynx.datalayer.Models.Record;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class ClassBusinessTests : IntegrationTestBase
    {
        private ClassBusiness _classBusiness = null!;
        private ProjectBusiness _projectBusiness = null!;
        private EventBusiness _eventBusiness = null!;
        private Mock<IDataSourceBusiness> _dataSourceBusiness = null!;
        private Mock<IRecordBusiness> _recordBusiness = null!;
        private Mock<IRelationshipBusiness> _relationshipBusiness = null!;
        private Mock<ILogger<ProjectBusiness>> _mockLogger = null!;
        private Mock<IObjectStorageBusiness> _objectStorageBusiness = null!;
        public long pid;
        public long did;
        public long os1;

        public ClassBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _recordBusiness = new Mock<IRecordBusiness>();
            _relationshipBusiness = new Mock<IRelationshipBusiness>();
            _dataSourceBusiness = new Mock<IDataSourceBusiness>();
            _mockLogger = new Mock<ILogger<ProjectBusiness>>();
            _eventBusiness = new EventBusiness(Context, _cacheBusiness);
            _objectStorageBusiness = new Mock<IObjectStorageBusiness>();

            _classBusiness = new ClassBusiness(
                Context, _cacheBusiness, _recordBusiness.Object, 
                _relationshipBusiness.Object, _eventBusiness);
            
            _projectBusiness = new ProjectBusiness(
                Context, _cacheBusiness, _mockLogger.Object, _classBusiness, 
                _dataSourceBusiness.Object, _objectStorageBusiness.Object, _eventBusiness);
        }

        [Fact]
        public async Task CreateClass_Success_ReturnsIdAndCreatedAt()
        {
            var now = DateTime.UtcNow;
            var dto = new CreateClassRequestDto
            {
                Name = $"Test Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Test Description",
                Uuid = $"test-uuid-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
            };

            var result = await _classBusiness.CreateClass(pid, dto);
            result.Id.Should().BeGreaterThan(0);
            result.LastUpdatedAt.Should().BeOnOrAfter(now);
            result.Name.Should().Be(dto.Name);
            result.Description.Should().Be(dto.Description);
            result.ProjectId.Should().Be(pid);
            
            // Ensure the create event is logged
            var eventList = await Context.Events.ToListAsync();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                ProjectId = pid,
                Operation = "create",
                EntityType = "class",
                EntityId = result.Id,
            });
        }

        [Fact]
        public async Task CreateClasses_Success_OnBulkCreate()
        {
            var now = DateTime.UtcNow;
            var bulkDto = new List<CreateClassRequestDto>
            {
                new CreateClassRequestDto
                {
                    Name = $"Test Class 1",
                    Description = "Test Description",
                    Uuid = $"test-uuid-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
                },
                new CreateClassRequestDto
                {
                    Name = $"Test Class 2",
                    Description = "Test Description",
                    Uuid = $"test-uuid-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
                }
            };
        
            var result = await _classBusiness.BulkCreateClasses(pid, bulkDto);
            result.Should().HaveCount(2);
            result.First().Name.Should().Be("Test Class 1");
            result.Last().Name.Should().Be("Test Class 2");
            
            // Ensure the create event is logged for each class create
            var eventList = await Context.Events.ToListAsync();
            eventList.Count.Should().Be(2);
            eventList[0].Should().BeEquivalentTo(new
            {
                ProjectId = pid,
                Operation = "create",
                EntityType = "class",
                EntityId = result[0].Id,
            });
            eventList[1].Should().BeEquivalentTo(new
            {
                ProjectId = pid,
                Operation = "create",
                EntityType = "class",
                EntityId = result[1].Id,
            });
        }

        [Fact]
        public async Task CreateClass_Fails_IfNoName()
        {
            var dto = new CreateClassRequestDto { Name = null, Description = "Test Description" };
            var result = () => _classBusiness.CreateClass(pid, dto);
            await result.Should().ThrowAsync<ValidationException>();
            
            // Ensure that no events were created on failed class creation
            var eventList = await Context.Events.ToListAsync();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task CreateClass_Fails_IfEmptyName()
        {
            var dto = new CreateClassRequestDto { Name = "", Description = "Test Description" };
            var result = () => _classBusiness.CreateClass(pid, dto);
            await result.Should().ThrowAsync<ValidationException>();
            
            // Ensure that no events were created on failed class creation
            var eventList = await Context.Events.ToListAsync();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task CreateClass_Fails_IfNoProjectId()
        {
            var dto = new CreateClassRequestDto { Name = "Test Class", Description = "Test Description" };
            var result = () => _classBusiness.CreateClass(pid + 99, dto);
            await result.Should().ThrowAsync<KeyNotFoundException>();
            
            // Ensure that no events were created on failed class creation
            var eventList = await Context.Events.ToListAsync();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task CreateClass_Fails_IfDeletedProjectId()
        {
            var project = await Context.Projects.FindAsync(pid);
            project.IsArchived = true;
            Context.Projects.Update(project);
            await Context.SaveChangesAsync();
            var dto = new CreateClassRequestDto { Name = "Test Class", Description = "Test Description" };
            var result = () => _classBusiness.CreateClass(pid, dto);
            await result.Should().ThrowAsync<KeyNotFoundException>();
            
            // Ensure that no events were created on failed class creation
            var eventList = await Context.Events.ToListAsync();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task CreateClass_Fails_IfDuplicateName()
        {
            var duplicateName = "Duplicate Class";
            var dto = new CreateClassRequestDto { Name = duplicateName, Description = "Test Description" };

            // Create first class
            var firstClass = await _classBusiness.CreateClass(pid, dto);

            // Try to create duplicate
            var result = () => _classBusiness.CreateClass(pid, dto);
            await result.Should().ThrowAsync<Exception>();
            
            // Ensure that only one event was logged (not the duplicate)
            var eventList = await Context.Events.ToListAsync();
        
            eventList.Count.Should().Be(1);
            eventList[0].Operation.Should().Be("create");
        }

        [Fact]
        public async Task GetAllClasses_ReturnsOnlyForProjects()
        {
            var p2 = new Project { Name = "ExtraProj" };
            Context.Projects.Add(p2);
            await Context.SaveChangesAsync();

            await _classBusiness.CreateClass(pid, new CreateClassRequestDto { Name = $"Class1-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", Description = "Test" });
            await _classBusiness.CreateClass(p2.Id, new CreateClassRequestDto { Name = $"Class2-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", Description = "Test" });
            
            var list = await _classBusiness.GetAllClasses(pid,true);
            Assert.All(list, c => Assert.Equal(pid, c.ProjectId));
        }

        [Fact]
        public async Task GetAllClasses_ExcludesSoftDeleted()
        {
            var activeClass = new Class
            {
                Name = $"Active Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                IsArchived = false

            };

            var archivedClass = new Class
            {
                Name = $"Archived Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                IsArchived = true

            };
            Context.Classes.Add(activeClass);
            Context.Classes.Add(archivedClass);
            await Context.SaveChangesAsync();
            var list = await _classBusiness.GetAllClasses(pid,true);
            Assert.DoesNotContain(list, c => c.Id == archivedClass.Id);
        }

        [Fact]
        public async Task GetClass_Success_WhenExists()
        {
            var testClass = new Class
            {
                Name = $"Test Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Test Description",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)

            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            var result = await _classBusiness.GetClass(pid, testClass.Id,true);
            Assert.Equal(testClass.Id, result.Id);
        }

        [Fact]
        public async Task GetClass_Fails_IfNoProjectID()
        {
            var testClass = new Class
            {
                Name = $"Test Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            var result = () => _classBusiness.GetClass(pid + 999, testClass.Id,true);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task GetClass_Fails_IfDeletedClass()
        {
            var testClass = new Class
            {
                Name = $"Deleted Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                IsArchived = true

            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            var result = () => _classBusiness.GetClass(pid, testClass.Id,true);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task UpdateClass_Success_ReturnsModifiedAt()
        {
            var testClass = new Class
            {
                Name = $"Original Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Original Description",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            // Add a small delay to ensure ModifiedAt is after CreatedAt
            await Task.Delay(50);

            var dto = new UpdateClassRequestDto { Name = $"Updated Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", Description = "Updated Description" };
            var updatedResult = await _classBusiness.UpdateClass(pid, testClass.Id, dto);
            
            Assert.NotNull(updatedResult.LastUpdatedAt);
            Assert.Equal("Updated Description", updatedResult.Description);
            
            // ensure that an event was logged for the update
            var eventList = Context.Events.ToList();
            eventList[0].Should().BeEquivalentTo(new
            {
                ProjectId = pid,
                Operation = "update",
                EntityType = "class",
                EntityId = updatedResult.Id,
            });
        }

        [Fact]
        public async Task UpdateClass_PartialUpdate_UpdatesClass()
        {
            // Arrange
            var testClass = new Class
            {
                Name = $"Original Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Original Description",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            // Add a small delay to ensure ModifiedAt can be tested
            await Task.Delay(50);

            var dto = new UpdateClassRequestDto
            {
                Description = "Updated Description"
            };

            // Act
            var updatedResult = await _classBusiness.UpdateClass(pid, testClass.Id, dto);

            // Assert
            Assert.NotNull(updatedResult);
            Assert.Equal("Updated Description", updatedResult.Description);
            Assert.Equal(testClass.Name, updatedResult.Name);
            Assert.NotNull(updatedResult.LastUpdatedAt);

            // Verify class was actually updated in database
            var updatedClass = await Context.Classes.FindAsync(testClass.Id);
            Assert.NotNull(updatedClass);
            Assert.Equal("Updated Description", updatedClass.Description);
            Assert.Equal(testClass.Name, updatedClass.Name);
            Assert.NotNull(updatedClass.LastUpdatedAt);

            // Verify that get function gets updated version
            var getResult = await _classBusiness.GetClass(pid, testClass.Id, true);
            Assert.NotNull(getResult);
            Assert.Equal("Updated Description", getResult.Description);
            Assert.Equal(testClass.Name, getResult.Name);
            Assert.NotNull(getResult.LastUpdatedAt);

            
            // Ensure that Update Event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                ProjectId = pid,
                Operation = "update",
                EntityType = "class",
                EntityId = updatedResult.Id,
            });
        }

        [Fact]
        public async Task UpdateClass_Fails_IfNotFound()
        {
            var dto = new UpdateClassRequestDto { Name = "Updated Class", Description = "Updated Description" };
            var updatedResult = () => _classBusiness.UpdateClass(pid, 99, dto);

            await updatedResult.Should().ThrowAsync<KeyNotFoundException>();
            
            // Ensure No Event was logged if update fails
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task ArchiveClass_Success_WhenExists()
        {
            var beforeArchive = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            var testClass = new Class
            {
                Name = $"Class to Archive {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            var archivedResult = await _classBusiness.ArchiveClass(pid, testClass.Id);
            Assert.True(archivedResult);

            // procedure is not traced by entity framework
            //this forces EF to sync to db on next query
            Context.ChangeTracker.Clear();

            var archivedClass = await Context.Classes.FindAsync(testClass.Id);
            Assert.NotNull(archivedClass);
            Assert.True(archivedClass.IsArchived);
            Assert.True(archivedClass.LastUpdatedAt >= beforeArchive);
            // Ensure that class soft delete event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                ProjectId = pid,
                Operation = "archive",
                EntityType = "class",
                EntityId = archivedClass.Id,
            });

        }

        [Fact]
        public async Task ArchiveClass_Fails_IfNotFound()
        {
            var result = () => _classBusiness.ArchiveClass(pid, 99);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task DeleteClass_Success_WhenExists()
        {
            var testClass = new Class
            {
                Name = $"Class to Delete {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            var deletedResult = await _classBusiness.DeleteClass(pid, testClass.Id);
            Assert.True(deletedResult);

            var deletedClass = await Context.Classes.FindAsync(testClass.Id);
            Assert.Null(deletedClass);
            
            // Ensure that class soft delete event was NOT logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task ClassArchived_WhenProjectArchived()
        {
            var beforeArchive = DateTime.UtcNow;
            var testClass = new Class
            {
                Name = $"Class in Project {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            var deletedResult = await _projectBusiness.ArchiveProject(pid);
            Assert.True(deletedResult);
            
            //Makes sure the db is refreshed 
            Context.ChangeTracker.Clear();

            var archivedClass = await Context.Classes.FindAsync(testClass.Id);
            Assert.NotNull(archivedClass);
            Assert.True(archivedClass.IsArchived);
            Assert.True(archivedClass.LastUpdatedAt >= beforeArchive);

        }
        
        [Fact]
        public async Task ForceDeleteClass_RemovesFromDatabase()
        {
            var testClass = new Class
            {
                Name = $"Class to Force Delete {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();
            
            var existingClass = await Context.Classes.FindAsync(testClass.Id);
            Assert.NotNull(existingClass);
            var deletedResult = await _classBusiness.DeleteClass(pid, testClass.Id);
            Assert.True(deletedResult);

            // Check if class is completely removed from database
            var deletedClass = await Context.Classes.FindAsync(testClass.Id);
            Assert.Null(deletedClass);
        }

        [Fact]
        public async Task DeleteClass_DeletesRelationshipsWithANullClass()
        {
            var testClass = new Class
            {
                Name = $"Class with Relationships {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();
            
            var relationship1 = new Relationship
            {
                Name = "Test Relationship 1",
                OriginId = testClass.Id,
                DestinationId = null,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            
            // Create relationship where testClass is Destination and orig is null
            var relationship2 = new Relationship
            {
                Name = "Test Relationship 2",
                OriginId = null,
                DestinationId = testClass.Id,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            
            // Create relationship where orig and dest are null
            var relationship3 = new Relationship
            {
                Name = "Test Relationship 3",
                OriginId = null,
                DestinationId = null,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            
            Context.Relationships.Add(relationship1);
            Context.Relationships.Add(relationship2);
            Context.Relationships.Add(relationship3);
            await Context.SaveChangesAsync();
            
            var deletedResult = await _classBusiness.DeleteClass(pid, testClass.Id);
            Assert.True(deletedResult);
            
            var  deletedClass = await Context.Classes.FindAsync(testClass.Id);
            var  deletedRelationship1 = await Context.Relationships.FindAsync(relationship1.Id);
            var  deletedRelationship2 = await Context.Relationships.FindAsync(relationship2.Id);
            var  intactRelationship3 = await Context.Relationships.FindAsync(relationship3.Id);
            
            Assert.Null(deletedClass);
            Assert.Null(deletedRelationship1);
            Assert.Null(deletedRelationship2);
            Assert.NotNull(intactRelationship3);
        }
    
        [Fact]
        public async Task DeleteClass_DeletesDownstreamRelationships()
        {
            var testClass = new Class
            {
                Name = $"Class with Relationships {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            // Create another class to be the other end of relationships
            var otherClass = new Class
            {
                Name = $"Other Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(otherClass);
            await Context.SaveChangesAsync();

            // Create relationship where testClass is Origin
            var relationship1 = new Relationship
            {
                Name = "Test Relationship 1",
                OriginId = testClass.Id,
                DestinationId = otherClass.Id,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };

            // Create relationship where testClass is Destination
            var relationship2 = new Relationship
            {
                Name = "Test Relationship 2",
                OriginId = otherClass.Id,
                DestinationId = testClass.Id,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            
            // Create relationship where testClass is Origin and dest is null
            var relationship3 = new Relationship
            {
                Name = "Test Relationship 3",
                OriginId = testClass.Id,
                DestinationId = null,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };

            // Create relationship where testClass is Destination and orig is null
            var relationship4 = new Relationship
            {
                Name = "Test Relationship 4",
                OriginId = null,
                DestinationId = testClass.Id,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            
            // Create relationship where orig and dest are null
            var relationship5 = new Relationship
            {
                Name = "Test Relationship 5",
                OriginId = null,
                DestinationId = null,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };

            Context.Relationships.Add(relationship1);
            Context.Relationships.Add(relationship2);
            Context.Relationships.Add(relationship3);
            Context.Relationships.Add(relationship4);
            Context.Relationships.Add(relationship5);
            await Context.SaveChangesAsync();

            var deletedResult = await _classBusiness.DeleteClass(pid, testClass.Id);
            Assert.True(deletedResult);

            var deletedRelationship1 = await Context.Relationships.FindAsync(relationship1.Id);
            var deletedRelationship2 = await Context.Relationships.FindAsync(relationship2.Id);
            var deletedRelationship3 = await Context.Relationships.FindAsync(relationship3.Id);
            var deletedRelationship4 = await Context.Relationships.FindAsync(relationship4.Id);
            var intactRelationship5 = await Context.Relationships.FindAsync(relationship5.Id);
            Assert.Null(deletedRelationship1);
            Assert.Null(deletedRelationship2);
            Assert.Null(deletedRelationship3);
            Assert.Null(deletedRelationship4);
            Assert.NotNull(intactRelationship5);
        }
        
        [Fact]
        public async Task DeleteClass_DeletesDownstreamRecords()
        {
            var testClass = new Class
            {
                Name = $"Class with Records {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();
            var record1 = new Record
            {
                Name = "Test Record 1",
                ClassId = testClass.Id,
                DataSourceId = did,
                ProjectId = pid,
                OriginalId = "og1",
                Description = "Test Description 1",
                Properties = "{\"test\": \"value1\"}",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };

            var record2 = new Record
            {
                Name = "Test Record 2",
                ClassId = testClass.Id,
                DataSourceId = did,
                ProjectId = pid,
                OriginalId = "og2",
                Description = "Test Description 2",
                Properties = "{\"test\": \"value2\"}",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };

            Context.Records.AddRange(record1, record2);
            await Context.SaveChangesAsync();
            var existingRecords = Context.Records
                .Where(r => r.ClassId == testClass.Id)
                .ToList();
            Assert.Equal(2, existingRecords.Count);
    
            var deletedResult = await _classBusiness.DeleteClass(pid, testClass.Id);
            Assert.True(deletedResult);

            // Verify downstream records are also deleted (cascade delete)
            var remainingRecords = Context.Records
                .Where(r => r.ClassId == testClass.Id)
                .ToList();
            Assert.Empty(remainingRecords);
        }
        
        #region UnarchiveClass Tests
        
        [Fact]
        public async Task UnarchiveClass_SuccessfullyUnarchivesClassAndReturnsTrue()
        {
            var testClass = new Class
            {
                Name = $"Archived Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                IsArchived = true

            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            var result = await _classBusiness.UnarchiveClass(pid, testClass.Id);
            Assert.True(result);
            
            //this forces EF to sync to db on next query
            Context.ChangeTracker.Clear();
            
            var updated = await Context.Classes.FindAsync(testClass.Id);
            Assert.False(updated?.IsArchived);
        }

        [Fact]
        public async Task UnarchiveClass_Throws_IfClassNotFound()
        {
            var act = () => _classBusiness.UnarchiveClass(pid, 99999);
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task UnarchiveClass_Throws_IfClassProjectMismatch()
        {
            var otherProject = new Project { Name = "Other Project" };
            Context.Projects.Add(otherProject);
            await Context.SaveChangesAsync();

            var testClass = new Class
            {
                Name = "Class from other project",
                ProjectId = otherProject.Id,
                IsArchived = false
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            var act = () => _classBusiness.UnarchiveClass(pid, testClass.Id);
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task UnarchiveClass_Throws_IfClassNotArchived()
        {
            var testClass = new Class
            {
                Name = "Active Class",
                ProjectId = pid,
                IsArchived = false
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            var act = () => _classBusiness.UnarchiveClass(pid, testClass.Id);
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }
        
        # endregion
        #region GetClassesByName Tests

[Fact]
public async Task GetClassesByName_ValidClassNames_ReturnsMatchingClasses()
{
    // Arrange  
    var testClass = new Class
    {
        Name = "TestValidationClass",
        ProjectId = pid,
        LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
    };

    Context.Classes.Add(testClass);
    await Context.SaveChangesAsync();

    var classNames = new List<string> { "TestValidationClass" };

    // Act
    var result = await _classBusiness.GetClassesByName(pid, classNames);

    // Assert
    Assert.Equal(1, result.Count);
    Assert.Equal("TestValidationClass", result.First().Name);
    Assert.Equal(pid, result.First().ProjectId);
}

[Fact]
public async Task GetClassesByName_MissingClassNames_ThrowsKeyNotFoundException()
{
    // Arrange
    var classNames = new List<string> { "NonExistentClass" };

    // Act & Assert
    var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
        () => _classBusiness.GetClassesByName(pid, classNames));

    Assert.Contains("Classes not found with names", exception.Message);
}

[Fact]
public async Task GetClassesByName_NullClassNames_ThrowsArgumentException()
{
    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(
        () => _classBusiness.GetClassesByName(pid, null));
}

[Fact]
public async Task GetClassesByName_ExcludesArchivedClasses()
{
    // Arrange
    var archivedClass = new Class
    {
        Name = "ArchivedClass",
        ProjectId = pid,
        LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
        IsArchived = true
    };

    Context.Classes.Add(archivedClass);
    await Context.SaveChangesAsync();

    var classNames = new List<string> { "ArchivedClass" };

    // Act & Assert
    var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
        () => _classBusiness.GetClassesByName(pid, classNames));
    
    Assert.Contains("ArchivedClass", exception.Message);
}

[Fact]
public async Task GetClassesByName_InvalidProjectId_ThrowsKeyNotFoundException()
{
    // Arrange
    var classNames = new List<string> { "SomeClass" };
    var invalidProjectId = 999L;

    // Act & Assert
    await Assert.ThrowsAsync<KeyNotFoundException>(
        () => _classBusiness.GetClassesByName(invalidProjectId, classNames));
}

#endregion
        protected override async Task SeedTestDataAsync()
        {
            await base.SeedTestDataAsync();
            var project = new Project { Name = "Project 1" };
            Context.Projects.Add(project);
            await Context.SaveChangesAsync();
            pid = project.Id;
            var dataSource = new DataSource()
            {
                Name = "Test Datasource",
                ProjectId = project.Id,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            Context.DataSources.Add(dataSource);
            await Context.SaveChangesAsync();
            did = dataSource.Id;
        }
    }
}