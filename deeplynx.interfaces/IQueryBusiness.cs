using deeplynx.models;

namespace deeplynx.interfaces;

public interface IQueryBusiness
{
    Task<IEnumerable<HistoricalRecordResponseDto>> Search(string query, long[] projectIds);

    IEnumerable<HistoricalRecordResponseDto> QueryBuilder(CustomQueryRequestDto[] request, long[] projectIds,
        string? textSearch);

    Task<List<ClassResponseDto>> GetAllClasses(long[] projectIds, bool hideArchived);

    Task<List<DataSourceResponseDto>> GetAllDataSources(long[] projectIds, bool hideArchived);

    Task<List<TagResponseDto>> GetAllTags(long[] projectIds, bool hideArchived);

    bool SaveSearch(long userId, string alias, string textSearch, CustomQueryRequestDto[] filters, bool favorite);
}