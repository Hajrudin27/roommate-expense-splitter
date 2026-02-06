using RoommateSplitter.Domain.Users;

namespace RoommateSplitter.Domain.Repositories;

public interface IUsersRepository
{
    User? GetById(Guid id);
    User? GetByEmail(string email);
    void Add(User user);
}
