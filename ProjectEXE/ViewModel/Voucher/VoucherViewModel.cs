using System.ComponentModel.DataAnnotations;

namespace ProjectEXE.ViewModel.Voucher
{
    public class VoucherViewModel
    {
        public string VoucherId { get; set; }

        [Required(ErrorMessage = "Mã voucher không được để trống")]
        [Display(Name = "Mã voucher")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Giảm giá không được để trống")]
        [Range(1, 100, ErrorMessage = "Giảm giá phải từ 1% đến 100%")]
        [Display(Name = "Giảm giá (%)")]
        public int Discount { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateOnly CreateAt { get; set; }

        [Display(Name = "Ngày hết hạn")]
        public DateOnly? ExpiryDate { get; set; }

        [Display(Name = "Giá trị đơn hàng tối thiểu")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị đơn hàng tối thiểu phải lớn hơn hoặc bằng 0")]
        public decimal? MinOrderValue { get; set; }

        [Display(Name = "Giảm giá tối đa")]
        [Range(0, double.MaxValue, ErrorMessage = "Giảm giá tối đa phải lớn hơn hoặc bằng 0")]
        public decimal? MaxDiscountAmount { get; set; }

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        [Display(Name = "Trạng thái kích hoạt")]
        public bool IsActive { get; set; }
    }
}
