using Marketplace.Api.Data;
using Marketplace.Api.Models;
using MarketPlace.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class JobsController : ControllerBase
{
    private readonly MarketplaceDbContext _db;

    public JobsController(MarketplaceDbContext db)
    {
        _db = db;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JobDetailDto>> GetById(Guid id)
    {
        var job = await _db.Jobs
            .Where(j => j.Id == id)
            .Select(j => new JobDetailDto(
                j.Id,
                j.Title,
                j.Description,
                j.Requirements,
                j.PosterName,
                j.PosterContact,
                j.CreatedAtUtc,
                j.AuctionEndsAtUtc,
                j.Bids
                    .OrderBy(b => b.Amount)
                    .Select(b => new BidDto(b.Id, b.BidderName, b.Amount, b.CreatedAtUtc))
                    .ToList()))
            .FirstOrDefaultAsync();

        if (job is null)
        {
            return NotFound();
        }

        return Ok(job);
    }

    [HttpPost]
    public async Task<ActionResult> Create(CreateJobDto input)
    {
        if (string.IsNullOrWhiteSpace(input.Title) ||
            string.IsNullOrWhiteSpace(input.Description) ||
            string.IsNullOrWhiteSpace(input.Requirements) ||
            string.IsNullOrWhiteSpace(input.PosterName) ||
            string.IsNullOrWhiteSpace(input.PosterContact))
        {
            return BadRequest("Title, description, requirements, poster name, and contact are required.");
        }

        if (input.Title.Length > 160)
        {
            return BadRequest("Title must be 160 characters or fewer.");
        }

        if (input.Description.Length > 16 * 1024 || input.Requirements.Length > 16 * 1024)
        {
            return BadRequest("Description or requirements exceed the 16KB limit.");
        }

        if (input.PosterName.Length > 120 || input.PosterContact.Length > 200)
        {
            return BadRequest("Poster name or contact exceeds allowed length.");
        }

        if (input.AuctionEndsAtUtc <= DateTime.UtcNow.AddHours(1))
        {
            return BadRequest("Auction end must be at least one hour in the future.");
        }

        var job = new JobPosting
        {
            Title = input.Title.Trim(),
            Description = input.Description.Trim(),
            Requirements = input.Requirements.Trim(),
            PosterName = input.PosterName.Trim(),
            PosterContact = input.PosterContact.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            AuctionEndsAtUtc = input.AuctionEndsAtUtc.ToUniversalTime()
        };

        await _db.Jobs.AddAsync(job);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = job.Id }, new { job.Id });
    }

    [HttpPost("{id:guid}/bids")]
    public async Task<ActionResult<BidDto>> PlaceBid(Guid id, CreateBidDto input)
    {
        if (string.IsNullOrWhiteSpace(input.BidderName))
        {
            return BadRequest("Bidder name is required.");
        }

        if (input.BidderName.Length > 120)
        {
            return BadRequest("Bidder name must be 120 characters or fewer.");
        }

        if (input.Amount <= 0)
        {
            return BadRequest("Bid amount must be greater than zero.");
        }

        var job = await _db.Jobs
            .Include(j => j.Bids)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job is null)
        {
            return NotFound("Job not found.");
        }

        if (job.AuctionEndsAtUtc <= DateTime.UtcNow)
        {
            return BadRequest("This auction has ended.");
        }

        var lowestBid = job.Bids
            .OrderBy(b => b.Amount)
            .FirstOrDefault();

        if (lowestBid is not null && input.Amount >= lowestBid.Amount)
        {
            return BadRequest($"Bid must be lower than the current lowest bid ({lowestBid.Amount:C}).");
        }

        var bid = new Bid
        {
            JobPostingId = id,
            BidderName = input.BidderName.Trim(),
            Amount = decimal.Round(input.Amount, 2, MidpointRounding.AwayFromZero),
            CreatedAtUtc = DateTime.UtcNow
        };

        await _db.Bids.AddAsync(bid);
        await _db.SaveChangesAsync();

        var response = new BidDto(bid.Id, bid.BidderName, bid.Amount, bid.CreatedAtUtc);

        return CreatedAtAction(nameof(GetById), new { id }, response);
    }
}