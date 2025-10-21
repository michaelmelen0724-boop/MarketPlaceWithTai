using System.ComponentModel.DataAnnotations;

namespace Marketplace.Api.Models;

public sealed class JobPosting
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(160)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(16 * 1024)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(16 * 1024)]
    public string Requirements { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string PosterName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string PosterContact { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime AuctionEndsAtUtc { get; set; }

    public ICollection<Bid> Bids { get; set; } = new List<Bid>();
}