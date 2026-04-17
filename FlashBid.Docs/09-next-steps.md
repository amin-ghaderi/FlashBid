# Next steps

_Living doc — merge investigation items with product hardening._

## Immediate (investigation / observability)

1. **Logging before Redis** — log `AuctionId`, `BidAmount`, `UserId` (sanitized), and **HTTP vs hub** entry on each attempt (optional sampling under extreme load).
2. **Log Redis outcome** — after `TryPlaceBidAtomicAsync`, log **accepted vs rejected**; ideally map `nil` to **reason codes** once the Lua script returns structured errors (requires script/API change).
3. **Reconcile counts** — compare **Artillery `http.requests`** vs **accepted** count (parse `IsAccepted` from responses in a processor, or export metrics). Remember: **200 OK ≠ bid accepted**.

## Testing

4. **Controlled throughput** — ramp **5 → 50** bids/sec with a **known-good** scenario (e.g. single auction, **incremental** amounts, one or few VUs) to separate **logic** from **saturation**.
5. **Remove randomness** when debugging acceptance — use `auction-bid-incremental.yml` / single-user variants so most attempts **should** win if the auction is valid.
6. **Repeat with concurrent VUs** only after baseline sequential behavior is understood.

## Investigation (latency by layer)

7. Measure **API** handling time (middleware → controller/hub → engine).
8. Measure **Redis** round-trip (`ScriptEvaluateAsync`) — client-side timing or Redis `SLOWLOG` in dev.
9. Measure **SignalR** broadcast time separately — does not change Redis accept/reject but explains UI lag.

**Goal:** identify whether low acceptance is **expected rejection**, **Redis contention**, **API limits**, or **client/scenario misconfiguration**.

## Hardening (product)

10. **Gate or remove `/test/bid` outside Development** — feature flag, environment check, or dedicated load-test deployment.
11. **Authentication** on hub and sensitive REST endpoints; stable `UserId` in `PlaceBidCommand`.
12. **Rate limiting** on bid endpoints.

## Domain and API

13. **Flesh out `FlashBid.Domain`** if you want richer models.
14. **Richer rejection contract** — typed reasons (too low, ended, not found, inactive).
15. **Auction closure** — `status` transitions, final snapshot API.

## Persistence and scale

16. **Async persistence** to **PostgreSQL** (or similar) without blocking Lua — when you add it, update [05-infrastructure](05-infrastructure.md).
17. **Health checks**, **SignalR backplane** for multi-node, **CI** with tests + optional Artillery smoke.

Keep [07-current-state](07-current-state.md) and this file in sync as work lands.
