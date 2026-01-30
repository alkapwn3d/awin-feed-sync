using AwinFeedSync.Core.Models;
using AwinFeedSync.Core.Services;
using FluentAssertions;

namespace AwinFeedSync.Tests;

public class ContentHasherTests
{
    [Fact]
    public void ComputeHash_SameData_ProducesSameHash()
    {
        var record1 = new ProductRecord
        {
            ProductName = "Test Product",
            ProductUrl = "https://example.com/product",
            Price = 99.99m,
            Currency = "USD"
        };
        
        var record2 = new ProductRecord
        {
            ProductName = "Test Product",
            ProductUrl = "https://example.com/product",
            Price = 99.99m,
            Currency = "USD"
        };
        
        var hash1 = ContentHasher.ComputeHash(record1);
        var hash2 = ContentHasher.ComputeHash(record2);
        
        hash1.Should().Be(hash2);
    }
    
    [Fact]
    public void ComputeHash_DifferentData_ProducesDifferentHash()
    {
        var record1 = new ProductRecord { ProductName = "Product A", Price = 10m };
        var record2 = new ProductRecord { ProductName = "Product B", Price = 20m };
        
        var hash1 = ContentHasher.ComputeHash(record1);
        var hash2 = ContentHasher.ComputeHash(record2);
        
        hash1.Should().NotBe(hash2);
    }
    
    [Fact]
    public void GenerateProductKey_WithSku_UsesSku()
    {
        var record = new ProductRecord
        {
            Sku = "SKU123",
            FeedProductId = "FEED456",
            ProductUrl = "https://example.com/product"
        };
        
        var key = ContentHasher.GenerateProductKey(100, record);
        
        key.Should().Be("100:SKU123");
    }
    
    [Fact]
    public void GenerateProductKey_WithoutSku_UsesFeedProductId()
    {
        var record = new ProductRecord
        {
            FeedProductId = "FEED456",
            ProductUrl = "https://example.com/product"
        };
        
        var key = ContentHasher.GenerateProductKey(100, record);
        
        key.Should().Be("100:FEED456");
    }
    
    [Fact]
    public void GenerateProductKey_WithoutSkuOrFeedId_UsesNormalizedUrl()
    {
        var record = new ProductRecord
        {
            ProductUrl = "https://example.com/Product/123?ref=abc"
        };
        
        var key = ContentHasher.GenerateProductKey(100, record);
        
        key.Should().Contain("100:");
        key.Should().Contain("example.com");
    }
}
