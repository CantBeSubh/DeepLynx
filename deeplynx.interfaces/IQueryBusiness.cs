using deeplynx.models;

namespace deeplynx.interfaces;

public interface IQueryBusiness
{
    IEnumerable<HistoricalRecordResponseDto> BuildQuery(string initialQuery, CustomQueryRequestDto[] queryRequests);
    
    Task<IEnumerable<HistoricalRecordResponseDto>> Search(string query);
    
}