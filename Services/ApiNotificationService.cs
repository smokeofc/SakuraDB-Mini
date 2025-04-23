using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SakuraDB_Mini.Config;
using SakuraDB_Mini.Models;

namespace SakuraDB_Mini.Services
{
    public class ApiNotificationService
    {
        private readonly ILogger<ApiNotificationService> _logger;
        private readonly HttpClient _httpClient;

        public ApiNotificationService(ILogger<ApiNotificationService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task NotifyFileProcessedAsync(ProcessedFileInfo fileInfo, ApiConfig apiConfig)
        {
            try
            {
                var payload = new
                {
                    fileName = fileInfo.Name,
                    fileSize = fileInfo.FileSize,
                    date = fileInfo.Date,
                    md5 = fileInfo.MD5,
                    crc32 = fileInfo.CRC32,
                    sha1 = fileInfo.SHA1,
                    source = fileInfo.Source,
                    processedAt = fileInfo.ProcessedAt
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                // Add API key as a header
                _httpClient.DefaultRequestHeaders.Clear();

                // Only add API key if it's not empty
                if (!string.IsNullOrEmpty(apiConfig.ApiKey))
                {
                    _httpClient.DefaultRequestHeaders.Add("apikey", apiConfig.ApiKey);
                }

                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response;

                switch (apiConfig.Method.ToUpper())
                {
                    case "POST":
                        response = await _httpClient.PostAsync(apiConfig.Url, content);
                        break;
                    case "PUT":
                        response = await _httpClient.PutAsync(apiConfig.Url, content);
                        break;
                    case "GET":
                        // For simple GET requests without query parameters
                        response = await _httpClient.GetAsync(apiConfig.Url);
                        break;
                    default:
                        _logger.LogError($"Unsupported HTTP method: {apiConfig.Method}");
                        return;
                }

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Successfully notified API for file: {fileInfo.Name}, status: {response.StatusCode}");
                }
                else
                {
                    _logger.LogWarning($"Failed to notify API for file: {fileInfo.Name}, status: {response.StatusCode}");
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Response content: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error notifying API for file: {fileInfo.Name}");
            }
        }

        private async Task<string> BuildQueryString(object payload)
        {
            var properties = payload.GetType().GetProperties();
            var queryParts = new List<string>();

            foreach (var prop in properties)
            {
                var value = prop.GetValue(payload)?.ToString();
                if (value != null)
                {
                    queryParts.Add($"{prop.Name.ToLowerInvariant()}={Uri.EscapeDataString(value)}");
                }
            }

            return string.Join("&", queryParts);
        }
    }
}