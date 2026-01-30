using AwinFeedSync.Core.Models;

namespace AwinFeedSync.Core.Interfaces;

public interface IAdvertiserSource
{
    Task<List<Advertiser>> GetApprovedAdvertisersAsync(CancellationToken ct = default);
}
