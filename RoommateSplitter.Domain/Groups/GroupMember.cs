namespace RoommateSplitter.Domain.Groups;

public enum GroupRole
{
    admin = 0,
    member = 1
}

public class GroupMember
{
    public Guid GroupID { get; private set; }
    public Guid UserID { get; private set; }
    public GroupRole Role { get; private set; }
    public DateTime JoinedAt { get; private set; }

    private GroupMember()
    {
    }

    public GroupMember(Guid groupId, Guid userId, GroupRole role)
    {
        GroupID = groupId;
        UserID = userId;
        Role = role;
        JoinedAt = DateTime.UtcNow;
    }
}