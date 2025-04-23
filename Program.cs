using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;
using System.Diagnostics;
using Serilog;
using SakuraDB_Mini.Config;
using SakuraDB_Mini.Data;
using SakuraDB_Mini.Data.Repositories;
using SakuraDB_Mini.Helpers;
using SakuraDB_Mini.Services;

namespace SakuraDB_Mini
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            // Get app config
            var appConfig = new AppConfig();
            configuration.GetSection("AppConfig").Bind(appConfig);

            // Create database directory if it doesn't exist
            EnsureDatabaseDirectoryExists(appConfig.ConnectionString);

            // Configure logging
            var logDirectory = configuration.GetValue<string>("Logging:LogDirectory") ?? "logs";
            Directory.CreateDirectory(logDirectory);
            Log.Logger = LogHelper.CreateLogger(logDirectory);

            try
            {
                Log.Information("Starting SakuraDB-Mini");

                // Create and run host
                using var host = CreateHostBuilder(args, configuration, appConfig, Log.Logger).Build();

                // Ensure database is created
                using (var scope = host.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    dbContext.Database.EnsureCreated();
                    Log.Information("Database initialized");
                }

                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "SakuraDB-Mini terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void EnsureDatabaseDirectoryExists(string connectionString)
        {
            // Extract the database path from the connection string
            string dbPath = string.Empty;

            if (connectionString.Contains("Data Source="))
            {
                dbPath = connectionString.Split("Data Source=")[1].Trim();

                // Remove any trailing parameters if present
                if (dbPath.Contains(";"))
                    dbPath = dbPath.Split(';')[0];

                // Get the directory part
                string dbDir = Path.GetDirectoryName(dbPath);

                if (!string.IsNullOrEmpty(dbDir) && !Directory.Exists(dbDir))
                {
                    Directory.CreateDirectory(dbDir);

                    // Set permissions on Linux
                    if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD() || OperatingSystem.IsMacOS())
                    {
                        try
                        {
                            using var process = new Process();
                            process.StartInfo.FileName = "chmod";
                            process.StartInfo.Arguments = $"777 \"{dbDir}\"";
                            process.StartInfo.UseShellExecute = false;
                            process.StartInfo.RedirectStandardOutput = true;
                            process.StartInfo.RedirectStandardError = true;
                            process.Start();
                            process.WaitForExit();
                        }
                        catch
                        {
                            // Just continue if chmod fails - we've at least created the directory
                        }
                    }
                }
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args, IConfiguration configuration, AppConfig appConfig, Serilog.ILogger logger) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Register configuration
                    services.AddSingleton(appConfig);

                    // Register DbContext
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlite(appConfig.ConnectionString));

                    // Register repositories
                    services.AddScoped<FileInfoRepository>();

                    // Register services
                    services.AddSingleton<ChecksumService>();
                    services.AddHttpClient<ApiNotificationService>();
                    services.AddScoped<ApiNotificationService>();
                    services.AddScoped<FileProcessingService>();
                    services.AddHostedService<FileWatcherService>();

                    // Configure logging
                    services.AddLogging(loggingBuilder =>
                    {
                        loggingBuilder.ClearProviders();
                        loggingBuilder.AddSerilog(logger);
                    });
                });
    }
}