using RoommateSplitter.Domain.Expenses;

namespace RoommateSplitter.Domain.Tests.Expenses;

public class ExpenseSplitTests
{
    [Fact]
    public void EqualSplit_SumsExactlyToTotal()
    {
        var total = 100.000m;
        var users = new List<Guid>
        {
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid()
        };

        var shares = ExpenseSplit.Equal(total, users);
        var sum = shares.Sum(s => s.Amount);

        Assert.Equal(3, shares.Count);
        Assert.Equal(total, sum);
    }
}