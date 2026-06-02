using KhachHang.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace KhachHang.Services
{
    public class PaymentApiService
    {
        private readonly HttpClient _httpClient;

        private const string BaseUrl =
            "http://sportplusklcn10-001-site1.ltempurl.com/api/payments";

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public PaymentApiService(HttpClient httpClient)
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

        public async Task<string> CreateVnPayPaymentAsync(
      string? token,
      int bookingId,
      string paymentMode = "deposit")
        {
            SetToken(token);

            paymentMode = string.IsNullOrWhiteSpace(paymentMode)
                ? "deposit"
                : paymentMode.Trim().ToLower();

            var response = await _httpClient.PostAsync(
              $"{BaseUrl}/vnpay/create/{bookingId}?platform=web&paymentMode={Uri.EscapeDataString(paymentMode)}",
              null);

            var content =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"VNPay API lỗi {(int)response.StatusCode}: {GetApiMessage(content)}");
            }

            var json = JsonNode.Parse(content);

            bool success =
                json?["success"]?.GetValue<bool>() ?? false;

            if (!success)
            {
                throw new Exception(
                    json?["message"]?.GetValue<string>()
                    ?? "Không tạo được thanh toán VNPay.");
            }

            string? paymentUrl =
                json?["data"]?["paymentUrl"]?.GetValue<string>();

            if (string.IsNullOrWhiteSpace(paymentUrl))
            {
                throw new Exception(
                    "VNPay API không trả về paymentUrl.");
            }

            return paymentUrl;
        }

        public async Task TestSuccessAsync(string? token, int bookingId)
        {
            SetToken(token);

            var response = await _httpClient.GetAsync(
                $"{BaseUrl}/test-success?bookingId={bookingId}");

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(GetApiMessage(content));
            }
        }

        public async Task TestFailedAsync(string? token, int bookingId)
        {
            SetToken(token);

            var response = await _httpClient.GetAsync(
                $"{BaseUrl}/test-failed?bookingId={bookingId}");

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(GetApiMessage(content));
            }
        }
    }
}