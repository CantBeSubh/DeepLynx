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
    public async Task<IEnumerable<Record>> GetAllRecords(long projectId, long dataSourceId)
    {
        return await _context.Records
            .Where(r => r.ProjectId == projectId && r.DataSourceId == dataSourceId)
            .ToListAsync();
    }
    public async Task<Record> GetRecord(long projectId, long dataSourceId, long recordId)
    {
        return await _context.Records
                   .FirstOrDefaultAsync(r =>
                       r.ProjectId == projectId && r.DataSourceId == dataSourceId && r.Id == recordId)
               ?? throw new KeyNotFoundException("Record not found.");
    }

    public async Task<Record> CreateRecord(long projectId, long dataSourceId, RecordRequestDto dto)
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
            ClassName = dto.ClassName,
            ClassId = dto.ClassId,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            CreatedBy = null  // TODO: Implement user ID here when JWT tokens are ready
        };

        _context.Records.Add(record);
        await _context.SaveChangesAsync();

        return record;
    }

    public async Task<Record> UpdateRecord(long projectId, long dataSourceId, long recordId, RecordRequestDto dto)
    {
        var record = await GetRecord(projectId, dataSourceId, recordId);

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

        return record;
    }

    public async Task<bool> DeleteRecord(long projectId, long dataSourceId, long recordId)
    {
        var record = await GetRecord(projectId, dataSourceId, recordId);

        _context.Records.Remove(record);
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
}
