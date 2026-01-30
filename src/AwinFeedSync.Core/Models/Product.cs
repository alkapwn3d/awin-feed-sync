namespace AwinFeedSync.Core.Models;

public class Product
{
    public long Id { get; set; }
    public int AdvertiserId { get; set; }
    public string ProductKey { get; set; } = string.Empty;
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
    public string? TrackingUrl { get; set; }
    public string? TrackingUrlSource { get; set; }
    public string? Extra { get; set; }
    public string ContentHash { get; set; } = string.Empty;
    public DateTime LastSeenAt { get; set; }
    public DateTime LastChangedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public DateTime? InactiveAt { get; set; }
    public string? AiSummary { get; set; }
    public string? AiSummaryStatus { get; set; }
    public DateTime? AiSummaryUpdatedAt { get; set; }
    
    public Advertiser? Advertiser { get; set; }
}
