using System.Globalization;
using System.Text;
using AwinFeedSync.Core.Models;
using AwinFeedSync.Infrastructure.Configuration;
using AwinFeedSync.Infrastructure.Feeds;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AwinFeedSync.Tests;

public class FeedParsingTests
{
    [Fact]
    public async Task ParseFeed_HandlesStandardCsvFormat()
    {
        var csvContent = @"product_id,product_name,aw_deep_link,search_price,currency,sku
123,Test Product,https://example.com/product,99.99,USD,SKU123
456,Another Product,https://example.com/product2,49.99,GBP,SKU456";
        
        var feedPath = await CreateTempFeedFile(csvContent, ".csv");
        
        try
        {
            var products = await ParseTestFeed(feedPath, 100);
            
            products.Should().HaveCount(2);
            products[0].FeedProductId.Should().Be("123");
            products[0].ProductName.Should().Be("Test Product");
            products[0].Price.Should().Be(99.99m);
            products[0].Currency.Should().Be("USD");
            products[0].Sku.Should().Be("SKU123");
        }
        finally
        {
            File.Delete(feedPath);
        }
    }
    
    [Fact]
    public async Task ParseFeed_HandlesCaseInsensitiveColumns()
    {
        var csvContent = @"PRODUCT_ID,Product_Name,AW_DEEP_LINK,SEARCH_PRICE
789,Case Test,https://example.com/test,29.99";
        
        var feedPath = await CreateTempFeedFile(csvContent, ".csv");
        
        try
        {
            var products = await ParseTestFeed(feedPath, 100);
            
            products.Should().HaveCount(1);
            products[0].FeedProductId.Should().Be("789");
            products[0].ProductName.Should().Be("Case Test");
        }
        finally
        {
            File.Delete(feedPath);
        }
    }
    
    [Fact]
    public async Task ParseFeed_StoresUnmappedColumnsInExtra()
    {
        var csvContent = @"product_id,product_name,custom_field1,custom_field2
111,Test,CustomValue1,CustomValue2";
        
        var feedPath = await CreateTempFeedFile(csvContent, ".csv");
        
        try
        {
            var products = await ParseTestFeed(feedPath, 100);
            
            products.Should().HaveCount(1);
            products[0].ExtraFields.Should().ContainKey("custom_field1");
            products[0].ExtraFields.Should().ContainKey("custom_field2");
            products[0].ExtraFields["custom_field1"].Should().Be("CustomValue1");
        }
        finally
        {
            File.Delete(feedPath);
        }
    }
    
    [Fact]
    public async Task ParseFeed_HandlesTsvFormat()
    {
        var tsvContent = "product_id\tproduct_name\tprice\n222\tTSV Product\t19.99";
        
        var feedPath = await CreateTempFeedFile(tsvContent, ".tsv");
        
        try
        {
            var products = await ParseTestFeed(feedPath, 100);
            
            products.Should().HaveCount(1);
            products[0].FeedProductId.Should().Be("222");
            products[0].ProductName.Should().Be("TSV Product");
        }
        finally
        {
            File.Delete(feedPath);
        }
    }
    
    [Fact]
    public async Task ParseFeed_HandlesEmptyFields()
    {
        var csvContent = @"product_id,product_name,sku,price
333,Product Without SKU,,";
        
        var feedPath = await CreateTempFeedFile(csvContent, ".csv");
        
        try
        {
            var products = await ParseTestFeed(feedPath, 100);
            
            products.Should().HaveCount(1);
            products[0].FeedProductId.Should().Be("333");
            products[0].Sku.Should().BeNullOrEmpty();
            products[0].Price.Should().BeNull();
        }
        finally
        {
            File.Delete(feedPath);
        }
    }
    
    private async Task<string> CreateTempFeedFile(string content, string extension)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_feed_{Guid.NewGuid()}{extension}");
        await File.WriteAllTextAsync(tempPath, content);
        return tempPath;
    }
    
    private async Task<List<ProductRecord>> ParseTestFeed(string feedPath, int advertiserId)
    {
        var config = Options.Create(new FeedsConfig
        {
            WorkingDirectory = Path.GetTempPath()
        });
        
        var source = new TestProductFeedSource(config, feedPath);
        var products = new List<ProductRecord>();
        
        await foreach (var product in source.GetProductsAsync(advertiserId))
        {
            products.Add(product);
        }
        
        return products;
    }
    
    private class TestProductFeedSource : AwinProductFeedSource
    {
        private readonly string _testFeedPath;
        
        public TestProductFeedSource(IOptions<FeedsConfig> config, string testFeedPath) 
            : base(new HttpClient(), config, NullLogger<AwinProductFeedSource>.Instance)
        {
            _testFeedPath = testFeedPath;
        }
        
        public new async IAsyncEnumerable<ProductRecord> GetProductsAsync(int advertiserId, CancellationToken ct = default)
        {
            await foreach (var product in ParseFeedAsync(advertiserId, _testFeedPath, ct))
            {
                yield return product;
            }
        }
        
        private async IAsyncEnumerable<ProductRecord> ParseFeedAsync(int advertiserId, string feedPath, CancellationToken ct)
        {
            var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null,
                BadDataFound = null,
                Delimiter = feedPath.EndsWith(".tsv") ? "\t" : ","
            };
            
            using var reader = new StreamReader(feedPath);
            using var csv = new CsvHelper.CsvReader(reader, config);
            
            await csv.ReadAsync();
            csv.ReadHeader();
            
            var headers = csv.HeaderRecord ?? Array.Empty<string>();
            var columnMap = BuildColumnMap(headers);
            
            while (await csv.ReadAsync())
            {
                if (ct.IsCancellationRequested) yield break;
                
                var record = new ProductRecord { AdvertiserId = advertiserId };
                
                record.FeedProductId = GetField(csv, columnMap, "product_id", "id", "aw_product_id");
                record.Sku = GetField(csv, columnMap, "sku", "merchant_product_id");
                record.ProductName = GetField(csv, columnMap, "product_name", "name", "title");
                record.ProductUrl = GetField(csv, columnMap, "aw_deep_link", "product_url", "link", "url");
                record.ImageUrl = GetField(csv, columnMap, "aw_image_url", "image_url", "image");
                
                var priceStr = GetField(csv, columnMap, "search_price", "price", "rrp_price");
                if (decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                {
                    record.Price = price;
                }
                
                record.Currency = GetField(csv, columnMap, "currency");
                record.Category = GetField(csv, columnMap, "merchant_category", "category");
                record.Subcategory = GetField(csv, columnMap, "category_name", "subcategory");
                record.CommissionText = GetField(csv, columnMap, "commission_group", "commission");
                
                var commRateStr = GetField(csv, columnMap, "commission_rate", "commission_amount");
                if (decimal.TryParse(commRateStr?.TrimEnd('%'), NumberStyles.Any, CultureInfo.InvariantCulture, out var commRate))
                {
                    record.CommissionRate = commRate;
                }
                
                var knownColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "product_id", "id", "aw_product_id",
                    "sku", "merchant_product_id",
                    "product_name", "name", "title",
                    "aw_deep_link", "product_url", "link", "url",
                    "aw_image_url", "image_url", "image",
                    "search_price", "price", "rrp_price",
                    "currency",
                    "merchant_category", "category",
                    "category_name", "subcategory",
                    "commission_group", "commission",
                    "commission_rate", "commission_amount"
                };
                
                foreach (var header in headers)
                {
                    if (!knownColumns.Contains(header))
                    {
                        record.ExtraFields[header] = csv.GetField(header) ?? string.Empty;
                    }
                }
                
                yield return record;
            }
        }
        
        private Dictionary<string, string> BuildColumnMap(string[] headers)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in headers)
            {
                map[header] = header;
            }
            return map;
        }
        
        private string? GetField(CsvHelper.CsvReader csv, Dictionary<string, string> columnMap, params string[] candidates)
        {
            foreach (var candidate in candidates)
            {
                if (columnMap.TryGetValue(candidate, out var actualColumn))
                {
                    return csv.GetField(actualColumn);
                }
            }
            return null;
        }
    }
}
