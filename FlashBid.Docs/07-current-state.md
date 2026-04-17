# Current state

_Living doc — update as the system changes._

## Implemented

- **Auction API**: `POST /auctions` (`AuctionController`) with duration validation; Redis create via `AuctionInitializer`.
- **Redis Lua bidding logic**: atomic create + place-bid in `RedisAuctionStateStore`.
- **SignalR realtime**: `/auctionHub`, `JoinAuction`, `PlaceBid`, `BidUpdated` / `BidRejected`.
- **HTTP test bridge**: `POST /test/bid` for Artillery; mirrors group broadcast on success.
- **Multiple load-test scenarios** under `load-tests/scenarios/`.
- **Static demo UI** in `wwwroot`.
- **Target framework**: `net10.0` for the API project.

## Partial / placeholder

- **`FlashBid.Domain`**: no hand-written domain models in source yet.
- **PostgreSQL / durable history**: not in this repo (see [05-infrastructure](05-infrastructure.md)).

## Load-test observations

- The API can sustain a **high request rate** in local/single-machine setups (see Artillery reports).
- **Accepted** bids are often **far lower** than **total requests** when scenarios use **random amounts**, **many concurrent bidders**, or **same-price** contention — Lua only accepts **strictly higher** prices, so most parallel attempts are **expected rejections**, not necessarily a defect.

## Suspicion / open questions

- If **acceptance rate** seems wrong **for a scenario designed so most bids should win** (e.g. single-user incremental), investigate per-layer latency and logging (see [09-next-steps](09-next-steps.md)).
- If **UI logs** disagree with Artillery counts, verify **group membership**, **auction id string format**, and whether the UI is connected for the full test window.

## Configuration in use

- Single **Redis** connection string; default `localhost:6379` with `abortConnect=false`.

## Documentation

- **`FlashBid.Docs`** — structured technical guide.
- Repo root **`README.md`** — high-level challenges and solutions.
