# Architecture

## Components

| Piece | Role |
|-------|------|
| **API** (`FlashBid.Api`, ASP.NET Core) | Host, `POST /auctions`, internal `POST /test/bid`, static demo UI, maps SignalR hub. |
| **Realtime** (`FlashBid.Realtime`) | `AuctionHub` — join group, place bid, broadcast results. |
| **Core** (`FlashBid.Core`) | `IBiddingEngine`, `BiddingEngine`, `AuctionInitializer`, bid command/result types. |
| **Infrastructure** (`FlashBid.Infrastructure`) | `RedisAuctionStateStore` — atomic Lua for create + place-bid. |
| **Redis** | Live auction state (hashes + Lua). |
| **PostgreSQL** | **Not present in the codebase today**; planned for durable history / final results per root `README.md`. |

## Solution layout (projects)

| Project | Role |
|---------|------|
| **FlashBid.Api** | `Program.cs`, REST controllers, `wwwroot`, `MapHub<AuctionHub>("/auctionHub")`. |
| **FlashBid.Core** | Domain services and bid types. |
| **FlashBid.Infrastructure** | Redis implementation of `IRedisAuctionStateStore`. |
| **FlashBid.Realtime** | SignalR hub. |
| **FlashBid.Domain** | Placeholder assembly (no hand-written source files yet). |

## Dependency direction

- **Api** references **Core**, **Infrastructure**, and **Realtime**.
- **Infrastructure** implements **Core** (`IRedisAuctionStateStore`).
- **Realtime** depends on **Core** (`IBiddingEngine`, `PlaceBidCommand`).

## Flow (bid)

1. **Client** sends a bid — either **SignalR** `PlaceBid` on `/auctionHub`, or **HTTP** `POST /test/bid` (load tests).
2. **App** calls **`IBiddingEngine.PlaceBidAsync`** (hub or `TestController`).
3. **Redis** runs the **place-bid Lua script**: validate + update price / bidder / version atomically.
4. **Result** returns to the caller — `PlaceBidResult` accepted or rejected.
5. On acceptance, **SignalR** broadcasts **`BidUpdated`** to the auction **group** (hub path or `IHubContext<AuctionHub>` from the test controller).

**Create auction:** `POST /auctions` → `AuctionInitializer` → Redis create script (no SignalR in that path).

## Design decisions

- **Redis + Lua** for **atomicity** on a hot key (no lost updates between read and write).
- **Lua script** avoids **race conditions** that would appear with separate GET/SET from the app.
- **SignalR** delivers **realtime updates** to every connection joined to that auction’s group.

## Demo client

Static **index.html** under `FlashBid.Api/wwwroot` connects to `/auctionHub`, joins a group, and invokes hub methods.

## Configuration

- Redis connection string: `ConnectionStrings:Redis` (see `FlashBid.Api/appsettings.json`).

Further detail: [03-bidding-engine](03-bidding-engine.md), [04-realtime](04-realtime.md), [05-infrastructure](05-infrastructure.md).
