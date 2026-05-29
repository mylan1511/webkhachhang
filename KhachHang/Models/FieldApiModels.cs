namespace KhachHang.Models
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    public class FieldDto
    {
        public int FieldId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public decimal PeakPrice { get; set; }
        public string? ImageUrl { get; set; }
        public string? FieldType { get; set; }
        public int TypeId { get; set; }
        public string? Status { get; set; }
        public int StatusId { get; set; }
        public double? AvgRating { get; set; }
        public int? TotalReviews { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class FieldScheduleDto
    {
        public int FieldId { get; set; }
        public string? FieldName { get; set; }
        public string? FieldType { get; set; }
        public string? ImageUrl { get; set; }
        public DateOnly SlotDate { get; set; }
        public List<FieldSlotDto> Slots { get; set; } = new();
    }

    public class FieldSlotDto
    {
        public int FieldSlotId { get; set; }

        public int SlotId { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public decimal Price { get; set; }

        public bool IsPeakHour { get; set; }

        public string? Status { get; set; }

        public int StatusId { get; set; }

        public int? HoldRemainingSeconds { get; set; }
    }
}