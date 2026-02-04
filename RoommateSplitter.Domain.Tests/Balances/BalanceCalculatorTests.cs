using RoommateSplitter.Domain.Balances;
using RoommateSplitter.Domain.Expenses;
using Xunit;

namespace RoommateSplitter.Domain.Tests.Balances;

public sealed class BalanceCalculatorTests
{
    [Fact]
    public void CalculateNetBalances_SingleExpense_ThreeUsers()
    {
        var groupId = Guid.NewGuid();
        var u1 = Guid.NewGuid();
        var u2 = Guid.NewGuid();
        var u3 = Guid.NewGuid();

        var shares = new List<ExpenseShare>
        {
            new ExpenseShare(u1, 30m),
            new ExpenseShare(u2, 30m),
            new ExpenseShare(u3, 30m),
        };

        var expense = new Expense(
            groupId,
            "Dinner",
            90m,
            u1,
            new DateOnly(2026, 2, 4),
            shares
        );

        var calculator = new BalanceCalculator();

        var net = calculator.CalculateNetBalances(new[] { expense });

        Assert.Equal(60m, net[u1]);   // paid 90, owed 30 => +60
        Assert.Equal(-30m, net[u2]);  // owed 30
        Assert.Equal(-30m, net[u3]);  // owed 30
    }

    [Fact]
    public void CalculateNetBalances_MultipleExpenses_NetsCorrectly()
    {
        var groupId = Guid.NewGuid();
        var u1 = Guid.NewGuid();
        var u2 = Guid.NewGuid();

        var expense1 = new Expense(
            groupId,
            "Groceries",
            100m,
            u1,
            new DateOnly(2026, 2, 1),
            new List<ExpenseShare>
            {
                new ExpenseShare(u1, 50m),
                new ExpenseShare(u2, 50m),
            }
        );

        var expense2 = new Expense(
            groupId,
            "Taxi",
            40m,
            u2,
            new DateOnly(2026, 2, 2),
            new List<ExpenseShare>
            {
                new ExpenseShare(u1, 20m),
                new ExpenseShare(u2, 20m),
            }
        );

        var calculator = new BalanceCalculator();

        var net = calculator.CalculateNetBalances(new[] { expense1, expense2 });

        Assert.Equal(30m, net[u1]);
        Assert.Equal(-30m, net[u2]);
    }

    [Fact]
    public void Calculate_IncludesTransfers_IfEnabled()
    {

        var groupId = Guid.NewGuid();
        var u1 = Guid.NewGuid();
        var u2 = Guid.NewGuid();
        var u3 = Guid.NewGuid();

        var expense = new Expense(
            groupId,
            "Dinner",
            90m,
            u1,
            new DateOnly(2026, 2, 4),
            new List<ExpenseShare>
            {
                new ExpenseShare(u1, 30m),
                new ExpenseShare(u2, 30m),
                new ExpenseShare(u3, 30m),
            }
        );

        var calculator = new BalanceCalculator();

        var result = calculator.Calculate(new[] { expense });

        Assert.Contains(result.Transfers, t => t.FromUserId == u2 && t.ToUserId == u1 && t.Amount == 30m);
        Assert.Contains(result.Transfers, t => t.FromUserId == u3 && t.ToUserId == u1 && t.Amount == 30m);
    }
}
