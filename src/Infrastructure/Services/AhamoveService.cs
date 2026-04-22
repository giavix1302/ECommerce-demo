using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Application.Common.DTOs.Ahamove;
using Application.Common.Interfaces;
using Infrastructure.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class AhamoveService : IAhamoveService
{
    private readonly HttpClient _http;
    private readonly AhamoveSettings _settings;
    private readonly IMemoryCache _cache;
    private const string TokenCacheKey = "ahamove_token";

    public AhamoveService(HttpClient http, IOptions<AhamoveSettings> options, IMemoryCache cache)
    {
        _http = http;
        _settings = options.Value;
        _cache = cache;
    }

    // ─────────────────────────────────────────────
    // Private: lấy token, cache 23h
    // ─────────────────────────────────────────────
    private async Task<string> GetTokenAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(TokenCacheKey, out string? cached) && cached is not null)
            return cached;

        var response = await _http.PostAsJsonAsync("/v3/accounts/token", new
        {
            api_key = _settings.ApiKey,
            mobile = _settings.Phone
        }, ct);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AhamoveTokenResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Ahamove: failed to retrieve token");

        _cache.Set(TokenCacheKey, result.Token, TimeSpan.FromHours(23));
        return result.Token;
    }

    // ─────────────────────────────────────────────
    // Estimate shipping fee
    // POST /v3/orders/estimates
    // ─────────────────────────────────────────────
    public async Task<IEnumerable<AhamoveServiceEstimate>> EstimateShippingFeeAsync(
        AhamoveEstimateRequest request,
        CancellationToken ct = default)
    {
        var token = await GetTokenAsync(ct);

        using var req = new HttpRequestMessage(HttpMethod.Post, "/v3/orders/estimates");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(req, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Ahamove estimate failed ({(int)response.StatusCode}): {error}",
                null,
                response.StatusCode);
        }

        var raw = await response.Content.ReadAsStringAsync(ct);
        Console.WriteLine($"[Ahamove] estimate raw: {raw}");
        return System.Text.Json.JsonSerializer.Deserialize<IEnumerable<AhamoveServiceEstimate>>(raw) ?? [];
    }

    // ─────────────────────────────────────────────
    // Tạo đơn vận chuyển
    // POST /v3/orders
    // ─────────────────────────────────────────────
    public async Task<AhamoveOrderResponse> CreateOrderAsync(
        AhamoveCreateOrderRequest request,
        CancellationToken ct = default)
    {
        var token = await GetTokenAsync(ct);

        using var req = new HttpRequestMessage(HttpMethod.Post, "/v3/orders");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        req.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(req, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AhamoveOrderResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Ahamove: failed to create order");
    }
}

// Internal — chỉ dùng để deserialize response từ /v3/auth/token
file class AhamoveTokenResponse
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
}
