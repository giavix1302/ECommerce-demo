namespace API.RateLimit;

public static class RateLimitPolicy
{
    /// <summary>
    /// POST /auth/login, POST /auth/register
    /// IP only — 5 req/min
    /// </summary>
    public const string AuthStrict = "auth_strict";

    /// <summary>
    /// POST /auth/refresh, POST /auth/logout
    /// IP + User — 20 req/min
    /// </summary>
    public const string AuthNormal = "auth_normal";

    /// <summary>
    /// POST/PUT/DELETE — cart, order, review, admin writes
    /// IP: 30 req/min | User: 60 req/min
    /// </summary>
    public const string Write = "write";

    /// <summary>
    /// GET — products, orders, cart, shipping fee, reviews
    /// IP: 60 req/min | User: 120 req/min
    /// </summary>
    public const string Read = "read";
}
