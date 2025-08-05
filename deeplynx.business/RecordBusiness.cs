using System.Text.Json.Nodes;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.helpers.exceptions;
using deeplynx.helpers.json;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace deeplynx.business;

public class RecordBusiness : IRecordBusiness
{
    private readonly DeeplynxContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context used for the record operations.</param>
    public RecordBusiness(DeeplynxContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Retrieves all records for a specific project and datasource.
    /// </summary>
    /// <param name="projectId">The ID of the project whose records are to be retrieved</param>
    /// <param name="dataSourceId">(Optional) The ID of the datasource by which to filter records</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived records from the result</param>
    /// <returns>A list of records based on the applied filters.</returns>
    public async Task<List<RecordResponseDto>> GetAllRecords(
        long projectId, long? dataSourceId, bool hideArchived)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, hideArchived);
        var recordQuery = _context.Records
            .Where(r => r.ProjectId == projectId);

        if (hideArchived)
        {
            recordQuery = recordQuery.Where(r => r.ArchivedAt == null);
        }
        
        var records = await recordQuery
            .Include(r => r.Tags)
            .ToListAsync();

        return records
            .Select(r => new RecordResponseDto()
            {
                Id = r.Id,
                Description = r.Description,
                Uri = r.Uri,
                Properties = r.Properties,
                OriginalId = r.OriginalId,
                Name = r.Name,
                ClassId = r.ClassId,
                DataSourceId = r.DataSourceId,
                ProjectId = r.ProjectId,
                CreatedBy = r.CreatedBy,
                CreatedAt = r.CreatedAt,
                ModifiedBy = r.ModifiedBy,
                ModifiedAt = r.ModifiedAt,
                ArchivedAt = r.ArchivedAt,
                Tags = r.Tags.Select(t => new RecordTagDto()
                {
                    Id = t.Id,
                    Name = t.Name
                }).ToList()
            }).ToList();
    }
    
    /// <summary>
    /// Retrieves a specific record by its ID
    /// </summary>
    /// <param name="projectId">The project of the record to retrieve</param>
    /// <param name="recordId">The ID of the record to retrieve</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived records from the result</param>
    /// <returns>The record in question</returns>
    /// <exception cref="KeyNotFoundException">Returned if record not found</exception>
    public async Task<RecordResponseDto> GetRecord(
        long projectId, long recordId, bool hideArchived)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, hideArchived);
        
        var record = await _context.Records
            .Where(r => r.ProjectId == projectId && r.Id == recordId)
            .Include(r => r.Tags)
            .FirstOrDefaultAsync();

        if (record == null)
        {
            throw new KeyNotFoundException($"Record with id {recordId} not found");
        }

        if (hideArchived && record.ArchivedAt != null)
        {
            throw new KeyNotFoundException($"Record with id {recordId} is archived");
        }

        return new RecordResponseDto
        {
            Id = record.Id,
            Description = record.Description,
            Uri = record.Uri,
            Properties = record.Properties,
            OriginalId = record.OriginalId,
            Name = record.Name,
            ClassId = record.ClassId,
            DataSourceId = record.DataSourceId,
            ProjectId = record.ProjectId,
            CreatedBy = record.CreatedBy,
            CreatedAt = record.CreatedAt,
            ModifiedBy = record.ModifiedBy,
            ModifiedAt = record.ModifiedAt,
            ArchivedAt = record.ArchivedAt,
            Tags = record.Tags.Select(t => new RecordTagDto()
            {
                Id = t.Id,
                Name = t.Name
            }).ToList()
        };
    }

    /// <summary>
    /// Create a new record
    /// </summary>
    /// <param name="projectId">The ID of the project under which to create the record</param>
    /// <param name="dataSourceId">The ID of the data source under which to create the record</param>
    /// <param name="dto">The data transfer object containing details on the record to be created</param>
    /// <returns>The newly created metadata record</returns>
    /// <exception cref="KeyNotFoundException">Returned if the project or datasource are not found</exception>
    /// <exception cref="Exception">Returned if the metadata is too deeply nested</exception>
    public async Task<RecordResponseDto> CreateRecord(long projectId, long dataSourceId, CreateRecordRequestDto dto)
    {
       await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
       await ExistenceHelper.EnsureDataSourceExistsAsync(_context, dataSourceId);
       ValidationHelper.ValidateModel(dto);
        
        if(dto.Properties == null)
            throw new ArgumentNullException(nameof(dto.Properties), "Properties cannot be null");
        
        var maxDepth = CalculateJsonMaxDepth(dto.Properties);
        if (maxDepth > 3)
        {
            throw new Exception($"The depth of the JSON structure exceeds the maximum allowed depth of 3. Current depth of properties is {maxDepth}.");
        }
        
        var record = new Record
        {
            ProjectId = projectId,
            DataSourceId = dataSourceId,
            Uri = dto.Uri,
            Properties = dto.Properties.ToString()!,
            OriginalId = dto.OriginalId,
            Name = dto.Name,
            Description = dto.Description,
            ClassId = dto.ClassId,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            CreatedBy = null  // TODO: Implement user ID here when JWT tokens are ready
        };

        _context.Records.Add(record);
        await _context.SaveChangesAsync();

        return new RecordResponseDto
        {
            Id = record.Id,
            Description = record.Description,
            Uri = record.Uri,
            Properties = record.Properties,
            OriginalId = record.OriginalId,
            Name = record.Name,
            ClassId = record.ClassId,
            DataSourceId = record.DataSourceId,
            ProjectId = record.ProjectId,
            CreatedBy = record.CreatedBy,
            CreatedAt = record.CreatedAt,
            ModifiedBy = record.ModifiedBy,
            ModifiedAt = record.ModifiedAt,
            ArchivedAt = record.ArchivedAt,
        };
    }
    
    /// <summary>
    /// Create new records
    /// </summary>
    /// <param name="projectId">The ID of the project under which to create the record</param>
    /// <param name="dataSourceId">The ID of the data source under which to create the record</param>
    /// <param name="records">The data transfer object containing details on the records to be created</param>
    /// <returns>The newly created metadata record</returns>
    /// <exception cref="KeyNotFoundException">Returned if the project or datasource are not found</exception>
    /// <exception cref="Exception">Returned if the metadata is too deeply nested</exception>
    public async Task<List<RecordResponseDto>> BulkCreateRecords(
        long projectId, 
        long dataSourceId, 
        List<CreateRecordRequestDto> records)
    {
       await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
       await ExistenceHelper.EnsureDataSourceExistsAsync(_context, dataSourceId);

       if (records.Count == 0)
       {
           throw new Exception("Unable to bulk create records: no records selected for creation");
       }
       
       // Bulk insert into records; if there is an original ID collision, update name, desc, uri, class, and props
       var sql = @"
            INSERT INTO deeplynx.records (project_id, data_source_id, name, description, uri,
                                          original_id, properties, class_id, created_at)
            VALUES {0}
            ON CONFLICT (project_id, data_source_id, original_id) DO UPDATE SET
                name = COALESCE(EXCLUDED.name, records.name),
                description = COALESCE(EXCLUDED.description, records.description),
                uri = COALESCE(EXCLUDED.uri, records.uri),
                properties = COALESCE(EXCLUDED.properties, records.properties),
                class_id = COALESCE(EXCLUDED.class_id, records.class_id),
                modified_at = @now
            RETURNING *;                                                          
        ";
       
       // establish "constant" parameters
       var parameters = new List<NpgsqlParameter>
       {
           new NpgsqlParameter("@projectId", projectId),
           new NpgsqlParameter("@dataSourceId", dataSourceId),
           new NpgsqlParameter("@now", DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified))
       };
       
       // establish "dynamic" parameters (new for each dto in the list)
       parameters.AddRange(records.SelectMany((dto, i) => new[]
       {
           new NpgsqlParameter($"@p{i}_name", dto.Name),
           new NpgsqlParameter($"@p{i}_desc", dto.Description),
           new NpgsqlParameter($"@p{i}_uri", (object?)dto.Uri ?? DBNull.Value),
           new NpgsqlParameter($"@p{i}_props", JsonSerializer.Serialize(dto.Properties)),
           new NpgsqlParameter($"@p{i}_orig", dto.OriginalId),
           new NpgsqlParameter($"@p{i}_class", (object?)dto.ClassId ?? DBNull.Value),
       }));

       // stringify the params and comma separate them
       var valueTuples = string.Join(", ", records.Select((dto, i) =>
           $"(@projectId, @dataSourceId, @p{i}_name, @p{i}_desc, " +
           $"@p{i}_uri, @p{i}_orig, @p{i}_props::jsonb, @p{i}_class, @now)"));
        
       // put everything together and execute the query
       sql = string.Format(sql, valueTuples);

       // returns the resulting upserted classes
       return await _context.Database
           .SqlQueryRaw<RecordResponseDto>(sql, parameters.ToArray())
           .ToListAsync();
    }

    /// <summary>
    /// Updates a record with new information
    /// </summary>
    /// <param name="projectId">The ID of the project to which the record belongs</param>
    /// <param name="recordId">The ID of the record to be updated</param>
    /// <param name="dto">The data transfer object containing details on the record to be updated</param>
    /// <returns>The newly updated metadata record</returns>
    /// <exception cref="KeyNotFoundException">Returned if record to be updated is not found</exception>
    public async Task<RecordResponseDto> UpdateRecord(long projectId, long recordId, UpdateRecordRequestDto dto)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        var record= await _context.Records.FindAsync(recordId);
        if (record == null || record.ProjectId != projectId || record.ArchivedAt != null)
        {
            throw new KeyNotFoundException($"Record with id {recordId} not found");
        }
        
        var maxDepth = CalculateJsonMaxDepth(dto.Properties);
        if (maxDepth > 3)
        {
            throw new Exception($"The depth of the JSON structure exceeds the maximum allowed depth of 3. Current depth of properties is {maxDepth}.");
        }
        
        record.Uri = dto.Uri ?? record.Uri;
        record.Properties = dto.Properties != null ? dto.Properties.ToString() : record.Properties;
        record.OriginalId = dto.OriginalId ?? record.OriginalId;
        record.Name = dto.Name ?? record.Name;
        record.Description = dto.Description ?? record.Description;
        record.ClassId = dto.ClassId ?? record.ClassId;
        record.ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        record.ModifiedBy = null; // TODO: Implement user ID here when JWT tokens are ready
        
        _context.Records.Update(record);
        await _context.SaveChangesAsync();
        
        return new RecordResponseDto
        {
            Id = record.Id,
            Description = record.Description,
            Uri = record.Uri,
            Properties = record.Properties,
            OriginalId = record.OriginalId,
            Name = record.Name,
            ClassId = record.ClassId,
            DataSourceId = record.DataSourceId,
            ProjectId = record.ProjectId,
            CreatedBy = record.CreatedBy,
            CreatedAt = record.CreatedAt,
            ModifiedBy = record.ModifiedBy,
            ModifiedAt = record.ModifiedAt,
            ArchivedAt = record.ArchivedAt,
        };
        
    }

    /// <summary>
    /// Delete a metadata record.
    /// </summary>
    /// <param name="projectId">The project to which the record belongs</param>
    /// <param name="recordId">The record in question</param>
    /// <returns>Boolean indicating record was deleted</returns>
    /// <exception cref="KeyNotFoundException">Returned if the record to delete was not found.</exception>
    /// TODO: return warning that historical data will be entirely wiped with this action
    public async Task<bool> DeleteRecord(long projectId, long recordId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        var record = await _context.Records.FindAsync(recordId);
        
        if (record == null || record.ProjectId != projectId)
            throw new KeyNotFoundException($"Record with id {recordId} not found");
        
        _context.Records.Remove(record);
        await _context.SaveChangesAsync();
        
        return true;
    }
    
    /// <summary>
    /// Archive a metadata record.
    /// </summary>
    /// <param name="projectId">The project to which the record belongs</param>
    /// <param name="recordId">The record to be archived</param>
    /// <returns>Boolean indicating record was archived</returns>
    /// <exception cref="KeyNotFoundException">Returned if the record to archive was not found.</exception>
    public async Task<bool> ArchiveRecord(long projectId, long recordId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        var record = await _context.Records.FindAsync(recordId);
        
        if (record == null || record.ProjectId != projectId || record.ArchivedAt != null)
            throw new KeyNotFoundException($"Record with id {recordId} not found");
        
        // set archivedAt timestamp
        var archivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        // run archive procedure in a transaction to roll back any errors
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // run the archive record procedure, which archives this record
                // and all child objects with record_id as a foreign key
                var archived = await _context.Database.ExecuteSqlRawAsync(
                    "CALL deeplynx.archive_record(@p0::INTEGER, @p1::TIMESTAMP WITHOUT TIME ZONE)", recordId, archivedAt);

                if (archived == 0) // if 0 records were updated, assume a failure
                {
                    throw new DependencyDeletionException($"unable to archive record {recordId} or its downstream dependents.");
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception exc)
            {
                await transaction.RollbackAsync();
                throw new DependencyDeletionException($"unable to archive record {recordId} or its downstream dependents: {exc}");
            }
        }
    }
    
    /// <summary>
    /// Unarchive a metadata record.
    /// </summary>
    /// <param name="projectId">The project to which the record belongs</param>
    /// <param name="recordId">The record to be unarchived</param>
    /// <returns>Boolean indicating record was unarchived</returns>
    /// <exception cref="KeyNotFoundException">Returned if the record to unarchive was not found.</exception>
    public async Task<bool> UnarchiveRecord(long projectId, long recordId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);
        var record = await _context.Records.FindAsync(recordId);
        
        if (record == null || record.ProjectId != projectId || record.ArchivedAt is null)
            throw new KeyNotFoundException($"Record with id {recordId} not found or is not archived.");

        // run unarchive procedure in a transaction to roll back any errors
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // run the unarchive record procedure, which unarchives this record
                // and all child objects with record_id as a foreign key
                var unarchived = await _context.Database.ExecuteSqlRawAsync(
                    "CALL deeplynx.unarchive_record({0}::INTEGER)", recordId);

                if (unarchived == 0) // if 0 records were updated, assume a failure
                {
                    throw new DependencyDeletionException($"unable to unarchive record {recordId} or its downstream dependents.");
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception exc)
            {
                await transaction.RollbackAsync();
                throw new DependencyDeletionException($"unable to archive record {recordId} or its downstream dependents: {exc}");
            }
        }
    }


    /// <summary>
    /// Attaches a tag to a record
    /// </summary>
    /// <param name="projectId">Project ID for the record and tag</param>
    /// <param name="recordId">The ID of the record</param>
    /// <param name="tagId">The ID of the tag</param>
    /// <returns>True if successful</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the record or tag are not found</exception>
    /// <exception cref="Exception">Thrown if the tag is already attached to the record</exception>
    public async Task<bool> AttachTag(long projectId, long recordId, long tagId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);

        // include tags in record return and find record
        var recordQueryable = _context.Records.Include(r => r.Tags);
        var record = await recordQueryable.FirstOrDefaultAsync(r => r.Id == recordId);
        if (record == null || record.ProjectId != projectId || record.ArchivedAt is not null)
            throw new KeyNotFoundException($"Record with id {recordId} not found or is archived.");
        
        // find tag
        var tag = await _context.Tags.FindAsync(tagId);
        if (tag == null || tag.ProjectId != projectId || tag.ArchivedAt is not null)
            throw new KeyNotFoundException($"Tag with id {tagId} not found or is archived.");
        
        // ensure the tag is not already attached to the record
        _context.Records.Include(r => r.Tags);
        if (record.Tags.Any(t => t.Id == tagId))
            throw new Exception($"Tag with id {tagId} is already attached to record {recordId}");
        
        record.Tags.Add(tag);
        await _context.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// Unattach a tag from a record
    /// </summary>
    /// <param name="projectId">Project ID for the record and tag</param>
    /// <param name="recordId">The ID of the record</param>
    /// <param name="tagId">The ID of the tag</param>
    /// <returns>True if successful</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the record or tag are not found</exception>
    public async Task<bool> UnattachTag(long projectId, long recordId, long tagId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId);

        // include tags in record return and find record
        var recordQueryable = _context.Records.Include(r => r.Tags);
        var record = await recordQueryable.FirstOrDefaultAsync(r => r.Id == recordId);
        if (record == null || record.ProjectId != projectId || record.ArchivedAt is not null)
            throw new KeyNotFoundException($"Record with id {recordId} not found or is archived.");
        
        // find tag
        var tag = await _context.Tags.FindAsync(tagId);
        if (tag == null || tag.ProjectId != projectId || tag.ArchivedAt is not null)
            throw new KeyNotFoundException($"Tag with id {tagId} not found or is archived.");
        
        record.Tags.Remove(tag);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Bulk attach tags and records
    /// </summary>
    /// <param name="dtos">A list of record_id/tag_id pairs to be inserted</param>
    /// <returns>True if successful</returns>
    /// <exception cref="Exception">Thrown if tags unable to be attached</exception>
    public async Task<bool> BulkAttachTags(List<RecordTagLinkDto> dtos)
    {
        // Bulk insert into record_tags
        var sql = @"INSERT INTO deeplynx.record_tags (record_id, tag_id) VALUES {0} ON CONFLICT DO NOTHING;";
        
        // establish parameters
        var parameters = new List<NpgsqlParameter>();
        parameters.AddRange(dtos.SelectMany((dto, i) => new[]
        {
            new NpgsqlParameter($"@record{i}_id", dto.RecordId),
            new NpgsqlParameter($"@tag{i}_id", dto.TagId)
        }));
        
        // stringify params and comma separate them
        var valueTuples = string.Join(", ", dtos.Select((dto, i) => $"(@record{i}_id, @tag{i}_id)"));
        
        // put everything together and execute the query
        sql = string.Format(sql, valueTuples);
        
        await _context.Database.ExecuteSqlRawAsync(sql, parameters.ToArray());
        
        return true;
    }

    /// <summary>
    /// Private method used to calculate json depth of properties (should be less than three)
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private int CalculateJsonMaxDepth(JsonNode node)
    {
        if (node is not JsonObject && node is not JsonArray)
            return 0;

        int maxDepth = 0;
        if (node is JsonObject jsonObject)
        {
            foreach (var prop in jsonObject)
            {
                int depth = CalculateJsonMaxDepth(prop.Value);
                if (depth > maxDepth)
                    maxDepth = depth;
            }
        }
        else if (node is JsonArray jsonArray)
        {
            foreach (JsonNode item in jsonArray)
            {
                int depth = CalculateJsonMaxDepth(item);
                if (depth > maxDepth)
                    maxDepth = depth;
            }
        }

        return maxDepth + 1;
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
