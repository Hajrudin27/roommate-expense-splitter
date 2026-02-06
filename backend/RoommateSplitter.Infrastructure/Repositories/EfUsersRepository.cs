using Microsoft.EntityFrameworkCore;
using RoommateSplitter.Domain.Repositories;
using RoommateSplitter.Domain.Users;
using RoommateSplitter.Infrastructure.Persistence;
using RoommateSplitter.Infrastructure.Persistence.Models;

namespace RoommateSplitter.Infrastructure.Repositories;

public sealed class EfUsersRepository : IUsersRepository
{
    private readonly RoommateSplitterDbContext _db;
    public EfUsersRepository(RoommateSplitterDbContext db) => _db = db;

    public User? GetById(Guid id)
    {
        var row = _db.Users.AsNoTracking().SingleOrDefault(x => x.Id == id);
        return row is null ? null : MapToDomain(row);
    }

    public User? GetByEmail(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var row = _db.Users.AsNoTracking().SingleOrDefault(x => x.Email.ToLower() == normalized);
        return row is null ? null : MapToDomain(row);
    }

    public void Add(User user)
    {
        var row = new UserRow
        {
            Id = user.Id,
            Email = user.Email.Trim(),
            Name = user.Name.Trim(),
            CreatedAt = user.CreatedAt
        };

        _db.Users.Add(row);
        _db.SaveChanges();
    }

    private static User MapToDomain(UserRow row)
    {
        var u = DomainHydrator.Create<User>();
        DomainHydrator.Set(u, nameof(User.Id), row.Id);
        DomainHydrator.Set(u, nameof(User.Email), row.Email);
        DomainHydrator.Set(u, nameof(User.Name), row.Name);
        DomainHydrator.Set(u, nameof(User.CreatedAt), row.CreatedAt);
        return u;
    }
}
