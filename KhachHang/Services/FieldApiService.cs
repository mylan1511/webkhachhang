using KhachHang.Models;
using KhachHang.Models.ViewModels;
using System.Text.Json;

namespace KhachHang.Services
{
    public class FieldApiService
    {
        private readonly HttpClient _httpClient;

        private const string BaseUrl =
            "http://sportplusklcn10-001-site1.ltempurl.com/api/fields";

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public FieldApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PagedResult<FieldDto>> GetFieldsAsync(
            SanFilterVM filter)
        {
            var query = new List<string>();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                query.Add(
                    $"Search={Uri.EscapeDataString(filter.Search.Trim())}");
            }

            if (filter.TypeId.HasValue &&
                filter.TypeId.Value > 0)
            {
                query.Add($"TypeId={filter.TypeId.Value}");
            }

            if (filter.StatusId.HasValue &&
                filter.StatusId.Value > 0)
            {
                query.Add($"StatusId={filter.StatusId.Value}");
            }

            query.Add($"Page={filter.Page}");
            query.Add($"PageSize={filter.PageSize}");

            var url =
                $"{BaseUrl}?{string.Join("&", query)}";

            var response =
                await _httpClient.GetAsync(url);

            var content =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new PagedResult<FieldDto>();
            }

            var result =
                JsonSerializer.Deserialize<
                    ApiResponse<PagedResult<FieldDto>>>(
                    content,
                    _jsonOptions);

            var data =
                result?.Data ?? new PagedResult<FieldDto>();


            if (filter.GiaTu.HasValue &&
                filter.GiaTu.Value > 0)
            {
                data.Items = data.Items
                    .Where(x =>
                        x.BasePrice >= filter.GiaTu.Value)
                    .ToList();

                data.TotalCount = data.Items.Count;
                data.TotalPages = 1;
                data.Page = 1;
                data.HasNextPage = false;
                data.HasPreviousPage = false;
            }

            return data;
        }

        public async Task<FieldDto?> GetFieldByIdAsync(
            int fieldId)
        {
            var response =
                await _httpClient.GetAsync(
                    $"{BaseUrl}/{fieldId}");

            var content =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result =
                JsonSerializer.Deserialize<
                    ApiResponse<FieldDto>>(
                    content,
                    _jsonOptions);

            return result?.Data;
        }

        public async Task<List<FieldScheduleDto>>
            GetScheduleAsync(
                int? fieldId,
                DateOnly date,
                int? typeId = null)
        {
            var query = new List<string>
            {
                $"Date={date:yyyy-MM-dd}"
            };

            if (fieldId.HasValue &&
                fieldId.Value > 0)
            {
                query.Add($"FieldId={fieldId.Value}");
            }

            if (typeId.HasValue &&
                typeId.Value > 0)
            {
                query.Add($"TypeId={typeId.Value}");
            }

            var url =
                $"{BaseUrl}/schedule?{string.Join("&", query)}";

            var response =
                await _httpClient.GetAsync(url);

            var content =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new List<FieldScheduleDto>();
            }

            var result =
                JsonSerializer.Deserialize<
                    ApiResponse<List<FieldScheduleDto>>>(
                    content,
                    _jsonOptions);

            return result?.Data ??
                   new List<FieldScheduleDto>();
        }
    }
}