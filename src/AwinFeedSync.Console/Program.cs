using AwinFeedSync.Core.Interfaces;
using AwinFeedSync.Infrastructure.Awin;
using AwinFeedSync.Infrastructure.Configuration;
using AwinFeedSync.Infrastructure.Data;
using AwinFeedSync.Infrastructure.Feeds;
using AwinFeedSync.Infrastructure.LinkBuilders;
using AwinFeedSync.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

// Parse command line arguments
bool runOnce = args.Contains("--run-once");
int? advertiserId = GetIntArg(args, "--advertiser");
bool dryRun = args.Contains("--dry-run");
int? max = GetIntArg(args, "--max");

var host = CreateHostBuilder(args).Build();

using var scope = host.Services.CreateScope();
var syncService = scope.ServiceProvider.GetRequiredService<FeedSyncService>();

await syncService.RunSyncAsync(advertiserId, max, dryRun);

return 0;

static int? GetIntArg(string[] args, string name)
{
    var idx = Array.IndexOf(args, name);
    if (idx >= 0 && idx + 1 < args.Length && int.TryParse(args[idx + 1], out var value))
        return value;
    return null;
}

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseSerilog((context, config) =>
        {
            config.ReadFrom.Configuration(context.Configuration);
        })
        .ConfigureServices((context, services) =>
        {
            var config = context.Configuration;
            
            services.Configure<AwinOAuthConfig>(config.GetSection("AwinOAuth"));
            services.Configure<AwinPublisherConfig>(config.GetSection("AwinPublisher"));
            services.Configure<FeedsConfig>(config.GetSection("Feeds"));
            services.Configure<DatabaseConfig>(config.GetSection("Database"));
            
            var connString = config.GetSection("Database:ConnectionString").Value 
                ?? throw new InvalidOperationException("Database connection string not configured");
            
            services.AddDbContext<AwinDbContext>(options =>
                options.UseNpgsql(connString));
            
            services.AddHttpClient<AwinOAuthClient>();
            services.AddHttpClient<AwinAdvertiserSource>();
            services.AddHttpClient<AwinProductFeedSource>();
            services.AddHttpClient<LinkBuilderApiLinkBuilder>();
            
            services.AddSingleton<AwinOAuthClient>();
            services.AddScoped<IAdvertiserSource, AwinAdvertiserSource>();
            services.AddScoped<IProductFeedSource, AwinProductFeedSource>();
            services.AddScoped<ILinkBuilder, ComputedLinkBuilder>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<FeedSyncService>();
        });
