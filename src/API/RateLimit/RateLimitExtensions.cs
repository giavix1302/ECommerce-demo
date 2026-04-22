using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using StackExchange.Redis;

namespace API.RateLimit;

public static class RateLimitExtensions
{
    public static IServiceCollection AddRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis");
        var useRedis = !string.IsNullOrWhiteSpace(redisConnection);

        IConnectionMultiplexer? redis = null;
        if (useRedis)
            redis = ConnectionMultiplexer.Connect(redisConnection!);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (ctx, ct) =>
            {
                ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await ctx.HttpContext.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    data = (object?)null,
                    message = "Too many requests. Please slow down and try again later.",
                    errors = (object?)null
                }, ct);
            };

            // -------------------------------------------------------
            // AUTH STRICT — 5 req/min per IP (login, register)
            // -------------------------------------------------------
            options.AddPolicy(RateLimitPolicy.AuthStrict, ctx =>
            {
                var ip = GetIp(ctx);
                return BuildFixedWindow(
                    partitionKey: $"rl:auth_strict:{ip}",
                    limit: 5,
                    windowSeconds: 60,
                    redis: redis);
            });

            // -------------------------------------------------------
            // AUTH NORMAL — 20 req/min (refresh, logout)
            // Per-user if authenticated, per-IP if not
            // -------------------------------------------------------
            options.AddPolicy(RateLimitPolicy.AuthNormal, ctx =>
            {
                var key = GetUserOrIpKey(ctx, "auth_normal");
                return BuildFixedWindow(
                    partitionKey: key,
                    limit: 20,
                    windowSeconds: 60,
                    redis: redis);
            });

            // -------------------------------------------------------
            // WRITE — POST/PUT/DELETE writes
            // IP: 30/min | Authenticated user: 60/min
            // -------------------------------------------------------
            options.AddPolicy(RateLimitPolicy.Write, ctx =>
            {
                var userId = GetUserId(ctx);
                if (userId is not null)
                {
                    return BuildFixedWindow(
                        partitionKey: $"rl:write:user:{userId}",
                        limit: 60,
                        windowSeconds: 60,
                        redis: redis);
                }

                var ip = GetIp(ctx);
                return BuildFixedWindow(
                    partitionKey: $"rl:write:ip:{ip}",
                    limit: 30,
                    windowSeconds: 60,
                    redis: redis);
            });

            // -------------------------------------------------------
            // READ — GET endpoints
            // IP: 60/min | Authenticated user: 120/min
            // -------------------------------------------------------
            options.AddPolicy(RateLimitPolicy.Read, ctx =>
            {
                var userId = GetUserId(ctx);
                if (userId is not null)
                {
                    return BuildFixedWindow(
                        partitionKey: $"rl:read:user:{userId}",
                        limit: 120,
                        windowSeconds: 60,
                        redis: redis);
                }

                var ip = GetIp(ctx);
                return BuildFixedWindow(
                    partitionKey: $"rl:read:ip:{ip}",
                    limit: 60,
                    windowSeconds: 60,
                    redis: redis);
            });
        });

        return services;
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    /// <summary>
    /// Dùng in-memory FixedWindowRateLimiter.
    /// Khi Redis có sẵn, dùng Redis counter để đồng bộ giữa nhiều instance.
    /// </summary>
    private static RateLimitPartition<string> BuildFixedWindow(
        string partitionKey,
        int limit,
        int windowSeconds,
        IConnectionMultiplexer? redis)
    {
        if (redis is not null)
        {
            return RateLimitPartition.Get(partitionKey, key =>
                new RedisFixedWindowRateLimiter(key, limit, windowSeconds, redis));
        }

        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = limit,
                Window = TimeSpan.FromSeconds(windowSeconds),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    }

    private static string? GetUserId(HttpContext ctx) =>
        ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? ctx.User.FindFirst("sub")?.Value;

    private static string GetIp(HttpContext ctx) =>
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private static string GetUserOrIpKey(HttpContext ctx, string prefix)
    {
        var userId = GetUserId(ctx);
        return userId is not null
            ? $"rl:{prefix}:user:{userId}"
            : $"rl:{prefix}:ip:{GetIp(ctx)}";
    }
}
