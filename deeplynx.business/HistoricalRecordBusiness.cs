using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.business;

public class HistoricalRecordBusiness: IHistoricalRecordBusiness
{
    private readonly DeeplynxContext _context;

    public HistoricalRecordBusiness(DeeplynxContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<HistoricalRecordResponseDto>> GetAllHistoricalRecords(
        long projectId,
        long? dataSourceId = null,
        DateTime? pointInTime = null,
        bool current = false)
    {
        var recordQuery = _context.HistoricalRecords
            .Where(r => r.ProjectId == projectId && r.ArchivedAt == null);

        if (dataSourceId.HasValue)
        {
            recordQuery = recordQuery.Where(r => r.DataSourceId == dataSourceId);
        }

        if (current)
        {
            recordQuery = recordQuery.Where(r => r.Current);
        }

        // specification for "current" should override any supplied pointInTime
        if (pointInTime.HasValue && !current)
        {
            // compare the timestamp to the most recent update
            recordQuery = recordQuery
                .Where(r => r.LastUpdatedAt <= pointInTime)
                .OrderByDescending(r => r.LastUpdatedAt);
        }
        
        return await recordQuery
            .Select(r=>new HistoricalRecordResponseDto()
            {
                Id = r.RecordId,
                Uri = r.Uri,
                Properties = r.Properties,
                OriginalId = r.OriginalId,
                Name = r.Name,
                ClassId = r.ClassId,
                ClassName = r.ClassName,
                DataSourceId = r.DataSourceId,
                DataSourceName = r.DataSourceName,
                MappingId = r.MappingId,
                ProjectId = r.ProjectId,
                ProjectName = r.ProjectName,
                Tags = r.Tags,
                CreatedBy = r.CreatedBy,
                CreatedAt = r.CreatedAt,
                ModifiedBy = r.ModifiedBy,
                ModifiedAt = r.ModifiedAt,
                ArchivedAt = r.ArchivedAt,
            })
            .ToListAsync();
    }
    

    public async Task<IEnumerable<HistoricalRecordResponseDto>> GetHistoryForRecord(long recordId)
    {
        return await _context.HistoricalRecords
            .Where(r => r.RecordId == recordId)
            .OrderByDescending(r => r.LastUpdatedAt)
            .Select(r=>new HistoricalRecordResponseDto()
            {
                Id = r.RecordId,
                Uri = r.Uri,
                Properties = r.Properties,
                OriginalId = r.OriginalId,
                Name = r.Name,
                ClassId = r.ClassId,
                ClassName = r.ClassName,
                DataSourceId = r.DataSourceId,
                DataSourceName = r.DataSourceName,
                MappingId = r.MappingId,
                ProjectId = r.ProjectId,
                ProjectName = r.ProjectName,
                Tags = r.Tags,
                CreatedBy = r.CreatedBy,
                CreatedAt = r.CreatedAt,
                ModifiedBy = r.ModifiedBy,
                ModifiedAt = r.ModifiedAt,
                ArchivedAt = r.ArchivedAt,
            })
            .ToListAsync();
    }

    public async Task<HistoricalRecordResponseDto> GetHistoricalRecord(
        long recordId, 
        DateTime? pointInTime, 
        bool current = false)
    {
        var recordQuery = _context.HistoricalRecords
            .Where(r => r.RecordId == recordId);

        if (current)
        {
            recordQuery = recordQuery.Where(r => r.Current);
        }

        if (pointInTime.HasValue && !current)
        {
            recordQuery = recordQuery
                .Where(r => r.LastUpdatedAt <= pointInTime)
                .OrderByDescending(r => r.LastUpdatedAt);
        }
        
        var record = await recordQuery.FirstOrDefaultAsync();
        
        if (record == null)
        {
            throw new KeyNotFoundException($"Record with id {recordId} not found at point in time {pointInTime}.");
        }

        return new HistoricalRecordResponseDto()
        {
            Id = record.RecordId,
            Uri = record.Uri,
            Properties = record.Properties,
            OriginalId = record.OriginalId,
            Name = record.Name,
            ClassId = record.ClassId,
            ClassName = record.ClassName,
            DataSourceId = record.DataSourceId,
            DataSourceName = record.DataSourceName,
            MappingId = record.MappingId,
            ProjectId = record.ProjectId,
            ProjectName = record.ProjectName,
            Tags = record.Tags,
            CreatedBy = record.CreatedBy,
            CreatedAt = record.CreatedAt,
            ModifiedBy = record.ModifiedBy,
            ModifiedAt = record.ModifiedAt,
            ArchivedAt = record.ArchivedAt
        };
    }

    public async Task<bool> CreateHistoricalRecord(long recordId)
    {
        // insert the appropriate data using insert into select
        // due to the complexity of the query, execute the query
        // using raw SQL instead of via entity framework
        var query = @"
            INSERT INTO historical_records (
                record_id, uri, name, properties, original_id, 
                class_id, mapping_id, data_source_id, project_id, 
                created_by, created_at, 
                last_updated_at, class_name, data_source_name, project_name, tags)
            SELECT r.id, r.uri, r.name, r.properties, r.original_id, 
                   r.class_id, r.mapping_id, r.data_source_id, r.project_id, 
                   r.created_by, r.created_at,
                   r.created_at AS last_updated_at, c.name, d.name, p.name, jsonb_agg(t.name)
            FROM records r
            JOIN record_tags rt ON r.id = rt.record_id
            JOIN tags t ON t.id = rt.tag_id
            JOIN classes c ON c.id = r.class_id
            JOIN data_sources d ON d.id = r.data_source_id
            JOIN projects p ON p.id = r.project_id
            WHERE r.id = @RecordId
            GROUP BY r.id, r.uri, r.name, r.properties, r.original_id, 
                     r.class_id, r.mapping_id, r.data_source_id, r.project_id, 
                     r.created_by, r.created_at, 
                     c.name, d.name, p.name;";

        var recordIdParam = new Npgsql.NpgsqlParameter("@RecordId", recordId);

        var created = await _context.Database.ExecuteSqlRawAsync(query, recordIdParam);

        if (created == 0) // if 0 records were created, assume a failure
        {
            throw new Exception($"Unable to create historical record with id {recordId}");
        }
        
        await _context.SaveChangesAsync();

        return true;
    }
    public async Task<bool> UpdateHistoricalRecord(long recordId)
    {
        // insert the appropriate data using insert into select
        // due to the complexity of the query, execute the query
        // using raw SQL instead of via entity framework
        var query = @"
            INSERT INTO historical_records (
                record_id, uri, name, properties, original_id, 
                class_id, mapping_id, data_source_id, project_id, 
                created_by, created_at, modified_by, modified_at,
                last_updated_at, class_name, data_source_name, project_name, tags)
            SELECT r.id, r.uri, r.name, r.properties, r.original_id, 
                   r.class_id, r.mapping_id, r.data_source_id, r.project_id, 
                   r.created_by, r.created_at, r.modified_by, r.modified_at,
                   r.modified_at AS last_updated_at, c.name, d.name, p.name, jsonb_agg(t.name)
            FROM records r
            JOIN record_tags rt ON r.id = rt.record_id
            JOIN tags t ON t.id = rt.tag_id
            JOIN classes c ON c.id = r.class_id
            JOIN data_sources d ON d.id = r.data_source_id
            JOIN projects p ON p.id = r.project_id
            WHERE r.id = @RecordId
            GROUP BY r.id, r.uri, r.name, r.properties, r.original_id, 
                     r.class_id, r.mapping_id, r.data_source_id, r.project_id, 
                     r.created_by, r.created_at, r.modified_by, r.modified_at,
                     c.name, d.name, p.name;";

        var recordIdParam = new Npgsql.NpgsqlParameter("@RecordId", recordId);

        var updated = await _context.Database.ExecuteSqlRawAsync(query, recordIdParam);

        if (updated == 0) // if 0 records were updated, assume a failure
        {
            throw new Exception($"Unable to update historical record with id {recordId}");
        }
        
        await _context.SaveChangesAsync();

        return true;
    }
    
    public async Task<bool> ArchiveHistoricalRecord(long recordId)
    {
        // insert the appropriate data using insert into select
        // due to the complexity of the query, execute the query
        // using raw SQL instead of via entity framework
        var query = @"
            INSERT INTO historical_records (
                record_id, uri, name, properties, original_id, 
                class_id, mapping_id, data_source_id, project_id, 
                created_by, created_at, modified_by, modified_at, archived_at,
                last_updated_at, class_name, data_source_name, project_name, tags)
            SELECT r.id, r.uri, r.name, r.properties, r.original_id, 
                   r.class_id, r.mapping_id, r.data_source_id, r.project_id, 
                   r.created_by, r.created_at, r.modified_by, r.modified_at, r.archived_at,
                   r.archived_at AS last_updated_at, c.name, d.name, p.name, jsonb_agg(t.name)
            FROM records r
            JOIN record_tags rt ON r.id = rt.record_id
            JOIN tags t ON t.id = rt.tag_id
            JOIN classes c ON c.id = r.class_id
            JOIN data_sources d ON d.id = r.data_source_id
            JOIN projects p ON p.id = r.project_id
            WHERE r.id = @RecordId
            GROUP BY r.id, r.uri, r.name, r.properties, r.original_id, 
                     r.class_id, r.mapping_id, r.data_source_id, r.project_id, 
                     r.created_by, r.created_at, r.modified_by, r.modified_at, r.archived_at,
                     c.name, d.name, p.name;";

        var recordIdParam = new Npgsql.NpgsqlParameter("@RecordId", recordId);

        var archived = await _context.Database.ExecuteSqlRawAsync(query, recordIdParam);

        if (archived == 0) // if 0 records were archived, assume a failure
        {
            throw new Exception($"Unable to archive historical record with id {recordId}");
        }
        
        await _context.SaveChangesAsync();

        return true;
    }
}