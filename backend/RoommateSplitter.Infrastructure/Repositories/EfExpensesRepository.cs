using Microsoft.EntityFrameworkCore;
using RoommateSplitter.Domain.Expenses;
using RoommateSplitter.Domain.Repositories;
using RoommateSplitter.Infrastructure.Persistence;
using RoommateSplitter.Infrastructure.Persistence.Models;

namespace RoommateSplitter.Infrastructure.Repositories;

public sealed class EfExpensesRepository : IExpensesRepository
{
    private readonly RoommateSplitterDbContext _db;

    public EfExpensesRepository(RoommateSplitterDbContext db) => _db = db;

    public IReadOnlyList<Expense> ListByGroupId(Guid groupId)
    {
        return _db.Expenses
            .AsNoTracking()
            .Where(e => e.GroupId == groupId)
            .Include(e => e.Shares)
            .OrderBy(e => e.ExpenseDate)
            .Select(MapToDomain)
            .ToList();
    }

    public Expense? GetById(Guid id)
    {
        var row = _db.Expenses
            .AsNoTracking()
            .Include(e => e.Shares)
            .SingleOrDefault(e => e.Id == id);

        return row is null ? null : MapToDomain(row);
    }

    public void Add(Expense expense)
    {
        var row = new ExpenseRow
        {
            Id = expense.Id,
            GroupId = expense.GroupId,
            PaidByUserId = expense.PaidByUserId,
            Amount = expense.Amount,
            Description = expense.Description,
            ExpenseDate = expense.ExpenseDate.ToDateTime(TimeOnly.MinValue),
            CreatedAt = expense.CreatedAt,
            Shares = expense.Shares.Select(s => new ExpenseShareRow
            {
                Id = Guid.NewGuid(),
                UserId = s.UserId,
                Amount = s.Amount
            }).ToList()
        };

        _db.Expenses.Add(row);
        _db.SaveChanges();
    }

    private static Expense MapToDomain(ExpenseRow row)
    {
        var e = DomainHydrator.Create<Expense>();

        DomainHydrator.Set(e, nameof(Expense.Id), row.Id);
        DomainHydrator.Set(e, nameof(Expense.GroupId), row.GroupId);
        DomainHydrator.Set(e, nameof(Expense.Description), row.Description);
        DomainHydrator.Set(e, nameof(Expense.Amount), row.Amount);
        DomainHydrator.Set(e, nameof(Expense.PaidByUserId), row.PaidByUserId);
        DomainHydrator.Set(e, nameof(Expense.ExpenseDate), DateOnly.FromDateTime(row.ExpenseDate));
        DomainHydrator.Set(e, nameof(Expense.CreatedAt), row.CreatedAt);


        var shares = row.Shares
            .Select(s => new ExpenseShare(s.UserId, s.Amount))
            .ToList();

        DomainHydrator.SetField(e, "_shares", shares);

        return e;
    }
}
