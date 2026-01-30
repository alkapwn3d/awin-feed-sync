# Awin Feed Sync

.NET 8 console application that synchronizes Awin affiliate product feeds to PostgreSQL with change detection and OAuth2 authentication.

## Features

- **OAuth2 Authentication**: Secure token management for Awin API access
- **Advertiser Discovery**: Automatically fetches approved/joined programmes
- **Product Feed Parsing**: Handles CSV/TSV feeds (including zipped formats)
- **Change Detection**: SHA-256 content hashing to track product changes
- **Flexible Column Mapping**: Case-insensitive mapping handles varying feed formats
- **Tracking Link Generation**: Supports both computed links and Awin Link Builder API
- **Batch Processing**: Configurable batch sizes for efficient database writes
- **Idempotent Runs**: Audit trail via `sync_runs` table

## Prerequisites

- .NET 8 SDK
- PostgreSQL 12+
- Awin Publisher Account with API access

## Configuration

### 1. Database Setup

Create a PostgreSQL database:

```sql
CREATE DATABASE awin_feeds;
```

Run migrations:

```bash
cd src/AwinFeedSync.Console
dotnet ef database update --project ../AwinFeedSync.Infrastructure
```

### 2. OAuth2 Credentials

Obtain OAuth2 credentials from your Awin account dashboard.

**Option A: User Secrets (Development)**

```bash
cd src/AwinFeedSync.Console
dotnet user-secrets init
dotnet user-secrets set "AwinOAuth:ClientId" "your-client-id"
dotnet user-secrets set "AwinOAuth:ClientSecret" "your-client-secret"
dotnet user-secrets set "AwinPublisher:PublisherId" "your-publisher-id"
```

**Option B: Environment Variables (Production)**

```bash
export AwinOAuth__ClientId="your-client-id"
export AwinOAuth__ClientSecret="your-client-secret"
export AwinPublisher__PublisherId="your-publisher-id"
export Database__ConnectionString="Host=localhost;Database=awin_feeds;Username=postgres;Password=yourpassword"
```

### 3. Configuration File

Edit `appsettings.json`:

```json
{
  "AwinOAuth": {
    "ClientId": "",
    "ClientSecret": "",
    "TokenUrl": "https://api.awin.com/oauth2/token"
  },
  "AwinPublisher": {
    "PublisherId": "",
    "ApiBaseUrl": "https://api.awin.com"
  },
  "Feeds": {
    "WorkingDirectory": "./feeds",
    "BatchSize": 2000,
    "TreatMissingProductsAsInactive": true
  },
  "Database": {
    "ConnectionString": "Host=localhost;Database=awin_feeds;Username=postgres;Password=postgres"
  }
}
```

## Usage

### Console Mode (One-Time Execution)

```bash
# Dry run - preview only, no changes
dotnet run --project src/AwinFeedSync.Console -- --console --dry-run --max 1

# Run once - process all advertisers
dotnet run --project src/AwinFeedSync.Console -- --console --run-once

# Process specific advertiser
dotnet run --project src/AwinFeedSync.Console -- --console --advertiser 12345

# Limit advertisers for testing
dotnet run --project src/AwinFeedSync.Console -- --console --max 5
```

### Service Mode (Continuous Operation)

```bash
# Run as background service (syncs every 6 hours)
dotnet run --project src/AwinFeedSync.Console

# Configure sync interval in appsettings.json:
# "Service": { "SyncIntervalHours": 6 }
```

**Note:** Use `--console` flag for one-time execution. Without it, runs as a service.

## Scheduling

### Linux (cron)

```bash
crontab -e
```

Add:

```
0 6 * * * cd /path/to/AwinFeedSync && /usr/bin/dotnet run --project src/AwinFeedSync.Console -- --run-once >> logs/cron.log 2>&1
```

### Windows (Task Scheduler)

1. Open Task Scheduler
2. Create Basic Task
3. Trigger: Daily at 6:00 AM
4. Action: Start a program
   - Program: `C:\Program Files\dotnet\dotnet.exe`
   - Arguments: `run --project src\AwinFeedSync.Console -- --run-once`
   - Start in: `C:\path\to\AwinFeedSync`

## Architecture

### Projects

- **AwinFeedSync.Core**: Domain models, interfaces, hashing logic
- **AwinFeedSync.Infrastructure**: Awin API clients, feed parsing, EF Core repository
- **AwinFeedSync.Console**: CLI entry point with dependency injection
- **AwinFeedSync.Tests**: xUnit tests

### Key Components

- **AwinOAuthClient**: Manages OAuth2 token acquisition/refresh
- **AwinAdvertiserSource**: Fetches approved programmes via Publisher API
- **AwinProductFeedSource**: Downloads and parses product feeds (CSV/TSV/ZIP)
- **ComputedLinkBuilder**: Generates `awin1.com/cread.php` tracking links
- **LinkBuilderApiLinkBuilder**: Uses Awin Link Builder API (optional)
- **ProductRepository**: EF Core repository with upsert logic
- **FeedSyncService**: Orchestrates sync workflow

### Database Schema

**advertisers**
- `advertiser_id` (PK)
- `name`, `status`, `default_commission_text`, `updated_at`

**products**
- `id` (PK), `advertiser_id` (FK)
- `product_key` (unique per advertiser)
- `feed_product_id`, `sku`, `product_name`, `product_url`, `image_url`
- `price`, `currency`, `category`, `subcategory`
- `commission_text`, `commission_rate`
- `tracking_url`, `tracking_url_source`
- `extra` (jsonb for unmapped columns)
- `content_hash` (SHA-256)
- `last_seen_at`, `last_changed_at`, `last_updated_at`, `inactive_at`
- `ai_summary`, `ai_summary_status`, `ai_summary_updated_at` (future use)

**sync_runs**
- `run_id` (PK), `started_at`, `finished_at`, `status`, `error_text`
- `advertisers_processed`, `products_seen`, `products_changed`

## Troubleshooting

### Large Feeds

Increase batch size in `appsettings.json`:

```json
"Feeds": {
  "BatchSize": 5000
}
```

### Missing Columns

The parser uses case-insensitive flexible mapping. Check logs for warnings about unmapped columns. Extra columns are stored in the `extra` jsonb field.

### Feed Discovery

Currently requires manual configuration. Implement `GetFeedUrlAsync` in `AwinProductFeedSource.cs` to use Awin's feed discovery API or configure `ManualFeedListPath`.

### OAuth Token Issues

Ensure `ClientId`, `ClientSecret`, and `PublisherId` are correct. Check Awin API documentation for required scopes.

## TODO

- [ ] Implement Awin Publisher API endpoint for advertiser discovery (see `AwinAdvertiserSource.cs`)
- [ ] Implement feed URL discovery via Awin API (see `AwinProductFeedSource.cs`)
- [ ] Complete Link Builder API integration (see `LinkBuilderApiLinkBuilder.cs`)
- [ ] Add support for manual feed list configuration file
- [ ] Implement refresh token persistence
- [ ] Add AI summarization worker for product descriptions

## References

- [Awin Publisher API Documentation](https://wiki.awin.com/index.php/Publisher_API)
- [Awin Product Feeds](https://wiki.awin.com/index.php/Product_Feeds)
- [Awin Link Builder API](https://wiki.awin.com/index.php/Link_Builder_API)
- [Awin OAuth2 Guide](https://wiki.awin.com/index.php/OAuth_2.0)

## License

MIT
