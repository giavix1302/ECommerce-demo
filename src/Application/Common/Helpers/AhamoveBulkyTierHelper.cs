namespace Application.Common.Helpers;

public record BulkyTierResult(string? Tier, bool ExceedsLimit);

public static class AhamoveBulkyTierHelper
{
    // Returns "TIER_2" / "TIER_3" / "TIER_4" / null (standard — no bulky request needed)
    // null also means exceeds TIER_4 limits (motorbike not supported) — caller should handle
    public static BulkyTierResult GetTier(float weightKg, float lengthCm, float widthCm, float heightCm)
    {
        var dims = new[] { lengthCm, widthCm, heightCm };
        Array.Sort(dims, (a, b) => b.CompareTo(a)); // sort descending

        var d0 = dims[0];
        var d1 = dims[1];
        var d2 = dims[2];

        // Standard — no bulky surcharge
        if (weightKg <= 30 && d0 <= 50 && d1 <= 40 && d2 <= 50)
            return new(null, false);

        if (weightKg <= 40 && d0 <= 60 && d1 <= 50 && d2 <= 60)
            return new("TIER_2", false);

        if (weightKg <= 60 && d0 <= 70 && d1 <= 60 && d2 <= 70)
            return new("TIER_3", false);

        if (weightKg <= 80 && d0 <= 90 && d1 <= 70 && d2 <= 90)
            return new("TIER_4", false);

        // Exceeds TIER_4 — motorbike not supported
        return new(null, true);
    }

    // Aggregate cart items → single tier
    // weight: sum of all items; dimensions: max across all items
    public static BulkyTierResult GetTierFromItems(IEnumerable<(float? WeightKg, float? LengthCm, float? WidthCm, float? HeightCm, int Quantity)> items)
    {
        float totalWeight = 0;
        float maxLength = 0;
        float maxWidth = 0;
        float maxHeight = 0;

        foreach (var item in items)
        {
            totalWeight += (item.WeightKg ?? 0) * item.Quantity;
            if (item.LengthCm > maxLength) maxLength = item.LengthCm ?? 0;
            if (item.WidthCm > maxWidth) maxWidth = item.WidthCm ?? 0;
            if (item.HeightCm > maxHeight) maxHeight = item.HeightCm ?? 0;
        }

        // All items have no dimensions → standard
        if (totalWeight == 0 && maxLength == 0 && maxWidth == 0 && maxHeight == 0)
            return new(null, false);

        var result = GetTier(totalWeight, maxLength, maxWidth, maxHeight);
        Console.WriteLine($"[BulkyTier] weight={totalWeight}kg  {maxLength}x{maxWidth}x{maxHeight}cm  →  tier={result.Tier ?? (result.ExceedsLimit ? "EXCEEDS_LIMIT" : "standard")}");
        return result;
    }
}
