using RoommateSplitter.Domain.Expenses;

namespace RoommateSplitter.Api.Repositories;

public class InMemoryExpensesRepository : IExpensesRepository
{
    private readonly List<Expense> _expenses = new();
    public IReadOnlyList<Expense> ListByGroupId(Guid groupId) => _expenses.Where(e => e.GroupId == groupId).ToList();
    public Expense? GetById(Guid id) => _expenses.SingleOrDefault(e => e.Id == id);
    public void Add(Expense expense) => _expenses.Add(expense);
}