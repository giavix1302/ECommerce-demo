using Domain.Interfaces;

namespace Domain.Entities;

public class ProductVariant : ISoftDelete
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; } = 0;
    public bool IsDefault { get; set; } = false;

    // Physical dimensions for shipping tier calculation (nullable — standard/small items don't need these)
    public float? WeightKg { get; set; }
    public float? LengthCm { get; set; }
    public float? WidthCm { get; set; }
    public float? HeightCm { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ISoftDelete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;
    public ICollection<VariantAttributeValue> AttributeValues { get; set; } = new List<VariantAttributeValue>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
