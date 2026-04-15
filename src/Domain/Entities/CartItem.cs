namespace Domain.Entities;

public class CartItem
{
    public long Id { get; set; }
    public long CartId { get; set; }
    public long VariantId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Cart Cart { get; set; } = null!;
    public ProductVariant Variant { get; set; } = null!;
}
