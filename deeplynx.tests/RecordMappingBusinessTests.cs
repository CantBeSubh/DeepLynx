using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.helpers.exceptions;
using deeplynx.interfaces;
using deeplynx.models;
using FluentAssertions;
using Moq;

namespace deeplynx.tests;

[Collection("Test Suite Collection")]
public class RecordMappingBusinessTests : IntegrationTestBase
{
    private RecordMappingBusiness _recordMappingBusiness = null!;
    private ProjectBusiness _projectBusiness = null!;
    private ClassBusiness _classBusiness = null!;
    private Mock<IEdgeBusiness> _edgeBusiness;
    private Mock<IEdgeMappingBusiness> _edgeMappingBusiness;
    private Mock<IRecordBusiness> _recordBusiness;
    private Mock<IRelationshipBusiness> _relationshipBusiness = null!;
    private DataSourceBusiness _dataSourceBusiness = null!;
    public long pid;
    public long tid;
    public long cid;
    public long did;
    
    public RecordMappingBusinessTests(TestSuiteFixture fixture) : base(fixture) { }
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _recordMappingBusiness = new RecordMappingBusiness(Context);
        _edgeMappingBusiness = new Mock<IEdgeMappingBusiness>();
        _edgeBusiness = new Mock<IEdgeBusiness>();
        _recordBusiness = new Mock<IRecordBusiness>();
        _relationshipBusiness = new Mock<IRelationshipBusiness>();
        _classBusiness = new ClassBusiness(Context, _edgeMappingBusiness.Object, _recordBusiness.Object, _recordMappingBusiness, _relationshipBusiness.Object);
        _projectBusiness = new ProjectBusiness(Context, _classBusiness);
        _dataSourceBusiness = new DataSourceBusiness(Context, _edgeBusiness.Object, _recordBusiness.Object);
    }

    [Fact]
    public async Task CreateRecordMapping_Success_ReturnsIdAndCreatedAt()
    {
        var now = DateTime.UtcNow;
        var dto = new RecordMappingRequestDto {RecordParams = new JsonObject{["hello"] = "world"}, ClassId = cid, TagId = tid, DataSourceId = did};

        var result = await _recordMappingBusiness.CreateRecordMapping(pid, dto);
        result.Id.Should().BeGreaterThan(0);
        result.CreatedAt.Should().BeOnOrAfter(now);
        result.ClassId.Should().Be(cid);
        result.TagId.Should().Be(tid);
        result.DataSourceId.Should().Be(did);
    }
    
    [Fact]
    public async Task CreateRecordMapping_Fails_WhenNoDataSourceId()
    {
        var dto = new RecordMappingRequestDto {RecordParams = new JsonObject{["hello"] = "world"}, ClassId = cid, TagId = tid};
        var result = () => _recordMappingBusiness.CreateRecordMapping(pid, dto);
        await result.Should().ThrowAsync<ValidationException>();
    }
    
    [Fact]
    public async Task CreateRecordMapping_Fails_IfNoRecordParams()
    {
        var dto = new RecordMappingRequestDto {RecordParams = null, ClassId = cid, TagId = tid};
        var result = () => _recordMappingBusiness.CreateRecordMapping(pid, dto);
        await result.Should().ThrowAsync<ValidationException>();
    }
    
    [Fact]
    public async Task CreateRecordMapping_Fails_IfNoProjectId()
    {
        var dto = new RecordMappingRequestDto {RecordParams = new JsonObject{["hello"] = "world"}, ClassId = cid, TagId = tid, DataSourceId = did};
        var result = () => _recordMappingBusiness.CreateRecordMapping(pid + 99, dto);
        await result.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    [Fact]
    public async Task CreateRecordMapping_Fails_IfDeletedProjectId()
    {
        var project = await Context.Projects.FindAsync(pid);
        project.ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        Context.Projects.Update(project);
        await Context.SaveChangesAsync();
        var dto = new RecordMappingRequestDto {RecordParams = new JsonObject{["hello"] = "world"}, ClassId = cid, TagId = tid, DataSourceId = did};
        var result = () => _recordMappingBusiness.CreateRecordMapping(pid, dto);
        await result.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    [Fact]
    public async Task CreateRecordMapping_Fails_IfTagIdAndClassIdMissing()
    {
        var dto = new RecordMappingRequestDto {RecordParams = new JsonObject{["hello"] = "world"}, ClassId = null, TagId = null,  DataSourceId = did};
        var result = () => _recordMappingBusiness.CreateRecordMapping(pid, dto);
        await result.Should().ThrowAsync<InvalidRequestException>();
    }
    
    [Fact]
    public async Task GetAllRecordMappings_ReturnsOnlyForProjects()
    {
        var p2 = new Project { Name = "ExtraProj" };
        Context.Projects.Add(p2);
        await Context.SaveChangesAsync();
    
        await _recordMappingBusiness.CreateRecordMapping(pid, new RecordMappingRequestDto { RecordParams = new JsonObject{["hello"] = "world"}, ClassId = cid, TagId = tid, DataSourceId = did});
        await _recordMappingBusiness.CreateRecordMapping(p2.Id, new RecordMappingRequestDto { RecordParams = new JsonObject{["hello"] = "world"}, ClassId = cid, TagId = tid,  DataSourceId = did});
    
        var list = await _recordMappingBusiness.GetAllRecordMappings(pid, cid, tid);
        Assert.All(list, c => Assert.Equal(pid, c.ProjectId));
    }
    
    [Fact]
    public async Task GetAllrecordMappings_ExcludesSoftDeleted()
    {
        var recordMapping1 = new RecordMapping
        {
            RecordParams = "{\"param1\":\"value1\"}",
            ClassId = cid,
            TagId = tid,
            DataSourceId = did,
            ProjectId = pid,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            CreatedBy = null
        };
    
        var recordMapping2 = new RecordMapping
        {
            RecordParams = "{\"param2\":\"value2\"}",
            ClassId = cid,
            TagId = tid,
            DataSourceId = did,
            ProjectId = pid,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            CreatedBy = null,
            ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        Context.RecordMappings.Add(recordMapping1);
        Context.RecordMappings.Add(recordMapping2);
        await Context.SaveChangesAsync();
        
        var list = await _recordMappingBusiness.GetAllRecordMappings(pid, cid, tid);
        Assert.DoesNotContain(list, c => c.Id == recordMapping2.Id);
    }
    
    [Fact]
    public async Task GetRecordMapping_Success_WhenExists()
    {
        var recordMapping1 = new RecordMapping
        {
            RecordParams = "{\"param1\":\"value1\"}",
            ClassId = cid,
            TagId = tid,
            ProjectId = pid,
            DataSourceId = did,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            CreatedBy = null
        };
        Context.RecordMappings.Add(recordMapping1);
        await Context.SaveChangesAsync();
        var result = await _recordMappingBusiness.GetRecordMapping(pid, recordMapping1.Id);
        Assert.Equal(recordMapping1.Id, result.Id);
    }
    
    [Fact]
    public async Task GetRecordMapping_Fails_IfNoProjectID()
    {
        var recordMapping1 = new RecordMapping
        {
            RecordParams = "{\"param1\":\"value1\"}",
            ClassId = cid,
            TagId = tid,
            DataSourceId = did,
            ProjectId = pid,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            CreatedBy = null
        };
        Context.RecordMappings.Add(recordMapping1);
        await Context.SaveChangesAsync();
        var result = () => _recordMappingBusiness.GetRecordMapping(pid + 999, recordMapping1.Id);
        await result.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    [Fact]
    public async Task GetRecordMapping_Fails_IfDeletedRecordMapping()
    {
        var recordMapping1 = new RecordMapping
        {
            RecordParams = "{\"param1\":\"value1\"}",
            ClassId = cid,
            TagId = tid,
            ProjectId = pid,
            DataSourceId = did,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            CreatedBy = null,
            ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        Context.RecordMappings.Add(recordMapping1);
        await Context.SaveChangesAsync();
        var result = () => _recordMappingBusiness.GetRecordMapping(pid, recordMapping1.Id);
        await result.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    [Fact]
    public async Task UpdateRecordMapping_Success_ReturnsModifiedAt()
    {
        var recordMapping1 = new RecordMapping
        {
            RecordParams = "{\"hello\":\"world1\"}",
            ClassId = cid,
            TagId = tid,
            DataSourceId = did,
            ProjectId = pid,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            CreatedBy = null,
        };
        Context.RecordMappings.Add(recordMapping1);
        await Context.SaveChangesAsync();
        
        var dto = new RecordMappingRequestDto {RecordParams = new JsonObject{["hello"] = "world2"}, ClassId = cid, TagId = tid, DataSourceId = did};
        var updatedResult = await _recordMappingBusiness.UpdateRecordMapping(pid, recordMapping1.Id, dto);
        
        updatedResult.ModifiedAt.Should().BeOnOrAfter(updatedResult.CreatedAt);
    }
    
    [Fact]
    public async Task UpdateRecordMapping_Fails_IfNotFound()
    {
        var dto = new RecordMappingRequestDto {RecordParams = new JsonObject{["hello"] = "world2"}, ClassId = cid, TagId = tid, DataSourceId = did};
        var updatedResult = () => _recordMappingBusiness.UpdateRecordMapping(pid, 99, dto);
        updatedResult.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    [Fact]
    public async Task ArchiveRecordMapping_Success_WhenExists()
    {
        var beforeArchive = DateTime.UtcNow;
    
        var recordMapping1 = new RecordMapping
        {
            RecordParams = "{\"hello\":\"world1\"}",
            ClassId = cid,
            TagId = tid,
            DataSourceId = did,
            ProjectId = pid,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            CreatedBy = null,
        };
        Context.RecordMappings.Add(recordMapping1);
        await Context.SaveChangesAsync();
        
        var archivedResult = await _recordMappingBusiness.ArchiveRecordMapping(pid, recordMapping1.Id);
        Assert.True(archivedResult);
        
        var archivedRecordMapping = await Context.RecordMappings.FindAsync(recordMapping1.Id);
        Assert.NotNull(archivedRecordMapping);
        Assert.NotNull(archivedRecordMapping.ArchivedAt);
        Assert.True(archivedRecordMapping.ArchivedAt >= beforeArchive);
        Assert.True(archivedRecordMapping.ArchivedAt <= DateTime.UtcNow);
    }
    
    [Fact]
    public async Task DeleteRecordMapping_Success_WhenExists()
    {
        var recordMapping1 = new RecordMapping
        {
            RecordParams = "{\"hello\":\"world1\"}",
            ClassId = cid,
            TagId = tid,
            DataSourceId = did,
            ProjectId = pid,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            CreatedBy = null,
        };
        Context.RecordMappings.Add(recordMapping1);
        await Context.SaveChangesAsync();
        
        var deletedResult = await _recordMappingBusiness.DeleteRecordMapping(pid, recordMapping1.Id);
        Assert.True(deletedResult);
        
        var archivedRecordMapping = await Context.RecordMappings.FindAsync(recordMapping1.Id);
        Assert.Null(archivedRecordMapping);
    }
    
    [Fact]
    public async Task RecordMappingArchived_WhenProjectArchived()
    {
        var beforeArchive = DateTime.UtcNow;
        var recordMapping1 = new RecordMapping
        {
            RecordParams = "{\"hello\":\"world1\"}",
            ClassId = cid,
            TagId = tid,
            DataSourceId = did,
            ProjectId = pid,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            CreatedBy = null,
        };
        Context.RecordMappings.Add(recordMapping1);
        await Context.SaveChangesAsync();
        
        var deletedResult = await _projectBusiness.ArchiveProject(pid);
        Assert.True(deletedResult);
        
        // procedure is not traced by entity framework
        //this forces EF to sync to db on next query
        Context.ChangeTracker.Clear();
        
        var archivedRecordMapping = await Context.RecordMappings.FindAsync(recordMapping1.Id);
        Assert.NotNull(archivedRecordMapping);
        Assert.NotNull(archivedRecordMapping.ArchivedAt);
        Assert.True(archivedRecordMapping.ArchivedAt >= beforeArchive);
        Assert.True(archivedRecordMapping.ArchivedAt <= DateTime.UtcNow); 
    }
    
    [Fact]
    public async Task RecordMappingArchived_WhenClassArchived()
    {
        var beforeArchive = DateTime.UtcNow;
        var recordMapping1 = new RecordMapping
        {
            RecordParams = "{\"hello\":\"world1\"}",
            ClassId = cid,
            TagId = tid,
            DataSourceId = did,
            ProjectId = pid,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            CreatedBy = null,
        };
        Context.RecordMappings.Add(recordMapping1);
        await Context.SaveChangesAsync();
        
        var deletedResult = await _classBusiness.ArchiveClass(pid,cid);
        Assert.True(deletedResult);
        
        // procedure is not traced by entity framework
        //this forces EF to sync to db on next query
        Context.ChangeTracker.Clear();
        
        var archivedRecordMapping = await Context.RecordMappings.FindAsync(recordMapping1.Id);
        Assert.NotNull(archivedRecordMapping);
        Assert.NotNull(archivedRecordMapping.ArchivedAt);
        Assert.True(archivedRecordMapping.ArchivedAt >= beforeArchive);
        Assert.True(archivedRecordMapping.ArchivedAt <= DateTime.UtcNow); 
    }
    
    [Fact]
    public async Task RecordMappingArchived_WhenDataSourceArchived()
    {
        var beforeArchive = DateTime.UtcNow;
        var recordMapping1 = new RecordMapping
        {
            RecordParams = "{\"hello\":\"world1\"}",
            ClassId = cid,
            TagId = tid,
            DataSourceId = did,
            ProjectId = pid,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            CreatedBy = null,
        };
        Context.RecordMappings.Add(recordMapping1);
        await Context.SaveChangesAsync();
        
        var deletedResult = await _dataSourceBusiness.ArchiveDataSource(pid,did);
        Assert.True(deletedResult);
        
        // procedure is not traced by entity framework
        //this forces EF to sync to db on next query
        Context.ChangeTracker.Clear();
        
        var archivedRecordMapping = await Context.RecordMappings.FindAsync(recordMapping1.Id);
        Assert.NotNull(archivedRecordMapping);
        Assert.NotNull(archivedRecordMapping.ArchivedAt);
        Assert.True(archivedRecordMapping.ArchivedAt >= beforeArchive);
        Assert.True(archivedRecordMapping.ArchivedAt <= DateTime.UtcNow); 
    }

    protected override async Task SeedTestDataAsync()
    {
        await base.SeedTestDataAsync();
        var project = new Project { Name = "Project 1" };
        Context.Projects.Add(project);
        
        await Context.SaveChangesAsync();
        pid = project.Id;
        var tag = new Tag { Name = "Tag 1", ProjectId = pid};
        Context.Tags.Add(tag);
        await Context.SaveChangesAsync();
        tid = tag.Id;
        var testClass = new Class{Name = "Class 1", ProjectId = pid};
        Context.Classes.Add(testClass);
        await Context.SaveChangesAsync();
        var testClass2 = new Class{Name = "Class 2", ProjectId = pid};
        Context.Classes.Add(testClass2);
        await Context.SaveChangesAsync();
        cid = testClass.Id;
        var dataSource1 = new DataSource { Name = "DataSource 1", ProjectId = pid };
        Context.DataSources.Add(dataSource1);
        await Context.SaveChangesAsync();
        did = dataSource1.Id;
    }
    
}