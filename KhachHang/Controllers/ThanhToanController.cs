using KhachHang.Services;
using Microsoft.AspNetCore.Mvc;

namespace KhachHang.Controllers
{
    public class ThanhToanController : Controller
    {
        private readonly BookingApiService _bookingApiService;
        private readonly PaymentApiService _paymentApiService;

        public ThanhToanController(
            BookingApiService bookingApiService,
            PaymentApiService paymentApiService)
        {
            _bookingApiService = bookingApiService;
            _paymentApiService = paymentApiService;
        }

        public async Task<IActionResult> Index(int id)
        {
            var token = HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(token))
            {
                TempData["Info"] = "Vui lòng đăng nhập để thanh toán.";

                return RedirectToAction(
                    "DangNhap",
                    "TaiKhoan",
                    new
                    {
                        returnUrl = Url.Action("Index", "ThanhToan", new { id })
                    });
            }

            try
            {
                var booking = await _bookingApiService.GetBookingByIdAsync(token, id);

                if (booking == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn đặt sân.";
                    return RedirectToAction("Index", "LichSu");
                }

                string statusLower = (booking.Status ?? "").ToLower();

                bool isCancelled =
                    booking.StatusId == 6 ||
                    statusLower.Contains("hủy");

                bool isCompleted =
                    booking.StatusId == 4 ||
                    statusLower.Contains("hoàn tất") ||
                    statusLower.Contains("đã thanh toán");

                if (isCancelled)
                {
                    TempData["ErrorMessage"] = "Đơn đặt sân đã hủy, không thể thanh toán.";

                    return RedirectToAction(
                        "ChiTietDon",
                        "DatSan",
                        new { id });
                }

                if (isCompleted)
                {
                    TempData["Info"] = "Đơn này đã được thanh toán hoàn tất.";

                    return RedirectToAction(
                        "ChiTietDon",
                        "DatSan",
                        new { id });
                }

                return View(booking);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "LichSu");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoThanhToanVnPay(
            int bookingId,
            string paymentMode = "deposit")
        {
            var token = HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(token))
            {
                TempData["Info"] = "Vui lòng đăng nhập để thanh toán.";

                return RedirectToAction(
                    "DangNhap",
                    "TaiKhoan",
                    new
                    {
                        returnUrl = Url.Action("Index", "ThanhToan", new { id = bookingId })
                    });
            }

            try
            {
                paymentMode = string.IsNullOrWhiteSpace(paymentMode)
                    ? "deposit"
                    : paymentMode.Trim().ToLower();

                var payUrl = await _paymentApiService.CreateVnPayPaymentAsync(
                    token,
                    bookingId,
                    paymentMode);

                if (paymentMode == "full")
                {
                    HttpContext.Session.SetString(
                        $"FullPayment_{bookingId}",
                        "true");
                }

                return Redirect(payUrl);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                return RedirectToAction(
                    "Index",
                    "ThanhToan",
                    new { id = bookingId });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TestThanhCong(int bookingId)
        {
            var token = HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(token))
            {
                TempData["Info"] = "Vui lòng đăng nhập để thanh toán.";

                return RedirectToAction(
                    "DangNhap",
                    "TaiKhoan",
                    new
                    {
                        returnUrl = Url.Action("Index", "ThanhToan", new { id = bookingId })
                    });
            }

            try
            {
                await _paymentApiService.TestSuccessAsync(token, bookingId);

                return RedirectToAction(
                    "ThanhCong",
                    "ThanhToan",
                    new { bookingId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                return RedirectToAction(
                    "Index",
                    "ThanhToan",
                    new { id = bookingId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TestThatBai(int bookingId)
        {
            var token = HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(token))
            {
                TempData["Info"] = "Vui lòng đăng nhập để thanh toán.";

                return RedirectToAction(
                    "DangNhap",
                    "TaiKhoan",
                    new
                    {
                        returnUrl = Url.Action("Index", "ThanhToan", new { id = bookingId })
                    });
            }

            try
            {
                await _paymentApiService.TestFailedAsync(token, bookingId);

                TempData["ErrorMessage"] =
                    "Thanh toán VNPay thất bại hoặc đã bị hủy.";

                return RedirectToAction(
                    "Index",
                    "ThanhToan",
                    new { id = bookingId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                return RedirectToAction(
                    "Index",
                    "ThanhToan",
                    new { id = bookingId });
            }
        }

        public async Task<IActionResult> ThanhCong(int? bookingId)
        {
            if (!bookingId.HasValue)
            {
                TempData["SuccessMessage"] = "Thanh toán thành công.";
                return RedirectToAction("Index", "LichSu");
            }

            var token = HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(token))
            {
                TempData["SuccessMessage"] = "Thanh toán VNPay thành công.";

                return RedirectToAction(
                    "ChiTietDon",
                    "DatSan",
                    new { id = bookingId.Value });
            }

            try
            {
                var booking = await _bookingApiService.GetBookingByIdAsync(
                    token,
                    bookingId.Value);

                if (booking == null)
                {
                    TempData["SuccessMessage"] = "Thanh toán VNPay thành công.";
                    return RedirectToAction("Index", "LichSu");
                }

                return View(booking);
            }
            catch
            {
                TempData["SuccessMessage"] = "Thanh toán VNPay thành công.";

                return RedirectToAction(
                    "ChiTietDon",
                    "DatSan",
                    new { id = bookingId.Value });
            }
        }

        public IActionResult ThatBai(int? bookingId)
        {
            TempData["ErrorMessage"] =
                "Thanh toán VNPay không thành công hoặc đã bị hủy.";

            if (bookingId.HasValue)
            {
                return RedirectToAction(
                    "Index",
                    "ThanhToan",
                    new { id = bookingId.Value });
            }

            return RedirectToAction("Index", "LichSu");
        }

        [HttpGet]
        [Route("ThanhToan/ThanhCong/{bookingId:int}")]
        public async Task<IActionResult> ThanhCong(int bookingId)
        {
            ViewBag.BookingId = bookingId;

            var token = HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(token))
            {
                TempData["SuccessMessage"] = "Thanh toán VNPay thành công.";
                return View("ThanhCong");
            }

            try
            {
                var booking = await _bookingApiService.GetBookingByIdAsync(
                    token,
                    bookingId);

                return View("ThanhCong", booking);
            }
            catch
            {
                TempData["SuccessMessage"] = "Thanh toán VNPay thành công.";
                return View("ThanhCong");
            }
        }
        [HttpGet]
        [Route("ThanhToan/ThatBai/{bookingId:int}")]
        public IActionResult ThatBai(int bookingId)
        {
            ViewBag.BookingId = bookingId;
            return View("ThatBai");
        }
    }
}