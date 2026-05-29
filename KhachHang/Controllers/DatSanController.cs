using KhachHang.Models;
using KhachHang.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace KhachHang.Controllers
{
    public class DatSanController : Controller
    {
        private readonly FieldApiService _fieldApiService;
        private readonly BookingApiService _bookingApiService;
        private readonly PromotionApiService _promotionApiService;
        private readonly ServiceApiService _serviceApiService;

        public DatSanController(
              FieldApiService fieldApiService,
              BookingApiService bookingApiService,
              PromotionApiService promotionApiService,
              ServiceApiService serviceApiService)
        {
            _fieldApiService = fieldApiService;
            _bookingApiService = bookingApiService;
            _promotionApiService = promotionApiService;
            _serviceApiService = serviceApiService;
        }

        public async Task<IActionResult> Index(int id)
        {
            var field = await _fieldApiService.GetFieldByIdAsync(id);

            if (field == null)
                return NotFound();

            var today = DateOnly.FromDateTime(DateTime.Today);
            var allSchedules = new List<FieldScheduleDto>();

            for (int i = 0; i <= 7; i++)
            {
                var date = today.AddDays(i);

                var schedules =
                    await _fieldApiService.GetScheduleAsync(id, date);

                allSchedules.AddRange(schedules);
            }

            var services =
                await _serviceApiService.GetAvailableServicesAsync();

            ViewBag.AvailableSchedules = allSchedules;
            ViewBag.Services = services;

            return View(field);
        }

        [HttpPost]
        public IActionResult AddToPending(
            int fieldId,
            List<int> selectedSlotIds,
            List<int>? serviceIds,
            List<int>? quantities,
            string? promotionCode)
        {
            if (selectedSlotIds == null || !selectedSlotIds.Any())
            {
                TempData["ErrorMessage"] =
                    "Vui lòng chọn ít nhất một khung giờ.";

                return RedirectToAction(
                    "Index",
                    new { id = fieldId });
            }

            selectedSlotIds =
                selectedSlotIds
                    .Distinct()
                    .ToList();

            var pending = new PendingBookingApi
            {
                FieldId = fieldId,
                SelectedSlotIds = selectedSlotIds,
                Services = BuildServiceRequests(
                    serviceIds,
                    quantities),
                PromotionCode = promotionCode
            };

            HttpContext.Session.SetString(
                "PendingBooking",
                JsonSerializer.Serialize(pending));

            var token =
                HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction(
                    "DangNhap",
                    "TaiKhoan",
                    new
                    {
                        returnUrl =
                            "/DatSan/ConfirmPending"
                    });
            }

            return RedirectToAction("ConfirmPending");
        }

        public IActionResult ConfirmPending()
        {
            var json =
                HttpContext.Session.GetString("PendingBooking");

            if (string.IsNullOrEmpty(json))
            {
                return RedirectToAction(
                    "Index",
                    "Home");
            }

            var pending =
                JsonSerializer.Deserialize<PendingBookingApi>(json);

            if (pending == null ||
                pending.SelectedSlotIds == null ||
                !pending.SelectedSlotIds.Any())
            {
                HttpContext.Session.Remove("PendingBooking");

                TempData["ErrorMessage"] =
                    "Không tìm thấy thông tin đặt sân tạm.";

                return RedirectToAction(
                    "Index",
                    "Home");
            }

            return View(pending);
        }

        [HttpPost]
        public async Task<IActionResult> CheckPromotion(string code)
        {
            try
            {
                var promotion =
                    await _promotionApiService.GetPromotionByCodeAsync(code);

                if (promotion == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Mã không hợp lệ."
                    });
                }

                return Json(new
                {
                    success = true,
                    code = promotion.Code,
                    name = promotion.Name,
                    discount = promotion.DiscountValue,
                    description = promotion.Description
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateConfirmed(
            int fieldId,
            List<int> selectedSlotIds,
            List<int>? serviceIds,
            List<int>? quantities,
            string? promotionCode)
        {
            var token =
                HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction(
                    "DangNhap",
                    "TaiKhoan",
                    new
                    {
                        returnUrl =
                            "/DatSan/ConfirmPending"
                    });
            }

            if (selectedSlotIds == null ||
                !selectedSlotIds.Any())
            {
                TempData["ErrorMessage"] =
                    "Vui lòng chọn khung giờ.";

                return RedirectToAction(
                    "Index",
                    new { id = fieldId });
            }

            selectedSlotIds =
                selectedSlotIds
                    .Distinct()
                    .ToList();

            var request =
                new CreateBookingRequest
                {
                    FieldSlotIds = selectedSlotIds,

                    Services = BuildServiceRequests(
                        serviceIds,
                        quantities),

                    PromotionCode =
                        string.IsNullOrWhiteSpace(promotionCode)
                            ? null
                            : promotionCode.Trim(),

                    Note = "",

                    // mặc định flow cọc
                    IsFullPayment = false
                };
            try
            {
                // giữ slot
                await _bookingApiService.HoldSlotsAsync(
                    token,
                    selectedSlotIds);

                // tạo booking
                var bookingId =
                    await _bookingApiService.CreateBookingAsync(
                        token,
                        request);

                // xóa pending
                HttpContext.Session.Remove("PendingBooking");

                // thông báo thành công
                TempData["SuccessMessage"] =
                    "Xác nhận đặt sân thành công. Vui lòng chọn hình thức thanh toán để tiếp tục.";

                // chuyển qua thanh toán
                return RedirectToAction(
                    "Index",
                    "ThanhToan",
                    new { id = bookingId });
            }
            catch (Exception ex)
            {
                var message =
                    ex.Message.ToLower();

                if (message.Contains("slot") ||
                    message.Contains("duplicate") ||
                    message.Contains("trùng"))
                {
                    TempData["ErrorMessage"] =
                        "Khung giờ đã có người đặt hoặc đang được giữ.";

                    return RedirectToAction(
                        "Index",
                        new { id = fieldId });
                }

                TempData["ErrorMessage"] =
                    ex.Message;

                return RedirectToAction(
                    "ConfirmPending");
            }
        }

        public async Task<IActionResult> ChiTietDon(int id)
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
                var booking =
                    await _bookingApiService.GetBookingByIdAsync(
                        token,
                        id);

                if (booking == null)
                    return NotFound();

                return View("ChiTietDon", booking);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;

                return RedirectToAction(
                    "Index",
                    "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> HuySan(
            int bookingId,
            string? reason,
            bool returnToHistory = false)
        {
            var token =
                HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(token))
            {
                TempData["Info"] =
                    "Bạn cần đăng nhập để hủy sân.";

                return RedirectToAction(
                    "DangNhap",
                    "TaiKhoan");
            }

            try
            {
                await _bookingApiService.CancelBookingAsync(
                    token,
                    bookingId,
                    string.IsNullOrWhiteSpace(reason)
                        ? "Khách hủy sân"
                        : reason);

                TempData["SuccessMessage"] =
                    "Hủy đơn đặt sân thành công.";

                if (returnToHistory)
                {
                    return RedirectToAction(
                        "Index",
                        "LichSu",
                        new { statusId = 4 });
                }

                return RedirectToAction(
                    "ChiTietDon",
                    new { id = bookingId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;

                if (returnToHistory)
                {
                    return RedirectToAction(
                        "Index",
                        "LichSu");
                }

                return RedirectToAction(
                    "ChiTietDon",
                    new { id = bookingId });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DoiLich(
    int bookingId,
    int bookingDetailId)
        {
            var token =
                HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(token))
            {
                TempData["Info"] =
                    "Vui lòng đăng nhập để đổi lịch.";

                return RedirectToAction(
                    "DangNhap",
                    "TaiKhoan");
            }

            try
            {
                var booking =
                    await _bookingApiService.GetBookingByIdAsync(
                        token,
                        bookingId);

                if (booking == null)
                {
                    TempData["ErrorMessage"] =
                        "Không tìm thấy đơn đặt sân.";

                    return RedirectToAction(
                        "Index",
                        "LichSu");
                }

                var currentDetail =
                    booking.Details?
                        .FirstOrDefault(x => x.BookingDetailId == bookingDetailId);

                if (currentDetail == null)
                {
                    TempData["ErrorMessage"] =
                        "Không tìm thấy khung giờ cần đổi.";

                    return RedirectToAction(
                        "ChiTietDon",
                        new { id = bookingId });
                }

                string statusLower =
                    (booking.Status ?? "").ToLower();

                bool forceFullPaid =
                    HttpContext.Session.GetString($"FullPayment_{bookingId}") == "true";

                bool isDeposited =
                    booking.StatusId == 2 ||
                    statusLower.Contains("đã đặt cọc") ||
                    statusLower.Contains("đã xác nhận");

                bool isPaidFull =
                    forceFullPaid ||
                    booking.StatusId == 3 ||
                    booking.StatusId == 4 ||
                    statusLower.Contains("đã thanh toán") ||
                    statusLower.Contains("hoàn tất");

                bool isCancelled =
                    booking.StatusId == 6 ||
                    statusLower.Contains("hủy");

                if (isCancelled)
                {
                    TempData["ErrorMessage"] =
                        "Đơn đã hủy nên không thể đổi lịch.";

                    return RedirectToAction(
                        "ChiTietDon",
                        new { id = bookingId });
                }

                if (!isDeposited && !isPaidFull)
                {
                    TempData["ErrorMessage"] =
                        "Chỉ được đổi lịch khi đơn đã thanh toán cọc hoặc đã thanh toán toàn bộ.";

                    return RedirectToAction(
                        "ChiTietDon",
                        new { id = bookingId });
                }

                DateTime matchTime =
                    currentDetail.SlotDate.ToDateTime(
                        currentDetail.StartTime);

                if (DateTime.Now > matchTime.AddHours(-2))
                {
                    TempData["ErrorMessage"] =
                        "Đã quá thời gian cho phép đổi lịch sân.";

                    return RedirectToAction(
                        "ChiTietDon",
                        new { id = bookingId });
                }

                var today =
                    DateOnly.FromDateTime(DateTime.Today);

                var allSchedules =
                    new List<FieldScheduleDto>();

                for (int i = 0; i <= 7; i++)
                {
                    var date =
                        today.AddDays(i);

                    var schedules =
                        await _fieldApiService.GetScheduleAsync(
                            currentDetail.FieldId,
                            date);

                    allSchedules.AddRange(schedules);
                }

                ViewBag.BookingId = bookingId;
                ViewBag.BookingDetailId = bookingDetailId;
                ViewBag.CurrentDetail = currentDetail;
                ViewBag.AvailableSlots = allSchedules;

                return View("DoiLich", booking);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;

                return RedirectToAction(
                    "ChiTietDon",
                    new { id = bookingId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiLich(
            int bookingId,
            int bookingDetailId,
            int newFieldSlotId)
        {
            var token =
                HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(token))
            {
                TempData["Info"] =
                    "Vui lòng đăng nhập để đổi lịch.";

                return RedirectToAction(
                    "DangNhap",
                    "TaiKhoan");
            }

            if (newFieldSlotId <= 0)
            {
                TempData["ErrorMessage"] =
                    "Vui lòng chọn khung giờ mới.";

                return RedirectToAction(
                    "DoiLich",
                    new
                    {
                        bookingId,
                        bookingDetailId
                    });
            }

            try
            {
                await _bookingApiService.RescheduleAsync(
                           token,
                           bookingId,
                           bookingDetailId,
                           newFieldSlotId);

                TempData["SuccessMessage"] =
                    "Đổi lịch sân thành công. Thông tin đơn đặt sân đã được cập nhật.";

                return RedirectToAction(
                    "ChiTietDon",
                    new { id = bookingId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;

                return RedirectToAction(
                    "DoiLich",
                    new
                    {
                        bookingId,
                        bookingDetailId
                    });
            }
        }

        public IActionResult ThanhToan(int id)
        {
            return RedirectToAction(
                "Index",
                "ThanhToan",
                new { id });
        }

        private List<CreateBookingServiceRequest> BuildServiceRequests(
            List<int>? serviceIds,
            List<int>? quantities)
        {
            var services =
                new List<CreateBookingServiceRequest>();

            if (serviceIds == null ||
                quantities == null)
            {
                return services;
            }

            int count =
                Math.Min(
                    serviceIds.Count,
                    quantities.Count);

            for (int i = 0; i < count; i++)
            {
                if (quantities[i] > 0)
                {
                    services.Add(
                        new CreateBookingServiceRequest
                        {
                            ServiceId = serviceIds[i],
                            Quantity = quantities[i]
                        });
                }
            }

            return services;
        }
    }
}