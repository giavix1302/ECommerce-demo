namespace Domain.Entities;

public class VariantAttributeValue
{
    public long Id { get; set; }
    public long VariantId { get; set; }
    public long AttributeId { get; set; }
    public string Value { get; set; } = string.Empty;

    // Navigation properties
    public ProductVariant Variant { get; set; } = null!;
    public ProductAttribute Attribute { get; set; } = null!;
}
