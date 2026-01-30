namespace AwinFeedSync.Core.Interfaces;

public interface ILinkBuilder
{
    Task<string> BuildTrackingLinkAsync(int advertiserId, string destinationUrl, string? clickRef = null, CancellationToken ct = default);
    Task<Dictionary<string, string>> BuildTrackingLinksBatchAsync(int advertiserId, List<string> destinationUrls, CancellationToken ct = default);
    string GetSource();
}
