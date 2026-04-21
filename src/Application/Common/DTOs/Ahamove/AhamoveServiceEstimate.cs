using System.Text.Json.Serialization;

namespace Application.Common.DTOs.Ahamove;

public class AhamoveServiceEstimate
{
    [JsonPropertyName("service_id")]
    public string ServiceId { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public AhamoveEstimateData? Data { get; set; }

    [JsonIgnore]
    public long TotalPrice => Data?.TotalPrice ?? 0;

    [JsonIgnore]
    public double Distance => Data?.Distance ?? 0;

    [JsonIgnore]
    public int Duration => Data?.Duration ?? 0;
}

public class AhamoveEstimateData
{
    [JsonPropertyName("total_price")]
    public long TotalPrice { get; set; }

    [JsonPropertyName("distance")]
    public double Distance { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }
}
