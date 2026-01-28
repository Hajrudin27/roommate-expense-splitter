using System.Diagnostics.Contracts;

namespace RoommateSplitter.Domain.Groups;

public class Group
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Currency { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Group()
    {
        Name = string.Empty;
        Currency = string.Empty;
    }

    public Group(string name, string currency)
    {
        Id = Guid.NewGuid();
        Name = name;
        Currency = currency;
        CreatedAt = DateTime.UtcNow;
    }
}

