# Windows Service Installation

## Prerequisites

- .NET 8 Runtime installed on Windows
- Administrator privileges
- PostgreSQL installed and configured

## Installation Steps

### 1. Publish the Application

```powershell
cd C:\path\to\affiliate-marketing
dotnet publish src\AwinFeedSync.Console\AwinFeedSync.Console.csproj -c Release -o C:\Services\AwinFeedSync
```

### 2. Configure Application

Edit `C:\Services\AwinFeedSync\appsettings.json`:

```json
{
  "AwinOAuth": {
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret"
  },
  "AwinPublisher": {
    "PublisherId": "your-publisher-id"
  },
  "Database": {
    "ConnectionString": "Host=localhost;Database=awin_feeds;Username=postgres;Password=yourpassword"
  },
  "Service": {
    "SyncIntervalHours": 6
  }
}
```

### 3. Create Windows Service

**Using sc.exe (built-in):**

```powershell
sc.exe create "AwinFeedSync" binPath="C:\Services\AwinFeedSync\AwinFeedSync.Console.exe" start=auto DisplayName="Awin Feed Sync Service"
sc.exe description "AwinFeedSync" "Synchronizes Awin affiliate product feeds to PostgreSQL database"
```

**Or using PowerShell:**

```powershell
New-Service -Name "AwinFeedSync" `
    -BinaryPathName "C:\Services\AwinFeedSync\AwinFeedSync.Console.exe" `
    -DisplayName "Awin Feed Sync Service" `
    -Description "Synchronizes Awin affiliate product feeds to PostgreSQL database" `
    -StartupType Automatic
```

### 4. Start the Service

```powershell
Start-Service AwinFeedSync
```

### 5. Verify Service is Running

```powershell
Get-Service AwinFeedSync
```

Or open Services (services.msc) and look for "Awin Feed Sync Service"

## Service Management

### Check Status
```powershell
Get-Service AwinFeedSync | Select-Object Status, StartType
```

### Stop Service
```powershell
Stop-Service AwinFeedSync
```

### Restart Service
```powershell
Restart-Service AwinFeedSync
```

### View Logs
Check: `C:\Services\AwinFeedSync\logs\awin-sync-YYYY-MM-DD.log`

Or Windows Event Viewer → Application logs

### Uninstall Service
```powershell
Stop-Service AwinFeedSync
sc.exe delete AwinFeedSync
```

## Testing Before Installing as Service

Run in console mode first to verify everything works:

```powershell
cd C:\Services\AwinFeedSync
.\AwinFeedSync.Console.exe --console --dry-run --max 1
```

## Configuration Options

### Sync Interval

Edit `appsettings.json`:
```json
"Service": {
  "SyncIntervalHours": 6  // Change to desired hours
}
```

### Database Connection

Update connection string for your PostgreSQL instance:
```json
"Database": {
  "ConnectionString": "Host=your-server;Database=awin_feeds;Username=user;Password=pass"
}
```

## Troubleshooting

### Service won't start
1. Check Event Viewer for errors
2. Verify .NET 8 Runtime is installed
3. Check database connectivity
4. Verify file permissions on service directory

### Service starts but doesn't sync
1. Check logs in `C:\Services\AwinFeedSync\logs\`
2. Verify Awin credentials are correct
3. Check database connection string
4. Ensure PostgreSQL is running

### Change service account
```powershell
sc.exe config AwinFeedSync obj="DOMAIN\Username" password="password"
```

## Command Line Options (Console Mode)

```powershell
# Run once and exit
.\AwinFeedSync.Console.exe --console --run-once

# Dry run (preview only)
.\AwinFeedSync.Console.exe --console --dry-run

# Process specific advertiser
.\AwinFeedSync.Console.exe --console --advertiser 12345

# Limit advertisers for testing
.\AwinFeedSync.Console.exe --console --max 5
```

## Service vs Console Mode

- **Without `--console` flag**: Runs as Windows Service (continuous background operation)
- **With `--console` or `--run-once` flag**: Runs as console app (one-time execution)

This allows you to test the application before installing it as a service.
