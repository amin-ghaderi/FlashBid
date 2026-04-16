// Designed and Implemented by Amin Ghaderi

using FlashBid.Core;
using StackExchange.Redis;

namespace FlashBid.Infrastructure;

/// <summary>
/// Redis-backed auction state. All time/price checks and updates run atomically in Lua (single round-trip).
/// Prices are stored as integer cents. The hash field <c>end_time</c> is optional: Unix milliseconds (UTC);
/// when present, bids are rejected when the client-supplied current instant is &gt;= <c>end_time</c>.
/// <c>highest_bidder</c> is stored only after a winning bid (field omitted at creation).
/// New auctions set <c>status</c> to <c>active</c>; place-bid rejects when <c>status</c> is not <c>active</c>.
/// </summary>
public sealed class RedisAuctionStateStore(IConnectionMultiplexer connection) : IRedisAuctionStateStore
{
    private const string KeyPrefix = "flashbid:auction:";

    private static readonly string LuaScript = """
        local key = KEYS[1]
        local bid_cents = tonumber(ARGV[1])
        local user_id = ARGV[2]
        local now = tonumber(ARGV[3])

        if bid_cents == nil or now == nil then
          return nil
        end

        if redis.call('EXISTS', key) == 0 then
          return nil
        end

        -- Status gate: bids allowed only while the auction is explicitly "active".
        local status = redis.call('HGET', key, 'status')
        if status ~= 'active' then
          return nil
        end

        local end_raw = redis.call('HGET', key, 'end_time')
        if end_raw then
          local end_ts = tonumber(end_raw)
          if end_ts and now >= end_ts then
            return nil
          end
        end

        local price_raw = redis.call('HGET', key, 'price_cents')
        local current = 0
        if price_raw then
          current = tonumber(price_raw)
        end

        if bid_cents <= current then
          return nil
        end

        local ver_raw = redis.call('HGET', key, 'version')
        local version = 0
        if ver_raw then
          version = tonumber(ver_raw)
        end
        version = version + 1

        -- Accepted bid only: update price, set highest_bidder (field absent until first win), bump version.
        redis.call('HSET', key,
          'price_cents', bid_cents,
          'highest_bidder', user_id,
          'version', version)

        return { tostring(bid_cents), user_id, tostring(version) }
        """;

    /// <summary>
    /// Creates the auction hash only if <c>flashbid:auction:{id}</c> does not exist (atomic Lua + EXISTS).
    /// Sets <c>price_cents</c>, <c>end_time</c> (Unix ms), <c>version</c> = 0, <c>status</c> = active.
    /// Does not set <c>highest_bidder</c> (omitted until a bid wins).
    /// </summary>
    private static readonly string CreateAuctionLuaScript = """
        local key = KEYS[1]
        if redis.call('EXISTS', key) == 1 then
          return 0
        end
        redis.call('HSET', key,
          'price_cents', ARGV[1],
          'end_time', ARGV[2],
          'version', '0',
          'status', 'active')
        return 1
        """;

    private readonly IDatabase _db = connection.GetDatabase();

    public async Task<bool> CreateAuctionAsync(
        Guid auctionId,
        decimal initialPrice,
        long endTimeUnixMs,
        CancellationToken ct)
    {
        var key = KeyPrefix + auctionId.ToString("N");
        var priceCents = ToCents(initialPrice);

        var result = (RedisResult)await _db.ScriptEvaluateAsync(
            CreateAuctionLuaScript,
            new RedisKey[] { key },
            new RedisValue[] { priceCents, endTimeUnixMs }).WaitAsync(ct).ConfigureAwait(false);

        return !result.IsNull && (long)result == 1L;
    }

    public async Task<BidSnapshot?> TryPlaceBidAtomicAsync(
        Guid auctionId,
        decimal bidAmount,
        string userId,
        CancellationToken ct)
    {
        var key = KeyPrefix + auctionId.ToString("N");
        var bidCents = ToCents(bidAmount);
        var nowUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var result = (RedisResult)await _db.ScriptEvaluateAsync(
            LuaScript,
            new RedisKey[] { key },
            new RedisValue[] { bidCents, userId, nowUnixMs }).WaitAsync(ct).ConfigureAwait(false);

        if (result.IsNull)
            return null;

        var values = (RedisValue[]?)result;
        if (values is null || values.Length != 3)
            return null;

        var priceCents = (long)values[0];
        var highest = (string)values[1]!;
        var version = (long)values[2];

        return new BidSnapshot(FromCents(priceCents), highest, version);
    }

    private static long ToCents(decimal amount) =>
        (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);

    private static decimal FromCents(long cents) => cents / 100m;
}
