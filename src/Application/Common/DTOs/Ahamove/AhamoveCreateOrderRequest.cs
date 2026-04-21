using System.Text.Json.Serialization;

namespace Application.Common.DTOs.Ahamove;

public class AhamoveCreateOrderRequest
{
    [JsonPropertyName("order_time")]
    public long OrderTime { get; set; } = 0;

    [JsonPropertyName("path")]
    public List<AhamoveOrderPath> Path { get; set; } = [];

    [JsonPropertyName("service_id")]
    public string ServiceId { get; set; } = string.Empty;

    // "CASH" hoặc "BALANCE"
    [JsonPropertyName("payment_method")]
    public string PaymentMethod { get; set; } = "CASH";

    [JsonPropertyName("remarks")]
    public string? Remarks { get; set; }
}
