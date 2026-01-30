using System.Text.Json;
using AwinFeedSync.Core.Models;
using AwinFeedSync.Core.Services;
using AwinFeedSync.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace AwinFeedSync.Tests;

public class ProductRepositoryTests
{
    [Fact]
    public async Task UpsertProducts_InsertsNewProduct()
    {
        await using var context = CreateInMemoryContext();
        var repo = new ProductRepository(context, NullLogger<ProductRepository>.Instance);
        
        var product = CreateTestProduct(1, "TEST-KEY-1", "Product 1");
        
        await repo.UpsertProductsAsync(new List<Product> { product });
        
        var saved = await context.Products.FirstOrDefaultAsync(p => p.ProductKey == "TEST-KEY-1");
        saved.Should().NotBeNull();
        saved!.ProductName.Should().Be("Product 1");
    }
    
    [Fact]
    public async Task UpsertProducts_UpdatesLastSeenAt_WhenHashUnchanged()
    {
        await using var context = CreateInMemoryContext();
        var repo = new ProductRepository(context, NullLogger<ProductRepository>.Instance);
        
        var product = CreateTestProduct(1, "TEST-KEY-2", "Product 2");
        product.LastSeenAt = DateTime.UtcNow.AddDays(-1);
        product.LastChangedAt = DateTime.UtcNow.AddDays(-1);
        
        context.Products.Add(product);
        await context.SaveChangesAsync();
        
        var originalChangedAt = product.LastChangedAt;
        await Task.Delay(10); // Ensure time difference
        
        var updateProduct = CreateTestProduct(1, "TEST-KEY-2", "Product 2");
        updateProduct.ContentHash = product.ContentHash; // Same hash
        
        await repo.UpsertProductsAsync(new List<Product> { updateProduct });
        
        var updated = await context.Products.FirstOrDefaultAsync(p => p.ProductKey == "TEST-KEY-2");
        updated!.LastSeenAt.Should().BeAfter(originalChangedAt);
        updated.LastChangedAt.Should().Be(originalChangedAt); // Should NOT change
    }
    
    [Fact]
    public async Task UpsertProducts_UpdatesAllFields_WhenHashChanged()
    {
        await using var context = CreateInMemoryContext();
        var repo = new ProductRepository(context, NullLogger<ProductRepository>.Instance);
        
        var product = CreateTestProduct(1, "TEST-KEY-3", "Product 3");
        product.Price = 10.00m;
        product.LastChangedAt = DateTime.UtcNow.AddDays(-1);
        
        context.Products.Add(product);
        await context.SaveChangesAsync();
        
        var originalChangedAt = product.LastChangedAt;
        await Task.Delay(10);
        
        var updateProduct = CreateTestProduct(1, "TEST-KEY-3", "Product 3 Updated");
        updateProduct.Price = 20.00m;
        updateProduct.ContentHash = "DIFFERENT-HASH";
        
        await repo.UpsertProductsAsync(new List<Product> { updateProduct });
        
        var updated = await context.Products.FirstOrDefaultAsync(p => p.ProductKey == "TEST-KEY-3");
        updated!.ProductName.Should().Be("Product 3 Updated");
        updated.Price.Should().Be(20.00m);
        updated.LastChangedAt.Should().BeAfter(originalChangedAt);
        updated.ContentHash.Should().Be("DIFFERENT-HASH");
    }
    
    [Fact]
    public async Task MarkMissingProductsInactive_MarksOldProducts()
    {
        await using var context = CreateInMemoryContext();
        var repo = new ProductRepository(context, NullLogger<ProductRepository>.Instance);
        
        var oldProduct = CreateTestProduct(1, "OLD-PRODUCT", "Old");
        oldProduct.LastSeenAt = DateTime.UtcNow.AddDays(-2);
        
        var recentProduct = CreateTestProduct(1, "RECENT-PRODUCT", "Recent");
        recentProduct.LastSeenAt = DateTime.UtcNow;
        
        context.Products.AddRange(oldProduct, recentProduct);
        await context.SaveChangesAsync();
        
        var runTime = DateTime.UtcNow.AddMinutes(-1);
        await repo.MarkMissingProductsInactiveAsync(1, runTime);
        
        var oldUpdated = await context.Products.FirstOrDefaultAsync(p => p.ProductKey == "OLD-PRODUCT");
        var recentUpdated = await context.Products.FirstOrDefaultAsync(p => p.ProductKey == "RECENT-PRODUCT");
        
        oldUpdated!.InactiveAt.Should().NotBeNull();
        recentUpdated!.InactiveAt.Should().BeNull();
    }
    
    [Fact]
    public async Task UpsertAdvertiser_InsertsNewAdvertiser()
    {
        await using var context = CreateInMemoryContext();
        var repo = new ProductRepository(context, NullLogger<ProductRepository>.Instance);
        
        var advertiser = new Advertiser
        {
            AdvertiserId = 100,
            Name = "Test Advertiser",
            Status = "active",
            UpdatedAt = DateTime.UtcNow
        };
        
        await repo.UpsertAdvertiserAsync(advertiser);
        
        var saved = await context.Advertisers.FindAsync(100);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Test Advertiser");
    }
    
    [Fact]
    public async Task UpsertAdvertiser_UpdatesExistingAdvertiser()
    {
        await using var context = CreateInMemoryContext();
        var repo = new ProductRepository(context, NullLogger<ProductRepository>.Instance);
        
        var advertiser = new Advertiser
        {
            AdvertiserId = 200,
            Name = "Original Name",
            Status = "active",
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        
        context.Advertisers.Add(advertiser);
        await context.SaveChangesAsync();
        
        var update = new Advertiser
        {
            AdvertiserId = 200,
            Name = "Updated Name",
            Status = "paused",
            UpdatedAt = DateTime.UtcNow
        };
        
        await repo.UpsertAdvertiserAsync(update);
        
        var updated = await context.Advertisers.FindAsync(200);
        updated!.Name.Should().Be("Updated Name");
        updated.Status.Should().Be("paused");
    }
    
    [Fact]
    public async Task CreateSyncRun_CreatesNewRun()
    {
        await using var context = CreateInMemoryContext();
        var repo = new ProductRepository(context, NullLogger<ProductRepository>.Instance);
        
        var run = await repo.CreateSyncRunAsync();
        
        run.RunId.Should().BeGreaterThan(0);
        run.Status.Should().Be("running");
        run.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
    
    private AwinDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AwinDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new AwinDbContext(options);
    }
    
    private Product CreateTestProduct(int advertiserId, string productKey, string name)
    {
        var record = new ProductRecord
        {
            AdvertiserId = advertiserId,
            ProductName = name,
            Price = 10.00m,
            Currency = "USD"
        };
        
        return new Product
        {
            AdvertiserId = advertiserId,
            ProductKey = productKey,
            ProductName = name,
            Price = 10.00m,
            Currency = "USD",
            ContentHash = ContentHasher.ComputeHash(record),
            LastSeenAt = DateTime.UtcNow,
            LastChangedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };
    }
}
