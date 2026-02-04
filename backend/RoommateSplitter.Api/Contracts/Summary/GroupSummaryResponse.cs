using RoommateSplitter.Api.Contracts.Balances;
using RoommateSplitter.Api.Contracts.Expenses;
using RoommateSplitter.Api.Contracts.Payments;
using RoommateSplitter.Api.Contracts.Groups;

namespace RoommateSplitter.Api.Contracts.Summary;

public sealed record GroupSummaryResponse(
    GroupResponse Group,
    IReadOnlyList<ExpenseResponse> Expenses,
    IReadOnlyList<PaymentResponse> Payments,
    GetBalancesResponse Balances
);
