// Designed and Implemented by Amin Ghaderi
//
// Artillery processor helpers for FlashBid load tests.

/** Shared bid counter for auction-bid-incremental (single Artillery worker process). */
let globalBid = 100;

/** auction-bid.yml: counts business outcome vs HTTP 200. */
let acceptedCount = 0;
let rejectedCount = 0;

/**
 * Builds a JSON body for POST /auctions (camelCase matches ASP.NET Core defaults).
 * @param {{ startPrice?: number, durationSeconds?: number }} overrides
 */
function buildCreateAuctionPayload(overrides = {}) {
  const allowedDurations = [60, 180, 300];
  const durationSeconds =
    overrides.durationSeconds ??
    allowedDurations[Math.floor(Math.random() * allowedDurations.length)];

  const startPrice =
    overrides.startPrice ?? roundMoney(10 + Math.random() * 500);

  return {
    startPrice,
    durationSeconds,
  };
}

function roundMoney(n) {
  return Math.round(n * 100) / 100;
}

/**
 * Random bid amount in cents-based decimal (aligned with server-side cent rounding).
 * Uses BID_MIN / BID_MAX env (defaults: 1.00 .. 5000.00).
 */
function randomBidAmount() {
  const min = Number(process.env.BID_MIN ?? 1);
  const max = Number(process.env.BID_MAX ?? 5000);
  const v = min + Math.random() * (max - min);
  return roundMoney(v);
}

/** Random delay in seconds (1 decimal) for Artillery `think` steps. */
function randomDelaySeconds() {
  const minS = Number(process.env.THINK_MIN_SECONDS ?? 0.1);
  const maxS = Number(process.env.THINK_MAX_SECONDS ?? 2);
  const s = minS + Math.random() * (maxS - minS);
  return Math.round(s * 10) / 10;
}

function setCreatePayload(context, events, done) {
  const body = buildCreateAuctionPayload();
  context.vars.startPrice = body.startPrice;
  context.vars.durationSeconds = body.durationSeconds;
  return done();
}

function setAuctionIdFromEnv(context, events, done) {
  const id = process.env.AUCTION_ID;
  if (!id || !String(id).trim()) {
    return done(
      new Error(
        "AUCTION_ID is required (existing auction GUID). Example: set AUCTION_ID=... before artillery run."
      )
    );
  }
  context.vars.auctionId = String(id).trim();
  return done();
}

function setRandomBidAmountVar(context, events, done) {
  context.vars.bidAmount = randomBidAmount();
  return done();
}

/** Random amount in [100, 1000] for POST /test/bid (per request). */
function setRandomTestBidAmount(context, events, done) {
  const min = 100;
  const max = 1000;
  context.vars.amount = roundMoney(min + Math.random() * (max - min));
  return done();
}

/** Variable pause between steps (uses THINK_MIN_SECONDS / THINK_MAX_SECONDS). */
function randomThinkWait(context, events, done) {
  const ms = Math.round(randomDelaySeconds() * 1000);
  setTimeout(() => done(), ms);
}

/** Global +1 per request; all virtual users in this worker share the same sequence. */
function setGlobalIncrementalBid(context, events, done) {
  globalBid += 1;
  context.vars.amount = globalBid;
  return done();
}

function trackAcceptance(context, events, done) {
  const v = context.vars.isAccepted;
  const ok = v === true || v === "true" || String(v).toLowerCase() === "true";
  if (ok) {
    acceptedCount++;
  } else {
    rejectedCount++;
  }
  return done();
}

/**
 * Invoked once from auction-bid.yml top-level `after` (Artillery: after all scenarios finish; once per worker).
 * Artillery does not expose `events.on('done')` on the processor module; `after` is the supported lifecycle hook.
 */
function printSummary(context, events, done) {
  const total = acceptedCount + rejectedCount;
  console.log("===== FINAL BID RESULTS =====");
  console.log("Accepted: " + acceptedCount);
  console.log("Rejected: " + rejectedCount);
  console.log("Total: " + total);
  return done();
}

module.exports = {
  buildCreateAuctionPayload,
  randomBidAmount,
  randomDelaySeconds,
  setCreatePayload,
  setAuctionIdFromEnv,
  setRandomBidAmountVar,
  setRandomTestBidAmount,
  randomThinkWait,
  setGlobalIncrementalBid,
  trackAcceptance,
  printSummary,
};
