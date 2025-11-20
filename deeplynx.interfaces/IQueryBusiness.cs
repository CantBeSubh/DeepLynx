using deeplynx.models;

namespace deeplynx.interfaces;

public interface IQueryBusiness
{
    Task<IEnumerable<HistoricalRecordResponseDto>> Search(string query, long[] projectIds);

    Task<IEnumerable<HistoricalRecordResponseDto>> QueryBuilder(CustomQueryDtos.CustomQueryRequestDto[] request,
        long[] projectIds, string? textSearch);

    Task<List<ClassResponseDto>> GetAllClasses(long[] projectIds, bool hideArchived);

    Task<List<DataSourceResponseDto>> GetAllDataSources(long[] projectIds, bool hideArchived);

    Task<List<TagResponseDto>> GetAllTags(long[] projectIds, bool hideArchived);

    Task<bool> SaveSearch(long userId, string alias, string textSearch, CustomQueryDtos.CustomQueryRequestDto[] filters,
        bool favorite);

    Task<List<CustomQueryDtos.CustomQueryResponseDto>> GetSavedSearches(long userId);

    Task<IEnumerable<HistoricalRecordResponseDto>> GetRecentlyAddedRecords(
        long[] projectId);

    Task<IEnumerable<HistoricalRecordResponseDto>> GetMultiProjectRecords(long[] projects, bool hideArchived);
}