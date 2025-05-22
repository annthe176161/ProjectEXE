namespace ProjectEXE.ViewModel.ProductViewModel
{
    public class ShopDetailViewModel
    {
        public int ShopId { get; set; }
        public string ShopName { get; set; }
        public string Description { get; set; }
        public string ProfileImage { get; set; }
        public string ContactInfo { get; set; }
        public string SellerName { get; set; }
        public string Location { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProductCount { get; set; }
        public bool IsPremium { get; set; }

        // Thông tin phân trang sản phẩm
        public List<ProductViewModels> Products { get; set; } = new List<ProductViewModels>();
        public PaginationViewModel Pagination { get; set; } = new PaginationViewModel();
    }
}
