using System.ComponentModel.DataAnnotations;

namespace KhachHang.Models.ViewModels
{
    public class DangNhapVM
    {
        [Required(ErrorMessage = "Email hoặc số điện thoại không được để trống")]
        [Display(Name = "Email hoặc Số điện thoại")]
        public string Identifier { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; } = false;
    }
}