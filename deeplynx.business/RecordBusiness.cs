using System.Text.Json.Nodes;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;


namespace deeplynx.business;

public class RecordBusiness : IRecordBusiness
{
    private readonly DeeplynxContext _context;

    public RecordBusiness(DeeplynxContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<RecordResponseDto>> GetAllRecords(long projectId, long dataSourceId)
    {
        return await _context.Records
            .Where(r => r.ProjectId == projectId && r.DataSourceId == dataSourceId && r.DeletedAt == null)
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
    public async Task<RecordResponseDto> GetRecord(long projectId, long dataSourceId, long recordId)
    {
        var record = await _context.Records
            .Where(r => r.Id == recordId && r.ProjectId == projectId && r.DataSourceId == dataSourceId &&
                        r.DeletedAt == null).FirstOrDefaultAsync();
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

    public async Task<RecordResponseDto> UpdateRecord(long projectId, long dataSourceId, long recordId, RecordRequestDto dto)
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

    public async Task<bool> DeleteRecord(long projectId, long dataSourceId, long recordId)
    {
        var record = await _context.Records.FindAsync(recordId);
        if (record == null || record.ProjectId != projectId || record.DeletedAt != null)
        {
            throw new KeyNotFoundException($"Record with id {recordId} not found");
        }
        record.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        _context.Records.Update(record);
        await _context.SaveChangesAsync();
        return true;
    }

    public int CalculateJsonMaxDepth(JsonNode node)
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
    /// <param name="domainId">The ID of the upstream domain calling this function</param>
    /// <returns>Boolean true on successful deletion</returns>
    public async Task<bool> BulkSoftDeleteRecords(string domainType, long domainId)
    {
        try
        {
            var recordQuery = _context.Records.Where(r => r.DeletedAt == null);

            if (domainType == "project")
            {
                recordQuery = recordQuery.Where(r => r.ProjectId == domainId);
            }
                    
            var records = await recordQuery.ToListAsync();
                
            foreach (var r in records)
            {
                r.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            }
                
            await _context.SaveChangesAsync();
            return true;
                
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while deleting roles for domain {domainType} with id {domainId}: {exc}";
            NLog.LogManager.GetCurrentClassLogger().Error(message);
            return false;
        }
    }
}
