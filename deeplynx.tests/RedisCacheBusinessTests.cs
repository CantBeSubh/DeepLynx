using deeplynx.business;
using deeplynx.models;
using FluentAssertions;
using Moq;

namespace deeplynx.tests;

[Collection("Test Suite Collection")]
public class RedisCacheBusinessTests : IntegrationTestBase
{
    public RedisCacheBusinessTests(TestSuiteFixture fixture) : base(fixture) { }
    
    [Fact]
    public async Task RedisCacheImpl_TestSetAsyncAndGetCache()
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

            // Act
            await _cacheBusiness.SetAsync(key, value, (TimeSpan?)null);
            var cachedValue = await _cacheBusiness.GetAsync<List<ProjectResponseDto>>(key);

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
            await _cacheBusiness.SetAsync(key, value, (TimeSpan?)null);

            // Act
            await _cacheBusiness.SetAsync(key, value, (TimeSpan?)null);
            await _cacheBusiness.DeleteAsync(key);
            var cachedValue = await _cacheBusiness.GetAsync<string>(key);

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
            await _cacheBusiness.SetAsync(key1, value, (TimeSpan?)null);
            await _cacheBusiness.SetAsync(key2, value, (TimeSpan?)null);

            // Act
            await _cacheBusiness.SetAsync(key1, value, (TimeSpan?)null);
            await _cacheBusiness.SetAsync(key2, value, (TimeSpan?)null);
            await _cacheBusiness.FlushAsync();
            var cachedValue1 = await _cacheBusiness.GetAsync<string>(key1);
            var cachedValue2 = await _cacheBusiness.GetAsync<string>(key2);

            // Assert
            Assert.Null(cachedValue1);
            Assert.Null(cachedValue2);
        }
}