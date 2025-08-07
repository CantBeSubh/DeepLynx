using deeplynx.models;

namespace deeplynx.interfaces;

public interface IQueryBusiness
{
    Task<IEnumerable<HistoricalRecordResponseDto>> FilterRecords(string[] filterRequest);
    IEnumerable<HistoricalRecordResponseDto> BuildQuery(AdvancedQueryRequestDto[] components);
    
    Task<IEnumerable<HistoricalRecordResponseDto>> Search(string query);
    
}