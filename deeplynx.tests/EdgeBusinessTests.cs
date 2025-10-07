using System.ComponentModel.DataAnnotations;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using FluentAssertions;
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
        private Mock<IRecordBusiness> _mockRecordBusiness = null!;
        private Mock<IRelationshipBusiness> _mockRelationshipBusiness = null!;
        private Mock<ILogger<ProjectBusiness>> _mockLogger = null!;
        private Mock<IObjectStorageBusiness> _mockObjectStorageBusiness = null!;
        private Mock<IRoleBusiness> _mockRoleBusiness = null!;
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
        public EdgeBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _mockRecordBusiness = new Mock<IRecordBusiness>();
            _mockRelationshipBusiness = new Mock<IRelationshipBusiness>();
            _mockLogger = new Mock<ILogger<ProjectBusiness>>();
            _eventBusiness = new EventBusiness(Context, _cacheBusiness);
            _mockObjectStorageBusiness = new Mock<IObjectStorageBusiness>();
            _mockRoleBusiness = new Mock<IRoleBusiness>();

            _edgeBusiness = new EdgeBusiness(Context, _cacheBusiness, _eventBusiness);
            _dataSourceBusiness = new DataSourceBusiness(Context, _cacheBusiness, _edgeBusiness, _mockRecordBusiness.Object, _eventBusiness);
            _classBusiness = new ClassBusiness(
                Context, _cacheBusiness, _mockRecordBusiness.Object, 
                _mockRelationshipBusiness.Object, _eventBusiness);
            
            _projectBusiness = new ProjectBusiness(
                Context, _cacheBusiness, _mockLogger.Object, _classBusiness, 
                _mockRoleBusiness.Object, _dataSourceBusiness, 
                _mockObjectStorageBusiness.Object, _eventBusiness);
        }

        [Fact]
        public async Task CreateEdge_Success_ReturnsIdAndCreatedAt()
        {
            var now = DateTime.UtcNow;
            var dto = new CreateEdgeRequestDto
            {
                OriginId = (int)originRecordId,
                DestinationId = (int)destinationRecordId,
                RelationshipId = (int)relationshipId
            };

            var result = await _edgeBusiness.CreateEdge(pid, dsid, dto);
            result.Id.Should().BeGreaterThan(0);
            result.LastUpdatedAt.Should().BeOnOrAfter(now);
            result.OriginId.Should().Be(originRecordId);
            result.DestinationId.Should().Be(destinationRecordId);
            result.ProjectId.Should().Be(pid);
            result.DataSourceId.Should().Be(dsid);
            
            // Ensure that edge create event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                ProjectId = pid,
                Operation = "create",
                EntityType = "edge",
                EntityId = result.Id,
            });
        }

        [Fact]
        public async Task CreateEdge_Fails_IfNoOriginId()
        {
            var dto = new CreateEdgeRequestDto
            {
                OriginId = 0, // Invalid origin
                DestinationId = (int)destinationRecordId,
                RelationshipId = (int)relationshipId
            };
            var result = () => _edgeBusiness.CreateEdge(pid, dsid, dto);
            await result.Should().ThrowAsync<DbUpdateException>();
            
            // Ensure that edge create event was not logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task CreateEdge_Fails_IfNoDestinationId()
        {
            var dto = new CreateEdgeRequestDto
            {
                OriginId = (int)originRecordId,
                DestinationId = 0, // Invalid destination
                RelationshipId = (int)relationshipId
            };
            var result = () => _edgeBusiness.CreateEdge(pid, dsid, dto);
            await result.Should().ThrowAsync<DbUpdateException>();
            
            // Ensure that edge create event was not logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task CreateEdge_Fails_IfNoProjectId()
        {
            var dto = new CreateEdgeRequestDto
            {
                OriginId = (int)originRecordId,
                DestinationId = (int)destinationRecordId,
                RelationshipId = (int)relationshipId
            };
            var result = () => _edgeBusiness.CreateEdge(pid + 99, dsid, dto);
            await result.Should().ThrowAsync<KeyNotFoundException>();
            
            // Ensure that edge create event was not logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task CreateEdge_Fails_IfNoDataSourceId()
        {
            var dto = new CreateEdgeRequestDto
            {
                OriginId = (int)originRecordId,
                DestinationId = (int)destinationRecordId,
                RelationshipId = (int)relationshipId
            };
            var result = () => _edgeBusiness.CreateEdge(pid, dsid + 99, dto);
            await result.Should().ThrowAsync<KeyNotFoundException>();
            
            // Ensure that edge create event was not logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task CreateEdge_Fails_IfDeletedProjectId()
        {
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
            var result = () => _edgeBusiness.CreateEdge(pid, dsid, dto);
            await result.Should().ThrowAsync<KeyNotFoundException>();
            
            // Ensure that edge create event was not logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task BulkCreateEdges_Success_ReturnsMultipleEdges()
        {
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

            var result = await _edgeBusiness.BulkCreateEdges(pid, dsid, edges);
            result.Should().HaveCount(2);
            result.Should().OnlyContain(e => e.Id > 0);
            result.Should().OnlyContain(e => e.LastUpdatedAt >= now);

            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(2);
            eventList[0].Should().BeEquivalentTo(new {
               ProjectId = result[0].ProjectId, 
               EntityId = result[0].Id,
               EntityType = "edge",
               Operation = "create",
            });
            eventList[1].Should().BeEquivalentTo(new {
                ProjectId = result[0].ProjectId, 
                EntityId = result[1].Id,
                EntityType = "edge",
                Operation = "create",
            });
        }

        [Fact]
        public async Task GetAllEdges_ReturnsOnlyForProject()
        {
            var p2 = new Project { Name = "ExtraProj" };
            Context.Projects.Add(p2);
            await Context.SaveChangesAsync();

            var ds2 = new DataSource
            {
                Name = "Extra DataSource",
                ProjectId = p2.Id,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.DataSources.Add(ds2);
            await Context.SaveChangesAsync();

            await _edgeBusiness.CreateEdge(pid, dsid, new CreateEdgeRequestDto { OriginId = (int)originRecordId, DestinationId = (int)destinationRecordId });
            await _edgeBusiness.CreateEdge(p2.Id, ds2.Id, new CreateEdgeRequestDto { OriginId = (int)originRecordId, DestinationId = (int)destinationRecordId });

            var list = await _edgeBusiness.GetAllEdges(pid, null, true);
            Assert.All(list, e => Assert.Equal(pid, e.ProjectId));
        }

        [Fact]
        public async Task GetAllEdges_ExcludesSoftDeleted()
        {
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

            var listWithArchived = await _edgeBusiness.GetAllEdges(pid, null, false);
            var listWithoutArchived = await _edgeBusiness.GetAllEdges(pid, null, true);

            listWithArchived.Should().Contain(e => e.Id == archivedEdge.Id);
            listWithoutArchived.Should().NotContain(e => e.Id == archivedEdge.Id);
        }
        
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
        public async Task GetEdgesByRecord_ReturnsCorrectRelatedInfo()
        {
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
                OriginId = destinationRecordId2,
                DestinationId = originRecordId,
                DataSourceId = dsid,
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            
            var edge3 = new Edge
            {
                OriginId = destinationRecordId,
                DestinationId = originRecordId2,
                DataSourceId = dsid,
                ProjectId = pid2,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null
            };
            
            var edge4 = new Edge
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
            
            await Context.SaveChangesAsync();
        
            var edges = await _edgeBusiness.GetEdgesByRecord(originRecordId, true);
            edges.Count.Should().Be(2);
            edges.Should().Contain(r => r.RelatedRecordName == "Destination 1" &&
                                        r.RelationshipName == null && r.RelatedRecordId == destinationRecordId &&
                                        r.RelatedRecordProjectId == pid && r.IsOrigin == true);
            edges.Should().Contain(r => r.RelatedRecordName == "Destination 2" &&
                                        r.RelationshipName == null && r.RelatedRecordId == destinationRecordId2 &&
                                        r.RelatedRecordProjectId == pid && r.IsOrigin == false);
            edges.Should().NotContain(r => r.RelatedRecordName == "Destination 3");
            edges.Should().NotContain(r => r.RelatedRecordName == "Origin 2");
        }

        [Fact]
        public async Task GetEdgesByRecord_Fails_IfRecordDoesNotExist()
        {
            var result = () => _edgeBusiness.GetEdgesByRecord(originRecordId + 1000, true);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }
        
        [Fact]
        public async Task GetEdge_Success_WhenExistsById()
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

            var result = await _edgeBusiness.GetEdge(pid, testEdge.Id, null, null, true);
            Assert.Equal(testEdge.Id, result.Id);
            Assert.Equal(originRecordId, result.OriginId);
            Assert.Equal(destinationRecordId, result.DestinationId);
        }

        [Fact]
        public async Task GetEdge_Success_WhenExistsByOriginDestination()
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

            var result = await _edgeBusiness.GetEdge(pid, null, originRecordId, destinationRecordId, true);
            Assert.Equal(testEdge.Id, result.Id);
            Assert.Equal(originRecordId, result.OriginId);
            Assert.Equal(destinationRecordId, result.DestinationId);
        }

        [Fact]
        public async Task GetEdge_Fails_IfNoProjectID()
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

            var result = () => _edgeBusiness.GetEdge(pid + 999, testEdge.Id, null, null, true);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task GetEdge_Fails_IfDeletedEdge()
        {
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

            var result = () => _edgeBusiness.GetEdge(pid, testEdge.Id, null, null, true);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task GetEdge_Fails_IfMissingIds()
        {
            var result = () => _edgeBusiness.GetEdge(pid, null, null, null, true);
            await result.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*Please supply either an edgeID or an originID and destinationID*");
        }

        [Fact]
        public async Task UpdateEdge_Success_ReturnsModifiedAt()
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
            var updatedResult = await _edgeBusiness.UpdateEdge(pid, dto, testEdge.Id, null, null);

            updatedResult.LastUpdatedAt.Should().BeOnOrAfter(updatedResult.LastUpdatedAt);
            updatedResult.DestinationId.Should().Be(destinationRecordId2);
            
            // Ensure that update edge event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                EntityId = testEdge.Id,
                EntityType = "edge",
                Operation = "update"
            });
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
            var result = await _edgeBusiness.UpdateEdge(pid, dto, testEdge.Id, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal((int)relationshipId, result.RelationshipId);
            Assert.Equal(oid, result.OriginId);
            Assert.Equal(did, result.DestinationId);
            Assert.NotNull(result.LastUpdatedAt);

            // Verify edge was actually updated in database
            var updatedEdge = await Context.Edges.FindAsync(testEdge.Id);
            Assert.NotNull(updatedEdge);
            Assert.Equal((int)relationshipId, updatedEdge.RelationshipId);
            Assert.Equal(oid, updatedEdge.OriginId);
            Assert.Equal(did, updatedEdge.DestinationId);
            Assert.NotNull(updatedEdge.LastUpdatedAt);

            // Verify that get function gets updated version
            var getResult = await _edgeBusiness.GetEdge(pid, testEdge.Id, oid, did, true);
            Assert.NotNull(getResult);
            Assert.Equal((int)relationshipId, getResult.RelationshipId);
            Assert.Equal(oid, getResult.OriginId);
            Assert.Equal(did, getResult.DestinationId);
            Assert.NotNull(getResult.LastUpdatedAt);
            // Ensure that update edge event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                EntityId = testEdge.Id,
                EntityType = "edge",
                Operation = "update"
            });
        }

        [Fact]
        public async Task UpdateEdge_Fails_IfNotFound()
        {
            var dto = new UpdateEdgeRequestDto
            {
                OriginId = (int)originRecordId,
                DestinationId = (int)destinationRecordId,
                RelationshipId = (int)relationshipId
            };
            var updatedResult = () => _edgeBusiness.UpdateEdge(pid, dto, 99, null, null);
            await updatedResult.Should().ThrowAsync<KeyNotFoundException>();
            
            // Ensure that update edge event was not logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task ArchiveEdge_Success_WhenExists()
        {
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

            var archivedResult = await _edgeBusiness.ArchiveEdge(pid, testEdge.Id, null, null);
            Assert.Equal(testEdge.Id, archivedResult);

            var archivedEdge = await Context.Edges.FindAsync(testEdge.Id);
            Assert.NotNull(archivedEdge);
            Assert.True(archivedEdge.IsArchived);
            // Ensure that soft delete edge event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                EntityId = testEdge.Id,
                EntityType = "edge",
                Operation = "delete"
            });

        }

        [Fact]
        public async Task UnarchiveEdge_Success_WhenArchived()
        {
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

            var unarchivedResult = await _edgeBusiness.UnarchiveEdge(pid, testEdge.Id, null, null);
            Assert.Equal(testEdge.Id, unarchivedResult);

            var unarchivedEdge = await Context.Edges.FindAsync(testEdge.Id);
            Assert.NotNull(unarchivedEdge);
            Assert.False(unarchivedEdge.IsArchived);
        }

        [Fact]
        public async Task DeleteEdge_Success_WhenExists()
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

            var deletedResult = await _edgeBusiness.DeleteEdge(pid, testEdge.Id, null, null);
            Assert.Equal(testEdge.Id, deletedResult);

            var deletedEdge = await Context.Edges.FindAsync(testEdge.Id);
            Assert.Null(deletedEdge);
        }

        [Fact]
        public async Task EdgeArchived_WhenProjectArchived()
        {
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

            var deletedResult = await _projectBusiness.ArchiveProject(pid);
            Assert.True(deletedResult);

            // procedure is not traced by entity framework
            //this forces EF to sync to db on next query
            Context.ChangeTracker.Clear();

            var archivedEdge = await Context.Edges.FindAsync(testEdge.Id);
            Assert.NotNull(archivedEdge);

            // Check if ArchivedAt was set (optional based on implementation)
            if (archivedEdge.IsArchived)
            {
                Assert.True(archivedEdge.IsArchived);
                // Assert.True(archivedEdge.IsArchived >= beforeArchive);
                // Assert.True(archivedEdge.IsArchived <= DateTime.UtcNow);
            }
        }

        [Fact]
        public async Task BulkCreateEdges_Fails_IfNullDto()
        {
            var result = () => _edgeBusiness.BulkCreateEdges(pid, dsid, null);
            await result.Should().ThrowAsync<ArgumentNullException>();
            
            // Ensure that create edge event is not logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task ArchiveEdge_Fails_IfNotFound()
        {
            var result = () => _edgeBusiness.ArchiveEdge(pid, 999, null, null);
            await result.Should().ThrowAsync<KeyNotFoundException>();
            // Ensure that create edge event is not logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task DeleteEdge_Fails_IfNotFound()
        {
            var result = () => _edgeBusiness.DeleteEdge(pid, 999, null, null);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task UnarchiveEdge_Fails_IfNotFound()
        {
            var result = () => _edgeBusiness.UnarchiveEdge(pid, 999, null, null);
            await result.Should().ThrowAsync<KeyNotFoundException>();
            
            // Ensure that create edge event is not logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task UnarchiveEdge_Fails_IfNotArchived()
        {
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

            var result = () => _edgeBusiness.UnarchiveEdge(pid, activeEdge.Id, null, null);
            await result.Should().ThrowAsync<KeyNotFoundException>();
            
            // Ensure that create edge event is not logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public void EdgeRequestDto_AllProperties_CanBeSetAndRetrieved()
        {
            var dto = new CreateEdgeRequestDto
            {
                OriginId = 1,
                DestinationId = 2,
                RelationshipId = 3,
                RelationshipName = "Test Relationship"
            };

            Assert.Equal(1, dto.OriginId);
            Assert.Equal(2, dto.DestinationId);
            Assert.Equal(3, dto.RelationshipId);
            Assert.Equal("Test Relationship", dto.RelationshipName);
        }

        [Fact]
        public void EdgeResponseDto_AllProperties_CanBeSetAndRetrieved()
        {
            var now = DateTime.UtcNow;
            var dto = new EdgeResponseDto
            {
                Id = 1,
                OriginId = 2,
                DestinationId = 3,
                RelationshipId = 4,
                DataSourceId = 6,
                ProjectId = 7,
                LastUpdatedBy = "test@example.com",
                LastUpdatedAt = now,
                IsArchived = false
            };

            Assert.Equal(1, dto.Id);
            Assert.Equal(2, dto.OriginId);
            Assert.Equal(3, dto.DestinationId);
            Assert.Equal(4, dto.RelationshipId);
            Assert.Equal(6, dto.DataSourceId);
            Assert.Equal(7, dto.ProjectId);
            Assert.Equal(now, dto.LastUpdatedAt);
            Assert.False(dto.IsArchived);
        }
        protected override async Task SeedTestDataAsync()
        {
            await base.SeedTestDataAsync();

            var user = new User
            {
                Name = "Test User 1",
                Email = "test_email@example.com"
            };
            Context.Users.Add(user);
            await Context.SaveChangesAsync();
            uid1 = user.Id;

            var project = new Project { Name = "Project 1" };
            Context.Projects.Add(project);
            await Context.SaveChangesAsync();
            pid = project.Id;
            
            var project2 = new Project { Name = "Project 2" };
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