using System.Text.Json;
using System.Text.Json.Nodes;
using System.ComponentModel.DataAnnotations;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.Extensions.Logging;

namespace deeplynx.tests
{
    [Collection("Test Suite Collection")]
    public class MetadataBusinessTests : IntegrationTestBase
    {
        private MetadataBusiness _metadataBusiness = null!;
        private ClassBusiness _classBusiness = null!;
        private RelationshipBusiness _relationshipBusiness = null!;
        private TagBusiness _tagBusiness = null!;
        private RecordBusiness _recordBusiness = null!;
        private EdgeBusiness _edgeBusiness = null!;
        private EventBusiness _eventBusiness = null!;
     
        private Mock<IRecordBusiness> _mockRecordBusiness = null!;
        private Mock<IRelationshipBusiness> _mockRelationshipBusiness = null!;
        private Mock<IEdgeBusiness> _mockEdgeBusiness = null!;
        
        public long pid;
        public long did;
        public long originClassId; 
        public long destClassId;  

        public MetadataBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            
            _mockRecordBusiness = new Mock<IRecordBusiness>();
            _mockRelationshipBusiness = new Mock<IRelationshipBusiness>();
            _mockEdgeBusiness = new Mock<IEdgeBusiness>();
            
            _eventBusiness = new EventBusiness(Context, _cacheBusiness);
            
            _classBusiness = new ClassBusiness(
                Context, _cacheBusiness, _mockRecordBusiness.Object, 
                _mockRelationshipBusiness.Object, _eventBusiness);
                
            _relationshipBusiness = new RelationshipBusiness(
                Context, _cacheBusiness, _mockEdgeBusiness.Object, _eventBusiness);
            
            _tagBusiness = new TagBusiness(Context, _cacheBusiness, _eventBusiness);
            _recordBusiness = new RecordBusiness(Context, _cacheBusiness, _eventBusiness);
            _edgeBusiness = new EdgeBusiness(Context, _cacheBusiness, _eventBusiness);
            
            _metadataBusiness = new MetadataBusiness(
                Context,
                _cacheBusiness,
                _classBusiness,
                _relationshipBusiness,
                _tagBusiness,
                _recordBusiness,
                _edgeBusiness
            );
        }

        [Fact]
        public async Task CreateMetadata_Success_ReturnsIdAndCreatedAt()
        {
            var now = DateTime.UtcNow;
            
            var classDtos = new List<CreateClassRequestDto>
            {
                new CreateClassRequestDto
                {
                    Name = "Test Metadata Class",
                    Description = "Test Description"
                }
            };

            var dto = new CreateMetadataRequestDto
            {
                Classes = JsonSerializer.SerializeToNode(classDtos) as JsonArray
            };

            var result = await _metadataBusiness.CreateMetadata(pid, did, dto);
            
            result.Should().NotBeNull();
            result.Classes.Should().HaveCount(1);
            result.Classes.First().Id.Should().BeGreaterThan(0);
            result.Classes.First().LastUpdatedAt.Should().BeOnOrAfter(now);
            result.Classes.First().Name.Should().Be("Test Metadata Class");
            result.Classes.First().Description.Should().Be("Test Description");
            result.Classes.First().ProjectId.Should().Be(pid);
            
            var eventList = await Context.Events.ToListAsync();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                ProjectId = pid,
                Operation = "create",
                EntityType = "class",
                EntityId = result.Classes.First().Id,
            });
        }

        [Fact]
        public async Task CreateMetadata_Success_OnBulkCreate()
        {
            var now = DateTime.UtcNow;
            
            var classDtos = new List<CreateClassRequestDto>
            {
                new CreateClassRequestDto
                {
                    Name = "Bulk Class 1",
                    Description = "First class"
                },
                new CreateClassRequestDto
                {
                    Name = "Bulk Class 2", 
                    Description = "Second class"
                }
            };

            var dto = new CreateMetadataRequestDto
            {
                Classes = JsonSerializer.SerializeToNode(classDtos) as JsonArray
            };

            var result = await _metadataBusiness.CreateMetadata(pid, did, dto);
            
            result.Classes.Should().HaveCount(2);
            result.Classes.First().Name.Should().Be("Bulk Class 1");
            result.Classes.Last().Name.Should().Be("Bulk Class 2");
            
           
            var eventList = await Context.Events.ToListAsync();
            eventList.Count.Should().Be(2);
            eventList[0].Should().BeEquivalentTo(new
            {
                ProjectId = pid,
                Operation = "create",
                EntityType = "class",
                EntityId = result.Classes[0].Id,
            });
            eventList[1].Should().BeEquivalentTo(new
            {
                ProjectId = pid,
                Operation = "create",
                EntityType = "class",
                EntityId = result.Classes[1].Id,
            });
        }

        [Fact]
        public async Task CreateMetadata_Success_WithRecordsAndAutoClasses()
        {
            
            var recordDtos = new List<CreateRecordRequestDto>
            {
                new CreateRecordRequestDto
                {
                    Name = "Test Record",
                    OriginalId = "rec-001", 
                    ClassName = "Auto Class",
                    Description = "Test Description",
                    Properties = JsonObject.Parse("{\"test\": \"value\"}") as JsonObject
                }
            };

            var dto = new CreateMetadataRequestDto
            {
                Records = JsonSerializer.SerializeToNode(recordDtos) as JsonArray
            };

            var result = await _metadataBusiness.CreateMetadata(pid, did, dto);

            result.Classes.Should().HaveCount(1);
            result.Records.Should().HaveCount(1);
            result.Classes.First().Name.Should().Be("Auto Class");
            result.Records.First().Name.Should().Be("Test Record");
            result.Records.First().OriginalId.Should().Be("rec-001");
            result.Records.First().ClassId.Should().Be(result.Classes.First().Id);
            
            var eventList = await Context.Events.ToListAsync();
            eventList.Count.Should().Be(2);
        }

        [Fact]
        public async Task CreateMetadata_Success_WithTagsAndRecords()
        {
            
            var recordDtos = new List<CreateRecordRequestDto>
            {
                new CreateRecordRequestDto
                {
                    Name = "Tagged Record",
                    OriginalId = "tagged-001",
                    Description = "Test Description",
                    Properties = JsonObject.Parse("{\"test\": \"value\"}") as JsonObject,
                    Tags = new List<string> { "important", "test" }
                }
            };

            var dto = new CreateMetadataRequestDto
            {
                Records = JsonSerializer.SerializeToNode(recordDtos) as JsonArray
            };

            var result = await _metadataBusiness.CreateMetadata(pid, did, dto);

            result.Records.Should().HaveCount(1);
            result.Tags.Should().HaveCount(2);
            result.Records.First().Tags.Should().HaveCount(2);
        }

        [Fact]
        public async Task CreateMetadata_Fails_IfNoProjectId()
        {
            
            var classDtos = new List<CreateClassRequestDto>
            {
                new CreateClassRequestDto { Name = "Test Class" }
            };

            var dto = new CreateMetadataRequestDto
            {
                Classes = JsonSerializer.SerializeToNode(classDtos) as JsonArray
            };

            var result = () => _metadataBusiness.CreateMetadata(pid + 99, did, dto);
            await result.Should().ThrowAsync<KeyNotFoundException>();
            
          
            var eventList = await Context.Events.ToListAsync();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task CreateMetadata_Fails_IfNoDataSourceId()
        {
            
            var recordDtos = new List<CreateRecordRequestDto>
            {
                new CreateRecordRequestDto
                {
                    Name = "Test Record",
                    OriginalId = "test-001",
                    Description = "Test Description",
                    Properties = JsonObject.Parse("{\"test\": \"value\"}") as JsonObject
                }
            };

            var dto = new CreateMetadataRequestDto
            {
                Records = JsonSerializer.SerializeToNode(recordDtos) as JsonArray
            };

            var result = () => _metadataBusiness.CreateMetadata(pid, did + 99, dto);
            await result.Should().ThrowAsync<KeyNotFoundException>();
            
          
            var eventList = await Context.Events.ToListAsync();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task CreateMetadata_Fails_IfDeletedProjectId()
        {
            var project = await Context.Projects.FindAsync(pid);
            project.IsArchived = true;
            Context.Projects.Update(project);
            await Context.SaveChangesAsync();
            
            
            var classDtos = new List<CreateClassRequestDto>
            {
                new CreateClassRequestDto { Name = "Test Class" }
            };

            var dto = new CreateMetadataRequestDto
            {
                Classes = JsonSerializer.SerializeToNode(classDtos) as JsonArray
            };
            
            var result = () => _metadataBusiness.CreateMetadata(pid, did, dto);
            await result.Should().ThrowAsync<KeyNotFoundException>();
            
          
            var eventList = await Context.Events.ToListAsync();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task CreateMetadata_Fails_IfDeletedDataSourceId()
        {
            var dataSource = await Context.DataSources.FindAsync(did);
            dataSource.IsArchived = true;
            Context.DataSources.Update(dataSource);
            await Context.SaveChangesAsync();
            
            
            var recordDtos = new List<CreateRecordRequestDto>
            {
                new CreateRecordRequestDto
                {
                    Name = "Test Record",
                    OriginalId = "test-001",
                    Description = "Test Description",
                    Properties = JsonObject.Parse("{\"test\": \"value\"}") as JsonObject
                }
            };

            var dto = new CreateMetadataRequestDto
            {
                Records = JsonSerializer.SerializeToNode(recordDtos) as JsonArray
            };
            
            var result = () => _metadataBusiness.CreateMetadata(pid, did, dto);
            await result.Should().ThrowAsync<KeyNotFoundException>();
            
          
            var eventList = await Context.Events.ToListAsync();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task CreateMetadata_Fails_IfNullDto()
        {
            var result = () => _metadataBusiness.CreateMetadata(pid, did, null);
            await result.Should().ThrowAsync<ArgumentNullException>();
            
          
            var eventList = await Context.Events.ToListAsync();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task CreateMetadata_Success_WithEmptyArrays()
        {
            var dto = new CreateMetadataRequestDto
            {
                Classes = JsonSerializer.SerializeToNode(new List<CreateClassRequestDto>()) as JsonArray,
                Relationships = JsonSerializer.SerializeToNode(new List<CreateRelationshipRequestDto>()) as JsonArray,
                Tags = JsonSerializer.SerializeToNode(new List<CreateTagRequestDto>()) as JsonArray,
                Records = JsonSerializer.SerializeToNode(new List<CreateRecordRequestDto>()) as JsonArray,
                Edges = JsonSerializer.SerializeToNode(new List<CreateEdgeRequestDto>()) as JsonArray
            };

            var result = await _metadataBusiness.CreateMetadata(pid, did, dto);

            result.Should().NotBeNull();
            result.Classes.Should().BeNull();
            result.Relationships.Should().BeNull();
            result.Tags.Should().BeNull();
            result.Records.Should().BeNull();
            result.Edges.Should().BeNull();
            
            var eventList = await Context.Events.ToListAsync();
            eventList.Count.Should().Be(0);
        }

        [Fact]
        public async Task CreateMetadata_Success_WithNullArrays()
        {
            var dto = new CreateMetadataRequestDto(); // All arrays are null by default

            var result = await _metadataBusiness.CreateMetadata(pid, did, dto);

            result.Should().NotBeNull();
            result.Classes.Should().BeNull();
            result.Relationships.Should().BeNull(); 
            result.Tags.Should().BeNull();
            result.Records.Should().BeNull();
            result.Edges.Should().BeNull();

            var eventList = await Context.Events.ToListAsync();
            eventList.Count.Should().Be(0);
        }
        
        [Fact]
        public async Task CreateMetadata_Success_WithComplexMetadata()
        {
            var classDtos = new List<CreateClassRequestDto>
            {
                new CreateClassRequestDto { Name = "New Class" }
            };

            var relationshipDtos = new List<CreateRelationshipRequestDto>
            {
                new CreateRelationshipRequestDto
                {
                    Name = "Test Relationship",
                    OriginId = originClassId,
                    DestinationId = destClassId
                }
            };

            var tagDtos = new List<CreateTagRequestDto>
            {
                new CreateTagRequestDto { Name = "Test Tag" }
            };

            var recordDtos = new List<CreateRecordRequestDto>
            {
                new CreateRecordRequestDto
                {
                    Name = "Test Record",
                    OriginalId = "test-1",
                    Description = "Test Description",
                    Properties = JsonObject.Parse("{\"test\": \"value\"}") as JsonObject
                },
                new CreateRecordRequestDto
                {
                    Name = "Test Record 2",
                    OriginalId = "test-2",
                    Description = "Test Description",
                    Properties = JsonObject.Parse("{\"test2\": \"value 2\"}") as JsonObject
                }
            };

            var edgeDtos = new List<CreateEdgeRequestDto>
            {
                new CreateEdgeRequestDto
                {
                    RelationshipName = "Test Relationship",
                    OriginOid = "test-1",
                    DestinationOid = "test-2"
                }
            };

            var dto = new CreateMetadataRequestDto
            {
                Classes = JsonSerializer.SerializeToNode(classDtos) as JsonArray,
                Relationships = JsonSerializer.SerializeToNode(relationshipDtos) as JsonArray,
                Tags = JsonSerializer.SerializeToNode(tagDtos) as JsonArray,
                Records = JsonSerializer.SerializeToNode(recordDtos) as JsonArray,
                Edges = JsonSerializer.SerializeToNode(edgeDtos) as JsonArray
            };

            var result = await _metadataBusiness.CreateMetadata(pid, did, dto);

            result.Classes.Should().HaveCount(1);
            result.Relationships.Should().HaveCount(1);
            result.Tags.Should().HaveCount(1);
            result.Records.Should().HaveCount(2);
            result.Edges.Should().HaveCount(1);
        }
        
        [Fact]
        public async Task CreateMetadata_Fails_WhenEdgeHasSameOriginAndDestination()
        {
            var classDtos = new List<CreateClassRequestDto>
            {
                new CreateClassRequestDto { Name = "New Class" }
            };

            var relationshipDtos = new List<CreateRelationshipRequestDto>
            {
                new CreateRelationshipRequestDto
                {
                    Name = "Test Relationship",
                    OriginId = originClassId,
                    DestinationId = destClassId
                }
            };

            var tagDtos = new List<CreateTagRequestDto>
            {
                new CreateTagRequestDto { Name = "Test Tag" }
            };

            var recordDtos = new List<CreateRecordRequestDto>
            {
                new CreateRecordRequestDto
                {
                    Name = "Test Record",
                    OriginalId = "test-1",
                    Description = "Test Description",
                    Properties = JsonObject.Parse("{\"test\": \"value\"}") as JsonObject
                }
            };

            var edgeDtos = new List<CreateEdgeRequestDto>
            {
                new CreateEdgeRequestDto
                {
                    RelationshipName = "Test Relationship",
                    OriginOid = "test-1",
                    DestinationOid = "test-1"
                }
            };

            var dto = new CreateMetadataRequestDto
            {
                Classes = JsonSerializer.SerializeToNode(classDtos) as JsonArray,
                Relationships = JsonSerializer.SerializeToNode(relationshipDtos) as JsonArray,
                Tags = JsonSerializer.SerializeToNode(tagDtos) as JsonArray,
                Records = JsonSerializer.SerializeToNode(recordDtos) as JsonArray,
                Edges = JsonSerializer.SerializeToNode(edgeDtos) as JsonArray
            };

            await Assert.ThrowsAsync<ValidationException>(() =>  _metadataBusiness.CreateMetadata(pid, did, dto));
        }

        [Fact]
        public async Task CreateMetadata_Fails_IfMissingRecordsForEdges()
        {
            
            var edgeDtos = new List<CreateEdgeRequestDto>
            {
                new CreateEdgeRequestDto
                {
                    RelationshipName = "Test Rel",
                    OriginOid = "missing-origin",
                    DestinationOid = "missing-dest"
                }
            };

            var dto = new CreateMetadataRequestDto
            {
                Edges = JsonSerializer.SerializeToNode(edgeDtos) as JsonArray
            };

            var result = () => _metadataBusiness.CreateMetadata(pid, did, dto);
            await result.Should().ThrowAsync<Exception>()
                .WithMessage("*Records not found matching Original IDs*missing-origin*missing-dest*");
            
            var eventList = await Context.Events.ToListAsync();
            eventList.Count.Should().Be(1);
            eventList[0].Should().BeEquivalentTo(new
            {
                ProjectId = pid,
                Operation = "create",
                EntityType = "relationship"
            });
        }

        protected override async Task SeedTestDataAsync()
        {
            await base.SeedTestDataAsync();
            
            // Create test project
            var project = new Project 
            { 
                Name = "Project 1", 
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null,
                IsArchived = false
            };
            Context.Projects.Add(project);
            await Context.SaveChangesAsync();
            pid = project.Id;
            
            var dataSource = new DataSource
            {
                Name = "Test Datasource",
                ProjectId = project.Id,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null,
                IsArchived = false
            };
            Context.DataSources.Add(dataSource);
            await Context.SaveChangesAsync();
            did = dataSource.Id;
            
            var originClass = new Class
            {
                Name = "Origin Class",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                IsArchived = false
            };
            var destClass = new Class
            {
                Name = "Dest Class",
                ProjectId = pid,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                IsArchived = false
            };
            Context.Classes.AddRange(originClass, destClass);
            await Context.SaveChangesAsync();
            originClassId = originClass.Id;
            destClassId = destClass.Id;
        }
    }
}