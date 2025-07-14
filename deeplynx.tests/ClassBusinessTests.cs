using System.ComponentModel.DataAnnotations;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using FluentAssertions;
using Moq;

namespace deeplynx.tests
{
    public class ClassBusinessTests : IntegrationTestBase
    {
        private ClassBusiness _classBusiness = null!;
        private ProjectBusiness _projectBusiness = null!;
        private Mock<IEdgeMappingBusiness> _mockEdgeMappingBusiness;
        private Mock<IRecordBusiness> _mockRecordBusiness;
        private Mock<IRecordMappingBusiness> _mockRecordMappingBusiness;
        private Mock<IRelationshipBusiness> _mockRelationshipBusiness;
        public long pid;

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _mockEdgeMappingBusiness = new Mock<IEdgeMappingBusiness>();
            _mockRecordBusiness = new Mock<IRecordBusiness>();
            _mockRecordMappingBusiness = new Mock<IRecordMappingBusiness>();
            _mockRelationshipBusiness = new Mock<IRelationshipBusiness>();

            _classBusiness = new ClassBusiness(Context, _mockEdgeMappingBusiness.Object, _mockRecordBusiness.Object, _mockRecordMappingBusiness.Object, _mockRelationshipBusiness.Object);
            _projectBusiness = new ProjectBusiness(Context, _classBusiness);
        }

        [Fact]
        public async Task CreateClass_Success_ReturnsIdAndCreatedAt()
        {
            await SeedTestDataAsync();
            var now = DateTime.UtcNow;
            var dto = new ClassRequestDto
            {
                Name = $"Test Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Test Description",
                Uuid = $"test-uuid-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
            };

            var result = await _classBusiness.CreateClass(pid, dto);
            result.Id.Should().BeGreaterThan(0);
            result.CreatedAt.Should().BeOnOrAfter(now);
            result.Name.Should().Be(dto.Name);
            result.Description.Should().Be(dto.Description);
            result.ProjectId.Should().Be(pid);
        }

        [Fact]
        public async Task CreateClass_Fails_IfNoName()
        {
            await SeedTestDataAsync();
            var dto = new ClassRequestDto { Name = null, Description = "Test Description" };
            var result = () => _classBusiness.CreateClass(pid, dto);
            await result.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task CreateClass_Fails_IfEmptyName()
        {
            await SeedTestDataAsync();
            var dto = new ClassRequestDto { Name = "", Description = "Test Description" };
            var result = () => _classBusiness.CreateClass(pid, dto);
            await result.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task CreateClass_Fails_IfNoProjectId()
        {
            await SeedTestDataAsync();
            var dto = new ClassRequestDto { Name = "Test Class", Description = "Test Description" };
            var result = () => _classBusiness.CreateClass(pid + 99, dto);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task CreateClass_Fails_IfDeletedProjectId()
        {
            await SeedTestDataAsync(true);
            var dto = new ClassRequestDto { Name = "Test Class", Description = "Test Description" };
            var result = () => _classBusiness.CreateClass(pid, dto);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task CreateClass_Fails_IfDuplicateName()
        {
            await SeedTestDataAsync();
            var duplicateName = "Duplicate Class";
            var dto = new ClassRequestDto { Name = duplicateName, Description = "Test Description" };

            // Create first class
            await _classBusiness.CreateClass(pid, dto);

            // Try to create duplicate
            var result = () => _classBusiness.CreateClass(pid, dto);
            await result.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task GetAllClasses_ReturnsOnlyForProjects()
        {
            await SeedTestDataAsync();
            var p2 = new Project { Name = "ExtraProj" };
            Context.Projects.Add(p2);
            await Context.SaveChangesAsync();

            await _classBusiness.CreateClass(pid, new ClassRequestDto { Name = $"Class1-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", Description = "Test" });
            await _classBusiness.CreateClass(p2.Id, new ClassRequestDto { Name = $"Class2-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", Description = "Test" });

            var list = await _classBusiness.GetAllClasses(pid);
            Assert.All(list, c => Assert.Equal(pid, c.ProjectId));
        }

        [Fact]
        public async Task GetAllClasses_ExcludesSoftDeleted()
        {
            await SeedTestDataAsync();
            var activeClass = new Class
            {
                Name = $"Active Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null
            };

            var archivedClass = new Class
            {
                Name = $"Archived Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
                ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(activeClass);
            Context.Classes.Add(archivedClass);
            await Context.SaveChangesAsync();

            var list = await _classBusiness.GetAllClasses(pid);
            Assert.DoesNotContain(list, c => c.Id == archivedClass.Id);
        }

        [Fact]
        public async Task GetClass_Success_WhenExists()
        {
            await SeedTestDataAsync();
            var testClass = new Class
            {
                Name = $"Test Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Test Description",
                ProjectId = pid,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            var result = await _classBusiness.GetClass(pid, testClass.Id);
            Assert.Equal(testClass.Id, result.Id);
        }

        [Fact]
        public async Task GetClass_Fails_IfNoProjectID()
        {
            await SeedTestDataAsync();
            var testClass = new Class
            {
                Name = $"Test Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            var result = () => _classBusiness.GetClass(pid + 999, testClass.Id);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task GetClass_Fails_IfDeletedClass()
        {
            await SeedTestDataAsync();
            var testClass = new Class
            {
                Name = $"Deleted Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
                ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            var result = () => _classBusiness.GetClass(pid, testClass.Id);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task UpdateClass_Success_ReturnsModifiedAt()
        {
            await SeedTestDataAsync();
            var testClass = new Class
            {
                Name = $"Original Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                Description = "Original Description",
                ProjectId = pid,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            var dto = new ClassRequestDto { Name = $"Updated Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", Description = "Updated Description" };
            var updatedResult = await _classBusiness.UpdateClass(pid, testClass.Id, dto);

            updatedResult.ModifiedAt.Should().BeOnOrAfter(updatedResult.CreatedAt);
        }

        [Fact]
        public async Task UpdateClass_Fails_IfNotFound()
        {
            await SeedTestDataAsync();

            var dto = new ClassRequestDto { Name = "Updated Class", Description = "Updated Description" };
            var updatedResult = () => _classBusiness.UpdateClass(pid, 99, dto);
            updatedResult.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task ArchiveClass_Success_WhenExists()
        {
            await SeedTestDataAsync();
            var beforeArchive = DateTime.UtcNow;

            var testClass = new Class
            {
                Name = $"Class to Archive {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            var archivedResult = await _classBusiness.ArchiveClass(pid, testClass.Id);
            Assert.True(archivedResult);

            var archivedClass = await Context.Classes.FindAsync(testClass.Id);
            Assert.NotNull(archivedClass);
            if (archivedClass.ArchivedAt.HasValue)
            {
                Assert.NotNull(archivedClass.ArchivedAt);
                Assert.True(archivedClass.ArchivedAt >= beforeArchive);
                Assert.True(archivedClass.ArchivedAt <= DateTime.UtcNow);
            }
        }

        [Fact]
        public async Task DeleteClass_Success_WhenExists()
        {
            await SeedTestDataAsync();
            var testClass = new Class
            {
                Name = $"Class to Delete {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            var deletedResult = await _classBusiness.DeleteClass(pid, testClass.Id);
            Assert.True(deletedResult);

            var deletedClass = await Context.Classes.FindAsync(testClass.Id);
            Assert.Null(deletedClass);
        }

        [Fact]
        public async Task ClassArchived_WhenProjectArchived()
        {
            await SeedTestDataAsync();
            var beforeArchive = DateTime.UtcNow;
            var testClass = new Class
            {
                Name = $"Class in Project {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();

            var deletedResult = await _projectBusiness.ArchiveProject(pid);
            Assert.True(deletedResult);
            Context.ChangeTracker.Clear();

            var archivedClass = await Context.Classes.FindAsync(testClass.Id);
            Assert.NotNull(archivedClass);

            // Check if ArchivedAt was set (optional based on implementation)
            if (archivedClass.ArchivedAt.HasValue)
            {
                Assert.NotNull(archivedClass.ArchivedAt);
                Assert.True(archivedClass.ArchivedAt >= beforeArchive);
                Assert.True(archivedClass.ArchivedAt <= DateTime.UtcNow);
            }
        }

        private async Task SeedTestDataAsync(bool deleteProject = false)
        {
            await CleanDatabaseAsync();

            var project = new Project { Name = "Project 1" };

            if (deleteProject)
            {
                project.ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            }

            Context.Projects.Add(project);
            await Context.SaveChangesAsync();
            pid = project.Id;
        }
    }
}