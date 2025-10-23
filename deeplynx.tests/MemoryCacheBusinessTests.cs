using deeplynx.business;
using deeplynx.models;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class MemoryCacheBusinessTests : IntegrationTestBase
    {
        public MemoryCacheBusinessTests(TestSuiteFixture fixture) : base(fixture)
        {
        }

        // Override InitializeAsync to set up Memory cache before each test
        public override async Task InitializeAsync()
        {
            // Set the cache provider type to memory (or null/empty for default)
            Environment.SetEnvironmentVariable("CACHE_PROVIDER_TYPE", "memory");
            
            // Reset the cache instance to pick up the new environment variable
            _cacheBusiness.ResetCacheInstance();
            
            await base.InitializeAsync();
        }

        [Fact]
        public async Task SetAndGetCache_Success()
        {
            // Arrange
            var key = "projects";
            var value = new List<ProjectResponseDto>
            {
                new ProjectResponseDto { Id = 1, Name = "Project 1", IsArchived = false },
                new ProjectResponseDto { Id = 2, Name = "Project 2", IsArchived = true }
            };

            // Act - use _cacheBusiness instead of _memoryCacheBusiness
            await _cacheBusiness.SetAsync(key, value, (TimeSpan?)null);
            var cachedValue = await _cacheBusiness.GetAsync<List<ProjectResponseDto>>(key);
            var type = await _cacheBusiness.GetAsync<string>("type");
            Console.WriteLine(type);

            // Assert
            Assert.Equivalent(value, cachedValue);
        }

        [Fact]
        public async Task DeleteCache_Success()
        {
            // Arrange
            var key = "projects";
            var value = new List<ProjectResponseDto>
            {
                new ProjectResponseDto { Id = 1, Name = "Project 1", IsArchived = false },
                new ProjectResponseDto { Id = 2, Name = "Project 2", IsArchived = true }
            };

            await _cacheBusiness.SetAsync(key, value, (TimeSpan?)null);

            // Act
            await _cacheBusiness.DeleteAsync(key);
            var cachedValue = await _cacheBusiness.GetAsync<List<ProjectResponseDto>>(key);

            // Assert
            Assert.Null(cachedValue);
        }

        [Fact]
        public async Task FlushCache_Success()
        {
            // Arrange
            var key1 = "projects-key1";
            var key2 = "projects-key2";
            var value = new List<ProjectResponseDto>
            {
                new ProjectResponseDto { Id = 1, Name = "Project 1", IsArchived = false },
                new ProjectResponseDto { Id = 2, Name = "Project 2", IsArchived = true }
            };

            await _cacheBusiness.SetAsync(key1, value, (TimeSpan?)null);
            await _cacheBusiness.SetAsync(key2, value, (TimeSpan?)null);

            // Act
            await _cacheBusiness.FlushAsync();
            var cachedValue1 = await _cacheBusiness.GetAsync<List<ProjectResponseDto>>(key1);
            var cachedValue2 = await _cacheBusiness.GetAsync<List<ProjectResponseDto>>(key2);

            // Assert
            Assert.Null(cachedValue1);
            Assert.Null(cachedValue2);
        }
    }
}