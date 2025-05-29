using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.models;
using NLog;

namespace deeplynx.tests
{
    public class RelationshipTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgresContainer;
        private DbContextOptions<DeeplynxContext> _options;

        public RelationshipTests()
        {
            _postgresContainer = new PostgreSqlBuilder()
                .WithImage("postgres:15-alpine")
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _postgresContainer.StartAsync();

            _options = new DbContextOptionsBuilder<DeeplynxContext>()
                .UseNpgsql(_postgresContainer.GetConnectionString())
                .EnableSensitiveDataLogging()
                .Options;

            await using var context = new DeeplynxContext(_options);
            await context.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            await _postgresContainer.DisposeAsync();
        }

        private async Task<(long projectId, long originId, long destinationId)> SeedProjectAndClassIds(bool deleteProject = false, bool deleteClasses = false)
        {
            await using var context = new DeeplynxContext(_options);

            var project = new Project { Name = "Test Project", Abbreviation = "TP" };
            context.Projects.Add(project);
            await context.SaveChangesAsync();

            if (deleteProject)
            {
                project.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                await context.SaveChangesAsync();
            }

            var origin = new Class { Name = "Origin Class", ProjectId = project.Id };
            var destination = new Class { Name = "Destination Class", ProjectId = project.Id };
            context.Classes.AddRange(origin, destination);
            await context.SaveChangesAsync();

            if (deleteClasses)
            {
                origin.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                destination.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                await context.SaveChangesAsync();
            }

            return (project.Id, origin.Id, destination.Id);
        }

        [Fact]
        public async Task CreateRelationship_Succeeds_WithRequiredFields()
        {
            var (projectId, originId, destId) = await SeedProjectAndClassIds();

            await using var context = new DeeplynxContext(_options);
            var business = new RelationshipBusiness(context);
            var dto = new RelationshipRequestDto { Name = "Rel1", OriginClass = originId.ToString(), DestinationClass = destId.ToString() };

            var result = await business.CreateRelationship(projectId, dto);

            Assert.NotNull(result);
            Assert.Equal("Rel1", result.Name);
            Assert.Equal(projectId, result.ProjectId);
            Assert.NotEqual(default, result.CreatedAt);
        }

        [Fact]
        public async Task CreateRelationship_Fails_IfNoName()
        {
            var (projectId, originId, destId) = await SeedProjectAndClassIds();

            await using var context = new DeeplynxContext(_options);
            var business = new RelationshipBusiness(context);
            var dto = new RelationshipRequestDto { Name = null, OriginClass = originId.ToString(), DestinationClass = destId.ToString() };

            await Assert.ThrowsAsync<DbUpdateException>(() => business.CreateRelationship(projectId, dto));
        }

        [Fact]
        public async Task CreateRelationship_Fails_IfProjectDeleted()
        {
                var (projectId, originId, destId) = await SeedProjectAndClassIds(deleteProject: true);

                await using var context = new DeeplynxContext(_options);
                var business = new RelationshipBusiness(context);
                var dto = new RelationshipRequestDto
                    { Name = "Rel1", OriginClass = originId.ToString(), DestinationClass = destId.ToString() };

                await Assert.ThrowsAsync<KeyNotFoundException>(() => business.CreateRelationship(projectId, dto));
            
           
        }

        [Fact]
        public async Task CreateRelationship_Fails_IfClassDeleted()
        {
            var (projectId, originId, destId) = await SeedProjectAndClassIds(deleteClasses: true);

            await using var context = new DeeplynxContext(_options);
            var business = new RelationshipBusiness(context);
            var dto = new RelationshipRequestDto { Name = "Rel1", OriginClass = originId.ToString(), DestinationClass = destId.ToString() };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => business.CreateRelationship(projectId, dto));
        }

        [Fact]
        public async Task GetAllRelationships_FiltersByProject()
        {
            var (projectId, originId, destId) = await SeedProjectAndClassIds();

            await using var context = new DeeplynxContext(_options);
            var business = new RelationshipBusiness(context);

            var otherProject = new Project { Name = "Other Project", Abbreviation = "OP" };
            context.Projects.Add(otherProject);
            await context.SaveChangesAsync();

            var dto = new RelationshipRequestDto { Name = "Rel1", OriginClass = originId.ToString(), DestinationClass = destId.ToString() };
            await business.CreateRelationship(projectId, dto);
            await business.CreateRelationship(otherProject.Id, dto);

            var result = await business.GetAllRelationships(projectId);
            Assert.Single(result);
            Assert.Equal("Rel1", result.First().Name);
        }

        [Fact]
        public async Task GetRelationship_Success()
        {
            var (projectId, originId, destId) = await SeedProjectAndClassIds();

            await using var context = new DeeplynxContext(_options);
            var relationship = new Relationship
            {
                Name = "Rel2",
                OriginId = originId,
                DestinationId = destId,
                ProjectId = projectId,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            context.Relationships.Add(relationship);
            await context.SaveChangesAsync();

            var business = new RelationshipBusiness(context);
            var result = await business.GetRelationship(projectId, relationship.Id);

            Assert.NotNull(result);
            Assert.Equal(relationship.Id, result.Id);
        }

        [Fact]
        public async Task DeleteRelationship_SoftDeletes()
        {
            var (projectId, originId, destId) = await SeedProjectAndClassIds();

            await using var context = new DeeplynxContext(_options);
            var relationship = new Relationship
            {
                Name = "Rel3",
                OriginId = originId,
                DestinationId = destId,
                ProjectId = projectId,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            context.Relationships.Add(relationship);
            await context.SaveChangesAsync();

            var business = new RelationshipBusiness(context);
            var result = await business.DeleteRelationship(projectId, relationship.Id);
            Assert.True(result);

            var list = await business.GetAllRelationships(projectId);
            Assert.Empty(list);
        }
    }
}