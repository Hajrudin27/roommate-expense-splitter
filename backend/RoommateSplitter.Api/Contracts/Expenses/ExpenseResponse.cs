namespace RoommateSplitter.Api.Contracts.Expenses;

public record ExpenseShareResponse(
    Guid UserId,
    decimal Amount
    );

public record ExpenseResponse(
    Guid Id,
    Guid GroupId,
    string Description,
    decimal Amount,
    Guid PaidByUserId,
    DateOnly ExpenseDate,
    DateTime CreatedAt,
    IReadOnlyList<ExpenseShareResponse> Shares
);