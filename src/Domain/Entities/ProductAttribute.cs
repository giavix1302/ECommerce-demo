namespace Domain.Entities;

public class ProductAttribute
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<VariantAttributeValue> VariantAttributeValues { get; set; } = new List<VariantAttributeValue>();
}
