namespace ProjectEXE.ViewModel.OrderViewModel
{
    public class ProductSummaryViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string MainImageUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Brand { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string Condition { get; set; } = string.Empty;
    }
}
