// Designed and Implemented by Amin Ghaderi

namespace FlashBid.Api.Models;

public sealed class CreateAuctionResponse
{
    public Guid AuctionId { get; set; }

    /// <summary>Unix time in milliseconds (UTC), aligned with Redis <c>end_time</c>.</summary>
    public long EndTime { get; set; }
}
