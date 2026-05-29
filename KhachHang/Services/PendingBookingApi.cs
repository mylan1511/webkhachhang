namespace KhachHang.Models
{
    public class PendingBookingApi
    {
        public int FieldId { get; set; }

        public List<int> SelectedSlotIds { get; set; } = new();

        public List<CreateBookingServiceRequest> Services { get; set; } = new();

        public string? PromotionCode { get; set; }
    }
}
