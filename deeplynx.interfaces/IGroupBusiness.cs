using deeplynx.models;

namespace deeplynx.interfaces;

public interface IGroupBusiness
{
    Task<IEnumerable<GroupResponseDto>> GetAllGroups(bool hideArchived = true);
    Task<GroupResponseDto> GetGroup(long groupId, bool hideArchived = true);
    Task<GroupResponseDto> CreateGroup(CreateGroupRequestDto dto);
    Task<GroupResponseDto> UpdateGroup(long groupId, UpdateGroupRequestDto dto);
    Task<bool> ArchiveGroup(long groupId);
    Task<bool> UnarchiveGroup(long groupId);
    Task<bool> DeleteGroup(long groupId);
    Task<bool>  AddUserToGroup(long userId, long groupId);
    Task<bool>  RemoveUserFromGroup(long userId, long groupId);
}