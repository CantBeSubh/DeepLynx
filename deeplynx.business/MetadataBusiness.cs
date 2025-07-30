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
    /// <param name="dataSourceId">The ID of the data source to which the metadata belongs.</param>
    /// <param name="metadataRequestDto">The metadata request data transfer object containing metadata.</param>
    /// <returns>The created metadata response DTO with saved details.</returns>
    public async Task<MetadataResponseDto> CreateMetadata(
        long projectId,
        long dataSourceId,
        MetadataRequestDto metadataRequestDto)
    {
        DoesProjectExist(projectId);
        if (metadataRequestDto == null)
            throw new ArgumentNullException(nameof(metadataRequestDto));
        
        return await ParseMetadata(projectId, dataSourceId, metadataRequestDto);
    }

    /// <summary>
    /// Individually call the bulk create functions of all metadata fields and append to return object.
    /// </summary>
    /// <param name="projectId">The ID of the project to which the metadata belongs.</param>
    /// <param name="dataSourceId">The ID of the data source to which the metadata belongs.</param>
    /// <param name="metadataRequestDto">The metadata request data transfer object containing metadata.</param>
    /// <returns>The created metadata response DTO with saved details.</returns>
    private async Task<MetadataResponseDto> ParseMetadata(
        long projectId,
        long dataSourceId,
        MetadataRequestDto metadataRequestDto
        )
    {
        MetadataResponseDto metadataResponseDto = new MetadataResponseDto();

        if (metadataRequestDto.Classes != null && metadataRequestDto.Classes.Any())
        {
            List<ClassRequestDto> classes = JsonSerialization.Deserialize<ClassRequestDto>(metadataRequestDto.Classes);
            metadataResponseDto.Classes = await _classBusiness.BulkCreateClass(projectId, classes);
        }
        
        if (metadataRequestDto.Relationships != null && metadataRequestDto.Relationships.Any())
        {
            List<RelationshipRequestDto> relationships = JsonSerialization.Deserialize<RelationshipRequestDto>(metadataRequestDto.Relationships);
            metadataResponseDto.Relationships = await _relationshipBusiness.BulkCreateRelationships(projectId, relationships);
        }
        
        if (metadataRequestDto.Tags != null && metadataRequestDto.Tags.Any())
        {
            List<TagRequestDto> tags = JsonSerialization.Deserialize<TagRequestDto>(metadataRequestDto.Tags);
            metadataResponseDto.Tags = await _tagBusiness.BulkCreateTags(projectId, tags);
        }
        
        if (metadataRequestDto.Records != null && metadataRequestDto.Records.Any())
        {
            List<CreateRecordRequestDto> records = JsonSerialization.Deserialize<CreateRecordRequestDto>(metadataRequestDto.Records);
            metadataResponseDto.Records = await _recordBusiness.BulkCreateRecords(projectId, dataSourceId, records);
        }
        
        if (metadataRequestDto.Edges != null && metadataRequestDto.Edges.Any())
        {
            List<EdgeRequestDto> edges = JsonSerialization.Deserialize<EdgeRequestDto>(metadataRequestDto.Edges);
            metadataResponseDto.Edges = await _edgeBusiness.BulkCreateEdges(projectId, dataSourceId, edges);
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
}