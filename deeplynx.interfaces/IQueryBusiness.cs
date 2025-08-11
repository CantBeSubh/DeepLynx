using deeplynx.models;

namespace deeplynx.interfaces;

public interface IQueryBusiness
{
    IEnumerable<HistoricalRecordResponseDto> BuildQuery(AdvancedQueryRequestDto[] components);
    
    Task<IEnumerable<HistoricalRecordResponseDto>> Search(string query);
    
}