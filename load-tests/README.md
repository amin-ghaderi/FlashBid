# FlashBid load tests (Artillery)

Designed and Implemented by Amin Ghaderi

External HTTP load tests for the FlashBid API. This folder is independent of the .NET application code.

## Prerequisites

- [Node.js](https://nodejs.org/) 18+
- [Artillery](https://www.artillery.io/) v2 (`npm install -g artillery@latest`)
- FlashBid.Api running and reachable (default: `http://localhost:5091`)
- Redis available if the API depends on it (see warnings below)

## Layout

| Path | Purpose |
|------|---------|
| `config/base-config.yml` | Canonical `target` and `phases` (warm-up, ramp-up, peak). Copy into scenarios when changing load globally. |
| `scenarios/auction-create.yml` | POST `/auctions` — creates many auctions. |
| `scenarios/auction-bid.yml` | Requires `AUCTION_ID`; **POST `/test/bid`** (real `IBiddingEngine` + Redis Lua), random amount 100–1000 per request. |
| `scenarios/auction-mixed.yml` | Mix: create auction, GET `/index.html` (client load), POST negotiate. |
| `scripts/helpers.js` | Payload builders, random bid amount, delays. |

## How to run

From the **repository root** (so relative `processor` paths resolve):

```bash
artillery run load-tests/scenarios/auction-create.yml
```

```bash
artillery run load-tests/scenarios/auction-mixed.yml
```

**Single-auction scenario** (requires an existing auction GUID):

```bash
# Windows PowerShell
$env:AUCTION_ID="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
artillery run load-tests/scenarios/auction-bid.yml
```

```bash
# Linux / macOS
export AUCTION_ID="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
artillery run load-tests/scenarios/auction-bid.yml
```

Create an auction first (e.g. `POST /auctions` or the web UI), then copy the returned `auctionId` into `AUCTION_ID`.

## What each scenario tests

- **auction-create.yml** — Throughput and correctness pressure on auction creation (Redis hash creation, validation of duration values). Uses random allowed `durationSeconds` and `startPrice` from helpers.
- **auction-bid.yml** — Concurrent **real bids** against one auction: `POST /test/bid` with `auctionId` from **`AUCTION_ID`** (required; fails fast if unset) and a **random `amount` between 100 and 1000** per request. This hits the same **BiddingEngine → Redis Lua** path as production bidding (test-only HTTP bridge). Phases ramp **5 → 20 → 50** virtual users per second.
- **auction-mixed.yml** — Combines API writes (create), static asset fetch (simulates loading the test UI / “join” client bundle), and negotiate calls.

## Changing load (arrivalRate, duration)

Phases are defined under `config.phases` in each scenario YAML (kept in sync with `config/base-config.yml`).

- **`duration`** — Length of the phase in **seconds**.
- **`arrivalRate`** — New virtual users started **per second** (approximate).

Example: to run a lighter peak:

```yaml
phases:
  - duration: 60
    arrivalRate: 2
    name: "warm-up"
```

Edit the scenario file (or `base-config.yml` as reference), then re-run Artillery.

Optional environment variables used by `scripts/helpers.js`:

| Variable | Default | Meaning |
|----------|---------|---------|
| `BID_MIN` | `1` | Min random bid amount for vars / headers |
| `BID_MAX` | `5000` | Max random bid amount |
| `THINK_MIN_SECONDS` | `0.1` | Min think time between steps |
| `THINK_MAX_SECONDS` | `2` | Max think time between steps |

## Redis and infrastructure warnings

- Auction creation and bidding **persist state in Redis**. High `arrivalRate` or long `duration` phases can **saturate CPU, memory, or connections** on the Redis host and on the API process.
- Run load tests against **non-production** environments unless you have capacity planning and monitoring in place.
- **`abortConnect=false`** (if configured on the API connection string) allows the app to start without Redis; under load, Redis unavailability will still cause errors and timeouts—watch logs and metrics.
- Creating many auctions fills Redis with many keys; plan cleanup or use a dedicated Redis instance for testing.

## Notes

- All scenario comments and identifiers are in **English**.
- This layer **does not modify** application source code; add HTTP endpoints only if you later want Artillery to hit bid APIs directly.
