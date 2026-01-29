namespace RoommateSplitter.Domain.Balances;

public sealed record BalanceResult(
    IReadOnlyDictionary<Guid, decimal> NetBalances,
    IReadOnlyList<Transfer> Transfers
);