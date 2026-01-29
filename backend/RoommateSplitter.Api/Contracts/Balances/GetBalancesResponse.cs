namespace RoommateSplitter.Api.Contracts.Balances;

public sealed record GetBalancesResponse(
    Guid GroupId,
    string Currency,
    IReadOnlyList<UserBalanceDto> NetBalances,
    IReadOnlyList<TransferDto> Transfers
);

public sealed record UserBalanceDto(Guid UserId, decimal Amount);

public sealed record TransferDto(Guid FromUserId, Guid ToUserId, decimal Amount);
