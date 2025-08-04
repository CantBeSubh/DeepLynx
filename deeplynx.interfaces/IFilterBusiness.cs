using deeplynx.models;

namespace deeplynx.interfaces;

public interface IFilterBusiness
{
    Task<IEnumerable<HistoricalRecordResponseDto>> FilterRecords(string[] filterRequest);
}