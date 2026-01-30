using System.Net.Http.Json;
using AwinFeedSync.Core.Interfaces;
using AwinFeedSync.Core.Models;
using AwinFeedSync.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AwinFeedSync.Infrastructure.Awin;

public class AwinAdvertiserSource : IAdvertiserSource
{
    private readonly HttpClient _httpClient;
    private readonly AwinOAuthClient _oauthClient;
    private readonly AwinPublisherConfig _config;
    private readonly ILogger<AwinAdvertiserSource> _logger;
    
    public AwinAdvertiserSource(
        HttpClient httpClient,
        AwinOAuthClient oauthClient,
        IOptions<AwinPublisherConfig> config,
        ILogger<AwinAdvertiserSource> logger)
    {
        _httpClient = httpClient;
        _oauthClient = oauthClient;
        _config = config.Value;
        _logger = logger;
    }
    
    public async Task<List<Advertiser>> GetApprovedAdvertisersAsync(CancellationToken ct = default)
    {
        // TODO: Replace with actual Awin Publisher API endpoint for joined/approved programmes
        // Example: GET /publishers/{publisherId}/programmes?relationship=joined
        // Refer to: https://wiki.awin.com/index.php/Publisher_API
        
        _logger.LogInformation("Fetching approved advertisers for publisher {PublisherId}", _config.PublisherId);
        
        var token = await _oauthClient.GetAccessTokenAsync(ct);
        
        var request = new HttpRequestMessage(HttpMethod.Get, 
            $"{_config.ApiBaseUrl}/publishers/{_config.PublisherId}/programmes?relationship=joined");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        var response = await _httpClient.SendAsync(request, ct);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch advertisers: {StatusCode}", response.StatusCode);
            return new List<Advertiser>();
        }
        
        // TODO: Parse actual Awin API response format
        var programmes = await response.Content.ReadFromJsonAsync<List<ProgrammeDto>>(ct);
        
        return programmes?.Select(p => new Advertiser
        {
            AdvertiserId = p.Id,
            Name = p.Name,
            Status = p.Status,
            DefaultCommissionText = p.CommissionGroup,
            UpdatedAt = DateTime.UtcNow
        }).ToList() ?? new List<Advertiser>();
    }
    
    private class ProgrammeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? CommissionGroup { get; set; }
    }
}
