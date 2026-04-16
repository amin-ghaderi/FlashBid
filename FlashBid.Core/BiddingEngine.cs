// Designed and Implemented by Amin Ghaderi

namespace FlashBid.Core;

public interface IRedisAuctionStateStore
{
    /// <summary>Returns true if the auction hash was created; false if the key already existed.</summary>
    Task<bool> CreateAuctionAsync(
        Guid auctionId,
        decimal initialPrice,
        long endTimeUnixMs,
        CancellationToken ct);

    Task<BidSnapshot?> TryPlaceBidAtomicAsync(
        Guid auctionId,
        decimal bidAmount,
        string userId,
        CancellationToken ct);
}

public interface IBiddingEngine
{
    Task<PlaceBidResult> PlaceBidAsync(PlaceBidCommand cmd, CancellationToken ct = default);
}

public sealed class BiddingEngine(IRedisAuctionStateStore store) : IBiddingEngine
{
    private readonly IRedisAuctionStateStore _store = store;

    public async Task<PlaceBidResult> PlaceBidAsync(PlaceBidCommand cmd, CancellationToken ct = default)
    {
        var snapshot = await _store.TryPlaceBidAtomicAsync(cmd.AuctionId, cmd.BidAmount, cmd.UserId, ct)
            .ConfigureAwait(false);

        return snapshot is null ? PlaceBidResult.Rejected() : PlaceBidResult.Accepted(snapshot);
    }
}
