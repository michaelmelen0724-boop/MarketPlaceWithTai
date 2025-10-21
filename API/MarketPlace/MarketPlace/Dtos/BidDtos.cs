namespace MarketPlace.Dtos;

public sealed record BidDto(
    Guid Id,
    string BidderName,
    decimal Amount,
    DateTime CreatedAtUtc);

public sealed record CreateBidDto(
    string BidderName,
    decimal Amount);