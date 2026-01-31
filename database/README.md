# Database Setup

## PostgreSQL Setup

1. **Install PostgreSQL**
   - Download from https://www.postgresql.org/download/windows/
   - Run installer and set password for `postgres` user
   - Default port: 5432

2. **Create Database**
   ```cmd
   psql -U postgres
   CREATE DATABASE awin_feeds;
   \q
   ```

3. **Run Migrations**
   ```cmd
   cd src\AwinFeedSync.Console
   dotnet ef database update --project ..\AwinFeedSync.Infrastructure
   ```

4. **Load Sample Data (Optional)**
   ```cmd
   psql -U postgres -d awin_feeds -f database\sample-data.sql
   ```

## Connection String

Update `appsettings.json` or use environment variables:

```json
{
  "Database": {
    "ConnectionString": "Host=localhost;Database=awin_feeds;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

Or set environment variable:
```bash
# Linux/Mac
export Database__ConnectionString="Host=localhost;Database=awin_feeds;Username=postgres;Password=YOUR_PASSWORD"

# Windows (PowerShell)
$env:Database__ConnectionString="Host=localhost;Database=awin_feeds;Username=postgres;Password=YOUR_PASSWORD"

# Windows (CMD)
set Database__ConnectionString=Host=localhost;Database=awin_feeds;Username=postgres;Password=YOUR_PASSWORD
```

## Database Schema

### Tables

- **advertisers**: Awin advertiser/merchant information
- **products**: Product catalog with tracking URLs
- **sync_runs**: Audit trail of sync operations
- **__EFMigrationsHistory**: EF Core migration tracking

### Sample Data

The `sample-data.sql` script includes:
- 3 sample advertisers (Tech Store, Fashion Boutique, Home & Garden)
- 5 sample products across different categories
- 1 completed sync run record

## Verify Setup

```sql
-- Check tables
\dt

-- View advertisers
SELECT advertiser_id, name, status FROM advertisers;

-- View products
SELECT product_name, price, currency, category FROM products;

-- View sync history
SELECT * FROM sync_runs;
```

## Troubleshooting

### Connection Issues

1. Verify PostgreSQL is running:
   - Open Services (services.msc)
   - Look for postgresql service and ensure it's running

2. Check connection string in `appsettings.json`

3. Verify user credentials:
   ```cmd
   psql -h localhost -U postgres -d awin_feeds
   ```

### Migration Issues

If migrations fail, ensure:
- `Microsoft.EntityFrameworkCore.Design` package is installed (version 8.0.23)
- `AwinDbContextFactory.cs` exists in Infrastructure\Data
- Connection string is accessible (appsettings.json or environment variable)

### Reset Database

```cmd
REM Drop and recreate
psql -U postgres -c "DROP DATABASE awin_feeds;"
psql -U postgres -c "CREATE DATABASE awin_feeds;"

REM Re-run migrations
cd src\AwinFeedSync.Console
dotnet ef database update --project ..\AwinFeedSync.Infrastructure
```
```
