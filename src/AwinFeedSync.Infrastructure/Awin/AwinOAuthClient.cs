using System.Net.Http.Json;
using System.Text.Json;
using AwinFeedSync.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AwinFeedSync.Infrastructure.Awin;

public class AwinOAuthClient
{
    private readonly HttpClient _httpClient;
    private readonly AwinOAuthConfig _config;
    private readonly ILogger<AwinOAuthClient> _logger;
    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    
    public AwinOAuthClient(HttpClient httpClient, IOptions<AwinOAuthConfig> config, ILogger<AwinOAuthClient> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
    }
    
    public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
        {
            return _accessToken;
        }
        
        _logger.LogInformation("Acquiring new OAuth2 access token");
        
        // TODO: Implement proper OAuth2 flow based on Awin documentation
        // This is a placeholder for client credentials grant
        var request = new HttpRequestMessage(HttpMethod.Post, _config.TokenUrl);
        var formData = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _config.ClientId,
            ["client_secret"] = _config.ClientSecret
        };
        
        if (_config.Scopes.Length > 0)
        {
            formData["scope"] = string.Join(" ", _config.Scopes);
        }
        
        request.Content = new FormUrlEncodedContent(formData);
        
        var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        
        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(ct);
        
        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
        {
            throw new InvalidOperationException("Failed to acquire access token");
        }
        
        _accessToken = tokenResponse.AccessToken;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);
        
        _logger.LogInformation("Access token acquired, expires at {Expiry}", _tokenExpiry);
        
        return _accessToken;
    }
    
    private class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public string? RefreshToken { get; set; }
    }
}
