using System.ComponentModel.DataAnnotations;

namespace RoommateSplitter.Infrastructure.Persistence.Models;

public class UserRow
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
