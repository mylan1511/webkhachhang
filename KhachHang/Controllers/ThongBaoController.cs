using KhachHang.Models;
using KhachHang.Services;
using Microsoft.AspNetCore.Mvc;

namespace KhachHang.Controllers
{
    public class ThongBaoController : Controller
    {
        private readonly NotificationApiService _notificationApiService;

        public ThongBaoController(
            NotificationApiService notificationApiService)
        {
            _notificationApiService = notificationApiService;
        }

        public async Task<IActionResult> Index(
            bool? isRead,
            int page = 1)
        {
            var token =
                HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(token))
            {
                TempData["Info"] =
                    "Vui lòng đăng nhập để xem thông báo.";

                return RedirectToAction(
                    "DangNhap",
                    "TaiKhoan",
                    new
                    {
                        returnUrl = Url.Action("Index", "ThongBao")
                    });
            }

            try
            {
                var result =
                    await _notificationApiService.GetNotificationsAsync(
                        token,
                        isRead,
                        page,
                        10);

                var unreadCount =
                    await _notificationApiService.GetUnreadCountAsync(token);

                HttpContext.Session.SetInt32(
                    "UnreadNotificationCount",
                    unreadCount);

                ViewBag.IsRead = isRead;
                ViewBag.Page = result.Page;
                ViewBag.TotalPages = result.TotalPages;
                ViewBag.TotalCount = result.TotalCount;
                ViewBag.UnreadCount = unreadCount;

                return View(result.Items);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                return View(new List<NotificationDto>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(
            int notificationId,
            int? refId,
            string? type)
        {
            var token =
                HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction(
                    "DangNhap",
                    "TaiKhoan");
            }

            try
            {
                await _notificationApiService.MarkAsReadAsync(
                    token,
                    notificationId);

                var unreadCount =
                    await _notificationApiService.GetUnreadCountAsync(token);

                HttpContext.Session.SetInt32(
                    "UnreadNotificationCount",
                    unreadCount);

                if (refId.HasValue &&
                    !string.IsNullOrWhiteSpace(type) &&
                    type.ToLower().Contains("booking"))
                {
                    return RedirectToAction(
                        "ChiTietDon",
                        "DatSan",
                        new { id = refId.Value });
                }

                TempData["SuccessMessage"] =
                    "Đã đánh dấu thông báo là đã đọc.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var token =
                HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction(
                    "DangNhap",
                    "TaiKhoan");
            }

            try
            {
                await _notificationApiService.MarkAllAsReadAsync(token);

                HttpContext.Session.SetInt32(
                    "UnreadNotificationCount",
                    0);

                TempData["SuccessMessage"] =
                    "Đã đánh dấu tất cả thông báo là đã đọc.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}