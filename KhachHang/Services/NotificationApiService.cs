using KhachHang.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace KhachHang.Services
{
    public class NotificationApiService
    {
        private readonly HttpClient _httpClient;

        private const string BaseUrl =
            "http://klcnhost-001-site1.ntempurl.com/api/notifications";

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public NotificationApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // FIX Bug 3: Tạo request riêng cho từng call,
        // tránh race condition khi set DefaultRequestHeaders chung.
        private HttpRequestMessage CreateRequest(
            HttpMethod method,
            string url,
            string? token)
        {
            var request = new HttpRequestMessage(method, url);

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return request;
        }

        private string GetApiMessage(string content)
        {
            try
            {
                var json = JsonNode.Parse(content);

                return json?["message"]?.GetValue<string>()
                       ?? content;
            }
            catch
            {
                return content;
            }
        }

        public async Task<PagedResult<NotificationDto>> GetNotificationsAsync(
            string? token,
            bool? isRead = null,
            int page = 1,
            int pageSize = 10)
        {
            var query = new List<string>
            {
                $"Page={page}",
                $"PageSize={pageSize}"
            };

            if (isRead.HasValue)
            {
                query.Add($"IsRead={isRead.Value.ToString().ToLower()}");
            }

            var request = CreateRequest(
                HttpMethod.Get,
                $"{BaseUrl}?{string.Join("&", query)}",
                token);

            var response = await _httpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(GetApiMessage(content));
            }

            var result =
                JsonSerializer.Deserialize<ApiResponse<PagedResult<NotificationDto>>>(
                    content,
                    _jsonOptions);

            return result?.Data ?? new PagedResult<NotificationDto>();
        }

        public async Task<int> GetUnreadCountAsync(string? token)
        {
            var request = CreateRequest(
                HttpMethod.Get,
                $"{BaseUrl}/unread-count",
                token);

            var response = await _httpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return 0;
            }

            var result =
                JsonSerializer.Deserialize<ApiResponse<int>>(
                    content,
                    _jsonOptions);

            return result?.Data ?? 0;
        }

        public async Task MarkAsReadAsync(
            string? token,
            int notificationId)
        {
            var request = CreateRequest(
                HttpMethod.Patch,
                $"{BaseUrl}/{notificationId}/read",
                token);

            // FIX Bug 4: Truyền empty content thay vì null
            // tránh server trả 415 Unsupported Media Type.
            request.Content = new StringContent(string.Empty);

            var response = await _httpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(GetApiMessage(content));
            }
        }

        public async Task MarkAllAsReadAsync(string? token)
        {
            var request = CreateRequest(
                HttpMethod.Patch,
                $"{BaseUrl}/read-all",
                token);

            // FIX Bug 4: Truyền empty content thay vì null
            request.Content = new StringContent(string.Empty);

            var response = await _httpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(GetApiMessage(content));
            }
        }
    }
}
