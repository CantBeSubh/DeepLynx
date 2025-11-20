using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using deeplynx.business;
using deeplynx.datalayer.Models;
using deeplynx.helpers.Hubs;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
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
        private INotificationBusiness _notificationBusiness = null!;
        private Mock<ILogger<NotificationBusiness>> _mockNotificationLogger = null!;
        private Mock<IHubContext<EventNotificationHub>> _mockHubContext = null!;
        private Mock<IRecordBusiness> _mockRecordBusiness = null!;
        private Mock<IRelationshipBusiness> _mockRelationshipBusiness = null!;
        private Mock<IEdgeBusiness> _mockEdgeBusiness = null!;
        
        public long pid;
        public long did;
        public long cid; // origin class ID
        public long cid2; // destination class ID
        public long uid;
        private long organizationId;

        public MetadataBusinessTests(TestSuiteFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            
            _mockRecordBusiness = new Mock<IRecordBusiness>();
            _mockRelationshipBusiness = new Mock<IRelationshipBusiness>();
            _mockEdgeBusiness = new Mock<IEdgeBusiness>();
            
            _mockHubContext = new Mock<IHubContext<EventNotificationHub>>();
            _mockNotificationLogger = new Mock<ILogger<NotificationBusiness>>();
            _notificationBusiness = new NotificationBusiness(Context, _mockNotificationLogger.Object, _mockHubContext.Object);
            _eventBusiness = new EventBusiness(Context, _cacheBusiness, _notificationBusiness);
            
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
        
        #region CreateMetadata Tests
        
        [Fact]
        public async Task CreateMetadata_Success_ReturnsIdAndCreatedAt()
        {
            // Arrange
            var now = DateTime.UtcNow;
    
            var dto = new CreateMetadataRequestDto
            {
                Classes = new List<CreateClassRequestDto>
                {
                    new CreateClassRequestDto
                    {
                        Name = "Test Metadata Class",
                        Description = "Test Description"
                    }
                }
            };
            // Act
            var result = await _metadataBusiness.CreateMetadata(uid, pid, did, dto);
            
            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Classes);
            Assert.True(result.Classes.First().Id > 0);
            Assert.True(result.Classes.First().LastUpdatedAt >= now);
            Assert.Equal("Test Metadata Class", result.Classes.First().Name);
            Assert.Equal("Test Description", result.Classes.First().Description);
            Assert.Equal(pid, result.Classes.First().ProjectId);

            // Ensure create class event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);
            
            var actualEvent = eventList[0];
            
            Assert.Equal(pid, actualEvent.ProjectId);
            Assert.Equal("create", actualEvent.Operation);
            Assert.Equal("class", actualEvent.EntityType);
            Assert.Equal(result.Classes.First().Id, actualEvent.EntityId);
        }

        [Fact]
        public async Task CreateMetadata_Success_OnBulkCreate()
        {
            // Arrange
            var now = DateTime.UtcNow;

            var dto = new CreateMetadataRequestDto
            {
                Classes = new List<CreateClassRequestDto>
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
                }
            };

            // Act
            var result = await _metadataBusiness.CreateMetadata(uid, pid, did, dto);
            
            // Assert
            Assert.Equal(2, result.Classes.Count);
            Assert.True(result.Classes.All(c => c.LastUpdatedBy == uid && 
                                                c.ProjectId == pid && 
                                                !c.IsArchived));
            Assert.Equal("Bulk Class 1", result.Classes.First().Name);
            Assert.Equal("First class", result.Classes.First().Description);
            Assert.Equal("Bulk Class 2", result.Classes.Last().Name);
            Assert.Equal("Second class", result.Classes.Last().Description);

            // Ensure both create class events are logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Equal(2, eventList.Count);

            var actualEvent0 = eventList[0];
            Assert.Equal(pid, actualEvent0.ProjectId);
            Assert.Equal("create", actualEvent0.Operation);
            Assert.Equal("class", actualEvent0.EntityType);
            Assert.Equal(result.Classes[0].Id, actualEvent0.EntityId);

            var actualEvent1 = eventList[1];
            Assert.Equal(pid, actualEvent1.ProjectId);
            Assert.Equal("create", actualEvent1.Operation);
            Assert.Equal("class", actualEvent1.EntityType);
            Assert.Equal(result.Classes[1].Id, actualEvent1.EntityId);
        }

        [Fact]
        public async Task CreateMetadata_Success_WithRecordsAndAutoClasses()
        {
            // Arrange
            var dto = new CreateMetadataRequestDto
            {
                Records = new List<CreateRecordRequestDto>
                {
                    new CreateRecordRequestDto
                    {
                        Name = "Test Record",
                        OriginalId = "rec-001", 
                        ClassName = "Auto Class",
                        Description = "Test Description",
                        Properties = JsonObject.Parse("{\"test\": \"value\"}") as JsonObject
                    }
                }
            };

            // Act
            var result = await _metadataBusiness.CreateMetadata(uid, pid, did, dto);
            
            // Assert
            Assert.Single(result.Classes);
            Assert.Single(result.Records);
            Assert.Equal("Auto Class", result.Classes.First().Name);
            Assert.Equal("Test Record", result.Records.First().Name);
            Assert.Equal("rec-001", result.Records.First().OriginalId);
            Assert.Equal(result.Classes.First().Id, result.Records.First().ClassId);

            // Ensure both create class and create record events are logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Equal(2, eventList.Count);
            
            var actualEvent0 = eventList[0];
            Assert.Equal(pid, actualEvent0.ProjectId);
            Assert.Equal("create", actualEvent0.Operation);
            Assert.Equal("class", actualEvent0.EntityType);
            Assert.Equal(result.Classes[0].Id, actualEvent0.EntityId);

            var actualEvent1 = eventList[1];
            Assert.Equal(pid, actualEvent1.ProjectId);
            Assert.Equal("create", actualEvent1.Operation);
            Assert.Equal("record", actualEvent1.EntityType);
            Assert.Equal(result.Records[0].Id, actualEvent1.EntityId);
        }

        [Fact]
        public async Task CreateMetadata_Success_WithTagsAndRecords()
        {
            // Arrange
            var dto = new CreateMetadataRequestDto
            {
                Records = new List<CreateRecordRequestDto>
                {
                    new CreateRecordRequestDto
                    {
                        Name = "Tagged Record",
                        OriginalId = "tagged-001",
                        Description = "Test Description",
                        Properties = JsonObject.Parse("{\"test\": \"value\"}") as JsonObject,
                        Tags = new List<string> { "important", "test" }
                    }
                }
            };

            // Act
            var result = await _metadataBusiness.CreateMetadata(uid, pid, did, dto);
            
            // Assert
            Assert.Single(result.Records);
            Assert.Equal(2, result.Tags.Count);
            Assert.Equal(2, result.Records.First().Tags.Count);
            
            // Ensure both create record and tags are logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Equal(3, eventList.Count);
            
            var actualEvent0 = eventList[0];
            Assert.Equal(pid, actualEvent0.ProjectId);
            Assert.Equal("create", actualEvent0.Operation);
            Assert.Equal("tag", actualEvent0.EntityType);
            Assert.Equal(result.Tags[0].Id, actualEvent0.EntityId);

            var actualEvent1 = eventList[1];
            Assert.Equal(pid, actualEvent1.ProjectId);
            Assert.Equal("create", actualEvent1.Operation);
            Assert.Equal("tag", actualEvent1.EntityType);
            Assert.Equal(result.Tags[1].Id, actualEvent1.EntityId);
            
            var actualEvent2 = eventList[2];
            Assert.Equal(pid, actualEvent2.ProjectId);
            Assert.Equal("create", actualEvent2.Operation);
            Assert.Equal("record", actualEvent2.EntityType);
            Assert.Equal(result.Records[0].Id, actualEvent2.EntityId);
        }

        [Fact]
        public async Task CreateMetadata_Fails_IfNoProjectId()
        {
            // Arrange
            var dto = new CreateMetadataRequestDto
            {
                Classes = new List<CreateClassRequestDto>
                {
                    new CreateClassRequestDto { Name = "Test Class" }
                }
            };
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _metadataBusiness.CreateMetadata(uid, pid + 99, did, dto));
            Assert.Contains($"Project with id {pid + 99} not found.", exception.Message);
          
            // Ensure create event was NOT logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreateMetadata_Fails_IfNoDataSourceId()
        {
            // Arrange
            var dto = new CreateMetadataRequestDto
            {
                Records = new List<CreateRecordRequestDto>
                {
                    new CreateRecordRequestDto
                    {
                        Name = "Test Record",
                        OriginalId = "test-001",
                        Description = "Test Description",
                        Properties = JsonObject.Parse("{\"test\": \"value\"}") as JsonObject
                    }
                }
            };
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _metadataBusiness.CreateMetadata(uid, pid, did + 99, dto));
            Assert.Contains($"DataSource with id {did + 99} not found in project with id {pid}", exception.Message);
            
            // Ensure create event was NOT logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreateMetadata_Fails_IfDeletedProjectId()
        {
            // Arrange
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
                Classes = classDtos
            };
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _metadataBusiness.CreateMetadata(uid, pid, did, dto));
            Assert.Contains($"Project with id {pid} not found.", exception.Message);
            
            // Ensure create event was NOT logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreateMetadata_Fails_IfDeletedDataSourceId()
        {
            // Arrange
            var dataSource = await Context.DataSources.FindAsync(did);
            dataSource.IsArchived = true;
            Context.DataSources.Update(dataSource);
            await Context.SaveChangesAsync();

            var dto = new CreateMetadataRequestDto
            {
                Records = new List<CreateRecordRequestDto>
                {
                    new CreateRecordRequestDto
                    {
                        Name = "Test Record",
                        OriginalId = "test-001",
                        Description = "Test Description",
                        Properties = JsonObject.Parse("{\"test\": \"value\"}") as JsonObject
                    }
                }
            };
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _metadataBusiness.CreateMetadata(uid, pid, did, dto));
            Assert.Contains($"DataSource with id {did} not found in project with id {pid}", exception.Message);
            
            // Ensure create event was NOT logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreateMetadata_Fails_IfNullDto()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _metadataBusiness.CreateMetadata(uid, pid, did, null));

            // Ensure create event was NOT logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreateMetadata_Success_WithEmptyArrays()
        {
            // Arrange
            var dto = new CreateMetadataRequestDto
            {
                Classes = new List<CreateClassRequestDto>(),
                Relationships = new List<CreateRelationshipRequestDto>(),
                Tags = new List<CreateTagRequestDto>(),
                Records = new List<CreateRecordRequestDto>(),
                Edges = new List<CreateEdgeRequestDto>()
            };

            // Act
            var result = await _metadataBusiness.CreateMetadata(uid, pid, did, dto);
            
            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Classes);
            Assert.Null(result.Relationships);
            Assert.Null(result.Tags);
            Assert.Null(result.Records);
            Assert.Null(result.Edges);

            // Ensure create event was NOT logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreateMetadata_Success_WithNullArrays()
        {
            // Arrange
            var dto = new CreateMetadataRequestDto(); // All arrays are null by default

            // Act
            var result = await _metadataBusiness.CreateMetadata(uid, pid, did, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Classes);
            Assert.Null(result.Relationships);
            Assert.Null(result.Tags);
            Assert.Null(result.Records);
            Assert.Null(result.Edges);

            // Ensure create event was NOT logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }
        
        [Fact]
        public async Task CreateMetadata_Success_WithComplexMetadata()
        {
            // Arrange
            var dto = new CreateMetadataRequestDto
            {
                Classes = new List<CreateClassRequestDto>
                {
                    new CreateClassRequestDto { Name = "New Class" }
                },
                Relationships = new List<CreateRelationshipRequestDto>
                {
                    new CreateRelationshipRequestDto
                    {
                        Name = "Test Relationship",
                    }
                },
                Tags = new List<CreateTagRequestDto>
                {
                    new CreateTagRequestDto { Name = "Test Tag" }
                },
                Records = new List<CreateRecordRequestDto>
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
                        Description = "Test Description 2",
                        Properties = JsonObject.Parse("{\"test2\": \"value 2\"}") as JsonObject
                    }
                },
                Edges = new List<CreateEdgeRequestDto>
                {
                    new CreateEdgeRequestDto
                    {
                        RelationshipName = "Test Relationship",
                        OriginOid = "test-1",
                        DestinationOid = "test-2"
                    }
                }
            };
            
            var result = await _metadataBusiness.CreateMetadata(uid, pid, did, dto);
            
            // Assert
            Assert.Single(result.Classes);
            Assert.Single(result.Relationships);
            Assert.Single(result.Tags);
            Assert.Equal(2, result.Records.Count);
            Assert.Single(result.Edges);
            
            var createdClass = result.Classes[0];
            Assert.Equal("New Class", createdClass.Name);
            Assert.Equal(pid, createdClass.ProjectId);
            
            var createdRelationship = result.Relationships[0];
            Assert.Equal("Test Relationship", createdRelationship.Name);
            Assert.Equal(pid, createdRelationship.ProjectId);
            
            var createdTag = result.Tags[0];
            Assert.Equal("Test Tag", createdTag.Name);
            Assert.Equal(pid, createdTag.ProjectId);
            
            var createdRecord1 = result.Records[0];
            Assert.Equal("Test Record", createdRecord1.Name);
            Assert.Equal("test-1", createdRecord1.OriginalId);
            Assert.Equal("Test Description", createdRecord1.Description);
            Assert.NotNull(createdRecord1.Properties);
            Assert.Equal(pid, createdRecord1.ProjectId);
            Assert.Equal(did, createdRecord1.DataSourceId);
            
            var createdRecord2 = result.Records[1];
            Assert.Equal("Test Record 2", createdRecord2.Name);
            Assert.Equal("test-2", createdRecord2.OriginalId);
            Assert.Equal("Test Description 2", createdRecord2.Description);
            Assert.NotNull(createdRecord2.Properties);
            Assert.Equal(pid, createdRecord2.ProjectId);
            Assert.Equal(did, createdRecord2.DataSourceId);
            
            var createdEdge = result.Edges[0];
            Assert.Equal(createdRelationship.Id, createdEdge.RelationshipId);
            Assert.Equal(createdRecord1.Id, createdEdge.OriginId);
            Assert.Equal(createdRecord2.Id, createdEdge.DestinationId);
            Assert.Equal(pid, createdEdge.ProjectId);
            Assert.Equal(did, createdEdge.DataSourceId);
            
            // Ensure all complex data events are created and logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Equal(6, eventList.Count);
            
            var actualEvent0 = eventList[0];
            Assert.Equal(pid, actualEvent0.ProjectId);
            Assert.Equal("create", actualEvent0.Operation);
            Assert.Equal("class", actualEvent0.EntityType);
            Assert.Equal(result.Classes[0].Id, actualEvent0.EntityId);

            var actualEvent1 = eventList[1];
            Assert.Equal(pid, actualEvent1.ProjectId);
            Assert.Equal("create", actualEvent1.Operation);
            Assert.Equal("relationship", actualEvent1.EntityType);
            Assert.Equal(result.Relationships[0].Id, actualEvent1.EntityId);
            
            var actualEvent2 = eventList[2];
            Assert.Equal(pid, actualEvent2.ProjectId);
            Assert.Equal("create", actualEvent2.Operation);
            Assert.Equal("tag", actualEvent2.EntityType);
            Assert.Equal(result.Tags[0].Id, actualEvent2.EntityId);
            
            var actualEvent3 = eventList[3];
            Assert.Equal(pid, actualEvent3.ProjectId);
            Assert.Equal("create", actualEvent3.Operation);
            Assert.Equal("record", actualEvent3.EntityType);
            Assert.Equal(result.Records[0].Id, actualEvent3.EntityId);
            
            var actualEvent4 = eventList[4];
            Assert.Equal(pid, actualEvent4.ProjectId);
            Assert.Equal("create", actualEvent4.Operation);
            Assert.Equal("record", actualEvent4.EntityType);
            Assert.Equal(result.Records[1].Id, actualEvent4.EntityId);
            
            var actualEvent5 = eventList[5];
            Assert.Equal(pid, actualEvent5.ProjectId);
            Assert.Equal("create", actualEvent5.Operation);
            Assert.Equal("edge", actualEvent5.EntityType);
            Assert.Equal(result.Edges[0].Id, actualEvent5.EntityId);
        }
        
        [Fact]
        public async Task CreateMetadata_Fails_WhenEdgeHasSameOriginAndDestination()
        {
            var dto = new CreateMetadataRequestDto
            {
                Classes = new List<CreateClassRequestDto>
                {
                    new CreateClassRequestDto { Name = "New Class" }
                },
                Relationships = new List<CreateRelationshipRequestDto>
                {
                    new CreateRelationshipRequestDto
                    {
                        Name = "Test Relationship",
                        OriginId = cid,
                        DestinationId = cid2
                    }
                },
                Tags = new List<CreateTagRequestDto>
                {
                    new CreateTagRequestDto { Name = "Test Tag" }
                },
                Records = new List<CreateRecordRequestDto>
                {
                    new CreateRecordRequestDto
                    {
                        Name = "Test Record",
                        OriginalId = "test-1",
                        Description = "Test Description",
                        Properties = JsonObject.Parse("{\"test\": \"value\"}") as JsonObject
                    }
                },
                Edges = new List<CreateEdgeRequestDto>
                {
                    new CreateEdgeRequestDto
                    {
                        RelationshipName = "Test Relationship",
                        OriginOid = "test-1",
                        DestinationOid = "test-1"
                    }
                }
            };

            // Act
            await Assert.ThrowsAsync<ValidationException> (() => _metadataBusiness.CreateMetadata(uid, pid, did, dto));
        }

        [Fact]
        public async Task CreateMetadata_Fails_IfMissingRecordsForEdges()
        {
            // Arrange
            var dto = new CreateMetadataRequestDto
            {
                Edges = new List<CreateEdgeRequestDto>
                {
                    new CreateEdgeRequestDto
                    {
                        RelationshipName = "Test Rel",
                        OriginOid = "missing-origin",
                        DestinationOid = "missing-dest"
                    }
                }
            };
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _metadataBusiness.CreateMetadata(uid, pid, did, dto));
            Assert.Contains("Records not found matching Original IDs", exception.Message);
            
            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);
            
            // Ensure we at least create relationship
            var actualEvent = eventList[0];
            
            Assert.Equal(pid, actualEvent.ProjectId);
            Assert.Equal("create", actualEvent.Operation);
            Assert.Equal("relationship", actualEvent.EntityType);
        }
        
        #endregion

        #region CreateMetadataFromFile Tests
        
        [Fact]
        public async Task CreateMetadataFromFile_Success_ReturnsIdAndCreatedAt()
        {
            // Arrange
            var now = DateTime.UtcNow;
    
            var metadataContent = new
            {
                classes = new[]
                {
                    new
                    {
                        name = "Test Metadata Class",
                        description = "Test Description"
                    }
                }
            };

            var file = CreateJsonFile(metadataContent);

            // Act
            var result = await _metadataBusiness.CreateMetadataFromFile(uid, pid, did, file);
    
            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Classes);
            Assert.True(result.Classes.First().Id > 0);
            Assert.True(result.Classes.First().LastUpdatedAt >= now);
            Assert.Equal("Test Metadata Class", result.Classes.First().Name);
            Assert.Equal("Test Description", result.Classes.First().Description);
            Assert.Equal(pid, result.Classes.First().ProjectId);

            // Ensure create class event was logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);
    
            var actualEvent = eventList[0];
    
            Assert.Equal(pid, actualEvent.ProjectId);
            Assert.Equal("create", actualEvent.Operation);
            Assert.Equal("class", actualEvent.EntityType);
            Assert.Equal(result.Classes.First().Id, actualEvent.EntityId);
        }

        [Fact]
        public async Task CreateMetadataFromFile_Success_OnBulkCreate()
        {
            // Arrange
            var metadataContent = new
            {
                classes = new[]
                {
                    new { name = "Bulk Class 1", description = "First class" },
                    new { name = "Bulk Class 2", description = "Second class" }
                }
            };

            var file = CreateJsonFile(metadataContent);

            // Act
            var result = await _metadataBusiness.CreateMetadataFromFile(uid, pid, did, file);
    
            // Assert
            Assert.Equal(2, result.Classes.Count);
            Assert.True(result.Classes.All(c => c.LastUpdatedBy == uid && 
                                                c.ProjectId == pid &&
                                                !c.IsArchived));
            Assert.Equal("Bulk Class 1", result.Classes.First().Name);
            Assert.Equal("First class", result.Classes.First().Description);
            Assert.Equal("Bulk Class 2", result.Classes.Last().Name);
            Assert.Equal("Second class", result.Classes.Last().Description);

            var eventList = await Context.Events.ToListAsync();
            Assert.Equal(2, eventList.Count);
    
            var actualEvent0 = eventList[0];
            Assert.Equal(pid, actualEvent0.ProjectId);
            Assert.Equal("create", actualEvent0.Operation);
            Assert.Equal("class", actualEvent0.EntityType);
            Assert.Equal(result.Classes[0].Id, actualEvent0.EntityId);

            var actualEvent1 = eventList[1];
            Assert.Equal(pid, actualEvent1.ProjectId);
            Assert.Equal("create", actualEvent1.Operation);
            Assert.Equal("class", actualEvent1.EntityType);
            Assert.Equal(result.Classes[1].Id, actualEvent1.EntityId);
        }
        
        [Fact]
        public async Task CreateMetadataFromFile_Success_WithRecordsAndAutoClasses()
        {
            // Arrange
            var metadataContent = new
            {
                records = new[]
                {
                    new
                    {
                        name = "Test Record",
                        original_id = "rec-001",
                        class_name = "Auto Class",
                        description = "Test Description",
                        properties = new { test = "value" }
                    }
                }
            };

            var file = CreateJsonFile(metadataContent);

            // Act
            var result = await _metadataBusiness.CreateMetadataFromFile(uid, pid, did, file);
    
            // Assert
            Assert.Single(result.Classes);
            Assert.Single(result.Records);
            Assert.Equal("Auto Class", result.Classes.First().Name);
            Assert.Equal("Test Record", result.Records.First().Name);
            Assert.Equal("rec-001", result.Records.First().OriginalId);
            Assert.Equal(result.Classes.First().Id, result.Records.First().ClassId);

            var eventList = await Context.Events.ToListAsync();
            Assert.Equal(2, eventList.Count);
    
            var actualEvent0 = eventList[0];
            Assert.Equal(pid, actualEvent0.ProjectId);
            Assert.Equal("create", actualEvent0.Operation);
            Assert.Equal("class", actualEvent0.EntityType);
            Assert.Equal(result.Classes[0].Id, actualEvent0.EntityId);

            var actualEvent1 = eventList[1];
            Assert.Equal(pid, actualEvent1.ProjectId);
            Assert.Equal("create", actualEvent1.Operation);
            Assert.Equal("record", actualEvent1.EntityType);
            Assert.Equal(result.Records[0].Id, actualEvent1.EntityId);
        }

        [Fact]
        public async Task CreateMetadataFromFile_Success_WithTagsAndRecords()
        {
            // Arrange
            var metadataContent = new
            {
                records = new[]
                {
                    new
                    {
                        name = "Tagged Record",
                        original_id = "tagged-001",
                        description = "Test Description",
                        properties = new { test = "value" },
                        tags = new[] { "important", "test" }
                    }
                }
            };

            var file = CreateJsonFile(metadataContent);

            // Act
            var result = await _metadataBusiness.CreateMetadataFromFile(uid, pid, did, file);
    
            // Assert
            Assert.Single(result.Records);
            Assert.Equal(2, result.Tags.Count);
            Assert.Equal(2, result.Records.First().Tags.Count);
    
            // Ensure both create record and tags are logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Equal(3, eventList.Count);
    
            var actualEvent0 = eventList[0];
            Assert.Equal(pid, actualEvent0.ProjectId);
            Assert.Equal("create", actualEvent0.Operation);
            Assert.Equal("tag", actualEvent0.EntityType);
            Assert.Equal(result.Tags[0].Id, actualEvent0.EntityId);

            var actualEvent1 = eventList[1];
            Assert.Equal(pid, actualEvent1.ProjectId);
            Assert.Equal("create", actualEvent1.Operation);
            Assert.Equal("tag", actualEvent1.EntityType);
            Assert.Equal(result.Tags[1].Id, actualEvent1.EntityId);
    
            var actualEvent2 = eventList[2];
            Assert.Equal(pid, actualEvent2.ProjectId);
            Assert.Equal("create", actualEvent2.Operation);
            Assert.Equal("record", actualEvent2.EntityType);
            Assert.Equal(result.Records[0].Id, actualEvent2.EntityId);
        }
        
        [Fact]
        public async Task CreateMetadataFromFile_Fails_IfNoProjectId()
        {
            // Arrange
            var metadataContent = new
            {
                classes = new[]
                {
                    new { name = "Test Class" }
                }
            };

            var file = CreateJsonFile(metadataContent);
    
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _metadataBusiness.CreateMetadataFromFile(uid, pid + 99, did, file));
            Assert.Contains($"Project with id {pid + 99} not found.", exception.Message);
  
            // Ensure create event was NOT logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreateMetadataFromFile_Fails_IfNoDataSourceId()
        {
            // Arrange
            var metadataContent = new
            {
                records = new[]
                {
                    new
                    {
                        name = "Test Record",
                        original_id = "test-001",
                        description = "Test Description",
                        properties = new { test = "value" }
                    }
                }
            };

            var file = CreateJsonFile(metadataContent);
    
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _metadataBusiness.CreateMetadataFromFile(uid, pid, did + 99, file));
            Assert.Contains($"DataSource with id {did + 99} not found in project with id {pid}", exception.Message);
    
            // Ensure create event was NOT logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreateMetadataFromFile_Fails_IfDeletedProjectId()
        {
            // Arrange
            var project = await Context.Projects.FindAsync(pid);
            project.IsArchived = true;
            Context.Projects.Update(project);
            await Context.SaveChangesAsync();
    
            var metadataContent = new
            {
                classes = new[]
                {
                    new { name = "Test Class" }
                }
            };

            var file = CreateJsonFile(metadataContent);
    
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _metadataBusiness.CreateMetadataFromFile(uid, pid, did, file));
            Assert.Contains($"Project with id {pid} not found.", exception.Message);
    
            // Ensure create event was NOT logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreateMetadataFromFile_Fails_IfDeletedDataSourceId()
        {
            // Arrange
            var dataSource = await Context.DataSources.FindAsync(did);
            dataSource.IsArchived = true;
            Context.DataSources.Update(dataSource);
            await Context.SaveChangesAsync();
    
            var metadataContent = new
            {
                records = new[]
                {
                    new
                    {
                        name = "Test Record",
                        original_id = "test-001",
                        description = "Test Description",
                        properties = new { test = "value" }
                    }
                }
            };

            var file = CreateJsonFile(metadataContent);
    
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _metadataBusiness.CreateMetadataFromFile(uid, pid, did, file));
            Assert.Contains($"DataSource with id {did} not found in project with id {pid}", exception.Message);
    
            // Ensure create event was NOT logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreateMetadataFromFile_Fails_IfNullFile()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                () => _metadataBusiness.CreateMetadataFromFile(uid, pid, did, null)
            );
    
            Assert.Equal("file", exception.ParamName);
            Assert.Contains("File cannot be null.", exception.Message);

            // Ensure create event was NOT logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }

        [Fact]
        public async Task CreateMetadataFromFile_Success_WithEmptyArrays()
        {
            // Arrange
            var metadataContent = new
            {
                classes = Array.Empty<object>(),
                relationships = Array.Empty<object>(),
                tags = Array.Empty<object>(),
                records = Array.Empty<object>(),
                edges = Array.Empty<object>()
            };

            var file = CreateJsonFile(metadataContent);

            // Act
            var result = await _metadataBusiness.CreateMetadataFromFile(uid, pid, did, file);
    
            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Classes);
            Assert.Null(result.Relationships);
            Assert.Null(result.Tags);
            Assert.Null(result.Records);
            Assert.Null(result.Edges);

            // Ensure create event was NOT logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }
        
        [Fact]
        public async Task CreateMetadataFromFile_Fails_IfEmptyFile()
        {
            // Arrange
            var emptyStream = new MemoryStream();
            var file = new FormFile(emptyStream, 0, 0, "file", "metadata.json")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/json"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _metadataBusiness.CreateMetadataFromFile(uid, pid, did, file)
            );
    
            Assert.Equal("file", exception.ParamName);
            Assert.Contains("File cannot be empty.", exception.Message);
    
            // Ensure no events were logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }
        
        [Fact]
        public async Task CreateMetadataFromFile_Fails_IfWrongFileExtension()
        {
            // Arrange
            var metadataContent = new
            {
                classes = new[] { new { name = "Test" } }
            };
    
            var file = CreateJsonFile(metadataContent, "metadata.txt"); // Wrong extension

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _metadataBusiness.CreateMetadataFromFile(uid, pid, did, file)
            );
    
            Assert.Equal("file", exception.ParamName);
            Assert.Contains("File must be a .json file.", exception.Message);
    
            // Ensure no events were logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }
        
        [Fact]
        public async Task CreateMetadataFromFile_Fails_IfXmlExtension()
        {
            // Arrange
            var metadataContent = new
            {
                classes = new[] { new { name = "Test" } }
            };
    
            var file = CreateJsonFile(metadataContent, "metadata.xml");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _metadataBusiness.CreateMetadataFromFile(uid, pid, did, file)
            );
    
            Assert.Contains("File must be a .json file", exception.Message);
        }
        
        [Fact]
        public async Task CreateMetadataFromFile_Fails_IfNoExtension()
        {
            // Arrange
            var metadataContent = new
            {
                classes = new[] { new { name = "Test" } }
            };
    
            var file = CreateJsonFile(metadataContent, "metadata");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _metadataBusiness.CreateMetadataFromFile(uid, pid, did, file)
            );
    
            Assert.Contains("File must be a .json file", exception.Message);
        }
        
        [Fact]
        public async Task CreateMetadataFromFile_Fails_IfInvalidJson()
        {
            // Arrange
            var invalidJson = "{ this is not valid json }";
            var bytes = Encoding.UTF8.GetBytes(invalidJson);
            var stream = new MemoryStream(bytes);
    
            var file = new FormFile(stream, 0, bytes.Length, "file", "metadata.json")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/json"
            };

            // Act & Assert
            await Assert.ThrowsAsync<JsonException>(
                () => _metadataBusiness.CreateMetadataFromFile(uid, pid, did, file)
            );
    
            // Ensure no events were logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }
        
        [Fact]
        public async Task CreateMetadataFromFile_Fails_IfMalformedJson()
        {
            // Arrange
            var malformedJson = "{\"classes\": [}"; // Missing closing bracket
            var bytes = Encoding.UTF8.GetBytes(malformedJson);
            var stream = new MemoryStream(bytes);
    
            var file = new FormFile(stream, 0, bytes.Length, "file", "metadata.json")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/json"
            };

            // Act & Assert
            await Assert.ThrowsAsync<JsonException>(
                () => _metadataBusiness.CreateMetadataFromFile(uid, pid, did, file)
            );
        }

        [Fact]
        public async Task CreateMetadataFromFile_Fails_IfJsonIsNull()
        {
            // Arrange
            var nullJson = "null";
            var bytes = Encoding.UTF8.GetBytes(nullJson);
            var stream = new MemoryStream(bytes);
    
            var file = new FormFile(stream, 0, bytes.Length, "file", "metadata.json")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/json"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<JsonException>(
                () => _metadataBusiness.CreateMetadataFromFile(uid, pid, did, file)
            );
    
            Assert.Contains("Failed to deserialize metadata from file", exception.Message);
        }

        [Fact]
        public async Task CreateMetadataFromFile_Success_WithJsonExtensionUpperCase()
        {
            // Arrange - Test that .JSON (uppercase) is accepted
            var metadataContent = new
            {
                classes = new[] { new { name = "Test Class" } }
            };
    
            var file = CreateJsonFile(metadataContent, "metadata.JSON");

            // Act
            var result = await _metadataBusiness.CreateMetadataFromFile(uid, pid, did, file);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Classes);
            Assert.Equal("Test Class", result.Classes.First().Name);
        }

        [Fact]
        public async Task CreateMetadataFromFile_Success_WithMixedCaseExtension()
        {
            // Arrange - Test that .Json (mixed case) is accepted
            var metadataContent = new
            {
                classes = new[] { new { name = "Test Class" } }
            };
    
            var file = CreateJsonFile(metadataContent, "metadata.Json");

            // Act
            var result = await _metadataBusiness.CreateMetadataFromFile(uid, pid, did, file);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Classes);
        }

        [Fact]
        public async Task CreateMetadataFromFile_Success_WithNullArrays()
        {
            // Arrange
            var metadataContent = new { }; // Empty JSON object - all properties will be null

            var file = CreateJsonFile(metadataContent);

            // Act
            var result = await _metadataBusiness.CreateMetadataFromFile(uid, pid, did, file);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Classes);
            Assert.Null(result.Relationships);
            Assert.Null(result.Tags);
            Assert.Null(result.Records);
            Assert.Null(result.Edges);

            // Ensure create event was NOT logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Empty(eventList);
        }
        
        [Fact]
        public async Task CreateMetadataFromFile_Success_WithComplexMetadata()
        {
            // Arrange
            var metadataContent = new
            {
                classes = new[]
                {
                    new { name = "New Class" }
                },
                relationships = new[]
                {
                    new
                    {
                        name = "Test Relationship",
                        origin_id = cid,
                        destination_id = cid2
                    }
                },
                tags = new[]
                {
                    new { name = "Test Tag" }
                },
                records = new object[]
                {
                    new
                    {
                        name = "Test Record",
                        original_id = "test-1",
                        description = "Test Description",
                        properties = new { test = "value" }
                    },
                    new
                    {
                        name = "Test Record 2",
                        original_id = "test-2",
                        description = "Test Description",
                        properties = new { test2 = "value 2" }
                    }
                },
                edges = new[]
                {
                    new
                    {
                        relationship_name = "Test Relationship",
                        origin_oid = "test-1",
                        destination_oid = "test-2"
                    }
                }
            };

            var file = CreateJsonFile(metadataContent);

            // Act
            var result = await _metadataBusiness.CreateMetadataFromFile(uid, pid, did, file);
            
            // Assert
            Assert.Single(result.Classes);
            Assert.Single(result.Relationships);
            Assert.Single(result.Tags);
            Assert.Equal(2, result.Records.Count);
            Assert.Single(result.Edges);
            
            // Ensure all complex data events are created and logged
            var eventList = await Context.Events.ToListAsync();
            Assert.Equal(6, eventList.Count);
            
            var actualEvent0 = eventList[0];
            Assert.Equal(pid, actualEvent0.ProjectId);
            Assert.Equal("create", actualEvent0.Operation);
            Assert.Equal("class", actualEvent0.EntityType);
            Assert.Equal(result.Classes[0].Id, actualEvent0.EntityId);

            var actualEvent1 = eventList[1];
            Assert.Equal(pid, actualEvent1.ProjectId);
            Assert.Equal("create", actualEvent1.Operation);
            Assert.Equal("relationship", actualEvent1.EntityType);
            Assert.Equal(result.Relationships[0].Id, actualEvent1.EntityId);
            
            var actualEvent2 = eventList[2];
            Assert.Equal(pid, actualEvent2.ProjectId);
            Assert.Equal("create", actualEvent2.Operation);
            Assert.Equal("tag", actualEvent2.EntityType);
            Assert.Equal(result.Tags[0].Id, actualEvent2.EntityId);
            
            var actualEvent3 = eventList[3];
            Assert.Equal(pid, actualEvent3.ProjectId);
            Assert.Equal("create", actualEvent3.Operation);
            Assert.Equal("record", actualEvent3.EntityType);
            Assert.Equal(result.Records[0].Id, actualEvent3.EntityId);
            
            var actualEvent4 = eventList[4];
            Assert.Equal(pid, actualEvent4.ProjectId);
            Assert.Equal("create", actualEvent4.Operation);
            Assert.Equal("record", actualEvent4.EntityType);
            Assert.Equal(result.Records[1].Id, actualEvent4.EntityId);
            
            var actualEvent5 = eventList[5];
            Assert.Equal(pid, actualEvent5.ProjectId);
            Assert.Equal("create", actualEvent5.Operation);
            Assert.Equal("edge", actualEvent5.EntityType);
            Assert.Equal(result.Edges[0].Id, actualEvent5.EntityId);
        }
        
        [Fact]
        public async Task CreateMetadataFromFile_Fails_WhenEdgeHasSameOriginAndDestination()
        {
            // Arrange
            var metadataContent = new
            {
                classes = new[]
                {
                    new { name = "New Class" }
                },
                relationships = new[]
                {
                    new
                    {
                        name = "Test Relationship",
                        origin_id = cid,
                        destination_id = cid2
                    }
                },
                tags = new[]
                {
                    new { name = "Test Tag" }
                },
                records = new[]
                {
                    new
                    {
                        name = "Test Record",
                        original_id = "test-1",
                        description = "Test Description",
                        properties = new { test = "value" }
                    }
                },
                edges = new[]
                {
                    new
                    {
                        relationship_name = "Test Relationship",
                        origin_oid = "test-1",
                        destination_oid = "test-1" // Same as origin - should fail
                    }
                }
            };

            var file = CreateJsonFile(metadataContent);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _metadataBusiness.CreateMetadataFromFile(uid, pid, did, file));
        }

        [Fact]
        public async Task CreateMetadataFromFile_Fails_IfMissingRecordsForEdges()
        {
            // Arrange
            var metadataContent = new
            {
                edges = new[]
                {
                    new
                    {
                        relationship_name = "Test Rel",
                        origin_oid = "missing-origin",
                        destination_oid = "missing-dest"
                    }
                }
            };

            var file = CreateJsonFile(metadataContent);
    
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _metadataBusiness.CreateMetadataFromFile(uid, pid, did, file));
            Assert.Contains("Records not found matching Original IDs", exception.Message);
    
            var eventList = await Context.Events.ToListAsync();
            Assert.Single(eventList);
    
            // Ensure we at least create relationship
            var actualEvent = eventList[0];
    
            Assert.Equal(pid, actualEvent.ProjectId);
            Assert.Equal("create", actualEvent.Operation);
            Assert.Equal("relationship", actualEvent.EntityType);
        }
        
        [Fact]
        public async Task CreateMetadataFromFile_Success_WithDeeplyNestedProperties()
        {
            // Arrange - Test very deep nesting
            var metadataContent = new
            {
                records = new[]
                {
                    new
                    {
                        name = "Complex System",
                        description = "Deeply nested test",
                        original_id = "test-1",
                        properties = new
                        {
                            level1 = new
                            {
                                level2 = new
                                {
                                    level3 = new
                                    {
                                        level4 = new
                                        {
                                            level5 = new
                                            {
                                                deep_value = "found",
                                                deep_number = 42,
                                                deep_array = new[] { 1, 2, 3, 4, 5 }
                                            }
                                        }
                                    }
                                }
                            },
                            metadata_version = "1.0.0"
                        }
                    }
                }
            };

            var file = CreateJsonFile(metadataContent);

            // Act
            var result = await _metadataBusiness.CreateMetadataFromFile(uid, pid, did, file);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Records);
            Assert.Equal("Complex System", result.Records.First().Name);
            Assert.NotNull(result.Records.First().Properties);
        }
        
        #endregion
        
        /// <summary>
        /// This is our file factory in essence within this test suite. We can feed it a C# object,
        /// and it will serialize it into a JSON file. We can then use this file
        /// as if it was submitted to our endpoint originally.
        /// </summary>
        /// <param name="content">Structured C# object to be serialized.</param>
        /// <param name="filename">Name of file produced (just required for FormFile instantiation).</param>
        /// <returns>A file instance with contents in the form of JSON.</returns>
        private static FormFile CreateJsonFile(object content, string filename = "metadata.json")
        {
            var json = JsonSerializer.Serialize(content, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true 
            });
    
            var bytes = Encoding.UTF8.GetBytes(json);
            var stream = new MemoryStream(bytes);
    
            var file = new FormFile(stream, 0, bytes.Length, "file", filename)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/json"
            };
    
            return file;
        }

        protected override async Task SeedTestDataAsync()
        {
            await base.SeedTestDataAsync();
            
            var organization = new Organization { Name = "Test Organization" };
            Context.Organizations.Add(organization);
            await Context.SaveChangesAsync();
            organizationId = organization.Id;
            
            // Create test project
            var project = new Project 
            { 
                Name = "Project 1", 
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                LastUpdatedBy = null,
                IsArchived = false,
                OrganizationId = organizationId,
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
            cid = originClass.Id;
            cid2 = destClass.Id;
            
            var testUser = new User
            {
                Name = "Test User",
                Email = "test.user@test.com",
                Password = "test_password",
                IsArchived = false
            };
            Context.Users.Add(testUser);
            await Context.SaveChangesAsync();
            uid = testUser.Id;
        }
    }
}