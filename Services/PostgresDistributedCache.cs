using Dapper;
using Microsoft.Extensions.Caching.Distributed;

namespace RetakePortal.Services;

public class PostgresDistributedCache : IDistributedCache
{
    private readonly DatabaseService _db;

    public PostgresDistributedCache(DatabaseService db) => _db = db;

    public byte[]? Get(string key)
    {
        using var conn = _db.Supabase();
        return conn.QueryFirstOrDefault<byte[]>(
            "SELECT value FROM sessions WHERE id = @key AND expires_at > NOW()",
            new { key });
    }

    public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
    {
        using var conn = _db.Supabase();
        return await conn.QueryFirstOrDefaultAsync<byte[]>(
            "SELECT value FROM sessions WHERE id = @key AND expires_at > NOW()",
            new { key });
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        var (expiresAt, slidingSeconds) = GetExpiry(options);
        using var conn = _db.Supabase();
        conn.Execute(@"
            INSERT INTO sessions (id, value, expires_at, sliding_seconds)
            VALUES (@key, @value, @expiresAt, @slidingSeconds)
            ON CONFLICT (id) DO UPDATE
            SET value = @value, expires_at = @expiresAt, sliding_seconds = @slidingSeconds",
            new { key, value, expiresAt, slidingSeconds });
    }

    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
        CancellationToken token = default)
    {
        var (expiresAt, slidingSeconds) = GetExpiry(options);
        using var conn = _db.Supabase();
        await conn.ExecuteAsync(@"
            INSERT INTO sessions (id, value, expires_at, sliding_seconds)
            VALUES (@key, @value, @expiresAt, @slidingSeconds)
            ON CONFLICT (id) DO UPDATE
            SET value = @value, expires_at = @expiresAt, sliding_seconds = @slidingSeconds",
            new { key, value, expiresAt, slidingSeconds });
    }

    public void Refresh(string key)
    {
        using var conn = _db.Supabase();
        conn.Execute(@"
            UPDATE sessions
            SET expires_at = NOW() + (sliding_seconds * INTERVAL '1 second')
            WHERE id = @key AND sliding_seconds IS NOT NULL", new { key });
    }

    public async Task RefreshAsync(string key, CancellationToken token = default)
    {
        using var conn = _db.Supabase();
        await conn.ExecuteAsync(@"
            UPDATE sessions
            SET expires_at = NOW() + (sliding_seconds * INTERVAL '1 second')
            WHERE id = @key AND sliding_seconds IS NOT NULL", new { key });
    }

    public void Remove(string key)
    {
        using var conn = _db.Supabase();
        conn.Execute("DELETE FROM sessions WHERE id = @key", new { key });
    }

    public async Task RemoveAsync(string key, CancellationToken token = default)
    {
        using var conn = _db.Supabase();
        await conn.ExecuteAsync("DELETE FROM sessions WHERE id = @key", new { key });
    }

    private static (DateTimeOffset expiresAt, int? slidingSeconds) GetExpiry(DistributedCacheEntryOptions options)
    {
        if (options.AbsoluteExpiration.HasValue)
            return (options.AbsoluteExpiration.Value, null);

        if (options.AbsoluteExpirationRelativeToNow.HasValue)
            return (DateTimeOffset.UtcNow + options.AbsoluteExpirationRelativeToNow.Value, null);

        if (options.SlidingExpiration.HasValue)
        {
            var secs = (int)options.SlidingExpiration.Value.TotalSeconds;
            return (DateTimeOffset.UtcNow + options.SlidingExpiration.Value, secs);
        }

        return (DateTimeOffset.UtcNow.AddHours(8), null);
    }
}
