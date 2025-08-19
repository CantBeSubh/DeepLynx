using deeplynx.models;

namespace deeplynx.interfaces;

public interface IQueryBusiness
{
    Task<IEnumerable<HistoricalRecordResponseDto>> Search(string query);

    IEnumerable<HistoricalRecordResponseDto> BuildAQuery(CustomQueryRequestDto[] request);

}