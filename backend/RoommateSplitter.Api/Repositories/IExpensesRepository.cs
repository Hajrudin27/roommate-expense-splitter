using RoommateSplitter.Api.Repositories;
using RoommateSplitter.Domain.Expenses;

public interface IExpensesRepository
{
    IReadOnlyList<Expense> ListByGroupId(Guid groupId);
    Expense? GetById(Guid id);
    void Add(Expense expense);
}