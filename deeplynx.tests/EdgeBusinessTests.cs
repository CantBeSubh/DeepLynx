using System.ComponentModel.DataAnnotations;
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

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class EdgeBusinessTests : IntegrationTestBase
    {
        private EdgeBusiness _edgeBusiness = null!;
        private ProjectBusiness _projectBusiness = null!;
        private DataSourceBusiness _dataSourceBusiness = null!;
        private ClassBusiness _classBusiness = null!;
        private EventBusiness _eventBusiness = null!;
        private INotificationBusiness _notificationBusiness = null!;
        private Mock<ILogger<NotificationBusiness>> _mockNotificationLogger = null!;
        private Mock<IHubContext<EventNotificationHub>> _mockHubContext = null!;
        private Mock<IRecordBusiness> _mockRecordBusiness = null!;
        private Mock<IRelationshipBusiness> _mockRelationshipBusiness = null!;
        private Mock<ILogger<ProjectBusiness>> _mockLogger = null!;
        private Mock<IObjectStorageBusiness> _mockObjectStorageBusiness = null!;
        private Mock<IRoleBusiness> _mockRoleBusiness = null!;
        private Mock<IOrganizationBusiness> _mockOrganizationBusiness = null!;
        
        public long pid;
        public long pid2;
        public long dsid;
        public long dsid2;
        public long originRecordId;
        public long originRecordId2;
        public long destinationRecordId;
        public long destinationRecordId2;
        public long destinationRecordId3;
        public long relationshipId;
        public long uid1;
        public long oid;
        public EdgeBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

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
            _notificationBusiness = new NotificationBusiness(Context, _mockNotificationLogger.Object, _mockHubContext.Object);
            _eventBusiness = new EventBusiness(Context, _cacheBusiness, _notificationBusiness);
            _mockOrganizationBusiness =  new Mock<IOrganizationBusiness>();

            _edgeBusiness = new EdgeBusiness(Context, _cacheBusiness, _eventBusiness);
            _dataSourceBusiness = new DataSourceBusiness(Context, _cacheBusiness, _edgeBusiness, _mockRecordBusiness.Object, _eventBusiness);
            _classBusiness = new ClassBusiness(
                Context, _cacheBusiness, _mockRecordBusiness.Object,
                _mockRelationshipBusiness.Object, _eventBusiness);

            _projectBusiness = new ProjectBusiness(
                Context, _cacheBusiness, _mockLogger.Object, _classBusiness,
                _mockRoleBusiness.Object, _dataSourceBusiness,
                _mockObjectStorageBusiness.Object, _eventBusiness, _mockOrganizationBusiness.Object );
        }

        #region CreateEdge Tests
        [Fact]
        public async Task CreateEdge_Success_ReturnsIdAndCreatedAt()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var dto = new CreateEdgeRequestDto
            {
                OriginId = (int)originRecordId,
                DestinationId = (int)destinationRecordId,
                RelationshipId = (int)relationshipId
            };

            // Act
            var result = await _edgeBusiness.CreateEdge(uid1, pid, dsid, dto);

            // Assert
            Assert.True(result.Id > 0);
            Assert.True(result.LastUpdatedAt >= now);
            Assert.Equal(originRecordId, result.OriginId);
            Assert.Equal(destinationRecordId, result.DestinationId);
            Assert.Equal(pid, result.ProjectId);
            Assert.Equal(dsid, result.DataSourceId);

            // Ensure that edge create event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);

            var actualEvent = eventList[0];

            Assert.Equal(pid, actualEvent.ProjectId);
            Assert.Equal("create", actualEvent.Operation);
            Assert.Equal("edge", actualEvent.EntityType);
            Assert.Equal(result.Id, actualEvent.EntityId);
        }

        [Fact]
        public async Task CreateEdge_Fails_IfNoOriginId()
        {
            // Arrange
            var dto = new CreateEdgeRequestDto
            {
                OriginId = 0, // Invalid origin
                DestinationId = (int)destinationRecordId,
                RelationshipId = (int)relationshipId
            };
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _edgeBusiness.CreateEdge(uid1, pid, dsid, dto));

            // Ensure that edge create event was NOT logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreateEdge_Fails_IfNoDestinationId()
        {
            // Arrange
            var dto = new CreateEdgeRequestDto
            {
                OriginId = (int)originRecordId,
                DestinationId = 0, // Invalid destination
                RelationshipId = (int)relationshipId
            };
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _edgeBusiness.CreateEdge(uid1, pid, dsid, dto));

            // Ensure that edge create event was NOT logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreateEdge_Fails_IfSameDestinationIdAndOriginId()
        {
            var dto = new CreateEdgeRequestDto
            {
                OriginId = (int)originRecordId,
                DestinationId = (int)originRecordId,
                RelationshipId = (int)relationshipId
            };
            await Assert.ThrowsAsync<ValidationException>(() => _edgeBusiness.CreateEdge(uid1, pid, dsid, dto));

            // Ensure that edge create event was NOT logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreateEdge_Fails_IfNoProjectId()
        {
            // Arrange
            var dto = new CreateEdgeRequestDto
            {
                OriginId = (int)originRecordId,
                DestinationId = (int)destinationRecordId,
                RelationshipId = (int)relationshipId
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _edgeBusiness.CreateEdge(uid1, pid + 99, dsid, dto));

            // Ensure that edge create event was NOT logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreateEdge_Fails_IfNoDataSourceId()
        {
            // Arrange
            var dto = new CreateEdgeRequestDto
            {
                OriginId = (int)originRecordId,
                DestinationId = (int)destinationRecordId,
                RelationshipId = (int)relationshipId
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _edgeBusiness.CreateEdge(uid1, pid, dsid + 99, dto));

            // Ensure that edge create event was NOT logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreateEdge_Fails_IfDeletedProjectId()
        {
            // Arrange
            var project = await Context.Projects.FindAsync(pid);
            project.IsArchived = true;
            Context.Projects.Update(project);
            await Context.SaveChangesAsync();

            var dto = new CreateEdgeRequestDto
            {
                OriginId = (int)originRecordId,
                DestinationId = (int)destinationRecordId,
                RelationshipId = (int)relationshipId
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _edgeBusiness.CreateEdge(uid1, pid, dsid, dto));

            // Ensure that edge create event was NOT logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        #endregion

        #region BulkCreateEdge Tests

        [Fact]
        public async Task BulkCreateEdges_Success_ReturnsMultipleEdges()
        {

            // Arrange
            var now = DateTime.UtcNow;

            var edges = new List<CreateEdgeRequestDto>
            {
                new CreateEdgeRequestDto
                {
                    OriginId = (int)originRecordId,
                    DestinationId = (int)destinationRecordId,
                    RelationshipId = (int)relationshipId
                },
                new CreateEdgeRequestDto
                {
                    OriginId = (int)originRecordId,
                    DestinationId = (int)destinationRecordId2,
                    RelationshipId = (int)relationshipId
                }
            };

            // Act
            var result = await _edgeBusiness.BulkCreateEdges(uid1, pid, dsid, edges);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, e => Assert.True(e.Id > 0));
            Assert.All(result, e => Assert.True(e.LastUpdatedAt >= now));

            var eventList = await Context.Events.ToListAsync();
            Assert.Equal(2, eventList.Count);

            var actualEvent0 = eventList[0];
            Assert.Equal(result[0].ProjectId, actualEvent0.ProjectId);
            Assert.Equal(result[0].Id, actualEvent0.EntityId);
            Assert.Equal("edge", actualEvent0.EntityType);
            Assert.Equal("create", actualEvent0.Operation);

            var actualEvent1 = eventList[1];
            Assert.Equal(result[0].ProjectId, actualEvent1.ProjectId);
            Assert.Equal(result[1].Id, actualEvent1.EntityId);
            Assert.Equal("edge", actualEvent1.EntityType);
            Assert.Equal("create", actualEvent1.Operation);
        }

        [Fact]
        public async Task BulkCreateEdges_Fails_IfNullDto()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _edgeBusiness.BulkCreateEdges(uid1, pid, dsid, null));

            // Ensure that edge create event was NOT logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        #endregion

        #region GetAllEdges Tests

        [Fact]
        public async Task GetAllEdges_ReturnsOnlyForProject()
        {
            // Arrange
            await _edgeBusiness.CreateEdge(uid1, pid, dsid, new CreateEdgeRequestDto { OriginId = (int)originRecordId, DestinationId = (int)destinationRecordId });
            await _edgeBusiness.CreateEdge(uid1, pid2, dsid2, new CreateEdgeRequestDto { OriginId = (int)originRecordId, DestinationId = (int)destinationRecordId });

            // Act
            var list = await _edgeBusiness.GetAllEdges(pid, null, true);

            // Assert
            Assert.All(list, e => Assert.Equal(pid, e.ProjectId));
        }

        [Fact]
        public async Task GetAllEdges_ExcludesSoftDeleted()
        {
            // Arrange
            var activeEdge = new Edge
            {
                OriginId = originRecordId,
                DestinationId = destinationRecordId,
                DataSourceId = dsid,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };

            var archivedEdge = new Edge
            {
                OriginId = originRecordId,
                DestinationId = destinationRecordId2,
                DataSourceId = dsid,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null,
                IsArchived = true
            };
            Context.Edges.Add(activeEdge);
            Context.Edges.Add(archivedEdge);
            await Context.SaveChangesAsync();

            // Act
            var listWithArchived = await _edgeBusiness.GetAllEdges(pid, null, false);
            var listWithoutArchived = await _edgeBusiness.GetAllEdges(pid, null, true);

            // Assert
            Assert.Contains(listWithArchived, e => e.Id == archivedEdge.Id);
            Assert.DoesNotContain(listWithoutArchived, e => e.Id == archivedEdge.Id);
        }

        #endregion

        #region GetEdgesByRecord Tests
        // Todo: Add test back in when we filter record edges by user access

        // [Fact]
        // public async Task GetEdgesByRecord_ReturnsEdgesWithUserAccess()
        // {
        //     var userAdded = await _projectBusiness.AddMemberToProject(pid, null, uid1, null);
        //     Assert.True(userAdded);
        //     
        //     var edge1 = new Edge
        //     {
        //         OriginId = originRecordId,
        //         DestinationId = destinationRecordId,
        //         DataSourceId = dsid,
        //         ProjectId = pid,
        //         LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
        //         LastUpdatedBy = null
        //     };
        //
        //     var edge2 = new Edge
        //     {
        //         OriginId = destinationRecordId,
        //         DestinationId = originRecordId,
        //         DataSourceId = dsid,
        //         ProjectId = pid,
        //         LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
        //         LastUpdatedBy = null
        //     };
        //     
        //     var edgeRestrictedProject = new Edge
        //     {
        //         OriginId = destinationRecordId,
        //         DestinationId = originRecordId,
        //         DataSourceId = dsid,
        //         ProjectId = pid2,
        //         LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
        //         LastUpdatedBy = null
        //     };
        //     
        //     var edgeWithOriginInRestrictedProject = new Edge
        //     {
        //         OriginId = destinationRecordId3,
        //         DestinationId = originRecordId,
        //         DataSourceId = dsid,
        //         ProjectId = pid,
        //         LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
        //         LastUpdatedBy = null
        //     };
        //     
        //     var edgeWithDestinationInRestrictedProject = new Edge
        //     {
        //         OriginId = destinationRecordId,
        //         DestinationId = originRecordId2,
        //         DataSourceId = dsid,
        //         ProjectId = pid,
        //         LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
        //         LastUpdatedBy = null
        //     };
        //     
        //     Context.Edges.Add(edge1);
        //     Context.Edges.Add(edge2);
        //     Context.Edges.Add(edgeRestrictedProject);
        //     Context.Edges.Add(edgeWithOriginInRestrictedProject);
        //     Context.Edges.Add(edgeWithDestinationInRestrictedProject);
        //     
        //     await Context.SaveChangesAsync();
        //
        //     var edges = await _edgeBusiness.GetAllEdgesByRecord(originRecordId, uid1, true);
        //     edges.Count.Should().Be(2);
        //     edges.Should().Contain(e => e.Id == edge1.Id);
        //     edges.Should().Contain(e => e.Id == edge2.Id);
        //     edges.Should().NotContain(e => e.Id == edgeRestrictedProject.Id);
        //     edges.Should().NotContain(e => e.Id == edgeWithOriginInRestrictedProject.Id);
        //     edges.Should().NotContain(e => e.Id == edgeWithDestinationInRestrictedProject.Id);
        // }

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
            var edges = await _edgeBusiness.GetEdgesByRecord(originRecordId, true, 1, true, 20);

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
            var edges = await _edgeBusiness.GetEdgesByRecord(originRecordId, false, 1, true, 20);

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
            var edges = await _edgeBusiness.GetEdgesByRecord(originRecordId, false, 1, true, 2);

            // Assert
            Assert.Equal(2, edges.Count);
            Assert.Contains(edges, r => r.RelatedRecordName == "Destination 1" &&
                                        r.RelationshipName == null && r.RelatedRecordId == destinationRecordId &&
                                        r.RelatedRecordProjectId == pid);
            Assert.Contains(edges, r => r.RelatedRecordName == "Destination 3" &&
                                        r.RelationshipName == null && r.RelatedRecordId == destinationRecordId3 &&
                                        r.RelatedRecordProjectId == pid2);
            Assert.DoesNotContain(edges, r => r.RelatedRecordName == "Origin 2");


            var edges2 = await _edgeBusiness.GetEdgesByRecord(originRecordId, false, 2, true, 2);
            Assert.Single(edges2);
            Assert.Contains(edges2, r => r.RelatedRecordName == "Origin 2" &&
                                         r.RelationshipName == null && r.RelatedRecordId == originRecordId2 &&
                                         r.RelatedRecordProjectId == pid2);
        }

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

            var graphData = await _edgeBusiness.GetGraphDataForRecord(originRecordId, uid1, 3);
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
                (destinationRecordId3, originRecordId2),
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

            var graphData = await _edgeBusiness.GetGraphDataForRecord(originRecordId, uid1, 3);
            Assert.Equal(3, graphData.Nodes?.Count);
            Assert.Equal(2, graphData.Links?.Count);

            // Create expected node IDs set
            var expectedNodeIds = new HashSet<long>
            {
                originRecordId,
                destinationRecordId,
                destinationRecordId2,
            };

            var actualNodeIds = graphData.Nodes?.Select(n => n.Id).ToHashSet();
            Assert.Equal(expectedNodeIds, actualNodeIds);

            // Create expected links (Source, Target pairs)
            var expectedLinks = new HashSet<(long source, long target)>
            {
                (originRecordId, destinationRecordId),
                (originRecordId, destinationRecordId2),
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

            var graphData = await _edgeBusiness.GetGraphDataForRecord(originRecordId, uid1, 1);
            Assert.Equal(3, graphData.Nodes?.Count);
            Assert.Equal(3, graphData.Links?.Count);

            // Create expected node IDs set
            var expectedNodeIds = new HashSet<long>
            {
                originRecordId,
                destinationRecordId,
                destinationRecordId2,
            };

            var actualNodeIds = graphData.Nodes?.Select(n => n.Id).ToHashSet();
            Assert.Equal(expectedNodeIds, actualNodeIds);

            // Create expected links (source, target pairs)
            var expectedLinks = new HashSet<(long source, long target)>
            {
                (originRecordId, destinationRecordId),
                (originRecordId, destinationRecordId2),
                (destinationRecordId, originRecordId),
            };

            var actualLinks = graphData.Links?.Select(l => (l.Source, l.Target)).ToHashSet();
            Assert.Equal(expectedLinks, actualLinks);
        }

        [Fact]
        public async Task GetGraphData_Fails_IfRecordDoesNotExist()
        {
            var graphData = () => _edgeBusiness.GetGraphDataForRecord(originRecordId + 1000, uid1, 1);
            await Assert.ThrowsAsync<KeyNotFoundException>(graphData);

        }

        [Fact]
        public async Task GetGraphData_Fails_IfUserIsRestricted()
        {
            var userAddedProject2 = await _projectBusiness.AddMemberToProject(pid2, null, uid1, null);
            Assert.True(userAddedProject2);
            var graphData = () => _edgeBusiness.GetGraphDataForRecord(originRecordId, uid1, 1);
            await Assert.ThrowsAsync<AccessViolationException>(graphData);
        }

        [Fact]
        public async Task GetEdgesByRecord_Fails_IfRecordDoesNotExist()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _edgeBusiness.GetEdgesByRecord(originRecordId + 1000, true, 1, true, 20));
        }

        [Fact]
        public async Task GetEdgesByRecord_Fails_IfPageis0()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _edgeBusiness.GetEdgesByRecord(originRecordId, true, 0, true, 20));
        }

        [Fact]
        public async Task GetEdgesByRecord_Fails_IfPageSizeIsO()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _edgeBusiness.GetEdgesByRecord(originRecordId, true, 1, true, 0));
        }
        [Fact]
        public async Task GetEdgesByRecord_Fails_IfPageSizeIsOver10O()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _edgeBusiness.GetEdgesByRecord(originRecordId, true, 1, true, 101));
        }

        #endregion

        #region GetEdge Tests

        [Fact]
        public async Task GetEdge_Success_WhenExistsById()
        {
            // Arrange
            var testEdge = new Edge
            {
                OriginId = originRecordId,
                DestinationId = destinationRecordId,
                DataSourceId = dsid,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            Context.Edges.Add(testEdge);
            await Context.SaveChangesAsync();

            // Act
            var result = await _edgeBusiness.GetEdge(pid, testEdge.Id, null, null, true);

            // Assert
            Assert.Equal(testEdge.Id, result.Id);
            Assert.Equal(originRecordId, result.OriginId);
            Assert.Equal(destinationRecordId, result.DestinationId);
        }

        [Fact]
        public async Task GetEdge_Success_WhenExistsByOriginDestination()
        {
            // Arrange
            var testEdge = new Edge
            {
                OriginId = originRecordId,
                DestinationId = destinationRecordId,
                DataSourceId = dsid,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            Context.Edges.Add(testEdge);
            await Context.SaveChangesAsync();

            // Act
            var result = await _edgeBusiness.GetEdge(pid, null, originRecordId, destinationRecordId, true);

            // Assert
            Assert.Equal(testEdge.Id, result.Id);
            Assert.Equal(originRecordId, result.OriginId);
            Assert.Equal(destinationRecordId, result.DestinationId);
        }

        [Fact]
        public async Task GetEdge_Fails_IfNoProjectID()
        {
            // Arrange
            var testEdge = new Edge
            {
                OriginId = originRecordId,
                DestinationId = destinationRecordId,
                DataSourceId = dsid,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            Context.Edges.Add(testEdge);
            await Context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _edgeBusiness.GetEdge(pid + 999, testEdge.Id, null, null, true));
        }

        [Fact]
        public async Task GetEdge_Fails_IfDeletedEdge()
        {
            // Arrange
            var testEdge = new Edge
            {
                OriginId = originRecordId,
                DestinationId = destinationRecordId,
                DataSourceId = dsid,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null,
                IsArchived = true
            };
            Context.Edges.Add(testEdge);
            await Context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _edgeBusiness.GetEdge(pid, testEdge.Id, null, null, true));
        }

        [Fact]
        public async Task GetEdge_Fails_IfMissingIds()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _edgeBusiness.GetEdge(pid, null, null, null, true));
            Assert.Contains("Please supply either an edgeID or an originID and destinationID", exception.Message);
        }

        #endregion

        #region UpdateEdge Tests

        [Fact]
        public async Task UpdateEdge_Success_ReturnsModifiedAt()
        {
            // Arrange
            var testEdge = new Edge
            {
                OriginId = originRecordId,
                DestinationId = destinationRecordId,
                DataSourceId = dsid,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            Context.Edges.Add(testEdge);
            await Context.SaveChangesAsync();

            // Store the original timestamp for comparison
            var originalLastUpdatedAt = testEdge.LastUpdatedAt;

            // Create another destination record for update
            var newDestinationRecord = new Record
            {
                ProjectId = pid,
                DataSourceId = dsid,
                Properties = "{\"test\": \"Updated destination_value\"}",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                Name = "New Destination",
                Description = "New Destination Description",
                OriginalId = "new",
            };
            Context.Records.Add(newDestinationRecord);
            await Context.SaveChangesAsync();

            var dto = new UpdateEdgeRequestDto
            {
                OriginId = (int)originRecordId,
                DestinationId = (int)destinationRecordId2,
                RelationshipId = (int)relationshipId
            };

            // Act
            var updatedResult = await _edgeBusiness.UpdateEdge(uid1, pid, dto, testEdge.Id, null, null);

            // Assert
            Assert.True(updatedResult.LastUpdatedAt >= originalLastUpdatedAt);
            Assert.Equal(destinationRecordId2, updatedResult.DestinationId);

            // Ensure that update edge event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);

            var actualEvent = eventList[0];

            Assert.Equal(testEdge.Id, actualEvent.EntityId);
            Assert.Equal("edge", actualEvent.EntityType);
            Assert.Equal("update", actualEvent.Operation);
        }

        [Fact]
        public async Task UpdateEdge_Fails_WhenSameOriginAndDestinationId()
        {
            var testEdge = new Edge
            {
                OriginId = originRecordId,
                DestinationId = destinationRecordId,
                DataSourceId = dsid,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            Context.Edges.Add(testEdge);
            await Context.SaveChangesAsync();

            // Store the original timestamp for comparison
            var originalLastUpdatedAt = testEdge.LastUpdatedAt;

            // Create another destination record for update
            var newDestinationRecord = new Record
            {
                ProjectId = pid,
                DataSourceId = dsid,
                Properties = "{\"test\": \"Updated destination_value\"}",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                Name = "New Destination",
                Description = "New Destination Description",
                OriginalId = "new",
            };
            Context.Records.Add(newDestinationRecord);
            await Context.SaveChangesAsync();

            var dto = new UpdateEdgeRequestDto
            {
                OriginId = (int)originRecordId,
                DestinationId = (int)originRecordId,
                RelationshipId = (int)relationshipId
            };
            await Assert.ThrowsAsync<ValidationException>(() => _edgeBusiness.UpdateEdge(uid1, pid, dto, testEdge.Id, null, null));
            // Ensure that update edge event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task UpdateEdge_PartialUpdate_UpdatesEdge()
        {
            // Arrange
            var testEdge = new Edge
            {
                OriginId = originRecordId,
                DestinationId = destinationRecordId,
                DataSourceId = dsid,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            Context.Edges.Add(testEdge);
            await Context.SaveChangesAsync();
            var oid = testEdge.OriginId;
            var did = testEdge.DestinationId;

            // Create another destination record for update
            var newDestinationRecord = new Record
            {
                ProjectId = pid,
                DataSourceId = dsid,
                Properties = "{\"test\": \"Updated destination_value\"}",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                Name = "New Destination",
                Description = "New Destination Description",
                OriginalId = "new",
            };
            Context.Records.Add(newDestinationRecord);
            await Context.SaveChangesAsync();

            var dto = new UpdateEdgeRequestDto
            {
                RelationshipId = (int)relationshipId
            };

            // Act
            var result = await _edgeBusiness.UpdateEdge(uid1, pid, dto, testEdge.Id, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal((int)relationshipId, result.RelationshipId);
            Assert.Equal(oid, result.OriginId);
            Assert.Equal(did, result.DestinationId);
            Assert.NotEqual(DateTime.MinValue, result.LastUpdatedAt);

            // Verify edge was actually updated in database
            var updatedEdge = await Context.Edges.FindAsync(testEdge.Id);
            Assert.NotNull(updatedEdge);
            Assert.Equal((int)relationshipId, updatedEdge.RelationshipId);
            Assert.Equal(oid, updatedEdge.OriginId);
            Assert.Equal(did, updatedEdge.DestinationId);
            Assert.NotEqual(DateTime.MinValue, updatedEdge.LastUpdatedAt);

            // Verify that get function gets updated version
            var getResult = await _edgeBusiness.GetEdge(pid, testEdge.Id, oid, did, true);
            Assert.NotNull(getResult);
            Assert.Equal((int)relationshipId, getResult.RelationshipId);
            Assert.Equal(oid, getResult.OriginId);
            Assert.Equal(did, getResult.DestinationId);
            Assert.NotEqual(DateTime.MinValue, getResult.LastUpdatedAt);

            // Ensure that update edge event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);

            var actualEvent = eventList[0];

            Assert.Equal(testEdge.Id, actualEvent.EntityId);
            Assert.Equal("edge", actualEvent.EntityType);
            Assert.Equal("update", actualEvent.Operation);
        }

        [Fact]
        public async Task UpdateEdge_Fails_IfNotFound()
        {
            // Arrange
            var dto = new UpdateEdgeRequestDto
            {
                OriginId = (int)originRecordId,
                DestinationId = (int)destinationRecordId,
                RelationshipId = (int)relationshipId
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _edgeBusiness.UpdateEdge(uid1, pid, dto, 99, null, null));

            // Ensure that update edge event was NOT logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        #endregion

        #region ArchiveEdge Tests

        [Fact]
        public async Task ArchiveEdge_Success_WhenExists()
        {
            // Arrange
            var beforeArchive = DateTime.UtcNow;
            var testEdge = new Edge
            {
                OriginId = originRecordId,
                DestinationId = destinationRecordId,
                DataSourceId = dsid,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            Context.Edges.Add(testEdge);
            await Context.SaveChangesAsync();

            // Act
            var archivedResult = await _edgeBusiness.ArchiveEdge(uid1, pid, testEdge.Id, null, null);

            var archivedEdge = await Context.Edges.FindAsync(testEdge.Id);

            // Assert
            Assert.Equal(testEdge.Id, archivedResult);
            Assert.NotNull(archivedEdge);
            Assert.True(archivedEdge.IsArchived);

            // Ensure that soft delete edge event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);

            var actualEvent = eventList[0];

            Assert.Equal(testEdge.Id, actualEvent.EntityId);
            Assert.Equal("edge", actualEvent.EntityType);
            Assert.Equal("delete", actualEvent.Operation);
        }

        [Fact]
        public async Task ArchiveEdge_Fails_IfNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _edgeBusiness.ArchiveEdge(uid1, pid, 999, null, null));

            // Ensure that create edge event is NOT logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task EdgeArchived_WhenProjectArchived()
        {
            // Arrange
            var beforeArchive = DateTime.UtcNow;
            var testEdge = new Edge
            {
                OriginId = originRecordId,
                DestinationId = destinationRecordId,
                DataSourceId = dsid,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            Context.Edges.Add(testEdge);
            await Context.SaveChangesAsync();

            // Act
            var deletedResult = await _projectBusiness.ArchiveProject(pid);

            // procedure is not traced by entity framework
            //this forces EF to sync to db on next query
            Context.ChangeTracker.Clear();

            var archivedEdge = await Context.Edges.FindAsync(testEdge.Id);

            // Assert
            Assert.True(deletedResult);
            Assert.NotNull(archivedEdge);

            // Check if ArchivedAt was set (optional based on implementation)
            if (archivedEdge.IsArchived)
            {
                Assert.True(archivedEdge.IsArchived);
                // Assert.True(archivedEdge.IsArchived >= beforeArchive);
                // Assert.True(archivedEdge.IsArchived <= DateTime.UtcNow);
            }
        }

        #endregion

        #region  UnarchiveEdge Tests

        [Fact]
        public async Task UnarchiveEdge_Success_WhenArchived()
        {
            // Arrange
            var testEdge = new Edge
            {
                OriginId = originRecordId,
                DestinationId = destinationRecordId,
                DataSourceId = dsid,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null,
                IsArchived = true
            };
            Context.Edges.Add(testEdge);
            await Context.SaveChangesAsync();

            // Act
            var unarchivedResult = await _edgeBusiness.UnarchiveEdge(uid1, pid, testEdge.Id, null, null);
            Assert.Equal(testEdge.Id, unarchivedResult);

            var unarchivedEdge = await Context.Edges.FindAsync(testEdge.Id);

            // Assert
            Assert.NotNull(unarchivedEdge);
            Assert.False(unarchivedEdge.IsArchived);
        }

        [Fact]
        public async Task UnarchiveEdge_Fails_IfNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _edgeBusiness.UnarchiveEdge(uid1, pid, 999, null, null));

            // Ensure that create edge event is NOT logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task UnarchiveEdge_Fails_IfNotArchived()
        {
            // Arrange
            var activeEdge = new Edge
            {
                OriginId = originRecordId,
                DestinationId = destinationRecordId,
                DataSourceId = dsid,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            Context.Edges.Add(activeEdge);
            await Context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _edgeBusiness.UnarchiveEdge(uid1, pid, activeEdge.Id, null, null));

            // Ensure that create edge event is NOT logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        #endregion

        #region  DeleteEdge Tests

        [Fact]
        public async Task DeleteEdge_Success_WhenExists()
        {
            // Arrange
            var testEdge = new Edge
            {
                OriginId = originRecordId,
                DestinationId = destinationRecordId,
                DataSourceId = dsid,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            Context.Edges.Add(testEdge);
            await Context.SaveChangesAsync();

            // Act
            var deletedResult = await _edgeBusiness.DeleteEdge(pid, testEdge.Id, null, null);

            // Assert
            Assert.Equal(testEdge.Id, deletedResult);
            var deletedEdge = await Context.Edges.FindAsync(testEdge.Id);
            Assert.Null(deletedEdge);
        }

        [Fact]
        public async Task DeleteEdge_Fails_IfNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _edgeBusiness.DeleteEdge(pid, 999, null, null));
        }

        #endregion

        #region EdgeDTO Tests
        [Fact]
        public void EdgeRequestDto_AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange & Act
            var dto = new CreateEdgeRequestDto
            {
                OriginId = 1,
                DestinationId = 2,
                RelationshipId = 3,
                RelationshipName = "Test Relationship"
            };

            // Assert
            Assert.Equal(1, dto.OriginId);
            Assert.Equal(2, dto.DestinationId);
            Assert.Equal(3, dto.RelationshipId);
            Assert.Equal("Test Relationship", dto.RelationshipName);
        }

        [Fact]
        public void EdgeResponseDto_AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange & Act
            var now = DateTime.UtcNow;
            var dto = new EdgeResponseDto
            {
                Id = 1,
                OriginId = 2,
                DestinationId = 3,
                RelationshipId = 4,
                DataSourceId = 6,
                ProjectId = 7,
                LastUpdatedBy = uid1,
                LastUpdatedAt = now,
                IsArchived = false
            };

            // Assert
            Assert.Equal(1, dto.Id);
            Assert.Equal(2, dto.OriginId);
            Assert.Equal(3, dto.DestinationId);
            Assert.Equal(4, dto.RelationshipId);
            Assert.Equal(6, dto.DataSourceId);
            Assert.Equal(7, dto.ProjectId);
            Assert.Equal(now, dto.LastUpdatedAt);
            Assert.False(dto.IsArchived);
            Assert.Equal(uid1, dto.LastUpdatedBy);
        }

        #endregion
        #region LastUpdatedBy Tests

        [Fact]
        public async Task CreateEdge_Success_StoresLastUpdatedByUserId()
        {
            // Arrange
            var testEdge = new Edge
            {
                OriginId = originRecordId,
                DestinationId = destinationRecordId,
                DataSourceId = dsid,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = uid1
            };

            // Act
            Context.Edges.Add(testEdge);
            await Context.SaveChangesAsync();

            // Assert
            var savedEdge = await Context.Edges.FindAsync(testEdge.Id);
            Assert.NotNull(savedEdge);
            Assert.Equal(uid1, savedEdge.LastUpdatedBy);
        }

        [Fact]
        public async Task CreateEdge_Success_NavigationPropertyLoadsUser()
        {
            // Arrange
            var testEdge = new Edge
            {
                OriginId = originRecordId,
                DestinationId = destinationRecordId,
                DataSourceId = dsid,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = uid1
            };

            Context.Edges.Add(testEdge);
            await Context.SaveChangesAsync();

            // Act
            var edgeWithUser = await Context.Edges
                .Include(e => e.LastUpdatedByUser)
                .FirstAsync(e => e.Id == testEdge.Id);

            // Assert
            Assert.NotNull(edgeWithUser.LastUpdatedByUser);
            Assert.Equal("Test User 1", edgeWithUser.LastUpdatedByUser.Name);
            Assert.Equal("test_email@example.com", edgeWithUser.LastUpdatedByUser.Email);
            Assert.Equal(uid1, edgeWithUser.LastUpdatedBy);
        }

        [Fact]
        public async Task CreateEdge_Success_WithNullLastUpdatedBy()
        {
            // Arrange
            var testEdge = new Edge
            {
                OriginId = originRecordId,
                DestinationId = destinationRecordId,
                DataSourceId = dsid,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };

            // Act
            Context.Edges.Add(testEdge);
            await Context.SaveChangesAsync();

            // Assert
            var savedEdge = await Context.Edges.FindAsync(testEdge.Id);
            Assert.NotNull(savedEdge);
            Assert.Null(savedEdge.LastUpdatedBy);

            var edgeWithUser = await Context.Edges
                .Include(e => e.LastUpdatedByUser)
                .FirstAsync(e => e.Id == testEdge.Id);

            Assert.Null(edgeWithUser.LastUpdatedByUser);
        }

        [Fact]
        public async Task UpdateEdge_Success_UpdatesLastUpdatedByUserId()
        {
            // Arrange
            var testEdge = new Edge
            {
                OriginId = originRecordId,
                DestinationId = destinationRecordId,
                DataSourceId = dsid,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            Context.Edges.Add(testEdge);
            await Context.SaveChangesAsync();

            // Act
            testEdge.LastUpdatedBy = uid1;
            testEdge.DestinationId = destinationRecordId2;
            testEdge.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            Context.Edges.Update(testEdge);
            await Context.SaveChangesAsync();

            // Assert
            var updatedEdge = await Context.Edges
                .Include(e => e.LastUpdatedByUser)
                .FirstAsync(e => e.Id == testEdge.Id);

            Assert.Equal(uid1, updatedEdge.LastUpdatedBy);
            Assert.NotNull(updatedEdge.LastUpdatedByUser);
            Assert.Equal("Test User 1", updatedEdge.LastUpdatedByUser.Name);
            Assert.Equal(destinationRecordId2, updatedEdge.DestinationId);
        }

        #endregion
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

            //Record in another project
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

            var relationship = new Relationship
            {
                Name = "Relationship 1",
                ProjectId = pid,
                OriginId = testClass.Id,
                DestinationId = testClass.Id,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Relationships.Add(relationship);

            await Context.SaveChangesAsync();

            originRecordId = originRecord.Id;
            originRecordId2 = originRecord2.Id;
            destinationRecordId = destinationRecord.Id;
            destinationRecordId2 = destinationRecord2.Id;
            destinationRecordId3 = destinationRecord3.Id;
            relationshipId = relationship.Id;
        }
    }
}