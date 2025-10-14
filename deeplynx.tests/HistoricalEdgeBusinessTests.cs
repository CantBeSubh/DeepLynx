using System.ComponentModel.DataAnnotations;
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
public class HistoricalEdgeBusinessTests: IntegrationTestBase
{
    public HistoricalEdgeBusiness _historicalEdgeBusiness = null!;
    public EdgeBusiness _edgeBusiness = null!;
    public EventBusiness _eventBusiness = null!;
    private INotificationBusiness _notificationBusiness = null!;
    private Mock<ILogger<NotificationBusiness>> _mockNotificationLogger = null!;
    private Mock<IHubContext<EventNotificationHub>> _mockHubContext = null!;
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
        _mockHubContext = new Mock<IHubContext<EventNotificationHub>>();
        _mockNotificationLogger = new Mock<ILogger<NotificationBusiness>>();
        _notificationBusiness = new NotificationBusiness(Context, _mockNotificationLogger.Object, _mockHubContext.Object);
        _eventBusiness = new EventBusiness(Context, _cacheBusiness, _notificationBusiness);
        _historicalEdgeBusiness = new HistoricalEdgeBusiness(Context);
        _edgeBusiness = new EdgeBusiness(Context, _cacheBusiness, _eventBusiness);
    }

    #region GetAllHistoricalEdges Tests
    [Fact]
    public async Task GetAllHistoricalEdges_ReturnsListOfCurrentHistoricalRecordsForProject()
    {
        var historicalEdges = await _historicalEdgeBusiness.GetAllHistoricalEdges(pid);
        Assert.NotNull(historicalEdges);
        Assert.Equal(2, historicalEdges.Count());
        Assert.Contains(historicalEdges, e => e.Id == eid);
        Assert.Contains(historicalEdges, e => e.Id == eid2);
        Assert.DoesNotContain(historicalEdges, e => e.Id == eid3);
        Assert.DoesNotContain(historicalEdges, e => e.Id == eid4);
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
         Assert.NotNull(historicalEdges);
         Assert.Equal(2, historicalEdges.Count());
         var edge1 = historicalEdges.First(e => e.Id == eid);
         var edge2 = historicalEdges.First(e => e.Id == eid2);
         Assert.Equal(destinationRecordId2, edge1.DestinationId);
         Assert.Equal(destinationRecordId, edge1.OriginId);
         Assert.Equal(destinationRecordId, edge2.DestinationId);
         Assert.Equal(destinationRecordId2, edge2.OriginId);
     }
    [Fact]
    public async Task GetAllHistoricalEdges_FiltersByDataSource()
    {
        var historicalEdges = await _historicalEdgeBusiness.GetAllHistoricalEdges(pid, dsid);
        Assert.NotNull(historicalEdges);
        Assert.Single(historicalEdges);
        Assert.Contains(historicalEdges, e => e.Id == eid && e.DataSourceId == dsid);
        Assert.DoesNotContain(historicalEdges, e => e.Id == eid2);
    }
     
    [Fact]
    public async Task GetAllHistoricalEdges_ExcludesArchivedHistoricalEdgesByDefault()
    {
        await _edgeBusiness.ArchiveEdge(pid, eid, null, null);
        Context.ChangeTracker.Clear();
        var historicalEdges = await _historicalEdgeBusiness.GetAllHistoricalEdges(pid);
       
        Assert.NotNull(historicalEdges);
        Assert.Single(historicalEdges, e => !e.IsArchived);
        Assert.Contains(historicalEdges, e => e.Id == eid2);
        Assert.DoesNotContain(historicalEdges, e => e.Id == eid);
    }

    [Fact]
    public async Task GetAllHistoricalEdges_InlcudesArchivedHistoricalEdges()
    {
        await _edgeBusiness.ArchiveEdge(pid, eid, null, null);
    
        // Add this line to force EF to reload from database
        Context.ChangeTracker.Clear();
    
        var historicalEdges = await _historicalEdgeBusiness.GetAllHistoricalEdges(pid);
        Assert.NotNull(historicalEdges);
        Assert.Single(historicalEdges);
        Assert.DoesNotContain(historicalEdges, e => e.Id == eid && e.IsArchived);
        Assert.Contains(historicalEdges, e => e.Id == eid2 && !e.IsArchived);
    }
    
    #endregion
    
    #region GetHistoricalEdges Tests

    [Fact]
    public async Task GetHistoricalEdges_ReturnsEmptyListWhenNoEdges()
    {
        await _edgeBusiness.DeleteEdge(pid, eid, null, null);
        await _edgeBusiness.DeleteEdge(pid, eid2, null, null);
        var historicalEdges = await _historicalEdgeBusiness.GetAllHistoricalEdges(pid);
        Assert.NotNull(historicalEdges);
        Assert.Empty(historicalEdges);
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
        Assert.NotNull(historicalEdges);
        Assert.Equal(2, historicalEdges.Count());
        Assert.DoesNotContain(historicalEdges, e => e.Id == edgeLate.Id);
    }
    
    #endregion
    
    #region GetHistoryForEdge Tests

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
        Assert.NotNull(edgeHistory);
        Assert.Equal(4, edgeHistory.Count());
        Assert.All(edgeHistory, e => Assert.Equal(eid, e.Id));
        Assert.True(edgeHistory.First().IsArchived);
    }
    
    [Fact]
    public async Task GetHistoryForEdge_ThrowsError_WhenEdgeDoesNotExist()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _historicalEdgeBusiness.GetHistoryForEdge(eid4 + 100000, null, null));
    }
    
    #endregion
    
    #region GetHistoricalEdge Tests

    [Fact]
    public async Task GetHistoricalEdge_ThrowsError_WhenEdgeDoesNotExist()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _historicalEdgeBusiness.GetHistoricalEdge(eid4 + 100000, null, null, null));
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
        Assert.NotNull(historicalEdge);
        Assert.Equal(eid, historicalEdge.Id);
        Assert.Equal(destinationRecordId, historicalEdge.OriginId);
        Assert.Equal(destinationRecordId2, historicalEdge.DestinationId);
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
        Assert.NotNull(historicalEdge);
        Assert.Equal(eid, historicalEdge.Id);
        Assert.Equal(destinationRecordId, historicalEdge.OriginId);
        Assert.Equal(destinationRecordId2, historicalEdge.DestinationId);
        Assert.True(historicalEdge.IsArchived);
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
        Assert.NotNull(historicalEdge);
        Assert.Equal(eid, historicalEdge.Id);
        Assert.Equal(originRecordId, historicalEdge.OriginId);
        Assert.Equal(destinationRecordId, historicalEdge.DestinationId);
    }

    [Fact]
    public async Task GetHistoricalEdge_ReturnsArchivedHistoricalEdge_WhenEdgeIsArchived()
    {
        await _edgeBusiness.ArchiveEdge(pid, eid, null, null);
        var historicalEdge = () => _historicalEdgeBusiness.GetHistoricalEdge(eid, null, null, null);
        Assert.NotNull(historicalEdge);
    }
    
    #endregion
    
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
        var historicalEdge1 = new HistoricalEdge
        {
            EdgeId = edge.Id,
            OriginId = edge.OriginId,
            DestinationId = edge.DestinationId,
            RelationshipId = edge.RelationshipId,
            RelationshipName = "Relationship 1",
            DataSourceId = edge.DataSourceId,
            DataSourceName = "DataSource 1",
            ProjectId = edge.ProjectId,
            ProjectName = "Project 1",
            LastUpdatedBy = edge.LastUpdatedBy,
            LastUpdatedAt = edge.LastUpdatedAt,
            IsArchived = false
        };

        var historicalEdge2 = new HistoricalEdge
        {
            EdgeId = edge2.Id,
            OriginId = edge2.OriginId,
            DestinationId = edge2.DestinationId,
            RelationshipId = edge2.RelationshipId,
            RelationshipName = "Relationship 1",
            DataSourceId = edge2.DataSourceId,
            DataSourceName = "DataSource 3",
            ProjectId = edge2.ProjectId,
            ProjectName = "Project 1",
            LastUpdatedBy = edge2.LastUpdatedBy,
            LastUpdatedAt = edge2.LastUpdatedAt,
            IsArchived = false
        };

        Context.HistoricalEdges.Add(historicalEdge1);
        Context.HistoricalEdges.Add(historicalEdge2);
        await Context.SaveChangesAsync();
    }
}