using Microsoft.Extensions.Logging;
using SakuraDB_Mini.Config;
using SakuraDB_Mini.Data.Repositories;
using SakuraDB_Mini.Models;
using System.Diagnostics;

namespace SakuraDB_Mini.Services
{
    public class FileProcessingService
    {
        private readonly ILogger<FileProcessingService> _logger;
        private readonly ChecksumService _checksumService;
        private readonly FileInfoRepository _fileInfoRepository;
        private readonly ApiNotificationService _apiNotificationService;

        public FileProcessingService(
            ILogger<FileProcessingService> logger,
            ChecksumService checksumService,
            FileInfoRepository fileInfoRepository,
            ApiNotificationService apiNotificationService)
        {
            _logger = logger;
            _checksumService = checksumService;
            _fileInfoRepository = fileInfoRepository;
            _apiNotificationService = apiNotificationService;
        }

        public async Task ProcessFileAsync(string filePath, FileWatcherConfig config)
        {
            try
            {
                _logger.LogInformation($"Processing file: {filePath}");

                // Calculate checksums
                _logger.LogInformation($"Calculating checksums for: {filePath}");
                var (md5, crc32, sha1) = _checksumService.CalculateChecksums(filePath);
                _logger.LogInformation($"Checksums calculated - MD5: {md5}, CRC32: {crc32}, SHA1: {sha1}");

                // Get file information
                var fileSystemInfo = new System.IO.FileInfo(filePath);
                var fileName = Path.GetFileName(filePath);

                // Create database entity
                var fileInfo = new ProcessedFileInfo
                {
                    Name = fileName,
                    FileSize = fileSystemInfo.Length,
                    Date = fileSystemInfo.CreationTime,
                    MD5 = md5,
                    CRC32 = crc32,
                    SHA1 = sha1,
                    Source = config.Source,
                    ProcessedAt = DateTime.UtcNow
                };

                // Save to database
                _logger.LogInformation($"Saving file info to database: {fileName}");
                await _fileInfoRepository.AddFileInfoAsync(fileInfo);

                // Move file to output directory
                var relativePath = Path.GetRelativePath(config.InPath, Path.GetDirectoryName(filePath) ?? string.Empty);
                var outputDir = Path.Combine(config.OutPath, relativePath);

                // Create output directory if it doesn't exist
                Directory.CreateDirectory(outputDir);

                // Try to set directory permissions
                try
                {
                    ChmodDirectory(outputDir);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Unable to set permissions on {outputDir}: {ex.Message}");
                }

                var destinationPath = Path.Combine(outputDir, fileName);
                _logger.LogInformation($"Moving file from {filePath} to {destinationPath}");

                // If file already exists in the destination, append a timestamp
                if (File.Exists(destinationPath))
                {
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    string extension = Path.GetExtension(fileName);
                    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    destinationPath = Path.Combine(outputDir, $"{fileNameWithoutExt}_{timestamp}{extension}");
                    _logger.LogInformation($"File already exists in destination. Using new path: {destinationPath}");
                }

                File.Move(filePath, destinationPath);

                // Notify API
                if (!string.IsNullOrEmpty(config.Api.Url))
                {
                    _logger.LogInformation($"Notifying API for file: {fileName}");
                    await _apiNotificationService.NotifyFileProcessedAsync(fileInfo, config.Api);
                }
                else
                {
                    _logger.LogInformation($"No API notification configured for source: {config.Source}");
                }

                _logger.LogInformation($"Successfully processed file: {fileName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing file: {filePath}");
                throw;
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