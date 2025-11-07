using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class MemoryCacheBusinessTests : IntegrationTestBase
    {
        private long organizationId;
        
        public MemoryCacheBusinessTests(TestSuiteFixture fixture) : base(fixture)
        {
        }
        
        public override async Task InitializeAsync()
        {
            // Environment.SetEnvironmentVariable("CACHE_PROVIDER_TYPE", "memory");
            SwitchCacheType("memory");
            await base.InitializeAsync();
        }
        
        [Fact]
        public async Task ConfirmTestingCorrectCacheType()
        {
            var type = _cacheBusiness.CacheType;
            Assert.True(type == "memory");
        }


        [Fact]
        public async Task SetAndGetCache_Success()
        {
            // Arrange
            var key = "projects";
            var value = new List<ProjectResponseDto>
            {
                new ProjectResponseDto { Id = 1, Name = "Project 1", IsArchived = false, OrganizationId = organizationId },
                new ProjectResponseDto { Id = 2, Name = "Project 2", IsArchived = true, OrganizationId = organizationId }
            };

            // Act
            await _cacheBusiness.SetAsync(key, value, (TimeSpan?)null);
            var cachedValue = await _cacheBusiness.GetAsync<List<ProjectResponseDto>>(key);

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
                new ProjectResponseDto { Id = 1, Name = "Project 1", IsArchived = false, OrganizationId = organizationId },
                new ProjectResponseDto { Id = 2, Name = "Project 2", IsArchived = true, OrganizationId = organizationId }
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
                new ProjectResponseDto { Id = 1, Name = "Project 1", IsArchived = false, OrganizationId = organizationId },
                new ProjectResponseDto { Id = 2, Name = "Project 2", IsArchived = true, OrganizationId = organizationId }
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

        protected override async Task SeedTestDataAsync()
        {
            await base.SeedTestDataAsync();
            var organization = new Organization { Name = "Test Organization" };
            Context.Organizations.Add(organization);
            await Context.SaveChangesAsync();
            organizationId = organization.Id;
        }
    }
}