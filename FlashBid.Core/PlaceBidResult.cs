// Designed and Implemented by Amin Ghaderi

namespace FlashBid.Core;

public sealed record PlaceBidCommand(Guid AuctionId, decimal BidAmount, string UserId);

public sealed record BidSnapshot(decimal CurrentPrice, string HighestBidder, long Version);

public sealed record PlaceBidResult(
    bool IsAccepted,
    decimal CurrentPrice,
    string HighestBidder,
    long Version)
{
    public static PlaceBidResult Rejected() => new(false, 0m, string.Empty, 0);

    public static PlaceBidResult Accepted(BidSnapshot snapshot) =>
        new(true, snapshot.CurrentPrice, snapshot.HighestBidder, snapshot.Version);
}
