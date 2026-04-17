// Designed and Implemented by Amin Ghaderi

using FlashBid.Api.Models;
using FlashBid.Core;
using FlashBid.Realtime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace FlashBid.Api.Controllers;

/// <summary>
/// INTERNAL test bridge: exposes HTTP POST for load tools that cannot drive SignalR bidding.
/// Forwards to <see cref="IBiddingEngine"/> only; on success, mirrors <see cref="AuctionHub"/> realtime fan-out.
/// </summary>
[ApiController]
[Route("test")]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class TestController(IBiddingEngine biddingEngine, IHubContext<AuctionHub> hubContext)
    : ControllerBase
{
    /// <summary>
    /// INTERNAL: simulate a bid via the same path as the hub (BiddingEngine → Redis).
    /// User id is synthetic per request so concurrent load tests act as distinct bidders.
    /// </summary>
    [HttpPost("bid")]
    public async Task<ActionResult<PlaceBidResult>> PlaceTestBid(
        [FromBody] TestBidRequest request,
        CancellationToken cancellationToken)
    {
        var userId = $"load-test-{Guid.NewGuid():N}";
        var cmd = new PlaceBidCommand(request.AuctionId, request.Amount, userId);
        var result = await biddingEngine.PlaceBidAsync(cmd, cancellationToken).ConfigureAwait(false);

        if (result.IsAccepted)
        {
            var groupName = request.AuctionId.ToString();
            await hubContext.Clients.Group(groupName).SendAsync("BidUpdated", result, cancellationToken)
                .ConfigureAwait(false);
        }

        return Ok(result);
    }
}
