using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Api.Models;

public sealed class Bid
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid JobPostingId { get; set; }

    public JobPosting? JobPosting { get; set; }

    [Required]
    [MaxLength(120)]
    public string BidderName { get; set; } = string.Empty;

    [Range(1, double.MaxValue)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}