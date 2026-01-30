using System.Text.Json;
using AwinFeedSync.Core.Interfaces;
using AwinFeedSync.Core.Models;
using AwinFeedSync.Core.Services;
using AwinFeedSync.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AwinFeedSync.Infrastructure.Services;

public class FeedSyncService
{
    private readonly IAdvertiserSource _advertiserSource;
    private readonly IProductFeedSource _feedSource;
    private readonly ILinkBuilder _linkBuilder;
    private readonly IProductRepository _repository;
    private readonly FeedsConfig _config;
    private readonly ILogger<FeedSyncService> _logger;
    
    public FeedSyncService(
        IAdvertiserSource advertiserSource,
        IProductFeedSource feedSource,
        ILinkBuilder linkBuilder,
        IProductRepository repository,
        IOptions<FeedsConfig> config,
        ILogger<FeedSyncService> logger)
    {
        _advertiserSource = advertiserSource;
        _feedSource = feedSource;
        _linkBuilder = linkBuilder;
        _repository = repository;
        _config = config.Value;
        _logger = logger;
    }
    
    public async Task<SyncRun> RunSyncAsync(int? specificAdvertiserId = null, int? maxAdvertisers = null, bool dryRun = false, CancellationToken ct = default)
    {
        var run = await _repository.CreateSyncRunAsync(ct);
        var runTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogInformation("Starting sync run {RunId}", run.RunId);
            
            var advertisers = await _advertiserSource.GetApprovedAdvertisersAsync(ct);
            
            if (specificAdvertiserId.HasValue)
            {
                advertisers = advertisers.Where(a => a.AdvertiserId == specificAdvertiserId.Value).ToList();
            }
            
            if (maxAdvertisers.HasValue)
            {
                advertisers = advertisers.Take(maxAdvertisers.Value).ToList();
            }
            
            _logger.LogInformation("Found {Count} advertisers to process", advertisers.Count);
            
            foreach (var advertiser in advertisers)
            {
                if (ct.IsCancellationRequested) break;
                
                try
                {
                    await ProcessAdvertiserAsync(advertiser, run, runTime, dryRun, ct);
                    run.AdvertisersProcessed++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process advertiser {AdvertiserId}", advertiser.AdvertiserId);
                }
            }
            
            run.Status = "completed";
            run.FinishedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sync run {RunId} failed", run.RunId);
            run.Status = "failed";
            run.ErrorText = ex.Message;
            run.FinishedAt = DateTime.UtcNow;
        }
        
        if (!dryRun)
        {
            await _repository.UpdateSyncRunAsync(run, ct);
        }
        
        _logger.LogInformation("Sync run {RunId} completed: {Status}, {Advertisers} advertisers, {Products} products seen, {Changed} changed",
            run.RunId, run.Status, run.AdvertisersProcessed, run.ProductsSeen, run.ProductsChanged);
        
        return run;
    }
    
    private async Task ProcessAdvertiserAsync(Advertiser advertiser, SyncRun run, DateTime runTime, bool dryRun, CancellationToken ct)
    {
        _logger.LogInformation("Processing advertiser {AdvertiserId}: {Name}", advertiser.AdvertiserId, advertiser.Name);
        
        if (!dryRun)
        {
            await _repository.UpsertAdvertiserAsync(advertiser, ct);
        }
        
        var batch = new List<Product>();
        var seenCount = 0;
        var changedCount = 0;
        
        await foreach (var record in _feedSource.GetProductsAsync(advertiser.AdvertiserId, ct))
        {
            var productKey = ContentHasher.GenerateProductKey(advertiser.AdvertiserId, record);
            var contentHash = ContentHasher.ComputeHash(record);
            
            var trackingUrl = await _linkBuilder.BuildTrackingLinkAsync(
                advertiser.AdvertiserId, 
                record.ProductUrl ?? string.Empty, 
                null, 
                ct);
            
            var product = new Product
            {
                AdvertiserId = advertiser.AdvertiserId,
                ProductKey = productKey,
                FeedProductId = record.FeedProductId,
                Sku = record.Sku,
                ProductName = record.ProductName,
                ProductUrl = record.ProductUrl,
                ImageUrl = record.ImageUrl,
                Price = record.Price,
                Currency = record.Currency,
                Category = record.Category,
                Subcategory = record.Subcategory,
                CommissionText = record.CommissionText,
                CommissionRate = record.CommissionRate,
                TrackingUrl = trackingUrl,
                TrackingUrlSource = _linkBuilder.GetSource(),
                Extra = record.ExtraFields.Count > 0 ? JsonSerializer.Serialize(record.ExtraFields) : null,
                ContentHash = contentHash,
                LastSeenAt = runTime,
                LastChangedAt = runTime,
                LastUpdatedAt = runTime
            };
            
            batch.Add(product);
            seenCount++;
            
            if (batch.Count >= _config.BatchSize)
            {
                if (!dryRun)
                {
                    await _repository.UpsertProductsAsync(batch, ct);
                }
                
                changedCount += batch.Count;
                batch.Clear();
            }
        }
        
        if (batch.Count > 0 && !dryRun)
        {
            await _repository.UpsertProductsAsync(batch, ct);
            changedCount += batch.Count;
        }
        
        if (_config.TreatMissingProductsAsInactive && !dryRun)
        {
            await _repository.MarkMissingProductsInactiveAsync(advertiser.AdvertiserId, runTime, ct);
        }
        
        run.ProductsSeen += seenCount;
        run.ProductsChanged += changedCount;
        
        _logger.LogInformation("Advertiser {AdvertiserId} complete: {Seen} products seen", 
            advertiser.AdvertiserId, seenCount);
    }
}
