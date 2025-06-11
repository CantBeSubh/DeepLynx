using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.models;
// Alias to disambiguate EF model from C# keyword
using RecordEntity = deeplynx.datalayer.Models.Record;

namespace deeplynx.tests;

public sealed class RecordTests : IAsyncLifetime
{
    private DeeplynxContext _context;
    private RecordBusiness _business;
    private readonly PostgreSqlContainer _postgresContainer;
    public EdgeBusiness _edgeBusiness;


    public RecordTests()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        var options = new DbContextOptionsBuilder<DeeplynxContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        _context = new DeeplynxContext(options);
        
        await _context.Database.MigrateAsync();
        _edgeBusiness = new EdgeBusiness(_context);
        _business = new RecordBusiness(_context, _edgeBusiness);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }

    private async Task<(long ProjectId, long DataSourceId)> SeedProjectAndDataSource(
        bool deletedProject = false,
        bool deletedDataSource = false)
    {
        var project = new Project { Name = "Proj", Abbreviation = "P" };
        if (deletedProject)
            project.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        _context.Projects.Add(project);

        var dataSource = new DataSource { Name = "DS", Project = project };
        if (deletedDataSource)
            dataSource.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        _context.DataSources.Add(dataSource);
        await _context.SaveChangesAsync();
        return (project.Id, dataSource.Id);
    }

    [Fact]
    public async Task CreateRecord_Success_ReturnsIdAndCreatedAt()
    {
        var (pid, dsid) = await SeedProjectAndDataSource();
        var dto = new RecordRequestDto
        {
            Uri = "uri",
            Name = "Test",
            Properties = new JsonObject { ["a"] = "b" },
            ClassId = null
        };

        var result = await _business.CreateRecord(pid, dsid, dto);

        Assert.True(result.Id > 0);
        Assert.Equal(pid, result.ProjectId);
        Assert.Equal(dsid, result.DataSourceId);
        Assert.NotNull(result.CreatedAt);
    }

    [Fact]
    public async Task CreateRecord_Fails_IfNoProperties()
    {
        var (pid, dsid) = await SeedProjectAndDataSource();
        var dto = new RecordRequestDto { Properties = null };

        await Assert.ThrowsAsync<NullReferenceException>(
            () => _business.CreateRecord(pid, dsid, dto));
    }

    [Fact]
    public async Task CreateRecord_Fails_IfProjectNotFound()
    {
        var (pid, dsid) = await SeedProjectAndDataSource();
        var missingPid = pid + 999; //create an ID that is not on the DB
        var dto = new RecordRequestDto
        {
            Properties = new JsonObject(),
            Name = "X"
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _business.CreateRecord(missingPid, dsid, dto));
    }

    [Fact]
    public async Task CreateRecord_Fails_IfProjectDeleted()
    {
        var (pid, dsid) = await SeedProjectAndDataSource(deletedProject: true);
        var dto = new RecordRequestDto
        {
            Properties = new JsonObject(),
            Name = "X"
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _business.CreateRecord(pid, dsid, dto));
    }

    [Fact]
    public async Task CreateRecord_Fails_IfDataSourceNotFound()
    {
        var (pid, dsid) = await SeedProjectAndDataSource();
        var missingDsid = dsid + 999;  //create an ID that is not on the DB
        var dto = new RecordRequestDto
        {
            Properties = new JsonObject(),
            Name = "X"
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _business.CreateRecord(pid, missingDsid, dto));
    }

    [Fact]
    public async Task CreateRecord_Fails_IfDataSourceDeleted()
    {
        var (pid, dsid) = await SeedProjectAndDataSource(deletedDataSource: true);
        var dto = new RecordRequestDto
        {
            Properties = new JsonObject(),
            Name = "X"
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _business.CreateRecord(pid, dsid, dto));
    }

    [Fact]
    public async Task CreateRecord_Returns_NullClass_WhenNoneProvided()
    {
        var (pid, dsid) = await SeedProjectAndDataSource();
        var dto = new RecordRequestDto
        {
            Properties = new JsonObject(),
            Name = "TC"
        };

        var result = await _business.CreateRecord(pid, dsid, dto);
        Assert.Null(result.ClassId);
    }

    [Fact]
    public async Task GetAllRecords_ReturnsOnlyForGivenProject()
    {
        var (p1, d1) = await SeedProjectAndDataSource();
        var (p2, d2) = await SeedProjectAndDataSource();

        await _business.CreateRecord(p1, d1, new RecordRequestDto { Properties = new JsonObject() });
        await _business.CreateRecord(p2, d2, new RecordRequestDto { Properties = new JsonObject() });

        var records = await _business.GetAllRecords(p1);
        Assert.Single(records);
    }

    [Fact]
    public async Task GetAllRecords_ExcludesSoftDeleted()
    {
        var (pid, dsid) = await SeedProjectAndDataSource();
        await _business.CreateRecord(pid, dsid, new RecordRequestDto { Properties = new JsonObject(), Name = "V" });

        var deleted = new RecordEntity
        {
            Name = "H",
            Properties = "{}",
            ProjectId = pid,
            DataSourceId = dsid,
            DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        _context.Records.Add(deleted);
        await _context.SaveChangesAsync();

        var records = await _business.GetAllRecords(pid);
        Assert.Single(records);
        Assert.DoesNotContain(records, r => r.Id == deleted.Id);
    }

    [Fact]
    public async Task GetRecord_Success_WhenExists()
    {
        var (pid, dsid) = await SeedProjectAndDataSource();
        var cr = await _business.CreateRecord(pid, dsid, new RecordRequestDto { Properties = new JsonObject() });

        var fetched = await _business.GetRecord(pid, cr.Id);
        Assert.Equal(cr.Id, fetched.Id);
    }

    [Fact]
    public async Task GetRecord_Fails_IfNotFound()
    {
        var (pid, dsid) = await SeedProjectAndDataSource();
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _business.GetRecord(pid, 9999));
    }

    [Fact]
    public async Task GetRecord_Fails_IfDeleted()
    {
        var (pid, dsid) = await SeedProjectAndDataSource();
        var cr = await _business.CreateRecord(pid, dsid, new RecordRequestDto { Properties = new JsonObject() });
        await _business.DeleteRecord(pid, dsid);
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _business.GetRecord(pid, cr.Id));
    }

    [Fact]
    public async Task UpdateRecord_Success_ModifiesAndReturns()
    {
        var (pid, dsid) = await SeedProjectAndDataSource();
        var cr = await _business.CreateRecord(pid, dsid, new RecordRequestDto { Properties = new JsonObject(), Name = "Old" });

        var updated = await _business.UpdateRecord(pid, cr.Id, new RecordRequestDto { Properties = new JsonObject { ["x"] = "y" }, Name = "New" });
        Assert.Equal("New", updated.Name);
        Assert.NotNull(updated.ModifiedAt);
    }

    [Fact]
    public async Task UpdateRecord_Fails_IfNotFound()
    {
        var (pid, dsid) = await SeedProjectAndDataSource();
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _business.UpdateRecord(pid, 9999, new RecordRequestDto { Properties = new JsonObject() }));
    }

    [Fact]
    public async Task UpdateRecord_Fails_IfDeleted()
    {
        var (pid, dsid) = await SeedProjectAndDataSource();
        var cr = await _business.CreateRecord(pid, dsid, new RecordRequestDto { Properties = new JsonObject() });
        await _business.DeleteRecord(pid, dsid);
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _business.UpdateRecord(pid, cr.Id, new RecordRequestDto { Properties = new JsonObject() }));
    }
    [Fact]
    public async Task DeleteRecord_SoftDelete_SetsDeletedAt()
    {
        var (pid, dsid) = await SeedProjectAndDataSource();
        var created = await _business.CreateRecord(pid, dsid, new RecordRequestDto { Properties = new JsonObject(), Name = "ToDelete" });

        var result = await _business.DeleteRecord(pid, dsid);

        Assert.True(result);
        var entity = await _context.Records.FindAsync(created.Id);
        Assert.NotNull(entity);
        Assert.NotNull(entity.DeletedAt);
    }
}