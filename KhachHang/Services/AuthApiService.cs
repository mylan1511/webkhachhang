using System.Net.Http.Json;

namespace KhachHang.Services
{
    public class AuthApiService
    {
        private readonly HttpClient _http;

        private const string BaseUrl =
            "http://klcnhost-001-site1.ntempurl.com/api/auth";

        public AuthApiService(HttpClient http)
        {
            _http = http;
        }

        // =========================
        // REGISTER
        // =========================
        public async Task<bool> Register(object request)
        {
            var res = await _http.PostAsJsonAsync($"{BaseUrl}/register", request);

            return res.IsSuccessStatusCode;
        }

        // =========================
        // LOGIN
        // =========================
        public async Task<LoginResponse?> Login(object request)
        {
            var res = await _http.PostAsJsonAsync($"{BaseUrl}/login", request);

            if (!res.IsSuccessStatusCode) return null;

            var result = await res.Content.ReadFromJsonAsync<LoginResponse>();

            return result;
        }

        // =========================
        // FORGOT PASSWORD
        // =========================
        public async Task<bool> ForgotPassword(object request)
        {
            var res = await _http.PostAsJsonAsync($"{BaseUrl}/forgot-password", request);

            return res.IsSuccessStatusCode;
        }

        // =========================
        // VERIFY OTP
        // =========================
        public async Task<ResetOtpResponse?> VerifyOtp(object request)
        {
            var res = await _http.PostAsJsonAsync($"{BaseUrl}/verify-otp", request);

            if (!res.IsSuccessStatusCode) return null;

            return await res.Content.ReadFromJsonAsync<ResetOtpResponse>();
        }

        // =========================
        // RESET PASSWORD
        // =========================
        public async Task<bool> ResetPassword(object request)
        {
            var res = await _http.PostAsJsonAsync($"{BaseUrl}/reset-password", request);

            return res.IsSuccessStatusCode;
        }
    }

    // =========================
    // DTO RESPONSE
    // =========================
    public class LoginResponse
    {
        public bool Success { get; set; }
        public LoginData data { get; set; }
    }

    public class LoginData
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public UserInfo user { get; set; }
    }

    public class UserInfo
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    public class ResetOtpResponse
    {
        public bool success { get; set; }
        public ResetData data { get; set; }
    }

    public class ResetData
    {
        public string resetToken { get; set; }
    }
}