# Known issues and gaps

_Living doc — not a full bug tracker; combines code gaps and load-test findings._

## Load testing / metrics

### Acceptance rate vs request count

- Artillery (or any client) can emit **thousands of bid requests** while the **UI** or Redis shows **far fewer accepted** updates.
- **Often expected**: scenarios like **random bids** or **same-price** floods produce **mostly rejections** by design (only strictly higher bids win). **Many concurrent winners** on one auction key are impossible.
- **Unexpected** if a **single sequential incremental** test shows near-zero acceptance — then investigate auction id, expiry, connectivity, or errors.

### Metrics mismatch (Artillery vs UI)

- Artillery reports **HTTP-level** stats for `/test/bid` (e.g. 200 responses). A **200** can still carry `PlaceBidResult.IsAccepted == false` — the API returns the body; “request succeeded” ≠ “bid won”.
- **SignalR** events only fire for **accepted** bids (and for rejections, hub sends `BidRejected` to caller; load tests may not listen).
- **Missing instrumentation**: today there is little structured logging of **rejection reasons** or per-layer timings (see [05-infrastructure](05-infrastructure.md) observability gap).

### Throughput feel “capped”

- **Accepted bids/sec** are bounded by **business rules** (one winner per successful attempt) and **infrastructure** (Redis single-key contention, CPU, network).
- Distinguish **rejection-heavy** scenarios from true **saturation**.

### Hypotheses when investigating

- **Logic-level rejection** — amounts too low, auction ended, wrong id (most common under random/same-price load).
- **Redis contention** — latency growth under many parallel scripts on one key.
- **API / host limits** — threadpool, connection limits, client-side VU configuration.
- **SignalR** — fan-out cost; not on the critical path for **accept/reject** decision but affects what observers see.

## Security and exposure

- **`/test/bid` is a real endpoint** in the running API. It is internal and hidden from Swagger, but **not disabled by environment**. Anyone who can reach the host can post bids with synthetic identities unless blocked by network policy or future middleware.
- **No authentication** on the hub or on auction creation in the sample.

## Correctness and product rules

- **Minimum bid increment** is not modeled — any strictly higher decimal amount wins after cent rounding.
- **`status` transitions** beyond `"active"` are not implemented in API code (no formal “closed” flow).

## Observability

- **Structured logging, metrics, and tracing** for rejection reasons, Redis latency, and SignalR fan-out are minimal.
- Lua **`nil`** does not distinguish **why** (missing auction vs ended vs low bid) in the API response today.

## Operations

- **Redis key growth** under load tests requires cleanup or TTL strategy (not defined in code).
- **Single Redis instance** assumptions in the sample.

## Testing

- **No in-solution automated tests** for Lua edge cases.
- Load tests use **HTTP**; they do not fully stress SignalR churn or backplane scale-out.

Update this file when you close a gap or introduce a new risk.
