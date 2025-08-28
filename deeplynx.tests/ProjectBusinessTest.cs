using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Record = deeplynx.datalayer.Models.Record;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class ProjectBusinessTests : IntegrationTestBase
    {
        private ProjectBusiness _projectBusiness = null!;
        private DataSourceBusiness _dataSourceBusiness = null!;
        private ClassBusiness _classBusiness = null!;
        private Mock<IEdgeBusiness> _mockEdgeBusiness = null!;
        private Mock<IRecordBusiness> _mockRecordBusiness = null!;
        private Mock<IRecordMappingBusiness> _mockRecordMappingBusiness = null!;
        private Mock<IRelationshipBusiness> _mockRelationshipBusiness = null!;
        private Mock<IEdgeMappingBusiness> _mockEdgeMappingBusiness = null!;
        private Mock<ILogger<ProjectBusiness>> _mockLogger = null!;
        private Mock<IObjectStorageBusiness> _objectStorageBusiness = null!;

        public long TestProjectId;
        public long TestClassId;
        public long TestDataSourceId;

        public ProjectBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _objectStorageBusiness = new Mock<IObjectStorageBusiness>();
            _mockRecordBusiness = new Mock<IRecordBusiness>();
            _mockRecordMappingBusiness = new Mock<IRecordMappingBusiness>();
            _mockRelationshipBusiness = new Mock<IRelationshipBusiness>();
            _mockEdgeMappingBusiness = new Mock<IEdgeMappingBusiness>();
            _mockEdgeBusiness = new Mock<IEdgeBusiness>();
            _mockLogger = new Mock<ILogger<ProjectBusiness>>();
            
            _dataSourceBusiness = new DataSourceBusiness(Context, _mockEdgeBusiness.Object, _mockRecordBusiness.Object);
            _classBusiness = new ClassBusiness(
                Context, _mockEdgeMappingBusiness.Object, _mockRecordBusiness.Object, 
                _mockRecordMappingBusiness.Object, _mockRelationshipBusiness.Object);
            _projectBusiness = new ProjectBusiness(Context, _mockLogger.Object, _classBusiness, _dataSourceBusiness, _objectStorageBusiness.Object);
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
            result.CreatedAt.Should().BeOnOrAfter(now);
            result.Name.Should().Be(dto.Name);
            result.Description.Should().Be(dto.Description);
            result.Abbreviation.Should().Be(dto.Abbreviation);
        }

        [Fact]
        public async Task CreateProject_Creates_DefaultClasses()
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
            var projectid = new List<long>(); 
            projectid.Add(project.Id);

            var classResult = await _classBusiness.GetAllClasses(projectid, true);
            classResult.Count.Should().Be(2);
            classResult[0].Name.Should().Be("Timeseries");
            classResult[1].Name.Should().Be("Report");
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

            var dataSourceResult = await _dataSourceBusiness.GetAllDataSources(project.Id, true);
            dataSourceResult.Count.Should().Be(1);
            dataSourceResult[0].Name.Should().Be("Default Data Source");
            dataSourceResult[0].Description.Should().Be("This data source was created alongside the project for ease of use.");
        }

        [Fact]
        public async Task CreateProject_Fails_IfNoName()
        {
           
            var dto = new CreateProjectRequestDto { Name = null!, Description = "Test Description" };

           
            var result = () => _projectBusiness.CreateProject(dto);
            await result.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task CreateProject_Fails_IfEmptyName()
        {
            
            var dto = new CreateProjectRequestDto { Name = "", Description = "Test Description" };

           
            var result = () => _projectBusiness.CreateProject(dto);
            await result.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task GetAllProjects_ExcludesSoftDeleted()
        {
            
            var activeProject = new Project
            {
                Name = $"Active Project {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null
            };

            var archivedProject = new Project
            {
                Name = $"Archived Project {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
                ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            Context.Projects.Add(activeProject);
            Context.Projects.Add(archivedProject);
            await Context.SaveChangesAsync();

           
            var listWithArchived = await _projectBusiness.GetAllProjects(false);
            var listWithoutArchived = await _projectBusiness.GetAllProjects(true);

          
            listWithArchived.Should().Contain(p => p.Id == archivedProject.Id);
            listWithoutArchived.Should().NotContain(p => p.Id == archivedProject.Id);
        }

        [Fact]
        public async Task GetProject_Success_WhenExists()
        {
           
            var result = await _projectBusiness.GetProject(TestProjectId, true);

          
            result.Should().NotBeNull();
            result.Id.Should().Be(TestProjectId);
            result.Name.Should().Be("Test Project");
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
            
            var testProject = new Project
            {
                Name = $"Deleted Project {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
                ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Projects.Add(testProject);
            await Context.SaveChangesAsync();

           
            var result = () => _projectBusiness.GetProject(testProject.Id, true);
            await result.Should().ThrowAsync<KeyNotFoundException>();
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
                CreatedAt = originalCreatedAt,
                CreatedBy = null,
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

          
            updatedResult.ModifiedAt.Should().BeOnOrAfter(updatedResult.CreatedAt);
            updatedResult.Name.Should().Be(dto.Name);
            updatedResult.Description.Should().Be(dto.Description);
            updatedResult.Abbreviation.Should().Be(dto.Abbreviation);
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
        }

        [Fact]
        public async Task DeleteProject_Success_WhenExists()
        {
            
            var testProject = new Project
            {
                Name = $"Project to Delete {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
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
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
            };
            Context.Projects.Add(testProject);
            await Context.SaveChangesAsync();

           
            var archivedResult = await _projectBusiness.ArchiveProject(testProject.Id);

          
            archivedResult.Should().BeTrue();

            // Force EF to sync with database
            Context.ChangeTracker.Clear();

            var archivedProject = await Context.Projects.FindAsync(testProject.Id);
            archivedProject.Should().NotBeNull();
            archivedProject!.ArchivedAt.Should().NotBeNull();
            archivedProject.ArchivedAt.Should().BeOnOrAfter(beforeArchive);
            archivedProject.ArchivedAt.Should().BeOnOrBefore(DateTime.UtcNow);
        }

        [Fact]
        public async Task UnarchiveProject_Success_WhenArchived()
        {
            
            var testProject = new Project
            {
                Name = $"Archived Project {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
                ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Projects.Add(testProject);
            await Context.SaveChangesAsync();

           
            var unarchivedResult = await _projectBusiness.UnarchiveProject(testProject.Id);

          
            unarchivedResult.Should().BeTrue();

            // Force EF to sync with database
            Context.ChangeTracker.Clear();

            var unarchivedProject = await Context.Projects.FindAsync(testProject.Id);
            unarchivedProject.Should().NotBeNull();
            unarchivedProject!.ArchivedAt.Should().BeNull();
        }

        [Fact]
        public async Task ArchiveProject_Fails_IfNotFound()
        {
            
            const long nonExistentId = 999999;

           
            var result = () => _projectBusiness.ArchiveProject(nonExistentId);
            await result.Should().ThrowAsync<KeyNotFoundException>();
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
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                ArchivedAt = null // Not archived
            };
            Context.Projects.Add(activeProject);
            await Context.SaveChangesAsync();

           
            var result = () => _projectBusiness.UnarchiveProject(activeProject.Id);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task GetProjectStats_Success_ReturnsCorrectCounts()
        {
            var result = await _projectBusiness.GetProjectStats(TestProjectId);

          
            result.classes.Should().BeGreaterThan(0);
            result.records.Should().BeGreaterThan(0);
            result.datasources.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetMultiProjectRecords_Success_ReturnsRecordsFromMultipleProjects()
        {
            
            var secondProject = new Project
            {
                Name = $"Second Project {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
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
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.ObjectStorages.Add(secondObjectStorage);
            await Context.SaveChangesAsync();

            // Create additional class and datasource for second project
            var secondClass = new Class
            {
                Name = "Second Test Class",
                ProjectId = secondProject.Id,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(secondClass);

            var secondDataSource = new DataSource
            {
                Name = "Second Test DataSource",
                ProjectId = secondProject.Id,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.DataSources.Add(secondDataSource);
            await Context.SaveChangesAsync();

            // Create actual Record entities first to satisfy foreign key constraints
            var record1 = new Record
            {
                Name = "Multi Project Record 1",
                ProjectId = TestProjectId,
                DataSourceId = TestDataSourceId,
                ClassId = TestClassId,
                Properties = "{}",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
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
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
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
                ProjectId = TestProjectId,
                ObjectStorageName = secondProject.Name,
                ProjectName = "Test Project",
                Properties = record1.Properties,
                ClassName = "Test Class",
                DataSourceName = "Test Datasource",
                OriginalId = record1.OriginalId,
                Tags = "[]",
                Description = record1.Description,
                CreatedAt = record1.CreatedAt,
                LastUpdatedAt = record1.CreatedAt
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
                CreatedAt = record2.CreatedAt,
                LastUpdatedAt = record2.CreatedAt
            };

            Context.HistoricalRecords.AddRange(historicalRecord1, historicalRecord2);
            await Context.SaveChangesAsync();

           
            var projectIds = new long[] { TestProjectId, secondProject.Id };
            var result = await _projectBusiness.GetMultiProjectRecords(projectIds, true);

            result.Should().Contain(r => r.ProjectId == TestProjectId);
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
                CreatedBy = "test@example.com",
                CreatedAt = now,
                ModifiedBy = "modified@example.com",
                ModifiedAt = now.AddDays(1),
                ArchivedAt = null
            };

          
            dto.Id.Should().Be(1);
            dto.Name.Should().Be("Test Project");
            dto.Description.Should().Be("Test Description");
            dto.Abbreviation.Should().Be("TST");
            dto.CreatedBy.Should().Be("test@example.com");
            dto.CreatedAt.Should().Be(now);
            dto.ModifiedBy.Should().Be("modified@example.com");
            dto.ModifiedAt.Should().Be(now.AddDays(1));
            dto.ArchivedAt.Should().BeNull();
        }

        protected override async Task SeedTestDataAsync()
        {
            await base.SeedTestDataAsync();
            var testProject = new Project
            {
                Name = "Test Project",
                Description = "Test project for unit tests",
                Abbreviation = "TST",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Projects.Add(testProject);
            await Context.SaveChangesAsync();
            TestProjectId = testProject.Id;

            var testClass = new Class
            {
                Name = "Test Class",
                ProjectId = TestProjectId,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();
            TestClassId = testClass.Id;
            
            var testDataSource = new DataSource
            {
                Name = "Test DataSource",
                ProjectId = TestProjectId,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.DataSources.Add(testDataSource);
            await Context.SaveChangesAsync();
            TestDataSourceId = testDataSource.Id;
            
            
            var testRecord = new Record
            {
                Name = "Test Record",
                ProjectId = TestProjectId,
                DataSourceId = TestDataSourceId,
                ClassId = TestClassId,
                Properties = "{}",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                OriginalId = "test-original-1",
                Description = "Test record for unit tests"
            };
            Context.Records.Add(testRecord);
            await Context.SaveChangesAsync();
        }
    }
}
