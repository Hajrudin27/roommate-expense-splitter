using RoommateSplitter.Domain.Groups;

namespace RoommateSplitter.Domain.Repositories;

public interface IGroupMembersRepository
{
    IReadOnlyList<GroupMember> ListByGroupId(Guid groupId);
    bool Exists(Guid groupId, Guid userId);
    void Add(GroupMember member);

    IReadOnlyList<(Guid GroupId, Guid UserId, string Email, string Name, string Role, DateTime JoinedAt)>
    ListWithUsersByGroupId(Guid groupId);
}

