using System.Text.Json.Nodes;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.business;

public class MetadataBusiness : IMetadataBusiness
{
    private readonly DeeplynxContext _context;
    private readonly IClassBusiness _classBusiness;
    private readonly IRelationshipBusiness _relationshipBusiness;
    private readonly ITagBusiness _tagBusiness;
    private readonly IRecordBusiness _recordBusiness;
    private readonly IEdgeBusiness _edgeBusiness;
    private readonly IEdgeMappingBusiness _edgeMappingBusiness;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context to be used for CRUD operations.</param>
    /// <param name="classBusiness">The class context to be used during metadata parsing.</param>
    /// <param name="relationshipBusiness">The relationship context to be used during metadata parsing.</param>
    /// <param name="tagBusiness">The tag context to be used during metadata parsing.</param>
    /// <param name="recordBusiness">The record context to be used during metadata parsing.</param>
    /// <param name="edgeBusiness">The edge context to be used during metadata parsing.</param>
    /// <param name="edgeMappingBusiness">The edge mapping context to be used during metadata parsing.</param>
    public MetadataBusiness(
        DeeplynxContext context, 
        IClassBusiness classBusiness,
        IRelationshipBusiness relationshipBusiness,
        ITagBusiness tagBusiness,
        IRecordBusiness recordBusiness,
        IEdgeBusiness edgeBusiness,
        IEdgeMappingBusiness edgeMappingBusiness
        )
    {
        _context = context;
        _classBusiness = classBusiness;
        _relationshipBusiness = relationshipBusiness;
        _tagBusiness = tagBusiness;
        _recordBusiness = recordBusiness;
        _edgeBusiness = edgeBusiness;
        _edgeMappingBusiness = edgeMappingBusiness;
    }

    /// <summary>
    /// Parse new metadata for a specified project.
    /// Note: Will error out with foreign key constraint violation if project is not found.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the metadata belongs.</param>
    /// <param name="metadataRequestDto">The metadata request data transfer object containing metadata.</param>
    /// <returns>The created metadata response DTO with saved details.</returns>
    public async Task<MetadataResponseDto> CreateMetadata(long projectId, long dataSourceId, MetadataRequestDto metadataRequestDto)
    {
        DoesProjectExist(projectId);
        DoesDataSourceExist(dataSourceId);
        if (metadataRequestDto == null)
            throw new ArgumentNullException(nameof(metadataRequestDto));

        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                var bulkClassRequestDto = await ParseMetadataForClasses(metadataRequestDto, projectId);

                BulkClassResponseDto? bulkClassResponseDto = null;
                if (bulkClassRequestDto != null)
                {
                    bulkClassResponseDto = await _classBusiness.BulkCreateClass(projectId, bulkClassRequestDto);
                }

                var bulkRelationshipRequestDto = await ParseMetadataForRelationships(metadataRequestDto, projectId);
                BulkRelationshipResponseDto? bulkRelationshipResponseDto = null;
                if (bulkRelationshipRequestDto != null)
                {
                    bulkRelationshipResponseDto =
                        await _relationshipBusiness.BulkCreateRelationships(projectId, bulkRelationshipRequestDto);
                }

                var bulkTagRequestDto = await ParseMetadataForTags(metadataRequestDto, projectId);
                BulkTagResponseDto? bulkTagResponseDto = null;
                if (bulkTagRequestDto != null)
                {
                    bulkTagResponseDto = await _tagBusiness.BulkCreateTags(projectId, bulkTagRequestDto);
                }


                var bulkRecordRequestDto = await ParseMetadataForRecords(metadataRequestDto, projectId);
                BulkRecordResponseDto? bulkRecordResponseDto = null;
                if (bulkRecordRequestDto != null)
                {
                    bulkRecordResponseDto =
                        await _recordBusiness.BulkCreateRecords(projectId, dataSourceId, bulkRecordRequestDto);
                }

                var bulkEdgeRequestDto = await ParseMetadataForEdges(metadataRequestDto, projectId);
                BulkEdgeResponseDto? bulkEdgeResponseDto = null;
                if (bulkEdgeRequestDto != null)
                {
                    bulkEdgeResponseDto =
                        await _edgeBusiness.BulkCreateEdges(projectId, dataSourceId, bulkEdgeRequestDto);
                }

                return new MetadataResponseDto() // Return validated response DTO back to user.
                {
                    Id = metadataRequestDto.Id,
                    ProjectId = projectId,
                    DataSourceId = dataSourceId,
                    Classes = bulkClassResponseDto,
                    Relationships = bulkRelationshipResponseDto,
                    Tags = bulkTagResponseDto,
                    Records = bulkRecordResponseDto,
                    Edges = bulkEdgeResponseDto,
                    CreatedBy = metadataRequestDto.CreatedBy,
                    CreatedAt = metadataRequestDto.CreatedAt
                };

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw ex;
            }
        }
    }
    
    /// <summary>
    /// Determine if project exists
    /// </summary>
    /// <param name="projectId">The ID of the project we are searching for</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived projects from the result (Default true)</param>
    /// <returns>Throws error if project does not exist</returns>
    private void DoesProjectExist(long projectId, bool hideArchived = true)
    {
        var project = hideArchived ? _context.Projects.Any(p => p.Id == projectId && p.ArchivedAt == null) 
            : _context.Projects.Any(p => p.Id == projectId);
        if (!project)
        {
            throw new KeyNotFoundException($"Project with id {projectId} not found");
        }
    }
    
    private void DoesDataSourceExist(long datasourceId, bool hideArchived = true)
    {
        var datasource = hideArchived ? _context.DataSources.Any(p => p.Id == datasourceId && p.ArchivedAt == null)
            : _context.DataSources.Any(p => p.Id == datasourceId);
        if (!datasource)
        {
            throw new KeyNotFoundException($"Datasource with id {datasourceId} not found");
        }
    }
    
    public async Task<BulkMetadataRequestDto> ParseMetadata(MetadataRequestDto dto, long projectId)
    {
        BulkClassRequestDto? classRequestDtos = null;
        if (dto.Classes != null && dto.Classes.Any())
        {
            classRequestDtos = new BulkClassRequestDto
            {
                BulkClassRequests = new List<ClassRequestDto>()
            };
            
            foreach (var metaClass in dto.Classes)
            {
                if (metaClass is not JsonObject obj)
                    throw new InvalidOperationException("Metadata request is not structured for classes so that Dto can correctly parse it");


                if (!obj.TryGetPropertyValue("name", out JsonNode? nameNode) ||
                    nameNode is not JsonValue nameValue ||
                    string.IsNullOrWhiteSpace(nameValue.ToString()))
                {
                    throw new InvalidOperationException("Name is missing or empty for a class");
                }
                
                var className = nameValue.ToString();
                
                var existingClass = await _context.Classes.FirstOrDefaultAsync(c => c.ProjectId == projectId && c.Name == className);
                if (existingClass == null)
                {
                     String? description = null;
                     if (obj.TryGetPropertyValue("description", out JsonNode? descNode) &&
                         descNode is JsonValue descValue &&
                         !string.IsNullOrWhiteSpace(descValue.ToString()))
                     {
                         description = descValue.ToString();
                     }
                     classRequestDtos.BulkClassRequests.Add(new ClassRequestDto
                     {
                         Name = className,
                         Description = description
                     });

                }
            }
        }
        BulkRelationshipRequestDto? relationshipRequestDtos = null;
        if (dto.Relationships != null && dto.Relationships.Any())
        {
            relationshipRequestDtos = new BulkRelationshipRequestDto
            {
                Relationships = new List<RelationshipRequestDto>()
            };

            foreach (var metaRelationship in dto.Relationships)
            {
                if (metaRelationship is not JsonObject obj)
                    throw new InvalidOperationException("Metadata request is not structured for relationships so that Dto can correctly parse it");
                
                if (!obj.TryGetPropertyValue("name", out JsonNode? nameNode) ||
                    nameNode is not JsonValue nameValue ||
                    string.IsNullOrWhiteSpace(nameValue.ToString()))
                {
                    throw new InvalidOperationException("Name is missing or empty for a relationship");
                }
                var relationshipName = nameValue.ToString();
                var existingRelationship = await _context.Classes.FirstOrDefaultAsync(r => r.ProjectId == projectId && r.Name == relationshipName);
                if (existingRelationship == null)
                {
                    String? description = null;
                    if (obj.TryGetPropertyValue("description", out JsonNode? descNode) &&
                        descNode is JsonValue descValue &&
                        !string.IsNullOrWhiteSpace(descValue.ToString()))
                    {
                        description = descValue.ToString();
                    }
                    
                    relationshipRequestDtos.Relationships.Add(new RelationshipRequestDto
                    {
                        Name = relationshipName,
                        Description = description
                    });
                }
            }
        }
        
        BulkTagRequestDto? tagRequestDtos = null;
        if (dto.Tags != null && dto.Tags.Any())
        {
            tagRequestDtos = new BulkTagRequestDto
            {
                Tags = new List<TagRequestDto>()
            };

            foreach (var metaTag in dto.Tags)
            {
                if (metaTag is not JsonObject obj)
                    throw new InvalidOperationException("Metadata request is not structured for tags so that Dto can correctly parse it");
                
                if (IsStringFieldNullOrEmpty(obj, "name"))
                {
                    throw new InvalidOperationException("Name is missing or empty for a tag");
                }

                obj.TryGetPropertyValue("name", out JsonNode? nameNodes);
                var tagName = nameNodes.AsValue().ToString();
                var existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Name == tagName);
                if (existingTag == null)
                {
                    tagRequestDtos.Tags.Add(new TagRequestDto
                    {
                        Name = tagName
                    });
                }
            }
        }
        
        BulkRecordRequestDto? recordRequestDtos = null;
        if (dto.Records != null && dto.Records.Any())
        {
            recordRequestDtos = new BulkRecordRequestDto
            {
                Records = new List<RecordRequestDto>()
            };

            foreach (var metaRecord in dto.Records)
            {
                if (metaRecord is not JsonObject obj)
                    throw new InvalidOperationException("Metadata request is not structured for edges so that Dto can correctly parse it");

                JsonObject properties;
                if (IsJsonObjectFieldNullOrInvalid(obj, "properties"))
                    throw new InvalidOperationException("Properties is missing or invalid for a record");
                
                obj.TryGetPropertyValue("properties", out JsonNode? propertiesNode);
                properties = propertiesNode.AsObject();

                string? name = null;
                if (!IsStringFieldNullOrEmpty(obj, "name"))
                {
                    obj.TryGetPropertyValue("name", out JsonNode? nameNode);
                    name = nameNode.AsValue().ToString();
                }
                
                string? className = null;
                long? classId = null;
                if (!IsStringFieldNullOrEmpty(obj, "class_name"))
                {
                    obj.TryGetPropertyValue("class_name", out JsonNode? classNameNode);
                    var tempName = classNameNode.AsValue().ToString();
                    var classExists = await _context.Classes.FirstOrDefaultAsync(c => c.ProjectId == projectId && c.Name == tempName);
                    if (classExists == null)
                    {
                        var classRequestDto = new ClassRequestDto
                        {
                            Name = tempName,
                        };
                        var newClass = await _classBusiness.CreateClass(projectId, classRequestDto);
                        classId = newClass.Id;
                        className = newClass.Name;
                    }
                    else
                    {
                        classId = classExists.Id;
                        className = classExists.Name;
                    }
                }
                
                string? originalId = null;
                if (!IsStringFieldNullOrEmpty(obj, "original_id"))
                {
                    obj.TryGetPropertyValue("original_id", out JsonNode? originalIdNode);
                    originalId = originalIdNode.AsValue().ToString();
                }
                    
                //Todo: Add logic for tags in a record. Need to include in RecordRequestDto
                //Need to make Records have a unique name within a project
                
                recordRequestDtos.Records.Add(new RecordRequestDto
                {
                    Properties = properties,
                    Name = name,
                    ClassId = classId,
                    ClassName = className,
                    OriginalId = originalId
                });
            }
        }
        //Todo: Change logic in this function so that each entity is saved after 
        //it is parsed. We will need to implement a transaction.
        BulkEdgeRequestDto? edgeRequestDtos = null;
        if (dto.Edges != null && dto.Edges.Any())
        {
            edgeRequestDtos = new BulkEdgeRequestDto
            {
                Edges = new List<EdgeRequestDto>()
            };

            foreach (var metaEdge in dto.Edges)
            {
                if (metaEdge is not JsonObject obj)
                    throw new InvalidOperationException("Metadata request is not structured for edges so that Dto can correctly parse it");

                if (IsStringFieldNullOrEmpty(obj, "origin_name") || IsStringFieldNullOrEmpty(obj, "destination_name"))
                {
                    throw new InvalidOperationException("Origin and/or destination name is missing or empty for an edge");
                }
                obj.TryGetPropertyValue("origin_name", out JsonNode? originNameNode);
                obj.TryGetPropertyValue("destination_name", out JsonNode? destinationNameNode);
                var originName = originNameNode.AsValue().ToString();
                var destinationName = destinationNameNode.AsValue().ToString();
                var originExists = await _context.Records.FirstOrDefaultAsync(r => r.ProjectId == projectId && r.Name == originName);
                if (originExists == null)
                    throw new KeyNotFoundException($"Edge references an origin record with name {originName} that does not exist in project {projectId} ");
                
                var destinationExists = await _context.Records.FirstOrDefaultAsync(r => r.ProjectId == projectId && r.Name == destinationName);
                if (destinationExists == null)
                    throw new KeyNotFoundException($"Edge references a destination record with name {destinationName} that does not exist in project {projectId}");
                
                String? relationshipName = null;
                long? relationshipId = null;
                if (!IsStringFieldNullOrEmpty(obj, "relationship_name"))
                {
                    obj.TryGetPropertyValue("relationship_name", out JsonNode? descriptionNode);
                    relationshipName = descriptionNode.AsValue().ToString();

                    var relationshipExists = await _context.Relationships.FirstOrDefaultAsync(r =>
                        r.ProjectId == projectId && r.Name == relationshipName);

                    if (relationshipExists != null)
                    {
                        relationshipId = relationshipExists.Id;
                    }
                    else
                    {
                        var relationshipRequestDto = new RelationshipRequestDto()
                        {
                            Name = relationshipName
                        };
                        var relationshipResponse = await _relationshipBusiness.CreateRelationship(projectId, relationshipRequestDto);
                        relationshipId = relationshipResponse.Id;
                    }
                }
                
                edgeRequestDtos.Edges.Add(new  EdgeRequestDto
                {
                    DestinationId = destinationExists.Id,
                    OriginId = originExists.Id,
                    RelationshipId = relationshipId,
                    RelationshipName = relationshipName
                });
            }
        }

        return new BulkMetadataRequestDto
        {
            Classes = classRequestDtos,
            Relationships = relationshipRequestDtos,
            Tags = tagRequestDtos,
            Edges = edgeRequestDtos,
            Records = recordRequestDtos
        };
    }
    
    
    private bool IsStringFieldNullOrEmpty(JsonObject json, string propertyName)
    {
        return !json.TryGetPropertyValue(propertyName, out JsonNode? node) ||
               node is not JsonValue value ||
               string.IsNullOrWhiteSpace(value.ToString());
    }
    
    
    private bool IsLongFieldNullOrInvalid(JsonObject json, string propertyName)
    {
        return !json.TryGetPropertyValue(propertyName, out JsonNode? node) ||
               node is not JsonValue value ||
               !long.TryParse(value.ToString(), out _);
    }

    
    private bool IsJsonObjectFieldNullOrInvalid(JsonObject json, string propertyName)
    {
        return !json.TryGetPropertyValue(propertyName, out JsonNode? node) ||
               node is not JsonObject;
    }

    private async Task<BulkClassRequestDto?> ParseMetadataForClasses(MetadataRequestDto dto, long projectId)
    {
        if (dto.Classes == null || !dto.Classes.Any())
            return null;
        
        var classRequestDtos = new BulkClassRequestDto
        {
            BulkClassRequests = new List<ClassRequestDto>()
        };
        
        foreach (var metaClass in dto.Classes)
        {
            if (metaClass is not JsonObject obj)
                throw new InvalidOperationException("Metadata request is not structured for classes so that Dto can correctly parse it");


            if (!obj.TryGetPropertyValue("name", out JsonNode? nameNode) ||
                nameNode is not JsonValue nameValue ||
                string.IsNullOrWhiteSpace(nameValue.ToString()))
            {
                throw new InvalidOperationException("Name is missing or empty for a class");
            }
            
            var className = nameValue.ToString();
            
            var existingClass = await _context.Classes.FirstOrDefaultAsync(c => c.ProjectId == projectId && c.Name == className);
            if (existingClass == null)
            {
                String? description = null;
                if (obj.TryGetPropertyValue("description", out JsonNode? descNode) &&
                    descNode is JsonValue descValue &&
                    !string.IsNullOrWhiteSpace(descValue.ToString()))
                {
                    description = descValue.ToString();
                }
                classRequestDtos.BulkClassRequests.Add(new ClassRequestDto
                {
                    Name = className,
                    Description = description
                });
            }
        }
        return classRequestDtos;
    }

    private async Task<BulkRelationshipRequestDto?> ParseMetadataForRelationships(MetadataRequestDto dto,
        long projectId)
    {
        if (dto.Relationships == null || !dto.Relationships.Any()) 
            return null;
        
        var relationshipRequestDtos = new BulkRelationshipRequestDto
        {
            Relationships = new List<RelationshipRequestDto>()
        };

        foreach (var metaRelationship in dto.Relationships)
        {
            if (metaRelationship is not JsonObject obj)
                throw new InvalidOperationException("Metadata request is not structured for relationships so that Dto can correctly parse it");
            
            if (!obj.TryGetPropertyValue("name", out JsonNode? nameNode) ||
                nameNode is not JsonValue nameValue ||
                string.IsNullOrWhiteSpace(nameValue.ToString()))
            {
                throw new InvalidOperationException("Name is missing or empty for a relationship");
            }
            var relationshipName = nameValue.ToString();
            var existingRelationship = await _context.Classes.FirstOrDefaultAsync(r => r.ProjectId == projectId && r.Name == relationshipName);
            if (existingRelationship == null)
            {
                String? description = null;
                if (obj.TryGetPropertyValue("description", out JsonNode? descNode) &&
                    descNode is JsonValue descValue &&
                    !string.IsNullOrWhiteSpace(descValue.ToString()))
                {
                    description = descValue.ToString();
                }
                
                relationshipRequestDtos.Relationships.Add(new RelationshipRequestDto
                {
                    Name = relationshipName,
                    Description = description
                });
            }
        }
        
        return relationshipRequestDtos;
    }

    private async Task<BulkTagRequestDto?> ParseMetadataForTags(MetadataRequestDto dto,
        long projectId)
    {
        if (dto.Tags == null || !dto.Tags.Any())
            return null;
        
        var tagRequestDtos = new BulkTagRequestDto
        {
            Tags = new List<TagRequestDto>()
        };

        foreach (var metaTag in dto.Tags)
        {
            if (metaTag is not JsonObject obj)
                throw new InvalidOperationException("Metadata request is not structured for tags so that Dto can correctly parse it");
            
            if (IsStringFieldNullOrEmpty(obj, "name"))
            {
                throw new InvalidOperationException("Name is missing or empty for a tag");
            }

            obj.TryGetPropertyValue("name", out JsonNode? nameNodes);
            var tagName = nameNodes.AsValue().ToString();
            var existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Name == tagName);
            if (existingTag == null)
            {
                tagRequestDtos.Tags.Add(new TagRequestDto
                {
                    Name = tagName
                });
            }
        }
        return tagRequestDtos;
    }

    private async Task<BulkRecordRequestDto?> ParseMetadataForRecords(MetadataRequestDto dto,
        long projectId)
    {
        if (dto.Records == null || !dto.Records.Any()) 
            return null;
        
        var recordRequestDtos = new BulkRecordRequestDto
        {
            Records = new List<RecordRequestDto>()
        };

        foreach (var metaRecord in dto.Records)
        {
            if (metaRecord is not JsonObject obj)
                throw new InvalidOperationException("Metadata request is not structured for edges so that Dto can correctly parse it");

            JsonObject properties;
            if (IsJsonObjectFieldNullOrInvalid(obj, "properties"))
                throw new InvalidOperationException("Properties is missing or invalid for a record");
            
            obj.TryGetPropertyValue("properties", out JsonNode? propertiesNode);
            properties = propertiesNode.AsObject();

            string? name = null;
            if (!IsStringFieldNullOrEmpty(obj, "name"))
            {
                obj.TryGetPropertyValue("name", out JsonNode? nameNode);
                name = nameNode.AsValue().ToString();
            }
            
            string? className = null;
            long? classId = null;
            if (!IsStringFieldNullOrEmpty(obj, "class_name"))
            {
                obj.TryGetPropertyValue("class_name", out JsonNode? classNameNode);
                var tempName = classNameNode.AsValue().ToString();
                var classExists = await _context.Classes.FirstOrDefaultAsync(c => c.ProjectId == projectId && c.Name == tempName);
                if (classExists == null)
                {
                    var classRequestDto = new ClassRequestDto
                    {
                        Name = tempName,
                    };
                    var newClass = await _classBusiness.CreateClass(projectId, classRequestDto);
                    classId = newClass.Id;
                    className = newClass.Name;
                }
                else
                {
                    classId = classExists.Id;
                    className = classExists.Name;
                }
            }
            
            string? originalId = null;
            if (!IsStringFieldNullOrEmpty(obj, "original_id"))
            {
                obj.TryGetPropertyValue("original_id", out JsonNode? originalIdNode);
                originalId = originalIdNode.AsValue().ToString();
            }
                
            //Todo: Add logic for tags in a record. Need to include in RecordRequestDto
            //Need to make Records have a unique name within a project
            
            recordRequestDtos.Records.Add(new RecordRequestDto
            {
                Properties = properties,
                Name = name,
                ClassId = classId,
                ClassName = className,
                OriginalId = originalId
            });
        }
        
        return recordRequestDtos;
    }

    public async Task<BulkEdgeRequestDto?> ParseMetadataForEdges(MetadataRequestDto dto,
        long projectId)
    {
        if (dto.Edges == null || !dto.Edges.Any())
            return null;
        
        var edgeRequestDtos = new BulkEdgeRequestDto
        {
            Edges = new List<EdgeRequestDto>()
        };

        foreach (var metaEdge in dto.Edges)
        {
            if (metaEdge is not JsonObject obj)
                throw new InvalidOperationException(
                    "Metadata request is not structured for edges so that Dto can correctly parse it");

            if (IsStringFieldNullOrEmpty(obj, "origin_name") || IsStringFieldNullOrEmpty(obj, "destination_name"))
            {
                throw new InvalidOperationException(
                    "Origin and/or destination name is missing or empty for an edge");
            }

            obj.TryGetPropertyValue("origin_name", out JsonNode? originNameNode);
            obj.TryGetPropertyValue("destination_name", out JsonNode? destinationNameNode);
            var originName = originNameNode.AsValue().ToString();
            var destinationName = destinationNameNode.AsValue().ToString();
            var originExists =
                await _context.Records.FirstOrDefaultAsync(r => r.ProjectId == projectId && r.Name == originName);
            if (originExists == null)
                throw new KeyNotFoundException(
                    $"Edge references an origin record with name {originName} that does not exist in project {projectId} ");

            var destinationExists =
                await _context.Records.FirstOrDefaultAsync(r =>
                    r.ProjectId == projectId && r.Name == destinationName);
            if (destinationExists == null)
                throw new KeyNotFoundException(
                    $"Edge references a destination record with name {destinationName} that does not exist in project {projectId}");

            String? relationshipName = null;
            long? relationshipId = null;
            if (!IsStringFieldNullOrEmpty(obj, "relationship_name"))
            {
                obj.TryGetPropertyValue("relationship_name", out JsonNode? descriptionNode);
                relationshipName = descriptionNode.AsValue().ToString();

                var relationshipExists = await _context.Relationships.FirstOrDefaultAsync(r =>
                    r.ProjectId == projectId && r.Name == relationshipName);

                if (relationshipExists != null)
                {
                    relationshipId = relationshipExists.Id;
                }
                else
                {
                    var relationshipRequestDto = new RelationshipRequestDto()
                    {
                        Name = relationshipName
                    };
                    var relationshipResponse =
                        await _relationshipBusiness.CreateRelationship(projectId, relationshipRequestDto);
                    relationshipId = relationshipResponse.Id;
                }
            }

            edgeRequestDtos.Edges.Add(new EdgeRequestDto
            {
                DestinationId = destinationExists.Id,
                OriginId = originExists.Id,
                RelationshipId = relationshipId,
                RelationshipName = relationshipName
            });
        }
    
        return edgeRequestDtos;
    }
    
    

}