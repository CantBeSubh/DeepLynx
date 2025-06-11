using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
// Alias to disambiguate EF model from C# keyword
using ClassEntity = deeplynx.datalayer.Models.Class;

namespace deeplynx.tests;

public sealed class ClassTests : IAsyncLifetime
{
    private DeeplynxContext _context;
    private ClassBusiness _business;
    private readonly PostgreSqlContainer _postgresContainer;
    public EdgeMappingBusiness _edgeMappingBusiness;
    public RelationshipBusiness _relationshipBusiness;
    public RecordMappingBusiness _recordMappingBusiness;
    public EdgeBusiness _edgeBusiness;
    public RecordBusiness _recordBusiness;


    public ClassTests()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .Build();
    }

    public async Task InitializeAsync()
    {
        _edgeMappingBusiness = new EdgeMappingBusiness(_context);
        _relationshipBusiness = new RelationshipBusiness(_context);
        _recordMappingBusiness = new RecordMappingBusiness(_context);
        _edgeBusiness = new EdgeBusiness(_context);
        _recordBusiness = new RecordBusiness(_context, _edgeBusiness);
        await _postgresContainer.StartAsync();

        var options = new DbContextOptionsBuilder<DeeplynxContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        _context = new DeeplynxContext(options);
        await _context.Database.MigrateAsync();

        _business = new ClassBusiness(_context, _edgeMappingBusiness, _recordBusiness,  _recordMappingBusiness, _relationshipBusiness);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }

    private async Task<long> SeedProject(bool deleted = false)
    {
        var project = new Project { Name = "Proj", Abbreviation = "P" };
        if (deleted)
            project.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return project.Id;
    }

    [Fact]
    public async Task CreateClass_Success_ReturnsIdAndCreatedAt()
    {
        var pid = await SeedProject();
        var dto = new ClassRequestDto { Name = "C1", Description = "D1", Uuid = "U1" };

        var result = await _business.CreateClass(pid, dto);

        Assert.True(result.Id > 0);
        Assert.Equal("C1", result.Name);
        Assert.Equal("D1", result.Description);
        Assert.Equal("U1", result.Uuid);
        Assert.Equal(pid, result.ProjectId);
        Assert.NotNull(result.CreatedAt);
    }

    [Fact]
    public async Task CreateClass_Fails_IfNoName()
    {
        var pid = await SeedProject();
        var missingNameDto = new ClassRequestDto { Name = null, Description = "D", Uuid = "U" };

        await Assert.ThrowsAsync<DbUpdateException>(
            () => _business.CreateClass(pid, missingNameDto));
    }

    [Fact]
    public async Task CreateClass_Succeeds_IfNoDescriptionOrUuid()
    {
        var pid = await SeedProject();
        var dto = new ClassRequestDto { Name = "C" };

        var result = await _business.CreateClass(pid, dto);

        Assert.True(result.Id > 0);
        Assert.Null(result.Description);
        Assert.Null(result.Uuid);
    }

    [Fact]
    public async Task CreateClass_Fails_IfProjectNotFound()
    {
        var pid = await SeedProject();
        var missing = pid + 999;
        var dto = new ClassRequestDto { Name = "C" };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _business.CreateClass(missing, dto));
    }

    [Fact]
    public async Task CreateClass_Fails_IfProjectDeleted()
    {
        var pid = await SeedProject(deleted: true);
        var dto = new ClassRequestDto { Name = "C" };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _business.CreateClass(pid, dto));
    }

    [Fact]
    public async Task GetAllClasses_ReturnsOnlyForProject()
    {
        var p1 = await SeedProject();
        var p2 = await SeedProject();

        await _business.CreateClass(p1, new ClassRequestDto { Name = "C1" });
        await _business.CreateClass(p2, new ClassRequestDto { Name = "C2" });

        var list = await _business.GetAllClasses(p1);
        Assert.Single(list);
        Assert.Equal("C1", list.First().Name);
    }

    [Fact]
    public async Task GetAllClasses_ExcludesSoftDeleted()
    {
        var pid = await SeedProject();
        await _business.CreateClass(pid, new ClassRequestDto { Name = "C1" });

        var deleted = new ClassEntity
        {
            Name = "C2",
            ProjectId = pid,
            DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        _context.Classes.Add(deleted);
        await _context.SaveChangesAsync();

        var list = await _business.GetAllClasses(pid);
        Assert.Single(list);
        Assert.DoesNotContain(list, c => c.Id == deleted.Id);
    }

    [Fact]
    public async Task GetClass_Success_WhenExists()
    {
        var pid = await SeedProject();
        var created = await _business.CreateClass(pid, new ClassRequestDto { Name = "C" });

        var result = await _business.GetClass(pid, created.Id);
        Assert.Equal(created.Id, result.Id);
    }

    [Fact]
    public async Task GetClass_Fails_IfNotFound()
    {
        var pid = await SeedProject();
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _business.GetClass(pid, 9999));
    }

    [Fact]
    public async Task GetClass_Fails_IfDeleted()
    {
        var pid = await SeedProject();
        var created = await _business.CreateClass(pid, new ClassRequestDto { Name = "C" });
        await _business.DeleteClass(pid, created.Id);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _business.GetClass(pid, created.Id));
    }

    [Fact]
    public async Task UpdateClass_Success_ReturnsModifiedAt()
    {
        var pid = await SeedProject();
        var created = await _business.CreateClass(pid, new ClassRequestDto { Name = "Old" });

        var updated = await _business.UpdateClass(pid, created.Id, new ClassRequestDto { Name = "New" });
        Assert.Equal("New", updated.Name);
        Assert.NotNull(updated.ModifiedAt);
    }

    [Fact]
    public async Task UpdateClass_Fails_IfNotFound()
    {
        var pid = await SeedProject();
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _business.UpdateClass(pid, 9999, new ClassRequestDto { Name = "X" }));
    }

    [Fact]
    public async Task UpdateClass_Fails_IfDeleted()
    {
        var pid = await SeedProject();
        var created = await _business.CreateClass(pid, new ClassRequestDto { Name = "C" });
        await _business.DeleteClass(pid, created.Id);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _business.UpdateClass(pid, created.Id, new ClassRequestDto { Name = "Y" }));
    }

    [Fact]
    public async Task DeleteClass_SoftDelete_SetsDeletedAt()
    {
        var pid = await SeedProject();
        var created = await _business.CreateClass(pid, new ClassRequestDto { Name = "C" });

        var result = await _business.DeleteClass(pid, created.Id);
        Assert.True(result);

        var entity = await _context.Classes.FindAsync(created.Id);
        Assert.NotNull(entity.DeletedAt);
    }

    [Fact(Skip = "Force delete not implemented yet")]
    public async Task ForceDeleteClass_RemovesFromDatabase()
    {
        //  future force delete logic
    }

    [Fact(Skip = "Cascade delete not implemented yet")]
    public async Task DeleteClass_DeletesDownstreamRelationships()
    {
        // Placeholder for cascading relationship deletion
    }

    [Fact(Skip = "Cascade delete not implemented yet")]
    public async Task DeleteClass_DeletesDownstreamRecords()
    {
        // Placeholder for cascading record deletion
    }

    [Fact(Skip = "Cascade delete not implemented yet")]
    public async Task DeleteClass_DeletesDownstreamRecordMappings()
    {
        // Placeholder for cascading record mapping deletion
    }
}