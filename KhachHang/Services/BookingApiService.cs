using KhachHang.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace KhachHang.Services
{
    public class BookingApiService
    {
        private readonly HttpClient _httpClient;

        private const string BaseUrl =
            "http://sportplusklcn10-001-site1.ltempurl.com/api/bookings";

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public BookingApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private void SetToken(string? token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;

            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
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

        public async Task HoldSlotsAsync(
            string? token,
            List<int> fieldSlotIds)
        {
            SetToken(token);

            var response = await _httpClient.PostAsJsonAsync(
                $"{BaseUrl}/hold",
                new
                {
                    fieldSlotIds
                });

            var content =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(GetApiMessage(content));
            }

            var json = JsonNode.Parse(content);

            var success =
                json?["success"]?.GetValue<bool>() ?? true;

            if (!success)
            {
                throw new Exception(GetApiMessage(content));
            }
        }

        public async Task<int> CreateBookingAsync(
            string? token,
            CreateBookingRequest request)
        {
            SetToken(token);

            var response = await _httpClient.PostAsJsonAsync(
                BaseUrl,
                request);

            var content =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(GetApiMessage(content));
            }

            var json = JsonNode.Parse(content);

            var success =
                json?["success"]?.GetValue<bool>() ?? false;

            if (!success)
            {
                throw new Exception(GetApiMessage(content));
            }

            var bookingId =
                json?["data"]?["bookingId"]?.GetValue<int>() ?? 0;

            if (bookingId <= 0)
            {
                throw new Exception(
                    "API không trả về mã đơn đặt sân.");
            }

            return bookingId;
        }

        public async Task<BookingDto?> GetBookingByIdAsync(
            string? token,
            int bookingId)
        {
            SetToken(token);

            var response =
                await _httpClient.GetAsync($"{BaseUrl}/{bookingId}");

            var content =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(GetApiMessage(content));
            }

            var result =
                JsonSerializer.Deserialize<ApiResponse<BookingDto>>(
                    content,
                    _jsonOptions);

            if (result == null || !result.Success)
            {
                throw new Exception(
                    result?.Message ?? "Không lấy được thông tin đơn đặt sân.");
            }

            return result.Data;
        }

        public async Task CancelBookingAsync(
    string? token,
    int bookingId,
    string reason)
        {
            SetToken(token);

            var response = await _httpClient.PostAsJsonAsync(
                $"{BaseUrl}/{bookingId}/cancel",
                new
                {
                    reason
                });

            var content =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(GetApiMessage(content));
            }

            var json = JsonNode.Parse(content);

            var success =
                json?["success"]?.GetValue<bool>() ?? true;

            if (!success)
            {
                throw new Exception(GetApiMessage(content));
            }
        }

        public async Task RescheduleAsync(
            string? token,
            int bookingId,
            int bookingDetailId,
            int newFieldSlotId)
        {
            SetToken(token);

            var response = await _httpClient.PostAsJsonAsync(
                $"{BaseUrl}/{bookingId}/reschedule",
                new
                {
                    bookingDetailId,
                    newFieldSlotId
                });

            var content =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(GetApiMessage(content));
            }

            var json = JsonNode.Parse(content);

            var success =
                json?["success"]?.GetValue<bool>() ?? true;

            if (!success)
            {
                throw new Exception(GetApiMessage(content));
            }
        }

        public async Task<PagedResult<BookingSummaryDto>> GetMyBookingsAsync(
    string? token,
    int? statusId = null,
    int page = 1,
    int pageSize = 10)
        {
            SetToken(token);

            var query = new List<string>
    {
        $"page={page}",
        $"pageSize={pageSize}"
    };

            if (statusId.HasValue && statusId.Value > 0)
            {
                query.Add($"statusId={statusId.Value}");
            }

            var url =
                $"{BaseUrl}/my?{string.Join("&", query)}";

            var response =
                await _httpClient.GetAsync(url);

            var content =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(GetApiMessage(content));
            }

            var result =
                JsonSerializer.Deserialize<ApiResponse<PagedResult<BookingSummaryDto>>>(
                    content,
                    _jsonOptions);

            return result?.Data ?? new PagedResult<BookingSummaryDto>();
        }


    }
}