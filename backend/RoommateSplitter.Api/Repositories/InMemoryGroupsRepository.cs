using RoommateSplitter.Domain.Groups;

namespace RoommateSplitter.Api.Repositories;

public class InMemoryGroupsRepository : IGroupsRepository
{
    private readonly List<Group> _groups = new();

    public IReadOnlyList<Group> List() => _groups;

    public Group? GetById(Guid id) => _groups.SingleOrDefault(g => g.Id == id);
    public void Add(Group group) => _groups.Add(group);
}