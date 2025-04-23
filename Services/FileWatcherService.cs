using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SakuraDB_Mini.Config;
using System.Diagnostics;

namespace SakuraDB_Mini.Services
{
    public class FileWatcherService : BackgroundService
    {
        private readonly ILogger<FileWatcherService> _logger;
        private readonly AppConfig _appConfig;
        private readonly FileProcessingService _fileProcessingService;
        private readonly TimeSpan _interval;

        // Define directories to ignore
        private readonly string[] _ignoredDirectories = new[] { "@eaDir", "#recycle", ".DS_Store", "System Volume Information" };

        public FileWatcherService(
            ILogger<FileWatcherService> logger,
            AppConfig appConfig,
            FileProcessingService fileProcessingService)
        {
            _logger = logger;
            _appConfig = appConfig;
            _fileProcessingService = fileProcessingService;
            _interval = TimeSpan.FromMinutes(_appConfig.ScanIntervalMinutes);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("File Watcher Service is starting.");

            // Make sure all watch directories exist
            foreach (var folderConfig in _appConfig.WatchFolders)
            {
                EnsureDirectoryExists(folderConfig.InPath);
                EnsureDirectoryExists(folderConfig.OutPath);

                // Create a test file to set proper permissions for future directories
                var permissionsMarkerFile = Path.Combine(folderConfig.OutPath, ".permissions_marker");
                if (!File.Exists(permissionsMarkerFile))
                {
                    File.WriteAllText(permissionsMarkerFile, "This file is used to maintain proper folder permissions");
                    _logger.LogInformation($"Created permissions marker file in {folderConfig.OutPath}");
                }
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Scanning watch folders for new files...");

                foreach (var folderConfig in _appConfig.WatchFolders)
                {
                    try
                    {
                        _logger.LogInformation($"Scanning folder: {folderConfig.InPath}");

                        // Get all files in the watch directory (recursively)
                        var directoryInfo = new DirectoryInfo(folderConfig.InPath);
                        if (!directoryInfo.Exists)
                        {
                            _logger.LogWarning($"Watch directory does not exist: {folderConfig.InPath}");
                            continue;
                        }

                        // Get files but exclude those in ignored directories
                        var files = directoryInfo.GetFiles("*", SearchOption.AllDirectories)
                            .Where(file => !_ignoredDirectories.Any(dir => file.FullName.Contains(dir)))
                            .ToArray();

                        _logger.LogInformation($"Found {files.Length} files in {folderConfig.InPath} (including subdirectories)");

                        // Process each file
                        foreach (var file in files)
                        {
                            try
                            {
                                await _fileProcessingService.ProcessFileAsync(file.FullName, folderConfig);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Error processing file {file.FullName}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing folder: {folderConfig.InPath}");
                    }
                }

                _logger.LogInformation($"Scan complete. Waiting {_interval.TotalMinutes} minutes until next scan.");
                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("File Watcher Service is stopping.");
        }

        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                _logger.LogInformation($"Creating directory: {path}");
                Directory.CreateDirectory(path);

                try
                {
                    // Try to set directory permissions
                    ChmodDirectory(path);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Unable to set permissions on {path}: {ex.Message}");
                }
            }
        }

        private void ChmodDirectory(string path)
        {
            if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD() || OperatingSystem.IsMacOS())
            {
                try
                {
                    // Use Process.Start to execute chmod on Linux/Unix
                    using (var process = new System.Diagnostics.Process())
                    {
                        process.StartInfo.FileName = "chmod";
                        process.StartInfo.Arguments = $"775 \"{path}\"";
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;
                        process.Start();
                        process.WaitForExit();

                        if (process.ExitCode != 0)
                        {
                            string error = process.StandardError.ReadToEnd();
                            _logger.LogWarning($"chmod command exited with code {process.ExitCode}: {error}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to execute chmod: {ex.Message}");
                }
            }
        }
    }
}