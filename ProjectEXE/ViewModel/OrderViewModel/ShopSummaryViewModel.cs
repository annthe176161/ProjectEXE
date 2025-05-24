namespace ProjectEXE.ViewModel.OrderViewModel
{
    public class ShopSummaryViewModel
    {
        public int ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string? ProfileImage { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProductCount { get; set; }
        public bool IsPremium { get; set; }
    }
}
