using KhachHang.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace KhachHang.Services
{
    public class ServiceApiService
    {
        private readonly HttpClient _httpClient;

        private const string BaseUrl =
            "http://klcnhost-001-site1.ntempurl.com/api/services";

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ServiceApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private string GetApiMessage(string content)
        {
            try
            {
                var json = JsonNode.Parse(content);
                return json?["message"]?.GetValue<string>() ?? content;
            }
            catch
            {
                return content;
            }
        }

        // GET /api/services?isAvailable=true
        public async Task<List<ServiceDto>> GetAvailableServicesAsync(
            bool? isAvailable = true)
        {
            var url = BaseUrl;

            if (isAvailable.HasValue)
                url += $"?isAvailable={isAvailable.Value.ToString().ToLower()}";

            var response = await _httpClient.GetAsync(url);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return new List<ServiceDto>();

            var result =
                JsonSerializer.Deserialize<ApiResponse<List<ServiceDto>>>(
                    content,
                    _jsonOptions);

            return result?.Data ?? new List<ServiceDto>();
        }

        // GET /api/services/{serviceId}
        public async Task<ServiceDto?> GetServiceByIdAsync(int serviceId)
        {
            var response =
                await _httpClient.GetAsync($"{BaseUrl}/{serviceId}");

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return null;

            var result =
                JsonSerializer.Deserialize<ApiResponse<ServiceDto>>(
                    content,
                    _jsonOptions);

            return result?.Data;
        }
    }
}
