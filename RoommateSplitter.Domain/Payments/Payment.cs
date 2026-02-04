namespace RoommateSplitter.Domain.Payments;

public sealed class Payment
{
    public Guid Id { get; }
    public Guid GroupId { get; }
    public Guid FromUserId { get; }
    public Guid ToUserId { get; }
    public decimal Amount { get; }
    public DateOnly PaymentDate { get; }
    public DateTime CreatedAt { get; }

    public Payment(
        Guid groupId,
        Guid fromUserId,
        Guid toUserId,
        decimal amount,
        DateOnly paymentDate)
    {
        if (groupId == Guid.Empty)
            throw new ArgumentException("GroupId is required.");

        if (fromUserId == Guid.Empty)
            throw new ArgumentException("FromUserId is required.");

        if (toUserId == Guid.Empty)
            throw new ArgumentException("ToUserId is required.");

        if (fromUserId == toUserId)
            throw new ArgumentException("FromUserId and ToUserId cannot be the same.");

        if (amount <= 0m)
            throw new ArgumentException("Amount must be greater than 0.");

        Id = Guid.NewGuid();
        GroupId = groupId;
        FromUserId = fromUserId;
        ToUserId = toUserId;
        Amount = amount;
        PaymentDate = paymentDate;
        CreatedAt = DateTime.UtcNow;
    }
}
