using deeplynx.models;

namespace deeplynx.interfaces;

public interface IQueryBusiness
{
    Task<IEnumerable<HistoricalRecordResponseDto>> Search(string query, long[] projectIds);

    Task<IEnumerable<HistoricalRecordResponseDto>> QueryBuilder(CustomQueryDtos.CustomQueryRequestDto[] request,
        long[] projectIds, string? textSearch);

    Task<IEnumerable<HistoricalRecordResponseDto>> GetRecentlyAddedRecords(
        long[] projectId);

    Task<IEnumerable<HistoricalRecordResponseDto>> GetMultiProjectRecords(long[] projects, bool hideArchived);
}