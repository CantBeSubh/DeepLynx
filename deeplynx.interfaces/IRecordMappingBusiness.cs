using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.interfaces;

public interface IRecordParameterBusiness
{
    Task<IEnumerable<RecordParameter>> GetAllRecordParameters(long projectId);
    Task<RecordParameter> GetRecordParameter(long recordParameterId);
    Task<RecordParameter> CreateRecordParameter(long projectId, RecordParameterRequestDto dto);
    Task<RecordParameter> UpdateRecordParameter(long projectId, long recordParamId, RecordParameterRequestDto dto);
    Task<bool> DeleteRecordParameter(long recordParamId);
}