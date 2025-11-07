using System.ComponentModel.DataAnnotations;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.helpers.Hubs;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.Extensions.Logging;
using Record = deeplynx.datalayer.Models.Record;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class ClassBusinessTests : IntegrationTestBase
    {
        private Config _config;
        private ClassBusiness _classBusiness = null!;
        private ProjectBusiness _projectBusiness = null!;
        private EventBusiness _eventBusiness = null!;
        private INotificationBusiness _notificationBusiness = null!;
        private Mock<ILogger<NotificationBusiness>> _mockNotificationLogger = null!;
        private Mock<IHubContext<EventNotificationHub>> _mockHubContext = null!;
        private Mock<IDataSourceBusiness> _dataSourceBusiness = null!;
        private Mock<IRecordBusiness> _recordBusiness = null!;
        private Mock<IRelationshipBusiness> _relationshipBusiness = null!;
        private Mock<ILogger<ProjectBusiness>> _mockLogger = null!;
        private Mock<IObjectStorageBusiness> _objectStorageBusiness = null!;
        private Mock<IRoleBusiness> _roleBusiness = null!;
        public long pid;
        public long did;
        public long os1;
        public long uid;
        public long oid;

        public ClassBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _config = new Config();
            _recordBusiness = new Mock<IRecordBusiness>();
            _relationshipBusiness = new Mock<IRelationshipBusiness>();
            _dataSourceBusiness = new Mock<IDataSourceBusiness>();
            _mockLogger = new Mock<ILogger<ProjectBusiness>>();
            _mockHubContext = new Mock<IHubContext<EventNotificationHub>>();
            _mockNotificationLogger = new Mock<ILogger<NotificationBusiness>>();
            _notificationBusiness = new NotificationBusiness(_config, Context, _mockNotificationLogger.Object, _mockHubContext.Object);
            _eventBusiness = new EventBusiness(_config, Context, _cacheBusiness, _notificationBusiness);
            _objectStorageBusiness = new Mock<IObjectStorageBusiness>();
            _roleBusiness = new Mock<IRoleBusiness>();

            _classBusiness = new ClassBusiness(
                Context, _cacheBusiness, _recordBusiness.Object,
                _relationshipBusiness.Object, _eventBusiness);

            _projectBusiness = new ProjectBusiness(
                _config, Context, _cacheBusiness, _mockLogger.Object,
                _classBusiness, _roleBusiness.Object, _dataSourceBusiness.Object,
                _objectStorageBusiness.Object, _eventBusiness);
        }

        #region CreateClass Tests

        [Fact]
        public async Task CreateClass_Success_ReturnsIdAndCreatedAt()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var dto = new CreateClassRequestDto
            {
                Name = $"Test Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Test Description",
                Uuid = $"test-uuid-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
            };

            // Act
            var result = await _classBusiness.CreateClass(pid, dto);

            // Assert
            Assert.True(result.Id > 0);
            Assert.True(result.LastUpdatedAt >= now);
            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(dto.Description, result.Description);
            Assert.Equal(pid, result.ProjectId);

            // Ensure the create event is logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);

            var actualEvent = eventList[0];

            Assert.Equal(pid, actualEvent.ProjectId);
            Assert.Equal("create", actualEvent.Operation);
            Assert.Equal("class", actualEvent.EntityType);
            Assert.Equal(result.Name, actualEvent.EntityName);
            Assert.Equal(result.Id, actualEvent.EntityId);
        }

        [Fact]
        public async Task CreateClasses_Success_OnBulkCreate()
        {
            // Arrange
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

            // Act
            var result = await _classBusiness.BulkCreateClasses(pid, bulkDto);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Test Class 1", result.First().Name);
            Assert.Equal("Test Class 2", result.Last().Name);

            // Ensure the create event is logged for each class create
            var eventList = await Context.Events.ToListAsync();
            Assert.Equal(2, eventList.Count);
            var firstEvent = eventList[0];
            var secondEvent = eventList[1];

            Assert.Equal(pid, firstEvent.ProjectId);
            Assert.Equal("create", firstEvent.Operation);
            Assert.Equal("class", firstEvent.EntityType);
            Assert.Equal(result[0].Id, firstEvent.EntityId);

            Assert.Equal(pid, secondEvent.ProjectId);
            Assert.Equal("create", secondEvent.Operation);
            Assert.Equal("class", secondEvent.EntityType);
            Assert.Equal(result[1].Id, secondEvent.EntityId);
        }

        [Fact]
        public async Task CreateClass_Fails_IfNoName()
        {
            // Arrange
            var dto = new CreateClassRequestDto { Name = null, Description = "Test Description" };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _classBusiness.CreateClass(pid, dto));

            // Ensure that no events were created on failed class creation
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreateClass_Fails_IfEmptyName()
        {
            // Arrange
            var dto = new CreateClassRequestDto { Name = "", Description = "Test Description" };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _classBusiness.CreateClass(pid, dto));

            // Ensure that no events were created on failed class creation
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreateClass_Fails_IfNoProjectId()
        {
            // Arrange
            var dto = new CreateClassRequestDto { Name = "Test Class", Description = "Test Description" };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _classBusiness.CreateClass(pid + 99, dto));

            // Ensure that no events were created on failed class creation
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreateClass_Fails_IfDeletedProjectId()
        {
            // Arrange
            var project = await Context.Projects.FindAsync(pid);
            project.IsArchived = true;
            Context.Projects.Update(project);
            await Context.SaveChangesAsync();
            var dto = new CreateClassRequestDto { Name = "Test Class", Description = "Test Description" };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _classBusiness.CreateClass(pid, dto));

            // Ensure that no events were created on failed class creation
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreateClass_Fails_IfDuplicateName()
        {
            // Arange
            var duplicateName = "Duplicate Class";
            var dto = new CreateClassRequestDto { Name = duplicateName, Description = "Test Description" };
            await _classBusiness.CreateClass(pid, dto);

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(() => _classBusiness.CreateClass(pid, dto));

            // Ensure that only one event was logged (not the duplicate)
            var eventList = await Context.Events.ToListAsync();
            var firstAndOnlyEvent = eventList[0];
            Assert.Single(eventList);
            Assert.Equal("create", firstAndOnlyEvent.Operation);
        }

        #endregion

        #region GetAllClasses Tests

        [Fact]
        public async Task GetAllClasses_ReturnsOnlyForProjects()
        {

            // Arrange
            var p2 = new Project { Name = "ExtraProj", OrganizationId = oid };
            Context.Projects.Add(p2);
            await Context.SaveChangesAsync();

            await _classBusiness.CreateClass(pid, new CreateClassRequestDto { Name = $"Class1-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", Description = "Test" });
            await _classBusiness.CreateClass(p2.Id, new CreateClassRequestDto { Name = $"Class2-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", Description = "Test" });

            // Act
            var list = await _classBusiness.GetAllClasses(pid, true);

            // Assert
            Assert.Single(list);
            Assert.All(list, c => Assert.Equal(pid, c.ProjectId));
        }

        [Fact]
        public async Task GetAllClasses_ExcludesSoftDeleted()
        {
            // Arrange
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

            // Act
            var list = await _classBusiness.GetAllClasses(pid, true);

            // Assert
            Assert.DoesNotContain(list, c => c.Id == archivedClass.Id);
        }
        #endregion

        #region GetClass Tests

        [Fact]
        public async Task GetClass_Success_WhenExists()
        {
            // Arrange
            var testClass = new Class
            {
                Name = $"Test Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Test Description",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)

            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            // Act
            var result = await _classBusiness.GetClass(pid, testClass.Id, true);

            // Assert
            Assert.Equal(testClass.Id, result.Id);
        }

        [Fact]
        public async Task GetClass_Fails_IfNoProjectID()
        {
            // Arrange
            var testClass = new Class
            {
                Name = $"Test Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _classBusiness.GetClass(pid + 999, testClass.Id, true));
        }

        [Fact]
        public async Task GetClass_Fails_IfDeletedClass()
        {
            // Arrange
            var testClass = new Class
            {
                Name = $"Deleted Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                IsArchived = true

            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _classBusiness.GetClass(pid, testClass.Id, true));
        }
        #endregion

        #region UpdateClass Tests

        [Fact]
        public async Task UpdateClass_Success_ReturnsModifiedAt()
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

            // Add a small delay to ensure ModifiedAt is after CreatedAt
            await Task.Delay(50);

            var dto = new UpdateClassRequestDto { Name = $"Updated Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", Description = "Updated Description" };

            // Act
            var updatedResult = await _classBusiness.UpdateClass(pid, testClass.Id, dto);


            // Assert
            Assert.NotEqual(DateTime.MinValue, updatedResult.LastUpdatedAt);
            Assert.Equal("Updated Description", updatedResult.Description);

            // Ensure that an event was logged for the update
            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);

            var actualEvent = eventList[0];

            Assert.Equal(pid, actualEvent.ProjectId);
            Assert.Equal("update", actualEvent.Operation);
            Assert.Equal("class", actualEvent.EntityType);
            Assert.Equal(updatedResult.Id, actualEvent.EntityId);
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
            Assert.NotEqual(DateTime.MinValue, updatedResult.LastUpdatedAt);

            // Verify class was actually updated in database
            var updatedClass = await Context.Classes.FindAsync(testClass.Id);
            Assert.NotNull(updatedClass);
            Assert.Equal("Updated Description", updatedClass.Description);
            Assert.Equal(testClass.Name, updatedClass.Name);
            Assert.NotEqual(DateTime.MinValue, updatedClass.LastUpdatedAt);

            // Verify that get function gets updated version
            var getResult = await _classBusiness.GetClass(pid, testClass.Id, true);
            Assert.NotNull(getResult);
            Assert.Equal("Updated Description", getResult.Description);
            Assert.Equal(testClass.Name, getResult.Name);
            Assert.NotEqual(DateTime.MinValue, getResult.LastUpdatedAt);

            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);

            var actualEvent = eventList[0];

            Assert.Equal(pid, actualEvent.ProjectId);
            Assert.Equal("update", actualEvent.Operation);
            Assert.Equal("class", actualEvent.EntityType);
            Assert.Equal(updatedResult.Id, actualEvent.EntityId);
        }

        [Fact]
        public async Task UpdateClass_Fails_IfNotFound()
        {
            // Arrange
            var dto = new UpdateClassRequestDto { Name = "Updated Class", Description = "Updated Description" };
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _classBusiness.UpdateClass(pid, 99, dto));

            // Ensure No Event was logged if update fails
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }
        #endregion

        #region ArchiveClass Tests

        [Fact]
        public async Task ArchiveClass_Success_WhenExists()
        {
            // Arrange
            var beforeArchive = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            var testClass = new Class
            {
                Name = $"Class to Archive {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            // Act
            var archivedResult = await _classBusiness.ArchiveClass(pid, testClass.Id);

            // Assert
            Assert.True(archivedResult);

            // procedure is not traced by entity framework
            //this forces EF to sync to db on next query
            Context.ChangeTracker.Clear();

            var archivedClass = await Context.Classes.FindAsync(testClass.Id);
            Assert.NotNull(archivedClass);
            Assert.True(archivedClass.IsArchived);
            Assert.True(archivedClass.LastUpdatedAt >= beforeArchive);

            // Ensure that class soft delete event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);

            var actualEvent = eventList[0];

            Assert.Equal(pid, actualEvent.ProjectId);
            Assert.Equal("archive", actualEvent.Operation);
            Assert.Equal("class", actualEvent.EntityType);
            Assert.Equal(testClass.Id, actualEvent.EntityId);
        }

        [Fact]
        public async Task ArchiveClass_Fails_IfNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _classBusiness.ArchiveClass(pid, 99));
        }

        [Fact]
        public async Task ClassArchived_WhenProjectArchived()
        {
            // Arrange
            var beforeArchive = DateTime.UtcNow;
            var testClass = new Class
            {
                Name = $"Class in Project {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            // Act
            var deletedResult = await _projectBusiness.ArchiveProject(pid);
            Assert.True(deletedResult);

            //Makes sure the db is refreshed 
            Context.ChangeTracker.Clear();

            var archivedClass = await Context.Classes.FindAsync(testClass.Id);

            // Assert
            Assert.NotNull(archivedClass);
            Assert.True(archivedClass.IsArchived);
            Assert.True(archivedClass.LastUpdatedAt >= beforeArchive);

        }
        #endregion

        #region DeleteClass Tests

        [Fact]
        public async Task DeleteClass_Success_WhenExists()
        {
            // Arrange
            var testClass = new Class
            {
                Name = $"Class to Delete {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            // Act
            var deletedResult = await _classBusiness.DeleteClass(pid, testClass.Id);

            // Assert
            Assert.True(deletedResult);

            var deletedClass = await Context.Classes.FindAsync(testClass.Id);
            Assert.Null(deletedClass);

            // Ensure that class soft delete event was NOT logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task ForceDeleteClass_RemovesFromDatabase()
        {
            // Arrange
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

            // Act
            var deletedResult = await _classBusiness.DeleteClass(pid, testClass.Id);

            // Assert
            Assert.True(deletedResult);

            // Check if class is completely removed from database
            var deletedClass = await Context.Classes.FindAsync(testClass.Id);
            Assert.Null(deletedClass);
        }

        [Fact]
        public async Task DeleteClass_DeletesRelationshipsWithANullClass()
        {

            // Arrange
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

            // Act
            var deletedResult = await _classBusiness.DeleteClass(pid, testClass.Id);
            Assert.True(deletedResult);

            var deletedClass = await Context.Classes.FindAsync(testClass.Id);
            var deletedRelationship1 = await Context.Relationships.FindAsync(relationship1.Id);
            var deletedRelationship2 = await Context.Relationships.FindAsync(relationship2.Id);
            var intactRelationship3 = await Context.Relationships.FindAsync(relationship3.Id);

            // Assert
            Assert.Null(deletedClass);
            Assert.Null(deletedRelationship1);
            Assert.Null(deletedRelationship2);
            Assert.NotNull(intactRelationship3);
        }

        [Fact]
        public async Task DeleteClass_DeletesDownstreamRelationships()
        {
            // Arrange
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

            // Act
            var deletedResult = await _classBusiness.DeleteClass(pid, testClass.Id);
            Assert.True(deletedResult);

            var deletedRelationship1 = await Context.Relationships.FindAsync(relationship1.Id);
            var deletedRelationship2 = await Context.Relationships.FindAsync(relationship2.Id);
            var deletedRelationship3 = await Context.Relationships.FindAsync(relationship3.Id);
            var deletedRelationship4 = await Context.Relationships.FindAsync(relationship4.Id);
            var intactRelationship5 = await Context.Relationships.FindAsync(relationship5.Id);

            // Assert
            Assert.Null(deletedRelationship1);
            Assert.Null(deletedRelationship2);
            Assert.Null(deletedRelationship3);
            Assert.Null(deletedRelationship4);
            Assert.NotNull(intactRelationship5);
        }

        [Fact]
        public async Task DeleteClass_DeletesDownstreamRecords()
        {
            // Arrange
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

            // Act
            var deletedResult = await _classBusiness.DeleteClass(pid, testClass.Id);

            // Verify downstream records are also deleted (cascade delete)
            var remainingRecords = Context.Records
                .Where(r => r.ClassId == testClass.Id)
                .ToList();

            // Assert
            Assert.True(deletedResult);
            Assert.Empty(remainingRecords);
        }
        #endregion

        #region UnarchiveClass Tests
        [Fact]
        public async Task UnarchiveClass_SuccessfullyUnarchivesClassAndReturnsTrue()
        {
            // Arrange
            var testClass = new Class
            {
                Name = $"Archived Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                IsArchived = true

            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            // Act
            var result = await _classBusiness.UnarchiveClass(pid, testClass.Id);

            //this forces EF to sync to db on next query
            Context.ChangeTracker.Clear();

            var updated = await Context.Classes.FindAsync(testClass.Id);
            // Assert
            Assert.True(result);
            Assert.False(updated?.IsArchived);
        }

        [Fact]
        public async Task UnarchiveClass_Throws_IfClassNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _classBusiness.UnarchiveClass(pid, 99999));
        }

        [Fact]
        public async Task UnarchiveClass_Throws_IfClassProjectMismatch()
        {
            // Arrange
            var otherProject = new Project { Name = "Other Project", OrganizationId = oid };
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

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _classBusiness.UnarchiveClass(pid, testClass.Id));
        }

        [Fact]
        public async Task UnarchiveClass_Throws_IfClassNotArchived()
        {
            // Arrrange
            var testClass = new Class
            {
                Name = "Active Class",
                ProjectId = pid,
                IsArchived = false
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _classBusiness.UnarchiveClass(pid, testClass.Id));
        }

        #endregion

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
            Assert.Single(result);
            Assert.Equal("TestValidationClass", result.First().Name);
            Assert.Equal(pid, result.First().ProjectId);
        }

        [Fact]
        public async Task GetClassesByName_MissingClassNames_ThrowsKeyNotFoundException()
        {
            // Arrange
            var classNames = new List<string> { "NonExistentClass" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _classBusiness.GetClassesByName(pid, classNames));
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
        #region LastUpdatedBy Tests
        [Fact]
        public async Task CreateClass_Success_StoresLastUpdatedByUserId()
        {
            // Arrange
            var testClass = new Class
            {
                Name = $"Test Class with User {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Test Description with User ID",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = uid,
                IsArchived = false
            };

            // Act
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            // Assert
            var savedClass = await Context.Classes.FindAsync(testClass.Id);
            Assert.NotNull(savedClass);
            Assert.Equal(uid, savedClass.LastUpdatedBy);
        }

        [Fact]
        public async Task CreateClass_Success_NavigationPropertyLoadsUser()
        {
            // Arrange
            var testClass = new Class
            {
                Name = $"Test Class Navigation {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Test Navigation Property",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = uid,
                IsArchived = false
            };

            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            // Act
            var classWithUser = await Context.Classes
                .Include(c => c.LastUpdatedByUser)
                .FirstAsync(c => c.Id == testClass.Id);

            // Assert
            Assert.NotNull(classWithUser.LastUpdatedByUser);
            Assert.Equal("Test User", classWithUser.LastUpdatedByUser.Name);
            Assert.Equal("test.user@test.com", classWithUser.LastUpdatedByUser.Email);
            Assert.Equal(uid, classWithUser.LastUpdatedBy);
        }

        [Fact]
        public async Task CreateClass_Success_WithNullLastUpdatedBy()
        {
            // Arrange
            var testClass = new Class
            {
                Name = $"Test Class Null User {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Test with null LastUpdatedBy",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null,
                IsArchived = false
            };

            // Act
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            // Assert
            var savedClass = await Context.Classes.FindAsync(testClass.Id);
            Assert.NotNull(savedClass);
            Assert.Null(savedClass.LastUpdatedBy);

            var classWithUser = await Context.Classes
                .Include(c => c.LastUpdatedByUser)
                .FirstAsync(c => c.Id == testClass.Id);

            Assert.Null(classWithUser.LastUpdatedByUser);
        }

        [Fact]
        public async Task UpdateClass_Success_UpdatesLastUpdatedByUserId()
        {
            // Arrange
            var testClass = new Class
            {
                Name = $"Original Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Original Description",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            // Act
            testClass.LastUpdatedBy = uid;
            testClass.Description = "Updated Description";
            testClass.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            Context.Classes.Update(testClass);
            await Context.SaveChangesAsync();

            // Assert
            var updatedClass = await Context.Classes
                .Include(c => c.LastUpdatedByUser)
                .FirstAsync(c => c.Id == testClass.Id);

            Assert.Equal(uid, updatedClass.LastUpdatedBy);
            Assert.NotNull(updatedClass.LastUpdatedByUser);
            Assert.Equal("Test User", updatedClass.LastUpdatedByUser.Name);
            Assert.Equal("Updated Description", updatedClass.Description);
        }
        #endregion
        protected override async Task SeedTestDataAsync()
        {
            await base.SeedTestDataAsync();

            var org = new Organization { Name = "Test Org" };
            Context.Organizations.Add(org);
            await Context.SaveChangesAsync();
            oid = org.Id;

            var project = new Project { Name = "Project 1", OrganizationId = oid };
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
        }
    }
}