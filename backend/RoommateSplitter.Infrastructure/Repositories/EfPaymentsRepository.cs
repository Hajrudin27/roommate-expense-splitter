using Microsoft.EntityFrameworkCore;
using RoommateSplitter.Domain.Payments;
using RoommateSplitter.Domain.Repositories;
using RoommateSplitter.Infrastructure.Persistence;
using RoommateSplitter.Infrastructure.Persistence.Models;

namespace RoommateSplitter.Infrastructure.Repositories;

public sealed class EfPaymentsRepository : IPaymentsRepository
{
    private readonly RoommateSplitterDbContext _db;

    public EfPaymentsRepository(RoommateSplitterDbContext db) => _db = db;

    public void Add(Payment payment)
    {
        var row = new PaymentRow
        {
            Id = payment.Id,
            GroupId = payment.GroupId,
            FromUserId = payment.FromUserId,
            ToUserId = payment.ToUserId,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate.ToDateTime(TimeOnly.MinValue),
            CreatedAt = payment.CreatedAt,
        };

        _db.Payments.Add(row);
        _db.SaveChanges();
    }

    public IReadOnlyList<Payment> ListByGroupId(Guid groupId)
    {
        return _db.Payments
            .AsNoTracking()
            .Where(p => p.GroupId == groupId)
            .OrderBy(p => p.PaymentDate)
            .Select(MapToDomain)
            .ToList();
    }

    private static Payment MapToDomain(PaymentRow row)
    {
        var p = DomainHydrator.Create<Payment>();
        DomainHydrator.Set(p, nameof(Payment.Id), row.Id);
        DomainHydrator.Set(p, nameof(Payment.GroupId), row.GroupId);
        DomainHydrator.Set(p, nameof(Payment.FromUserId), row.FromUserId);
        DomainHydrator.Set(p, nameof(Payment.ToUserId), row.ToUserId);
        DomainHydrator.Set(p, nameof(Payment.Amount), row.Amount);
        DomainHydrator.Set(p, nameof(Payment.PaymentDate), DateOnly.FromDateTime(row.PaymentDate));
        DomainHydrator.Set(p, nameof(Payment.CreatedAt), row.CreatedAt);
        return p;
    }
}
