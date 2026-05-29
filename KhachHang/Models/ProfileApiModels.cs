namespace KhachHang.Models
{
    public class ProfileUserDto
    {
        public int UserId { get; set; }

        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Role { get; set; }

        public int RoleId { get; set; }

        public string? Status { get; set; }

        public int StatusId { get; set; }

        public DateTime? CreatedAt { get; set; }

        public ProfileInfoDto? Profile { get; set; }
    }

    public class ProfileInfoDto
    {
        public string? AvatarUrl { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        public string? Address { get; set; }
    }

    public class UpdateProfileRequest
    {
        public string? FullName { get; set; }

        public string? Phone { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        public string? Address { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = "";

        public string NewPassword { get; set; } = "";

        public string ConfirmPassword { get; set; } = "";
    }
}