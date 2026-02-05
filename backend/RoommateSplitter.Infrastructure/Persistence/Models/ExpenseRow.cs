using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoommateSplitter.Infrastructure.Persistence.Models;

public class ExpenseRow
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid GroupId { get; set; }

    [Required]
    public Guid PaidByUserId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = null!;

    public DateTime ExpenseDate { get; set; }

    public List<ExpenseShareRow> Shares { get; set; } = new();
}
