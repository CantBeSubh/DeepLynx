using deeplynx.models;

namespace deeplynx.interfaces;

public interface IFilterBusiness
{
    Task<IEnumerable<RecordResponseDto>> FilterRecords(string[] filterRequest);
}