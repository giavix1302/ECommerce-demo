using System.Text.Json.Serialization;

namespace Application.Common.DTOs.Ahamove;

public class AhamoveWebhookPayload
{
    [JsonPropertyName("_id")]
    public string OrderId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("service_id")]
    public string? ServiceId { get; set; }

    [JsonPropertyName("supplier_id")]
    public string? SupplierId { get; set; }

    [JsonPropertyName("total_pay")]
    public long TotalPay { get; set; }

    [JsonPropertyName("order_time")]
    public double OrderTime { get; set; }

    [JsonPropertyName("complete_time")]
    public double CompleteTime { get; set; }

    [JsonPropertyName("cancel_time")]
    public double CancelTime { get; set; }

    [JsonPropertyName("cancel_comment")]
    public string? CancelComment { get; set; }

    [JsonPropertyName("path")]
    public List<AhamoveOrderPath>? Path { get; set; }
}
