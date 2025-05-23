namespace ProjectEXE.ViewModel.OrderViewModel
{
    public class CreateOrderRequestViewModel
    {
        public int ProductId { get; set; }
        public int BuyerId { get; set; }
        public int SellerId { get; set; }

        // Thông tin liên hệ từ User profile
        public string BuyerName { get; set; } = string.Empty;
        public string BuyerPhone { get; set; } = string.Empty;
        public string? BuyerEmail { get; set; }
        public string BuyerAddress { get; set; } = string.Empty;
    }
}
