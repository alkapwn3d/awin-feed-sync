using System.Globalization;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using AwinFeedSync.Core.Interfaces;
using AwinFeedSync.Core.Models;
using AwinFeedSync.Infrastructure.Configuration;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AwinFeedSync.Infrastructure.Feeds;

public class AwinProductFeedSource : IProductFeedSource
{
    private readonly HttpClient _httpClient;
    private readonly FeedsConfig _config;
    private readonly ILogger<AwinProductFeedSource> _logger;
    
    public AwinProductFeedSource(
        HttpClient httpClient,
        IOptions<FeedsConfig> config,
        ILogger<AwinProductFeedSource> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
    }
    
    public async IAsyncEnumerable<ProductRecord> GetProductsAsync(int advertiserId, [EnumeratorCancellation] CancellationToken ct = default)
    {
        // TODO: Determine feed URL from Awin API or manual config
        // Example: https://productdata.awin.com/datafeed/download/apikey/{apiKey}/language/en/fid/{feedId}/columns/...
        
        var feedUrl = await GetFeedUrlAsync(advertiserId, ct);
        
        if (string.IsNullOrEmpty(feedUrl))
        {
            _logger.LogWarning("No feed URL found for advertiser {AdvertiserId}", advertiserId);
            yield break;
        }
        
        _logger.LogInformation("Downloading feed for advertiser {AdvertiserId} from {Url}", advertiserId, feedUrl);
        
        var feedPath = await DownloadFeedAsync(advertiserId, feedUrl, ct);
        
        await foreach (var product in ParseFeedAsync(advertiserId, feedPath, ct))
        {
            yield return product;
        }
    }
    
    private async Task<string?> GetFeedUrlAsync(int advertiserId, CancellationToken ct)
    {
        // TODO: Implement feed discovery via Awin API or manual config file
        // For now, return placeholder
        _logger.LogWarning("Feed discovery not implemented. Configure ManualFeedListPath or implement API discovery.");
        return null;
    }
    
    private async Task<string> DownloadFeedAsync(int advertiserId, string feedUrl, CancellationToken ct)
    {
        Directory.CreateDirectory(_config.WorkingDirectory);
        
        var fileName = $"feed_{advertiserId}_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        var filePath = Path.Combine(_config.WorkingDirectory, fileName);
        
        using var response = await _httpClient.GetAsync(feedUrl, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();
        
        var contentType = response.Content.Headers.ContentType?.MediaType;
        
        if (contentType?.Contains("zip") == true || feedUrl.EndsWith(".zip"))
        {
            var zipPath = filePath + ".zip";
            await using (var fs = File.Create(zipPath))
            {
                await response.Content.CopyToAsync(fs, ct);
            }
            
            using var archive = ZipFile.OpenRead(zipPath);
            var entry = archive.Entries.FirstOrDefault(e => e.Name.EndsWith(".csv") || e.Name.EndsWith(".tsv"));
            
            if (entry != null)
            {
                entry.ExtractToFile(filePath, true);
            }
            
            File.Delete(zipPath);
        }
        else
        {
            await using var fs = File.Create(filePath);
            await response.Content.CopyToAsync(fs, ct);
        }
        
        return filePath;
    }
    
    private async IAsyncEnumerable<ProductRecord> ParseFeedAsync(int advertiserId, string feedPath, [EnumeratorCancellation] CancellationToken ct)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null,
            BadDataFound = null,
            Delimiter = feedPath.EndsWith(".tsv") ? "\t" : ","
        };
        
        using var reader = new StreamReader(feedPath);
        using var csv = new CsvReader(reader, config);
        
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
            
            foreach (var header in headers)
            {
                if (!columnMap.ContainsValue(header))
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
    
    private string? GetField(CsvReader csv, Dictionary<string, string> columnMap, params string[] candidates)
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
