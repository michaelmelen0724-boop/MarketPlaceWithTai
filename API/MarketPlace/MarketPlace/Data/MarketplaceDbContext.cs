using Microsoft.EntityFrameworkCore;
using Marketplace.Api.Models;

namespace Marketplace.Api.Data;

public sealed class MarketplaceDbContext : DbContext
{
    public MarketplaceDbContext(DbContextOptions<MarketplaceDbContext> options)
        : base(options)
    {
    }

    public DbSet<JobPosting> Jobs => Set<JobPosting>();
    public DbSet<Bid> Bids => Set<Bid>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<JobPosting>()
            .HasMany(j => j.Bids)
            .WithOne(b => b.JobPosting!)
            .HasForeignKey(b => b.JobPostingId)
            .OnDelete(DeleteBehavior.Cascade);

        Seed(modelBuilder);
    }

    private static void Seed(ModelBuilder modelBuilder)
    {
        var now = DateTime.UtcNow;
        var jobs = new List<JobPosting>();
        var bids = new List<Bid>();

        for (int i = 0; i < 12; i++)
        {
            var jobId = Guid.NewGuid();
            var created = now.AddDays(-i);
            var expires = created.AddDays(7 + i % 3);

            var job = new JobPosting
            {
                Id = jobId,
                Title = $"Contract Job #{i + 1}",
                Description = "Sample job description showcasing the expected format.",
                Requirements = "• Relevant experience\n• Strong communication skills\n• Ability to meet deadlines",
                PosterName = $"Poster {i + 1}",
                PosterContact = $"poster{i + 1}@example.com",
                CreatedAtUtc = created,
                AuctionEndsAtUtc = expires
            };

            jobs.Add(job);

            var bidCount = i % 4 == 0 ? 0 : 5 + i;
            for (int b = 0; b < bidCount; b++)
            {
                bids.Add(new Bid
                {
                    Id = Guid.NewGuid(),
                    JobPostingId = jobId,
                    BidderName = $"Bidder {b + 1}",
                    Amount = 250m - (b * 5) - i,
                    CreatedAtUtc = created.AddHours(b + 1)
                });
            }
        }

        modelBuilder.Entity<JobPosting>().HasData(jobs);
        modelBuilder.Entity<Bid>().HasData(bids);
    }
}