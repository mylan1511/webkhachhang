using KhachHang.Models;
using KhachHang.Models.ViewModels;
using KhachHang.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace KhachHang.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ProfileApiService _profileApiService;

        private const string BaseUrl =
            "http://sportplusklcn10-001-site1.ltempurl.com/api/auth";

        public TaiKhoanController(
            HttpClient httpClient,
            ProfileApiService profileApiService)
        {
            _httpClient = httpClient;
            _profileApiService = profileApiService;
        }

        public IActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(DangKyVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var request = new
            {
                email = model.Email.Trim(),
                phone = model.Phone.Trim(),
                password = model.Password,
                fullName = model.FullName.Trim()
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{BaseUrl}/register",
                request);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("",
                    string.IsNullOrWhiteSpace(content)
                        ? "Đăng ký thất bại."
                        : content);

                return View(model);
            }

            TempData["SuccessMessage"] =
                "Đăng ký thành công!";

            return RedirectToAction("DangNhap");
        }

        public IActionResult DangNhap(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(
            DangNhapVM model,
            string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            var request = new
            {
                identifier = model.Identifier.Trim(),
                password = model.Password.Trim()
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{BaseUrl}/login",
                request);

            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                string errorMessage =
                    "Sai tài khoản hoặc mật khẩu.";

                try
                {
                    var errorResult =
                        JsonSerializer.Deserialize<ApiResponse<object>>(
                            content,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                    if (!string.IsNullOrWhiteSpace(errorResult?.Message))
                    {
                        errorMessage = errorResult.Message;
                    }
                }
                catch
                {
                }

                ModelState.AddModelError("", errorMessage);

                return View(model);
            }

            ApiResponse<LoginData>? result;

            try
            {
                result = JsonSerializer.Deserialize<ApiResponse<LoginData>>(
                    content,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch
            {
                ModelState.AddModelError("",
                    "API trả về dữ liệu không đúng định dạng.");

                return View(model);
            }

            if (result == null ||
                !result.Success ||
                result.Data == null ||
                result.Data.User == null)
            {
                ModelState.AddModelError("",
                    result?.Message ?? "Đăng nhập thất bại.");

                return View(model);
            }

            HttpContext.Session.SetString(
                "AccessToken",
                result.Data.AccessToken ?? "");

            HttpContext.Session.SetString(
                "RefreshToken",
                result.Data.RefreshToken ?? "");

            HttpContext.Session.SetInt32(
                "UserId",
                result.Data.User.UserId);

            HttpContext.Session.SetString(
                "FullName",
                result.Data.User.FullName ?? "");

            HttpContext.Session.SetString(
                "Email",
                result.Data.User.Email ?? "");

            HttpContext.Session.SetString(
                "Phone",
                result.Data.User.Phone ?? "");

            HttpContext.Session.SetString(
                "Role",
                result.Data.User.Role ?? "");

            HttpContext.Session.SetString(
                "AvatarUrl",
                result.Data.User.AvatarUrl ?? "");

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    result.Data.User.UserId.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    result.Data.User.FullName ?? ""),

                new Claim(
                    ClaimTypes.Email,
                    result.Data.User.Email ?? ""),

                new Claim(
                    ClaimTypes.MobilePhone,
                    result.Data.User.Phone ?? ""),

                new Claim(
                    ClaimTypes.Role,
                    result.Data.User.Role ?? "")
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            TempData["SuccessMessage"] =
                "Đăng nhập thành công!";

            var pendingBooking =
                HttpContext.Session.GetString("PendingBooking");

            if (!string.IsNullOrEmpty(pendingBooking))
            {
                return RedirectToAction(
                    "ConfirmPending",
                    "DatSan");
            }

            if (!string.IsNullOrEmpty(returnUrl)
                && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> DangXuat()
        {
            var token =
                HttpContext.Session.GetString("AccessToken");

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        token);

                await _httpClient.PostAsync(
                    $"{BaseUrl}/logout",
                    null);
            }

            HttpContext.Session.Clear();

            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["SuccessMessage"] =
                "Đăng xuất thành công!";

            return RedirectToAction("DangNhap");
        }

        public IActionResult QuenMatKhau()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> QuenMatKhauSendOtp(
            [FromBody] ForgotPasswordRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{BaseUrl}/forgot-password",
                new
                {
                    email = request.Email
                });

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(
                    (int)response.StatusCode,
                    content);
            }

            return Content(content, "application/json");
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtpAjax(
            [FromBody] VerifyOtpRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{BaseUrl}/verify-otp",
                new
                {
                    email = request.Email,
                    otp = request.Otp
                });

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(
                    (int)response.StatusCode,
                    content);
            }

            return Content(content, "application/json");
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswordAjax(
            [FromBody] ResetPasswordRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{BaseUrl}/reset-password",
                new
                {
                    resetToken = request.ResetToken,
                    newPassword = request.NewPassword,
                    confirmPassword = request.ConfirmPassword
                });

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(
                    (int)response.StatusCode,
                    content);
            }

            return Content(content, "application/json");
        }

        public async Task<IActionResult> HoSo()
        {
            var token =
                HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(token))
            {
                TempData["Info"] =
                    "Vui lòng đăng nhập để xem hồ sơ.";

                return RedirectToAction(
                    "DangNhap",
                    "TaiKhoan");
            }

            try
            {
                var profile =
                    await _profileApiService.GetProfileAsync(token);

                return View(profile);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CapNhatHoSo(
            string FullName,
            string Phone,
            DateOnly? DateOfBirth,
            string? Address)
        {
            var token =
                HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("DangNhap");
            }

            try
            {
                var updated =
                    await _profileApiService.UpdateProfileAsync(
                        token,
                        new UpdateProfileRequest
                        {
                            FullName = FullName,
                            Phone = Phone,
                            DateOfBirth = DateOfBirth,
                            Address = Address
                        });

                if (updated != null)
                {
                    HttpContext.Session.SetString(
                        "FullName",
                        updated.FullName ?? "");

                    HttpContext.Session.SetString(
                        "Phone",
                        updated.Phone ?? "");

                    HttpContext.Session.SetString(
                        "AvatarUrl",
                        updated.Profile?.AvatarUrl ?? "");
                }

                TempData["SuccessMessage"] =
                    "Cập nhật hồ sơ thành công.";

                return RedirectToAction("HoSo");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                return RedirectToAction("HoSo");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DoiMatKhau(
            string CurrentPassword,
            string NewPassword,
            string ConfirmPassword)
        {
            var token =
                HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("DangNhap");
            }

            try
            {
                await _profileApiService.ChangePasswordAsync(
                    token,
                    new ChangePasswordRequest
                    {
                        CurrentPassword = CurrentPassword,
                        NewPassword = NewPassword,
                        ConfirmPassword = ConfirmPassword
                    });

                TempData["SuccessMessage"] =
                    "Đổi mật khẩu thành công.";

                return RedirectToAction("HoSo");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                return RedirectToAction("HoSo");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadAvatar(
            IFormFile avatarUpload)
        {
            var token =
                HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("DangNhap");
            }

            if (avatarUpload == null ||
                avatarUpload.Length <= 0)
            {
                TempData["ErrorMessage"] =
                    "Vui lòng chọn ảnh đại diện.";

                return RedirectToAction("HoSo");
            }

            try
            {
                var avatarUrl =
                    await _profileApiService.UploadAvatarAsync(
                     token,
                     avatarUpload);

                // LOAD LẠI PROFILE
                var profile =
                    await _profileApiService.GetProfileAsync(token);

                if (!string.IsNullOrWhiteSpace(
                    profile?.Profile?.AvatarUrl))
                {
                    HttpContext.Session.SetString(
                        "AvatarUrl",
                        profile.Profile.AvatarUrl);
                }
                TempData["SuccessMessage"] =
                    "Cập nhật ảnh đại diện thành công.";

                return RedirectToAction("HoSo");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                return RedirectToAction("HoSo");
            }
        }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }

    public class LoginData
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public UserDto? User { get; set; }
    }

    public class UserDto
    {
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Role { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = "";
    }

    public class VerifyOtpRequest
    {
        public string Email { get; set; } = "";
        public string Otp { get; set; } = "";
    }

    public class ResetPasswordRequest
    {
        public string ResetToken { get; set; } = "";
        public string NewPassword { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";
    }
}