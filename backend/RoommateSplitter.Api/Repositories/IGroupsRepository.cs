using RoommateSplitter.Domain.Groups;

namespace RoommateSplitter.Api.Repositories;

public interface IGroupsRepository
{
    IReadOnlyList<Group> List();
    Group? GetById(Guid id);
    void Add(Group group);
}
