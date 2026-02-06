namespace RoommateSplitter.Api.Contracts.Members;

public sealed record MemberResponse(
    Guid GroupId,
    Guid UserId,
    string Email,
    string Name,
    string Role,
    DateTime JoinedAt
);
