using System.ComponentModel.DataAnnotations;

namespace ProjectEXE.ViewModel.ShopOrderViewModel
{
    public class ShopOrderItemViewModel
    {
        public int OrderId { get; set; }

        [Display(Name = "Sản phẩm")]
        public string ProductName { get; set; }
        public int ProductId { get; set; }
        public string ProductImageUrl { get; set; }
        public decimal Price { get; set; }

        [Display(Name = "Khách hàng")]
        public string BuyerName { get; set; }
        public int BuyerId { get; set; }
        public string BuyerPhone { get; set; }
        public string BuyerAddress { get; set; }
        public string BuyerEmail { get; set; }

        [Display(Name = "Trạng thái")]
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public string StatusClass { get; set; } // CSS class cho từng trạng thái

        [Display(Name = "Ngày đặt hàng")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Cập nhật lần cuối")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "Lý do hủy")]
        public string CancelReason { get; set; }
    }
}
