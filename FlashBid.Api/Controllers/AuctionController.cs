// Designed and Implemented by Amin Ghaderi

using FlashBid.Api.Models;
using FlashBid.Core;
using Microsoft.AspNetCore.Mvc;

namespace FlashBid.Api.Controllers;

[ApiController]
[Route("auctions")]
public sealed class AuctionController(AuctionInitializer initializer) : ControllerBase
{
    private static readonly HashSet<int> AllowedDurations = [60, 180, 300];

    [HttpPost]
    public async Task<ActionResult<CreateAuctionResponse>> Create(
        [FromBody] CreateAuctionRequest request,
        CancellationToken cancellationToken)
    {
        if (!AllowedDurations.Contains(request.DurationSeconds))
            return BadRequest();

        var auctionId = Guid.NewGuid();
        var end = DateTimeOffset.UtcNow.AddSeconds(request.DurationSeconds);
        var endUnixMs = end.ToUnixTimeMilliseconds();

        var created = await initializer
            .CreateAuctionAsync(auctionId, request.StartPrice, endUnixMs, cancellationToken)
            .ConfigureAwait(false);

        if (!created)
            return Conflict();

        return Ok(new CreateAuctionResponse { AuctionId = auctionId, EndTime = endUnixMs });
    }
}
