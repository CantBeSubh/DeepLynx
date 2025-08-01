using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.helpers.exceptions;
using deeplynx.interfaces;
using deeplynx.models;
using FluentAssertions;
using FluentAssertions.Extensions;
using Moq;
using Record = deeplynx.datalayer.Models.Record;

namespace deeplynx.tests;

[Collection("Test Suite Collection")]
public class HistoricalRecordBusinessTests: IntegrationTestBase
{
    public DateTime firstPointInTime;
    public long pid;
    public long pid2;
    public long did;
    public long did2;
    public long did3;
    public long cid;
    public long cid2;
    public long rid;
    public long rid2;
    public long tid;
    public long hr;
    public long hr2;
    public long hr3;
    public long hr4;
    public long hr5;
    private HistoricalRecordBusiness _historicalRecordBusiness = null!;
    private RecordBusiness _recordBusiness = null!;
    
    public HistoricalRecordBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _historicalRecordBusiness = new HistoricalRecordBusiness(Context);
        _recordBusiness = new RecordBusiness(Context);
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
        await SeedTestDataAsync();
        
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
        await SeedTestDataAsync();

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
        await SeedTestDataAsync();

        await _recordBusiness.ArchiveRecord(pid, rid);
        
        var historicalRecords = await _historicalRecordBusiness.GetAllHistoricalRecords(pid);
        historicalRecords.Should().NotBeNull();
        historicalRecords.Should().HaveCount(1);
        historicalRecords.Should().NotContain(x => x.Name == "Test Record");
        historicalRecords.Should().Contain(x => x.Name == "Test Record 2");
    }
    
    [Fact]
    public async Task GetHistoricalRecords_ReturnsEmptyListWhenNoRecords()
    {
        await SeedTestDataAsync();

        await _recordBusiness.DeleteRecord(pid, rid);
        await _recordBusiness.DeleteRecord(pid, rid2);
        
        var historicalRecords = await _historicalRecordBusiness.GetAllHistoricalRecords(pid);
        historicalRecords.Should().NotBeNull();
        historicalRecords.Should().HaveCount(0);
    }
    
    [Fact]
    public async Task GetHistoricalRecords_FiltersByDataSource()
    {
        await SeedTestDataAsync();
        
        var historicalRecords = await _historicalRecordBusiness.GetAllHistoricalRecords(pid2, did2);
        historicalRecords.Should().NotBeNull();
        historicalRecords.Should().HaveCount(1);
        historicalRecords.Should().Contain(x => x.Name == "Test Record 3");
        historicalRecords.Should().NotContain(x => x.Name == "Test Record 4");
    }
    
    [Fact]
    public async Task GetHistoricalRecords_FiltersByTime()
    {
        await SeedTestDataAsync();
        
        firstPointInTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        
        var testRecordLate = new Record
        {
            Name = "Test Record Late",
            Description = "Test record late for unit tests",
            OriginalId = "og_idlate",
            Properties = JsonSerializer.Serialize(new { TestProperty = "TestValue late" }),
            ProjectId = pid,
            DataSourceId = did,
            ClassId = cid,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            Uri = "localhost:8090"
        };
        
        Context.Records.Add(testRecordLate);
        await Context.SaveChangesAsync();
        
        var historicalRecords = await _historicalRecordBusiness.GetAllHistoricalRecords(pid, null, firstPointInTime, false, false);
        historicalRecords.Should().NotBeNull();
        historicalRecords.Should().HaveCount(2);
        historicalRecords.Should().Contain(x => x.Name == "Test Record");
        historicalRecords.Should().Contain(x => x.Name == "Test Record 2");
    }

    [Fact]
    public async Task GetHistoryForRecord_ReturnsFullHistory()
    {
        await SeedTestDataAsync();
        
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
        recordHistory.Should().Contain(x => x.Name == "Test Record" && x.Tags == "[null]");
        recordHistory.Should().Contain(x => x.Name == "Test Record" && x.Tags != "[null]");
        recordHistory.Should().Contain(x => x.Name == "Updated Test Record" && x.ArchivedAt == null);
        recordHistory.Should().Contain(x => x.Name == "Updated Test Record" && x.ArchivedAt != null);
    }
    
    
    
    
    
    protected override async Task SeedTestDataAsync()
    {
        await base.SeedTestDataAsync();

        var project = new Project
        {
            Name = "Test Project",
            Description = "Test project for unit tests",
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        var project2 = new Project
        {
            Name = "Test Project 2",
            Description = "Test project 2 for unit tests",
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
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
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        
        var dataSource2 = new DataSource
        {
            Name = "Test Data Source 2",
            Description = "Test data source 2 for unit tests",
            ProjectId = project2.Id,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        
        var dataSource3 = new DataSource
        {
            Name = "Test Data Source 3",
            Description = "Test data source 3 for unit tests",
            ProjectId = project2.Id,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        
        Context.DataSources.Add(dataSource);
        Context.DataSources.Add(dataSource2);
        Context.DataSources.Add(dataSource3);
        await Context.SaveChangesAsync();
        did = dataSource.Id;
        did2 = dataSource2.Id;
        did3 = dataSource3.Id;
        
        var testClass = new Class
        {
            Name = "Test Class",
            Description = "Test class for unit tests",
            ProjectId = project.Id,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        var testClass2 = new Class
        {
            Name = "Test Class 2",
            Description = "Test class 2 for unit tests",
            ProjectId = project2.Id,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        
        Context.Classes.Add(testClass);
        Context.Classes.Add(testClass2);
        await Context.SaveChangesAsync();
        cid = testClass.Id;
        cid2 = testClass2.Id;

        var testTag = new Tag
        {
            Name = "Test Tag",
            ProjectId = project.Id,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        var testTag2 = new Tag
        {
            Name = "Test Tag 2",
            ProjectId = project2.Id,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
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
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            Tags =  new List<Tag> { testTag },
            Uri = "localhost:8090"
        };
        
        var testRecord2 = new Record
        {
            Name = "Test Record 2",
            Description = "Test record 2 for unit tests",
            OriginalId = "og_id2",
            Properties = JsonSerializer.Serialize(new { TestProperty = "TestValue" }),
            ProjectId = project.Id,
            DataSourceId = dataSource.Id,
            ClassId = testClass.Id,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow.AddSeconds(1), DateTimeKind.Unspecified),
            Uri = "localhost:8090"
        };
        
        var testRecord3 = new Record
        {
            Name = "Test Record 3",
            Description = "Test record 3 for unit tests",
            OriginalId = "og_id3",
            Properties = JsonSerializer.Serialize(new { TestProperty = "TestValue" }),
            ProjectId = project2.Id,
            DataSourceId = dataSource2.Id,
            ClassId = testClass2.Id,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            Tags =  new List<Tag> { testTag2 },
            Uri = "localhost:8090"
        };
        
        var testRecord4 = new Record
        {
            Name = "Test Record 4",
            Description = "Test record 4 for unit tests",
            OriginalId = "og_id4",
            Properties = JsonSerializer.Serialize(new { TestProperty = "TestValue" }),
            ProjectId = project2.Id,
            DataSourceId = dataSource3.Id,
            ClassId = testClass2.Id,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            Tags =  new List<Tag> { testTag2 },
            Uri = "localhost:8090"
        };
        
        Context.Records.Add(testRecord);
        Context.Records.Add(testRecord2);
        Context.Records.Add(testRecord3);
        Context.Records.Add(testRecord4);
        Context.Tags.Add(testTag);
        await Context.SaveChangesAsync();
        
        rid =  testRecord.Id;
        rid2 = testRecord2.Id;
        tid = testTag.Id;

        // var historicalRecord = new HistoricalRecord
        // {
        //     RecordId = rid,
        //     Name = "Test Historical Record",
        //     Properties = JsonSerializer.Serialize(new { TestProperty = "TestValue" }),
        //     ProjectId = project.Id,
        //     ProjectName = project.Name,
        //     DataSourceId = dataSource.Id,
        //     DataSourceName = dataSource.Name,
        //     ClassId = testClass.Id,
        //     ClassName = testClass.Name,
        //     CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
        //     Tags =  JsonSerializer.Serialize(testTag.Name),
        //     Current = true,
        //     LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        // };
        //
        // var historicalRecord2 = new HistoricalRecord
        // {
        //     RecordId = rid,
        //     Name = "Test Historical Record 2",
        //     Properties = JsonSerializer.Serialize(new { TestProperty = "TestValue 2" }),
        //     ProjectId = project.Id,
        //     ProjectName = project.Name,
        //     DataSourceId = dataSource.Id,
        //     DataSourceName = dataSource.Name,
        //     ClassId = testClass.Id,
        //     ClassName = testClass.Name,
        //     CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
        //     Tags =  JsonSerializer.Serialize(testTag.Name),
        //     Current = false,
        //     LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        // };
        //
        // var historicalRecord3 = new HistoricalRecord
        // {
        //     RecordId = rid2,
        //     Name = "Test Historical Record 3",
        //     Properties = JsonSerializer.Serialize(new { TestProperty = "TestValue 3" }),
        //     ProjectId = project.Id,
        //     ProjectName = project.Name,
        //     DataSourceId = dataSource.Id,
        //     DataSourceName = dataSource.Name,
        //     ClassId = testClass.Id,
        //     ClassName = testClass.Name,
        //     CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
        //     Tags =  JsonSerializer.Serialize(testTag.Name),
        //     Current = true,
        //     LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        // };
        //
        // var historicalRecord4 = new HistoricalRecord
        // {
        //     RecordId = rid2,
        //     Name = "Test Historical Record 4",
        //     Properties = JsonSerializer.Serialize(new { TestProperty = "TestValue 4" }),
        //     ProjectId = project.Id,
        //     ProjectName = project.Name,
        //     DataSourceId = dataSource.Id,
        //     DataSourceName = dataSource.Name,
        //     ClassId = testClass.Id,
        //     ClassName = testClass.Name,
        //     CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
        //     Tags =  JsonSerializer.Serialize(testTag.Name),
        //     Current = false,
        //     LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        // };
        //
        // var historicalRecord5 = new HistoricalRecord
        // {
        //     RecordId = rid2,
        //     Name = "Test Historical Record 5",
        //     Properties = JsonSerializer.Serialize(new { TestProperty = "TestValue 5" }),
        //     ProjectId = project.Id,
        //     ProjectName = project.Name,
        //     DataSourceId = dataSource.Id,
        //     DataSourceName = dataSource.Name,
        //     ClassId = testClass.Id,
        //     ClassName = testClass.Name,
        //     CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
        //     Tags =  JsonSerializer.Serialize(testTag.Name),
        //     Current = false,
        //     LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
        //     ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        // };
        //
        // Context.HistoricalRecords.Add(historicalRecord);
        // Context.HistoricalRecords.Add(historicalRecord2);
        // Context.HistoricalRecords.Add(historicalRecord3);
        // Context.HistoricalRecords.Add(historicalRecord4);
        // Context.HistoricalRecords.Add(historicalRecord5);
        // await Context.SaveChangesAsync();
        // hr = historicalRecord.Id;
        // hr2 = historicalRecord2.Id;
        // hr3 = historicalRecord3.Id;
        // hr4 = historicalRecord4.Id;
        // hr5 = historicalRecord5.Id;
        
    }
}