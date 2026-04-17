# Testing

## Load testing — tool

- **[Artillery](https://www.artillery.io/)** (v2), Node.js 18+
- Scenarios and docs: **`load-tests/`** (see `load-tests/README.md`).

## Scenarios (examples)

| Scenario file | Intent |
|---------------|--------|
| `auction-bid.yml` | Concurrent **random** bids against one auction (`AUCTION_ID` required). |
| `auction-bid-incremental.yml` | Deterministic **incremental** amounts (sequential winners). |
| `auction-bid-sameprice.yml` | Same-price attempts (**contention / rejection** pressure). |
| `auction-bid-singleuser.yml` | **Single VU**, many bids — throughput of the accept path without multi-client races. |
| `auction-create.yml` | Many **new** auctions (`POST /auctions`). |
| `auction-mixed.yml` | Mixed: create, static asset, negotiate-style traffic. |

Exact phase settings (arrival rate, duration) live in each YAML under `load-tests/scenarios/`.

## Goals (what to measure)

- **Requests per second** and latency (Artillery summary).
- **Accepted vs rejected** bids — with **random** or **same-price** scenarios, **rejections are expected** and dominate; interpret counts against scenario design.
- **Stability** under ramp (errors, timeouts, Redis/API saturation).

## HTTP API surface used in tests

- **`POST /auctions`** — creates auctions. Validates allowed durations **60, 180, 300** seconds.
- **`POST /test/bid`** — **internal load-test bridge** (`TestController`): same `IBiddingEngine` + Redis as SignalR; **synthetic `userId` per request**; on acceptance, **`BidUpdated`** via `IHubContext<AuctionHub>`. Hidden from Swagger but **reachable** unless blocked by deployment.

## Typical run

From repo root (so `processor` paths resolve):

```bash
artillery run load-tests/scenarios/auction-bid.yml
```

Set `AUCTION_ID` to a live auction GUID (PowerShell: `$env:AUCTION_ID="..."`).

## What is not in-repo

- Automated **unit/integration** test projects (xUnit/NUnit) for `BiddingEngine` or Lua semantics; validation is primarily manual + Artillery.

## Safety

Run high-volume tests against **non-production** Redis and API instances; keys accumulate and CPU/memory can saturate (see `load-tests/README.md`).
