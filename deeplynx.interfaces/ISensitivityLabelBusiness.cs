using deeplynx.models;

namespace deeplynx.interfaces;

public interface ISensitivityLabelBusiness
{
    Task<IEnumerable<SensitivityLabelResponseDto>> GetAllSensitivityLabels(
        long? projectId, long? organizationId, bool hideArchived = true);
    Task<SensitivityLabelResponseDto> GetSensitivityLabel(long labelId, bool hideArchived = true);
    Task<SensitivityLabelResponseDto> CreateSensitivityLabel(
        long currentUserId, CreateSensitivityLabelRequestDto dto, long? projectId, long? organizationId);
    Task<SensitivityLabelResponseDto> UpdateSensitivityLabel(long currentUserId, long labelId, UpdateSensitivityLabelRequestDto dto);
    Task<bool> ArchiveSensitivityLabel(long currentUserId, long labelId);
    Task<bool> UnarchiveSensitivityLabel(long currentUserId, long labelId);
    Task<bool> DeleteSensitivityLabel(long labelId);
}