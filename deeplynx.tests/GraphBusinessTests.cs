using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.helpers.Hubs;
using deeplynx.interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Record = deeplynx.datalayer.Models.Record;

namespace deeplynx.tests;

[Collection("Test Suite Collection")]
public class GraphBusinessTests : IntegrationTestBase
{
    private ClassBusiness _classBusiness = null!;
    private DataSourceBusiness _dataSourceBusiness = null!;
    private EdgeBusiness _edgeBusiness = null!;
    private EventBusiness _eventBusiness = null!;
    private GraphBusiness _graphBusiness = null!;
    private Mock<IHubContext<EventNotificationHub>> _mockHubContext = null!;
    private Mock<ILogger<ProjectBusiness>> _mockLogger = null!;
    private Mock<ILogger<NotificationBusiness>> _mockNotificationLogger = null!;
    private Mock<IObjectStorageBusiness> _mockObjectStorageBusiness = null!;
    private Mock<IOrganizationBusiness> _mockOrganizationBusiness = null!;
    private Mock<IRecordBusiness> _mockRecordBusiness = null!;
    private Mock<IRelationshipBusiness> _mockRelationshipBusiness = null!;
    private Mock<IRoleBusiness> _mockRoleBusiness = null!;
    private INotificationBusiness _notificationBusiness = null!;
    private ProjectBusiness _projectBusiness = null!;
    public long destinationRecordId;
    public long destinationRecordId2;
    public long destinationRecordId3;
    public long dsid;
    public long dsid2;
    public long oid;
    public long originRecordId;
    public long originRecordId2;

    public long pid;
    public long pid2;
    public long uid1;

    public GraphBusinessTests(TestSuiteFixture fixture) : base(fixture)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _mockRecordBusiness = new Mock<IRecordBusiness>();
        _mockRelationshipBusiness = new Mock<IRelationshipBusiness>();
        _mockLogger = new Mock<ILogger<ProjectBusiness>>();
        _mockObjectStorageBusiness = new Mock<IObjectStorageBusiness>();
        _mockRoleBusiness = new Mock<IRoleBusiness>();
        _mockHubContext = new Mock<IHubContext<EventNotificationHub>>();
        _mockNotificationLogger = new Mock<ILogger<NotificationBusiness>>();
        _notificationBusiness =
            new NotificationBusiness(Context, _mockNotificationLogger.Object, _mockHubContext.Object);
        _eventBusiness = new EventBusiness(Context, _cacheBusiness, _notificationBusiness);
        _mockOrganizationBusiness = new Mock<IOrganizationBusiness>();

        _graphBusiness = new GraphBusiness(Context, _cacheBusiness, _eventBusiness);
        _edgeBusiness = new EdgeBusiness(Context, _cacheBusiness, _eventBusiness);
        _dataSourceBusiness = new DataSourceBusiness(Context, _cacheBusiness, _edgeBusiness, _mockRecordBusiness.Object,
            _eventBusiness);
        _classBusiness = new ClassBusiness(
            Context, _cacheBusiness, _mockRecordBusiness.Object,
            _mockRelationshipBusiness.Object, _eventBusiness);

        _projectBusiness = new ProjectBusiness(
            Context, _cacheBusiness, _mockLogger.Object, _classBusiness,
            _mockRoleBusiness.Object, _dataSourceBusiness,
            _mockObjectStorageBusiness.Object, _eventBusiness, _mockOrganizationBusiness.Object);
    }

    protected override async Task SeedTestDataAsync()
    {
        await base.SeedTestDataAsync();

        var user = new User
        {
            Name = "Test User 1",
            Email = "test_email@example.com",
            Password = "test_password",
            IsArchived = false
        };
        Context.Users.Add(user);
        await Context.SaveChangesAsync();
        uid1 = user.Id;

        var org = new Organization { Name = "Test Org" };
        Context.Organizations.Add(org);
        await Context.SaveChangesAsync();
        oid = org.Id;

        var project = new Project { Name = "Project 1", OrganizationId = oid };
        Context.Projects.Add(project);
        await Context.SaveChangesAsync();
        pid = project.Id;

        var project2 = new Project { Name = "Project 2", OrganizationId = oid };
        Context.Projects.Add(project2);
        await Context.SaveChangesAsync();
        pid2 = project2.Id;

        var dataSource = new DataSource
        {
            Name = "DataSource 1",
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        Context.DataSources.Add(dataSource);
        await Context.SaveChangesAsync();
        dsid = dataSource.Id;

        var dataSource2 = new DataSource
        {
            Name = "DataSource 2",
            ProjectId = pid2,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        Context.DataSources.Add(dataSource2);
        await Context.SaveChangesAsync();
        dsid2 = dataSource2.Id;

        var testClass = new Class
        {
            Name = "Class 1",
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        Context.Classes.Add(testClass);
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
        Context.Records.Add(originRecord);

        var originRecord2 = new Record
        {
            ProjectId = pid2,
            DataSourceId = dsid2,
            Properties = "{\"test\": \"origin2_value\"}",
            Name = "Origin 2",
            Description = "Origin Description 2",
            OriginalId = "orig2",
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
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
        Context.Records.Add(destinationRecord);

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

        var destinationRecord3 = new Record
        {
            ProjectId = pid2,
            DataSourceId = dsid2,
            Properties = "{\"test\": \"destination3_value\"}",
            Name = "Destination 3",
            Description = "Destination Description 3",
            OriginalId = "dest3",
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        };
        Context.Records.Add(destinationRecord3);

        await Context.SaveChangesAsync();

        originRecordId = originRecord.Id;
        originRecordId2 = originRecord2.Id;
        destinationRecordId = destinationRecord.Id;
        destinationRecordId2 = destinationRecord2.Id;
        destinationRecordId3 = destinationRecord3.Id;
    }

    #region GetEdgesByRecord Tests

    [Fact]
    public async Task GetEdgesByRecord_FiltersByOriginRecord()
    {
        // Arrange
        var userAdded = await _projectBusiness.AddMemberToProject(pid, null, uid1, null);
        Assert.True(userAdded);

        var edge1 = new Edge
        {
            OriginId = originRecordId,
            DestinationId = destinationRecordId,
            DataSourceId = dsid,
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        var edge2 = new Edge
        {
            OriginId = originRecordId,
            DestinationId = destinationRecordId2,
            DataSourceId = dsid,
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        var edge3 = new Edge
        {
            OriginId = destinationRecordId,
            DestinationId = originRecordId,
            DataSourceId = dsid,
            ProjectId = pid2,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        var edge4 = new Edge
        {
            OriginId = destinationRecordId3,
            DestinationId = originRecordId,
            DataSourceId = dsid,
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        var edge5 = new Edge
        {
            OriginId = originRecordId2,
            DestinationId = originRecordId,
            DataSourceId = dsid,
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        Context.Edges.Add(edge1);
        Context.Edges.Add(edge2);
        Context.Edges.Add(edge3);
        Context.Edges.Add(edge4);
        Context.Edges.Add(edge5);

        await Context.SaveChangesAsync();

        // Act
        var edges = await _graphBusiness.GetEdgesByRecord(originRecordId, true, 1, true, 20);

        // Assert
        Assert.Equal(2, edges.Count);
        Assert.Contains(edges, r => r.RelatedRecordName == "Destination 1" &&
                                    r.RelationshipName == null && r.RelatedRecordId == destinationRecordId &&
                                    r.RelatedRecordProjectId == pid);
        Assert.Contains(edges, r => r.RelatedRecordName == "Destination 2" &&
                                    r.RelationshipName == null && r.RelatedRecordId == destinationRecordId2 &&
                                    r.RelatedRecordProjectId == pid);
        Assert.DoesNotContain(edges, r => r.RelatedRecordName == "Destination 3");
        Assert.DoesNotContain(edges, r => r.RelatedRecordName == "Origin 2");
    }

    [Fact]
    public async Task GetEdgesByRecord_FiltersByDestinationRecord()
    {
        // Arrange
        var userAdded = await _projectBusiness.AddMemberToProject(pid, null, uid1, null);
        Assert.True(userAdded);

        var edge1 = new Edge
        {
            OriginId = originRecordId,
            DestinationId = destinationRecordId,
            DataSourceId = dsid,
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        var edge2 = new Edge
        {
            OriginId = originRecordId,
            DestinationId = destinationRecordId2,
            DataSourceId = dsid,
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        var edge3 = new Edge
        {
            OriginId = destinationRecordId,
            DestinationId = originRecordId,
            DataSourceId = dsid,
            ProjectId = pid2,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        var edge4 = new Edge
        {
            OriginId = destinationRecordId3,
            DestinationId = originRecordId,
            DataSourceId = dsid,
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        var edge5 = new Edge
        {
            OriginId = originRecordId2,
            DestinationId = originRecordId,
            DataSourceId = dsid,
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        Context.Edges.Add(edge1);
        Context.Edges.Add(edge2);
        Context.Edges.Add(edge3);
        Context.Edges.Add(edge4);
        Context.Edges.Add(edge5);

        await Context.SaveChangesAsync();

        // Act
        var edges = await _graphBusiness.GetEdgesByRecord(originRecordId, false, 1, true, 20);

        // Assert
        Assert.Equal(3, edges.Count);
        Assert.Contains(edges, r => r.RelatedRecordName == "Destination 3" &&
                                    r.RelationshipName == null && r.RelatedRecordId == destinationRecordId3 &&
                                    r.RelatedRecordProjectId == pid2);
        Assert.Contains(edges, r => r.RelatedRecordName == "Origin 2" &&
                                    r.RelationshipName == null && r.RelatedRecordId == originRecordId2 &&
                                    r.RelatedRecordProjectId == pid2);
        Assert.Contains(edges, r => r.RelatedRecordName == "Destination 1" &&
                                    r.RelationshipName == null && r.RelatedRecordId == destinationRecordId &&
                                    r.RelatedRecordProjectId == pid);
    }

    [Fact]
    public async Task GetEdgesByRecord_FiltersByPage()
    {
        // Arrange
        var userAdded = await _projectBusiness.AddMemberToProject(pid, null, uid1, null);
        Assert.True(userAdded);

        var edge1 = new Edge
        {
            OriginId = originRecordId,
            DestinationId = destinationRecordId,
            DataSourceId = dsid,
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        var edge2 = new Edge
        {
            OriginId = originRecordId,
            DestinationId = destinationRecordId2,
            DataSourceId = dsid,
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        var edge3 = new Edge
        {
            OriginId = destinationRecordId,
            DestinationId = originRecordId,
            DataSourceId = dsid,
            ProjectId = pid2,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        var edge4 = new Edge
        {
            OriginId = destinationRecordId3,
            DestinationId = originRecordId,
            DataSourceId = dsid,
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        var edge5 = new Edge
        {
            OriginId = originRecordId2,
            DestinationId = originRecordId,
            DataSourceId = dsid,
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        Context.Edges.Add(edge1);
        Context.Edges.Add(edge2);
        Context.Edges.Add(edge3);
        Context.Edges.Add(edge4);
        Context.Edges.Add(edge5);

        await Context.SaveChangesAsync();

        // Act
        var edges = await _graphBusiness.GetEdgesByRecord(originRecordId, false, 1, true, 2);

        // Assert
        Assert.Equal(2, edges.Count);
        Assert.Contains(edges, r => r.RelatedRecordName == "Destination 1" &&
                                    r.RelationshipName == null && r.RelatedRecordId == destinationRecordId &&
                                    r.RelatedRecordProjectId == pid);
        Assert.Contains(edges, r => r.RelatedRecordName == "Destination 3" &&
                                    r.RelationshipName == null && r.RelatedRecordId == destinationRecordId3 &&
                                    r.RelatedRecordProjectId == pid2);
        Assert.DoesNotContain(edges, r => r.RelatedRecordName == "Origin 2");


        var edges2 = await _graphBusiness.GetEdgesByRecord(originRecordId, false, 2, true, 2);
        Assert.Single(edges2);
        Assert.Contains(edges2, r => r.RelatedRecordName == "Origin 2" &&
                                     r.RelationshipName == null && r.RelatedRecordId == originRecordId2 &&
                                     r.RelatedRecordProjectId == pid2);
    }

    [Fact]
    public async Task GetEdgesByRecord_Fails_IfRecordDoesNotExist()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _graphBusiness.GetEdgesByRecord(originRecordId + 1000, true, 1, true, 20));
    }

    [Fact]
    public async Task GetEdgesByRecord_Fails_IfPageis0()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _graphBusiness.GetEdgesByRecord(originRecordId, true, 0, true, 20));
    }

    [Fact]
    public async Task GetEdgesByRecord_Fails_IfPageSizeIsO()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _graphBusiness.GetEdgesByRecord(originRecordId, true, 1, true, 0));
    }

    [Fact]
    public async Task GetEdgesByRecord_Fails_IfPageSizeIsOver10O()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _graphBusiness.GetEdgesByRecord(originRecordId, true, 1, true, 101));
    }

    #endregion

    #region GetGraphData Tests

    [Fact]
    public async Task GetGraphData_GetsCorrectNodesAndLinks()
    {
        var userAddedProject1 = await _projectBusiness.AddMemberToProject(pid, null, uid1, null);
        var userAddedProject2 = await _projectBusiness.AddMemberToProject(pid2, null, uid1, null);
        Assert.True(userAddedProject1 && userAddedProject2);

        var edge1 = new Edge
        {
            OriginId = originRecordId,
            DestinationId = destinationRecordId,
            DataSourceId = dsid,
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        var edge2 = new Edge
        {
            OriginId = originRecordId,
            DestinationId = destinationRecordId2,
            DataSourceId = dsid,
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        var edge3 = new Edge
        {
            OriginId = destinationRecordId,
            DestinationId = originRecordId,
            DataSourceId = dsid,
            ProjectId = pid2,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        var edge4 = new Edge
        {
            OriginId = destinationRecordId2,
            DestinationId = destinationRecordId3,
            DataSourceId = dsid,
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        var edge5 = new Edge
        {
            OriginId = destinationRecordId3,
            DestinationId = originRecordId2,
            DataSourceId = dsid,
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        Context.Edges.Add(edge1);
        Context.Edges.Add(edge2);
        Context.Edges.Add(edge3);
        Context.Edges.Add(edge4);
        Context.Edges.Add(edge5);

        await Context.SaveChangesAsync();

        var graphData = await _graphBusiness.GetGraphDataForRecord(originRecordId, uid1, 3);
        Assert.Equal(5, graphData.Nodes?.Count);
        Assert.Equal(5, graphData.Links?.Count);

        // Create expected node IDs set
        var expectedNodeIds = new HashSet<long>
        {
            originRecordId,
            destinationRecordId,
            destinationRecordId2,
            destinationRecordId3,
            originRecordId2
        };

        var actualNodeIds = graphData.Nodes?.Select(n => n.Id).ToHashSet();
        Assert.Equal(expectedNodeIds, actualNodeIds);

        // Create expected links (Source, Target pairs)
        var expectedLinks = new HashSet<(long source, long target)>
        {
            (originRecordId, destinationRecordId),
            (originRecordId, destinationRecordId2),
            (destinationRecordId, originRecordId),
            (destinationRecordId2, destinationRecordId3),
            (destinationRecordId3, originRecordId2)
        };

        var actualLinks = graphData.Links?.Select(l => (l.Source, l.Target)).ToHashSet();
        Assert.Equal(expectedLinks, actualLinks);
    }

    [Fact]
    public async Task GetGraphData_FiltersByUserProject()
    {
        var userAddedProject1 = await _projectBusiness.AddMemberToProject(pid, null, uid1, null);
        Assert.True(userAddedProject1);

        var edge1 = new Edge
        {
            OriginId = originRecordId,
            DestinationId = destinationRecordId,
            DataSourceId = dsid,
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        var edge2 = new Edge
        {
            OriginId = originRecordId,
            DestinationId = destinationRecordId2,
            DataSourceId = dsid,
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        // edge in restricted project
        var edge3 = new Edge
        {
            OriginId = destinationRecordId,
            DestinationId = originRecordId,
            DataSourceId = dsid,
            ProjectId = pid2,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        // destination is in restricted project
        var edge4 = new Edge
        {
            OriginId = destinationRecordId2,
            DestinationId = destinationRecordId3,
            DataSourceId = dsid,
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        // origin is in restricted project
        var edge5 = new Edge
        {
            OriginId = destinationRecordId3,
            DestinationId = originRecordId2,
            DataSourceId = dsid,
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        Context.Edges.Add(edge1);
        Context.Edges.Add(edge2);
        Context.Edges.Add(edge3);
        Context.Edges.Add(edge4);
        Context.Edges.Add(edge5);

        await Context.SaveChangesAsync();

        var graphData = await _graphBusiness.GetGraphDataForRecord(originRecordId, uid1, 3);
        Assert.Equal(3, graphData.Nodes?.Count);
        Assert.Equal(2, graphData.Links?.Count);

        // Create expected node IDs set
        var expectedNodeIds = new HashSet<long>
        {
            originRecordId,
            destinationRecordId,
            destinationRecordId2
        };

        var actualNodeIds = graphData.Nodes?.Select(n => n.Id).ToHashSet();
        Assert.Equal(expectedNodeIds, actualNodeIds);

        // Create expected links (Source, Target pairs)
        var expectedLinks = new HashSet<(long source, long target)>
        {
            (originRecordId, destinationRecordId),
            (originRecordId, destinationRecordId2)
        };

        var actualLinks = graphData.Links?.Select(l => (l.Source, l.Target)).ToHashSet();
        Assert.Equal(expectedLinks, actualLinks);
    }

    [Fact]
    public async Task GetGraphData_FiltersByDepth()
    {
        var userAddedProject1 = await _projectBusiness.AddMemberToProject(pid, null, uid1, null);
        var userAddedProject2 = await _projectBusiness.AddMemberToProject(pid2, null, uid1, null);
        Assert.True(userAddedProject1 && userAddedProject2);

        var edge1 = new Edge
        {
            OriginId = originRecordId,
            DestinationId = destinationRecordId,
            DataSourceId = dsid,
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        var edge2 = new Edge
        {
            OriginId = originRecordId,
            DestinationId = destinationRecordId2,
            DataSourceId = dsid,
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        var edge3 = new Edge
        {
            OriginId = destinationRecordId,
            DestinationId = originRecordId,
            DataSourceId = dsid,
            ProjectId = pid2,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        var edge4 = new Edge
        {
            OriginId = destinationRecordId2,
            DestinationId = destinationRecordId3,
            DataSourceId = dsid,
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        var edge5 = new Edge
        {
            OriginId = destinationRecordId3,
            DestinationId = originRecordId2,
            DataSourceId = dsid,
            ProjectId = pid,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null
        };

        Context.Edges.Add(edge1);
        Context.Edges.Add(edge2);
        Context.Edges.Add(edge3);
        Context.Edges.Add(edge4);
        Context.Edges.Add(edge5);

        await Context.SaveChangesAsync();

        var graphData = await _graphBusiness.GetGraphDataForRecord(originRecordId, uid1, 1);
        Assert.Equal(3, graphData.Nodes?.Count);
        Assert.Equal(3, graphData.Links?.Count);

        // Create expected node IDs set
        var expectedNodeIds = new HashSet<long>
        {
            originRecordId,
            destinationRecordId,
            destinationRecordId2
        };

        var actualNodeIds = graphData.Nodes?.Select(n => n.Id).ToHashSet();
        Assert.Equal(expectedNodeIds, actualNodeIds);

        // Create expected links (source, target pairs)
        var expectedLinks = new HashSet<(long source, long target)>
        {
            (originRecordId, destinationRecordId),
            (originRecordId, destinationRecordId2),
            (destinationRecordId, originRecordId)
        };

        var actualLinks = graphData.Links?.Select(l => (l.Source, l.Target)).ToHashSet();
        Assert.Equal(expectedLinks, actualLinks);
    }

    [Fact]
    public async Task GetGraphData_Fails_IfRecordDoesNotExist()
    {
        var graphData = () => _graphBusiness.GetGraphDataForRecord(originRecordId + 1000, uid1, 1);
        await Assert.ThrowsAsync<KeyNotFoundException>(graphData);
    }

    [Fact]
    public async Task GetGraphData_Fails_IfUserIsRestricted()
    {
        var userAddedProject2 = await _projectBusiness.AddMemberToProject(pid2, null, uid1, null);
        Assert.True(userAddedProject2);
        var graphData = () => _graphBusiness.GetGraphDataForRecord(originRecordId, uid1, 1);
        await Assert.ThrowsAsync<AccessViolationException>(graphData);
    }

    #endregion
}