# System overview

## What

FlashBid is a **real-time auction** backend: many clients can **place bids concurrently**; the system must keep a single consistent winning price and bidder per auction.

## Core features

- **Realtime bidding** via **SignalR** (`/auctionHub`): join an auction group, invoke `PlaceBid`, receive `BidUpdated` / `BidRejected`.
- **Atomic bid validation** using **Redis** and **Lua** (check + update in one script execution).
- **Auction lifecycle (create)** via HTTP: `POST /auctions` with start price, allowed duration, and end time stored in Redis.
- **Load testing** under concurrency: **Artillery** scenarios under `load-tests/`, using `POST /test/bid` as an HTTP bridge to the same bidding engine.

## Goals

- **Atomic bidding**: concurrent `PlaceBid` operations serialize inside Redis script execution, not in application memory.
- **Fast path**: bidding does not depend on a relational database; live state lives in **Redis**.
- **Live updates**: successful bids are pushed to subscribers via **SignalR** groups keyed by auction id.
- **Correctness and performance** under high-frequency bidding attempts (many requests will legitimately **reject** when amounts are random, duplicate, or too low — see [08-known-issues](08-known-issues.md)).

## Non-goals (today)

- Durable audit history or settlement in **PostgreSQL** (described as a future direction in the repo root `README.md`; not implemented in this repo).
- Full product **authentication and authorization** for bidders (hub uses `UserIdentifier` when present, otherwise `ConnectionId`).
- Multi-region or Redis Cluster deployment guides.

## Technology

- **.NET** (solution targets `net10.0` for the web API and libraries).
- **ASP.NET Core**: controllers, SignalR, static files for a small demo client.
- **StackExchange.Redis**: connection multiplexer and script evaluation.
- **Node.js / Artillery** (`load-tests/`): HTTP load and scenario tests.

## Related reading

- [02-architecture](02-architecture.md) — how projects map to responsibilities.
- [07-current-state](07-current-state.md) — implemented surface area.
