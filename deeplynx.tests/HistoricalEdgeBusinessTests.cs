using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Record = deeplynx.datalayer.Models.Record;

namespace deeplynx.tests;

[Collection("Test Suite Collection")]
public class HistoricalEdgeBusinessTests: IntegrationTestBase
{
    public HistoricalEdgeBusiness _historicalEdgeBusiness = null!;
    public EdgeBusiness _edgeBusiness = null!;
    public long pid;
    public long pid2;
    public long eid;
    public long eid2;
    public long eid3;
    public long eid4;
    public long dsid;
    public long dsid2;
    public long originRecordId;
    public long originRecordId2;
    public long destinationRecordId;
    public long destinationRecordId2;
    public long relationshipId;
    public long relationshipId2;
    public HistoricalEdgeBusinessTests(TestSuiteFixture fixture) : base(fixture) { }
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _historicalEdgeBusiness = new HistoricalEdgeBusiness(Context);
        _edgeBusiness = new EdgeBusiness(Context);
    }

    [Fact]
    public async Task GetAllHistoricalEdges_ReturnsListOfCurrentHistoricalRecordsForProject()
    {
        var historicalEdges = await _historicalEdgeBusiness.GetAllHistoricalEdges(pid);
        historicalEdges.Should().NotBeNull();
        historicalEdges.Should().HaveCount(2);
        historicalEdges.Should().Contain(e => e.Id == eid);
        historicalEdges.Should().Contain(e => e.Id == eid2);
        historicalEdges.Should().NotContain(e => e.Id == eid3);
        historicalEdges.Should().NotContain(e => e.Id == eid4);
    }
    
    [Fact]
     public async Task GetAllHistoricalEdges_ReturnsListOfUpdatedHistoricalEdges()
     {
         var dto = new UpdateEdgeRequestDto()
         {
             OriginId = (int)destinationRecordId,
             DestinationId = (int)destinationRecordId2,
             RelationshipId = (int)relationshipId
         };
         
         var dto2 = new UpdateEdgeRequestDto
         {
             OriginId = (int)destinationRecordId2,
             DestinationId = (int)destinationRecordId,
             RelationshipId = (int)relationshipId
         };

         // Act
         await _edgeBusiness.UpdateEdge(pid, dto, eid, null, null);
         await _edgeBusiness.UpdateEdge(pid, dto2, eid2, null, null);
         

         var historicalEdges = await _historicalEdgeBusiness.GetAllHistoricalEdges(pid);
         historicalEdges.Should().NotBeNull();
         historicalEdges.Should().HaveCount(2);
         historicalEdges.First().DestinationId.Should().Be(destinationRecordId2);
         historicalEdges.First().OriginId.Should().Be(destinationRecordId);
         historicalEdges.Last().DestinationId.Should().Be(destinationRecordId);
         historicalEdges.Last().OriginId.Should().Be(destinationRecordId2);
     }
    [Fact]
    public async Task GetAllHistoricalEdges_FiltersByDataSource()
    {
        var historicalEdges = await _historicalEdgeBusiness.GetAllHistoricalEdges(pid, dsid);
        historicalEdges.Should().NotBeNull();
        historicalEdges.Should().HaveCount(1);
        historicalEdges.Should().Contain(e => e.Id == eid && e.DataSourceId == dsid);
        historicalEdges.Should().NotContain(e => e.Id == eid2);
    }
     
    [Fact]
    public async Task GetAllHistoricalEdges_ExcludesArchivedHistoricalEdgesByDefault()
    {
        await _edgeBusiness.ArchiveEdge(pid, eid, null, null);
        
        var historicalEdges = await _historicalEdgeBusiness.GetAllHistoricalEdges(pid);
        historicalEdges.Should().NotBeNull();
        historicalEdges.Should().HaveCount(1);
        historicalEdges.Should().NotContain(e => e.Id == eid && e.IsArchived);
        historicalEdges.Should().Contain(e => e.Id == eid2);
    }

    [Fact]
    public async Task GetAllHistoricalEdges_InlcudesArchivedHistoricalEdges()
    {
        await _edgeBusiness.ArchiveEdge(pid, eid, null, null);
        
        var historicalEdges = await _historicalEdgeBusiness.GetAllHistoricalEdges(pid,null, null, false);
        historicalEdges.Should().NotBeNull();
        historicalEdges.Should().HaveCount(2);
        historicalEdges.Should().Contain(e => e.Id == eid && e.IsArchived);
        historicalEdges.Should().Contain(e => e.Id == eid2);
    }

    [Fact]
    public async Task GetHistoricalEdges_ReturnsEmptyListWhenNoEdges()
    {
        await _edgeBusiness.DeleteEdge(pid, eid, null, null);
        await _edgeBusiness.DeleteEdge(pid, eid2, null, null);
        var historicalEdges = await _historicalEdgeBusiness.GetAllHistoricalEdges(pid);
        historicalEdges.Should().NotBeNull();
        historicalEdges.Should().HaveCount(0);
    }

    [Fact]
    public async Task GetHistoricalEdges_FiltersByTime()
    {
        var pointInTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        var edgeLate = new Edge()
        {
            ProjectId = pid,
            DataSourceId = dsid,
            OriginId = destinationRecordId2,
            LastUpdatedAt = pointInTime.AddMilliseconds(1),
            RelationshipId = relationshipId,
            DestinationId = originRecordId,
        };
        Context.Edges.Add(edgeLate);
        await Context.SaveChangesAsync();
        
        var historicalEdges = await _historicalEdgeBusiness.GetAllHistoricalEdges(pid, null, pointInTime);
        historicalEdges.Should().NotBeNull();
        historicalEdges.Should().HaveCount(2);
        historicalEdges.Should().NotContain(e => e.Id == edgeLate.Id);
        
    }

    [Fact]
    public async Task GetHistoryForEdge_ReturnsFullHistory()
    {
        var dto = new UpdateEdgeRequestDto
        {
            OriginId = (int)destinationRecordId,
            DestinationId = (int)destinationRecordId2,
            RelationshipId = (int)relationshipId
        };
        await _edgeBusiness.UpdateEdge(pid, dto, eid, null, null);
        await _edgeBusiness.ArchiveEdge(pid, eid, null, null);

        var edgeHistory = await _historicalEdgeBusiness.GetHistoryForEdge(eid, null, null);
        edgeHistory.Should().NotBeNull();
        edgeHistory.Should().HaveCount(3);
        edgeHistory.Should().NotContain(e => e.Id != eid);
    }
    
    [Fact]
    public async Task GetHistoryForEdge_ThrowsError_WhenEdgeDoesNotExist()
    {
        var historicalEdges = () => _historicalEdgeBusiness.GetHistoryForEdge(eid4 + 100000, null, null);
        await historicalEdges.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetHistoricalEdge_ThrowsError_WhenEdgeDoesNotExist()
    {
        var historicalEdge = () => _historicalEdgeBusiness.GetHistoricalEdge(eid4 + 100000, null, null, null);
        await historicalEdge.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    [Fact]
    public async Task GetHistoricalEdge_ReturnsMostCurrentEdge()
    {
        var dto = new UpdateEdgeRequestDto
        {
            OriginId = (int)destinationRecordId,
            DestinationId = (int)destinationRecordId2,
            RelationshipId = (int)relationshipId
        };
        await _edgeBusiness.UpdateEdge(pid, dto, eid, null, null);
        var historicalEdge = await _historicalEdgeBusiness.GetHistoricalEdge(eid, null, null, null);
        historicalEdge.Should().NotBeNull();
        historicalEdge.Id.Should().Be(eid);
        historicalEdge.OriginId.Should().Be(destinationRecordId);
        historicalEdge.DestinationId.Should().Be(destinationRecordId2);
    }
    
    [Fact]
    public async Task GetHistoricalEdge_CanIncludeArchivedHistoricalEdges()
    {
        var dto = new UpdateEdgeRequestDto
        {
            OriginId = (int)destinationRecordId,
            DestinationId = (int)destinationRecordId2,
            RelationshipId = (int)relationshipId
        };
        await _edgeBusiness.UpdateEdge(pid, dto, eid, null, null);
        await _edgeBusiness.ArchiveEdge(pid, eid, null, null);
        var historicalEdge = await _historicalEdgeBusiness.GetHistoricalEdge(eid, null, null, null, false);
        historicalEdge.Should().NotBeNull();
        historicalEdge.Id.Should().Be(eid);
        historicalEdge.OriginId.Should().Be(destinationRecordId);
        historicalEdge.DestinationId.Should().Be(destinationRecordId2);
        historicalEdge.IsArchived.Should().BeTrue();
    }

    [Fact]
    public async Task GetHistoricalEdge_FiltersByTime()
    {
        var pointInTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        var dto = new UpdateEdgeRequestDto
        {
            OriginId = (int)destinationRecordId,
            DestinationId = (int)destinationRecordId2,
            RelationshipId = (int)relationshipId
        };
        await _edgeBusiness.UpdateEdge(pid, dto, eid, null, null);
        var historicalEdge = await _historicalEdgeBusiness.GetHistoricalEdge(eid, null, null, pointInTime);
        historicalEdge.Should().NotBeNull();
        historicalEdge.Id.Should().Be(eid);
        historicalEdge.OriginId.Should().Be(originRecordId);
        historicalEdge.DestinationId.Should().Be(destinationRecordId);
    }

    [Fact]
    public async Task GetHistoricalEdge_ThrowsError_WhenCurrentRecordIsArchived()
    {
        await _edgeBusiness.ArchiveEdge(pid, eid, null, null);
        var historicalEdge = () => _historicalEdgeBusiness.GetHistoricalEdge(eid, null, null, null);
        await historicalEdge.Should().ThrowAsync<KeyNotFoundException>();
    }
    
    protected override async Task SeedTestDataAsync()
    {
        await base.SeedTestDataAsync();
        var project = new Project() { Name = "Project 1" };
        var project2 = new Project() { Name = "Project 2" };
        Context.Projects.Add(project);
        Context.Projects.Add(project2);
        await Context.SaveChangesAsync();
        pid = project.Id;
        pid2 = project2.Id;

        var dataSource = new DataSource
        {
            Name = "DataSource 1",
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        var dataSource2 = new DataSource
        {
            Name = "DataSource 2",
            ProjectId = pid2,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        var dataSource3 = new DataSource
        {
            Name = "DataSource 3",
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        Context.DataSources.Add(dataSource);
        Context.DataSources.Add(dataSource2);
        Context.DataSources.Add(dataSource3);
        await Context.SaveChangesAsync();
        dsid = dataSource.Id;
        dsid2 = dataSource2.Id;

        var testClass = new Class
        {
            Name = "Class 1",
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        var testClass2 = new Class
        {
            Name = "Class 2",
            ProjectId = pid2,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        Context.Classes.Add(testClass);
        Context.Classes.Add(testClass2);
        await Context.SaveChangesAsync();

        var originRecord = new Record
        {
            ProjectId = pid,
            DataSourceId = dsid,
            ClassId = testClass.Id,
            Properties = "{\"test\": \"origin_value\"}",
            Name = "Origin",
            Description = "Origin Description",
            OriginalId = "orig",
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        
        var originRecord2 = new Record
        {
            ProjectId = pid2,
            DataSourceId = dsid2,
            ClassId = testClass2.Id,
            Properties = "{\"test\": \"origin_value 2\"}",
            Name = "Origin 2",
            Description = "Origin Description 2",
            OriginalId = "orig 2",
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        Context.Records.Add(originRecord);
        Context.Records.Add(originRecord2);

        var destinationRecord = new Record
        {
            ProjectId = pid,
            DataSourceId = dsid,
            ClassId = testClass.Id,
            Properties = "{\"test\": \"destination_value\"}", 
            Name = "Destination 1",
            Description = "Destination Description 1",
            OriginalId = "dest1",
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        var destinationRecord3 = new Record
        {
            ProjectId = pid2,
            DataSourceId = dsid2,
            ClassId = testClass2.Id,
            Properties = "{\"test\": \"destination_value 3\"}", 
            Name = "Destination 3",
            Description = "Destination Description 3",
            OriginalId = "dest3",
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        Context.Records.Add(destinationRecord);
        Context.Records.Add(destinationRecord3);
        
        // Create additional records for second edge
        var destinationRecord2 = new Record
        {
            ProjectId = pid,
            DataSourceId = dsid,
            Properties = "{\"test\": \"destination2_value\"}",
            Name = "Destination 2",
            Description = "Destination Description 2",
            OriginalId = "dest2",
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        Context.Records.Add(destinationRecord2);

        var relationship = new Relationship
        {
            Name = "Relationship 1",
            ProjectId = pid,
            OriginId = testClass.Id,
            DestinationId = testClass.Id,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        var relationship2 = new Relationship
        {
            Name = "Relationship 2",
            ProjectId = pid2,
            OriginId = testClass2.Id,
            DestinationId = testClass2.Id,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        Context.Relationships.Add(relationship);
        Context.Relationships.Add(relationship2);

        await Context.SaveChangesAsync();

        originRecordId = originRecord.Id;
        originRecordId2= originRecord2.Id;
        destinationRecordId = destinationRecord.Id;
        destinationRecordId2 = destinationRecord2.Id;
        relationshipId = relationship.Id;
        relationshipId2 = relationship2.Id;
        
        var edge = new Edge()
        {
            ProjectId = pid,
            DataSourceId = dsid,
            OriginId = originRecordId,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            RelationshipId = relationshipId,
            DestinationId = destinationRecordId,
        };
        var edge2 = new Edge()
        {
            ProjectId = pid,
            DataSourceId = dataSource3.Id,
            OriginId = destinationRecordId,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            RelationshipId = relationshipId,
            DestinationId = originRecordId,
        };
        var edge3 = new Edge()
        {
            ProjectId = pid2,
            DataSourceId = dsid2,
            OriginId = originRecordId2,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            RelationshipId = relationshipId2,
            DestinationId = destinationRecordId2,
        };
        var edge4 = new Edge()
        {
            ProjectId = pid2,
            DataSourceId = dsid2,
            OriginId = destinationRecordId2,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            RelationshipId = relationshipId2,
            DestinationId = originRecordId2,
        };
        Context.Edges.Add(edge);
        Context.Edges.Add(edge2);
        Context.Edges.Add(edge3);
        Context.Edges.Add(edge4);
        await Context.SaveChangesAsync();
        eid = edge.Id;
        eid2 = edge2.Id;
        eid3 = edge3.Id;
        eid4 = edge4.Id;
        
        Context.ChangeTracker.Clear();
    }
}