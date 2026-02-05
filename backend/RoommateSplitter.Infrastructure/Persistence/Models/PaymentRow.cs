using System.ComponentModel.DataAnnotations;

namespace RoommateSplitter.Infrastructure.Persistence.Models;

public class PaymentRow
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid GroupId { get; set; }

    [Required]
    public Guid FromUserId { get; set; }

    [Required]
    public Guid ToUserId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
