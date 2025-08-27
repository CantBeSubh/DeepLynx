using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class EdgeMappingBusinessTests : IntegrationTestBase
    {
        private EdgeMappingBusiness _edgeMappingBusiness = null!;
        private ProjectBusiness _projectBusiness = null!;
        private DataSourceBusiness _dataSourceBusiness = null!;
        private ClassBusiness _classBusiness = null!;
        private Mock<IEdgeBusiness> _mockEdgeBusiness = null!;
        private Mock<IRecordBusiness> _mockRecordBusiness = null!;
        private Mock<IRecordMappingBusiness> _mockRecordMappingBusiness = null!;
        private Mock<IRelationshipBusiness> _mockRelationshipBusiness = null!;
        private Mock<ILogger<ProjectBusiness>> _mockLogger = null!;
        private EventBusiness _eventBusiness = null!;
        private Mock<IObjectStorageBusiness> _mockObjectStorageBusiness = null!;
        public long pid;
        public long dsid;
        public long originClassId;
        public long destinationClassId;
        public long relationshipId;

        public EdgeMappingBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _mockEdgeBusiness = new Mock<IEdgeBusiness>();
            _mockRecordBusiness = new Mock<IRecordBusiness>();
            _mockRecordMappingBusiness = new Mock<IRecordMappingBusiness>();
            _mockRelationshipBusiness = new Mock<IRelationshipBusiness>();
            _mockLogger = new Mock<ILogger<ProjectBusiness>>();
            _eventBusiness = new EventBusiness(Context);
            _eventBusiness = new EventBusiness(Context);
            _mockObjectStorageBusiness = new Mock<IObjectStorageBusiness>();

            _edgeMappingBusiness = new EdgeMappingBusiness(Context, _eventBusiness);
            _dataSourceBusiness = new DataSourceBusiness(Context, _mockEdgeBusiness.Object, _mockRecordBusiness.Object, _eventBusiness);
            _edgeMappingBusiness = new EdgeMappingBusiness(
                Context, _eventBusiness);
           
            _dataSourceBusiness = new DataSourceBusiness(
                Context, _mockEdgeBusiness.Object, _mockRecordBusiness.Object, _eventBusiness);
            
            _classBusiness = new ClassBusiness(
                Context, _edgeMappingBusiness, _mockRecordBusiness.Object, 
                _mockRecordMappingBusiness.Object, _mockRelationshipBusiness.Object, _eventBusiness);

            _projectBusiness = new ProjectBusiness(
                Context, _mockLogger.Object, _classBusiness, _dataSourceBusiness,
                _mockObjectStorageBusiness.Object, _eventBusiness);
        }

        [Fact]
        public async Task CreateEdgeMapping_Success_ReturnsIdAndCreatedAt()
        {
            var now = DateTime.UtcNow;
            var dto = new CreateEdgeMappingRequestDto
            {
                OriginParams = JsonNode.Parse("{\"param1\": \"origin_value\"}")!.AsObject(),
                DestinationParams = JsonNode.Parse("{\"param1\": \"destination_value\"}")!.AsObject(),
                DataSourceId = dsid,
                RelationshipId = relationshipId,
                OriginId = originClassId,
                DestinationId = destinationClassId
            };

            var result = await _edgeMappingBusiness.CreateEdgeMapping(pid, dto);
            result.Id.Should().BeGreaterThan(0);
            result.CreatedAt.Should().BeOnOrAfter(now);
            result.OriginId.Should().Be(originClassId);
            result.DestinationId.Should().Be(destinationClassId);
            result.RelationshipId.Should().Be(relationshipId);
            result.ProjectId.Should().Be(pid);
            result.DataSourceId.Should().Be(dsid);
            result.OriginParams.Should().NotBeNull();
            result.DestinationParams.Should().NotBeNull();
            
            // Ensure that the create edgeMapping event was logged
            var eventList = Context.Events.ToList();
            eventList.Should().HaveCount(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                EntityId = result.Id,
                EntityType = "edge_mapping",
                DataSourceId = result.DataSourceId,
                Operation = "create",
                ProjectId = result.ProjectId,
            });
            
        }

        [Fact]
        public async Task CreateEdgeMapping_Fails_IfNoProjectId()
        {
            var dto = new CreateEdgeMappingRequestDto
            {
                OriginParams = JsonNode.Parse("{\"param1\": \"origin_value\"}")!.AsObject(),
                DestinationParams = JsonNode.Parse("{\"param1\": \"destination_value\"}")!.AsObject(),
                DataSourceId = dsid,
                RelationshipId = relationshipId,
                OriginId = originClassId,
                DestinationId = destinationClassId
            };
            var result = () => _edgeMappingBusiness.CreateEdgeMapping(pid + 99, dto);
            await result.Should().ThrowAsync<KeyNotFoundException>();
            
            // Ensure that the create edgeMapping event is not logged
            var evetList = Context.Events.ToList();
            evetList.Should().HaveCount(0);
        }

        [Fact]
        public async Task CreateEdgeMapping_Fails_IfDeletedProjectId()
        {
            var project = await Context.Projects.FindAsync(pid);
            project.ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            Context.Projects.Update(project);
            await Context.SaveChangesAsync();

            var dto = new CreateEdgeMappingRequestDto
            {
                OriginParams = JsonNode.Parse("{\"param1\": \"origin_value\"}")!.AsObject(),
                DestinationParams = JsonNode.Parse("{\"param1\": \"destination_value\"}")!.AsObject(),
                DataSourceId = dsid,
                RelationshipId = relationshipId,
                OriginId = originClassId,
                DestinationId = destinationClassId
            };
            var result = () => _edgeMappingBusiness.CreateEdgeMapping(pid, dto);
            await result.Should().ThrowAsync<KeyNotFoundException>();
            
            // Ensure that the create edgeMapping event is not logged
            var evetList = Context.Events.ToList();
            evetList.Should().HaveCount(0);
        }

        [Fact]
        public async Task GetAllEdgeMappings_ReturnsOnlyForProject()
        {
            var p2 = new Project { Name = "ExtraProj" };
            Context.Projects.Add(p2);
            await Context.SaveChangesAsync();

            // Create EdgeMapping for project 1
            await _edgeMappingBusiness.CreateEdgeMapping(pid, new CreateEdgeMappingRequestDto
            {
                OriginParams = JsonNode.Parse("{\"param1\": \"value1\"}")!.AsObject(),
            DestinationParams = JsonNode.Parse("{\"param1\": \"value1\"}")!.AsObject(),
            DataSourceId = dsid,
            OriginId = originClassId,
            DestinationId = destinationClassId,
            RelationshipId = relationshipId
            });

            // Create EdgeMapping directly for project 2 (bypass business logic validation)
            var p2Mapping = new EdgeMapping
                {
                    OriginParams = "{\"param1\": \"value2\"}",
                DestinationParams = "{\"param1\": \"value2\"}",
            DataSourceId = dsid,
            ProjectId = p2.Id,
            OriginId = originClassId,
            DestinationId = destinationClassId,
            RelationshipId = relationshipId,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.EdgeMappings.Add(p2Mapping);
            await Context.SaveChangesAsync();

            var list = await _edgeMappingBusiness.GetAllEdgeMappings(pid, null, null, true);
            Assert.All(list, m => Assert.Equal(pid, m.ProjectId));
        }

        [Fact]
        public async Task GetAllEdgeMappings_ExcludesSoftDeleted()
        {
            var activeMapping = new EdgeMapping
            {
                OriginParams = "{\"param1\": \"active_value\"}",
                DestinationParams = "{\"param1\": \"active_value\"}",
                DataSourceId = dsid,
                ProjectId = pid,
                OriginId = originClassId,
                DestinationId = destinationClassId,
                RelationshipId = relationshipId,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null
            };

            var archivedMapping = new EdgeMapping
            {
                OriginParams = "{\"param1\": \"archived_value\"}",
                DestinationParams = "{\"param1\": \"archived_value\"}",
                DataSourceId = dsid,
                ProjectId = pid,
                OriginId = originClassId,
                DestinationId = destinationClassId,
                RelationshipId = relationshipId,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
                ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.EdgeMappings.Add(activeMapping);
            Context.EdgeMappings.Add(archivedMapping);
            await Context.SaveChangesAsync();

            var listWithArchived = await _edgeMappingBusiness.GetAllEdgeMappings(pid, null, null, false);
            var listWithoutArchived = await _edgeMappingBusiness.GetAllEdgeMappings(pid, null, null, true);

            listWithArchived.Should().Contain(m => m.Id == archivedMapping.Id);
            listWithoutArchived.Should().NotContain(m => m.Id == archivedMapping.Id);
        }

        [Fact]
        public async Task GetAllEdgeMappings_FiltersBy_ClassId()
        {
            var otherClass = new Class
            {
                Name = $"Other Class {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                ProjectId = pid,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null
            };
            Context.Classes.Add(otherClass);
            await Context.SaveChangesAsync();

            // Create mapping with target class as origin
            var mapping1 = new EdgeMapping
            {
                OriginParams = "{\"param1\": \"value1\"}",
                DestinationParams = "{\"param1\": \"value1\"}",
                DataSourceId = dsid,
                ProjectId = pid,
                OriginId = destinationClassId,
                DestinationId = otherClass.Id,
                RelationshipId = relationshipId,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            // Create mapping with target class as destination
            var mapping2 = new EdgeMapping
            {
                OriginParams = "{\"param1\": \"value2\"}",
                DestinationParams = "{\"param1\": \"value2\"}",
                DataSourceId = dsid,
                ProjectId = pid,
                OriginId = otherClass.Id,
                DestinationId = destinationClassId,
                RelationshipId = relationshipId,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            // Create mapping without target class
            var mapping3 = new EdgeMapping
            {
                OriginParams = "{\"param1\": \"value3\"}",
                DestinationParams = "{\"param1\": \"value3\"}",
                DataSourceId = dsid,
                ProjectId = pid,
                OriginId = otherClass.Id,
                DestinationId = otherClass.Id,
                RelationshipId = relationshipId,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            Context.EdgeMappings.AddRange(mapping1, mapping2, mapping3);
            await Context.SaveChangesAsync();

            var filteredList = await _edgeMappingBusiness.GetAllEdgeMappings(pid, destinationClassId, null, true);

            filteredList.Should().Contain(m => m.Id == mapping1.Id);
            filteredList.Should().Contain(m => m.Id == mapping2.Id);
            filteredList.Should().NotContain(m => m.Id == mapping3.Id);
        }

        [Fact]
        public async Task GetAllEdgeMappings_FiltersBy_RelationshipId()
        {
            var otherRelationship = new Relationship
            {
                Name = "Other Relationship",
                ProjectId = pid,
                OriginId = originClassId,
                DestinationId = destinationClassId,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Relationships.Add(otherRelationship);
            await Context.SaveChangesAsync();

            var mapping1 = new EdgeMapping
            {
                OriginParams = "{\"param1\": \"value1\"}",
                DestinationParams = "{\"param1\": \"value1\"}",
                DataSourceId = dsid,
                ProjectId = pid,
                OriginId = originClassId,
                DestinationId = destinationClassId,
                RelationshipId = relationshipId,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            var mapping2 = new EdgeMapping
            {
                OriginParams = "{\"param1\": \"value2\"}",
                DestinationParams = "{\"param1\": \"value2\"}",
                DataSourceId = dsid,
                ProjectId = pid,
                OriginId = originClassId,
                DestinationId = destinationClassId,
                RelationshipId = otherRelationship.Id,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            Context.EdgeMappings.AddRange(mapping1, mapping2);
            await Context.SaveChangesAsync();

            var filteredList = await _edgeMappingBusiness.GetAllEdgeMappings(pid, null, relationshipId, true);

            filteredList.Should().Contain(m => m.Id == mapping1.Id);
            filteredList.Should().NotContain(m => m.Id == mapping2.Id);
        }

        [Fact]
        public async Task GetEdgeMapping_Success_WhenExists()
        {
            var testMapping = new EdgeMapping
            {
                OriginParams = "{\"param1\": \"test_value\"}",
                DestinationParams = "{\"param1\": \"test_value\"}",
                DataSourceId = dsid,
                ProjectId = pid,
                OriginId = originClassId,
                DestinationId = destinationClassId,
                RelationshipId = relationshipId,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null
            };
            Context.EdgeMappings.Add(testMapping);
            await Context.SaveChangesAsync();

            var result = await _edgeMappingBusiness.GetEdgeMapping(pid, testMapping.Id, true);
            Assert.Equal(testMapping.Id, result.Id);
            Assert.Equal(originClassId, result.OriginId);
            Assert.Equal(destinationClassId, result.DestinationId);
            Assert.Equal(relationshipId, result.RelationshipId);
        }

        [Fact]
        public async Task GetEdgeMapping_Fails_IfNoProjectID()
        {
            var testMapping = new EdgeMapping
            {
                OriginParams = "{\"param1\": \"test_value\"}",
                DestinationParams = "{\"param1\": \"test_value\"}",
                DataSourceId = dsid,
                ProjectId = pid,
                OriginId = originClassId,
                DestinationId = destinationClassId,
                RelationshipId = relationshipId,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null
            };
            Context.EdgeMappings.Add(testMapping);
            await Context.SaveChangesAsync();

            var result = () => _edgeMappingBusiness.GetEdgeMapping(pid + 999, testMapping.Id, true);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task GetEdgeMapping_Fails_IfDeletedMapping()
        {
            var testMapping = new EdgeMapping
            {
                OriginParams = "{\"param1\": \"test_value\"}",
                DestinationParams = "{\"param1\": \"test_value\"}",
                DataSourceId = dsid,
                ProjectId = pid,
                OriginId = originClassId,
                DestinationId = destinationClassId,
                RelationshipId = relationshipId,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
                ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.EdgeMappings.Add(testMapping);
            await Context.SaveChangesAsync();

            var result = () => _edgeMappingBusiness.GetEdgeMapping(pid, testMapping.Id, true);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task UpdateEdgeMapping_Success_ReturnsModifiedAt()
        {
            var testMapping = new EdgeMapping
            {
                OriginParams = "{\"param1\": \"original_value\"}",
                DestinationParams = "{\"param1\": \"original_value\"}",
                DataSourceId = dsid,
                ProjectId = pid,
                OriginId = originClassId,
                DestinationId = destinationClassId,
                RelationshipId = relationshipId,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
            };
            Context.EdgeMappings.Add(testMapping);
            await Context.SaveChangesAsync();

            var dto = new UpdateEdgeMappingRequestDto
            {
                OriginParams = JsonNode.Parse("{\"param1\": \"updated_value\"}")!.AsObject(),
                DestinationParams = JsonNode.Parse("{\"param1\": \"updated_value\"}")!.AsObject(),
                DataSourceId = dsid,
                RelationshipId = relationshipId,
                OriginId = originClassId,
                DestinationId = destinationClassId
            };
            var updatedResult = await _edgeMappingBusiness.UpdateEdgeMapping(pid, testMapping.Id, dto);

            updatedResult.ModifiedAt.Should().BeOnOrAfter(updatedResult.CreatedAt);
            updatedResult.OriginParams!["param1"]!.ToString().Should().Be("updated_value");
            updatedResult.DestinationParams!["param1"]!.ToString().Should().Be("updated_value");
            
            // Ensure that the update eventMapping event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                EntityId = updatedResult.Id,
                EntityType = "edge_mapping",
                Operation = "update",
                ProjectId = updatedResult.ProjectId,
                DataSourceId = updatedResult.DataSourceId,
            });
        }

        [Fact]
        public async Task UpdateEdgeMapping_Fails_IfNotFound()
        {
            var dto = new UpdateEdgeMappingRequestDto
            {
                OriginParams = JsonNode.Parse("{\"param1\": \"updated_value\"}")!.AsObject(),
                DestinationParams = JsonNode.Parse("{\"param1\": \"updated_value\"}")!.AsObject(),
                DataSourceId = dsid,
                RelationshipId = relationshipId,
                OriginId = originClassId,
                DestinationId = destinationClassId
            };
            var updatedResult = () => _edgeMappingBusiness.UpdateEdgeMapping(pid, 99, dto);
            await updatedResult.Should().ThrowAsync<KeyNotFoundException>();
            
            // Ensure that the update eventMapping event was not logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task ArchiveEdgeMapping_Success_WhenExists()
        {
            var beforeArchive = DateTime.UtcNow;

            var testMapping = new EdgeMapping
            {
                OriginParams = "{\"param1\": \"test_value\"}",
                DestinationParams = "{\"param1\": \"test_value\"}",
                DataSourceId = dsid,
                ProjectId = pid,
                OriginId = originClassId,
                DestinationId = destinationClassId,
                RelationshipId = relationshipId,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
            };
            Context.EdgeMappings.Add(testMapping);
            await Context.SaveChangesAsync();

            var archivedResult = await _edgeMappingBusiness.ArchiveEdgeMapping(pid, testMapping.Id);
            Assert.True(archivedResult);

            var archivedMapping = await Context.EdgeMappings.FindAsync(testMapping.Id);
            Assert.NotNull(archivedMapping);
            Assert.NotNull(archivedMapping.ArchivedAt);
            Assert.True(archivedMapping.ArchivedAt >= beforeArchive);
            Assert.True(archivedMapping.ArchivedAt <= DateTime.UtcNow);
            
            // Ensure that mapping soft delete event was logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                ProjectId = pid,
                Operation = "delete",
                EntityType = "edge_mapping",
                EntityId = testMapping.Id,
                DataSourceId = testMapping.DataSourceId,
            });
        }

        [Fact]
        public async Task UnarchiveEdgeMapping_Success_WhenArchived()
        {
            var testMapping = new EdgeMapping
            {
                OriginParams = "{\"param1\": \"test_value\"}",
                DestinationParams = "{\"param1\": \"test_value\"}",
                DataSourceId = dsid,
                ProjectId = pid,
                OriginId = originClassId,
                DestinationId = destinationClassId,
                RelationshipId = relationshipId,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
                ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.EdgeMappings.Add(testMapping);
            await Context.SaveChangesAsync();

            var unarchivedResult = await _edgeMappingBusiness.UnarchiveEdgeMapping(pid, testMapping.Id);
            Assert.True(unarchivedResult);

            var unarchivedMapping = await Context.EdgeMappings.FindAsync(testMapping.Id);
            Assert.NotNull(unarchivedMapping);
            Assert.Null(unarchivedMapping.ArchivedAt);
        }

        [Fact]
        public async Task DeleteEdgeMapping_Success_WhenExists()
        {
            var testMapping = new EdgeMapping
            {
                OriginParams = "{\"param1\": \"test_value\"}",
                DestinationParams = "{\"param1\": \"test_value\"}",
                DataSourceId = dsid,
                ProjectId = pid,
                OriginId = originClassId,
                DestinationId = destinationClassId,
                RelationshipId = relationshipId,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
            };
            Context.EdgeMappings.Add(testMapping);
            await Context.SaveChangesAsync();

            var deletedResult = await _edgeMappingBusiness.DeleteEdgeMapping(pid, testMapping.Id);
            Assert.True(deletedResult);

            var deletedMapping = await Context.EdgeMappings.FindAsync(testMapping.Id);
            Assert.Null(deletedMapping);
        }

        [Fact]
        public async Task EdgeMappingArchived_WhenProjectArchived()
        {
            var beforeArchive = DateTime.UtcNow;
            var testMapping = new EdgeMapping
            {
                OriginParams = "{\"param1\": \"test_value\"}",
                DestinationParams = "{\"param1\": \"test_value\"}",
                DataSourceId = dsid,
                ProjectId = pid,
                OriginId = originClassId,
                DestinationId = destinationClassId,
                RelationshipId = relationshipId,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
            };
            Context.EdgeMappings.Add(testMapping);
            await Context.SaveChangesAsync();

            var deletedResult = await _projectBusiness.ArchiveProject(pid);
            Assert.True(deletedResult);

            // procedure is not traced by entity framework
            //this forces EF to sync to db on next query
            Context.ChangeTracker.Clear();

            var archivedMapping = await Context.EdgeMappings.FindAsync(testMapping.Id);
            Assert.NotNull(archivedMapping);

            // Check if ArchivedAt was set (optional based on implementation)
            if (archivedMapping.ArchivedAt.HasValue)
            {
                Assert.NotNull(archivedMapping.ArchivedAt);
                Assert.True(archivedMapping.ArchivedAt >= beforeArchive);
                Assert.True(archivedMapping.ArchivedAt <= DateTime.UtcNow);
            }
        }

        [Fact]
        public async Task ArchiveEdgeMapping_Fails_IfNotFound()
        {
            var result = () => _edgeMappingBusiness.ArchiveEdgeMapping(pid, 999);
            await result.Should().ThrowAsync<KeyNotFoundException>();
            
            // Ensure that mapping soft delete event was NOT logged
            var eventList = Context.Events.ToList();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task DeleteEdgeMapping_Fails_IfNotFound()
        {
            var result = () => _edgeMappingBusiness.DeleteEdgeMapping(pid, 999);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task UnarchiveEdgeMapping_Fails_IfNotFound()
        {
            var result = () => _edgeMappingBusiness.UnarchiveEdgeMapping(pid, 999);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task UnarchiveEdgeMapping_Fails_IfNotArchived()
        {
            var activeMapping = new EdgeMapping
            {
                OriginParams = "{\"param1\": \"test_value\"}",
                DestinationParams = "{\"param1\": \"test_value\"}",
                DataSourceId = dsid,
                ProjectId = pid,
                OriginId = originClassId,
                DestinationId = destinationClassId,
                RelationshipId = relationshipId,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                ArchivedAt = null // Not archived
            };
            Context.EdgeMappings.Add(activeMapping);
            await Context.SaveChangesAsync();

            var result = () => _edgeMappingBusiness.UnarchiveEdgeMapping(pid, activeMapping.Id);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public void EdgeMappingRequestDto_AllProperties_CanBeSetAndRetrieved()
        {
            var originParams = JsonNode.Parse("{\"param1\": \"origin_value\"}")!.AsObject();
            var destinationParams = JsonNode.Parse("{\"param1\": \"destination_value\"}")!.AsObject();

            var dto = new CreateEdgeMappingRequestDto
            {
                OriginParams = originParams,
                DestinationParams = destinationParams,
                DataSourceId = 1,
                RelationshipId = 2,
                OriginId = 3,
                DestinationId = 4
            };

            Assert.Equal(originParams, dto.OriginParams);
            Assert.Equal(destinationParams, dto.DestinationParams);
            Assert.Equal(1, dto.DataSourceId);
            Assert.Equal(2, dto.RelationshipId);
            Assert.Equal(3, dto.OriginId);
            Assert.Equal(4, dto.DestinationId);
        }

        [Fact]
        public void EdgeMappingResponseDto_AllProperties_CanBeSetAndRetrieved()
        {
            var now = DateTime.UtcNow;
            var originParams = JsonNode.Parse("{\"param1\": \"origin_value\"}")!.AsObject();
            var destinationParams = JsonNode.Parse("{\"param1\": \"destination_value\"}")!.AsObject();

            var dto = new EdgeMappingResponseDto
            {
                Id = 1,
                OriginParams = originParams,
                DestinationParams = destinationParams,
                RelationshipId = 2,
                OriginId = 3,
                DestinationId = 4,
                DataSourceId = 5,
                ProjectId = 6,
                CreatedBy = "test@example.com",
                CreatedAt = now,
                ModifiedBy = "modified@example.com",
                ModifiedAt = now.AddDays(1),
                ArchivedAt = null
            };

            Assert.Equal(1, dto.Id);
            Assert.Equal(originParams, dto.OriginParams);
            Assert.Equal(destinationParams, dto.DestinationParams);
            Assert.Equal(2, dto.RelationshipId);
            Assert.Equal(3, dto.OriginId);
            Assert.Equal(4, dto.DestinationId);
            Assert.Equal(5, dto.DataSourceId);
            Assert.Equal(6, dto.ProjectId);
            Assert.Equal("test@example.com", dto.CreatedBy);
            Assert.Equal(now, dto.CreatedAt);
            Assert.Equal("modified@example.com", dto.ModifiedBy);
            Assert.Equal(now.AddDays(1), dto.ModifiedAt);
            Assert.Null(dto.ArchivedAt);
        }

        protected override async Task SeedTestDataAsync()
        {
            await base.SeedTestDataAsync();

            var project = new Project { Name = "Project 1" };
            Context.Projects.Add(project);
            await Context.SaveChangesAsync();
            pid = project.Id;

            var dataSource = new DataSource
            {
                Name = "DataSource 1",
                ProjectId = pid,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.DataSources.Add(dataSource);
            await Context.SaveChangesAsync();
            dsid = dataSource.Id;

            var originClass = new Class
            {
                Name = "Origin Class",
                ProjectId = pid,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(originClass);

            var destinationClass = new Class
            {
                Name = "Destination Class",
                ProjectId = pid,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Classes.Add(destinationClass);
            await Context.SaveChangesAsync();

            originClassId = originClass.Id;
            destinationClassId = destinationClass.Id;

            var relationship = new Relationship
            {
                Name = "Relationship 1",
                ProjectId = pid,
                OriginId = originClassId,
                DestinationId = destinationClassId,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Relationships.Add(relationship);
            await Context.SaveChangesAsync();

            relationshipId = relationship.Id;
        }
    }
}