namespace KhachHang.Models
{
    public class CreateBookingRequest
    {
        public List<int> FieldSlotIds { get; set; } = new();

        public List<CreateBookingServiceRequest> Services { get; set; } = new();

        public string? PromotionCode { get; set; }

        public string? Note { get; set; }

        public bool IsFullPayment { get; set; }
    }

    public class CreateBookingServiceRequest
    {
        public int ServiceId { get; set; }

        public int Quantity { get; set; }
    }

    public class BookingDto
    {
        public int BookingId { get; set; }

        public UserInfo? Customer { get; set; }  // ← đổi UserDto → UserInfo

        public string? Status { get; set; }

        public int StatusId { get; set; }

        public decimal? SubTotal { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? DepositAmount { get; set; }

        public string? PromotionCode { get; set; }

        public string? Note { get; set; }

        public string? CancelReason { get; set; }

        public int RescheduleCount { get; set; }

        public List<BookingDetailDto> Details { get; set; } = new();

        public List<BookingServiceDto> Services { get; set; } = new();

        public BookingDepositDto? Deposit { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    public class BookingDetailDto
    {
        public int BookingDetailId { get; set; }

        public int FieldId { get; set; }

        public string? FieldName { get; set; }

        public string? FieldType { get; set; }

        public DateOnly SlotDate { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public decimal Price { get; set; }
    }

    public class BookingServiceDto
    {
        public int ServiceId { get; set; }

        public string? ServiceName { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Total { get; set; }
    }

    public class BookingDepositDto
    {
        public int DepositId { get; set; }

        public int BookingId { get; set; }

        public decimal? RequiredAmount { get; set; }

        public decimal? PaidAmount { get; set; }

        public string? Status { get; set; }

        public int StatusId { get; set; }

        public DateTime? DeadlineAt { get; set; }

        public int? MinutesLeft { get; set; }

        public DateTime? PaidAt { get; set; }
    }

    public class BookingSummaryDto
    {
        public int BookingId { get; set; }

        public string? CustomerName { get; set; }

        public string? CustomerPhone { get; set; }

        public string? Status { get; set; }

        public int StatusId { get; set; }

        public decimal? TotalAmount { get; set; }

        public int SlotCount { get; set; }

        public DateOnly? EarliestSlotDate { get; set; }

        public TimeOnly? EarliestSlotTime { get; set; }

        public string? FieldName { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
