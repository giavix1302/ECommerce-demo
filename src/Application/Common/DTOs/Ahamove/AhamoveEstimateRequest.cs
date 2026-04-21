using System.Text.Json.Serialization;

namespace Application.Common.DTOs.Ahamove;

public class AhamoveEstimateRequest
{
    [JsonPropertyName("order_time")]
    public long OrderTime { get; set; } = 0;

    [JsonPropertyName("path")]
    public List<AhamoveOrderPath> Path { get; set; } = [];

    [JsonPropertyName("services")]
    public List<AhamoveEstimateService> Services { get; set; } = [];

    // "CASH" hoặc "BALANCE"
    [JsonPropertyName("payment_method")]
    public string PaymentMethod { get; set; } = "CASH";
}

public class AhamoveEstimateService
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("requests")]
    public List<object> Requests { get; set; } = [];
}
