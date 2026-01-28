namespace RoommateSplitter.Domain.Expenses;

public static class ExpenseSplit
{
    public static IReadOnlyList<ExpenseShare> Equal(decimal totalAmount, IReadOnlyList<Guid> participantUserIds)
    {
        if (totalAmount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalAmount), "Total amount must be greater than zero.");
        }
        if (participantUserIds is null)
        {
            throw new ArgumentNullException(nameof(participantUserIds));
        }
        if (participantUserIds.Count == 0)
        {
            throw new ArgumentException("At least one participant is required.", nameof(participantUserIds));
        }

        // base share rounded to 2 decimals
        var n = participantUserIds.Count;
        var baseShare = Math.Round(totalAmount / n, 2, MidpointRounding.AwayFromZero);
        var shares = new List<ExpenseShare>(n);

        // Give baseShare to first n-1 participants
        for (var i = 0; i < n - 1; i++)
        {
            shares.Add(new ExpenseShare(participantUserIds[i], baseShare));
        }

        // Last participant gets the remainder to ensure total matches
        var assigned = baseShare * (n - 1);
        var remainder = totalAmount - assigned;
        remainder = Math.Round(remainder, 2, MidpointRounding.AwayFromZero);
        shares.Add(new ExpenseShare(participantUserIds[n - 1], remainder));
        return shares;
    }
}