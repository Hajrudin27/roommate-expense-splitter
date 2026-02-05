using RoommateSplitter.Domain.Payments;

namespace RoommateSplitter.Domain.Repositories;

public interface IPaymentsRepository
{
    void Add(Payment payment);
    IReadOnlyList<Payment> ListByGroupId(Guid groupId);
}
