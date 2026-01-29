using RoommateSplitter.Domain.Expenses;

namespace RoommateSplitter.Domain.Balances;

public sealed class BalanceCalculator
{
    public BalanceResult Calculate(IEnumerable<Expense> expenses)
    {
        var net = CalculateNetBalances(expenses);
        var transfers = SuggestTransfers(net);

        return new BalanceResult(net, transfers);
    }

    public IReadOnlyDictionary<Guid, decimal> CalculateNetBalances(IEnumerable<Expense> expenses)
    {
        var net = new Dictionary<Guid, decimal>();

        foreach (var expense in expenses)
        {
            // payer gets credited full amount
            Add(net, expense.PaidByUserId, expense.Amount);

            // each participant share is a debit
            foreach (var share in expense.Shares)
            {
                Add(net, share.UserId, -share.Amount);
            }
        }
        return net.ToDictionary(kvp => kvp.Key, kvp => Round2(kvp.Value));
    }

    public IReadOnlyList<Transfer> SuggestTransfers(IReadOnlyDictionary<Guid, decimal> netBalances)
    {
    
        // return Array.Empty<Transfer>();

        var creditors = netBalances
            .Where(kvp => kvp.Value > 0m)
            .Select(kvp => (UserId: kvp.Key, Amount: kvp.Value))
            .ToList();

        var debtors = netBalances
            .Where(kvp => kvp.Value < 0m)
            .Select(kvp => (UserId: kvp.Key, Amount: -kvp.Value)) // store debt as positive
            .ToList();

        var transfers = new List<Transfer>();
        int i = 0, j = 0;

        while (i < debtors.Count && j < creditors.Count)
        {
            var debtor = debtors[i];
            var creditor = creditors[j];

            var amount = Math.Min(debtor.Amount, creditor.Amount);
            amount = Round2(amount);

            if (amount > 0m)
                transfers.Add(new Transfer(debtor.UserId, creditor.UserId, amount));

            debtor.Amount = Round2(debtor.Amount - amount);
            creditor.Amount = Round2(creditor.Amount - amount);

            debtors[i] = debtor;
            creditors[j] = creditor;

            if (debtor.Amount == 0m) i++;
            if (creditor.Amount == 0m) j++;
        }

        return transfers;
    }

    private static void Add(Dictionary<Guid, decimal> dict, Guid userId, decimal amount)
    {
        dict.TryGetValue(userId, out var current);
        dict[userId] = current + amount;
    }

    private static decimal Round2(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
