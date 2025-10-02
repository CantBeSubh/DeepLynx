using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.interfaces;

public interface IProjectBusiness
{
    Task<IEnumerable<ProjectResponseDto>> GetAllProjects(long? organizationId, bool hideArchived = true);
    Task<ProjectResponseDto> GetProject(long projectId, bool hideArchived = true);
    Task<ProjectResponseDto> CreateProject(CreateProjectRequestDto dto);
    Task<ProjectResponseDto> UpdateProject(long projectId, UpdateProjectRequestDto dto);
    Task<bool> DeleteProject(long projectId);
    Task<bool> ArchiveProject(long projectId);
    Task<bool> UnarchiveProject(long projectId);
    Task<ProjectStatResponseDto> GetProjectStats(long projectId);

    Task<bool> AddMemberToProject(long projectId, long? roleId, long? userId, long? groupId);
    Task<bool> UpdateProjectMemberRole(long projectId, long roleId, long? userId, long? groupId);
    Task<bool> RemoveMemberFromProject(long projectId, long? userId, long? groupId);

    // TODO: move this to query business
    Task<IEnumerable<HistoricalRecordResponseDto>> GetMultiProjectRecords(long[] projects, bool hideArchived); 
}