using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.helpers.exceptions;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using Moq;
using Record = deeplynx.datalayer.Models.Record;

namespace deeplynx.tests;

[Collection("Test Suite Collection")]
public class RecordBusinessTests : IntegrationTestBase
{
    private RecordBusiness _recordBusiness;
    public long pid;
    public long did;
    public long cid;
    public long rid;
    public long tid;
    public long os1;
    public string rprop;
    public string rogid;
    public string rdesc;
    public string ruri;

    public RecordBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _recordBusiness = new RecordBusiness(Context);
    }

    protected override async Task SeedTestDataAsync()
    {
        await base.SeedTestDataAsync();
        
        // Seed test data
        var project = new Project
        {
            Name = "Test Project",
            Description = "Test project for unit tests",
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        Context.Projects.Add(project);
        await Context.SaveChangesAsync();
        pid = project.Id;
        
        var dataSource = new DataSource
        {
            Name = "Test Data Source",
            Description = "Test data source for unit tests",
            ProjectId = project.Id,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        Context.DataSources.Add(dataSource);
        await Context.SaveChangesAsync();
        did = dataSource.Id;
        
        var testClass = new Class
        {
            Name = "Test Class",
            Description = "Test class for unit tests",
            ProjectId = project.Id,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        Context.Classes.Add(testClass);
        await Context.SaveChangesAsync();
        cid = testClass.Id;
        
        var config = new JsonObject();
        var objectStorage = new ObjectStorage
        {
            Name = "Object Storage 1",
            Type = "filesystem",
            Config = config.ToString(),
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        Context.ObjectStorages.Add(objectStorage);
        await Context.SaveChangesAsync();
        os1 = objectStorage.Id;

        var testTag = new Tag
        {
            Name = "Test Tag",
            ProjectId = project.Id,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        
        var testRecord = new Record
        {
            Name = "Test Record",
            Description = "Test record for unit tests",
            OriginalId = "og_id",
            Properties = JsonSerializer.Serialize(new { TestProperty = "TestValue" }),
            ProjectId = project.Id,
            DataSourceId = dataSource.Id,
            ClassId = testClass.Id,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            Tags =  new List<Tag> { testTag },
            Uri = "localhost:8090"
        };
        
        Context.Records.Add(testRecord);
        Context.Tags.Add(testTag);
        await Context.SaveChangesAsync();
        
        rid =  testRecord.Id;
        tid = testTag.Id;
        rprop = testRecord.Properties;
        rogid = testRecord.OriginalId;
        rdesc = testRecord.Description;
        ruri = testRecord.Uri;
    }

    #region GetAllRecords Tests

    [Fact]
    public async Task GetAllRecords_ValidProjectId_ReturnsRecords()
    {
        // Arrange
        var projectId = pid;

        // Act
        var result = await _recordBusiness.GetAllRecords(projectId, null, true);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Test Record", result.First().Name);
    }
    
    [Fact]
    public async Task GetAllRecords_ReturnsTags()
    {
        // Arrange
        var projectId = pid;

        // Act
        var result = await _recordBusiness.GetAllRecords(projectId, null, true);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.First().Tags);
        Assert.Equal("Test Tag", result.First().Tags.First().Name);
        Assert.Single(result);
        Assert.Equal("Test Record", result.First().Name);
    }

    [Fact]
    public async Task GetAllRecords_WithDataSourceId_ReturnsFilteredRecords()
    {
        // Arrange
        var projectId = pid;
        var dataSourceId = did;

        // Act
        var result = await _recordBusiness.GetAllRecords(projectId, dataSourceId, true);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(dataSourceId, result.First().DataSourceId);
    }
    
    [Fact]
    public async Task GetAllRecords_InvalidProjectId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var invalidProjectId = 999L;

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _recordBusiness.GetAllRecords(invalidProjectId, null, true));
    }

    #endregion

    #region GetRecord Tests

    [Fact]
    public async Task GetRecord_ValidIds_ReturnsRecord()
    {
        // Arrange
        var projectId = pid;
        var recordId = rid;

        // Act
        var result = await _recordBusiness.GetRecord(projectId, recordId, true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(recordId, result.Id);
        Assert.Equal("Test Record", result.Name);
    }

    [Fact]
    public async Task GetRecord_InvalidProjectId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var invalidProjectId = 999L;
        var recordId = rid;

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _recordBusiness.GetRecord(invalidProjectId, recordId, true));
    }

    #endregion

    #region CreateRecord Tests

    [Fact]
    public async Task CreateRecord_ValidData_CreatesRecord()
    {
        // Arrange
        var projectId = pid;
        var dataSourceId = did;
        var dto = new CreateRecordRequestDto
        {
            Name = "New Test Record",
            Description = "Test Record Description",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { TestProp = "TestValue" }))!,
            Uri = "test://uri",
            OriginalId = "original-123",
            ClassId = cid
        };

        // Act
        var result = await _recordBusiness.CreateRecord(projectId, dataSourceId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Test Record", result.Name);
        Assert.Equal("Test Record Description", result.Description);
        Assert.Equal(projectId, result.ProjectId);
        Assert.Equal(dataSourceId, result.DataSourceId);
        Assert.Equal("test://uri", result.Uri);
        Assert.Equal("original-123", result.OriginalId);
        Assert.Equal(cid, result.ClassId);

        // Verify record was actually created in database
        var createdRecord = await Context.Records.FindAsync(result.Id);
        Assert.NotNull(createdRecord);
        Assert.Equal("New Test Record", createdRecord.Name);
    }

    [Fact]
    public async Task CreateRecord_InvalidProjectId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var invalidProjectId = 999L;
        var dataSourceId = did;
        var dto = new CreateRecordRequestDto
        {
            Name = "Test Record",
            Description = "Test Record Description",
            OriginalId = "original-123",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { TestProp = "TestValue" }))!,
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _recordBusiness.CreateRecord(invalidProjectId, dataSourceId, dto));
    }

    [Fact]
    public async Task CreateRecord_InvalidDataSourceId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var projectId = pid;
        var invalidDataSourceId = 999L;
        var dto = new CreateRecordRequestDto
        {
            Name = "Test Record",
            Description = "Test Record Description",
            OriginalId = "original-123",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { TestProp = "TestValue" }))!,
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _recordBusiness.CreateRecord(projectId, invalidDataSourceId, dto));
    }

    [Fact]
    public async Task CreateRecord_TooDeepJson_ThrowsException()
    {
        // Arrange
        var projectId = pid;
        var dataSourceId = did;
        var deepJson = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new 
        { 
            Level1 = new 
            { 
                Level2 = new 
                { 
                    Level3 = new 
                    { 
                        Level4 = new 
                        { 
                            Value = "Too deep" 
                        } 
                    } 
                } 
            } 
        }))!;
    
        var dto = new CreateRecordRequestDto
        {
            Name = "Deep JSON Record",
            Description = "Deep JSON Record Description",
            OriginalId = "original-123",
            Properties = deepJson
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _recordBusiness.CreateRecord(projectId, dataSourceId, dto));
        Assert.Contains("depth of the JSON structure exceeds", exception.Message);
    }
    
    #endregion

    #region BulkCreateRecords Tests

    [Fact]
    public async Task BulkCreateRecords_ValidData_CreatesMultipleRecords()
    {
        // Arrange
        var projectId = pid;
        var dataSourceId = did;
        List<CreateRecordRequestDto> records = new List<CreateRecordRequestDto>
        {
            new CreateRecordRequestDto
            {
                Name = "Bulk Record 1",
                Description = "Bulk Record 1 Description",
                ObjectStorageId = os1,
                OriginalId = "br1",
                Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { TestProp = "Value1" }))!
            },
            new CreateRecordRequestDto
            {
                Name = "Bulk Record 2",
                Description = "Bulk Record 2 Description",
                ObjectStorageId = os1,
                OriginalId = "br2",
                Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { TestProp = "Value2" }))!
            }
        };

        // Act
        var result = await _recordBusiness.BulkCreateRecords(projectId, dataSourceId, records);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, r => r.Name == "Bulk Record 1");
        Assert.Contains(result, r => r.Name == "Bulk Record 2");

        // Verify records were actually created in database
        var recordCount = await Context.Records.CountAsync(r => r.ProjectId == projectId);
        Assert.Equal(3, recordCount); // 1 from seed + 2 new
    }

    [Fact]
    public async Task BulkCreateRecords_EmptyList_ThrowsException()
    {
        // Arrange
        var projectId = pid;
        var dataSourceId = did;
        List<CreateRecordRequestDto> records = new List<CreateRecordRequestDto>();
        
        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _recordBusiness.BulkCreateRecords(projectId, dataSourceId, records));
    }

    [Fact]
    public async Task BulkCreateRecords_InvalidProjectId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var invalidProjectId = 999L;
        var dataSourceId = 1L;
        List<CreateRecordRequestDto> records = new List<CreateRecordRequestDto>
        {
            new CreateRecordRequestDto
            {
                Name = "Test Record",
                Description = "Test Record Description",
                OriginalId = "test",
                Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { TestProp = "TestValue" }))!
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _recordBusiness.BulkCreateRecords(invalidProjectId, dataSourceId, records));
    }

    #endregion

    #region UpdateRecord Tests

    [Fact]
    public async Task UpdateRecord_ValidData_UpdatesRecord()
    {
        // Arrange
        var projectId = pid;
        var recordId = rid;
        var dto = new UpdateRecordRequestDto
        {
            Name = "Updated Test Record",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { UpdatedProp = "UpdatedValue" }))!,
            Uri = "updated://uri",
            OriginalId = "updated-123",
            Description = "Updated Description",
            ClassId = cid
        };

        // Act
        var result = await _recordBusiness.UpdateRecord(projectId, recordId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Test Record", result.Name);
        Assert.Equal("updated://uri", result.Uri);
        Assert.Equal("updated-123", result.OriginalId);
        Assert.Equal("Updated Description", result.Description);

        // Verify record was actually updated in database
        var updatedRecord = await Context.Records.FindAsync(recordId);
        Assert.NotNull(updatedRecord);
        Assert.Equal("Updated Test Record", updatedRecord.Name);
        
        // Verify that get function gets updated version
        var getResult = await _recordBusiness.GetRecord(projectId, recordId, true);
        Assert.NotNull(getResult);
        Assert.Equal("Updated Test Record", getResult.Name);
        Assert.Equal("Updated Description", getResult.Description);
    }

    [Fact]
    public async Task UpdateRecord_PartialUpdate_UpdatesRecord()
    {
        // Arrange
        var projectId = pid;
        var recordId = rid;
        var dto = new UpdateRecordRequestDto
        {
            Name = "New-ish Test Record"
        };

        // Act
        var result = await _recordBusiness.UpdateRecord(projectId, recordId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New-ish Test Record", result.Name);
        Assert.Equal(ruri, result.Uri);
        Assert.Equal(rogid, result.OriginalId);
        Assert.Equal(rdesc, result.Description);
        Assert.Equal(rprop, result.Properties);

        // Verify record was actually updated in database
        var updatedRecord = await Context.Records.FindAsync(recordId);
        Assert.NotNull(updatedRecord);
        Assert.Equal("New-ish Test Record", updatedRecord.Name);
        Assert.Equal(rdesc, updatedRecord.Description);
        
        // Verify that get function gets updated version
        var getResult = await _recordBusiness.GetRecord(projectId, recordId, true);
        Assert.NotNull(getResult);
        Assert.Equal("New-ish Test Record", getResult.Name);
        Assert.Equal(rdesc, getResult.Description);
        
    }
    
    [Fact]
    public async Task UpdateRecord_InvalidRecordId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var projectId = pid;
        var invalidRecordId = 999L;
        var dto = new UpdateRecordRequestDto
        {
            Name = "Updated Record",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { TestProp = "TestValue" }))!
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _recordBusiness.UpdateRecord(projectId, invalidRecordId, dto));
    }

    [Fact]
    public async Task UpdateRecord_RecordFromDifferentProject_ThrowsKeyNotFoundException()
    {
        // Arrange
        var wrongProjectId = 999L;
        var recordId = rid;
        var dto = new UpdateRecordRequestDto
        {
            Name = "Updated Record",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { TestProp = "TestValue" }))!
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _recordBusiness.UpdateRecord(wrongProjectId, recordId, dto));
    }

    [Fact]
    public async Task UpdateRecord_TooDeepJson_ThrowsException()
    {
        // Arrange
        var projectId = pid;
        var recordId = rid;
        var deepJson = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new 
        { 
            Level1 = new 
            { 
                Level2 = new 
                { 
                    Level3 = new 
                    { 
                        Level4 = new 
                        { 
                            Value = "Too deep" 
                        } 
                    } 
                } 
            } 
        }))!;
        
        var dto = new UpdateRecordRequestDto
        {
            Name = "Deep JSON Record",
            Properties = deepJson
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _recordBusiness.UpdateRecord(projectId, recordId, dto));
        Assert.Contains("depth of the JSON structure exceeds", exception.Message);
    }

    #endregion

    #region DeleteRecord Tests

    [Fact]
    public async Task DeleteRecord_ValidData_DeletesRecord()
    {
        // Arrange
        var projectId = pid;
        var recordId = rid;

        // Verify record exists before deletion
        var recordExists = await Context.Records.AnyAsync(r => r.Id == recordId);
        Assert.True(recordExists);

        // Act
        var result = await _recordBusiness.DeleteRecord(projectId, recordId);

        // Assert
        Assert.True(result);

        // Verify record was actually deleted from database
        var deletedRecord = await Context.Records.FindAsync(recordId);
        Assert.Null(deletedRecord);
    }

    [Fact]
    public async Task DeleteRecord_InvalidRecordId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var projectId = pid;
        var invalidRecordId = 999L;

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _recordBusiness.DeleteRecord(projectId, invalidRecordId));
    }

    [Fact]
    public async Task DeleteRecord_RecordFromDifferentProject_ThrowsKeyNotFoundException()
    {
        // Arrange
        var wrongProjectId = 999L;
        var recordId = rid;

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _recordBusiness.DeleteRecord(wrongProjectId, recordId));
    }

    #endregion

    #region ArchiveRecord Tests

    [Fact]
    public async Task ArchiveRecord_InvalidRecordId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var projectId = 1L;
        var invalidRecordId = 999L;

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _recordBusiness.ArchiveRecord(projectId, invalidRecordId));
    }

    [Fact]
    public async Task ArchiveRecord_RecordFromDifferentProject_ThrowsKeyNotFoundException()
    {
        // Arrange
        var wrongProjectId = 999L;
        var recordId = rid;

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _recordBusiness.ArchiveRecord(wrongProjectId, recordId));
    }

    [Fact]
    public async Task ArchiveRecord_AlreadyArchivedRecord_ThrowsKeyNotFoundException()
    {
        // Arrange
        var projectId = pid;
        var recordId = rid;

        // First archive the record
        var record = await Context.Records.FindAsync(recordId);
        record.IsArchived = true;
        await Context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _recordBusiness.ArchiveRecord(projectId, recordId));
    }

    #endregion

    #region Edge Cases and Integration Tests

    [Fact]
    public async Task CreateRecord_ValidJsonDepthThree_Success()
    {
        // Arrange
        var projectId = pid;
        var dataSourceId = did;
        var validDepthJson = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new 
        { 
            Level1 = new 
            { 
                Level2 = new 
                { 
                    Level3 = "Valid depth" 
                } 
            } 
        }))!;
        
        var dto = new CreateRecordRequestDto
        {
            Name = "Valid Depth Record",
            Description = "Valid Depth Description",
            ObjectStorageId = os1,
            OriginalId = "VDR1",
            Properties = validDepthJson
        };

        // Act
        var result = await _recordBusiness.CreateRecord(projectId, dataSourceId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Valid Depth Record", result.Name);
    }

    [Fact]
    public async Task CreateRecord_NullProperties_ThrowsException()
    {
        // Arrange
        var projectId = pid;
        var dataSourceId = did;
        var dto = new CreateRecordRequestDto
        {
            Name = "No Properties Record",
            Description = "No Properties Description",
            OriginalId = "NoProps",
            Properties = null
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _recordBusiness.CreateRecord(projectId, dataSourceId, dto));
    }
    
    [Fact]
    public async Task CreateRecord_NoName_ThrowsException()
    {
        // Arrange
        var projectId = pid;
        var dataSourceId = did;
        var dto = new CreateRecordRequestDto
        {
            Name = null,
            Description = "No Name Description",
            OriginalId = "NoName",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { TestProp = "TestValue" }))!,
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _recordBusiness.CreateRecord(projectId, dataSourceId, dto));
    }

    [Fact]
    public async Task CreateRecord_NoDescription_ThrowsException()
    {
        // Arrange
        var projectId = pid;
        var dataSourceId = did;
        var dto = new CreateRecordRequestDto
        {
            Name = "No Description Record",
            Description = null,
            OriginalId = "NoDesc",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { TestProp = "TestValue" }))!,
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _recordBusiness.CreateRecord(projectId, dataSourceId, dto));
    }

    [Fact]
    public async Task CreateRecord_NoOriginalId_ThrowsException()
    {
        // Arrange
        var projectId = pid;
        var dataSourceId = did;
        var dto = new CreateRecordRequestDto
        {
            Name = "No Original ID Record",
            Description = "No Original ID Description",
            OriginalId = null,
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { TestProp = "TestValue" }))!,
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _recordBusiness.CreateRecord(projectId, dataSourceId, dto));
    }

    #endregion
    
    #region UnarchiveRecord Tests

    [Fact]
    public async Task UnarchiveRecord_ValidArchivedRecord_UnarchivesSuccessfully()
    {
        var projectId = pid;
        var archivedRecord = new Record
        {
            Name = "Archived Record",
            Description = "Archived Record Description",
            OriginalId = "Archived Record OriginalId",
            ObjectStorageId = os1,
            Properties = JsonSerializer.Serialize(new { Foo = "Bar" }),
            ProjectId = projectId,
            DataSourceId = did,
            ClassId = cid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            IsArchived = true
        };
        Context.Records.Add(archivedRecord);
        await Context.SaveChangesAsync();

        var result = await _recordBusiness.UnarchiveRecord(projectId, archivedRecord.Id);
        
        //this forces EF to sync to db on next query
        Context.ChangeTracker.Clear();

        Assert.True(result);
        var reloaded = await Context.Records.FindAsync(archivedRecord.Id);
        Assert.NotNull(reloaded);
        Assert.True(reloaded.IsArchived);
    }

    [Fact]
    public async Task UnarchiveRecord_InvalidRecordId_ThrowsKeyNotFoundException()
    {
        var projectId = pid;
        var invalidRecordId = 999L;

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _recordBusiness.UnarchiveRecord(projectId, invalidRecordId));
    }

    [Fact]
    public async Task UnarchiveRecord_RecordFromDifferentProject_ThrowsKeyNotFoundException()
    {
        var differentProjectId = 999L;
        var recordId = rid;

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _recordBusiness.UnarchiveRecord(differentProjectId, recordId));
    }

    [Fact]
    public async Task UnarchiveRecord_AlreadyUnarchived_ThrowsKeyNotFoundException()
    {
        var projectId = pid;
        var recordId = rid;

        // Confirm record is not archived
        var existing = await Context.Records.FindAsync(recordId);
        existing.IsArchived = false;
        await Context.SaveChangesAsync();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _recordBusiness.UnarchiveRecord(projectId, recordId));
    }

    #endregion
    
    #region Attach/Unattach Tag Tests

    [Fact]
    public async Task AttachTag_SuccessfullyAttachesTagToRecord()
    {
        var projectId = pid;

        var newTag = new Tag
        {
            Name = "Tag to Attach",
            ProjectId = projectId,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        Context.Tags.Add(newTag);

        var record = await Context.Records.Include(r => r.Tags).FirstAsync(r => r.Id == rid);
        record.Tags.Clear(); // ensure tag not already attached
        await Context.SaveChangesAsync();

        var result = await _recordBusiness.AttachTag(projectId, record.Id, newTag.Id);

        Assert.True(result);
        var updatedRecord = await Context.Records.Include(r => r.Tags).FirstAsync(r => r.Id == record.Id);
        Assert.Contains(updatedRecord.Tags, t => t.Id == newTag.Id);
    }

    [Fact]
    public async Task AttachTag_RecordNotFound_ThrowsKeyNotFound()
    {
        var projectId = pid;
        var validTagId = tid;

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _recordBusiness.AttachTag(projectId, 9999L, validTagId));
    }

    [Fact]
    public async Task AttachTag_TagNotFound_ThrowsKeyNotFound()
    {
        var projectId = pid;
        var validRecordId = rid;

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _recordBusiness.AttachTag(projectId, validRecordId, 9999L));
    }

    [Fact]
    public async Task AttachTag_AlreadyAttached_ThrowsException()
    {
        var projectId = pid;
        var recordId = rid;
        var tagId = tid;

        await Assert.ThrowsAsync<Exception>(() =>
            _recordBusiness.AttachTag(projectId, recordId, tagId));
    }

    [Fact]
    public async Task UnattachTag_SuccessfullyDetachesTagFromRecord()
    {
        var projectId = pid;
        var record = await Context.Records.Include(r => r.Tags).FirstAsync(r => r.Id == rid);
        var tagId = tid;
        Assert.Contains(record.Tags, t => t.Id == tagId);

        var result = await _recordBusiness.UnattachTag(projectId, record.Id, tagId);

        Assert.True(result);
        var refreshed = await Context.Records.Include(r => r.Tags).FirstAsync(r => r.Id == record.Id);
        Assert.DoesNotContain(refreshed.Tags, t => t.Id == tagId);
    }

    [Fact]
    public async Task UnattachTag_RecordNotFound_ThrowsKeyNotFound()
    {
        var projectId = pid;
        var tagId = tid;

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _recordBusiness.UnattachTag(projectId, 9999L, tagId));
    }

    [Fact]
    public async Task UnattachTag_TagNotFound_ThrowsKeyNotFound()
    {
        var projectId = pid;
        var recordId = rid;

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _recordBusiness.UnattachTag(projectId, recordId, 9999L));
    }

    #endregion
#region GetRecordsByOriginalId Tests

[Fact]
public async Task GetRecordsByOriginalId_ValidOriginalIds_ReturnsMatchingRecords()
{
    // Arrange
    var record1 = new Record
    {
        Name = "Test Record 1",
        ProjectId = pid,
        DataSourceId = did,
        ObjectStorageId = os1,
        ClassId = cid,
        Properties = "{}",
        OriginalId = "original-id-1",
        Description = "Test record 1",
        LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
    };

    Context.Records.Add(record1);
    await Context.SaveChangesAsync();

    var originalIds = new List<string> { "original-id-1" };

    // Act
    var result = await _recordBusiness.GetRecordsByOriginalId(pid, originalIds);

    // Assert
    Assert.Equal(1, result.Count);
    Assert.Equal("original-id-1", result.First().OriginalId);
    Assert.Equal(pid, result.First().ProjectId);
}

[Fact]
public async Task GetRecordsByOriginalId_MissingOriginalIds_ThrowsKeyNotFoundException()
{
    // Arrange
    var originalIds = new List<string> { "non-existent-id" };

    // Act & Assert
    var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
        () => _recordBusiness.GetRecordsByOriginalId(pid, originalIds));
    
    Assert.Contains("Records not found with original IDs", exception.Message);
}

[Fact]
public async Task GetRecordsByOriginalId_NullOriginalIds_ThrowsArgumentException()
{
    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(
        () => _recordBusiness.GetRecordsByOriginalId(pid, null));
}

[Fact]
public async Task GetRecordsByOriginalId_ExcludesArchivedRecords()
{
    // Arrange
    var archivedRecord = new Record
    {
        Name = "Archived Record",
        ProjectId = pid,
        DataSourceId = did,
        ObjectStorageId = os1,
        ClassId = cid,
        Properties = "{}",
        OriginalId = "archived-id",
        Description = "Archived record",
        LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
        IsArchived = true
    };

    Context.Records.Add(archivedRecord);
    await Context.SaveChangesAsync();

    var originalIds = new List<string> { "archived-id" };

    // Act & Assert
    var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
        () => _recordBusiness.GetRecordsByOriginalId(pid, originalIds));
    
    Assert.Contains("archived-id", exception.Message);
}

[Fact]
public async Task GetRecordsByOriginalId_InvalidProjectId_ThrowsKeyNotFoundException()
{
    // Arrange
    var originalIds = new List<string> { "some-id" };
    var invalidProjectId = 999L;

    // Act & Assert
    await Assert.ThrowsAsync<KeyNotFoundException>(
        () => _recordBusiness.GetRecordsByOriginalId(invalidProjectId, originalIds));
}

#endregion
    
}