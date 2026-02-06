using Microsoft.EntityFrameworkCore;
using RoommateSplitter.Domain.Groups;
using RoommateSplitter.Domain.Repositories;
using RoommateSplitter.Infrastructure.Persistence;
using RoommateSplitter.Infrastructure.Persistence.Models;

namespace RoommateSplitter.Infrastructure.Repositories;

public sealed class EfGroupMembersRepository : IGroupMembersRepository
{
    private readonly RoommateSplitterDbContext _db;
    public EfGroupMembersRepository(RoommateSplitterDbContext db) => _db = db;

    public IReadOnlyList<GroupMember> ListByGroupId(Guid groupId)
    {
        return _db.GroupMembers
            .AsNoTracking()
            .Where(m => m.GroupId == groupId)
            .OrderBy(m => m.JoinedAt)
            .Select(MapToDomain)
            .ToList();
    }

    public bool Exists(Guid groupId, Guid userId)
    {
        return _db.GroupMembers.AsNoTracking().Any(m => m.GroupId == groupId && m.UserId == userId);
    }

    public void Add(GroupMember member)
    {
        var row = new GroupMemberRow
        {
            GroupId = member.GroupID,
            UserId = member.UserID,
            Role = (int)member.Role,
            JoinedAt = member.JoinedAt
        };

        _db.GroupMembers.Add(row);
        _db.SaveChanges();
    }

    private static GroupMember MapToDomain(GroupMemberRow row)
    {
        var m = DomainHydrator.Create<GroupMember>();
        DomainHydrator.Set(m, nameof(GroupMember.GroupID), row.GroupId);
        DomainHydrator.Set(m, nameof(GroupMember.UserID), row.UserId);
        DomainHydrator.Set(m, nameof(GroupMember.Role), (GroupRole)row.Role);
        DomainHydrator.Set(m, nameof(GroupMember.JoinedAt), row.JoinedAt);
        return m;
    }
    public IReadOnlyList<(Guid GroupId, Guid UserId, string Email, string Name, string Role, DateTime JoinedAt)>
    ListWithUsersByGroupId(Guid groupId)
{
    var rows =
        from m in _db.GroupMembers.AsNoTracking()
        join u in _db.Users.AsNoTracking() on m.UserId equals u.Id
        where m.GroupId == groupId
        orderby m.JoinedAt
        select new
        {
            m.GroupId,
            m.UserId,
            u.Email,
            u.Name,
            m.Role,
            m.JoinedAt
        };

    return rows
        .ToList()
        .Select(x => (x.GroupId, x.UserId, x.Email, x.Name, ((RoommateSplitter.Domain.Groups.GroupRole)x.Role).ToString(), x.JoinedAt))
        .ToList();
}

}
