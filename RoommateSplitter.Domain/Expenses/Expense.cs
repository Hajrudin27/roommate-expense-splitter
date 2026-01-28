namespace RoommateSplitter.Domain.Expenses;

public class Expense
{
    public Guid Id { get; private set; }
    public Guid GroupId { get; private set; }
    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public Guid PaidByUserId { get; private set; }
    public DateOnly ExpenseDate { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<ExpenseShare> _shares = new();
    public IReadOnlyList<ExpenseShare> Shares => _shares;

    private Expense()
    {
        Description = string.Empty;
    }

    public Expense(
        Guid groupId,
        string description,
        decimal amount,
        Guid paidByUserId,
        DateOnly expenseDate,
        IEnumerable<ExpenseShare> shares)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description is required.", nameof(description));
        }
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
        }
        var shareList = shares?.ToList() ?? throw new ArgumentNullException(nameof(shares));

        if (shareList.Count == 0)
        {
            throw new ArgumentException("At least one share is required.", nameof(shares));
        }

        var sum = shareList.Sum(s => s.Amount);
        if (Math.Abs(sum - amount) > 0.01m)
        {
            throw new ArgumentException($"Shares must sum to total amount. Sum={sum}, Total={amount}");
        }

        Id = Guid.NewGuid();
        GroupId = groupId;
        Description = description.Trim();
        Amount = amount;
        PaidByUserId = paidByUserId;
        ExpenseDate = expenseDate;
        CreatedAt = DateTime.UtcNow;
        
        _shares.AddRange(shareList);
    }
}