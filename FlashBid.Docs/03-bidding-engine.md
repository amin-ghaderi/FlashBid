# Bidding engine

## What

Handles **validation** and **acceptance** of bids: one attempt either wins (updates state) or loses (no change). All critical checks and writes run inside **Redis Lua** for the auction key.

## Rules (summary)

- Bid must be **strictly greater** than the current stored price (after cent rounding).
- Only **one** winning mutation per script run — concurrent losers get **rejected** without corrupting state.
- **`version`** increments on each **accepted** bid (monotonic).

See [05-infrastructure](05-infrastructure.md) for the full rule chain (auction exists, `status == active`, optional `end_time`, etc.).

## How

- **`BiddingEngine`** calls **`IRedisAuctionStateStore.TryPlaceBidAtomicAsync`**.
- **`RedisAuctionStateStore`** runs the **place-bid Lua script** in a single round trip: validate + optional update + return snapshot or `nil`.

## Public API (types)

- **`IBiddingEngine`**: `PlaceBidAsync(PlaceBidCommand, CancellationToken)`.
- **`PlaceBidCommand`**: `AuctionId`, `BidAmount`, `UserId`.
- **`PlaceBidResult`**: `IsAccepted`, `CurrentPrice`, `HighestBidder`, `Version`; or `Rejected()` sentinel.

## Code reference

| Area | Files |
|------|--------|
| Engine + initializer | `FlashBid.Core/BiddingEngine.cs`, `FlashBid.Core/AuctionInitializer.cs`, `FlashBid.Core/PlaceBidResult.cs` (and related types) |
| Redis + Lua | `FlashBid.Infrastructure/RedisAuctionStateStore.cs` |
| SignalR entry | `FlashBid.Realtime/AuctionHub.cs` |
| HTTP load-test entry | `FlashBid.Api/Controllers/TestController.cs` |

There is **no** `BidController` in this repo; auction creation is **`AuctionController`** (`POST /auctions`).

## What the engine does not do

- It does not write to **PostgreSQL**, enqueue outbox events, or send notifications — only returns a result; callers handle SignalR fan-out when applicable.
