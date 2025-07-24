using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
using deeplynx.helpers.json;

namespace deeplynx.business;

public class MetadataBusiness : IMetadataBusiness
{
    private readonly DeeplynxContext _context;
    private readonly IClassBusiness _classBusiness;
    private readonly IRelationshipBusiness _relationshipBusiness;
    private readonly ITagBusiness _tagBusiness;
    private readonly IRecordBusiness _recordBusiness;
    private readonly IEdgeBusiness _edgeBusiness;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context to be used for CRUD operations.</param>
    /// <param name="classBusiness">The class context to be used during metadata parsing.</param>
    /// <param name="relationshipBusiness">The relationship context to be used during metadata parsing.</param>
    /// <param name="tagBusiness">The tag context to be used during metadata parsing.</param>
    /// <param name="recordBusiness">The record context to be used during metadata parsing.</param>
    /// <param name="edgeBusiness">The edge context to be used during metadata parsing.</param>
    public MetadataBusiness(
        DeeplynxContext context, 
        IClassBusiness classBusiness,
        IRelationshipBusiness relationshipBusiness,
        ITagBusiness tagBusiness,
        IRecordBusiness recordBusiness,
        IEdgeBusiness edgeBusiness
        )
    {
        _context = context;
        _classBusiness = classBusiness;
        _relationshipBusiness = relationshipBusiness;
        _tagBusiness = tagBusiness;
        _recordBusiness = recordBusiness;
        _edgeBusiness = edgeBusiness;
    }

    /// <summary>
    /// Call the parse and perform pre-processing and final returned data validation of all metadata.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the metadata belongs.</param>
    /// <param name="dataSourceId">The ID of the project data source to which some metadata belongs.</param>
    /// <param name="metadataRequestDto">The metadata request data transfer object containing metadata.</param>
    /// <returns>The created metadata response DTO with saved details.</returns>
    /// <exception cref="KeyNotFoundException">If project is not found.</exception>
    /// <exception cref="KeyNotFoundException">If data source is not found.</exception>
    public async Task<MetadataResponseDto> CreateMetadata(long projectId, long dataSourceId, MetadataRequestDto metadataRequestDto)
    {
        DoesProjectExist(projectId);
        DoesDataSourceExist(dataSourceId);
        
        if (metadataRequestDto == null)
            throw new ArgumentNullException(nameof(metadataRequestDto));
        
        return await ParseMetadata(metadataRequestDto, dataSourceId, projectId);
    }

    /// <summary>
    /// Individually call the bulk create functions of all metadata fields and append to return object.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the metadata belongs.</param>
    /// <param name="dataSourceId">The ID of the project data source to which the metadata belongs.</param>
    /// <param name="metadataRequestDto">The metadata request data transfer object containing metadata.</param>
    /// <returns>The created metadata response DTO with saved details.</returns>
    private async Task<MetadataResponseDto> ParseMetadata(MetadataRequestDto metadataRequestDto, long dataSourceId, long projectId)
    {
        MetadataResponseDto metadataResponseDto = new MetadataResponseDto();

        if (metadataRequestDto.Classes != null && metadataRequestDto.Classes.Any())
        {
            List<ClassRequestDto> classes = JsonSerialization.DeserializeJsonArray<ClassRequestDto>(metadataRequestDto.Classes);
            // List<ClassResponseDto> classResponseDtos = await _classBusiness.BulkCreateClass(projectId, classes); 
            // metadataResponseDto.Classes = classResponseDtos;
            // TODO: if we expect to bulk insert 10k+ rows at a time, implement chunking/batching or a binary COPY
            // TODO: print total x created
            int updated_classes = await _classBusiness.BulkCreateClass(projectId, classes);
        }
        
        if (metadataRequestDto.Relationships != null && metadataRequestDto.Relationships.Any())
        {
            List<RelationshipRequestDto> relationships = JsonSerialization.DeserializeJsonArray<RelationshipRequestDto>(metadataRequestDto.Relationships);
            List<RelationshipResponseDto> relationshipResponseDtos = await _relationshipBusiness.BulkCreateRelationships(projectId, relationships);
            metadataResponseDto.Relationships = relationshipResponseDtos;
        }
        
        if (metadataRequestDto.Records != null && metadataRequestDto.Records.Any())
        {
            List<RecordRequestDto> records = JsonSerialization.DeserializeJsonArray<RecordRequestDto>(metadataRequestDto.Records);
            List<RecordResponseDto> recordResponseDtos = await _recordBusiness.BulkCreateRecords(projectId, dataSourceId, records);
            metadataResponseDto.Records = recordResponseDtos;
        }
        
        if (metadataRequestDto.Edges != null && metadataRequestDto.Edges.Any())
        {
            List<EdgeRequestDto> edges = JsonSerialization.DeserializeJsonArray<EdgeRequestDto>(metadataRequestDto.Edges);
            List<EdgeResponseDto> edgeResponseDtos = await _edgeBusiness.BulkCreateEdges(projectId, dataSourceId, edges);
            metadataResponseDto.Edges = edgeResponseDtos;
        }

        return metadataResponseDto;
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
