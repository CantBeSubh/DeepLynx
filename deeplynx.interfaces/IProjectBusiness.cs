using deeplynx.models;

namespace deeplynx.interfaces;

public interface IProjectBusiness
{
    Task<IEnumerable<ProjectResponseDto>> GetAllProjects(long userId, long? organizationId, bool hideArchived = true);
    Task<ProjectResponseDto> GetProject(long projectId, bool hideArchived = true);
    Task<ProjectResponseDto> CreateProject(long currentUserId, long organizationId, CreateProjectRequestDto dto);
    Task<ProjectResponseDto> UpdateProject(long currentUserId, long projectId, UpdateProjectRequestDto dto);
    Task<bool> DeleteProject(long projectId);
    Task<bool> ArchiveProject(long currentUserId, long projectId);
    Task<bool> UnarchiveProject(long currentUserId, long projectId);
    Task<ProjectStatResponseDto> GetProjectStats(long projectId);
    Task<IEnumerable<ProjectMemberResponseDto>> GetProjectMembers(long projectId);
    Task<bool> AddMemberToProject(long projectId, long? roleId, long? userId, long? groupId);
    Task<bool> UpdateProjectMemberRole(long projectId, long roleId, long? userId, long? groupId);
    Task<bool> RemoveMemberFromProject(long projectId, long? userId, long? groupId);
}