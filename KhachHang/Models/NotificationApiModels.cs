namespace KhachHang.Models
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }

        public string? Title { get; set; }

        public string? Body { get; set; }

        public string? Type { get; set; }

        public int? RefId { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}