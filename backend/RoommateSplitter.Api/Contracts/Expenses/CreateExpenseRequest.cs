namespace RoommateSplitter.Api.Contracts.Expenses;

public record CreateExpenseRequest(
    string Description,
    decimal Amount,
    Guid PaidByUserId,
    DateOnly ExpenseDate,
    IReadOnlyList<Guid> ParticipantUserIds
    );