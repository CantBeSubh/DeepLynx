using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;
using deeplynx.helpers;

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
        var record = new Record
        {
            ProjectId = projectId,
            DataSourceId = dataSourceId,
            Properties = dto.Properties.ToString()!,
            Name = JsonHelper.ExtractStringOrJson(dto.Name),
            OriginalId = JsonHelper.ExtractStringOrJson(dto.original_id),
            ClassName = dto.ClassName,
            CreatedAt = DateTime.UtcNow.ToLocalTime(),
            ModifiedAt = DateTime.UtcNow.ToLocalTime()
        };

        _context.Records.Add(record);
        await _context.SaveChangesAsync();

        return record;
    }

    public async Task<Record> UpdateRecord(long projectId, long dataSourceId, long recordId, RecordRequestDto dto)
    {
        var record = await GetRecord(projectId, dataSourceId, recordId);

        record.Properties = dto.Properties.ToString()!;
        record.Name = JsonHelper.ExtractStringOrJson(dto.Name);
        record.OriginalId = JsonHelper.ExtractStringOrJson(dto.original_id);
        record.ClassName = dto.ClassName;
        record.ModifiedAt = DateTime.UtcNow.ToLocalTime();

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
}
