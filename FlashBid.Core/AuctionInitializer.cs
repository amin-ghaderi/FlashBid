// Designed and Implemented by Amin Ghaderi

namespace FlashBid.Core;

/// <summary>
/// Explicit auction creation; does not overwrite existing Redis state.
/// </summary>
public sealed class AuctionInitializer(IRedisAuctionStateStore store)
{
    public Task<bool> CreateAuctionAsync(
        Guid auctionId,
        decimal initialPrice,
        long endTimeUnixMs,
        CancellationToken ct = default) =>
        store.CreateAuctionAsync(auctionId, initialPrice, endTimeUnixMs, ct);
}
