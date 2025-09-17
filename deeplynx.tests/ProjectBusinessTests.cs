using System.ComponentModel.DataAnnotations;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Record = deeplynx.datalayer.Models.Record;
using System.Text.Json.Nodes;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class ProjectBusinessTests : IntegrationTestBase
    {
        private ProjectBusiness _projectBusiness = null!;
        private DataSourceBusiness _dataSourceBusiness = null!;
        private ClassBusiness _classBusiness = null!;
        private EventBusiness _eventBusiness = null!;
        private Mock<IEdgeBusiness> _mockEdgeBusiness = null!;
        private Mock<IRecordBusiness> _mockRecordBusiness = null!;
        private Mock<IRelationshipBusiness> _mockRelationshipBusiness = null!;
        private Mock<ILogger<ProjectBusiness>> _mockLogger = null!;
        private Mock<IObjectStorageBusiness> _objectStorageBusiness = null!;
        
        public long TestProject1Id;
        public long ArchivedProject2Id;
        public long TestClassId;
        public long TestDataSourceId;

        public ProjectBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            Environment.SetEnvironmentVariable("FILE_STORAGE_METHOD", "filesystem");
            Environment.SetEnvironmentVariable("STORAGE_DIRECTORY", "./storage/");
            Environment.SetEnvironmentVariable("AZURE_OBJECT_CONNECTION_STRING", "azure-example-connection-string");
            Environment.SetEnvironmentVariable("AWS_S3_CONNECTION_STRING", "aws-example-connection-string");
            _eventBusiness = new EventBusiness(Context, _cacheBusiness);
            _objectStorageBusiness = new Mock<IObjectStorageBusiness>();
            _mockRecordBusiness = new Mock<IRecordBusiness>();
            _mockRelationshipBusiness = new Mock<IRelationshipBusiness>();
            _mockEdgeBusiness = new Mock<IEdgeBusiness>();
            _mockLogger = new Mock<ILogger<ProjectBusiness>>();

            _dataSourceBusiness = new DataSourceBusiness(Context, _cacheBusiness, _mockEdgeBusiness.Object, _mockRecordBusiness.Object, _eventBusiness);
            _classBusiness = new ClassBusiness(
                Context, _cacheBusiness, _mockRecordBusiness.Object, 
                _mockRelationshipBusiness.Object, _eventBusiness);
            _projectBusiness = new ProjectBusiness(Context, _cacheBusiness, _mockLogger.Object, _classBusiness, _dataSourceBusiness, _objectStorageBusiness.Object, _eventBusiness);
        }
        
        [Fact]
        public async Task GetAllProjects_ExcludesSoftDeleted()
        {
            var listWithArchived = await _projectBusiness.GetAllProjects(false);
            var listWithoutArchived = await _projectBusiness.GetAllProjects(true);

            // Assert
            listWithArchived.Should().Contain(p => p.Id == ArchivedProject2Id);
            listWithoutArchived.Should().NotContain(p => p.Id == ArchivedProject2Id);
        }
        
        [Fact]
        public async Task CreateProject_Success_ReturnsIdAndCreatedAt()
        {
            var now = DateTime.UtcNow;
            var dto = new CreateProjectRequestDto
            {
                Name = $"Test Project {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Test Description",
                Abbreviation = "TST"
            };
            
            var result = await _projectBusiness.CreateProject(dto);
            
            result.Id.Should().BeGreaterThan(0);
            result.LastUpdatedAt.Should().BeOnOrAfter(now);
            result.Name.Should().Be(dto.Name);
            result.Description.Should().Be(dto.Description);
            result.Abbreviation.Should().Be(dto.Abbreviation);
            
            // Ensure that the project create event was logged
            var eventList = Context.Events.ToList();
            eventList.Should().HaveCount(5);
            // three classes and a datasource will be logged before project event is logged
            eventList[4].Should().BeEquivalentTo(new
            {
                ProjectId = result.Id,
                Operation = "create",
                EntityType = "project",
                EntityId = result.Id,
            });
        }

        [Fact]
        public async Task CreateProject_Creates_DefaultClasses()
        {
            var dto = new CreateProjectRequestDto
            {
                Name = $"Test Project {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Test Description",
                Abbreviation = "TST"
            };
            
            var project = await _projectBusiness.CreateProject(dto);
            project.Name.Should().Be(dto.Name);
            var classResult = await _classBusiness.GetAllClasses([project.Id], true);
            classResult.Count.Should().Be(3);
            classResult[0].Name.Should().Be("Timeseries");
            classResult[1].Name.Should().Be("Report");
            classResult[2].Name.Should().Be("File");
            
            // Ensure that the project create event was logged
            var eventList = Context.Events.ToList();
            eventList.Should().HaveCount(5);
            // three classes and a datasource will be logged before project event is logged
            eventList[4].Should().BeEquivalentTo(new
            {
                ProjectId = project.Id,
                Operation = "create",
                EntityType = "project",
                EntityId = project.Id,
            });
        }
        
        [Fact]
        public async Task CreateProject_Creates_DefaultDataSource()
        {
            var now = DateTime.UtcNow;
            var dto = new CreateProjectRequestDto
            {
                Name = $"Test Project {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Test Description",
                Abbreviation = "TST"
            };
            
            var project = await _projectBusiness.CreateProject(dto);
            project.Name.Should().Be(dto.Name);
            
            var dataSourceResult = await _dataSourceBusiness.GetAllDataSources([project.Id], true);
            dataSourceResult.Count.Should().Be(1);
            dataSourceResult[0].Name.Should().Be("Default Data Source");
            dataSourceResult[0].Description.Should().Be("This data source was created alongside the project for ease of use.");
            
            // Ensure that the project create event was logged
            var eventList = Context.Events.ToList();
            eventList.Should().HaveCount(5);
            // three classes and a datasource will be logged before project event is logged
            eventList[4].Should().BeEquivalentTo(new
            {
                ProjectId = project.Id,
                Operation = "create",
                EntityType = "project",
                EntityId = project.Id,
            });
        }
        
        [Fact]
        public async Task CreateProject_Fails_IfNoName()
        {
            var dto = new CreateProjectRequestDto { Name = null!, Description = "Test Description" };

           
            var result = () => _projectBusiness.CreateProject(dto);
            await result.Should().ThrowAsync<ValidationException>();
            
            // Ensure that no project create event is logged
            var eventList = Context.Events.ToList();
            eventList.Should().HaveCount(0);
        }

        [Fact]
        public async Task CreateProject_Fails_IfEmptyName()
        {
            var dto = new CreateProjectRequestDto { Name = "", Description = "Test Description" };

           
            var result = () => _projectBusiness.CreateProject(dto);
            await result.Should().ThrowAsync<ValidationException>();
            
            // Ensure that no project create event is logged
            var eventList = Context.Events.ToList();
            eventList.Should().HaveCount(0);
        }
        
        [Fact]
        public async Task GetProject_Success_WhenExists()
        {
            var result = await _projectBusiness.GetProject(TestProject1Id, true);
        
            // Assert
            result.Id.Should().Be(TestProject1Id);
            result.Name.Should().Be("Test Project");
            result.Description.Should().Be("Test project for unit tests");
        }
        
        [Fact]
        public async Task GetProject_Fails_IfNotFound()
        {
            const long nonExistentId = 999999;

           
            var result = () => _projectBusiness.GetProject(nonExistentId, true);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }
        
        [Fact]
        public async Task GetProject_Fails_IfDeletedProject()
        {
            Func<Task> act = async () => { await _projectBusiness.GetProject(ArchivedProject2Id, true); };

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task UpdateProject_Success_ReturnsModifiedAt()
        {
            var originalCreatedAt = DateTime.SpecifyKind(DateTime.UtcNow.AddMinutes(-5), DateTimeKind.Unspecified);
            var testProject = new Project
            {
                Name = $"Original Project {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Original Description",
                Abbreviation = "ORG",
                LastUpdatedAt = originalCreatedAt,
                LastUpdatedBy = null,
            };
            Context.Projects.Add(testProject);
            await Context.SaveChangesAsync();

            // Add delay to ensure ModifiedAt is after CreatedAt
            await Task.Delay(100);

            var dto = new UpdateProjectRequestDto
            {
                Name = $"Updated Project {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Updated Description",
                Abbreviation = "UPD"
            };
            
            var updatedResult = await _projectBusiness.UpdateProject(testProject.Id, dto);
            
            updatedResult.Name.Should().Be(dto.Name);
            updatedResult.Description.Should().Be(dto.Description);
            updatedResult.Abbreviation.Should().Be(dto.Abbreviation);
            
            // Ensure that Project Update Event was logged
            var eventList = Context.Events.ToList();
            eventList.Should().HaveCount(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                ProjectId = testProject.Id,
                EntityType = "project",
                EntityId = testProject.Id,
                Operation = "update"
            });
        }

        [Fact]
        public async Task UpdateProject_Fails_IfNotFound()
        {
            var dto = new UpdateProjectRequestDto
            {
                Name = "Updated Project",
                Description = "Updated Description",
                Abbreviation = "UPD"
            };
            const long nonExistentId = 999999;

           
            var result = () => _projectBusiness.UpdateProject(nonExistentId, dto);
            await result.Should().ThrowAsync<KeyNotFoundException>();
            
            // Ensure that no project create event is logged
            var eventList = Context.Events.ToList();
            eventList.Should().HaveCount(0);
        }

        [Fact]
        public async Task DeleteProject_Success_WhenExists()
        {
            var testProject = new Project
            {
                Name = $"Project to Delete {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null,
            };
            Context.Projects.Add(testProject);
            await Context.SaveChangesAsync();

           
            var deletedResult = await _projectBusiness.DeleteProject(testProject.Id);

          
            deletedResult.Should().BeTrue();

            var deletedProject = await Context.Projects.FindAsync(testProject.Id);
            deletedProject.Should().BeNull();
        }

        [Fact]
        public async Task ArchiveProject_Success_WhenExists()
        {
            var beforeArchive = DateTime.UtcNow;
            var testProject = new Project
            {
                Name = $"Project to Archive {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null,
            };
            Context.Projects.Add(testProject);
            await Context.SaveChangesAsync();
            
            var verify = Context.Projects.Find(testProject.Id);
           
            var archivedResult = await _projectBusiness.ArchiveProject(testProject.Id);
            
            archivedResult.Should().BeTrue();

            // Force EF to sync with database
            Context.ChangeTracker.Clear();

            var archivedProject = await Context.Projects.FindAsync(testProject.Id);
            archivedProject.Should().NotBeNull();
            archivedProject!.IsArchived.Should().BeTrue();
            archivedProject.LastUpdatedAt.Should().BeOnOrAfter(beforeArchive);
            archivedProject.LastUpdatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
            // Ensure that project soft delete event was logged
            var eventList = Context.Events.ToList();
            eventList.Should().HaveCount(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                ProjectId = testProject.Id,
                EntityType = "project",
                EntityId = testProject.Id,
                Operation = "archive"
            });
        }

        [Fact]
        public async Task UnarchiveProject_Success_WhenArchived()
        {
            var testProject = new Project
            {
                Name = $"Archived Project {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null,
                IsArchived = true
            };
            Context.Projects.Add(testProject);
            await Context.SaveChangesAsync();

            var unarchivedResult = await _projectBusiness.UnarchiveProject(testProject.Id);

            unarchivedResult.Should().BeTrue();

            // Force EF to sync with database
            Context.ChangeTracker.Clear();

            var unarchivedProject = await Context.Projects.FindAsync(testProject.Id);
            unarchivedProject.Should().NotBeNull();
            unarchivedProject!.IsArchived.Should().BeFalse(); 
        }

        [Fact]
        public async Task ArchiveProject_Fails_IfNotFound()
        {
            const long nonExistentId = 999999;

           
            var result = () => _projectBusiness.ArchiveProject(nonExistentId);
            await result.Should().ThrowAsync<KeyNotFoundException>();
            
            // Ensure that project soft delete event was NOT logged
            var eventList = Context.Events.ToList();
            eventList.Should().HaveCount(0);
        }

        [Fact]
        public async Task DeleteProject_Fails_IfNotFound()
        {
            const long nonExistentId = 999999;

           
            var result = () => _projectBusiness.DeleteProject(nonExistentId);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task UnarchiveProject_Fails_IfNotFound()
        {
            const long nonExistentId = 999999;

           
            var result = () => _projectBusiness.UnarchiveProject(nonExistentId);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task UnarchiveProject_Fails_IfNotArchived()
        {
            var activeProject = new Project
            {
                Name = $"Active Project {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null,
            };
            Context.Projects.Add(activeProject);
            await Context.SaveChangesAsync();

           
            var result = () => _projectBusiness.UnarchiveProject(activeProject.Id);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task GetProjectStats_Success_ReturnsCorrectCounts()
        {
            var result = await _projectBusiness.GetProjectStats(TestProject1Id);

          
            result.classes.Should().Be(1);    // 1 test class
            result.records.Should().Be(1);    // 1 test record  
            result.datasources.Should().Be(1); //1 datasource
        }

        [Fact]
        public async Task GetMultiProjectRecords_Success_ReturnsRecordsFromMultipleProjects()
        {
            var secondProject = new Project
            {
                Name = $"Second Project {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Projects.Add(secondProject);
            await Context.SaveChangesAsync();
            
            var config = new JsonObject();
            var secondObjectStorage = new ObjectStorage
            {
                Name = "Object Storage 1",
                Type = "filesystem",
                Config = config.ToString(),
                ProjectId = secondProject.Id,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.ObjectStorages.Add(secondObjectStorage);
            await Context.SaveChangesAsync();

            // Create additional class and datasource for second project
            var secondClass = new Class
            {
                Name = "Second Test Class",
                ProjectId = secondProject.Id,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(secondClass);

            var secondDataSource = new DataSource
            {
                Name = "Second Test DataSource",
                ProjectId = secondProject.Id,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.DataSources.Add(secondDataSource);
            await Context.SaveChangesAsync();

            // Create actual Record entities first to satisfy foreign key constraints
            var record1 = new Record
            {
                Name = "Multi Project Record 1",
                ProjectId = TestProject1Id,
                DataSourceId = TestDataSourceId,
                ClassId = TestClassId,
                Properties = "{}",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                OriginalId = "multi-original-1",
                Description = "Multi project test description 1"
            };

            var record2 = new Record
            {
                Name = "Multi Project Record 2",
                ProjectId = secondProject.Id,
                DataSourceId = secondDataSource.Id,
                ClassId = secondClass.Id,
                Properties = "{}",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                OriginalId = "multi-original-2",
                Description = "Multi project test description 2"
            };

            Context.Records.AddRange(record1, record2);
            await Context.SaveChangesAsync();

            // Create historical records for both projects using valid RecordIds
            var historicalRecord1 = new HistoricalRecord
            {
                RecordId = record1.Id,
                Name = record1.Name,
                ProjectId = TestProject1Id,
                ObjectStorageName = secondProject.Name,
                ProjectName = "Test Project",
                Properties = record1.Properties,
                ClassName = "Test Class",
                DataSourceName = "Test Datasource",
                OriginalId = record1.OriginalId,
                Tags = "[]",
                Description = record1.Description,
                LastUpdatedAt = record1.LastUpdatedAt
            };

            var historicalRecord2 = new HistoricalRecord
            {
                RecordId = record2.Id,
                Name = record2.Name,
                ProjectId = secondProject.Id,
                ObjectStorageId = secondObjectStorage.Id,
                ObjectStorageName = secondObjectStorage.Name,
                ProjectName = secondProject.Name,
                Properties = record2.Properties,
                DataSourceName = secondDataSource.Name,
                ClassName = secondClass.Name,
                OriginalId = record2.OriginalId,
                Tags = "[]",
                Description = record2.Description,
                LastUpdatedAt = record2.LastUpdatedAt
            };

            Context.HistoricalRecords.AddRange(historicalRecord1, historicalRecord2);
            await Context.SaveChangesAsync();

           
            var projectIds = new long[] { TestProject1Id, secondProject.Id };
            var result = await _projectBusiness.GetMultiProjectRecords(projectIds, true);

            result.Should().Contain(r => r.ProjectId == TestProject1Id);
            result.Should().Contain(r => r.ProjectId == secondProject.Id);
        }

        [Fact]
        public void ProjectRequestDto_AllProperties_CanBeSetAndRetrieved()
        {
            var dto = new CreateProjectRequestDto
            {
                Name = "Test Project",
                Description = "Test Description",
                Abbreviation = "TST"
            };

          
            dto.Name.Should().Be("Test Project");
            dto.Description.Should().Be("Test Description");
            dto.Abbreviation.Should().Be("TST");
        }

        [Fact]
        public void ProjectResponseDto_AllProperties_CanBeSetAndRetrieved()
        {
            
            var now = DateTime.UtcNow;

           
            var dto = new ProjectResponseDto
            {
                Id = 1,
                Name = "Test Project",
                Description = "Test Description",
                Abbreviation = "TST",
                LastUpdatedBy = "test@example.com",
                LastUpdatedAt = now,
                IsArchived = false,
                OrganizationId = null
            };

          
            dto.Id.Should().Be(1);
            dto.Name.Should().Be("Test Project");
            dto.Description.Should().Be("Test Description");
            dto.Abbreviation.Should().Be("TST");
            dto.LastUpdatedBy.Should().Be("test@example.com");
            dto.IsArchived.Should().BeFalse();
        }
        
        protected override async Task SeedTestDataAsync()
        {
            await base.SeedTestDataAsync();
            var testProjects = new List<Project>
            {
                new Project {
                    Name = "Test Project",
                    Description = "Test project for unit tests",
                    Abbreviation = "TST",
                    LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    IsArchived = false 
                },
                new Project
                {
                    Name = $"Archived Project",
                    Description = "Archived project for unit tests",
                    Abbreviation = "TST",
                    LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    IsArchived = true
                }
            };
            Context.Projects.AddRange(testProjects);
            await Context.SaveChangesAsync();
            TestProject1Id = testProjects[0].Id;
            ArchivedProject2Id = testProjects[1].Id;

            var testClass = new Class
            {
                Name = "Test Class",
                ProjectId = TestProject1Id,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                IsArchived = false
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();
            TestClassId = testClass.Id;
            
            var testDataSource = new DataSource
            {
                Name = "Test DataSource",
                ProjectId = TestProject1Id,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                IsArchived = false
            };
            Context.DataSources.Add(testDataSource);
            await Context.SaveChangesAsync();
            TestDataSourceId = testDataSource.Id;
            
            var testRecord = new Record
            {
                Name = "Test Record",
                ProjectId = TestProject1Id,
                DataSourceId = TestDataSourceId,
                ClassId = TestClassId,
                Properties = "{}",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                OriginalId = "test-original-1",
                Description = "Test record for unit tests",
                IsArchived = false
            };
            Context.Records.Add(testRecord);
            await Context.SaveChangesAsync();
            
        }
    }
}
