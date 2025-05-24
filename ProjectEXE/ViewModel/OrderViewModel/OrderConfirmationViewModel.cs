using System.ComponentModel.DataAnnotations;

namespace ProjectEXE.ViewModel.OrderViewModel
{
    public class OrderConfirmationViewModel
    {
        public ProductSummaryViewModel Product { get; set; } = null!;
        public ShopSummaryViewModel Shop { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 100 ký tự")]
        [VietnameseName]
        [Display(Name = "Họ và tên")]
        public string BuyerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [VietnamesePhone]
        [Display(Name = "Số điện thoại")]
        public string BuyerPhone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        [Display(Name = "Email")]
        public string? BuyerEmail { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Địa chỉ phải từ 10 đến 500 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string BuyerAddress { get; set; } = string.Empty;

        public int ProductId { get; set; }
        public int BuyerId { get; set; }
        public int SellerId { get; set; }
    }
}
