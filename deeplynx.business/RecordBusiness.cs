using System.Data;
using System.Text.Json;
using System.Text.Json.Nodes;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.helpers.exceptions;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace deeplynx.business;

public class RecordBusiness : IRecordBusiness
{
    private readonly ICacheBusiness _cacheBusiness;
    private readonly DeeplynxContext _context;
    private readonly IEventBusiness _eventBusiness;
    private readonly IBulkCopyUpsertExecutor _bulkCopyUpsertExecutor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RecordBusiness" /> class.
    /// </summary>
    /// <param name="context">The database context used for the record operations.</param>
    /// <param name="cacheBusiness">Used to access cache operations</param>
    /// <param name="eventBusiness">Used for logging events during create, update, and delete Operations.</param>
    public RecordBusiness(DeeplynxContext context, ICacheBusiness cacheBusiness, IEventBusiness eventBusiness, IBulkCopyUpsertExecutor bulkCopyUpsertExecutor)
    {
        _context = context;
        _cacheBusiness = cacheBusiness;
        _eventBusiness = eventBusiness;
        _bulkCopyUpsertExecutor = bulkCopyUpsertExecutor;
    }

    /// <summary>
    ///     Retrieves all records for a specific project and datasource.
    /// </summary>
    /// <param name="projectId">The ID of the project whose records are to be retrieved</param>
    /// <param name="dataSourceId">(Optional) The ID of the datasource by which to filter records</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived records from the result</param>
    /// <param name="fileType">File extension to filter by (e.g., pdf, png, jpg)</param>
    /// <returns>A list of records based on the applied filters.</returns>
    public async Task<List<RecordResponseDto>> GetAllRecords(
        long projectId, long? dataSourceId, bool hideArchived, string? fileType = null)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness, hideArchived);
        var recordQuery = _context.Records
            .Where(r => r.ProjectId == projectId);

        if (hideArchived) recordQuery = recordQuery.Where(r => !r.IsArchived);

        if (dataSourceId.HasValue) recordQuery = recordQuery.Where(r => r.DataSourceId == dataSourceId);

        if (!string.IsNullOrWhiteSpace(fileType))
        {
            var formattedFileType = fileType.TrimStart('.').ToLower();
            recordQuery = recordQuery.Where(r => r.FileType == formattedFileType);
        }

        var records = await recordQuery
            .Include(r => r.Tags)
            .ToListAsync();

        return records
            .Select(r => new RecordResponseDto
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
                LastUpdatedBy = r.LastUpdatedBy,
                LastUpdatedAt = r.LastUpdatedAt,
                IsArchived = r.IsArchived,
                FileType = r.FileType,
                Tags = r.Tags.Select(t => new RecordTagDto
                {
                    Id = t.Id,
                    Name = t.Name
                }).ToList()
            }).ToList();
    }

    /// <summary>
    ///     Get all records that contain all given tags
    /// </summary>
    /// <param name="projectId">The ID of the project whose records are to be retrieved</param>
    /// <param name="tagIds">List of tag IDs - returned records must contain every given ID</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived records from the result</param>
    /// <returns></returns>
    public async Task<List<RecordResponseDto>> GetRecordsByTags(
        long projectId, long[] tagIds, bool hideArchived)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness, hideArchived);
        var recordQuery = _context.Records
            .Where(r => r.ProjectId == projectId);

        if (hideArchived) recordQuery = recordQuery.Where(r => !r.IsArchived);

        // Only return records that contain ALL given IDs
        recordQuery = recordQuery.Where(r =>
            tagIds.All(tagId => r.Tags.Any(t => t.Id == tagId)));

        var records = await recordQuery.Include(r => r.Tags).ToListAsync();

        return records
            .Select(r => new RecordResponseDto
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
                LastUpdatedBy = r.LastUpdatedBy,
                LastUpdatedAt = r.LastUpdatedAt,
                IsArchived = r.IsArchived,
                FileType = r.FileType,
                Tags = r.Tags.Select(t => new RecordTagDto
                {
                    Id = t.Id,
                    Name = t.Name
                }).ToList()
            }).ToList();
    }

    /// <summary>
    ///     Retrieves a specific record by its ID
    /// </summary>
    /// <param name="projectId">The project of the record to retrieve</param>
    /// <param name="recordId">The ID of the record to retrieve</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived records from the result</param>
    /// <returns>The record in question</returns>
    /// <exception cref="KeyNotFoundException">Returned if record not found</exception>
    public async Task<RecordResponseDto> GetRecord(
        long projectId, long recordId, bool hideArchived)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness, hideArchived);

        var record = await _context.Records
            .Where(r => r.ProjectId == projectId && r.Id == recordId)
            .Include(r => r.Tags)
            .FirstOrDefaultAsync();

        if (record == null) throw new KeyNotFoundException($"Record with id {recordId} not found");

        if (hideArchived && record.IsArchived) throw new KeyNotFoundException($"Record with id {recordId} is archived");

        return new RecordResponseDto
        {
            Id = record.Id,
            Description = record.Description,
            Uri = record.Uri,
            Properties = record.Properties,
            OriginalId = record.OriginalId,
            ObjectStorageId = record.ObjectStorageId,
            Name = record.Name,
            ClassId = record.ClassId,
            DataSourceId = record.DataSourceId,
            ProjectId = record.ProjectId,
            LastUpdatedBy = record.LastUpdatedBy,
            LastUpdatedAt = record.LastUpdatedAt,
            IsArchived = record.IsArchived,
            FileType = record.FileType,
            Tags = record.Tags.Select(t => new RecordTagDto
            {
                Id = t.Id,
                Name = t.Name
            }).ToList()
        };
    }

    /// <summary>
    ///     Create a new record
    /// </summary>
    /// <param name="projectId">The ID of the project under which to create the record</param>
    /// <param name="dataSourceId">The ID of the data source under which to create the record</param>
    /// <param name="dto">The data transfer object containing details on the record to be created</param>
    /// <returns>The newly created metadata record</returns>
    /// <exception cref="KeyNotFoundException">Returned if the project or datasource are not found</exception>
    /// <exception cref="Exception">Returned if the metadata is too deeply nested</exception>
    public async Task<RecordResponseDto> CreateRecord(long projectId, long dataSourceId, CreateRecordRequestDto dto)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
        await ExistenceHelper.EnsureDataSourceExistsForProjectAsync(_context, dataSourceId, projectId);
        ValidationHelper.ValidateModel(dto);

        if (dto.Properties == null)
            throw new ArgumentNullException(nameof(dto.Properties), "Properties cannot be null");

        var maxDepth = CalculateJsonMaxDepth(dto.Properties);
        if (maxDepth > 3)
            throw new Exception(
                $"The depth of the JSON structure exceeds the maximum allowed depth of 3. Current depth of properties is {maxDepth}.");

        if (dto.ObjectStorageId != null) await CheckObjectStorageExists(projectId, dto.ObjectStorageId.Value);

        var record = new Record
        {
            ProjectId = projectId,
            DataSourceId = dataSourceId,
            Uri = dto.Uri,
            ObjectStorageId = dto.ObjectStorageId,
            Properties = dto.Properties.ToString()!,
            OriginalId = dto.OriginalId,
            Name = dto.Name,
            Description = dto.Description,
            ClassId = dto.ClassId,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = null, // TODO: Implement user ID here when JWT tokens are ready
            FileType = dto.FileType
        };

        _context.Records.Add(record);
        await _context.SaveChangesAsync();

        // Log Record Create Event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            ProjectId = record.ProjectId,
            EntityType = "record",
            EntityId = record.Id,
            EntityName = record.Name,
            Operation = "create",
            Properties = "{}",
            DataSourceId = record.DataSourceId
        });

        return new RecordResponseDto
        {
            Id = record.Id,
            Description = record.Description,
            Uri = record.Uri,
            Properties = record.Properties,
            ObjectStorageId = record.ObjectStorageId,
            OriginalId = record.OriginalId,
            Name = record.Name,
            ClassId = record.ClassId,
            DataSourceId = record.DataSourceId,
            ProjectId = record.ProjectId,
            LastUpdatedBy = record.LastUpdatedBy,
            LastUpdatedAt = record.LastUpdatedAt,
            IsArchived = record.IsArchived,
            FileType = record.FileType
        };
    }

    // Adapter around your executor, focused on the records table
    public async Task<List<RecordResponseDto>> BulkCreateRecords(
        long projectId, long dataSourceId, List<CreateRecordRequestDto> records)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
        await ExistenceHelper.EnsureDataSourceExistsForProjectAsync(_context, dataSourceId, projectId);
        if (records.Count == 0) return new List<RecordResponseDto>();

        // One-shot validation for object storage references (optional, fast)
        await EnsureObjectStoragesExistOnce(projectId, records);

        var conn = (NpgsqlConnection)_context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        const string createTempSql = @"
        CREATE TEMP TABLE tmp_records
        (
            project_id          BIGINT NOT NULL,
            data_source_id      BIGINT NOT NULL,
            name                TEXT NULL,
            description         TEXT NULL,
            uri                 TEXT NULL,
            original_id         TEXT NOT NULL,
            properties          JSONB NULL,
            class_id            BIGINT NULL,
            object_storage_id   BIGINT NULL,
            file_type           TEXT NULL,
            last_updated_at     TIMESTAMP WITHOUT TIME ZONE NOT NULL,
            is_archived         BOOLEAN NOT NULL,
            last_updated_by     BIGINT NULL
        ) ON COMMIT DROP;";

        const string copyCmd = @"
        COPY tmp_records
        (project_id, data_source_id, name, description, uri,
         original_id, properties, class_id, object_storage_id, file_type,
         last_updated_at, is_archived, last_updated_by)
        FROM STDIN (FORMAT BINARY)";

        const string upsertSql = @"
        INSERT INTO deeplynx.records
        (project_id, data_source_id, name, description, uri,
         original_id, properties, class_id, object_storage_id, file_type,
         last_updated_at, is_archived, last_updated_by)
        SELECT project_id, data_source_id, name, description, uri,
               original_id, properties, class_id, object_storage_id, file_type,
               last_updated_at, is_archived, last_updated_by
        FROM tmp_records
        ON CONFLICT (project_id, data_source_id, original_id) DO UPDATE
          SET name              = COALESCE(EXCLUDED.name, records.name),
              description       = COALESCE(EXCLUDED.description, records.description),
              uri               = COALESCE(EXCLUDED.uri, records.uri),
              properties        = COALESCE(EXCLUDED.properties, records.properties),
              class_id          = COALESCE(EXCLUDED.class_id, records.class_id),
              object_storage_id = COALESCE(EXCLUDED.object_storage_id, records.object_storage_id),
              last_updated_at   = EXCLUDED.last_updated_at,
              file_type         = COALESCE(EXCLUDED.file_type, records.file_type)
        RETURNING id, project_id, data_source_id, original_id, name, class_id, object_storage_id, file_type;";

        var inserted = await _bulkCopyUpsertExecutor.CopyUpsertAsync<CreateRecordRequestDto, RecordResponseDto>(
            conn, tx,
            createTempSql,
            copyCmd,
            records,
            (w, dto) =>
            {
                w.Write(projectId, NpgsqlDbType.Bigint);
                w.Write(dataSourceId, NpgsqlDbType.Bigint);
                if (dto.Name is null) w.WriteNull();
                else w.Write(dto.Name, NpgsqlDbType.Text);
                if (dto.Description is null) w.WriteNull();
                else w.Write(dto.Description, NpgsqlDbType.Text);
                if (dto.Uri is null) w.WriteNull();
                else w.Write(dto.Uri, NpgsqlDbType.Text);
                w.Write(dto.OriginalId, NpgsqlDbType.Text);

                if (dto.Properties is null) w.WriteNull();
                else w.Write(JsonSerializer.Serialize(dto.Properties), NpgsqlDbType.Jsonb);

                if (dto.ClassId.HasValue) w.Write(dto.ClassId.Value, NpgsqlDbType.Bigint);
                else w.WriteNull();
                if (dto.ObjectStorageId.HasValue) w.Write(dto.ObjectStorageId.Value, NpgsqlDbType.Bigint);
                else w.WriteNull();
                if (dto.FileType is null) w.WriteNull();
                else w.Write(dto.FileType, NpgsqlDbType.Text);

                w.Write(now, NpgsqlDbType.Timestamp);
                w.Write(false, NpgsqlDbType.Boolean);
                w.WriteNull(); // last_updated_by
            },
            upsertSql,
            r => new RecordResponseDto
            {
                Id = r.GetInt64(r.GetOrdinal("id")),
                ProjectId = r.GetInt64(r.GetOrdinal("project_id")),
                DataSourceId = r.GetInt64(r.GetOrdinal("data_source_id")),
                OriginalId = r.GetString(r.GetOrdinal("original_id")),
                Name = r.IsDBNull(r.GetOrdinal("name")) ? null : r.GetString(r.GetOrdinal("name")),
                ClassId = r.IsDBNull(r.GetOrdinal("class_id")) ? null : r.GetInt64(r.GetOrdinal("class_id")),
                ObjectStorageId = r.IsDBNull(r.GetOrdinal("object_storage_id"))
                    ? null
                    : r.GetInt64(r.GetOrdinal("object_storage_id")),
                FileType = r.IsDBNull(r.GetOrdinal("file_type")) ? null : r.GetString(r.GetOrdinal("file_type"))
            });

        // Bulk events (copy→insert), see next section
        //await _eventsWriter.BulkInsertRecordCreatesAsync(conn, tx, projectId, inserted);

        await tx.CommitAsync();
        return inserted;
    }


    /// <summary>
    ///     Updates a record with new information
    /// </summary>
    /// <param name="projectId">The ID of the project to which the record belongs</param>
    /// <param name="recordId">The ID of the record to be updated</param>
    /// <param name="dto">The data transfer object containing details on the record to be updated</param>
    /// <returns>The newly updated metadata record</returns>
    /// <exception cref="KeyNotFoundException">Returned if record to be updated is not found</exception>
    public async Task<RecordResponseDto> UpdateRecord(long projectId, long recordId, UpdateRecordRequestDto dto)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
        var record = await _context.Records.FindAsync(recordId);
        if (record == null || record.ProjectId != projectId || record.IsArchived)
            throw new KeyNotFoundException($"Record with id {recordId} not found");

        var maxDepth = CalculateJsonMaxDepth(dto.Properties);
        if (maxDepth > 3)
            throw new Exception(
                $"The depth of the JSON structure exceeds the maximum allowed depth of 3. Current depth of properties is {maxDepth}.");

        if (dto.ObjectStorageId != null) await CheckObjectStorageExists(projectId, dto.ObjectStorageId.Value);

        record.Uri = dto.Uri ?? record.Uri;
        record.Properties = dto.Properties != null ? dto.Properties.ToString() : record.Properties;
        record.OriginalId = dto.OriginalId ?? record.OriginalId;
        record.ObjectStorageId = dto.ObjectStorageId ?? record.ObjectStorageId;
        record.Name = dto.Name ?? record.Name;
        record.Description = dto.Description ?? record.Description;
        record.ClassId = dto.ClassId ?? record.ClassId;
        record.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        record.LastUpdatedBy = null; // TODO: Implement user ID here when JWT tokens are ready
        record.FileType = dto.FileType ?? record.FileType;

        _context.Records.Update(record);
        await _context.SaveChangesAsync();

        // Log Record Update Event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            ProjectId = record.ProjectId,
            EntityType = "record",
            EntityId = record.Id,
            EntityName = record.Name,
            Operation = "update",
            Properties = "{}",
            DataSourceId = record.DataSourceId
        });

        return new RecordResponseDto
        {
            Id = record.Id,
            Description = record.Description,
            Uri = record.Uri,
            Properties = record.Properties,
            ObjectStorageId = record.ObjectStorageId,
            OriginalId = record.OriginalId,
            Name = record.Name,
            ClassId = record.ClassId,
            DataSourceId = record.DataSourceId,
            ProjectId = record.ProjectId,
            LastUpdatedBy = record.LastUpdatedBy,
            LastUpdatedAt = record.LastUpdatedAt,
            IsArchived = record.IsArchived,
            FileType = record.FileType
        };
    }

    /// <summary>
    ///     Delete a metadata record.
    /// </summary>
    /// <param name="projectId">The project to which the record belongs</param>
    /// <param name="recordId">The record in question</param>
    /// <returns>Boolean indicating record was deleted</returns>
    /// <exception cref="KeyNotFoundException">Returned if the record to delete was not found.</exception>
    /// TODO: return warning that historical data will be entirely wiped with this action
    public async Task<bool> DeleteRecord(long projectId, long recordId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
        var record = await _context.Records.FindAsync(recordId);

        if (record == null || record.ProjectId != projectId)
            throw new KeyNotFoundException($"Record with id {recordId} not found");

        _context.Records.Remove(record);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    ///     Archive a metadata record.
    /// </summary>
    /// <param name="projectId">The project to which the record belongs</param>
    /// <param name="recordId">The record to be archived</param>
    /// <returns>Boolean indicating record was archived</returns>
    /// <exception cref="KeyNotFoundException">Returned if the record to archive was not found.</exception>
    public async Task<bool> ArchiveRecord(long projectId, long recordId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
        var record = await _context.Records.FindAsync(recordId);

        if (record == null || record.ProjectId != projectId || record.IsArchived)
            throw new KeyNotFoundException($"Record with id {recordId} not found");

        // set lastUpdatedAt timestamp
        var lastUpdatedAt = DateTime.UtcNow;

        // run archive procedure in a transaction to roll back any errors
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // run the archive record procedure, which archives this record
                // and all child objects with record_id as a foreign key
                var archived = await _context.Database.ExecuteSqlRawAsync(
                    "CALL deeplynx.archive_record({0}::INTEGER, {1}::TIMESTAMP WITHOUT TIME ZONE)",
                    recordId, lastUpdatedAt
                );

                if (archived == 0) // if 0 records were updated, assume a failure
                    throw new DependencyDeletionException(
                        $"unable to archive record {recordId} or its downstream dependents.");

                await transaction.CommitAsync();
            }
            catch (Exception exc)
            {
                await transaction.RollbackAsync();
                throw new DependencyDeletionException(
                    $"unable to archive record {recordId} or its downstream dependents: {exc}");
            }
        }

        // Log record soft delete event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            ProjectId = projectId,
            Operation = "archive",
            EntityType = "record",
            EntityId = record.Id,
            EntityName = record.Name,
            DataSourceId = record.DataSourceId,
            Properties = JsonSerializer.Serialize(new { record.Name })
        });

        return true;
    }

    /// <summary>
    ///     Unarchive a metadata record.
    /// </summary>
    /// <param name="projectId">The project to which the record belongs</param>
    /// <param name="recordId">The record to be unarchived</param>
    /// <returns>Boolean indicating record was unarchived</returns>
    /// <exception cref="KeyNotFoundException">Returned if the record to unarchive was not found.</exception>
    public async Task<bool> UnarchiveRecord(long projectId, long recordId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);
        var record = await _context.Records.FindAsync(recordId);

        if (record == null || record.ProjectId != projectId || !record.IsArchived)
            throw new KeyNotFoundException($"Record with id {recordId} not found");

        // set lastUpdatedAt timestamp
        var lastUpdatedAt = DateTime.UtcNow;

        // run unarchive procedure in a transaction to roll back any errors
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // run the unarchive record procedure, which unarchives this record
                // and all child objects with record_id as a foreign key
                var unarchived = await _context.Database.ExecuteSqlRawAsync(
                    "CALL deeplynx.unarchive_record({0}::INTEGER, {1}::TIMESTAMP WITHOUT TIME ZONE)",
                    recordId, lastUpdatedAt
                );

                if (unarchived == 0) // if 0 records were updated, assume a failure
                    throw new DependencyDeletionException(
                        $"unable to unarchive record {recordId} or its downstream dependents.");

                await transaction.CommitAsync();
            }
            catch (Exception exc)
            {
                await transaction.RollbackAsync();
                throw new DependencyDeletionException(
                    $"unable to unarchive record {recordId} or its downstream dependents: {exc}");
            }
        }

        // Log record soft delete event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            ProjectId = projectId,
            Operation = "unarchive",
            EntityType = "record",
            EntityId = record.Id,
            EntityName = record.Name,
            DataSourceId = record.DataSourceId,
            Properties = JsonSerializer.Serialize(new { record.Name })
        });

        return true;
    }


    /// <summary>
    ///     Attaches a tag to a record
    /// </summary>
    /// <param name="projectId">Project ID for the record and tag</param>
    /// <param name="recordId">The ID of the record</param>
    /// <param name="tagId">The ID of the tag</param>
    /// <returns>True if successful</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the record or tag are not found</exception>
    /// <exception cref="Exception">Thrown if the tag is already attached to the record</exception>
    public async Task<bool> AttachTag(long projectId, long recordId, long tagId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);

        // include tags in record return and find record
        var recordQueryable = _context.Records.Include(r => r.Tags);
        var record = await recordQueryable.FirstOrDefaultAsync(r => r.Id == recordId);
        if (record == null || record.ProjectId != projectId || record.IsArchived)
            throw new KeyNotFoundException($"Record with id {recordId} not found or is archived.");

        // find tag
        var tag = await _context.Tags.FindAsync(tagId);
        if (tag == null || tag.ProjectId != projectId || tag.IsArchived)
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
    ///     Unattach a tag from a record
    /// </summary>
    /// <param name="projectId">Project ID for the record and tag</param>
    /// <param name="recordId">The ID of the record</param>
    /// <param name="tagId">The ID of the tag</param>
    /// <returns>True if successful</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the record or tag are not found</exception>
    public async Task<bool> UnattachTag(long projectId, long recordId, long tagId)
    {
        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);

        // include tags in record return and find record
        var recordQueryable = _context.Records.Include(r => r.Tags);
        var record = await recordQueryable.FirstOrDefaultAsync(r => r.Id == recordId);
        if (record == null || record.ProjectId != projectId || record.IsArchived)
            throw new KeyNotFoundException($"Record with id {recordId} not found or is archived.");

        // find tag
        var tag = await _context.Tags.FindAsync(tagId);
        if (tag == null || tag.ProjectId != projectId || tag.IsArchived)
            throw new KeyNotFoundException($"Tag with id {tagId} not found or is archived.");

        record.Tags.Remove(tag);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    ///     Bulk attach tags and records
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

    // </summary>
    /// <param name="projectId">The project ID to search within</param>
    /// <param name="originalIds">List of original IDs to validate</param>
    /// <returns>List of records that were found</returns>
    /// <exception cref="KeyNotFoundException">Thrown if one or more original IDs not found</exception>
    /// <exception cref="ArgumentException">Thrown if originalIds list is null or empty</exception>
    public async Task<List<RecordResponseDto>> GetRecordsByOriginalId(long projectId, List<string> originalIds)
    {
        if (originalIds == null || !originalIds.Any())
            throw new ArgumentException("Original IDs list cannot be null or empty", nameof(originalIds));

        await ExistenceHelper.EnsureProjectExistsAsync(_context, projectId, _cacheBusiness);

        // Remove duplicates and filter out null/empty values
        var cleanOriginalIds = originalIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        if (!cleanOriginalIds.Any())
            throw new ArgumentException("No valid original IDs provided", nameof(originalIds));

        // Query for existing records (excluding archived)
        var existingRecords = await _context.Records
            .Where(r => r.ProjectId == projectId
                        && !r.IsArchived
                        && cleanOriginalIds.Contains(r.OriginalId))
            .Include(r => r.Tags)
            .ToListAsync();

        // Check for missing records
        var foundOriginalIds = existingRecords.Select(r => r.OriginalId).ToHashSet();
        var missingOriginalIds = cleanOriginalIds.Where(id => !foundOriginalIds.Contains(id)).ToList();

        if (missingOriginalIds.Any())
            throw new KeyNotFoundException(
                $"Records not found with original IDs: {string.Join(", ", missingOriginalIds)}");

        // Convert to DTOs
        return existingRecords.Select(r => new RecordResponseDto
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
            LastUpdatedBy = r.LastUpdatedBy,
            LastUpdatedAt = r.LastUpdatedAt,
            IsArchived = r.IsArchived,
            FileType = r.FileType,
            Tags = r.Tags.Select(t => new RecordTagDto
            {
                Id = t.Id,
                Name = t.Name
            }).ToList()
        }).ToList();
    }

    private static async Task BulkInsertEventsForRecords(
        NpgsqlConnection conn, NpgsqlTransaction tx, long projectId, List<RecordResponseDto> rows)
    {
        if (rows.Count == 0) return;

        // Example: minimally log “create” events via INSERT … SELECT over VALUES
        // (If you have an events service that must be used, pass the full list to it once;
        // avoid a per-row loop of awaits.)
        const string sql = @"
        INSERT INTO events (operation, entity_type, entity_id, entity_name, project_id, properties, data_source_id)
        SELECT x.operation, x.entity_type, x.entity_id, x.entity_name, x.project_id, x.properties, x.data_source_id
        FROM (VALUES {0}) AS x(operation, entity_type, entity_id, entity_name, project_id, properties, data_source_id);";

        // Build VALUES tuples in manageable chunks to keep the SQL size reasonable
        const int chunk = 5000;
        for (var i = 0; i < rows.Count; i += chunk)
        {
            var slice = rows.Skip(i).Take(chunk);
            var values = string.Join(",",
                slice.Select(e =>
                    $"('create','record',{e.Id}, {SqlLiteralOrNull(e.Name)}, {projectId}, '{{}}', {e.DataSourceId})"));

            var stmt = string.Format(sql, values);
            await using var cmd = new NpgsqlCommand(stmt, conn, tx);
            await cmd.ExecuteNonQueryAsync();
        }

        static string SqlLiteralOrNull(string? s)
        {
            return s is null ? "NULL" : $"'{s.Replace("'", "''")}'";
        }
    }

    private static RecordResponseDto MapRecord(NpgsqlDataReader r)
    {
        return new RecordResponseDto
        {
            Id = r.GetInt64(r.GetOrdinal("id")),
            ProjectId = r.GetInt64(r.GetOrdinal("project_id")),
            DataSourceId = r.GetInt64(r.GetOrdinal("data_source_id")),
            Name = r.IsDBNull(r.GetOrdinal("name")) ? null : r.GetString(r.GetOrdinal("name")),
            Description = r.IsDBNull(r.GetOrdinal("description")) ? null : r.GetString(r.GetOrdinal("description")),
            Uri = r.IsDBNull(r.GetOrdinal("uri")) ? null : r.GetString(r.GetOrdinal("uri")),
            OriginalId = r.GetString(r.GetOrdinal("original_id")),
            ClassId = r.IsDBNull(r.GetOrdinal("class_id")) ? null : r.GetInt64(r.GetOrdinal("class_id")),
            ObjectStorageId = r.IsDBNull(r.GetOrdinal("object_storage_id"))
                ? null
                : r.GetInt64(r.GetOrdinal("object_storage_id")),
            FileType = r.IsDBNull(r.GetOrdinal("file_type")) ? null : r.GetString(r.GetOrdinal("file_type"))
        };
    }

    /// <summary>
    ///     Private method used to calculate json depth of properties (should be less than three)
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private int CalculateJsonMaxDepth(JsonNode? node)
    {
        if (node is not JsonObject && node is not JsonArray)
            return 0;

        var maxDepth = 0;
        if (node is JsonObject jsonObject)
            foreach (var prop in jsonObject)
            {
                var depth = CalculateJsonMaxDepth(prop.Value);
                if (depth > maxDepth)
                    maxDepth = depth;
            }
        else if (node is JsonArray jsonArray)
            foreach (var item in jsonArray)
            {
                var depth = CalculateJsonMaxDepth(item);
                if (depth > maxDepth)
                    maxDepth = depth;
            }

        return maxDepth + 1;
    }

    /// <summary>
    ///     Determine if datasource exists
    /// </summary>
    /// <param name="datasourceId">The ID of the datasource we are searching for</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived projects from the result (Default true)</param>
    /// <returns>Throws error if datasource does not exist</returns>
    private void DoesDataSourceExist(long datasourceId, bool hideArchived = true)
    {
        var datasource = hideArchived
            ? _context.DataSources.Any(p => p.Id == datasourceId && !p.IsArchived)
            : _context.DataSources.Any(p => p.Id == datasourceId);
        if (!datasource) throw new KeyNotFoundException($"Datasource with id {datasourceId} not found");
    }

    private async Task EnsureObjectStoragesExistOnce(long projectId, List<CreateRecordRequestDto> records)
    {
        var ids = records.Where(r => r.ObjectStorageId.HasValue)
            .Select(r => r.ObjectStorageId!.Value)
            .Distinct()
            .ToArray();
        if (ids.Length == 0) return;

        // One database round trip                                                                                                            
        var ok = await _context.ObjectStorages
            .Where(os => os.ProjectId == projectId && ids.Contains(os.Id))
            .Select(os => os.Id)
            .ToListAsync();

        if (ok.Count != ids.Length)
        {
            var missing = ids.Except(ok).Take(5);
            throw new Exception(
                $"One or more object storage IDs do not exist in project {projectId} (e.g., {string.Join(",", missing)}).");
        }
    }

    private async Task CheckObjectStorageExists(long projectId, long objectStorageId)
    {
        var referencedObjectStorage =
            await _context.ObjectStorages.FirstOrDefaultAsync(o => o.ProjectId == projectId && o.Id == objectStorageId);
        if (referencedObjectStorage == null)
            throw new KeyNotFoundException($"Object storage with ID {objectStorageId} does not exist");
    }
}