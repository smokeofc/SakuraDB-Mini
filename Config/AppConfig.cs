namespace SakuraDB_Mini.Config
{
    public class AppConfig
    {
        public List<FileWatcherConfig> WatchFolders { get; set; } = new();
        public int ScanIntervalMinutes { get; set; } = 5;
        public string ConnectionString { get; set; } = string.Empty;
    }
}