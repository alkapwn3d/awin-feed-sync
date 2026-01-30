namespace AwinFeedSync.Infrastructure.Configuration;

public class AwinOAuthConfig
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string AuthBaseUrl { get; set; } = "https://api.awin.com/oauth2";
    public string TokenUrl { get; set; } = "https://api.awin.com/oauth2/token";
    public string[] Scopes { get; set; } = Array.Empty<string>();
    public string? RedirectUri { get; set; }
}

public class AwinPublisherConfig
{
    public string PublisherId { get; set; } = string.Empty;
    public string ApiBaseUrl { get; set; } = "https://api.awin.com";
}

public class FeedsConfig
{
    public string WorkingDirectory { get; set; } = "./feeds";
    public int MaxParallelDownloads { get; set; } = 4;
    public int BatchSize { get; set; } = 2000;
    public string FeedDiscoveryMode { get; set; } = "api";
    public string? ManualFeedListPath { get; set; }
    public bool TreatMissingProductsAsInactive { get; set; } = true;
    public bool SkipUpdateIfUnchanged { get; set; } = true;
}

public class DatabaseConfig
{
    public string ConnectionString { get; set; } = string.Empty;
}
