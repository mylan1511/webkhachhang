namespace KhachHang.Models.ViewModels
{
    public class SanFilterVM
    {
        public string? Search { get; set; }

        public int? TypeId { get; set; }

        public int? StatusId { get; set; }

        public decimal? GiaTu { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 9;
    }
}