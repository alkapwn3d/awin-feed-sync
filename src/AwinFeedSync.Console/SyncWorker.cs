using AwinFeedSync.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AwinFeedSync.Console;

public class SyncWorker : BackgroundService
{
    private readonly FeedSyncService _syncService;
    private readonly ILogger<SyncWorker> _logger;
    private readonly TimeSpan _syncInterval;

    public SyncWorker(FeedSyncService syncService, ILogger<SyncWorker> logger, IConfiguration configuration)
    {
        _syncService = syncService;
        _logger = logger;
        
        // Default to 6 hours, configurable via appsettings
        var intervalHours = configuration.GetValue<int>("Service:SyncIntervalHours", 6);
        _syncInterval = TimeSpan.FromHours(intervalHours);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Awin Feed Sync Service started. Sync interval: {Interval} hours", _syncInterval.TotalHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Starting scheduled sync at {Time}", DateTime.UtcNow);
                await _syncService.RunSyncAsync(null, null, false, stoppingToken);
                _logger.LogInformation("Scheduled sync completed at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during scheduled sync");
            }

            _logger.LogInformation("Next sync scheduled in {Hours} hours", _syncInterval.TotalHours);
            await Task.Delay(_syncInterval, stoppingToken);
        }

        _logger.LogInformation("Awin Feed Sync Service stopped");
    }
}
