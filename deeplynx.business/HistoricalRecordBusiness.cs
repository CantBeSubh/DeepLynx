using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.business;

public class HistoricalRecordBusiness : IHistoricalRecordBusiness
{
    private readonly DeeplynxContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="HistoricalRecordBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context used for the record operations.</param>
    public HistoricalRecordBusiness(DeeplynxContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all Historical Records for a specific project and datasource
    /// </summary>
    /// <param name="projectId">The ID of the project whose records are to be retrieved</param>
    /// <param name="dataSourceId">(Optional) The ID of the datasource by which to filter records</param>
    /// <param name="pointInTime">(Optional) Find the most current records that existed before this point in time</param>
    /// <param name="hideArchived">(Optional) Flag indicating whether to hide archived records from the result.</param>
    /// <param name="current">(Optional) Find only the most current records. Overrides point in time.</param>
    /// <returns>An array of records</returns>
    /// TODO: create an endpoint for this
    public async Task<IEnumerable<HistoricalRecordResponseDto>> GetAllHistoricalRecords(
        long projectId,
        long? dataSourceId = null,
        DateTime? pointInTime = null,
        bool hideArchived = true,
        bool current = true)
    {
        var recordQuery = _context.HistoricalRecords
            .Where(r => r.ProjectId == projectId);

        if (dataSourceId.HasValue)
        {
            recordQuery = recordQuery.Where(r => r.DataSourceId == dataSourceId);
        }

        if (current)
        {
            recordQuery = recordQuery.Where(r => r.Current);
        }

        if (hideArchived)
        {
            recordQuery = recordQuery.Where(r => r.ArchivedAt == null);
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
            .Select(r => new HistoricalRecordResponseDto()
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
                ArchivedAt = r.ArchivedAt
            })
            .ToListAsync();
    }
    
    /// <summary>
    /// Show the historical updates of a specific record
    /// </summary>
    /// <param name="recordId">The ID of the record to list history for</param>
    /// <returns>An array of record instances for the given record</returns>
    /// TODO: create an endpoint for this
    public async Task<IEnumerable<HistoricalRecordResponseDto>> GetHistoryForRecord(long recordId)
    {
        return await _context.HistoricalRecords
            .Where(r => r.RecordId == recordId)
            .OrderByDescending(r => r.LastUpdatedAt)
            .Select(r => new HistoricalRecordResponseDto()
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
                ArchivedAt = r.ArchivedAt
            })
            .ToListAsync();
    }

    /// <summary>
    /// Find an record at a given point in time
    /// </summary>
    /// <param name="recordId">The ID of the record to retrieve</param>
    /// <param name="pointInTime">(Optional) Find the most current record that existed before this point in time</param>
    /// <param name="hideArchived">(Optional) Flag indicating whether to hide archived records from the result.</param>
    /// <param name="current">(Optional) Find only the most current record. Overrides point in time.</param>
    /// <returns>A record that matches the applied filters.</returns>
    /// <exception cref="KeyNotFoundException">Returned if record not found</exception>
    /// TODO: create an endpoint for this
    public async Task<HistoricalRecordResponseDto> GetHistoricalRecord(
        long recordId,
        DateTime? pointInTime,
        bool hideArchived = true,
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

        if (hideArchived)
        {
            recordQuery = recordQuery.Where(r => r.ArchivedAt == null);
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
}