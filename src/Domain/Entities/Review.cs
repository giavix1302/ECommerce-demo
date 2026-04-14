namespace Domain.Entities;

public class Review
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long ProductId { get; set; }
    public long OrderId { get; set; }
    public byte Rating { get; set; } // 1-5
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Order Order { get; set; } = null!;
}
