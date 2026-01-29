namespace RoommateSplitter.Domain.Balances;

public sealed record Transfer(
    Guid FromUserId,
    Guid ToUserId,
    decimal Amount
);