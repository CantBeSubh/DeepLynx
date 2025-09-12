using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Record = deeplynx.datalayer.Models.Record;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class CacheBusinessTests : IntegrationTestBase
    {
        private CacheBusiness _cacheBusiness;
        private Mock<ICacheBusiness> _mockCacheService;
        private EventBusiness _eventBusiness;
        private ProjectBusiness _projectBusiness = null!;
        private DataSourceBusiness _dataSourceBusiness = null!;
        private ClassBusiness _classBusiness = null!;
        private Mock<IEdgeBusiness> _mockEdgeBusiness = null!;
        private Mock<IRecordBusiness> _mockRecordBusiness = null!;
        private Mock<IRelationshipBusiness> _mockRelationshipBusiness = null!;
        private Mock<ILogger<ProjectBusiness>> _mockLogger = null!;
        private Mock<IObjectStorageBusiness> _objectStorageBusiness = null!;

        public long TestProject1Id;
        public long TestProject2Id;
        public long TestClassId;
        public long TestDataSourceId;

        public CacheBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _eventBusiness = new EventBusiness(Context);
            _objectStorageBusiness = new Mock<IObjectStorageBusiness>();
            _mockRecordBusiness = new Mock<IRecordBusiness>();
            _mockRelationshipBusiness = new Mock<IRelationshipBusiness>();
            _mockEdgeBusiness = new Mock<IEdgeBusiness>();
            _mockLogger = new Mock<ILogger<ProjectBusiness>>();
            _dataSourceBusiness = new DataSourceBusiness(Context, _mockEdgeBusiness.Object, _mockRecordBusiness.Object, _eventBusiness);
            _classBusiness = new ClassBusiness(
                Context, _mockRecordBusiness.Object, 
                _mockRelationshipBusiness.Object, _eventBusiness);
            _projectBusiness = new ProjectBusiness(Context, _mockLogger.Object, _classBusiness, _dataSourceBusiness, _objectStorageBusiness.Object, _eventBusiness);
            _mockCacheService = new Mock<ICacheBusiness>();
            _cacheBusiness = CacheBusiness.Instance;
            _cacheBusiness.SetCacheService(_mockCacheService.Object);
        }

        #region RedisCacheImpl Tests

        [Fact]
        public async Task RedisCacheImpl_TestSetAndGetCache()
        {
            // Arrange
            Environment.SetEnvironmentVariable("CACHE_PROVIDER_TYPE", "redis");
            var key = "projects";
            var value = Context.Projects.Select(project => new ProjectResponseDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Abbreviation = project.Abbreviation,
                LastUpdatedAt = project.LastUpdatedAt,
                LastUpdatedBy = project.LastUpdatedBy,
                IsArchived = project.IsArchived
            }).ToList();

            _mockCacheService.Setup(s => s.Set(key, value, It.IsAny<TimeSpan?>())).Returns(Task.FromResult(true));
            _mockCacheService.Setup(s => s.Get<List<ProjectResponseDto>>(key)).ReturnsAsync(value);

            // Act
            await _cacheBusiness.Set(key, value, (TimeSpan?)null);
            var cachedValue = await _cacheBusiness.Get<List<ProjectResponseDto>>(key);

            // Assert
            value.Should().BeEquivalentTo(cachedValue);
        }

        [Fact]
        public async Task RedisCacheImpl_TestDeleteCache()
        {
            // Arrange
            Environment.SetEnvironmentVariable("CACHE_PROVIDER_TYPE", "redis");
            var key = "projects";
            var value = Context.Projects.Select(project => new ProjectResponseDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Abbreviation = project.Abbreviation,
                LastUpdatedAt = project.LastUpdatedAt,
                LastUpdatedBy = project.LastUpdatedBy,
                IsArchived = project.IsArchived
            }).ToList();
            await _cacheBusiness.Set(key, value, (TimeSpan?)null);

            _mockCacheService.Setup(s => s.Set(key, value, It.IsAny<TimeSpan?>())).Returns(Task.FromResult(true));
            _mockCacheService.Setup(s => s.Delete(key)).Returns(Task.FromResult(true));
            _mockCacheService.Setup(s => s.Get<string>(key)).ReturnsAsync((string)null);

            // Act
            await _cacheBusiness.Set(key, value, (TimeSpan?)null);
            await _cacheBusiness.Delete(key);
            var cachedValue = await _cacheBusiness.Get<string>(key);

            // Assert
            Assert.Null(cachedValue);
        }

        [Fact]
        public async Task RedisCacheImpl_TestFlushCache()
        {
            // Arrange
            Environment.SetEnvironmentVariable("CACHE_PROVIDER_TYPE", "redis");
            var key1 = "projects-key1";
            var key2 = "projects-key2";
            var value = Context.Projects.Select(project => new ProjectResponseDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Abbreviation = project.Abbreviation,
                LastUpdatedAt = project.LastUpdatedAt,
                LastUpdatedBy = project.LastUpdatedBy,
                IsArchived = project.IsArchived
            }).ToList();
            await _cacheBusiness.Set(key1, value, (TimeSpan?)null);
            await _cacheBusiness.Set(key2, value, (TimeSpan?)null);

            _mockCacheService.Setup(s => s.Set(key1, value, It.IsAny<TimeSpan?>())).Returns(Task.FromResult(true));
            _mockCacheService.Setup(s => s.Set(key2, value, It.IsAny<TimeSpan?>())).Returns(Task.FromResult(true));
            _mockCacheService.Setup(s => s.Flush()).Returns(Task.FromResult(true));
            _mockCacheService.Setup(s => s.Get<string>(key1)).ReturnsAsync((string)null);
            _mockCacheService.Setup(s => s.Get<string>(key2)).ReturnsAsync((string)null);

            // Act
            await _cacheBusiness.Set(key1, value, (TimeSpan?)null);
            await _cacheBusiness.Set(key2, value, (TimeSpan?)null);
            await _cacheBusiness.Flush();
            var cachedValue1 = await _cacheBusiness.Get<string>(key1);
            var cachedValue2 = await _cacheBusiness.Get<string>(key2);

            // Assert
            Assert.Null(cachedValue1);
            Assert.Null(cachedValue2);
        }
        
        #endregion

        #region MemoryCacheImpl Tests
        
        [Fact]
        public async Task MemoryCacheImpl_TestSetAndGetCache()
        {
            // Arrange
            Environment.SetEnvironmentVariable("CACHE_PROVIDER_TYPE", "memory");
            var key = "projects";
            var value = Context.Projects.Select(project => new ProjectResponseDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Abbreviation = project.Abbreviation,
                LastUpdatedAt = project.LastUpdatedAt,
                LastUpdatedBy = project.LastUpdatedBy,
                IsArchived = project.IsArchived
            }).ToList();
        
            _mockCacheService.Setup(s => s.Set(key, value, It.IsAny<TimeSpan?>())).Returns(Task.FromResult(true));
            _mockCacheService.Setup(s => s.Get<List<ProjectResponseDto>>(key)).ReturnsAsync(value);
        
            // Act
            await _cacheBusiness.Set(key, value, (TimeSpan?)null);
            var cachedValue = await _cacheBusiness.Get<List<ProjectResponseDto>>(key);
        
            // Assert
            value.Should().BeEquivalentTo(cachedValue);
        }
        
        [Fact]
        public async Task MemoryCacheImpl_TestDeleteCache()
        {
            // Arrange
            Environment.SetEnvironmentVariable("CACHE_PROVIDER_TYPE", "memory");
            var key = "projects";
            var value = Context.Projects.Select(project => new ProjectResponseDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Abbreviation = project.Abbreviation, 
                LastUpdatedAt = project.LastUpdatedAt,
                LastUpdatedBy = project.LastUpdatedBy,
                IsArchived = project.IsArchived
            }).ToList();
            await _cacheBusiness.Set(key, value, (TimeSpan?)null);
        
            _mockCacheService.Setup(s => s.Set(key, value, It.IsAny<TimeSpan?>())).Returns(Task.FromResult(true));
            _mockCacheService.Setup(s => s.Delete(key)).Returns(Task.FromResult(true));
            _mockCacheService.Setup(s => s.Get<string>(key)).ReturnsAsync((string)null);
        
            // Act
            await _cacheBusiness.Set(key, value, (TimeSpan?)null);
            await _cacheBusiness.Delete(key);
            var cachedValue = await _cacheBusiness.Get<string>(key);
        
            // Assert
            Assert.Null(cachedValue);
        }
        
        [Fact]
        public async Task MemoryCacheImpl_TestFlushCache()
        {
            // Arrange
            Environment.SetEnvironmentVariable("CACHE_PROVIDER_TYPE", "memory");
            var key1 = "projects-key1";
            var key2 = "projects-key2";
            var value = Context.Projects.Select(project => new ProjectResponseDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Abbreviation = project.Abbreviation,
                LastUpdatedAt = project.LastUpdatedAt,
                LastUpdatedBy = project.LastUpdatedBy,
                IsArchived = project.IsArchived
            }).ToList();
            await _cacheBusiness.Set(key1, value, (TimeSpan?)null);
            await _cacheBusiness.Set(key2, value, (TimeSpan?)null);
        
            _mockCacheService.Setup(s => s.Set(key1, value, It.IsAny<TimeSpan?>())).Returns(Task.FromResult(true));
            _mockCacheService.Setup(s => s.Set(key2, value, It.IsAny<TimeSpan?>())).Returns(Task.FromResult(true));
            _mockCacheService.Setup(s => s.Flush()).Returns(Task.FromResult(true));
            _mockCacheService.Setup(s => s.Get<string>(key1)).ReturnsAsync((string)null);
            _mockCacheService.Setup(s => s.Get<string>(key2)).ReturnsAsync((string)null);
        
            // Act
            await _cacheBusiness.Set(key1, value, (TimeSpan?)null);
            await _cacheBusiness.Set(key2, value, (TimeSpan?)null);
            await _cacheBusiness.Flush();
            var cachedValue1 = await _cacheBusiness.Get<string>(key1);
            var cachedValue2 = await _cacheBusiness.Get<string>(key2);
        
            // Assert
            Assert.Null(cachedValue1);
            Assert.Null(cachedValue2);
        }
        
        #endregion
        protected override async Task SeedTestDataAsync()
        {
            await base.SeedTestDataAsync();

            var testProjects = new List<Project>
            {
                new Project
                {
                    Name = "Test Project #1",
                    Description = "Test project #1 for unit tests",
                    Abbreviation = "TST",
                    LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                }, 
                new Project
                {
                    Name = "Test Project #2",
                    Description = "Test project #2 for unit tests",
                    Abbreviation = "TST",
                    LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                }, 
            };
            
            Context.Projects.AddRange(testProjects);
            await Context.SaveChangesAsync();
            TestProject1Id = testProjects[0].Id;
            TestProject2Id = testProjects[1].Id;
            
            var testClass = new Class
            {
                Name = "Test Class",
                ProjectId = TestProject1Id,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(testClass);
            await Context.SaveChangesAsync();
            TestClassId = testClass.Id;
            
            var testDataSource = new DataSource
            {
                Name = "Test DataSource",
                ProjectId = TestProject1Id,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
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
                Description = "Test record for unit tests"
            };
            Context.Records.Add(testRecord);
            await Context.SaveChangesAsync();
        }
    }
}
