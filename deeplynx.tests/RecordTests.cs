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

public sealed class RecordTests : IClassFixture<RecordContainerFixture>
{
    private readonly RecordContainerFixture _fixture;

    public RecordTests(RecordContainerFixture fixture)
    {
        _fixture = fixture;
    }
    

    [Fact]
    public async Task CreateRecord_Success_ReturnsIdAndCreatedAt()
    {
        var (pid, dsid) = await _fixture.SeedProjectAndDataSource();
        var dto = new RecordRequestDto
        {
            Uri = "uri",
            Name = "Test",
            Properties = new JsonObject { ["a"] = "b" },
            ClassId = null
        };

        var result = await _fixture.RecordBusiness.CreateRecord(pid, dsid, dto);

        Assert.True(result.Id > 0);
        Assert.Equal(pid, result.ProjectId);
        Assert.Equal(dsid, result.DataSourceId);
        Assert.NotNull(result.CreatedAt);
    }

    [Fact]
    public async Task CreateRecord_Fails_IfNoProperties()
    {
        var (pid, dsid) = await _fixture.SeedProjectAndDataSource();
        var dto = new RecordRequestDto { Properties = null };

        await Assert.ThrowsAsync<NullReferenceException>(
            () => _fixture.RecordBusiness.CreateRecord(pid, dsid, dto));
    }

    [Fact]
    public async Task CreateRecord_Fails_IfProjectNotFound()
    {
        var (pid, dsid) = await _fixture.SeedProjectAndDataSource();
        var missingPid = pid + 999; //create an ID that is not on the DB
        var dto = new RecordRequestDto
        {
            Properties = new JsonObject(),
            Name = "X"
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _fixture.RecordBusiness.CreateRecord(missingPid, dsid, dto));
    }

    [Fact]
    public async Task CreateRecord_Fails_IfProjectDeleted()
    {
        var (pid, dsid) = await _fixture.SeedProjectAndDataSource(deletedProject: true);
        var dto = new RecordRequestDto
        {
            Properties = new JsonObject(),
            Name = "X"
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _fixture.RecordBusiness.CreateRecord(pid, dsid, dto));
    }

    [Fact]
    public async Task CreateRecord_Fails_IfDataSourceNotFound()
    {
        var (pid, dsid) = await _fixture.SeedProjectAndDataSource();
        var missingDsid = dsid + 999;  //create an ID that is not on the DB
        var dto = new RecordRequestDto
        {
            Properties = new JsonObject(),
            Name = "X"
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _fixture.RecordBusiness.CreateRecord(pid, missingDsid, dto));
    }

    [Fact]
    public async Task CreateRecord_Fails_IfDataSourceDeleted()
    {
        var (pid, dsid) = await _fixture.SeedProjectAndDataSource(deletedDataSource: true);
        var dto = new RecordRequestDto
        {
            Properties = new JsonObject(),
            Name = "X"
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _fixture.RecordBusiness.CreateRecord(pid, dsid, dto));
    }

    [Fact]
    public async Task CreateRecord_Returns_NullClass_WhenNoneProvided()
    {
        var (pid, dsid) = await _fixture.SeedProjectAndDataSource();
        var dto = new RecordRequestDto
        {
            Properties = new JsonObject(),
            Name = "TC"
        };

        var result = await _fixture.RecordBusiness.CreateRecord(pid, dsid, dto);
        Assert.Null(result.ClassId);
    }

    [Fact]
    public async Task GetAllRecords_ReturnsOnlyForGivenProject()
    {
        var (p1, d1) = await _fixture.SeedProjectAndDataSource();
        var (p2, d2) = await _fixture.SeedProjectAndDataSource();

        await _fixture.RecordBusiness.CreateRecord(p1, d1, new RecordRequestDto { Properties = new JsonObject() });
        await _fixture.RecordBusiness.CreateRecord(p2, d2, new RecordRequestDto { Properties = new JsonObject() });

        var records = await _fixture.RecordBusiness.GetAllRecords(p1);
        Assert.Single(records);
    }

    [Fact]
    public async Task GetAllRecords_ExcludesSoftDeleted()
    {
        var (pid, dsid) = await _fixture.SeedProjectAndDataSource();
        await _fixture.RecordBusiness.CreateRecord(pid, dsid, new RecordRequestDto { Properties = new JsonObject(), Name = "V" });

        var deleted = new RecordEntity
        {
            Name = "H",
            Properties = "{}",
            ProjectId = pid,
            DataSourceId = dsid,
            ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        _fixture.Context.Records.Add(deleted);
        await _fixture.Context.SaveChangesAsync();

        var records = await _fixture.RecordBusiness.GetAllRecords(pid);
        Assert.Single(records);
        Assert.DoesNotContain(records, r => r.Id == deleted.Id);
    }

    [Fact]
    public async Task GetRecord_Success_WhenExists()
    {
        var (pid, dsid) = await _fixture.SeedProjectAndDataSource();
        var cr = await _fixture.RecordBusiness.CreateRecord(pid, dsid, new RecordRequestDto { Properties = new JsonObject() });

        var fetched = await _fixture.RecordBusiness.GetRecord(pid, cr.Id);
        Assert.Equal(cr.Id, fetched.Id);
    }

    [Fact]
    public async Task GetRecord_Fails_IfNotFound()
    {
        var (pid, dsid) = await _fixture.SeedProjectAndDataSource();
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _fixture.RecordBusiness.GetRecord(pid, 9999));
    }

    [Fact]
    public async Task GetRecord_Fails_IfDeleted()
    {
        var (pid, dsid) = await _fixture.SeedProjectAndDataSource();
        var cr = await _fixture.RecordBusiness.CreateRecord(pid, dsid, new RecordRequestDto { Properties = new JsonObject() });
        await _fixture.RecordBusiness.DeleteRecord(pid, cr.Id);
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _fixture.RecordBusiness.GetRecord(pid, cr.Id));
    }

    [Fact]
    public async Task UpdateRecord_Success_ModifiesAndReturns()
    {
        var (pid, dsid) = await _fixture.SeedProjectAndDataSource();
        var cr = await _fixture.RecordBusiness.CreateRecord(pid, dsid, new RecordRequestDto { Properties = new JsonObject(), Name = "Old" });

        var updated = await _fixture.RecordBusiness.UpdateRecord(pid, cr.Id, new RecordRequestDto { Properties = new JsonObject { ["x"] = "y" }, Name = "New" });
        Assert.Equal("New", updated.Name);
        Assert.NotNull(updated.ModifiedAt);
    }

    [Fact]
    public async Task UpdateRecord_Fails_IfNotFound()
    {
        var (pid, dsid) = await _fixture.SeedProjectAndDataSource();
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _fixture.RecordBusiness.UpdateRecord(pid, 9999, new RecordRequestDto { Properties = new JsonObject() }));
    }

    [Fact]
    public async Task UpdateRecord_Fails_IfDeleted()
    {
        var (pid, dsid) = await _fixture.SeedProjectAndDataSource();
        var cr = await _fixture.RecordBusiness.CreateRecord(pid, dsid, new RecordRequestDto { Properties = new JsonObject() });
        await _fixture.RecordBusiness.DeleteRecord(pid, cr.Id);
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _fixture.RecordBusiness.UpdateRecord(pid, cr.Id, new RecordRequestDto { Properties = new JsonObject() }));
    }
    [Fact]
    public async Task DeleteRecord_SoftDelete_SetsArchivedAt()
    {
        var (pid, dsid) = await _fixture.SeedProjectAndDataSource();
        var created = await _fixture.RecordBusiness.CreateRecord(pid, dsid, new RecordRequestDto { Properties = new JsonObject(), Name = "ToDelete" });

        var result = await _fixture.RecordBusiness.DeleteRecord(pid, created.Id);

        Assert.True(result);
        var entity = await _fixture.Context.Records.FindAsync(created.Id);
        Assert.NotNull(entity);
        Assert.NotNull(entity.ArchivedAt);
    }
}