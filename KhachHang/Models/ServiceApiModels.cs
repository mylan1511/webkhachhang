namespace KhachHang.Models
{
    public class ServiceDto
    {
        public int ServiceId { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsAvailable { get; set; }
    }
}
