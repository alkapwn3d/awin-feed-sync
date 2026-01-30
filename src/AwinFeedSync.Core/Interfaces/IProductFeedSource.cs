using AwinFeedSync.Core.Models;

namespace AwinFeedSync.Core.Interfaces;

public interface IProductFeedSource
{
    IAsyncEnumerable<ProductRecord> GetProductsAsync(int advertiserId, CancellationToken ct = default);
}
