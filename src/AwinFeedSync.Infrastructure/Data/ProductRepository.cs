using System.Text.Json;
using AwinFeedSync.Core.Interfaces;
using AwinFeedSync.Core.Models;
using AwinFeedSync.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AwinFeedSync.Infrastructure.Data;

public class ProductRepository : IProductRepository
{
    private readonly AwinDbContext _context;
    private readonly ILogger<ProductRepository> _logger;
    
    public ProductRepository(AwinDbContext context, ILogger<ProductRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<SyncRun> CreateSyncRunAsync(CancellationToken ct = default)
    {
        var run = new SyncRun
        {
            StartedAt = DateTime.UtcNow,
            Status = "running"
        };
        
        _context.SyncRuns.Add(run);
        await _context.SaveChangesAsync(ct);
        return run;
    }
    
    public async Task UpdateSyncRunAsync(SyncRun run, CancellationToken ct = default)
    {
        _context.SyncRuns.Update(run);
        await _context.SaveChangesAsync(ct);
    }
    
    public async Task UpsertProductsAsync(List<Product> products, CancellationToken ct = default)
    {
        foreach (var product in products)
        {
            var existing = await _context.Products
                .FirstOrDefaultAsync(p => p.AdvertiserId == product.AdvertiserId && p.ProductKey == product.ProductKey, ct);
            
            if (existing == null)
            {
                _context.Products.Add(product);
            }
            else if (existing.ContentHash != product.ContentHash)
            {
                existing.FeedProductId = product.FeedProductId;
                existing.Sku = product.Sku;
                existing.ProductName = product.ProductName;
                existing.ProductUrl = product.ProductUrl;
                existing.ImageUrl = product.ImageUrl;
                existing.Price = product.Price;
                existing.Currency = product.Currency;
                existing.Category = product.Category;
                existing.Subcategory = product.Subcategory;
                existing.CommissionText = product.CommissionText;
                existing.CommissionRate = product.CommissionRate;
                existing.TrackingUrl = product.TrackingUrl;
                existing.TrackingUrlSource = product.TrackingUrlSource;
                existing.Extra = product.Extra;
                existing.ContentHash = product.ContentHash;
                existing.LastChangedAt = DateTime.UtcNow;
                existing.LastUpdatedAt = DateTime.UtcNow;
                existing.LastSeenAt = DateTime.UtcNow;
                existing.InactiveAt = null;
            }
            else
            {
                existing.LastSeenAt = DateTime.UtcNow;
            }
        }
        
        await _context.SaveChangesAsync(ct);
    }
    
    public async Task MarkMissingProductsInactiveAsync(int advertiserId, DateTime runTime, CancellationToken ct = default)
    {
        await _context.Products
            .Where(p => p.AdvertiserId == advertiserId && p.LastSeenAt < runTime && p.InactiveAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.InactiveAt, runTime), ct);
    }
    
    public async Task<Advertiser?> GetAdvertiserAsync(int advertiserId, CancellationToken ct = default)
    {
        return await _context.Advertisers.FindAsync(new object[] { advertiserId }, ct);
    }
    
    public async Task UpsertAdvertiserAsync(Advertiser advertiser, CancellationToken ct = default)
    {
        var existing = await _context.Advertisers.FindAsync(new object[] { advertiser.AdvertiserId }, ct);
        
        if (existing == null)
        {
            _context.Advertisers.Add(advertiser);
        }
        else
        {
            existing.Name = advertiser.Name;
            existing.Status = advertiser.Status;
            existing.DefaultCommissionText = advertiser.DefaultCommissionText;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync(ct);
    }
}
