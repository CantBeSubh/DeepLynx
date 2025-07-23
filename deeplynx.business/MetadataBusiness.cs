using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using System.Text.Json;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System.Reflection;

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
        
        return await ParseMetadata(metadataRequestDto, dataSourceId, projectId);
    }

    public async Task<MetadataResponseDto> ParseMetadata(MetadataRequestDto dto, long dataSourceId, long projectId)
    {
        MetadataResponseDto metadataResponseDto = new MetadataResponseDto();

        if (dto.Classes != null)
        {
            List<ClassResponseDto> classResponseDtos = new List<ClassResponseDto>();
            List<string> erroredClassnames = new List<string>();
            JsonArray jsonArray = dto.Classes;
            string jsonString = jsonArray.ToString();
            List<ClassRequestDto> classes = JsonSerializer.Deserialize<List<ClassRequestDto>>(jsonString);

            (classResponseDtos,  erroredClassnames) = await ParseClassMetadata(classes, projectId);
            metadataResponseDto.Classes = classResponseDtos;
        }
        
        if (dto.Relationships != null)
        {
            List<RelationshipResponseDto> relationshipResponseDtos = new List<RelationshipResponseDto>();
            List<RelationshipRequestDto> erroredObjects = new List<RelationshipRequestDto>();
            JsonArray jsonArray = dto.Relationships;
            string jsonString = jsonArray.ToString();
            List<RelationshipRequestDto> relationships = JsonSerializer.Deserialize<List<RelationshipRequestDto>>(jsonString);

            (relationshipResponseDtos, erroredObjects) = await ParseRelationshipMetadata(relationships, projectId);
            metadataResponseDto.Relationships = relationshipResponseDtos;
        }
        
        if (dto.Records != null)
        {
            List<RecordResponseDto> recordResponseDtos = new List<RecordResponseDto>();
            List<RecordRequestDto> erroredObjects = new List<RecordRequestDto>();
            JsonArray jsonArray = dto.Records;
            string jsonString = jsonArray.ToString();
            List<RecordRequestDto> records = JsonSerializer.Deserialize<List<RecordRequestDto>>(jsonString);

            (recordResponseDtos, erroredObjects) = await ParseRecordMetadata(records, dataSourceId, projectId);
            metadataResponseDto.Records = recordResponseDtos;
        }
        
        if (dto.Edges != null)
        {
            List<EdgeResponseDto> edgeResponseDtos = new List<EdgeResponseDto>();
            List<EdgeRequestDto> erroredObjects = new List<EdgeRequestDto>();
            JsonArray jsonArray = dto.Edges;
            string jsonString = jsonArray.ToString();
            List<EdgeRequestDto> edges = JsonSerializer.Deserialize<List<EdgeRequestDto>>(jsonString);

            (edgeResponseDtos, erroredObjects) = await ParseEdgeMetadata(edges, dataSourceId, projectId);
            metadataResponseDto.Edges = edgeResponseDtos;
        }

        return metadataResponseDto;
    }
    
    /*******************************************
     * Individual per-business parse functions *
     *******************************************/
    
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
                ClassResponseDto result = await scopedClassBusiness.CreateClass(projectId, nexusClass);
                classResponseDtos.Add(result);
            }
            catch (Exception ex)
            {
                erroredClassnames.Add(nexusClass.Name);
            }
        }
        return (classResponseDtos, erroredClassnames);
    }  
    
    public async Task<(List<RelationshipResponseDto> classResponseDtos, List<RelationshipRequestDto> erroredObjects)> ParseRelationshipMetadata(List<RelationshipRequestDto> relationships, long projectId )
    {
        List<RelationshipResponseDto> relationshipResponseDtos = new List<RelationshipResponseDto>();
        List<RelationshipRequestDto> erroredObjects = new List<RelationshipRequestDto>();

        using var scope = _provider.CreateScope();
        var scopedRelationshipBusiness = scope.ServiceProvider.GetRequiredService<IRelationshipBusiness>();
        foreach (var relationship in relationships)
        {
            try
            {
                RelationshipResponseDto result = await scopedRelationshipBusiness.CreateRelationship(projectId, relationship);
                relationshipResponseDtos.Add(result);
            }
            catch (Exception ex)
            {
                erroredObjects.Add(relationship);
            }
        }
        return (relationshipResponseDtos, erroredObjects);
    }  
    
    public async Task<(List<RecordResponseDto> recordResponseDtos, List<RecordRequestDto> erroredObjects)> ParseRecordMetadata(List<RecordRequestDto> records, long dataSourceId, long projectId )
    {
        List<RecordResponseDto> recordResponseDtos = new List<RecordResponseDto>();
        List<RecordRequestDto> erroredObjects = new List<RecordRequestDto>();

        using var scope = _provider.CreateScope();
        var scopedRecordBusiness = scope.ServiceProvider.GetRequiredService<IRecordBusiness>();
        foreach (var record in records)
        {
            try
            {
                RecordResponseDto result = await scopedRecordBusiness.CreateRecord(projectId, dataSourceId, record);
                recordResponseDtos.Add(result);
            }
            catch (Exception ex)
            {
                erroredObjects.Add(record);
            }
        }
        return (recordResponseDtos, erroredObjects);
    }  
    
    public async Task<(List<EdgeResponseDto> edgeResponseDtos, List<EdgeRequestDto> erroredObjects)> ParseEdgeMetadata(List<EdgeRequestDto> edges, long dataSourceId, long projectId )
    {
        List<EdgeResponseDto> edgeResponseDtos = new List<EdgeResponseDto>();
        List<EdgeRequestDto> erroredObjects = new List<EdgeRequestDto>();

        using var scope = _provider.CreateScope();
        var scopedEdgeBusiness = scope.ServiceProvider.GetRequiredService<IEdgeBusiness>();
        foreach (var edge in edges)
        {
            try
            {
                EdgeResponseDto result = await scopedEdgeBusiness.CreateEdge(projectId, dataSourceId, edge);
                edgeResponseDtos.Add(result);
            }
            catch (Exception ex)
            {
                erroredObjects.Add(edge);
            }
        }
        return (edgeResponseDtos, erroredObjects);
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
