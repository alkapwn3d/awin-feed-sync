using AwinFeedSync.Core.Interfaces;
using AwinFeedSync.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AwinFeedSync.Infrastructure.LinkBuilders;

public class ComputedLinkBuilder : ILinkBuilder
{
    private readonly AwinPublisherConfig _config;
    private readonly ILogger<ComputedLinkBuilder> _logger;
    
    public ComputedLinkBuilder(IOptions<AwinPublisherConfig> config, ILogger<ComputedLinkBuilder> logger)
    {
        _config = config.Value;
        _logger = logger;
    }
    
    public Task<string> BuildTrackingLinkAsync(int advertiserId, string destinationUrl, string? clickRef = null, CancellationToken ct = default)
    {
        var encodedUrl = Uri.EscapeDataString(destinationUrl);
        var link = $"https://www.awin1.com/cread.php?awinmid={advertiserId}&awinaffid={_config.PublisherId}&ued={encodedUrl}";
        
        if (!string.IsNullOrEmpty(clickRef))
        {
            link += $"&clickref={Uri.EscapeDataString(clickRef)}";
        }
        
        return Task.FromResult(link);
    }
    
    public async Task<Dictionary<string, string>> BuildTrackingLinksBatchAsync(int advertiserId, List<string> destinationUrls, CancellationToken ct = default)
    {
        var result = new Dictionary<string, string>();
        
        foreach (var url in destinationUrls)
        {
            result[url] = await BuildTrackingLinkAsync(advertiserId, url, null, ct);
        }
        
        return result;
    }
    
    public string GetSource() => "computed";
}
