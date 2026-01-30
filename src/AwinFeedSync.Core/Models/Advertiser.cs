namespace AwinFeedSync.Core.Models;

public class Advertiser
{
    public int AdvertiserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? DefaultCommissionText { get; set; }
    public DateTime UpdatedAt { get; set; }
}
