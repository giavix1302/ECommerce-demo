namespace Infrastructure.Settings;

public class AhamoveSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    // Pickup location (warehouse)
    public string PickupAddress { get; set; } = string.Empty;
    public string PickupName { get; set; } = string.Empty;
    public string PickupMobile { get; set; } = string.Empty;
    public double PickupLat { get; set; }
    public double PickupLng { get; set; }
}
