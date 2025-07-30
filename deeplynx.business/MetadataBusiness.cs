using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
using deeplynx.helpers.json;
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
    
    // private async Task<MetadataResponseDto> ParseMetadata(
    //     MetadataRequestDto metadataRequestDto, 
    //     long dataSourceId, 
    //     long projectId)
    // {
    //     MetadataResponseDto metadataResponseDto = new MetadataResponseDto();
    //     
    //     // deserialize metadata subdomains
    //     List<ClassRequestDto> classes = JsonSerialization.Deserialize<ClassRequestDto>(metadataRequestDto.Classes);
    //     List<RelationshipRequestDto> relationships = JsonSerialization.Deserialize<RelationshipRequestDto>(metadataRequestDto.Relationships);
    //     List<TagRequestDto> tags = JsonSerialization.Deserialize<TagRequestDto>(metadataRequestDto.Tags);
    //     List<CreateRecordRequestDto> records = JsonSerialization.Deserialize<CreateRecordRequestDto>(metadataRequestDto.Records);
    //     List<EdgeRequestDto> edges = JsonSerialization.Deserialize<EdgeRequestDto>(metadataRequestDto.Edges);
    //     List<RecordTagLinkDto> recordTags = new List<RecordTagLinkDto>();
    //
    //     // iterate through classes and records to ensure we are processing all requested classes
    //     var classDict = classes.ToDictionary(c => c.Name);
    //     foreach (var record in records)
    //     {
    //         if (record.ClassName != null)
    //         {
    //             classDict.TryAdd(record.ClassName, new ClassRequestDto{Name = record.ClassName});
    //         }
    //     }
    //     var classesToInsert = classDict.Values.ToList();
    //     
    //     // iterate through relationships and edges to ensure we are processing all requested relationships
    //     var relDict = relationships.ToDictionary(r => r.Name);
    //     foreach (var edge in edges)
    //     {
    //         if (edge.RelationshipName != null)
    //         {
    //             relDict.TryAdd(edge.RelationshipName, new RelationshipRequestDto{Name = edge.RelationshipName});
    //         }
    //     }
    //     var relationshipsToInsert = relDict.Values.ToList();
    //     
    //     // iterate through tags and records to ensure we are processing all requested tags
    //     var tagDict = tags.ToDictionary(t => t.Name);
    //     foreach (var record in records)
    //     {
    //         if (record.Tags == null || !record.Tags.Any())
    //             continue;
    //
    //         foreach (var tag in record.Tags)
    //         {
    //             tagDict.TryAdd(tag, new TagRequestDto{Name = tag});
    //         }
    //     }
    //     var tagsToInsert = tagDict.Values.ToList();
    //     
    //     // bulk insert classes
    //     metadataResponseDto.Classes = await _classBusiness.BulkCreateClasses(projectId, classesToInsert);
    //     // map ID to the natural key Name for use in record inserts
    //     var classMap = metadataResponseDto.Classes
    //         .ToDictionary(c => c.Name, c => c.Id);
    //
    //     // bulk insert relationships
    //     metadataResponseDto.Relationships = await _relationshipBusiness.BulkCreateRelationships(projectId, relationshipsToInsert);
    //     // map ID to the natural key Name for use in edge inserts
    //     var relMap = metadataResponseDto.Relationships
    //         .ToDictionary(r => r.Name, r => r.Id);
    //
    //     // bulk insert tags
    //     metadataResponseDto.Tags = await _tagBusiness.BulkCreateTags(projectId, tagsToInsert);
    //     // map ID to the natural key Name for use in recordTags inserts
    //     var tagMap = metadataResponseDto.Tags
    //         .ToDictionary(t => t.Name, t => t);
    //     
    //     // update records before insert with the returned class ID that corresponds with class name
    //     if (classMap.Count > 0)
    //     {
    //         foreach (var record in records)
    //         {
    //             if (record.ClassName != null && classMap.TryGetValue(record.ClassName, out long classId))
    //             {
    //                 record.ClassId = classId;
    //             }
    //         }
    //     }
    //     
    //     // bulk insert records
    //     metadataResponseDto.Records = await _recordBusiness.BulkCreateRecords(projectId, dataSourceId, records);
    //     // map ID to the natural key original id for use in recordTags inserts
    //     var recordMap = metadataResponseDto.Records
    //         .ToDictionary(r => r.OriginalId, r => r.Id);
    //     
    //     // set recordTags before insert with IDs that correspond with tag name and record original id
    //     foreach (var record in records)
    //     {
    //         if (record.Tags == null || !record.Tags.Any() || !recordMap.TryGetValue(record.OriginalId, out var recordId))
    //             continue;
    //
    //         foreach (var tag in record.Tags)
    //         {
    //             if (tagMap.TryGetValue(tag, out var tagDto))
    //                 recordTags.Add(new RecordTagLinkDto
    //                 {
    //                     RecordId = recordId,
    //                     TagId = tagDto.Id
    //                 });
    //         }
    //     }
    //     
    //     // bulk insert recordTags
    //     await _recordBusiness.BulkAttachTags(recordTags);
    //     var recordTagDict = recordTags
    //         .GroupBy(t => t.RecordId)
    //         .ToDictionary(
    //             t => t.Key, 
    //             t => t.Select(rt => rt.TagId).ToList());
    //
    //     var tagsById = metadataResponseDto.Tags
    //         .ToDictionary(t => t.Id, t => t);
    //
    //     foreach (var record in metadataResponseDto.Records)
    //     {
    //         if (recordTagDict.TryGetValue(record.Id, out var recordTagIds))
    //         {
    //             record.Tags = new List<RecordTagDto>();
    //             foreach (var tag in recordTagIds)
    //             {
    //                 if (tagsById.TryGetValue(tag, out var tagDto))
    //                     record.Tags.Add(new RecordTagDto
    //                     {
    //                         Id = tagDto.Id,
    //                         Name = tagDto.Name
    //                     });
    //             }
    //         }
    //     }
    //     
    //     // update edges before insert with the IDs that correspond with relationship name and record original IDs
    //     if (relMap.Count > 0 || recordMap.Count > 0)
    //     {
    //         foreach (var edge in edges)
    //         {
    //             if (edge.RelationshipName != null && relMap.TryGetValue(edge.RelationshipName, out long relId))
    //             {
    //                 edge.RelationshipId = relId;
    //             }
    //             
    //             if (edge.OriginOid != null && recordMap.TryGetValue(edge.OriginOid, out long originId))
    //             {
    //                 edge.OriginId = originId;
    //             }
    //             
    //             if (edge.DestinationOid != null && recordMap.TryGetValue(edge.DestinationOid, out long destinationId))
    //             {
    //                 edge.DestinationId = destinationId;
    //             }
    //         }
    //     }
    //     
    //     // bulk insert edges
    //     metadataResponseDto.Edges = await _edgeBusiness.BulkCreateEdges(projectId, dataSourceId, edges);
    //     
    //     return metadataResponseDto;
    // }


    private async Task<MetadataResponseDto> ParseMetadata(
        MetadataRequestDto metadataRequestDto,
        long dataSourceId,
        long projectId)
    {
        var metadataResponseDto = new MetadataResponseDto();
        
        // deserialize metadata subdomains
        var classes = JsonSerialization.Deserialize<ClassRequestDto>(metadataRequestDto.Classes);
        var relationships = JsonSerialization.Deserialize<RelationshipRequestDto>(metadataRequestDto.Relationships);
        var tags = JsonSerialization.Deserialize<TagRequestDto>(metadataRequestDto.Tags);
        var records = JsonSerialization.Deserialize<CreateRecordRequestDto>(metadataRequestDto.Records);
        var edges = JsonSerialization.Deserialize<EdgeRequestDto>(metadataRequestDto.Edges);
        
        // check dependent objects for additional classes, relationships and tags to add before insert
        var classesToInsert = BuildClasses(classes, records);
        var relationshipsToInsert = BuildRelationships(relationships, edges);
        var tagsToInsert = BuildTags(tags, records);
        
        // bulk insert classes, relationships and tags
        var classMap = await BulkUpsertClasses(projectId, classesToInsert);
        var relMap = await BulkUpsertRelationships(projectId, relationshipsToInsert);
        var tagMap = await BulkUpsertTags(projectId, tagsToInsert);
        
        // load class IDs into records objects before insert
        UpdateRecordsWithIds(records, classMap);
        
        // insert records
        var recordMap = await BulkUpsertRecords(projectId, dataSourceId, records);
        
        // build and insert record tags
        var recordTags = BuildRecordTags(records, tagMap, recordMap);
        await _recordBusiness.BulkAttachTags(recordTags);
        AttachTagsToRecordDtos(metadataResponseDto, recordTags, tagMap);
        
        // load relationship, origin and destination IDs into classes before insert
        UpdateEdgesWithIds(edges, relMap, recordMap);
        
        // insert edges
        metadataResponseDto.Edges = await _edgeBusiness.BulkCreateEdges(projectId, dataSourceId, edges);
        return metadataResponseDto;
    }

    /// <summary>
    /// Add any classes found in the records objects to a list of classes waiting to be inserted
    /// </summary>
    /// <param name="classes"></param>
    /// <param name="records"></param>
    /// <returns>A list of classes to be inserted</returns>
    private List<ClassRequestDto> BuildClasses(
        List<ClassRequestDto> classes,
        List<CreateRecordRequestDto> records)
    {
        var dict = classes.ToDictionary(c => c.Name);
        foreach (var record in records)
        {
            if (!string.IsNullOrEmpty(record.ClassName))
                dict.TryAdd(record.ClassName, new ClassRequestDto{Name = record.ClassName});
        }
        return dict.Values.ToList();
    }
    
    /// <summary>
    /// Add any relationships found in the edges objects to a list of relationships waiting to be inserted
    /// </summary>
    /// <param name="relationships"></param>
    /// <param name="edges"></param>
    /// <returns>A list of relationships to be inserted</returns>
    private List<RelationshipRequestDto> BuildRelationships(
        List<RelationshipRequestDto> relationships,
        List<EdgeRequestDto> edges)
    {
        var dict = relationships.ToDictionary(r => r.Name);
        foreach (var edge in edges)
        {
            if (!string.IsNullOrEmpty(edge.RelationshipName))
                dict.TryAdd(edge.RelationshipName, new RelationshipRequestDto{Name = edge.RelationshipName});
        }
        return dict.Values.ToList();
    }
    
    /// <summary>
    /// Add any tags found in the records objects to a list of tags waiting to be inserted
    /// </summary>
    /// <param name="tags"></param>
    /// <param name="records"></param>
    /// <returns>A list of tags to be inserted</returns>
    private List<TagRequestDto> BuildTags(
        List<TagRequestDto> tags,
        List<CreateRecordRequestDto> records)
    {
        var dict = tags.ToDictionary(r => r.Name);
        foreach (var record in records)
        {
            if (record.Tags == null) continue;
            foreach (var tag in record.Tags)
                dict.TryAdd(tag, new TagRequestDto{Name = tag});
        }
        return dict.Values.ToList();
    }

    /// <summary>
    /// Creates a list of record-tag pairs to be inserted into the linking table
    /// </summary>
    /// <param name="records"></param>
    /// <param name="tagMap"></param>
    /// <param name="recordMap"></param>
    /// <returns>A list of record_id: tag_id to be inserted into the linking table</returns>
    private List<RecordTagLinkDto> BuildRecordTags(
        List<CreateRecordRequestDto> records,
        Dictionary<string, TagResponseDto> tagMap,
        Dictionary<string, long> recordMap)
    {
        return records
            .Where(r => r.Tags != null && recordMap.ContainsKey(r.OriginalId))
            .SelectMany(r => r.Tags
                .Where(tag => tagMap.ContainsKey(tag))
                .Select(tag => new RecordTagLinkDto
                {
                    RecordId = recordMap[r.OriginalId],
                    TagId = tagMap[tag].Id
                }))
            .ToList();
    }

    /// <summary>
    /// Bulk upserts classes and returns a mapping of class name to ID
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="classes"></param>
    /// <returns>A mapping of class name to class ID</returns>
    private async Task<Dictionary<string, long>> BulkUpsertClasses(
        long projectId,
        List<ClassRequestDto> classes)
    {
        var inserted = await _classBusiness.BulkCreateClasses(projectId, classes);
        return inserted.ToDictionary(c => c.Name, c => c.Id);
    }
    
    /// <summary>
    /// Bulk upserts relationships and returns a mapping of relationship name to ID
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="relationships"></param>
    /// <returns>A mapping of relationship name to relationship ID</returns>
    private async Task<Dictionary<string, long>> BulkUpsertRelationships(
        long projectId,
        List<RelationshipRequestDto> relationships)
    {
        var inserted = await _relationshipBusiness.BulkCreateRelationships(projectId, relationships);
        return inserted.ToDictionary(r => r.Name, r => r.Id);
    }
    
    /// <summary>
    /// Bulk upserts tags and returns a mapping of tag name to ID
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="tags"></param>
    /// <returns>A mapping of tag name to tag ID</returns>
    private async Task<Dictionary<string, TagResponseDto>> BulkUpsertTags(
        long projectId,
        List<TagRequestDto> tags)
    {
        var inserted = await _tagBusiness.BulkCreateTags(projectId, tags);
        return inserted.ToDictionary(t => t.Name, t => t);
    }
    
    /// <summary>
    /// Bulk upserts records and returns a mapping of record original ID to database ID
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="dataSourceId"></param>
    /// <param name="records"></param>
    /// <returns>A mapping of record name to record ID</returns>
    private async Task<Dictionary<string, long>> BulkUpsertRecords(
        long projectId,
        long dataSourceId,
        List<CreateRecordRequestDto> records)
    {
        var inserted = await _recordBusiness.BulkCreateRecords(projectId, dataSourceId, records);
        return inserted.ToDictionary(r => r.Name, r => r.Id);
    }

    /// <summary>
    /// Add the appropriate class IDs to any records with class names
    /// </summary>
    /// <param name="records"></param>
    /// <param name="classMap"></param>
    private void UpdateRecordsWithIds(
        List<CreateRecordRequestDto> records,
        Dictionary<string, long> classMap)
    {
        foreach (var record in records)
        {
            if (!string.IsNullOrEmpty(record.ClassName) 
                && classMap.TryGetValue(record.ClassName, out long classId))
            {
                record.ClassId = classId;
            }
                
        }
    }

    /// <summary>
    /// Add the appropriate relationship and record IDs to any edges with relationship names or record original IDs
    /// </summary>
    /// <param name="edges"></param>
    /// <param name="relMap"></param>
    /// <param name="recordMap"></param>
    private void UpdateEdgesWithIds(
        List<EdgeRequestDto> edges, 
        Dictionary<string, long> relMap,
        Dictionary<string, long> recordMap)
    {
        foreach (var edge in edges)
        {
            if (!string.IsNullOrEmpty(edge.RelationshipName)
                && recordMap.TryGetValue(edge.RelationshipName, out long relationshipId))
            {
                edge.RelationshipId = relationshipId;
            }
            
            if (!string.IsNullOrEmpty(edge.OriginOid)
                && recordMap.TryGetValue(edge.OriginOid, out long originId))
            {
                edge.OriginId = originId;
            }
                
            if (!string.IsNullOrEmpty(edge.DestinationOid)
                && recordMap.TryGetValue(edge.DestinationOid, out long destinationId))
            {
                edge.DestinationId = destinationId;
            }
        }
    }

    private void AttachTagsToRecordDtos(
        MetadataResponseDto metadataResponseDto,
        List<RecordTagLinkDto> recordTags,
        Dictionary<string, TagResponseDto> tagMap)
    {
        // create lookup dictionaries for quick reference
        var recordTagLookup = recordTags.ToLookup(rt => rt.RecordId, rt => rt.TagId);
        var tagsById = tagMap.Values.ToDictionary(t => t.Id, t => t);

        foreach (var record in metadataResponseDto.Records)
        {
            record.Tags = recordTagLookup[record.Id]
                .Select(tagId => new RecordTagDto
                {
                    Id = tagsById[tagId].Id,
                    Name = tagsById[tagId].Name
                })
                .ToList();
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
