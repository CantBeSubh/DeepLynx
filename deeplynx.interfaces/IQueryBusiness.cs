using deeplynx.models;

namespace deeplynx.interfaces;

public interface IQueryBusiness
{
    Task<IEnumerable<HistoricalRecordResponseDto>> Search(string query);

    IEnumerable<HistoricalRecordResponseDto> QueryBuilder(CustomQueryRequestDto[] request,  string? textSearch);

}