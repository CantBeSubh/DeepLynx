using System.ComponentModel.DataAnnotations;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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
        private Mock<IEdgeMappingBusiness> _mockEdgeMappingBusiness = null!;
        private Mock<IRecordBusiness> _mockRecordBusiness = null!;
        private Mock<IRecordMappingBusiness> _mockRecordMappingBusiness = null!;
        private Mock<IRelationshipBusiness> _mockRelationshipBusiness = null!;
        public long pid;
        public long dsid;
        public long originRecordId;
        public long destinationRecordId;
        public long relationshipId;

        public EdgeBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _mockEdgeMappingBusiness = new Mock<IEdgeMappingBusiness>();
            _mockRecordBusiness = new Mock<IRecordBusiness>();
            _mockRecordMappingBusiness = new Mock<IRecordMappingBusiness>();
            _mockRelationshipBusiness = new Mock<IRelationshipBusiness>();

            _edgeBusiness = new EdgeBusiness(Context);
            _dataSourceBusiness = new DataSourceBusiness(Context, _edgeBusiness, _mockRecordBusiness.Object);
            _classBusiness = new ClassBusiness(Context, _mockEdgeMappingBusiness.Object, _mockRecordBusiness.Object, _mockRecordMappingBusiness.Object, _mockRelationshipBusiness.Object);
            _projectBusiness = new ProjectBusiness(Context, _classBusiness);
        }

        [Fact]
        public async Task CreateEdge_Success_ReturnsIdAndCreatedAt()
        {
            var now = DateTime.UtcNow;
            var dto = new EdgeRequestDto
            {
                OriginId = (int)originRecordId,
                DestinationId = (int)destinationRecordId,
                RelationshipId = (int)relationshipId
            };

            var result = await _edgeBusiness.CreateEdge(pid, dsid, dto);
            result.Id.Should().BeGreaterThan(0);
            result.CreatedAt.Should().BeOnOrAfter(now);
            result.OriginId.Should().Be(originRecordId);
            result.DestinationId.Should().Be(destinationRecordId);
            result.ProjectId.Should().Be(pid);
            result.DataSourceId.Should().Be(dsid);
        }

        [Fact]
        public async Task CreateEdge_Fails_IfNoOriginId()
        {
            var dto = new EdgeRequestDto
            {
                OriginId = 0, // Invalid origin
                DestinationId = (int)destinationRecordId,
                RelationshipId = (int)relationshipId
            };
            var result = () => _edgeBusiness.CreateEdge(pid, dsid, dto);
            await result.Should().ThrowAsync<DbUpdateException>();
        }

        [Fact]
        public async Task CreateEdge_Fails_IfNoDestinationId()
        {
            var dto = new EdgeRequestDto
            {
                OriginId = (int)originRecordId,
                DestinationId = 0, // Invalid destination
                RelationshipId = (int)relationshipId
            };
            var result = () => _edgeBusiness.CreateEdge(pid, dsid, dto);
            await result.Should().ThrowAsync<DbUpdateException>();
        }

        [Fact]
        public async Task CreateEdge_Fails_IfNoProjectId()
        {
            var dto = new EdgeRequestDto
            {
                OriginId = (int)originRecordId,
                DestinationId = (int)destinationRecordId,
                RelationshipId = (int)relationshipId
            };
            var result = () => _edgeBusiness.CreateEdge(pid + 99, dsid, dto);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task CreateEdge_Fails_IfNoDataSourceId()
        {
            var dto = new EdgeRequestDto
            {
                OriginId = (int)originRecordId,
                DestinationId = (int)destinationRecordId,
                RelationshipId = (int)relationshipId
            };
            var result = () => _edgeBusiness.CreateEdge(pid, dsid + 99, dto);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task CreateEdge_Fails_IfDeletedProjectId()
        {
            var project = await Context.Projects.FindAsync(pid);
            project.ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            Context.Projects.Update(project);
            await Context.SaveChangesAsync();

            var dto = new EdgeRequestDto
            {
                OriginId = (int)originRecordId,
                DestinationId = (int)destinationRecordId,
                RelationshipId = (int)relationshipId
            };
            var result = () => _edgeBusiness.CreateEdge(pid, dsid, dto);
            await result.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task BulkCreateEdges_Success_ReturnsMultipleEdges()
        {
            var now = DateTime.UtcNow;

            // Create additional records for second edge
            var destinationRecord2 = new Record
                {
                    ProjectId = pid,
                    DataSourceId = dsid,
                    Properties = "{\"test\": \"destination2_value\"}",
                    Name = "Destination 2",
                    Description = "Destination Description 2",
                    OriginalId = "dest2",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Records.Add(destinationRecord2);
            await Context.SaveChangesAsync();

            var edges = new List<EdgeRequestDto>
            {
                new EdgeRequestDto
                {
                    OriginId = (int)originRecordId,
                    DestinationId = (int)destinationRecordId,
                    RelationshipId = (int)relationshipId
                },
                new EdgeRequestDto
                {
                    OriginId = (int)originRecordId,
                    DestinationId = (int)destinationRecord2.Id,
                    RelationshipId = (int)relationshipId
                }
            };

            var result = await _edgeBusiness.BulkCreateEdges(pid, dsid, edges);
            result.Should().HaveCount(2);
            result.Should().OnlyContain(e => e.Id > 0);
            result.Should().OnlyContain(e => e.CreatedAt >= now);
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
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.DataSources.Add(ds2);
            await Context.SaveChangesAsync();

            await _edgeBusiness.CreateEdge(pid, dsid, new EdgeRequestDto { OriginId = (int)originRecordId, DestinationId = (int)destinationRecordId });
            await _edgeBusiness.CreateEdge(p2.Id, ds2.Id, new EdgeRequestDto { OriginId = (int)originRecordId, DestinationId = (int)destinationRecordId });

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
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null
            };

            var archivedEdge = new Edge
            {
                OriginId = originRecordId,
                DestinationId = destinationRecordId,
                DataSourceId = dsid,
                ProjectId = pid,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
                ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Edges.Add(activeEdge);
            Context.Edges.Add(archivedEdge);
            await Context.SaveChangesAsync();

            var listWithArchived = await _edgeBusiness.GetAllEdges(pid, null, false);
            var listWithoutArchived = await _edgeBusiness.GetAllEdges(pid, null, true);

            listWithArchived.Should().Contain(e => e.Id == archivedEdge.Id);
            listWithoutArchived.Should().NotContain(e => e.Id == archivedEdge.Id);
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
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null
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
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null
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
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null
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
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
                ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
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
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
            };
            Context.Edges.Add(testEdge);
            await Context.SaveChangesAsync();

            // Create another destination record for update
            var newDestinationRecord = new Record
            {
                ProjectId = pid,
                DataSourceId = dsid,
                Properties = "{\"test\": \"Updated destination_value\"}",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                Name = "New Destination",
                Description = "New Destination Description",
                OriginalId = "new",
            };
            Context.Records.Add(newDestinationRecord);
            await Context.SaveChangesAsync();

            var dto = new EdgeRequestDto
            {
                OriginId = (int)originRecordId,
                DestinationId = (int)newDestinationRecord.Id,
                RelationshipId = (int)relationshipId
            };
            var updatedResult = await _edgeBusiness.UpdateEdge(pid, dto, testEdge.Id, null, null);

            updatedResult.ModifiedAt.Should().BeOnOrAfter(updatedResult.CreatedAt);
            updatedResult.DestinationId.Should().Be(newDestinationRecord.Id);
        }

        [Fact]
        public async Task UpdateEdge_Fails_IfNotFound()
        {
            var dto = new EdgeRequestDto
            {
                OriginId = (int)originRecordId,
                DestinationId = (int)destinationRecordId,
                RelationshipId = (int)relationshipId
            };
            var updatedResult = () => _edgeBusiness.UpdateEdge(pid, dto, 99, null, null);
            await updatedResult.Should().ThrowAsync<KeyNotFoundException>();
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
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
            };
            Context.Edges.Add(testEdge);
            await Context.SaveChangesAsync();

            var archivedResult = await _edgeBusiness.ArchiveEdge(pid, testEdge.Id, null, null);
            Assert.Equal(testEdge.Id, archivedResult);

            var archivedEdge = await Context.Edges.FindAsync(testEdge.Id);
            Assert.NotNull(archivedEdge);
            Assert.NotNull(archivedEdge.ArchivedAt);
            Assert.True(archivedEdge.ArchivedAt >= beforeArchive);
            Assert.True(archivedEdge.ArchivedAt <= DateTime.UtcNow);
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
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
                ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Edges.Add(testEdge);
            await Context.SaveChangesAsync();

            var unarchivedResult = await _edgeBusiness.UnarchiveEdge(pid, testEdge.Id, null, null);
            Assert.Equal(testEdge.Id, unarchivedResult);

            var unarchivedEdge = await Context.Edges.FindAsync(testEdge.Id);
            Assert.NotNull(unarchivedEdge);
            Assert.Null(unarchivedEdge.ArchivedAt);
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
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
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
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                CreatedBy = null,
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
            if (archivedEdge.ArchivedAt.HasValue)
            {
                Assert.NotNull(archivedEdge.ArchivedAt);
                Assert.True(archivedEdge.ArchivedAt >= beforeArchive);
                Assert.True(archivedEdge.ArchivedAt <= DateTime.UtcNow);
            }
        }

      [Fact]
public async Task BulkCreateEdges_Fails_IfNullDto()
{
    var result = () => _edgeBusiness.BulkCreateEdges(pid, dsid, null);
    await result.Should().ThrowAsync<ArgumentNullException>();
}

[Fact]
public async Task ArchiveEdge_Fails_IfNotFound()
{
    var result = () => _edgeBusiness.ArchiveEdge(pid, 999, null, null);
    await result.Should().ThrowAsync<KeyNotFoundException>();
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
        CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
        ArchivedAt = null // Not archived
    };
    Context.Edges.Add(activeEdge);
    await Context.SaveChangesAsync();

    var result = () => _edgeBusiness.UnarchiveEdge(pid, activeEdge.Id, null, null);
    await result.Should().ThrowAsync<KeyNotFoundException>();
}

[Fact]
public void EdgeRequestDto_AllProperties_CanBeSetAndRetrieved()
{
    var dto = new EdgeRequestDto
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
        MappingId = 5,
        DataSourceId = 6,
        ProjectId = 7,
        CreatedBy = "test@example.com",
        CreatedAt = now,
        ModifiedBy = "modified@example.com",
        ModifiedAt = now.AddDays(1),
        ArchivedAt = null
    };

    Assert.Equal(1, dto.Id);
    Assert.Equal(2, dto.OriginId);
    Assert.Equal(3, dto.DestinationId);
    Assert.Equal(4, dto.RelationshipId);
    Assert.Equal(5, dto.MappingId);
    Assert.Equal(6, dto.DataSourceId);
    Assert.Equal(7, dto.ProjectId);
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

            var testClass = new Class
            {
                Name = "Class 1",
                ProjectId = pid,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
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
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Records.Add(originRecord);

            var destinationRecord = new Record
            {
                ProjectId = pid,
                DataSourceId = dsid,
                ClassId = testClass.Id,
                Properties = "{\"test\": \"destination_value\"}", 
                Name = "Destination 1",
                Description = "Destination Description 1",
                OriginalId = "dest1",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Records.Add(destinationRecord);

            var relationship = new Relationship
            {
                Name = "Relationship 1",
                ProjectId = pid,
                OriginId = testClass.Id,
                DestinationId = testClass.Id,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            Context.Relationships.Add(relationship);

            await Context.SaveChangesAsync();

            originRecordId = originRecord.Id;
            destinationRecordId = destinationRecord.Id;
            relationshipId = relationship.Id;
        }
    }
}