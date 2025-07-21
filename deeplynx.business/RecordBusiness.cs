using System.Text.Json.Nodes;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.helpers.exceptions;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.business;

public class RecordBusiness : IRecordBusiness
{
    private readonly DeeplynxContext _context;
    
    // dependent used to trigger downstream actions
    private readonly IHistoricalRecordBusiness _historicalRecordBusiness;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context used for the record operations.</param>
    /// <param name="edgeBusiness">Passed in context of historical record objects.</param>
    public RecordBusiness(DeeplynxContext context, IHistoricalRecordBusiness historicalRecordBusiness)
    {
        _context = context;
        _historicalRecordBusiness = historicalRecordBusiness;
    }
    
    /// <summary>
    /// Retrieves all records for a specific project and datasource.
    /// </summary>
    /// <param name="projectId">The ID of the project whose records are to be retrieved</param>
    /// <param name="dataSourceId">(Optional) The ID of the datasource by which to filter records</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived records from the result</param>
    /// <returns>A list of records based on the applied filters.</returns>
    public async Task<IEnumerable<HistoricalRecordResponseDto>> GetAllRecords(
        long projectId, long? dataSourceId, bool hideArchived)
    {
        DoesProjectExist(projectId, hideArchived);
        return await _historicalRecordBusiness.GetAllHistoricalRecords(
            projectId, dataSourceId, null, hideArchived, true);
    }
    
    /// <summary>
    /// Retrieves a specific record by its ID
    /// </summary>
    /// <param name="projectId">The project of the record to retrieve</param>
    /// <param name="recordId">The ID of the record to retrieve</param>
    /// <param name="hideArchived">Flag indicating whether to hide archived records from the result</param>
    /// <returns>The record in question</returns>
    /// <exception cref="KeyNotFoundException">Returned if record not found</exception>
    public async Task<HistoricalRecordResponseDto> GetRecord(
        long projectId, long recordId, bool hideArchived)
    {
        DoesProjectExist(projectId, hideArchived);
        return await _historicalRecordBusiness.GetHistoricalRecord(
            recordId, null, hideArchived, true);
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
    public async Task<RecordResponseDto> CreateRecord(long projectId, long dataSourceId, RecordRequestDto dto)
    {
       DoesProjectExist(projectId);
       DoesDataSourceExist(dataSourceId);
        
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
            ClassId = dto.ClassId,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            CreatedBy = null  // TODO: Implement user ID here when JWT tokens are ready
        };

        _context.Records.Add(record);
        await _context.SaveChangesAsync();

        return new RecordResponseDto
        {
            Id = record.Id,
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
        };
    }
    
    /// <summary>
    /// Create new records
    /// </summary>
    /// <param name="projectId">The ID of the project under which to create the record</param>
    /// <param name="dataSourceId">The ID of the data source under which to create the record</param>
    /// <param name="bulkDto">The data transfer object containing details on the records to be created</param>
    /// <returns>The newly created metadata record</returns>
    /// <exception cref="KeyNotFoundException">Returned if the project or datasource are not found</exception>
    /// <exception cref="Exception">Returned if the metadata is too deeply nested</exception>
    public async Task<BulkRecordResponseDto> BulkCreateRecords(long projectId, long dataSourceId, BulkRecordRequestDto bulkDto)
    {
       DoesProjectExist(projectId);
       DoesDataSourceExist(dataSourceId);
        
       var records = new List<Record>();
       var recordResponses = new List<RecordResponseDto>();
       foreach (var dto in bulkDto.Records)
       {
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
               ClassId = dto.ClassId,
               CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
               CreatedBy = null  // TODO: Implement user ID here when JWT tokens are ready
           };
           records.Add(record);
       }

       await _context.Records.AddRangeAsync(records);
       await _context.SaveChangesAsync();

        foreach (var record in records)
        {
            var recordResponse = new RecordResponseDto
            {
                Id = record.Id,
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
            };
            
            recordResponses.Add(recordResponse);
        }

        return new BulkRecordResponseDto()
        {
            Records = recordResponses,
        };
    }

    /// <summary>
    /// Updates a record with new information
    /// </summary>
    /// <param name="projectId">The ID of the project to which the record belongs</param>
    /// <param name="recordId">The ID of the record to be updated</param>
    /// <param name="dto">The data transfer object containing details on the record to be updated</param>
    /// <returns>The newly updated metadata record</returns>
    /// <exception cref="KeyNotFoundException">Returned if record to be updated is not found</exception>
    public async Task<RecordResponseDto> UpdateRecord(long projectId, long recordId, RecordRequestDto dto)
    {
        DoesProjectExist(projectId);
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
        
        record.Uri = dto.Uri;
        record.Properties = dto.Properties.ToString()!;
        record.OriginalId = dto.OriginalId;
        record.Name = dto.Name;
        record.ClassId = dto.ClassId;
        record.ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        record.ModifiedBy = null; // TODO: Implement user ID here when JWT tokens are ready
        
        // TODO: check property depth like we do on create
        
        _context.Records.Update(record);
        await _context.SaveChangesAsync();
        
        return new RecordResponseDto
        {
            Id = record.Id,
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
        DoesProjectExist(projectId);
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
        DoesProjectExist(projectId);
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
        DoesProjectExist(projectId);
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
    /// Private method used to calculate json depth of properties (should be <3)
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
