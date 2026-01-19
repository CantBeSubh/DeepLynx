using deeplynx.models;

namespace deeplynx.interfaces;

public interface IQueryBusiness
{
    Task<IEnumerable<HistoricalRecordResponseDto>> Search(string query, long organizationId, long[] projectIds);

    Task<IEnumerable<HistoricalRecordResponseDto>> QueryBuilder(CustomQueryDtos.CustomQueryRequestDto[] request,
        long organizationId,
        long[] projectIds, string? textSearch);

    Task<IEnumerable<HistoricalRecordResponseDto>> GetRecentlyAddedRecords(long organizationId,
        long[] projectId);

    Task<IEnumerable<HistoricalRecordResponseDto>> GetMultiProjectRecords(long organizationId, long[] projects,
        bool hideArchived);
}