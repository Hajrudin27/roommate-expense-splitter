namespace RoommateSplitter.Api.Contracts.Members;

public sealed record CreateMemberRequest(
    string Email,
    string Name,
    string? Role // "admin" | "member" (optional)
);
