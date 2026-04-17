# Infrastructure

## Redis

- **Client**: `StackExchange.Redis` `IConnectionMultiplexer` (singleton in `Program.cs`).
- **Store**: `RedisAuctionStateStore` implements `IRedisAuctionStateStore`.
- **Role**: holds **current auction state**, runs **atomic updates** via Lua.

### Why Redis (for this system)

- **Low latency** on the bidding hot path.
- **Atomic operations** via **Lua** — check-and-set without application-level races on a single key.

## Key naming

- Pattern: `flashbid:auction:{auctionId:N}` (32 hex digits, no hyphens — `Guid.ToString("N")`).

## Hash fields

| Field | Meaning |
|-------|---------|
| `price_cents` | Current price in integer cents. |
| `end_time` | Optional; Unix milliseconds (UTC). Bids rejected when `now >= end_time`. |
| `version` | Monotonic counter incremented on each accepted bid; `0` at creation. |
| `status` | `"active"` at creation; place-bid requires this value. |
| `highest_bidder` | Set on first accepted bid; absent at creation. |

## Scripts

Two Lua scripts (embedded in `RedisAuctionStateStore.cs`):

1. **Create auction** — runs only if key does not exist; initializes fields; returns `1` if created, `0` if already present.
2. **Place bid** — validates existence, status, optional end time, strictly increasing price; updates price, bidder, version; returns result array or `nil` on rejection.

All bidding validation and mutation for a single key happen in **one** `EVAL` round trip.

## PostgreSQL

- **Not implemented** in this repository: there is no schema, no EF/Npgsql usage, no write path from the bidding engine to SQL.
- **Intended direction** (per repo root `README.md`): durable **auction history**, **final results**, or async persistence **after** bidding — without blocking the Redis/Lua path. When added, this doc should list connection settings, tables, and how it relates to Redis state.

## Configuration

`appsettings.json` example:

```json
"ConnectionStrings": {
  "Redis": "localhost:6379,abortConnect=false"
}
```

`abortConnect=false` allows the process to start if Redis is temporarily down; bidding will fail until Redis is reachable.

## Persistence today

Redis is the **system of record** for live auction state in this codebase.
