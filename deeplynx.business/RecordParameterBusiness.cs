using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.business;

public class RecordParameterBusiness : IRecordParameterBusiness
{
    private readonly DeeplynxContext _context;

    public RecordParameterBusiness(DeeplynxContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RecordParameter>> GetAllRecordParameters(long projectId)
    {
        return await _context.RecordParameters
            .Where(rp => rp.ProjectId == projectId).ToListAsync();
    }

    public async Task<RecordParameter> GetRecordParameter(long recordParameterId)
    {
        return await _context.RecordParameters
            .FirstOrDefaultAsync(rp => rp.Id == recordParameterId);
    }

    public async Task<RecordParameter> CreateRecordParameter(
        long projectId, 
        RecordParameterRequestDto dto)
    {
        var recordParameter = new RecordParameter
        {
            RecordParams = dto.RecordParams.ToString(),
            ProjectId = projectId,
            ClassId = dto.ClassId,
            TagId = dto.TagId
        };
        
        _context.RecordParameters.Add(recordParameter);
        await _context.SaveChangesAsync();
        
        return recordParameter;
    }

    public async Task<RecordParameter> UpdateRecordParameter(
        long projectId,
        long recordParameterId,
        RecordParameterRequestDto dto)
    {
        var recordParameter = await GetRecordParameter(recordParameterId);

        recordParameter.RecordParams = dto.RecordParams.ToString();
        recordParameter.ProjectId = projectId;
        recordParameter.ClassId = dto.ClassId;
        recordParameter.TagId = dto.TagId;
        
        _context.RecordParameters.Add(recordParameter);
        await _context.SaveChangesAsync();
        
        return recordParameter;
    }

    public async Task<bool> DeleteRecordParameter(long recordParameterId)
    {
        var recordParameter = await GetRecordParameter(recordParameterId);
        
        _context.RecordParameters.Remove(recordParameter);
        await _context.SaveChangesAsync();
        
        return true;
    }
}