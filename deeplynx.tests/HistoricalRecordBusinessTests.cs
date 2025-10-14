using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.helpers.exceptions;
using deeplynx.helpers.Hubs;
using deeplynx.interfaces;
using deeplynx.models;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Record = deeplynx.datalayer.Models.Record;

namespace deeplynx.tests;

[Collection("Test Suite Collection")]
public class HistoricalRecordBusinessTests: IntegrationTestBase
{
    public long pid;
    public long pid2;
    public long did;
    public long did2;
    public long cid;
    public long rid;
    public long rid2;
    public long os1;
    private HistoricalRecordBusiness _historicalRecordBusiness = null!;
    private RecordBusiness _recordBusiness = null!;
    private EventBusiness _eventBusiness;
    private INotificationBusiness _notificationBusiness = null!;
    private Mock<ILogger<NotificationBusiness>> _mockNotificationLogger = null!;
    private Mock<IHubContext<EventNotificationHub>> _mockHubContext = null!;
    
    public HistoricalRecordBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _historicalRecordBusiness = new HistoricalRecordBusiness(Context);
        _mockHubContext = new Mock<IHubContext<EventNotificationHub>>();
        _mockNotificationLogger = new Mock<ILogger<NotificationBusiness>>();
        _notificationBusiness = new NotificationBusiness(Context, _mockNotificationLogger.Object, _mockHubContext.Object);
        _eventBusiness = new EventBusiness(Context, _cacheBusiness, _notificationBusiness);
        _recordBusiness = new RecordBusiness(Context, _cacheBusiness, _eventBusiness);
    }

    [Fact]
    public async Task GetHistoricalRecords_ReturnsListOfCurrentHistoricalRecordsForProject()
    {
        await SeedTestDataAsync();
        var historicalRecords = await _historicalRecordBusiness.GetAllHistoricalRecords(pid);
        historicalRecords.Should().NotBeNull();
        historicalRecords.Should().HaveCount(2);
        historicalRecords.First().Name.Should().Be("Test Record");
        historicalRecords.Last().Name.Should().Be("Test Record 2");
        historicalRecords.Should().NotContain(x => x.Name == "Test Record 3");
        historicalRecords.Should().NotContain(x => x.Name == "Test Record 4");

    }
    
    [Fact]
    public async Task GetHistoricalRecords_ReturnsListOfUpdatedHistoricalRecords()
    {
        var dto = new UpdateRecordRequestDto
        {
            Name = "Updated Test Record",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { UpdatedProp = "UpdatedValue" }))!,
            Uri = "updated://uri",
            OriginalId = "updated-123",
            Description = "Updated Description",
            ClassId = cid
        };
        
        var dto2 = new UpdateRecordRequestDto
        {
            Name = "Updated Test Record 2",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { UpdatedProp = "UpdatedValue 2" }))!,
            Uri = "updated2://uri",
            OriginalId = "updated2-123",
            Description = "Updated 2 Description",
            ClassId = cid
        };

        // Act
        await _recordBusiness.UpdateRecord(pid, rid, dto);
        await _recordBusiness.UpdateRecord(pid, rid2, dto2);

        var historicalRecords = await _historicalRecordBusiness.GetAllHistoricalRecords(pid);
        historicalRecords.Should().NotBeNull();
        historicalRecords.Should().HaveCount(2);
        historicalRecords.First().Name.Should().Be("Updated Test Record");
        historicalRecords.Last().Name.Should().Be("Updated Test Record 2");
    }
    
    [Fact]
    public async Task GetHistoricalRecords_ContainsArchivedHistoricalRecords()
    {
        await _recordBusiness.ArchiveRecord(pid, rid);
        
        var historicalRecords = await _historicalRecordBusiness.GetAllHistoricalRecords(pid, null, null, false);
        historicalRecords.Should().NotBeNull();
        historicalRecords.Should().HaveCount(2);
        historicalRecords.Should().Contain(x => x.Name == "Test Record");
        historicalRecords.Should().Contain(x => x.Name == "Test Record 2");
    }
    
    [Fact]
    public async Task GetHistoricalRecords_DoesNotContainArchivedHistoricalRecords()
    {
        var historicalRecords = await _historicalRecordBusiness.GetAllHistoricalRecords(pid);
        historicalRecords.Should().NotBeNull();
        historicalRecords.Should().HaveCount(2);
        
        await _recordBusiness.ArchiveRecord(pid, rid);
        
        var arcHistoricalRecords = await _historicalRecordBusiness.GetAllHistoricalRecords(pid);
        arcHistoricalRecords.Should().NotBeNull();
        arcHistoricalRecords.Should().HaveCount(1);
        arcHistoricalRecords.Should().NotContain(x => x.Name == "Test Record");
        arcHistoricalRecords.Should().Contain(x => x.Name == "Test Record 2");
    }
    
    [Fact]
    public async Task GetHistoricalRecords_ReturnsEmptyListWhenNoRecords()
    {
        await _recordBusiness.DeleteRecord(pid, rid);
        await _recordBusiness.DeleteRecord(pid, rid2);
        
        var historicalRecords = await _historicalRecordBusiness.GetAllHistoricalRecords(pid);
        historicalRecords.Should().NotBeNull();
        historicalRecords.Should().HaveCount(0);
    }
    
    [Fact]
    public async Task GetHistoricalRecords_FiltersByDataSource()
    {
        var historicalRecords = await _historicalRecordBusiness.GetAllHistoricalRecords(pid2, did2);
        historicalRecords.Should().NotBeNull();
        historicalRecords.Should().HaveCount(1);
        historicalRecords.Should().Contain(x => x.Name == "Test Record 3");
        historicalRecords.Should().NotContain(x => x.Name == "Test Record 4");
    }
    
    [Fact]
    public async Task GetHistoricalRecords_FiltersByTime()
    {
        var pointInTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        
        var testRecordLate = new Record
        {
            Name = "Test Record Late",
            Description = "Test record late for unit tests",
            OriginalId = "og_idlate",
            Properties = JsonSerializer.Serialize(new { TestProperty = "TestValue late" }),
            ProjectId = pid,
            DataSourceId = did,
            ClassId = cid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            Uri = "localhost:8090"
        };
        
        Context.Records.Add(testRecordLate);
        await Context.SaveChangesAsync();
        
        var historicalRecords = await _historicalRecordBusiness.GetAllHistoricalRecords(pid, null, pointInTime, false);
        historicalRecords.Should().NotBeNull();
        historicalRecords.Should().HaveCount(2);
        historicalRecords.Should().Contain(x => x.Name == "Test Record");
        historicalRecords.Should().Contain(x => x.Name == "Test Record 2");
    }

    [Fact]
    public async Task GetHistoryForRecord_ReturnsFullHistory()
    {
        var dto = new UpdateRecordRequestDto
        {
            Name = "Updated Test Record",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { UpdatedProp = "UpdatedValue" }))!,
            Uri = "updated://uri",
            OriginalId = "updated-123",
            Description = "Updated Description",
            ClassId = cid
        };
        
        await _recordBusiness.UpdateRecord(pid, rid, dto);
        await _recordBusiness.ArchiveRecord(pid, rid);

        var recordHistory = await _historicalRecordBusiness.GetHistoryForRecord(rid);
        
        recordHistory.Should().NotBeNull();
        recordHistory.Should().HaveCount(4);
        recordHistory.Should().Contain(x => x.Name == "Test Record" && x.Tags == null);
        recordHistory.Should().Contain(x => x.Name == "Test Record" && x.Tags != null);
        recordHistory.Should().Contain(x => x.Name == "Updated Test Record" && !x.IsArchived );
        recordHistory.Should().Contain(x => x.Name == "Updated Test Record" && x.IsArchived);
    }

    [Fact]
    public async Task GetHistoryForRecord_ThrowsError_WhenRecordDoesNotExist()
    {
        var historicalRecords = () => _historicalRecordBusiness.GetHistoryForRecord(rid + 100000);
        await historicalRecords.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    [Fact]
    public async Task GetHistoricalRecord_ReturnsAllCorrectFields()
    {
        // TODO: insert tags after record to avoid race condition
        var record = await Context.Records.Where(r => r.ProjectId == pid && r.Id == rid).FirstOrDefaultAsync();
        record.Should().NotBeNull();
        
        var historicalRecord = await _historicalRecordBusiness.GetHistoricalRecord(rid, null);
        historicalRecord.Should().NotBeNull();
        historicalRecord.Name.Should().Be(record.Name);
        historicalRecord.Id.Should().Be(record.Id);
        historicalRecord.Tags.Should().NotBeNull();
        historicalRecord.ClassId.Should().Be(record.ClassId);
        historicalRecord.ClassName.Should().Be("Test Class");
        historicalRecord.Description.Should().Be(record.Description);
        historicalRecord.OriginalId.Should().Be(record.OriginalId);
        historicalRecord.Uri.Should().Be(record.Uri);
        historicalRecord.ObjectStorageId.Should().Be(record.ObjectStorageId);
        historicalRecord.ObjectStorageName.Should().Be("Object Storage 1");
        historicalRecord.ProjectId.Should().Be(record.ProjectId);
        historicalRecord.ProjectName.Should().Be("Test Project");
        historicalRecord.DataSourceId.Should().Be(record.DataSourceId);
        historicalRecord.DataSourceName.Should().Be("Test Data Source");
    }
    
    [Fact]
    public async Task GetHistoricalRecord_ReturnsAllCorrectFields_AfterUpdate()
    {
        var dto = new UpdateRecordRequestDto
        {
            Name = "Updated Test Record",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { UpdatedProp = "UpdatedValue" }))!,
            Uri = "updated://uri",
            OriginalId = "updated-123",
            Description = "Updated Description",
            ClassId = cid
        };
        
        var updatedRecord = await _recordBusiness.UpdateRecord(pid, rid, dto);
        updatedRecord.Should().NotBeNull();
        
        var historicalRecord = await _historicalRecordBusiness.GetHistoricalRecord(rid, null);
        historicalRecord.Should().NotBeNull();
        historicalRecord.Name.Should().Be(updatedRecord.Name);
        historicalRecord.Id.Should().Be(updatedRecord.Id);
        historicalRecord.Tags.Should().NotBeNull();
        historicalRecord.ClassId.Should().Be(updatedRecord.ClassId);
        historicalRecord.ClassName.Should().Be("Test Class");
        historicalRecord.Description.Should().Be(updatedRecord.Description);
        historicalRecord.OriginalId.Should().Be(updatedRecord.OriginalId);
        historicalRecord.Uri.Should().Be(updatedRecord.Uri);
        historicalRecord.ObjectStorageId.Should().Be(updatedRecord.ObjectStorageId);
        historicalRecord.ObjectStorageName.Should().Be("Object Storage 1");
        historicalRecord.ProjectId.Should().Be(updatedRecord.ProjectId);
        historicalRecord.ProjectName.Should().Be("Test Project");
        historicalRecord.DataSourceId.Should().Be(updatedRecord.DataSourceId);
        historicalRecord.DataSourceName.Should().Be("Test Data Source");
    }
    
    [Fact]
    public async Task GetHistoricalRecord_ReturnsAllCorrectFields_AfterArchive()
    {
        var archived = await _recordBusiness.ArchiveRecord(pid, rid);
        archived.Should().BeTrue();

        var archivedRecord = await _recordBusiness.GetRecord(pid, rid, false);
        archivedRecord.Should().NotBeNull();
        
        var historicalRecord = await _historicalRecordBusiness.GetHistoricalRecord(rid, null, false);
        historicalRecord.Should().NotBeNull();
        historicalRecord.Name.Should().Be(archivedRecord.Name);
        historicalRecord.Id.Should().Be(archivedRecord.Id);
        historicalRecord.Tags.Should().NotBeNull();
        historicalRecord.ClassId.Should().Be(archivedRecord.ClassId);
        historicalRecord.ClassName.Should().Be("Test Class");
        historicalRecord.Description.Should().Be(archivedRecord.Description);
        historicalRecord.OriginalId.Should().Be(archivedRecord.OriginalId);
        historicalRecord.Uri.Should().Be(archivedRecord.Uri);
        historicalRecord.ObjectStorageId.Should().Be(archivedRecord.ObjectStorageId);
        historicalRecord.ObjectStorageName.Should().Be("Object Storage 1");
        historicalRecord.ProjectId.Should().Be(archivedRecord.ProjectId);
        historicalRecord.ProjectName.Should().Be("Test Project");
        historicalRecord.DataSourceId.Should().Be(archivedRecord.DataSourceId);
        historicalRecord.DataSourceName.Should().Be("Test Data Source");
    }

    [Fact]
    public async Task GetHistoricalRecord_ReturnsMostCurrentRecord_WhenCurrentIsTrue()
    {
        var dto = new UpdateRecordRequestDto
        {
            Name = "Updated Test Record",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { UpdatedProp = "UpdatedValue" }))!,
            Uri = "updated://uri",
            OriginalId = "updated-123",
            Description = "Updated Description",
            ClassId = cid
        };
        
        await _recordBusiness.UpdateRecord(pid, rid, dto);
        
        var historicalRecord = await _historicalRecordBusiness.GetHistoricalRecord(rid, null);
        historicalRecord.Should().NotBeNull();
        historicalRecord.Name.Should().Be("Updated Test Record");
    }
    
    [Fact]
    public async Task GetHistoricalRecord_ReturnsMostRecentRecordByDefault()
    {
        var dto = new UpdateRecordRequestDto
        {
            Name = "Updated Test Record",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { UpdatedProp = "UpdatedValue" }))!,
            Uri = "updated://uri",
            OriginalId = "updated-123",
            Description = "Updated Description",
            ClassId = cid
        };
        
        await _recordBusiness.UpdateRecord(pid, rid, dto);
        
        var historicalRecord = await _historicalRecordBusiness.GetHistoricalRecord(rid, null);
        historicalRecord.Should().NotBeNull();
        historicalRecord.Name.Should().Be("Updated Test Record");
    }
    
    [Fact]
    public async Task GetHistoricalRecord_CanIncludeArchived()
    {
        var dto = new UpdateRecordRequestDto
        {
            Name = "Updated Test Record",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { UpdatedProp = "UpdatedValue" }))!,
            Uri = "updated://uri",
            OriginalId = "updated-123",
            Description = "Updated Description",
            ClassId = cid
        };
        
        await _recordBusiness.UpdateRecord(pid, rid, dto);
        await _recordBusiness.ArchiveRecord(pid, rid);
        
        var historicalRecord = await _historicalRecordBusiness.GetHistoricalRecord(rid, null, false);
        historicalRecord.Should().NotBeNull();
        historicalRecord.Name.Should().Be("Updated Test Record");
        historicalRecord.IsArchived.Should().BeTrue();
    }
    
    // Ask if this should be good behavior
    [Fact]
    public async Task GetHistoricalRecord_ThrowsError_WhenCurrentRecordIsArchived()
    {
        var dto = new UpdateRecordRequestDto
        {
            Name = "Updated Test Record",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { UpdatedProp = "UpdatedValue" }))!,
            Uri = "updated://uri",
            OriginalId = "updated-123",
            Description = "Updated Description",
            ClassId = cid
        };
        
        await _recordBusiness.UpdateRecord(pid, rid, dto);
        await _recordBusiness.ArchiveRecord(pid, rid);
        
        var result = () => _historicalRecordBusiness.GetHistoricalRecord(rid, null);
        await result.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    
    [Fact]
    public async Task GetHistoricalRecord_FiltersByTime()
    {
        var pointInTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        var dto = new UpdateRecordRequestDto
        {
            Name = "Updated Test Record",
            Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { UpdatedProp = "UpdatedValue" }))!,
            Uri = "updated://uri",
            OriginalId = "updated-123",
            Description = "Updated Description",
            ClassId = cid
        };
        
        await _recordBusiness.UpdateRecord(pid, rid, dto);
        
        var historicalRecord = await _historicalRecordBusiness.GetHistoricalRecord(rid, pointInTime);
        historicalRecord.Should().NotBeNull();
        historicalRecord.Name.Should().Be("Test Record");
    }
    
    [Fact]
    public async Task GetHistoricalRecord_ThrowsError_WhenRecordDoesNotExist()
    {
        var result = () => _historicalRecordBusiness.GetHistoricalRecord(rid+ 100000, null);
        await result.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    protected override async Task SeedTestDataAsync()
    {
        await base.SeedTestDataAsync();

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
        
        var dataSource = new DataSource
        {
            Name = "Test Data Source",
            Description = "Test data source for unit tests",
            ProjectId = project.Id,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        
        var dataSource2 = new DataSource
        {
            Name = "Test Data Source 2",
            Description = "Test data source 2 for unit tests",
            ProjectId = project2.Id,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        
        var dataSource3 = new DataSource
        {
            Name = "Test Data Source 3",
            Description = "Test data source 3 for unit tests",
            ProjectId = project2.Id,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        
        Context.DataSources.Add(dataSource);
        Context.DataSources.Add(dataSource2);
        Context.DataSources.Add(dataSource3);
        await Context.SaveChangesAsync();
        did = dataSource.Id;
        did2 = dataSource2.Id;
        
        var testClass = new Class
        {
            Name = "Test Class",
            Description = "Test class for unit tests",
            ProjectId = project.Id,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        var testClass2 = new Class
        {
            Name = "Test Class 2",
            Description = "Test class 2 for unit tests",
            ProjectId = project2.Id,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        
        Context.Classes.Add(testClass);
        Context.Classes.Add(testClass2);
        await Context.SaveChangesAsync();
        cid = testClass.Id;

        var testTag = new Tag
        {
            Name = "Test Tag",
            ProjectId = project.Id,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        var testTag2 = new Tag
        {
            Name = "Test Tag 2",
            ProjectId = project2.Id,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        
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
        
        var testRecord = new Record
        {
            Name = "Test Record",
            Description = "Test record for unit tests",
            ObjectStorageId = os1,
            OriginalId = "og_id",
            Properties = JsonSerializer.Serialize(new { TestProperty = "TestValue" }),
            ProjectId = pid,
            DataSourceId = dataSource.Id,
            ClassId = testClass.Id,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            Tags =  new List<Tag> { testTag },
            Uri = "localhost:8090"
        };
        
        var testRecord2 = new Record
        {
            Name = "Test Record 2",
            Description = "Test record 2 for unit tests",
            OriginalId = "og_id2",
            ObjectStorageId = os1,
            Properties = JsonSerializer.Serialize(new { TestProperty = "TestValue" }),
            ProjectId = pid,
            DataSourceId = dataSource.Id,
            ClassId = testClass.Id,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            Uri = "localhost:8090"
        };
        
        var testRecord3 = new Record
        {
            Name = "Test Record 3",
            Description = "Test record 3 for unit tests",
            OriginalId = "og_id3",
            Properties = JsonSerializer.Serialize(new { TestProperty = "TestValue" }),
            ProjectId = pid2,
            DataSourceId = dataSource2.Id,
            ClassId = testClass2.Id,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            Tags =  new List<Tag> { testTag2 },
            Uri = "localhost:8090"
        };
        
        var testRecord4 = new Record
        {
            Name = "Test Record 4",
            Description = "Test record 4 for unit tests",
            OriginalId = "og_id4",
            Properties = JsonSerializer.Serialize(new { TestProperty = "TestValue" }),
            ProjectId = pid2,
            DataSourceId = dataSource3.Id,
            ClassId = testClass2.Id,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            Tags =  new List<Tag> { testTag2 },
            Uri = "localhost:8090"
        };
        
        Context.Tags.Add(testTag);
        await Context.SaveChangesAsync();
        
        Context.Records.Add(testRecord);
        Context.Records.Add(testRecord2);
        Context.Records.Add(testRecord3);
        Context.Records.Add(testRecord4);
        await Context.SaveChangesAsync();
        
        rid =  testRecord.Id;
        rid2 = testRecord2.Id;
    }
}