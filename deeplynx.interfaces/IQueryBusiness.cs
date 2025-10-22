using deeplynx.models;

namespace deeplynx.interfaces;

public interface IQueryBusiness
{
    Task<IEnumerable<HistoricalRecordResponseDto>> Search(string query, long[] projectIds);

    Task<IEnumerable<HistoricalRecordResponseDto>> QueryBuilder(CustomQueryRequestDto[] request, long[] projectIds, string? textSearch);
    
    Task<List<ClassResponseDto>> GetAllClasses(long[] projectIds,  bool hideArchived);

    Task<List<DataSourceResponseDto>> GetAllDataSources(long[] projectIds, bool hideArchived);

    Task<List<TagResponseDto>> GetAllTags(long[] projectIds, bool hideArchived);

}