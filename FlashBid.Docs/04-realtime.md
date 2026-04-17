# Realtime system

## Technology

- **ASP.NET Core SignalR**
- Hub type: **`AuctionHub`** (`FlashBid.Realtime`), mapped at **`/auctionHub`** in `FlashBid.Api`.

## Hub

- **Dependencies**: `IBiddingEngine` (constructor injection).

## Hub methods

| Method | Behavior |
|--------|----------|
| `JoinAuction(auctionId)` | Adds the connection to a SignalR group named exactly `auctionId` (string). Clients must join to receive group broadcasts. |
| `PlaceBid(auctionId, amount)` | Parses `auctionId` as `Guid`. On parse failure: **`BidRejected`** to caller. Uses `Context.UserIdentifier ?? Context.ConnectionId` as `userId`. Calls `PlaceBidAsync`. On success: **`BidUpdated`** to the **group**; on rejection: **`BidRejected`** to **caller only**. |

## Client flow

1. Client **connects** to `/auctionHub`.
2. Client **joins** the auction group (`JoinAuction`).
3. Client may invoke **`PlaceBid`** or only listen.
4. On accepted bids (from any participant), clients in the group receive **`BidUpdated`** with `PlaceBidResult`.

## Client events

- **`BidUpdated`**: payload is **`PlaceBidResult`** (accepted bid details).
- **`BidRejected`**: auction id string (or invalid id on parse failure).

## Responsibility

- **Broadcast** accepted bids to everyone in the auction group.
- Keep the **demo UI** (`wwwroot/index.html`) responsive: log events for quick manual verification.

## Demo UI

`FlashBid.Api/wwwroot/index.html` loads the SignalR client from CDN, subscribes to the events above, and exposes connect / join / bid actions.

## Operational note

The group name is the **string** `auctionId`. HTTP test bidding uses `request.AuctionId.ToString()` for the group — use the **same string format** when joining (default GUID string with hyphens matches `Guid.ToString()`).
