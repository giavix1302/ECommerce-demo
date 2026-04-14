namespace Domain.Constants;

public static class MembershipPolicy
{
    // Rank thresholds (loyalty points)
    public const int GoldThreshold = 100;
    public const int DiamondThreshold = 500;

    // Rank discounts
    public const decimal SilverDiscount = 0m;
    public const decimal GoldDiscount = 0.05m;
    public const decimal DiamondDiscount = 0.10m;

    // Points earning rate
    public const decimal PointsPerAmount = 100_000m; // 1 điểm / 100.000đ
}
