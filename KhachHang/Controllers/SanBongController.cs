using KhachHang.Models.ViewModels;
using KhachHang.Services;
using Microsoft.AspNetCore.Mvc;

namespace KhachHang.Controllers
{
    public class SanBongController : Controller
    {
        private readonly FieldApiService _fieldApiService;
        private readonly IHttpClientFactory _httpClientFactory;

        public SanBongController(FieldApiService fieldApiService, IHttpClientFactory httpClientFactory)
        {
            _fieldApiService = fieldApiService;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(
            string? search,
            int? typeId,
            int? statusId,
            decimal? giaTu,
            int page = 1)
        {
            var filter = new SanFilterVM
            {
                Search = search,
                TypeId = typeId,
                StatusId = statusId,
                GiaTu = giaTu,
                Page = page,
                PageSize = 9
            };

            var result = await _fieldApiService.GetFieldsAsync(filter);

            var items = result.Items.AsEnumerable();

            if (giaTu.HasValue && giaTu.Value > 0)
            {
                items = items.Where(x => x.BasePrice >= giaTu.Value);
            }

            ViewBag.Search = search;
            ViewBag.SelectedTypeId = typeId;
            ViewBag.SelectedStatusId = statusId;
            ViewBag.GiaTu = giaTu;

            ViewBag.Page = result.Page;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalCount = items.Count();

            return View(items.ToList());
        }

        // Proxy ảnh từ API server để tránh lỗi CORS
        [HttpGet]
        public async Task<IActionResult> ProxyImage(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return Redirect("/images/default-field.jpg");

            try
            {
                var http = _httpClientFactory.CreateClient();
                var bytes = await http.GetByteArrayAsync(url);

                // Xác định content type theo đuôi file
                var contentType = "image/jpeg";
                if (url.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    contentType = "image/png";
                else if (url.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
                    contentType = "image/webp";
                else if (url.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                    contentType = "image/gif";

                Response.Headers["Cache-Control"] = "public, max-age=3600";
                return File(bytes, contentType);
            }
            catch
            {
                return Redirect("/images/default-field.jpg");
            }
        }
    }
}
