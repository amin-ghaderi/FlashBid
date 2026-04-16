// Designed and Implemented by Amin Ghaderi

namespace FlashBid.Api.Models;

public sealed class CreateAuctionRequest
{
    public decimal StartPrice { get; set; }

    public int DurationSeconds { get; set; }
}
