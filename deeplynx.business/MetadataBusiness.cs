using System.Text.Json.Nodes;
using System.Text.Json;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.business;

public class MetadataBusiness : IMetadataBusiness
{
    private readonly DeeplynxContext _context;
    private readonly IClassBusiness _classBusiness;
    private readonly IRelationshipBusiness _relationshipBusiness;
    private readonly IRecordBusiness _recordBusiness;
    private readonly IEdgeBusiness _edgeBusiness;
    private readonly ITagBusiness _tagBusiness;
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
    /// Call the parse and perform pre-processing and final data return validation of all metadata
    /// Note: Will error out with foreign key constraint violation if project or data source is not found.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the metadata belongs.</param>
    /// <param name="dataSourceId">The ID of the project data source to which some metadata belongs.</param>
    /// <param name="metadataRequestDto">The metadata request data transfer object containing metadata.</param>
    /// <returns>The created metadata response DTO with saved details.</returns>
    public async Task<MetadataResponseDto> CreateMetadata(long projectId, long dataSourceId, MetadataRequestDto metadataRequestDto)
    {
        DoesProjectExist(projectId);
        DoesDataSourceExist(dataSourceId);
        
        if (metadataRequestDto == null)
            throw new ArgumentNullException(nameof(metadataRequestDto));
        
        return await ParseMetadata(metadataRequestDto, dataSourceId, projectId);
    }

    /// <summary>
    /// Individually call the bulk create functions of all metadata fields
    /// Note: Will error out with foreign key constraint violation if project or data source is not found.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the metadata belongs.</param>
    /// <param name="dataSourceId">The ID of the project data source to which the metadata belongs.</param>
    /// <param name="metadataRequestDto">The metadata request data transfer object containing metadata.</param>
    /// <returns>The created metadata response DTO with saved details.</returns>
    public async Task<MetadataResponseDto> ParseMetadata(MetadataRequestDto metadataRequestDto, long dataSourceId, long projectId)
    {
        MetadataResponseDto metadataResponseDto = new MetadataResponseDto();

        if (metadataRequestDto.Classes != null && metadataRequestDto.Classes.Any())
        {
            List<ClassRequestDto> classes = DeserializeJsonArray<ClassRequestDto>(metadataRequestDto.Classes);
            List<ClassResponseDto> classResponseDtos = await _classBusiness.BulkCreateClass(projectId, classes); 
            metadataResponseDto.Classes = classResponseDtos;
        }
        
        if (metadataRequestDto.Relationships != null && metadataRequestDto.Relationships.Any())
        {
            List<RelationshipRequestDto> relationships = DeserializeJsonArray<RelationshipRequestDto>(metadataRequestDto.Relationships);
            List<RelationshipResponseDto> relationshipResponseDtos = await _relationshipBusiness.BulkCreateRelationships(projectId, relationships);
            metadataResponseDto.Relationships = relationshipResponseDtos;
        }
        
        if (metadataRequestDto.Records != null && metadataRequestDto.Records.Any())
        {
            List<RecordRequestDto> records = DeserializeJsonArray<RecordRequestDto>(metadataRequestDto.Records);
            List<RecordResponseDto> recordResponseDtos = await _recordBusiness.BulkCreateRecords(projectId, dataSourceId, records);
            metadataResponseDto.Records = recordResponseDtos;
        }
        
        if (metadataRequestDto.Edges != null && metadataRequestDto.Edges.Any())
        {
            List<EdgeRequestDto> edges = DeserializeJsonArray<EdgeRequestDto>(metadataRequestDto.Edges);
            List<EdgeResponseDto> edgeResponseDtos = await _edgeBusiness.BulkCreateEdges(projectId, dataSourceId,  edges);
            metadataResponseDto.Edges = edgeResponseDtos;
        }

        return metadataResponseDto;
    }

    /// <summary>
    /// Deserialize input json into list of generic object type
    /// </summary>
    /// <param name="jsonArray">The input json to be serialized to an object</param>
    /// <returns>List of serialized objects parsed from json</returns>
    /// <note>Due to possible null reference return, returns an empty generic list on failure.</note>
    public List<T> DeserializeJsonArray<T>(JsonArray jsonArray)
    {
        string jsonString = jsonArray.ToString();
        var result = JsonSerializer.Deserialize<List<T>>(jsonString);
        return result ?? new List<T>();
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
    
    /// <summary>
    /// Determine if datasource exists
    /// </summary>
    /// <param name="datasourceId">The ID of the datasource we are searching for</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived projects from the result (Default true)</param>
    /// <returns>Throws error if datasource does not exist</returns>
    private void DoesDataSourceExist(long datasourceId, bool hideArchived = true)
    {
        var datasource = hideArchived ? _context.DataSources.Any(p => p.Id == datasourceId && p.ArchivedAt == null)
            : _context.DataSources.Any(p => p.Id == datasourceId);
        if (!datasource)
        {
            throw new KeyNotFoundException($"Datasource with id {datasourceId} not found");
        }
    }
}
