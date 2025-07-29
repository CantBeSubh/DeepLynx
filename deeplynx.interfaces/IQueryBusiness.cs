using deeplynx.models;

namespace deeplynx.interfaces;

public interface IQueryBusiness
{
    Task<IEnumerable<RecordResponseDto>> FilterRecords(string[] filterRequest);
    IEnumerable<HistoricalRecordResponseDto> BuildQuery(AdvancedQueryRequestDto[] components);
}