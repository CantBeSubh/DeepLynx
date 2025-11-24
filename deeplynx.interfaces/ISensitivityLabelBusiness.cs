using deeplynx.models;

namespace deeplynx.interfaces;

public interface ISensitivityLabelBusiness
{
    Task<SensitivityLabelResponseDto> UpdateSensitivityLabel(long currentUserId, long labelId, long? projectId,
        long organizationId, UpdateSensitivityLabelRequestDto dto);

    Task<bool> ArchiveSensitivityLabel(long currentUserId, long labelId, long? projectId, long organizationId);
    Task<bool> UnarchiveSensitivityLabel(long currentUserId, long labelId, long? projectId, long organizationId);
    Task<bool> DeleteSensitivityLabel(long currentUserId, long labelId, long? projectId, long organizationId);

    Task<SensitivityLabelResponseDto> CreateSensitivityLabel(
        long currentUserId, CreateSensitivityLabelRequestDto dto, long? projectId, long organizationId);

    Task<IEnumerable<SensitivityLabelResponseDto>> GetAllSensitivityLabels(
        long[]? projectIds, long organizationId, bool hideArchived = true);

    Task<SensitivityLabelResponseDto> GetSensitivityLabel(long labelId, long? projectId, long organizationId,
        bool hideArchived = true);
}