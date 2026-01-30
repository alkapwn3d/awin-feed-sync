using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AwinFeedSync.Core.Models;

namespace AwinFeedSync.Core.Services;

public static class ContentHasher
{
    public static string ComputeHash(ProductRecord record)
    {
        var normalized = new
        {
            record.ProductName,
            record.ProductUrl,
            record.ImageUrl,
            record.Price,
            record.Currency,
            record.Category,
            record.Subcategory,
            record.CommissionText,
            record.CommissionRate
        };
        
        var json = JsonSerializer.Serialize(normalized);
        var bytes = Encoding.UTF8.GetBytes(json);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
    
    public static string GenerateProductKey(int advertiserId, ProductRecord record)
    {
        var identifier = !string.IsNullOrWhiteSpace(record.Sku) 
            ? record.Sku 
            : !string.IsNullOrWhiteSpace(record.FeedProductId) 
                ? record.FeedProductId 
                : NormalizeUrl(record.ProductUrl ?? string.Empty);
        
        return $"{advertiserId}:{identifier}";
    }
    
    private static string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return string.Empty;
        
        try
        {
            var uri = new Uri(url);
            return $"{uri.Host}{uri.AbsolutePath}".ToLowerInvariant();
        }
        catch
        {
            return url.ToLowerInvariant();
        }
    }
}
