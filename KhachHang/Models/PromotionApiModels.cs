namespace KhachHang.Models
{
    public class PromotionDto
    {
        public int PromotionId { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        public int TypeId { get; set; }
        public string? TypeName { get; set; }

        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscount { get; set; }
        public decimal? MinOrderAmount { get; set; }

        public int? UsageLimit { get; set; }
        public int? UsedCount { get; set; }

        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        public bool IsActive { get; set; }
    }
}