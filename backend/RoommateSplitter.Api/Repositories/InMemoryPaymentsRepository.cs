using RoommateSplitter.Domain.Payments;
using RoommateSplitter.Domain.Repositories;

namespace RoommateSplitter.Api.Repositories;

public sealed class InMemoryPaymentsRepository : IPaymentsRepository
{
    private readonly List<Payment> _payments = new();

    public void Add(Payment payment)
    {
        _payments.Add(payment);
    }

    public IReadOnlyList<Payment> ListByGroupId(Guid groupId)
    {
        return _payments
            .Where(p => p.GroupId == groupId)
            .OrderByDescending(p => p.PaymentDate)
            .ThenByDescending(p => p.CreatedAt)
            .ToList();
    }
}
