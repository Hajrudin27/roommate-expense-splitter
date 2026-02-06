namespace RoommateSplitter.Infrastructure.ReadModels;

public sealed record GroupMemberWithUserRow(
    Guid GroupId,
    Guid UserId,
    string Email,
    string Name,
    int Role,
    DateTime JoinedAt
);
