namespace RoommateSplitter.Domain.Expenses;

public class ExpenseShare
{
    public Guid UserId { get; private set; }
    public decimal Amount { get; private set; }

    private ExpenseShare()
    {
    }

    public ExpenseShare(Guid userId, decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero.");
        }
        UserId = userId;
        Amount = amount;
    }
}