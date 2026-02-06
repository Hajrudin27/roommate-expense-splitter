using System.ComponentModel.DataAnnotations;

namespace RoommateSplitter.Infrastructure.Persistence.Models;

public class GroupMemberRow
{
    [Required]
    public Guid GroupId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public int Role { get; set; }

    public DateTime JoinedAt { get; set; }

    // optional navigation (nice for querying later)
    public UserRow? User { get; set; }
}
