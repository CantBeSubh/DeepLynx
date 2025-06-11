using System.Text.Json.Nodes;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.helpers.exceptions;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;


namespace deeplynx.business;

public class RecordBusiness : IRecordBusiness
{
    private readonly DeeplynxContext _context;
    
    // dependant used to trigger downstream soft deletes
    private readonly IEdgeBusiness _edgeBusiness;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordBusiness"/> class.
    /// </summary>
    /// <param name="context">The database context used for the edge operations.</param>
    /// <param name="edgeBusiness">Passed in context of downstream edge objects.</param>
    public RecordBusiness(DeeplynxContext context, IEdgeBusiness edgeBusiness)
    {
        _context = context;
        _edgeBusiness = edgeBusiness;
    }
    
    /// <summary>
    /// Retrieves all records for a specific project and datasource.
    /// </summary>
    /// <param name="projectId">The ID of the project whose records are to be retrieved</param>
    /// <param name="dataSourceId">(Optional) The ID of the datasource by which to filter edges</param>
    /// <returns>A list of records based on the applied filters.</returns>
    public async Task<IEnumerable<RecordResponseDto>> GetAllRecords(long projectId, long? dataSourceId = null)
    {
        var recordQuery = _context.Records
            .Where(r => r.ProjectId == projectId && r.DeletedAt == null);

        if (dataSourceId.HasValue)
        {
            recordQuery = recordQuery.Where(r => r.DataSourceId == dataSourceId);
        }
        
        return await recordQuery
            .Select(r=>new RecordResponseDto()
            {
                Id = r.Id,
                Uri = r.Uri,
                Properties = r.Properties,
                OriginalId = r.OriginalId,
                Name = r.Name,
                CustomId = r.CustomId,
                ClassId = r.ClassId,
                ClassName = r.ClassName,
                DataSourceId = r.DataSourceId,
                ProjectId = r.ProjectId,
                CreatedBy = r.CreatedBy,
                ModifiedBy = r.ModifiedBy,
                ModifiedAt = r.ModifiedAt,
            })
            .ToListAsync();
    }
    
    /// <summary>
    /// Retrieves a specific record by its ID
    /// </summary>
    /// <param name="projectId">The project of the record to retrieve</param>
    /// <param name="dataSourceId">The data source of the record to retrieve</param>
    /// <param name="recordId">The ID of the record to retrieve</param>
    /// <returns>The record in question</returns>
    /// <exception cref="KeyNotFoundException">Returned if record not found</exception>
    public async Task<RecordResponseDto> GetRecord(long projectId, long recordId)
    {
        var record = await _context.Records
            .Where(r => r.Id == recordId && r.ProjectId == projectId && r.DeletedAt == null)
            .FirstOrDefaultAsync();
        
        if (record == null)
        {
            throw new KeyNotFoundException($"Record with id {recordId} not found");
        }

        return new RecordResponseDto
        {
            Id = record.Id,
            Uri = record.Uri,
            Properties = record.Properties,
            OriginalId = record.OriginalId,
            Name = record.Name,
            CustomId = record.CustomId,
            ClassId = record.ClassId,
            ClassName = record.ClassName,
            DataSourceId = record.DataSourceId,
            ProjectId = record.ProjectId,
            CreatedBy = record.CreatedBy,
            ModifiedBy = record.ModifiedBy,
            ModifiedAt = record.ModifiedAt,
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
    public async Task<RecordResponseDto> CreateRecord(long projectId, long dataSourceId, RecordRequestDto dto)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.DeletedAt == null);
        if (project == null)
            throw new KeyNotFoundException($"Project with id {projectId} not found");
        
        var ds = await _context.DataSources
            .FirstOrDefaultAsync(d => d.Id == dataSourceId && d.DeletedAt == null);
        if (ds == null)
            throw new KeyNotFoundException($"DataSource with id {dataSourceId} not found");
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
            ClassName = dto.ClassName,
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
            CustomId = record.CustomId,
            ClassId = record.ClassId,
            ClassName = record.ClassName,
            DataSourceId = record.DataSourceId,
            ProjectId = record.ProjectId,
            CreatedBy = record.CreatedBy,
            ModifiedBy = record.ModifiedBy,
            ModifiedAt = record.ModifiedAt,
        };
    }

    /// <summary>
    /// Updates a record with new information
    /// </summary>
    /// <param name="projectId">The ID of the project to which the record belongs</param>
    /// <param name="dataSourceId">The ID of the datasource to which the record belongs</param>
    /// <param name="recordId">The ID of the record to be updated</param>
    /// <param name="dto">The data transfer object containing details on the record to be updated</param>
    /// <returns>The newly updated metadata record</returns>
    /// <exception cref="KeyNotFoundException">Returned if record to be updated is not found</exception>
    public async Task<RecordResponseDto> UpdateRecord(long projectId, long recordId, RecordRequestDto dto)
    {
        var record= await _context.Records.FindAsync(recordId);
        if (record == null || record.ProjectId != projectId || record.DeletedAt != null)
        {
            throw new KeyNotFoundException($"Record with id {recordId} not found");
        }
        record.Uri = dto.Uri;
        record.Properties = dto.Properties.ToString()!;
        record.OriginalId = dto.OriginalId;
        record.Name = dto.Name;
        record.ClassName = dto.ClassName;
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
            CustomId = record.CustomId,
            ClassId = record.ClassId,
            ClassName = record.ClassName,
            DataSourceId = record.DataSourceId,
            ProjectId = record.ProjectId,
            CreatedBy = record.CreatedBy,
            ModifiedBy = record.ModifiedBy,
            ModifiedAt = record.ModifiedAt,
        };
        
    }

    /// <summary>
    /// Delete a metadata record and any downstream edges.
    /// </summary>
    /// <param name="projectId">The project to which the record belongs</param>
    /// <param name="recordId">The record in question</param>
    /// <param name="force">If force is true, permanently delete the record. Otherwise, soft delete</param>
    /// <returns>Boolean indicating record was deleted</returns>
    /// <exception cref="KeyNotFoundException">Returned if the record to delete was not found.</exception>
    /// <exception cref="ProjectDependencyDeletionException">Returned if downstream deletions failed.</exception>
    public async Task<bool> DeleteRecord(long projectId, long recordId, bool force=false)
    {
        var record = await _context.Records.FindAsync(recordId);
        
        if (record == null || record.ProjectId != projectId || record.DeletedAt != null)
        {
            throw new KeyNotFoundException($"Record with id {recordId} not found");
        }

        if (force)
        {
            // hard delete
            _context.Records.Remove(record);
            await _context.SaveChangesAsync();
        }
        else
        {
            // start a database transaction to ensure deletion changes are rolled back if errors occur
            var transaction = await _context.Database.BeginTransactionAsync();
            
            // define a list of lambda functions for bulk deletes to sequentially iterate
            // so as not to block the thread of our lone database context
            // there is only one downstream task currently, but there may be more in the future
            var softDeleteTasks = new List<Func<Task<bool>>>
            {
                () => _edgeBusiness.BulkSoftDeleteEdges("record", [recordId])
            };


            foreach (var task in softDeleteTasks)
            {
                bool result = await task();
                if (!result)
                {
                    // rollback the transaction and then throw an error
                    await transaction.RollbackAsync();
                    throw new ProjectDependencyDeletionException("An error occurred during deletion of downstream record dependants.");
                }
            }
                
            // soft delete
            record.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            _context.Records.Update(record);

            // save changes and commit the transaction to close it
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        
        return true;
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
    /// Bulk Soft Delete records by a specific upstream domain. Used to avoid repeating functions.
    /// </summary>
    /// <param name="domainType">The type of domain which is calling this function</param>
    /// <param name="domainIds">The ID of the upstream domain calling this function</param>
    /// <param name="transaction">(Optional) a transaction passed in from the parent to ensure ACID compliance</param>
    /// <returns>Boolean true on successful deletion</returns>
    public async Task<bool> BulkSoftDeleteRecords(
        string domainType, 
        IEnumerable<long> domainIds, 
        IDbContextTransaction? transaction)
    {
        try
        {
            var recordQuery = _context.Records.Where(r => r.DeletedAt == null);

            if (domainType == "project")
            {
                recordQuery = recordQuery.Where(r => domainIds.Contains(r.ProjectId));
            }

            if (domainType == "dataSource")
            {
                recordQuery = recordQuery.Where(r => domainIds.Contains(r.DataSourceId));
            }
            else if (domainType == "class")
            {
                recordQuery = recordQuery.Where(r => r.ClassId.HasValue && domainIds.Contains(r.ClassId.Value));
            }
            
            var records = await recordQuery.ToListAsync();
            
            // start a database transaction to ensure deletion changes are rolled back if errors occur
            var commit = false; // variable to indicate whether we can commit or if parent should commit transaction
            if (transaction == null)
            {
                commit = true;
                transaction = await _context.Database.BeginTransactionAsync();
            }
            
            // define a list of lambda functions for bulk deletes to sequentially iterate
            // so as not to block the thread of our lone database context
            // there is only one downstream task currently, but there may be more in the future
            var softDeleteTasks = new List<Func<Task<bool>>>
            {
                () => _edgeBusiness.BulkSoftDeleteEdges("record", records.Select(r => r.Id))
            };
    
            // execute all downstream update tasks (just edges currently)
            foreach (var task in softDeleteTasks)
            {
                bool result = await task();
                if (!result)
                {
                    // rollback the transaction and then throw an error
                    await transaction.RollbackAsync();
                    throw new ProjectDependencyDeletionException("An error occurred during deletion of downstream record dependants.");
                }
            }
                
            // update all records with the new deletion date
            foreach (var r in records)
            {
                r.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            }
                    
            // save changes and close the transaction
            await _context.SaveChangesAsync();
            if (commit)
            {
                await transaction.CommitAsync();
            }
            return true;
                
        }
        catch (Exception exc)
        {
            var id_list = string.Join(",", domainIds);
            var message = $"An error occurred while deleting roles for domain {domainType} with id(s) {id_list}: {exc}";
            NLog.LogManager.GetCurrentClassLogger().Error(message);
            return false;
        }
    }
}
