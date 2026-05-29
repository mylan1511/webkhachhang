using KhachHang.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace KhachHang.Services
{
    public class ProfileApiService
    {
        private readonly HttpClient _httpClient;

        private const string BaseUrl =
            "http://klcnhost-001-site1.ntempurl.com/api/profile";

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ProfileApiService(HttpClient httpClient)
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

        public async Task<ProfileUserDto?> GetProfileAsync(string? token)
        {
            SetToken(token);

            var response = await _httpClient.GetAsync(BaseUrl);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(GetApiMessage(content));

            var result =
                JsonSerializer.Deserialize<ApiResponse<ProfileUserDto>>(
                    content,
                    _jsonOptions);

            if (result == null || !result.Success)
                throw new Exception(result?.Message ?? "Không lấy được hồ sơ.");

            return result.Data;
        }

        public async Task<ProfileUserDto?> UpdateProfileAsync(
            string? token,
            UpdateProfileRequest request)
        {
            SetToken(token);

            var response =
                await _httpClient.PutAsJsonAsync(BaseUrl, request);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(GetApiMessage(content));

            var result =
                JsonSerializer.Deserialize<ApiResponse<ProfileUserDto>>(
                    content,
                    _jsonOptions);

            if (result == null || !result.Success)
                throw new Exception(result?.Message ?? "Cập nhật hồ sơ thất bại.");

            return result.Data;
        }

        public async Task ChangePasswordAsync(
            string? token,
            ChangePasswordRequest request)
        {
            SetToken(token);

            var response =
                await _httpClient.PutAsJsonAsync(
                    $"{BaseUrl}/change-password",
                    request);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(GetApiMessage(content));

            var result =
                JsonSerializer.Deserialize<ApiResponse<string>>(
                    content,
                    _jsonOptions);

            if (result != null && !result.Success)
                throw new Exception(result.Message ?? "Đổi mật khẩu thất bại.");
        }

        public async Task<string?> UploadAvatarAsync(
      string? token,
      IFormFile file)
        {
            SetToken(token);

            using var form = new MultipartFormDataContent();

            using var stream = file.OpenReadStream();

            var fileContent = new StreamContent(stream);

            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue(file.ContentType);

            form.Add(
                fileContent,
                "file",
                file.FileName);

            var response =
                await _httpClient.PutAsync(
                    $"{BaseUrl}/avatar",
                    form);

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
                throw new Exception(
                    json?["message"]?.GetValue<string>()
                    ?? "Upload avatar thất bại.");
            }

            var dataNode = json?["data"];

            if (dataNode == null)
                return null;

            // TH1: data là string
            if (dataNode.GetValueKind() == JsonValueKind.String)
            {
                return dataNode.GetValue<string>();
            }

            // TH2: data là object có avatarUrl
            var avatarUrl =
                dataNode?["avatarUrl"]?.GetValue<string>();

            return avatarUrl;
        }
    }
}