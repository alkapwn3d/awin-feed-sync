namespace AwinFeedSync.Core.Models;

public class ProductRecord
{
    public int AdvertiserId { get; set; }
    public string? FeedProductId { get; set; }
    public string? Sku { get; set; }
    public string? ProductName { get; set; }
    public string? ProductUrl { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public string? Category { get; set; }
    public string? Subcategory { get; set; }
    public string? CommissionText { get; set; }
    public decimal? CommissionRate { get; set; }
    public Dictionary<string, string> ExtraFields { get; set; } = new();
}
