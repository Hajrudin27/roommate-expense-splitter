using RoommateSplitter.Domain.Balances;
using RoommateSplitter.Domain.Expenses;
using RoommateSplitter.Domain.Payments;
using Xunit;

namespace RoommateSplitter.Domain.Tests.Balances;

public sealed class BalanceCalculatorPaymentsTests
{
    [Fact]
    public void CalculateNetBalances_PaymentReducesDebt()
    {
        var groupId = Guid.NewGuid();
        var u1 = Guid.NewGuid();
        var u2 = Guid.NewGuid();

        // Expense: u1 paid 100, split 50/50 => u2 owes u1 50
        var expense = new Expense(
            groupId,
            "Groceries",
            100m,
            u1,
            new DateOnly(2026, 2, 4),
            new List<ExpenseShare>
            {
                new ExpenseShare(u1, 50m),
                new ExpenseShare(u2, 50m),
            }
        );

        // Payment: u2 pays u1 20 => u2 now owes 30
        var payment = new Payment(
            groupId,
            u2,
            u1,
            20m,
            new DateOnly(2026, 2, 4)
        );

        var calc = new BalanceCalculator();
        var net = calc.CalculateNetBalances(new[] { expense }, new[] { payment });

        Assert.Equal(30m, net[u1]);   // was +50, payment reduces to +30
        Assert.Equal(-30m, net[u2]);  // was -50, payment reduces to -30
    }

    [Fact]
    public void CalculateNetBalances_MultipleExpensesAndPayments_NetsCorrectly()
    {
        var groupId = Guid.NewGuid();
        var u1 = Guid.NewGuid();
        var u2 = Guid.NewGuid();
        var u3 = Guid.NewGuid();

        // Expense 1: u1 paid 90, split 30 each => u2 owes 30, u3 owes 30
        var e1 = new Expense(
            groupId, "Dinner", 90m, u1, new DateOnly(2026, 2, 1),
            new List<ExpenseShare>
            {
                new ExpenseShare(u1, 30m),
                new ExpenseShare(u2, 30m),
                new ExpenseShare(u3, 30m),
            }
        );

        // Expense 2: u2 paid 60, split 20 each => u1 owes 20, u3 owes 20
        var e2 = new Expense(
            groupId, "Taxi", 60m, u2, new DateOnly(2026, 2, 2),
            new List<ExpenseShare>
            {
                new ExpenseShare(u1, 20m),
                new ExpenseShare(u2, 20m),
                new ExpenseShare(u3, 20m),
            }
        );

        // Without payments:
        // u1: +60 (from e1) -20 (from e2 share) = +40
        // u2: -30 (e1 share) +40 (e2 net) = +10
        // u3: -30 (e1 share) -20 (e2 share) = -50

        // Payment: u3 pays u1 15 => u3 debt reduced, u1 credit reduced
        var p1 = new Payment(groupId, u3, u1, 15m, new DateOnly(2026, 2, 3));

        var calc = new BalanceCalculator();
        var net = calc.CalculateNetBalances(new[] { e1, e2 }, new[] { p1 });

        Assert.Equal(25m, net[u1]);  // +40 - 15 = +25
        Assert.Equal(10m, net[u2]);  // unchanged
        Assert.Equal(-35m, net[u3]); // -50 + 15 = -35
    }

    [Fact]
    public void CalculateNetBalances_OverpaymentCanFlipBalances()
    {
        var groupId = Guid.NewGuid();
        var u1 = Guid.NewGuid();
        var u2 = Guid.NewGuid();

        // Expense: u1 paid 100 split 50/50 => u2 owes u1 50
        var expense = new Expense(
            groupId, "Groceries", 100m, u1, new DateOnly(2026, 2, 4),
            new List<ExpenseShare>
            {
                new ExpenseShare(u1, 50m),
                new ExpenseShare(u2, 50m),
            }
        );

        // Payment: u2 pays u1 70 (overpay by 20)
        // After payment, u2 should be owed 20 (u2 net +20), u1 owes 20 (u1 net -20)
        var payment = new Payment(groupId, u2, u1, 70m, new DateOnly(2026, 2, 4));

        var calc = new BalanceCalculator();
        var net = calc.CalculateNetBalances(new[] { expense }, new[] { payment });

        Assert.Equal(-20m, net[u1]);
        Assert.Equal(20m, net[u2]);
    }
}
