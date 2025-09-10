using deeplynx.models;

namespace deeplynx.interfaces;

public interface IQueryBusiness
{
    Task<IEnumerable<FullTextQueryResponseDto>> Search(string query);

    IEnumerable<HistoricalRecordResponseDto> QueryBuilder(CustomQueryRequestDto[] request,  string? textSearch);
    
    Task<List<ClassResponseDto>> GetAllClasses(long[] projectIds,  bool hideArchived);

    Task<List<DataSourceResponseDto>> GetAllDataSources(long[] projectIds, bool hideArchived);

    Task<List<TagResponseDto>> GetAllTags(long[] projectIds, bool hideArchived);

}