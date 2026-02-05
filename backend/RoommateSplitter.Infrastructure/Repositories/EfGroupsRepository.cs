using Microsoft.EntityFrameworkCore;
using RoommateSplitter.Domain.Groups;
using RoommateSplitter.Domain.Repositories;
using RoommateSplitter.Infrastructure.Persistence;
using RoommateSplitter.Infrastructure.Persistence.Models;

namespace RoommateSplitter.Infrastructure.Repositories;

public sealed class EfGroupsRepository : IGroupsRepository
{
    private readonly RoommateSplitterDbContext _db;

    public EfGroupsRepository(RoommateSplitterDbContext db) => _db = db;

    public IReadOnlyList<Group> List()
    {
        return _db.Groups
            .AsNoTracking()
            .OrderBy(g => g.CreatedAt)
            .Select(MapToDomain)
            .ToList();
    }

    public Group? GetById(Guid id)
    {
        var row = _db.Groups.AsNoTracking().SingleOrDefault(g => g.Id == id);
        return row is null ? null : MapToDomain(row);
    }

    public void Add(Group group)
    {
        var row = new GroupRow
        {
            Id = group.Id,
            Name = group.Name,
            Currency = group.Currency,
            CreatedAt = group.CreatedAt
        };

        _db.Groups.Add(row);
        _db.SaveChanges();
    }

    private static Group MapToDomain(GroupRow row)
    {
        var g = DomainHydrator.Create<Group>();
        DomainHydrator.Set(g, nameof(Group.Id), row.Id);
        DomainHydrator.Set(g, nameof(Group.Name), row.Name);
        DomainHydrator.Set(g, nameof(Group.Currency), row.Currency);
        DomainHydrator.Set(g, nameof(Group.CreatedAt), row.CreatedAt);
        return g;
    }
}
