using KhachHang.Models.ViewModels;
using KhachHang.Services;
using Microsoft.AspNetCore.Mvc;

namespace KhachHang.Controllers
{
    public class HomeController : Controller
    {
        private readonly FieldApiService _fieldApiService;

        public HomeController(FieldApiService fieldApiService)
        {
            _fieldApiService = fieldApiService;
        }

        public async Task<IActionResult> Index()
        {
            bool isLoggedIn =
                HttpContext.Session.GetString("UserId") != null;

            if (isLoggedIn)
            {
                var filter = new SanFilterVM
                {
                    Page = 1,
                    PageSize = 6
                };

                var result =
                    await _fieldApiService.GetFieldsAsync(filter);

                return View("Private", result.Items);
            }

            return View(); // Chưa login
        }
    }
}
