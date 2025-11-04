using deeplynx.models;

namespace deeplynx.interfaces;

public interface IOauthApplicationBusiness
{
    Task<IEnumerable<OauthApplicationResponseDto>> GetAllOauthApplications(bool hideArchived = true);
    Task<OauthApplicationResponseDto> GetOauthApplication(long applicationId, bool hideArchived = true);
    Task<OauthApplicationSecureResponseDto> CreateOauthApplication(
        CreateOauthApplicationRequestDto requestDto, long userId);
    Task<OauthApplicationResponseDto> UpdateOauthApplication(
        long applicationId, UpdateOauthApplicationRequestDto requestDto, long userId);
    Task<bool> ArchiveOauthApplication(long applicationId, long userId);
    Task<bool> UnarchiveOauthApplication(long applicationId, long userId);
    Task<bool> DeleteOauthApplication(long applicationId, long userId);
}