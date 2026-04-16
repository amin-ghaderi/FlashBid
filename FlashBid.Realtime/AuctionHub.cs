// Designed and Implemented by Amin Ghaderi

using FlashBid.Core;
using Microsoft.AspNetCore.SignalR;

namespace FlashBid.Realtime;

public sealed class AuctionHub(IBiddingEngine biddingEngine) : Hub
{
    public Task JoinAuction(string auctionId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, auctionId);

    public async Task PlaceBid(string auctionId, decimal amount)
    {
        if (!Guid.TryParse(auctionId, out var id))
        {
            await Clients.Caller.SendAsync("BidRejected", auctionId).ConfigureAwait(false);
            return;
        }

        var userId = Context.UserIdentifier ?? Context.ConnectionId;
        var result = await biddingEngine
            .PlaceBidAsync(new PlaceBidCommand(id, amount, userId), Context.ConnectionAborted)
            .ConfigureAwait(false);

        if (result.IsAccepted)
        {
            await Clients.Group(auctionId).SendAsync("BidUpdated", result).ConfigureAwait(false);
        }
        else
        {
            await Clients.Caller.SendAsync("BidRejected", auctionId).ConfigureAwait(false);
        }
    }
}
