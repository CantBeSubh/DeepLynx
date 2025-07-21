using System.Text.Json;
using System.Text.Json.Nodes;
using deeplynx.business;
using deeplynx.datalayer.Models;
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
    private readonly Mock<IHistoricalRecordBusiness> _mockHistoricalRecordBusiness;
    
    public RecordBusinessTests(TestSuiteFixture fixture) : base(fixture)
    {
        _mockHistoricalRecordBusiness = new Mock<IHistoricalRecordBusiness>();
        
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _recordBusiness = new RecordBusiness(Context, _mockHistoricalRecordBusiness.Object);
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
        
        var testRecord = new Record
        {
            Id = 100,
            Name = "Test Record",
            Properties = JsonSerializer.Serialize(new { TestProperty = "TestValue" }),
            ProjectId = 100,
            DataSourceId = 100,
            ClassId = 100,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };

        Context.Projects.Add(project);
        Context.DataSources.Add(dataSource);
        Context.Classes.Add(testClass);
        Context.Records.Add(testRecord);
        await Context.SaveChangesAsync();
    }

    #region GetAllRecords Tests

    [Fact]
    public async Task GetAllRecords_ValidProjectId_ReturnsRecords()
    {
        // Arrange
        var projectId = 100L;
        var expectedRecords = new List<HistoricalRecordResponseDto>
        {
            new HistoricalRecordResponseDto
            {
                Id = 100,
                Name = "Test Record",
                ProjectId = projectId
            }
        };

        _mockHistoricalRecordBusiness
            .Setup(x => x.GetAllHistoricalRecords(projectId, null, null, true, true))
            .ReturnsAsync(expectedRecords);

        // Act
        var result = await _recordBusiness.GetAllRecords(projectId, null, true);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Test Record", result.First().Name);
        _mockHistoricalRecordBusiness.Verify(x => x.GetAllHistoricalRecords(projectId, null, null, true, true), Times.Once);
    }

    [Fact]
    public async Task GetAllRecords_WithDataSourceId_ReturnsFilteredRecords()
    {
        // Arrange
        var projectId = 100L;
        var dataSourceId = 100L;
        var expectedRecords = new List<HistoricalRecordResponseDto>
        {
            new HistoricalRecordResponseDto
            {
                Id = 100,
                Name = "Test Record",
                ProjectId = projectId,
                DataSourceId = dataSourceId
            }
        };

        _mockHistoricalRecordBusiness
            .Setup(x => x.GetAllHistoricalRecords(projectId, dataSourceId, null, true, true))
            .ReturnsAsync(expectedRecords);

        // Act
        var result = await _recordBusiness.GetAllRecords(projectId, dataSourceId, true);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(dataSourceId, result.First().DataSourceId);
        _mockHistoricalRecordBusiness.Verify(x => x.GetAllHistoricalRecords(projectId, dataSourceId, null, true, true), Times.Once);
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
        var expectedRecord = new HistoricalRecordResponseDto
        {
            Id = recordId,
            Name = "Test Record",
            ProjectId = projectId
        };

        _mockHistoricalRecordBusiness
            .Setup(x => x.GetHistoricalRecord(recordId, null, true, true))
            .ReturnsAsync(expectedRecord);

        // Act
        var result = await _recordBusiness.GetRecord(projectId, recordId, true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(recordId, result.Id);
        Assert.Equal("Test Record", result.Name);
        _mockHistoricalRecordBusiness.Verify(x => x.GetHistoricalRecord(recordId, null, true, true), Times.Once);
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
        var bulkDto = new BulkRecordRequestDto
        {
            Records = new List<RecordRequestDto>
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
            }
        };

        // Act
        var result = await _recordBusiness.BulkCreateRecords(projectId, dataSourceId, bulkDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Records.Count());
        Assert.Contains(result.Records, r => r.Name == "Bulk Record 1");
        Assert.Contains(result.Records, r => r.Name == "Bulk Record 2");

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
        var bulkDto = new BulkRecordRequestDto
        {
            Records = new List<RecordRequestDto>()
        };

        // Act
        var result = await _recordBusiness.BulkCreateRecords(projectId, dataSourceId, bulkDto);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Records);
    }

    [Fact]
    public async Task BulkCreateRecords_InvalidProjectId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var invalidProjectId = 999L;
        var dataSourceId = 1L;
        var bulkDto = new BulkRecordRequestDto
        {
            Records = new List<RecordRequestDto>
            {
                new RecordRequestDto
                {
                    Name = "Test Record",
                    Properties = (JsonObject)JsonNode.Parse(JsonSerializer.Serialize(new { TestProp = "TestValue" }))!
                }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _recordBusiness.BulkCreateRecords(invalidProjectId, dataSourceId, bulkDto));
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
            ClassId = 100
        };

        // Act
        var result = await _recordBusiness.UpdateRecord(projectId, recordId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Test Record", result.Name);
        Assert.Equal("updated://uri", result.Uri);
        Assert.Equal("updated-123", result.OriginalId);
        Assert.NotNull(result.ModifiedAt);

        // Verify record was actually updated in database
        var updatedRecord = await Context.Records.FindAsync(recordId);
        Assert.NotNull(updatedRecord);
        Assert.Equal("Updated Test Record", updatedRecord.Name);
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
}