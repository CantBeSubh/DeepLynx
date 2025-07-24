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
            Id = 100,
            Name = "Test Project",
            Description = "Test project for unit tests",
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        
        var dataSource = new DataSource
        {
            Id = 100,
            Name = "Test Data Source",
            Description = "Test data source for unit tests",
            ProjectId = 100,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        
        var testClass = new Class
        {
            Id = 100,
            Name = "Test Class",
            Description = "Test class for unit tests",
            ProjectId = 100,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };

        var testTag = new Tag
        {
            Id = 100,
            Name = "Test Tag",
            ProjectId = 100,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        
        var testRecord = new Record
        {
            Id = 100,
            Name = "Test Record",
            Properties = JsonSerializer.Serialize(new { TestProperty = "TestValue" }),
            ProjectId = 100,
            DataSourceId = 100,
            ClassId = 100,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            Tags =  new List<Tag> { testTag }
        };

        Context.Projects.Add(project);
        Context.DataSources.Add(dataSource);
        Context.Classes.Add(testClass);
        Context.Records.Add(testRecord);
        Context.Tags.Add(testTag);
        await Context.SaveChangesAsync();
    }

    #region GetAllRecords Tests

    [Fact]
    public async Task GetAllRecords_ValidProjectId_ReturnsRecords()
    {
        // Arrange
        var projectId = 100L;

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
        var projectId = 100L;

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
        var projectId = 100L;
        var dataSourceId = 100L;

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
        var projectId = 100L;
        var recordId = 100L;

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
        var recordId = 100L;

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
        var projectId = 100L;
        var dataSourceId = 100L;
        var dto = new RecordRequestDto
        {
            Name = "New Test Record",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { TestProp = "TestValue" }))!,
            Uri = "test://uri",
            OriginalId = "original-123",
            ClassId = 100
        };

        // Act
        var result = await _recordBusiness.CreateRecord(projectId, dataSourceId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Test Record", result.Name);
        Assert.Equal(projectId, result.ProjectId);
        Assert.Equal(dataSourceId, result.DataSourceId);
        Assert.Equal("test://uri", result.Uri);
        Assert.Equal("original-123", result.OriginalId);
        Assert.Equal(100, result.ClassId);

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
        var dataSourceId = 100L;
        var dto = new RecordRequestDto
        {
            Name = "Test Record",
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
        var projectId = 100L;
        var invalidDataSourceId = 999L;
        var dto = new RecordRequestDto
        {
            Name = "Test Record",
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
        var projectId = 100L;
        var dataSourceId = 100L;
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
    
        var dto = new RecordRequestDto
        {
            Name = "Deep JSON Record",
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
        var projectId = 100L;
        var dataSourceId = 100L;
        List<RecordRequestDto> records = new List<RecordRequestDto>
        {
            new RecordRequestDto
            {
                Name = "Bulk Record 1",
                Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { TestProp = "Value1" }))!
            },
            new RecordRequestDto
            {
                Name = "Bulk Record 2",
                Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { TestProp = "Value2" }))!
            }
        };

        // Act
        var result = await _recordBusiness.BulkCreateRecords(projectId, dataSourceId, records);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, records.Count());
        Assert.Contains(records, r => r.Name == "Bulk Record 1");
        Assert.Contains(records, r => r.Name == "Bulk Record 2");

        // Verify records were actually created in database
        var recordCount = await Context.Records.CountAsync(r => r.ProjectId == projectId);
        Assert.Equal(3, recordCount); // 1 from seed + 2 new
    }

    [Fact]
    public async Task BulkCreateRecords_EmptyList_ReturnsEmptyResult()
    {
        // Arrange
        var projectId = 100L;
        var dataSourceId = 100L;
        List<RecordRequestDto> records = new List<RecordRequestDto>();

        // Act
        var result = await _recordBusiness.BulkCreateRecords(projectId, dataSourceId, records);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(records);
    }

    [Fact]
    public async Task BulkCreateRecords_InvalidProjectId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var invalidProjectId = 999L;
        var dataSourceId = 1L;
        List<RecordRequestDto> records = new List<RecordRequestDto>
        {
            new RecordRequestDto
            {
                Name = "Test Record",
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
        var projectId = 100L;
        var recordId = 100L;
        var dto = new RecordRequestDto
        {
            Name = "Updated Test Record",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { UpdatedProp = "UpdatedValue" }))!,
            Uri = "updated://uri",
            OriginalId = "updated-123",
            Description = "Updated Description",
            ClassId = 100
        };

        // Act
        var result = await _recordBusiness.UpdateRecord(projectId, recordId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Test Record", result.Name);
        Assert.Equal("updated://uri", result.Uri);
        Assert.Equal("updated-123", result.OriginalId);
        Assert.Equal("Updated Description", result.Description);
        Assert.NotNull(result.ModifiedAt);

        // Verify record was actually updated in database
        var updatedRecord = await Context.Records.FindAsync(recordId);
        Assert.NotNull(updatedRecord);
        Assert.Equal("Updated Test Record", updatedRecord.Name);
        
        // Verify that get function gets updated version
        var getResult = await _recordBusiness.GetRecord(projectId, recordId, true);
        Assert.NotNull(getResult);
        Assert.Equal("Updated Test Record", getResult.Name);
        Assert.Equal("Updated Description", getResult.Description);
        Assert.NotNull(getResult.ModifiedAt);
    }

    [Fact]
    public async Task UpdateRecord_InvalidRecordId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var projectId = 100L;
        var invalidRecordId = 999L;
        var dto = new RecordRequestDto
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
        var recordId = 100L;
        var dto = new RecordRequestDto
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
        var projectId = 100L;
        var recordId = 100L;
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
        
        var dto = new RecordRequestDto
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
        var projectId = 100L;
        var recordId = 100L;

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
        var projectId = 100L;
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
        var recordId = 100L;

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
        var projectId = 100L;
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
        var recordId = 100L;

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _recordBusiness.ArchiveRecord(wrongProjectId, recordId));
    }

    [Fact]
    public async Task ArchiveRecord_AlreadyArchivedRecord_ThrowsKeyNotFoundException()
    {
        // Arrange
        var projectId = 100L;
        var recordId = 100L;

        // First archive the record
        var record = await Context.Records.FindAsync(recordId);
        record.ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
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
        var projectId = 100L;
        var dataSourceId = 100L;
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
        
        var dto = new RecordRequestDto
        {
            Name = "Valid Depth Record",
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
        var projectId = 100L;
        var dataSourceId = 100L;
        var dto = new RecordRequestDto
        {
            Name = "No Properties Record",
            Properties = null
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _recordBusiness.CreateRecord(projectId, dataSourceId, dto));
    }

    #endregion
    
    #region UnarchiveRecord Tests

    [Fact]
    public async Task UnarchiveRecord_ValidArchivedRecord_UnarchivesSuccessfully()
    {
        var projectId = 100L;
        var archivedRecord = new Record
        {
            Name = "Archived Record",
            Properties = JsonSerializer.Serialize(new { Foo = "Bar" }),
            ProjectId = projectId,
            DataSourceId = 100,
            ClassId = 100,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-2), DateTimeKind.Unspecified)
        };
        Context.Records.Add(archivedRecord);
        await Context.SaveChangesAsync();

        var result = await _recordBusiness.UnarchiveRecord(projectId, archivedRecord.Id);
        
        //this forces EF to sync to db on next query
        Context.ChangeTracker.Clear();

        Assert.True(result);
        var reloaded = await Context.Records.FindAsync(archivedRecord.Id);
        Assert.NotNull(reloaded);
        Assert.Null(reloaded.ArchivedAt);
    }

    [Fact]
    public async Task UnarchiveRecord_InvalidRecordId_ThrowsKeyNotFoundException()
    {
        var projectId = 100L;
        var invalidRecordId = 999L;

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _recordBusiness.UnarchiveRecord(projectId, invalidRecordId));
    }

    [Fact]
    public async Task UnarchiveRecord_RecordFromDifferentProject_ThrowsKeyNotFoundException()
    {
        var differentProjectId = 999L;
        var recordId = 100L;

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _recordBusiness.UnarchiveRecord(differentProjectId, recordId));
    }

    [Fact]
    public async Task UnarchiveRecord_AlreadyUnarchived_ThrowsKeyNotFoundException()
    {
        var projectId = 100L;
        var recordId = 100L;

        // Confirm record is not archived
        var existing = await Context.Records.FindAsync(recordId);
        existing.ArchivedAt = null;
        await Context.SaveChangesAsync();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _recordBusiness.UnarchiveRecord(projectId, recordId));
    }

    #endregion
    
    #region Attach/Unattach Tag Tests

    [Fact]
    public async Task AttachTag_SuccessfullyAttachesTagToRecord()
    {
        var projectId = 100L;

        var newTag = new Tag
        {
            Id = 101,
            Name = "Tag to Attach",
            ProjectId = projectId,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        Context.Tags.Add(newTag);

        var record = await Context.Records.Include(r => r.Tags).FirstAsync(r => r.Id == 100);
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
        var projectId = 100L;
        var validTagId = 100L;

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _recordBusiness.AttachTag(projectId, 9999L, validTagId));
    }

    [Fact]
    public async Task AttachTag_TagNotFound_ThrowsKeyNotFound()
    {
        var projectId = 100L;
        var validRecordId = 100L;

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _recordBusiness.AttachTag(projectId, validRecordId, 9999L));
    }

    [Fact]
    public async Task AttachTag_AlreadyAttached_ThrowsException()
    {
        var projectId = 100L;
        var recordId = 100L;
        var tagId = 100L;

        await Assert.ThrowsAsync<Exception>(() =>
            _recordBusiness.AttachTag(projectId, recordId, tagId));
    }

    [Fact]
    public async Task UnattachTag_SuccessfullyDetachesTagFromRecord()
    {
        var projectId = 100L;
        var record = await Context.Records.Include(r => r.Tags).FirstAsync(r => r.Id == 100L);
        var tagId = 100L;
        Assert.Contains(record.Tags, t => t.Id == tagId);

        var result = await _recordBusiness.UnattachTag(projectId, record.Id, tagId);

        Assert.True(result);
        var refreshed = await Context.Records.Include(r => r.Tags).FirstAsync(r => r.Id == record.Id);
        Assert.DoesNotContain(refreshed.Tags, t => t.Id == tagId);
    }

    [Fact]
    public async Task UnattachTag_RecordNotFound_ThrowsKeyNotFound()
    {
        var projectId = 100L;
        var tagId = 100L;

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _recordBusiness.UnattachTag(projectId, 9999L, tagId));
    }

    [Fact]
    public async Task UnattachTag_TagNotFound_ThrowsKeyNotFound()
    {
        var projectId = 100L;
        var recordId = 100L;

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _recordBusiness.UnattachTag(projectId, recordId, 9999L));
    }

    #endregion

    
}