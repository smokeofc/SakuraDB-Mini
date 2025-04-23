# SakuraDB-Mini

A file processing service that monitors directories for new files, calculates checksums (MD5, CRC32, SHA-1), stores file metadata in SQLite, moves processed files, and triggers API notifications.

## Features

- Monitors configured folders for new files
- Calculates MD5, CRC32, and SHA-1 checksums with double verification
- Stores file metadata in SQLite database using Entity Framework Core
- Moves processed files to output directories
- Sends API notifications when files are processed
- Configurable via JSON
- Daily log files with YYYYMMDD.log naming format
- Docker containerization for easy deployment
- Designed to work with Portainer

## Configuration

Configuration is done via `appsettings.json`:

```json
{
  "AppConfig": {
    "WatchFolders": [
      {
        "InPath": "/data/watch/folder1",
        "OutPath": "/data/out/folder1",
        "Source": "Source1",
        "Api": {
          "Url": "https://api.example.com/filenotification",
          "ApiKey": "your_api_key_here",
          "Method": "POST"
        }
      }
    ],
    "ScanIntervalMinutes": 5,
    "ConnectionString": "Data Source=/data/db/fileprocessing.db"
  },
  "Logging": {
    "LogDirectory": "/data/logs",
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

### Configuration Options

- `WatchFolders`: List of folders to monitor
  - `InPath`: Directory to watch for new files
  - `OutPath`: Directory to move processed files to
  - `Source`: Source identifier stored in the database
  - `Api`: API notification configuration
    - `Url`: API endpoint URL
    - `ApiKey`: API key for authentication
    - `Method`: HTTP method (POST/PUT)
- `ScanIntervalMinutes`: Interval between folder scans in minutes
- `ConnectionString`: SQLite database connection string
- `LogDirectory`: Directory for storing log files

## Running the Application

### Using Docker Compose

1. Clone the repository
2. Configure `appsettings.json` with your settings
3. Run with Docker Compose:

```bash
docker-compose up -d
```

### Using Portainer

1. Navigate to your Portainer instance
2. Go to "Stacks"
3. Click "Add stack"
4. Enter a name for the stack (e.g., "sakuradb-mini")
5. Upload the docker-compose.yml file or paste its content
6. Click "Deploy the stack"

## Volume Mounts

The application uses the following directory structure in the container:

- `/data/watch/` - Input directories
- `/data/out/` - Output directories
- `/data/db/` - SQLite database
- `/data/logs/` - Log files

You should mount these directories to persistent storage on your host system.

## Database Schema

The application uses SQLite with Entity Framework Core. The database schema includes:

- `FileInfos` table with columns:
  - `Id` (Primary Key)
  - `Name` (File name)
  - `FileSize` (Size in bytes)
  - `Date` (File creation date)
  - `MD5` (MD5 hash)
  - `CRC32` (CRC32 hash)
  - `SHA1` (SHA-1 hash)
  - `Source` (Source identifier)
  - `ProcessedAt` (Processing timestamp)

## Building from Source

```bash
dotnet restore
dotnet build
dotnet publish -c Release
```