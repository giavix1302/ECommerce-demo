using System.Text.Json.Serialization;

namespace Application.Common.DTOs.Ahamove;

public class AhamoveOrderResponse
{
    [JsonPropertyName("order_id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("shared_link")]
    public string? SharedLink { get; set; }

    // Nested "order" object contains the detailed fields
    [JsonPropertyName("order")]
    public AhamoveOrderDetail? Order { get; set; }

    [JsonIgnore]
    public long TotalPay => Order?.TotalPay ?? 0;

    [JsonIgnore]
    public string? ServiceId => Order?.ServiceId;

    [JsonIgnore]
    public double OrderTime => Order?.OrderTime ?? 0;

    [JsonIgnore]
    public string? SupplierId => Order?.SupplierId;
}

public class AhamoveOrderDetail
{
    [JsonPropertyName("total_pay")]
    public long TotalPay { get; set; }

    [JsonPropertyName("service_id")]
    public string? ServiceId { get; set; }

    [JsonPropertyName("order_time")]
    public double OrderTime { get; set; }

    [JsonPropertyName("supplier_id")]
    public string? SupplierId { get; set; }
}
