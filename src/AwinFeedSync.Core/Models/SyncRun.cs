namespace AwinFeedSync.Core.Models;

public class SyncRun
{
    public long RunId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorText { get; set; }
    public int AdvertisersProcessed { get; set; }
    public int ProductsSeen { get; set; }
    public int ProductsChanged { get; set; }
}
