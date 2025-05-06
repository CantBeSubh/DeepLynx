using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.business;

public class RecordMappingBusiness : IRecordMappingBusiness
{
    private readonly DeeplynxContext _context;

    public RecordMappingBusiness(DeeplynxContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RecordMapping>> GetAllRecordMappings(long projectId)
    {
        return await _context.RecordMappings
            .Where(rp => rp.ProjectId == projectId).ToListAsync();
    }

    public async Task<RecordMapping> GetRecordMapping(long mappingId)
    {
        return await _context.RecordMappings
            .FirstOrDefaultAsync(rp => rp.Id == mappingId);
    }

    public async Task<RecordMapping> CreateRecordMapping(
        long projectId, 
        RecordMappingRequestDto dto)
    {
        var mapping = new RecordMapping
        {
            RecordParams = dto.RecordParams.ToString(),
            ProjectId = projectId,
            ClassId = dto.ClassId,
            TagId = dto.TagId
        };
        
        _context.RecordMappings.Add(mapping);
        await _context.SaveChangesAsync();
        
        return mapping;
    }

    public async Task<RecordMapping> UpdateRecordMapping(
        long projectId,
        long mappingId,
        RecordMappingRequestDto dto)
    {
        var mapping = await GetRecordMapping(mappingId);

        mapping.RecordParams = dto.RecordParams.ToString();
        mapping.ProjectId = projectId;
        mapping.ClassId = dto.ClassId;
        mapping.TagId = dto.TagId;
        
        _context.RecordMappings.Add(mapping);
        await _context.SaveChangesAsync();
        
        return mapping;
    }

    public async Task<bool> DeleteRecordMapping(long mappingId)
    {
        var mapping = await GetRecordMapping(mappingId);
        
        _context.RecordMappings.Remove(mapping);
        await _context.SaveChangesAsync();
        
        return true;
    }
}