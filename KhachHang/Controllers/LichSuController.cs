using KhachHang.Services;
using Microsoft.AspNetCore.Mvc;
using KhachHang.Models;

namespace KhachHang.Controllers
{
    public class LichSuController : Controller
    {
        private readonly BookingApiService _bookingApiService;

        public LichSuController(BookingApiService bookingApiService)
        {
            _bookingApiService = bookingApiService;
        }

        public async Task<IActionResult> Index(int? statusId, int page = 1)
        {
            var token = HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(token))
            {
                TempData["Info"] = "Bạn cần đăng nhập để xem lịch sử đặt sân.";

                return RedirectToAction(
                    "DangNhap",
                    "TaiKhoan",
                    new
                    {
                        returnUrl = Url.Action("Index", "LichSu")
                    });
            }

            try
            {
                var result = await _bookingApiService.GetMyBookingsAsync(
                    token,
                    null,
                    page,
                    50);

                IEnumerable<BookingSummaryDto> filtered = result.Items;

                if (statusId.HasValue)
                {
                    filtered = statusId.Value switch
                    {
                        5 => filtered.Where(x =>
                            x.StatusId == 1 ||
                            x.StatusId == 5 ||
                            (x.Status ?? "").ToLower().Contains("chờ")),

                        2 => filtered.Where(x =>
                            x.StatusId == 2 ||
                            (x.Status ?? "").ToLower().Contains("đã đặt cọc")),

                        4 => filtered.Where(x =>
                            x.StatusId == 4 ||
                            x.StatusId == 3 ||
                            (x.Status ?? "").ToLower().Contains("hoàn tất") ||
                            (x.Status ?? "").ToLower().Contains("đã thanh toán")),

                        6 => filtered.Where(x =>
                            x.StatusId == 6 ||
                            (x.Status ?? "").ToLower().Contains("hủy") ||
                            (x.Status ?? "").ToLower().Contains("quá hạn")),

                        _ => filtered.Where(x => x.StatusId == statusId.Value)
                    };
                }

                ViewBag.StatusId = statusId;
                ViewBag.Page = 1;
                ViewBag.TotalPages = 1;
                ViewBag.TotalCount = filtered.Count();

                return View(filtered.ToList());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(new List<BookingSummaryDto>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> HuySan(
            int bookingId,
            string reason,
            bool returnToHistory = false)
        {
            var token = HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(token))
            {
                TempData["Info"] = "Vui lòng đăng nhập để tiếp tục.";
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            try
            {
                var booking = await _bookingApiService.GetBookingByIdAsync(
                    token,
                    bookingId);

                if (booking == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn đặt sân.";
                    return RedirectToAction("Index", "LichSu");
                }

                string statusLower = (booking.Status ?? "").ToLower();

                bool isCancelled =
                    booking.StatusId == 6 ||
                    statusLower.Contains("hủy");

                if (isCancelled)
                {
                    TempData["Info"] = "Đơn này đã được hủy trước đó.";

                    return RedirectToAction(
                        "ChiTietDon",
                        "DatSan",
                        new { id = bookingId });
                }

                var firstSlot = booking.Details?
                    .OrderBy(x => x.SlotDate)
                    .ThenBy(x => x.StartTime)
                    .FirstOrDefault();

                if (firstSlot != null)
                {
                    DateTime matchTime =
                        firstSlot.SlotDate.ToDateTime(firstSlot.StartTime);

                    DateTime cancelDeadline =
                        matchTime.AddHours(-2);

                    if (DateTime.Now > cancelDeadline)
                    {
                        TempData["ErrorMessage"] =
                            "Đã quá thời gian cho phép hủy sân (trước 2 tiếng).";

                        return RedirectToAction(
                            "ChiTietDon",
                            "DatSan",
                            new { id = bookingId });
                    }
                }

                reason = string.IsNullOrWhiteSpace(reason)
                    ? "Khách hủy sân - mất tiền cọc theo chính sách"
                    : reason;

                await _bookingApiService.CancelBookingAsync(
                    token,
                    bookingId,
                    reason);

                HttpContext.Session.Remove($"FullPayment_{bookingId}");
                HttpContext.Session.Remove($"FakeDepositedBooking_{bookingId}");
                HttpContext.Session.Remove($"FakePaidBooking_{bookingId}");

                if (booking.StatusId == 2)
                {
                    TempData["SuccessMessage"] =
                        "Hủy sân thành công. Tiền cọc sẽ không được hoàn lại theo chính sách hệ thống.";
                }
                else if (
                    booking.StatusId == 3 ||
                    booking.StatusId == 4 ||
                    HttpContext.Session.GetString($"FullPayment_{bookingId}") == "true")
                {
                    TempData["SuccessMessage"] =
                        "Hủy sân thành công. Hệ thống sẽ giữ lại tiền theo chính sách đối với đơn đã thanh toán toàn bộ.";
                }
                else
                {
                    TempData["SuccessMessage"] =
                        "Hủy sân thành công. Đơn đặt sân đã được hủy.";
                }

                if (returnToHistory)
                {
                    return RedirectToAction("Index", "LichSu");
                }

                return RedirectToAction(
                    "ChiTietDon",
                    "DatSan",
                    new { id = bookingId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                if (returnToHistory)
                {
                    return RedirectToAction("Index", "LichSu");
                }

                return RedirectToAction(
                    "ChiTietDon",
                    "DatSan",
                    new { id = bookingId });
            }
        }
    }
}