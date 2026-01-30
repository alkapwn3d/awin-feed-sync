# Quick Start Guide

## Prerequisites

- .NET 8 SDK installed
- PostgreSQL 12+ running
- Awin Publisher account with API credentials

## Setup Steps

### 1. Configure Database

```bash
# Create database
psql -U postgres -c "CREATE DATABASE awin_feeds;"

# Update connection string in appsettings.json or use environment variable
export Database__ConnectionString="Host=localhost;Database=awin_feeds;Username=postgres;Password=yourpassword"
```

### 2. Configure Awin Credentials

**Development (User Secrets):**

```bash
cd src/AwinFeedSync.Console
dotnet user-secrets init
dotnet user-secrets set "AwinOAuth:ClientId" "your-client-id"
dotnet user-secrets set "AwinOAuth:ClientSecret" "your-client-secret"
dotnet user-secrets set "AwinPublisher:PublisherId" "your-publisher-id"
```

**Production (Environment Variables):**

```bash
export AwinOAuth__ClientId="your-client-id"
export AwinOAuth__ClientSecret="your-client-secret"
export AwinPublisher__PublisherId="your-publisher-id"
```

### 3. Run Migrations

```bash
# Install EF Core tools if not already installed
dotnet tool install --global dotnet-ef

# Run migrations
cd src/AwinFeedSync.Console
dotnet ef database update --project ../AwinFeedSync.Infrastructure
```

### 4. Test Run (Dry Run)

```bash
# Preview what would happen without making changes
dotnet run --project src/AwinFeedSync.Console -- --dry-run --max 1
```

### 5. First Real Sync

```bash
# Process first advertiser only
dotnet run --project src/AwinFeedSync.Console -- --run-once --max 1
```

## Important: Complete TODOs

Before running in production, you must implement the following:

### 1. Advertiser Discovery (AwinAdvertiserSource.cs)

Replace the placeholder in `GetApprovedAdvertisersAsync()` with actual Awin Publisher API endpoint:

```csharp
// TODO: Replace with actual endpoint
// Example: GET /publishers/{publisherId}/programmes?relationship=joined
// Refer to: https://wiki.awin.com/index.php/Publisher_API
```

### 2. Feed URL Discovery (AwinProductFeedSource.cs)

Implement `GetFeedUrlAsync()` to fetch feed URLs either:
- Via Awin API feed discovery endpoint
- From a manual configuration file (JSON/CSV)

```csharp
// TODO: Implement feed discovery via Awin API or manual config file
```

### 3. OAuth2 Flow (AwinOAuthClient.cs)

Update `GetAccessTokenAsync()` with proper OAuth2 implementation based on Awin's requirements:

```csharp
// TODO: Implement proper OAuth2 flow based on Awin documentation
```

### 4. Link Builder API (Optional - LinkBuilderApiLinkBuilder.cs)

If you want to use Awin's Link Builder API instead of computed links:

```csharp
// TODO: Implement Awin Link Builder API call
// Refer to: https://wiki.awin.com/index.php/Link_Builder_API
```

## Scheduling

### Linux (cron)

```bash
crontab -e
```

Add:
```
0 6 * * * cd /path/to/affiliate-marketing && /usr/bin/dotnet run --project src/AwinFeedSync.Console -- --run-once >> logs/cron.log 2>&1
```

### Windows (Task Scheduler)

1. Open Task Scheduler
2. Create Basic Task
3. Trigger: Daily at 6:00 AM
4. Action: Start a program
   - Program: `C:\Program Files\dotnet\dotnet.exe`
   - Arguments: `run --project src\AwinFeedSync.Console -- --run-once`
   - Start in: `C:\path\to\affiliate-marketing`

## Monitoring

Check sync run status:

```sql
SELECT * FROM sync_runs ORDER BY started_at DESC LIMIT 10;
```

Check product counts:

```sql
SELECT 
    a.name,
    COUNT(*) as total_products,
    COUNT(*) FILTER (WHERE p.inactive_at IS NULL) as active_products,
    MAX(p.last_seen_at) as last_sync
FROM products p
JOIN advertisers a ON p.advertiser_id = a.advertiser_id
GROUP BY a.advertiser_id, a.name
ORDER BY total_products DESC;
```

## Troubleshooting

### "No feed URL found"

Implement feed discovery in `AwinProductFeedSource.cs` or configure `ManualFeedListPath`.

### "Failed to fetch advertisers"

Check OAuth credentials and implement actual API endpoint in `AwinAdvertiserSource.cs`.

### Large feeds timing out

Increase batch size in `appsettings.json`:

```json
"Feeds": {
  "BatchSize": 5000
}
```

### Database connection errors

Verify PostgreSQL is running and connection string is correct.

## Command Line Options

- `--run-once` - Run sync once and exit (for scheduled tasks)
- `--advertiser <id>` - Process only specific advertiser
- `--dry-run` - Preview changes without saving to database
- `--max <n>` - Limit number of advertisers to process (testing)

## Example Workflows

**Test with one advertiser:**
```bash
dotnet run --project src/AwinFeedSync.Console -- --advertiser 12345 --dry-run
```

**Process first 5 advertisers:**
```bash
dotnet run --project src/AwinFeedSync.Console -- --max 5
```

**Full production sync:**
```bash
dotnet run --project src/AwinFeedSync.Console -- --run-once
```

## Support

- Awin API Documentation: https://wiki.awin.com/
- Project README: See README.md
- Implementation Details: See IMPLEMENTATION.md
