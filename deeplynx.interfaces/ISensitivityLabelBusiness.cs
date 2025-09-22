using deeplynx.models;

namespace deeplynx.interfaces;

public interface ISensitivityLabelBusiness
{
    Task<IEnumerable<SensitivityLabelResponseDto>> GetAllSensitivityLabels(
        long? projectId, long? organizationId, bool hideArchived = true);
    Task<SensitivityLabelResponseDto> GetSensitivityLabel(long labelId, bool hideArchived = true);
    Task<SensitivityLabelResponseDto> CreateSensitivityLabel(
        CreateSensitivityLabelRequestDto dto, long? projectId, long? organizationId);
    Task<SensitivityLabelResponseDto> UpdateSensitivityLabel(long labelId, UpdateSensitivityLabelRequestDto dto);
    Task<bool> ArchiveSensitivityLabel(long labelId);
    Task<bool> UnarchiveSensitivityLabel(long labelId);
    Task<bool> DeleteSensitivityLabel(long labelId);
}