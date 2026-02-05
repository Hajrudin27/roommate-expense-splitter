using System.ComponentModel.DataAnnotations;

namespace RoommateSplitter.Infrastructure.Persistence.Models;

public class GroupRow
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
