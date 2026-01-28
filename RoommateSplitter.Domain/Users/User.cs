namespace RoommateSplitter.Domain.Users;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string Name { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private User()
    {
        Email = string.Empty;
        Name = string.Empty;
    }

    public User(string email, string name)
    {
        Id = Guid.NewGuid();
        Email = email;
        Name = name;
        CreatedAt = DateTime.UtcNow;
    }
}

  
