using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.helpers.Hubs;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Record = deeplynx.datalayer.Models.Record;

namespace deeplynx.tests;

[Collection("Test Suite Collection")]
public class RecordBusinessTests : IntegrationTestBase
{
    private RecordBusiness _recordBusiness;
    private EventBusiness _eventBusiness;
    private INotificationBusiness _notificationBusiness = null!;
    private Mock<ILogger<NotificationBusiness>> _mockNotificationLogger = null!;
    private Mock<IHubContext<EventNotificationHub>> _mockHubContext = null!;
    public long pid; // project ID
    public long pid2;
    public long did; // datasource ID
    public long cid; // class ID
    public long tid; // tag ID
    public long osid; // object storage ID
    public long rid; // record ID
    public string rprop; // additional record props
    public string rogid;
    public string rdesc;
    public string ruri;
    public string rfiletype;

    public RecordBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _mockHubContext = new Mock<IHubContext<EventNotificationHub>>();
        _mockNotificationLogger = new Mock<ILogger<NotificationBusiness>>();
        _notificationBusiness = new NotificationBusiness(Context, _mockNotificationLogger.Object, _mockHubContext.Object);
        _eventBusiness = new EventBusiness(Context, _cacheBusiness, _notificationBusiness);
        _recordBusiness = new RecordBusiness(Context, _cacheBusiness, _eventBusiness);
    }
    
    #region GetAllRecords Tests

    [Fact]
    public async Task GetAllRecords_ValidProjectId_ReturnsRecords()
    {
        // Act
        var result = await _recordBusiness.GetAllRecords(pid, null, true);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Test Record", result.First().Name);
    }
    
    [Fact]
    public async Task GetAllRecords_ReturnsTags()
    {
        // Act
        var result = await _recordBusiness.GetAllRecords(pid, null, true);

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
        // Act
        var result = await _recordBusiness.GetAllRecords(pid, did, true);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(did, result.First().DataSourceId);
    }
    
    [Fact]
    public async Task GetAllRecords_WithFileType_ReturnsFilteredRecords()
    {
        // Arrange - Make sure incorrect fileType filter results in no results (we only have 1 record seeded and its of pdf type)
        var incorrectFileTypeResponse = await _recordBusiness.GetAllRecords(pid, did, true, "png");
        Assert.Empty(incorrectFileTypeResponse);
        
        // Act
        var correctFileTypeResponse = await _recordBusiness.GetAllRecords(pid, did, true, "pdf");
        
        // Assert
        Assert.NotNull(correctFileTypeResponse);
        Assert.Single(correctFileTypeResponse);
        Assert.Equal("pdf", correctFileTypeResponse.First().FileType);
    }
    
    [Fact]
    public async Task GetAllRecords_InvalidProjectId_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _recordBusiness.GetAllRecords(999L, null, true));

        Assert.Contains("Project with id 999 not found.", exception.Message);
    }

    #endregion

    #region GetRecord Tests

    [Fact]
    public async Task GetRecord_ValidIds_ReturnsRecord()
    {
        // Act
        var result = await _recordBusiness.GetRecord(pid, rid, true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(rid, result.Id);
        Assert.Equal("Test Record", result.Name);
    }

    [Fact]
    public async Task GetRecord_InvalidProjectId_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _recordBusiness.GetRecord(999L, rid, true));
        
        Assert.Contains("Project with id 999 not found.", exception.Message);
    }

    #endregion

    #region CreateRecord Tests

    [Fact]
    public async Task CreateRecord_ValidData_CreatesRecord()
    {
        // Arrange
        var dto = new CreateRecordRequestDto
        {
            Name = "New Test Record",
            Description = "Test Record Description",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { TestProp = "TestValue" }))!,
            Uri = "test://uri",
            OriginalId = "original-123",
            ClassId = cid,
            FileType = "png"
        };

        // Act
        var result = await _recordBusiness.CreateRecord(pid, did, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Test Record", result.Name);
        Assert.Equal("Test Record Description", result.Description);
        Assert.Equal(pid, result.ProjectId);
        Assert.Equal(did, result.DataSourceId);
        Assert.Equal("test://uri", result.Uri);
        Assert.Equal("original-123", result.OriginalId);
        Assert.Equal(cid, result.ClassId);
        Assert.Equal("png", result.FileType);

        // Verify record was actually created in database
        var createdRecord = await Context.Records.FindAsync(result.Id);
        Assert.NotNull(createdRecord);
        Assert.Equal("New Test Record", createdRecord.Name);
        
        // Ensure that record create event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Single(eventList);

        var actualEvent = eventList[0];
        
        Assert.Equal(createdRecord.ProjectId, actualEvent.ProjectId);
        Assert.Equal("create", actualEvent.Operation);
        Assert.Equal("record", actualEvent.EntityType);
        Assert.Equal(createdRecord.Id, actualEvent.EntityId);
    }

    [Fact]
    public async Task CreateRecord_InvalidProjectId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var dto = new CreateRecordRequestDto
        {
            Name = "Test Record",
            Description = "Test Record Description",
            OriginalId = "original-123",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { TestProp = "TestValue" }))!,
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _recordBusiness.CreateRecord(1000999L, did, dto));
        
        Assert.Contains("Project with id 1000999 not found.", exception.Message);
        
        // Ensure that no record create event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    [Fact]
    public async Task CreateRecord_InvalidDataSourceId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var dto = new CreateRecordRequestDto
        {
            Name = "Test Record",
            Description = "Test Record Description",
            OriginalId = "original-123",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { TestProp = "TestValue" }))!,
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _recordBusiness.CreateRecord(pid, 999L, dto));
        
        Assert.Contains($"DataSource with id 999 not found in project with id {pid}", exception.Message);
        
        // Ensure that no record create event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    [Fact]
    public async Task CreateRecord_TooDeepJson_ThrowsException()
    {
        // Arrange
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
            _recordBusiness.CreateRecord(pid, did, dto));
        Assert.Contains("depth of the JSON structure exceeds", exception.Message);
        
        // Ensure that no record create event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    [Fact]
    public async Task CreateRecord_WithInvalidDataSource_ThrowsException()
    {
        var dataSourceInWrongProject = new DataSource
        {
            Name = "Test Data Source",
            Description = "Test data source for unit tests",
            ProjectId = pid2,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        Context.DataSources.Add(dataSourceInWrongProject);
        await Context.SaveChangesAsync(); 
        
        var dto = new CreateRecordRequestDto
        {
            Name = "Invalid Record",
            Description = "Invalid Record Description",
            OriginalId = "original-12334532",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { TestProp = "TestValue" }))!
        };
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _recordBusiness.CreateRecord(pid, dataSourceInWrongProject.Id, dto));
        
        Assert.Contains($"DataSource with id {dataSourceInWrongProject.Id} not found in project with id {pid}", exception.Message);
        
        // Ensure that no record create event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }
    
    #endregion

    #region BulkCreateRecords Tests

    [Fact]
    public async Task BulkCreateRecords_ValidData_CreatesMultipleRecords()
    {
        // Arrange
        List<CreateRecordRequestDto> records = new List<CreateRecordRequestDto>
        {
            new CreateRecordRequestDto
            {
                Name = "Bulk Record 1",
                Description = "Bulk Record 1 Description",
                ObjectStorageId = osid,
                OriginalId = "br1",
                Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { TestProp = "Value1" }))!
            },
            new CreateRecordRequestDto
            {
                Name = "Bulk Record 2",
                Description = "Bulk Record 2 Description",
                ObjectStorageId = osid,
                OriginalId = "br2",
                Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { TestProp = "Value2" }))!
            }
        };

        // Act
        var result = await _recordBusiness.BulkCreateRecords(pid, did, records);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, r => r.Name == "Bulk Record 1");
        Assert.Contains(result, r => r.Name == "Bulk Record 2");

        // Verify records were actually created in database
        var recordCount = await Context.Records.CountAsync(r => r.ProjectId == pid);
        Assert.Equal(3, recordCount); // 1 from seed + 2 new
        
        // Ensure that a record create event was logged for each record
        var eventList = await Context.Events.ToListAsync();
        Assert.Equal(2, eventList.Count);

        var firstEvent = eventList[0];
        Assert.Equal(pid, firstEvent.ProjectId);
        Assert.Equal(result[0].Id, firstEvent.EntityId);
        Assert.Equal("record", firstEvent.EntityType);
        Assert.Equal("create", firstEvent.Operation);
        Assert.Equal(result[0].DataSourceId, firstEvent.DataSourceId);

        var secondEvent = eventList[1];
        Assert.Equal(pid, secondEvent.ProjectId);
        Assert.Equal(result[1].Id, secondEvent.EntityId);
        Assert.Equal("record", secondEvent.EntityType);
        Assert.Equal("create", secondEvent.Operation);
        Assert.Equal(result[1].DataSourceId, secondEvent.DataSourceId);
    }

    [Fact]
    public async Task BulkCreateRecords_EmptyList_ThrowsException()
    {
        // Arrange
        List<CreateRecordRequestDto> records = new List<CreateRecordRequestDto>();
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _recordBusiness.BulkCreateRecords(pid, did, records));

        Assert.Contains("Unable to bulk create records: no records selected for creation", exception.Message);
        
        // Ensure that no record create event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    [Fact]
    public async Task BulkCreateRecords_InvalidProjectId_ThrowsKeyNotFoundException()
    {
        // Arrange
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
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _recordBusiness.BulkCreateRecords(999L, 1L, records));
        
        Assert.Contains("Project with id 999 not found.",  exception.Message);
        
        // Ensure that no record create event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    #endregion

    #region UpdateRecord Tests

    [Fact]
    public async Task UpdateRecord_ValidData_UpdatesRecord()
    {
        // Arrange
        var dto = new UpdateRecordRequestDto
        {
            Name = "Updated Test Record",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { UpdatedProp = "UpdatedValue" }))!,
            Uri = "updated://uri",
            OriginalId = "updated-123",
            Description = "Updated Description",
            ClassId = cid,
            FileType = "png"
        };

        // Act
        var result = await _recordBusiness.UpdateRecord(pid, rid, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Test Record", result.Name);
        Assert.Equal("updated://uri", result.Uri);
        Assert.Equal("updated-123", result.OriginalId);
        Assert.Equal("Updated Description", result.Description);
        Assert.Equal("png", result.FileType);

        // Verify record was actually updated in database
        var updatedRecord = await Context.Records.FindAsync(rid);
        Assert.NotNull(updatedRecord);
        Assert.Equal("Updated Test Record", updatedRecord.Name);
        
        // Verify that get function gets updated version
        var getResult = await _recordBusiness.GetRecord(pid, rid, true);
        Assert.NotNull(getResult);
        Assert.Equal("Updated Test Record", getResult.Name);
        Assert.Equal("Updated Description", getResult.Description);
        Assert.Equal("png", getResult.FileType);
        Assert.NotNull(getResult.LastUpdatedAt);
        
        // Ensure that a record update event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Single(eventList);

        var actualEvent = eventList[0];
        
        Assert.Equal(pid, actualEvent.ProjectId);
        Assert.Equal(result.Id, actualEvent.EntityId);
        Assert.Equal("record", actualEvent.EntityType);
        Assert.Equal("update", actualEvent.Operation);
        Assert.Equal(result.DataSourceId, actualEvent.DataSourceId);
    }

    [Fact]
    public async Task UpdateRecord_PartialUpdate_UpdatesRecord()
    {
        // Arrange
        var dto = new UpdateRecordRequestDto
        {
            Name = "New-ish Test Record"
        };

        // Act
        var result = await _recordBusiness.UpdateRecord(pid, rid, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New-ish Test Record", result.Name);
        Assert.Equal(ruri, result.Uri);
        Assert.Equal(rogid, result.OriginalId);
        Assert.Equal(rdesc, result.Description);
        Assert.Equal(rprop, result.Properties);
        Assert.Equal(rfiletype, result.FileType);

        // Verify record was actually updated in database
        var updatedRecord = await Context.Records.FindAsync(rid);
        Assert.NotNull(updatedRecord);
        Assert.Equal("New-ish Test Record", updatedRecord.Name);
        Assert.Equal(rdesc, updatedRecord.Description);
        
        // Verify that get function gets updated version
        var getResult = await _recordBusiness.GetRecord(pid, rid, true);
        Assert.NotNull(getResult);
        Assert.Equal("New-ish Test Record", getResult.Name);
        Assert.Equal(rdesc, getResult.Description);
        Assert.NotNull(getResult.LastUpdatedAt);
        
        // Ensure that a record update event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Single(eventList);

        var actualEvent = eventList[0];
        Assert.Equal(pid, actualEvent.ProjectId);
        Assert.Equal(result.Id, actualEvent.EntityId);
        Assert.Equal("record", actualEvent.EntityType);
        Assert.Equal("update", actualEvent.Operation);
        Assert.Equal(result.DataSourceId, actualEvent.DataSourceId);
    }
    
    [Fact]
    public async Task UpdateRecord_InvalidRecordId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var dto = new UpdateRecordRequestDto
        {
            Name = "Updated Record",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { TestProp = "TestValue" }))!
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _recordBusiness.UpdateRecord(pid, 999L, dto));
        
        Assert.Contains("Record with id 999 not found", exception.Message);
        
        // Ensure that no record create event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    [Fact]
    public async Task UpdateRecord_RecordFromDifferentProject_ThrowsKeyNotFoundException()
    {
        // Arrange
        var dto = new UpdateRecordRequestDto
        {
            Name = "Updated Record",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { TestProp = "TestValue" }))!
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _recordBusiness.UpdateRecord(999L, rid, dto));

        Assert.Contains("Project with id 999 not found.", exception.Message);
        
        // Ensure that no record create event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    [Fact]
    public async Task UpdateRecord_TooDeepJson_ThrowsException()
    {
        // Arrange
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
            _recordBusiness.UpdateRecord(pid, rid, dto));
        Assert.Contains("depth of the JSON structure exceeds", exception.Message);
        
        // Ensure that no record create event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    #endregion

    #region DeleteRecord Tests

    [Fact]
    public async Task DeleteRecord_ValidData_DeletesRecord()
    {
        // Arrange - Verify record exists before deletion
        var recordExists = await Context.Records.AnyAsync(r => r.Id == rid);
        Assert.True(recordExists);

        // Act
        var result = await _recordBusiness.DeleteRecord(pid, rid);

        // Assert
        Assert.True(result);

        // Verify record was actually deleted from database
        var deletedRecord = await Context.Records.FindAsync(rid);
        Assert.Null(deletedRecord);
    }

    [Fact]
    public async Task DeleteRecord_InvalidRecordId_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _recordBusiness.DeleteRecord(pid, 999L));
        
        Assert.Contains("Record with id 999 not found", exception.Message);
    }

    [Fact]
    public async Task DeleteRecord_RecordFromDifferentProject_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _recordBusiness.DeleteRecord(999L, rid));
        
        Assert.Contains("Project with id 999 not found.", exception.Message);
    }

    #endregion

    #region ArchiveRecord Tests

    [Fact]
    public async Task ArchiveRecord_InvalidRecordId_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _recordBusiness.ArchiveRecord(pid, 999L));
        
        Assert.Contains("Record with id 999 not found", exception.Message);
        
        // Ensure that no record soft delete event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    [Fact]
    public async Task ArchiveRecord_RecordFromDifferentProject_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _recordBusiness.ArchiveRecord(999L, rid));
        
        Assert.Contains("Project with id 999 not found.", exception.Message);
        
        // Ensure that no record soft delete event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    [Fact]
    public async Task ArchiveRecord_AlreadyArchivedRecord_ThrowsKeyNotFoundException()
    {
        // Arrange - First archive the record
        var record = await Context.Records.FindAsync(rid);
        record.IsArchived = true;
        await Context.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _recordBusiness.ArchiveRecord(pid, rid));
        
        Assert.Contains($"Record with id {rid} not found", exception.Message);
        
        // Ensure that no record soft delete event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    #endregion

    #region Edge Cases and Integration Tests

    [Fact]
    public async Task CreateRecord_ValidJsonDepthThree_Success()
    {
        // Arrange
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
            ObjectStorageId = osid,
            OriginalId = "VDR1",
            Properties = validDepthJson
        };

        // Act
        var result = await _recordBusiness.CreateRecord(pid, did, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Valid Depth Record", result.Name);
        
        // Ensure that record create event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Single(eventList);

        var actualEvent = eventList[0];
        
        Assert.Equal(result.ProjectId, actualEvent.ProjectId);
        Assert.Equal("create", actualEvent.Operation);
        Assert.Equal("record", actualEvent.EntityType);
        Assert.Equal(result.Id, actualEvent.EntityId);
    }

    [Fact]
    public async Task CreateRecord_NullProperties_ThrowsException()
    {
        // Arrange
        var dto = new CreateRecordRequestDto
        {
            Name = "No Properties Record",
            Description = "No Properties Description",
            OriginalId = "NoProps",
            Properties = null
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _recordBusiness.CreateRecord(pid, did, dto));
        
        // Ensure that no record create event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }
    
    [Fact]
    public async Task CreateRecord_NoName_ThrowsException()
    {
        // Arrange
        var dto = new CreateRecordRequestDto
        {
            Name = null,
            Description = "No Name Description",
            OriginalId = "NoName",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { TestProp = "TestValue" }))!,
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _recordBusiness.CreateRecord(pid, did, dto));
        
        // Ensure that no record create event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    [Fact]
    public async Task CreateRecord_NoDescription_ThrowsException()
    {
        // Arrange
        var dto = new CreateRecordRequestDto
        {
            Name = "No Description Record",
            Description = null,
            OriginalId = "NoDesc",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { TestProp = "TestValue" }))!,
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _recordBusiness.CreateRecord(pid, did, dto));
        
        // Ensure that no record create event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    [Fact]
    public async Task CreateRecord_NoOriginalId_ThrowsException()
    {
        // Arrange
        var dto = new CreateRecordRequestDto
        {
            Name = "No Original ID Record",
            Description = "No Original ID Description",
            OriginalId = null,
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { TestProp = "TestValue" }))!,
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _recordBusiness.CreateRecord(pid, did, dto));
        
        // Ensure that no record create event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    #endregion
    
    #region UnarchiveRecord Tests

    [Fact]
    public async Task UnarchiveRecord_ValidArchivedRecord_UnarchivesSuccessfully()
    {
        // Arrange
        var record = await Context.Records.FindAsync(rid);
        record.IsArchived = true;
        await Context.SaveChangesAsync();

        // Act
        var result = await _recordBusiness.UnarchiveRecord(pid, rid);
    
        //this forces EF to sync to db on next query
        Context.ChangeTracker.Clear();

        // Assert
        Assert.True(result);
        var reloaded = await Context.Records.FindAsync(rid);
        Assert.NotNull(reloaded);
        Assert.Equal("Test Record", reloaded.Name);
        Assert.False(reloaded.IsArchived); 
        
        // Ensure that the record unarchive event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Single(eventList);
            
        var actualEvent = eventList[0];
            
        Assert.Equal("unarchive", actualEvent.Operation);
        Assert.Equal("record", actualEvent.EntityType);
        Assert.Equal(rid, actualEvent.EntityId);
    }

    [Fact]
    public async Task UnarchiveRecord_InvalidRecordId_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _recordBusiness.UnarchiveRecord(pid, 999L));
        
        Assert.Contains("Record with id 999 not found", exception.Message);

        // Ensure that no event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    [Fact]
    public async Task UnarchiveRecord_RecordFromDifferentProject_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _recordBusiness.UnarchiveRecord(999L, rid));
        
        Assert.Contains("Project with id 999 not found.", exception.Message);
        
        // Ensure that no event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    [Fact]
    public async Task UnarchiveRecord_AlreadyUnarchived_ThrowsKeyNotFoundException()
    {
        // Arrange - Confirm record is not archived
        var existing = await Context.Records.FindAsync(rid);
        existing.IsArchived = false;
        await Context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _recordBusiness.UnarchiveRecord(pid, rid));
        
        // Ensure that no event was logged
        var eventList = await Context.Events.ToListAsync();
        Assert.Empty(eventList);
    }

    #endregion
    
    #region Attach/Unattach Tag Tests

    [Fact]
    public async Task AttachTag_SuccessfullyAttachesTagToRecord()
    {
        // Arrange
        var newTag = new Tag
        {
            Name = "Tag to Attach",
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        Context.Tags.Add(newTag);

        var record = await Context.Records.Include(r => r.Tags).FirstAsync(r => r.Id == rid);
        record.Tags.Clear(); // ensure tag not already attached
        await Context.SaveChangesAsync();

        // Act
        var result = await _recordBusiness.AttachTag(pid, record.Id, newTag.Id);

        // Assert
        Assert.True(result);
        var updatedRecord = await Context.Records.Include(r => r.Tags).FirstAsync(r => r.Id == record.Id);
        Assert.Contains(updatedRecord.Tags, t => t.Id == newTag.Id);
    }

    [Fact]
    public async Task AttachTag_RecordNotFound_ThrowsKeyNotFound()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _recordBusiness.AttachTag(pid, 9999L, tid));
        
        Assert.Contains("Record with id 9999 not found", exception.Message);
    }

    [Fact]
    public async Task AttachTag_TagNotFound_ThrowsKeyNotFound()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _recordBusiness.AttachTag(pid, rid, 9999L));
        
        Assert.Contains("Tag with id 9999 not found or is archived.", exception.Message);
    }

    [Fact]
    public async Task AttachTag_AlreadyAttached_ThrowsException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _recordBusiness.AttachTag(pid, rid, tid));

        Assert.Contains($"Tag with id {tid} is already attached to record {rid}", exception.Message);
    }

    [Fact]
    public async Task UnattachTag_SuccessfullyDetachesTagFromRecord()
    {
        // Arrange
        var record = await Context.Records.Include(r => r.Tags).FirstAsync(r => r.Id == rid);
        Assert.Contains(record.Tags, t => t.Id == tid);

        // Act
        var result = await _recordBusiness.UnattachTag(pid, record.Id, tid);

        // Assert
        Assert.True(result);
        var refreshed = await Context.Records.Include(r => r.Tags).FirstAsync(r => r.Id == record.Id);
        Assert.DoesNotContain(refreshed.Tags, t => t.Id == tid);
    }

    [Fact]
    public async Task UnattachTag_RecordNotFound_ThrowsKeyNotFound()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _recordBusiness.UnattachTag(pid, 9999L, tid));
        
        Assert.Contains("Record with id 9999 not found or is archived.", exception.Message);
    }

    [Fact]
    public async Task UnattachTag_TagNotFound_ThrowsKeyNotFound()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _recordBusiness.UnattachTag(pid, rid, 9999L));
        
        Assert.Contains("Tag with id 9999 not found or is archived.", exception.Message);
    }

    #endregion
    
    #region GetRecordsByOriginalId Tests

    [Fact]
    public async Task GetRecordsByOriginalId_ValidOriginalIds_ReturnsMatchingRecords()
    {
        // Act
        var result = await _recordBusiness.GetRecordsByOriginalId(pid, ["og_id"]);

        // Assert
        Assert.Equal(1, result.Count);
        Assert.Equal("og_id", result.First().OriginalId);
        Assert.Equal(pid, result.First().ProjectId);
    }

    [Fact]
    public async Task GetRecordsByOriginalId_MissingOriginalIds_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _recordBusiness.GetRecordsByOriginalId(pid, ["non-existent-id"]));
        
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
        var record = await Context.Records.FindAsync(rid);
        record.IsArchived = true;
        await Context.SaveChangesAsync();
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _recordBusiness.GetRecordsByOriginalId(pid, ["og_id"]));
        
        Assert.Contains("og_id", exception.Message);
    }

    [Fact]
    public async Task GetRecordsByOriginalId_InvalidProjectId_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _recordBusiness.GetRecordsByOriginalId(999L, ["some-id"]));
        
        Assert.Contains("Project with id 999 not found.",  exception.Message);
    }

    #endregion
        
    protected override async Task SeedTestDataAsync()
    {
        await base.SeedTestDataAsync();
        
        // Add projects
        var project = new Project
        {
            Name = "Test Project",
            Description = "Test project for unit tests",
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        var project2 = new Project
        {
            Name = "Test Project 2",
            Description = "Test project 2 for unit tests",
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        Context.Projects.Add(project);
        Context.Projects.Add(project2);
        await Context.SaveChangesAsync();
        pid = project.Id;
        pid2 = project2.Id;
        
        // Add datasource
        var dataSource = new DataSource
        {
            Name = "Test Data Source",
            Description = "Test data source for unit tests",
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        Context.DataSources.Add(dataSource);
        await Context.SaveChangesAsync();
        did = dataSource.Id;
        
        
        // Add class
        var testClass = new Class
        {
            Name = "Test Class",
            Description = "Test class for unit tests",
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        Context.Classes.Add(testClass);
        await Context.SaveChangesAsync();
        cid = testClass.Id;
        
        // Add object storage
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
        osid = objectStorage.Id;

        // Add tag
        var testTag = new Tag
        {
            Name = "Test Tag",
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        
        // Add record
        var testRecord = new Record
        {
            Name = "Test Record",
            Description = "Test record for unit tests",
            OriginalId = "og_id",
            Properties = JsonSerializer.Serialize(new { TestProperty = "TestValue" }),
            ProjectId = pid,
            DataSourceId = dataSource.Id,
            ClassId = testClass.Id,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            Tags =  new List<Tag> { testTag },
            Uri = "localhost:8090",
            FileType = "pdf"
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
        rfiletype = testRecord.FileType;
    }
}