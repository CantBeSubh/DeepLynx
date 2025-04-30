namespace Testing;
using Record = deeplynx.datalayer.Models.Record;
using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json.Nodes;
using System;
using deeplynx.models;  
using deeplynx.business;  
using deeplynx.datalayer.Models;
using MockQueryable.Moq;



public class RecordBusinessTests
{
    private readonly Mock<DeeplynxContext> _mockContext;
    private readonly Mock<DbSet<Record>> _mockSet;
    private readonly RecordBusiness _service;

    public RecordBusinessTests()
    {
        _mockSet = new Mock<DbSet<Record>>();
        _mockContext = new Mock<DeeplynxContext>();
        _mockContext.Setup(c => c.Records).Returns(_mockSet.Object);

        _service = new RecordBusiness(_mockContext.Object);
    }
    
    private static Mock<DbSet<T>> GetQueryableMockDbSet<T>(List<T> data) where T : class
    {
        return data.AsQueryable().BuildMockDbSet();
    }
    
    [Fact]
    public async Task GetAllRecords_ReturnsCorrectRecords()
    {
        // Arrange
        var records = new List<Record>
        {
            new Record { Id = 1, ProjectId = 1, DataSourceId = 1, Name = "Record1" },
            new Record { Id = 2, ProjectId = 1, DataSourceId = 1, Name = "Record2" }
        };

        var mockSet = records.AsQueryable().BuildMockDbSet();

        var mockContext = new Mock<DeeplynxContext>();
        mockContext.Setup(c => c.Records).Returns(mockSet.Object);

        var service = new RecordBusiness(mockContext.Object);

        // Act
        var result = await service.GetAllRecords(1, 1);

        // Assert
        Assert.Equal(2, result.Count());
    }

   [Fact]
    public async Task GetRecord_ValidId_ReturnsRecord()
    {
        var records = new List<Record>
        {
            new Record { Id = 1, ProjectId = 1, DataSourceId = 1 }
        };
        var mockSet = records.AsQueryable().BuildMockDbSet();
        _mockContext.Setup(c => c.Records).Returns(mockSet.Object);
        var result = await _service.GetRecord(1, 1, 1);
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }
    [Fact]
    public async Task GetRecord_InvalidId_ThrowsKeyNotFoundException()
    {
        var records = new List<Record>().AsQueryable().BuildMockDbSet();
        _mockContext.Setup(c => c.Records).Returns(records.Object);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetRecord(1, 1, 999));
    }
    [Fact]
    public async Task CreateRecord_ValidInput_AddsRecord()
    {
        var records = new List<Record>();
        var mockSet = records.AsQueryable().BuildMockDbSet();
        mockSet.Setup(d => d.Add(It.IsAny<Record>())).Callback<Record>(r => records.Add(r));
        _mockContext.Setup(c => c.Records).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);
        var dto = new RecordRequestDto
        {
            Uri = "uri",
            Properties = new JsonObject { ["key"] = "value" },
            OriginalId = "orig",
            Name = "Test",
            ClassName = "TestClass"
        };
        var result = await _service.CreateRecord(1, 1, dto);
        Assert.Single(records);
        Assert.Equal("Test", result.Name);
    }
    [Fact]
    public async Task CreateRecord_ExceedsDepth_ThrowsException()
    {
        var nested = new JsonObject
        {
            ["level1"] = new JsonObject
            {
                ["level2"] = new JsonObject
                {
                    ["level3"] = new JsonObject
                    {
                        ["level4"] = "too deep"
                    }
                }
            }
        };
        var dto = new RecordRequestDto
        {
            Uri = "uri",
            Properties = nested,
            OriginalId = "orig",
            Name = "TooDeep",
            ClassName = "Class"
        };
        await Assert.ThrowsAsync<Exception>(() => _service.CreateRecord(1, 1, dto));
    }
    [Fact]
    public async Task UpdateRecord_ValidInput_UpdatesRecord()
    {
        var record = new Record { Id = 1, ProjectId = 1, DataSourceId = 1, Name = "Old" };
        var list = new List<Record> { record };
        var mockSet = list.AsQueryable().BuildMockDbSet();
        mockSet.Setup(d => d.Update(It.IsAny<Record>())).Callback<Record>(r =>
        {
            var idx = list.FindIndex(x => x.Id == r.Id);
            if (idx >= 0) list[idx] = r;
        });
        _mockContext.Setup(c => c.Records).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);
        var dto = new RecordRequestDto
        {
            Uri = "newuri",
            Properties = new JsonObject { ["key"] = "updated" },
            OriginalId = "newid",
            Name = "Updated",
            ClassName = "UpdatedClass"
        };
        var result = await _service.UpdateRecord(1, 1, 1, dto);
        Assert.Equal("Updated", result.Name);
    }
    [Fact]
    public async Task DeleteRecord_ValidId_DeletesRecord()
    {
        var record = new Record { Id = 1, ProjectId = 1, DataSourceId = 1 };
        var records = new List<Record> { record };
        var mockSet = records.AsQueryable().BuildMockDbSet();
        mockSet.Setup(d => d.Remove(It.IsAny<Record>())).Callback<Record>(r => records.Remove(r));
        _mockContext.Setup(c => c.Records).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);
        var result = await _service.DeleteRecord(1, 1, 1);
        Assert.True(result);
        Assert.Empty(records);
    }
    [Fact]
    public void CalculateJsonMaxDepth_ReturnsCorrectDepth()
    {
        var json = new JsonObject
        {
            ["a"] = new JsonObject
            {
                ["b"] = new JsonObject
                {
                    ["c"] = "value"
                }
            }
        };
        var depth = _service.CalculateJsonMaxDepth(json);
        Assert.Equal(3, depth);
    }
}