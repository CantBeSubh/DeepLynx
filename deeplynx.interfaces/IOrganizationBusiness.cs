using deeplynx.models;

namespace deeplynx.interfaces;

public interface IOrganizationBusiness
{
    Task<IEnumerable<OrganizationResponseDto>> GetAllOrganizations(bool hideArchived = true);
    Task<OrganizationResponseDto> GetOrganization(long organizationId, bool hideArchived = true);
    Task<OrganizationResponseDto> CreateOrganization(CreateOrganizationRequestDto dto);
    Task<OrganizationResponseDto> UpdateOrganization(long organizationId, UpdateOrganizationRequestDto dto);
    Task<bool> ArchiveOrganization(long organizationId);
    Task<bool> UnarchiveOrganization(long organizationId);
    Task<bool> DeleteOrganization(long organizationId);
    Task<bool>  AddUserToOrganization(long organizationId, long userId, bool isAdmin = false);
    Task<bool>  UpdateOrganizationAdminStatus(long organizationId, long userId, bool isAdmin = false);
    Task<bool>  RemoveUserFromOrganization(long organizationId, long userId);
}