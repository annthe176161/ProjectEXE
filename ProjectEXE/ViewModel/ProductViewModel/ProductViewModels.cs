namespace ProjectEXE.ViewModel.ProductViewModel
{
    public class ProductViewModels
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string MainImageUrl { get; set; }
        public bool IsInStock { get; set; }
        public string Condition { get; set; }
        public string Category { get; set; }
        public ShopBasicInfoViewModel Shop { get; set; }
        public bool IsNew { get; set; }
        public bool IsBestSeller { get; set; }
    }
}
