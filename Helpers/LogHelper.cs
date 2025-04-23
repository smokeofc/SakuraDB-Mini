using Serilog;
using Serilog.Events;

namespace SakuraDB_Mini.Helpers
{
    public static class LogHelper
    {
        public static ILogger CreateLogger(string logDirectory)
        {
            // Ensure log directory exists
            Directory.CreateDirectory(logDirectory);
            
            // Get current date for log file name in YYYYMMDD format
            string logFileName = DateTime.Now.ToString("yyyyMMdd") + ".log";
            string logFilePath = Path.Combine(logDirectory, logFileName);

            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }
    }
}