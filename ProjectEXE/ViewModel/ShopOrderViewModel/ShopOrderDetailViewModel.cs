using ProjectEXE.ViewModel.OrderViewModel;

namespace ProjectEXE.ViewModel.ShopOrderViewModel
{
    public class ShopOrderDetailViewModel
    {
        public int OrderId { get; set; }

        // Thông tin sản phẩm
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImageUrl { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public string Condition { get; set; }

        // Thông tin người mua
        public int BuyerId { get; set; }
        public string BuyerName { get; set; }
        public string BuyerPhone { get; set; }
        public string BuyerEmail { get; set; }
        public string BuyerAddress { get; set; }

        // Thông tin đơn hàng
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public string StatusClass { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CancelReason { get; set; }

        // Lịch sử trạng thái (nếu bạn có bảng theo dõi lịch sử)
        public List<OrderStatusHistoryViewModel> StatusHistory { get; set; } = new List<OrderStatusHistoryViewModel>();
    }
}
