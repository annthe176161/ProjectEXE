using System.ComponentModel.DataAnnotations;

namespace ProjectEXE.ViewModel.ShopOrderViewModel
{
    public class UpdateOrderStatusViewModel
    {
        [Required(ErrorMessage = "OrderId là bắt buộc")]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Trạng thái đơn hàng là bắt buộc")]
        [Range(1, 5, ErrorMessage = "Trạng thái không hợp lệ")]
        public int StatusId { get; set; }

        // Bỏ trường Notes vì không có trong database

        [StringLength(500, ErrorMessage = "Lý do hủy không được vượt quá 500 ký tự")]
        public string? CancelReason { get; set; }
    }
}
