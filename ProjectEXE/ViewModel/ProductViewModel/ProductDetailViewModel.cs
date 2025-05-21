namespace ProjectEXE.ViewModel.ProductViewModel
{
    public class ProductDetailViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        public string MainImageUrl => ImageUrls.FirstOrDefault() ?? "/images/placeholder.png";
        public bool IsInStock { get; set; }
        public string Condition { get; set; }
        public string ConditionDescription { get; set; }
        public string Category { get; set; }
        public string CategoryId { get; set; }
        public string Gender { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public string Brand { get; set; }
        public string Material { get; set; }
        public ShopDetailViewModel Shop { get; set; }
        public List<ProductViewModels> RelatedProducts { get; set; } = new List<ProductViewModels>();
    }
}
