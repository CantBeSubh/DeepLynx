using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using System.Text.Json;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.Extensions.DependencyInjection;

namespace deeplynx.business;

public class MetadataBusiness : IMetadataBusiness
{
    private readonly DeeplynxContext _context;
    private readonly IClassBusiness _classBusiness;
    private readonly IServiceProvider _provider;
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
        IServiceProvider provider,
        IClassBusiness classBusiness,
        IRelationshipBusiness relationshipBusiness,
        ITagBusiness tagBusiness,
        IRecordBusiness recordBusiness,
        IEdgeBusiness edgeBusiness,
        IEdgeMappingBusiness edgeMappingBusiness
        )
    {
        _context = context;
        _provider = provider;
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
        //TODO: validate dataSourceID
        if (metadataRequestDto == null)
            throw new ArgumentNullException(nameof(metadataRequestDto));
        
        ParseMetadata(metadataRequestDto, projectId);
        
        return new MetadataResponseDto() // Return validated response DTO back to user.
        {
            Id = metadataRequestDto.Id,
            ProjectId = projectId,
            /*Classes = metadataRequestDto.Classes,
            Relationships = metadataRequestDto.Relationships,
            Tags = metadataRequestDto.Tags,
            Records = metadataRequestDto.Records,
            Edges = metadataRequestDto.Edges,
            */
            CreatedBy = metadataRequestDto.CreatedBy,
            CreatedAt = metadataRequestDto.CreatedAt
        };
    }

    public async Task<List<ClassResponseDto>> ParseMetadata(MetadataRequestDto dto, long projectId)
    {
        MetadataResponseDto metadataResponseDto = new MetadataResponseDto();
        List<ClassResponseDto> classResponseDtos = new List<ClassResponseDto>();
        List<string> erroredClassnames = new List<string>();
        if (dto.Classes != null)
        {
            JsonArray jsonArray = dto.Classes;
            string jsonString = jsonArray.ToString();
            List<ClassRequestDto> classes = JsonSerializer.Deserialize<List<ClassRequestDto>>(jsonString);

            (classResponseDtos,  erroredClassnames) = await ParseClassMetadata(classes, projectId);
        }
        
        //metadataResponseDto.Classes = classResponseDtos;

        return classResponseDtos;

        /*
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
            //Classes = classRequestDtos,
            Relationships = relationshipRequestDtos,
            Tags = tagRequestDtos,
            Edges = edgeRequestDtos
        };
        */
        //return metadataResponseDto;
    }
    
    public async Task<(List<ClassResponseDto> classResponseDtos, List<string> erroredClassnames)> ParseClassMetadata(List<ClassRequestDto> classes, long projectId )
    {
        List<ClassResponseDto> classResponseDtos = new List<ClassResponseDto>();
        List<string> erroredClassnames = new List<string>();

        using var scope = _provider.CreateScope();
        var scopedClassBusiness = scope.ServiceProvider.GetRequiredService<IClassBusiness>();
        foreach (var nexusClass in classes)
        {
            if (string.IsNullOrWhiteSpace(nexusClass.Name))
                throw new ValidationException("Name is missing or empty for a class"); 
            
            try
            {
                Console.WriteLine($"Processing class {nexusClass.Name}");
                ClassResponseDto result = await scopedClassBusiness.CreateClass(projectId, nexusClass);
                classResponseDtos.Add(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                erroredClassnames.Add(nexusClass.Name);
            }
        }
        return (classResponseDtos, erroredClassnames);
    }  
    //TODO: parse function for every business layer to be called in ParseMetadata
    
    private bool IsStringFieldNullOrEmpty(JsonObject json, string propertyName)
    {
        return !json.TryGetPropertyValue(propertyName, out JsonNode? node) ||
               node is not JsonValue value ||
               string.IsNullOrWhiteSpace(value.ToString());
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
}
