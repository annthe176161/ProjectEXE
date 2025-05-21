namespace ProjectEXE.ViewModel.ShopOrderViewModel
{
    public class UpdateOrderStatusViewModel
    {
        public int OrderId { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Trạng thái đơn hàng là bắt buộc")]
        public int StatusId { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(200, ErrorMessage = "Ghi chú không được vượt quá 200 ký tự")]
        public string Notes { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(255, ErrorMessage = "Lý do hủy không được vượt quá 255 ký tự")]
        public string CancelReason { get; set; }
    }
}
