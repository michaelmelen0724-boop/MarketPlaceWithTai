using Marketplace.Api.Data;
using MarketPlace.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/home")]
public sealed class HomeController : ControllerBase
{
    private readonly MarketplaceDbContext _db;

    public HomeController(MarketplaceDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<HomeSummaryDto>> GetSummary(
        [FromQuery] int recentCount = 10,
        [FromQuery] int activeCount = 10)
    {
        var now = DateTime.UtcNow;

        var recentJobs = await _db.Jobs
            .OrderByDescending(j => j.CreatedAtUtc)
            .Take(recentCount)
            .Select(j => new JobSummaryDto(
                j.Id,
                j.Title,
                j.PosterName,
                j.AuctionEndsAtUtc,
                j.Bids.Count,
                j.Bids
                    .OrderBy(b => b.Amount)
                    .Select(b => (decimal?)b.Amount)
                    .FirstOrDefault(),
                j.AuctionEndsAtUtc > now))
            .ToListAsync();

        var activeJobs = await _db.Jobs
            .Where(j => j.AuctionEndsAtUtc > now)
            .OrderByDescending(j => j.Bids.Count)
            .ThenBy(j => j.AuctionEndsAtUtc)
            .Take(activeCount)
            .Select(j => new JobSummaryDto(
                j.Id,
                j.Title,
                j.PosterName,
                j.AuctionEndsAtUtc,
                j.Bids.Count,
                j.Bids
                    .OrderBy(b => b.Amount)
                    .Select(b => (decimal?)b.Amount)
                    .FirstOrDefault(),
                true))
            .ToListAsync();

        var response = new HomeSummaryDto(recentJobs, activeJobs);

        return Ok(response);
    }
}