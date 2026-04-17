// Designed and Implemented by Amin Ghaderi

namespace FlashBid.Api.Models;

/// <summary>
/// INTERNAL load-test payload only. Not for production clients.
/// </summary>
public sealed class TestBidRequest
{
    public Guid AuctionId { get; set; }

    public decimal Amount { get; set; }
}
