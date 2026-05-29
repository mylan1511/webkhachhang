namespace KhachHang.Models
{
    public class PendingBooking
    {
        public int FieldId { get; set; }
        public List<int> SelectedSlotIds { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}