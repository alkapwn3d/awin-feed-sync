using AwinFeedSync.Core.Models;

namespace AwinFeedSync.Core.Interfaces;

public interface IProductRepository
{
    Task<SyncRun> CreateSyncRunAsync(CancellationToken ct = default);
    Task UpdateSyncRunAsync(SyncRun run, CancellationToken ct = default);
    Task UpsertProductsAsync(List<Product> products, CancellationToken ct = default);
    Task MarkMissingProductsInactiveAsync(int advertiserId, DateTime runTime, CancellationToken ct = default);
    Task<Advertiser?> GetAdvertiserAsync(int advertiserId, CancellationToken ct = default);
    Task UpsertAdvertiserAsync(Advertiser advertiser, CancellationToken ct = default);
}
