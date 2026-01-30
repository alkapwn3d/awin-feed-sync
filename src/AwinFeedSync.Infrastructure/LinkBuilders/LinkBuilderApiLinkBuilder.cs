using System.Net.Http.Json;
using AwinFeedSync.Core.Interfaces;
using AwinFeedSync.Infrastructure.Awin;
using AwinFeedSync.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AwinFeedSync.Infrastructure.LinkBuilders;

public class LinkBuilderApiLinkBuilder : ILinkBuilder
{
    private readonly HttpClient _httpClient;
    private readonly AwinOAuthClient _oauthClient;
    private readonly AwinPublisherConfig _config;
    private readonly ILogger<LinkBuilderApiLinkBuilder> _logger;
    
    public LinkBuilderApiLinkBuilder(
        HttpClient httpClient,
        AwinOAuthClient oauthClient,
        IOptions<AwinPublisherConfig> config,
        ILogger<LinkBuilderApiLinkBuilder> logger)
    {
        _httpClient = httpClient;
        _oauthClient = oauthClient;
        _config = config.Value;
        _logger = logger;
    }
    
    public async Task<string> BuildTrackingLinkAsync(int advertiserId, string destinationUrl, string? clickRef = null, CancellationToken ct = default)
    {
        // TODO: Implement Awin Link Builder API call
        // Example endpoint: POST /publishers/{publisherId}/deeplink
        // Refer to: https://wiki.awin.com/index.php/Link_Builder_API
        
        var token = await _oauthClient.GetAccessTokenAsync(ct);
        
        var request = new HttpRequestMessage(HttpMethod.Post, 
            $"{_config.ApiBaseUrl}/publishers/{_config.PublisherId}/deeplink");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        var payload = new
        {
            advertiserId,
            url = destinationUrl,
            clickRef
        };
        
        request.Content = JsonContent.Create(payload);
        
        var response = await _httpClient.SendAsync(request, ct);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Link Builder API failed for {Url}: {StatusCode}", destinationUrl, response.StatusCode);
            throw new HttpRequestException($"Link Builder API returned {response.StatusCode}");
        }
        
        var result = await response.Content.ReadFromJsonAsync<DeepLinkResponse>(ct);
        return result?.DeepLink ?? destinationUrl;
    }
    
    public async Task<Dictionary<string, string>> BuildTrackingLinksBatchAsync(int advertiserId, List<string> destinationUrls, CancellationToken ct = default)
    {
        var result = new Dictionary<string, string>();
        
        foreach (var url in destinationUrls)
        {
            try
            {
                result[url] = await BuildTrackingLinkAsync(advertiserId, url, null, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to build tracking link for {Url}", url);
                result[url] = url;
            }
        }
        
        return result;
    }
    
    public string GetSource() => "api";
    
    private class DeepLinkResponse
    {
        public string DeepLink { get; set; } = string.Empty;
    }
}
