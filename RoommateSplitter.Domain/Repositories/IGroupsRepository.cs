using RoommateSplitter.Domain.Groups;

namespace RoommateSplitter.Domain.Repositories;

public interface IGroupsRepository
{
    IReadOnlyList<Group> List();
    Group? GetById(Guid id);
    void Add(Group group);
}
