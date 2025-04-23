namespace SakuraDB_Mini.Config
{
    public class FileWatcherConfig
    {
        public string InPath { get; set; } = string.Empty;
        public string OutPath { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public ApiConfig Api { get; set; } = new();
    }

    public class ApiConfig
    {
        public string Url { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string Method { get; set; } = "POST";
    }
}