using KhachHang.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace KhachHang.Services
{
    public class PromotionApiService
    {
        private readonly HttpClient _httpClient;

        private const string BaseUrl =
            "http://sportplusklcn10-001-site1.ltempurl.com/api/promotions";

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public PromotionApiService(HttpClient httpClient)
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

        public async Task<PromotionDto?> GetPromotionByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            var response = await _httpClient.GetAsync(
                $"{BaseUrl}/{Uri.EscapeDataString(code.Trim())}");

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(GetApiMessage(content));

            var result =
                JsonSerializer.Deserialize<ApiResponse<PromotionDto>>(
                    content,
                    _jsonOptions);

            if (result == null || !result.Success)
                throw new Exception(result?.Message ?? "Mã khuyến mãi không hợp lệ.");

            return result.Data;
        }
    }
}