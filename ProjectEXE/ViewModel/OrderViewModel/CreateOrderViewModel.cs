namespace ProjectEXE.ViewModel.OrderViewModel
{
    public class CreateOrderViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public string ProductImageUrl { get; set; }
        public int SellerId { get; set; }
        public string SellerName { get; set; }
        public string ShopName { get; set; }
    }
}
