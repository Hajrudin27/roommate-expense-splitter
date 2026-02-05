using System.ComponentModel.DataAnnotations;

namespace RoommateSplitter.Infrastructure.Persistence.Models;

public class ExpenseShareRow
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid ExpenseId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public decimal Amount { get; set; }
}
