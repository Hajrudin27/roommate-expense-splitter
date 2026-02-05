using RoommateSplitter.Domain.Expenses;

namespace RoommateSplitter.Domain.Repositories;

public interface IExpensesRepository
{
    IReadOnlyList<Expense> ListByGroupId(Guid groupId);
    Expense? GetById(Guid id);
    void Add(Expense expense);
}
