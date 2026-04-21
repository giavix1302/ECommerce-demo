using System.Text.Json.Serialization;

namespace Application.Common.DTOs.Ahamove;

public class AhamoveOrderPath
{
    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lng")]
    public double Lng { get; set; }

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("mobile")]
    public string Mobile { get; set; } = string.Empty;

    [JsonPropertyName("remarks")]
    public string? Remarks { get; set; }

    // Delivery point only
    [JsonPropertyName("cod")]
    public long? Cod { get; set; }

    [JsonPropertyName("item_value")]
    public long? ItemValue { get; set; }

    [JsonPropertyName("tracking_number")]
    public string? TrackingNumber { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}
