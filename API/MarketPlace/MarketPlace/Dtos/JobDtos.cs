namespace MarketPlace.Dtos;

public sealed record HomeSummaryDto(
    IReadOnlyCollection<JobSummaryDto> RecentJobs,
    IReadOnlyCollection<JobSummaryDto> ActiveJobs);

public sealed record JobSummaryDto(
    Guid Id,
    string Title,
    string PosterName,
    DateTime AuctionEndsAtUtc,
    int TotalBids,
    decimal? LowestBidAmount,
    bool IsActive);

public sealed record JobDetailDto(
    Guid Id,
    string Title,
    string Description,
    string Requirements,
    string PosterName,
    string PosterContact,
    DateTime CreatedAtUtc,
    DateTime AuctionEndsAtUtc,
    IReadOnlyCollection<BidDto> Bids);

public sealed record CreateJobDto(
    string Title,
    string Description,
    string Requirements,
    string PosterName,
    string PosterContact,
    DateTime AuctionEndsAtUtc);