using System.ComponentModel.DataAnnotations;

namespace ProjectEXE.ViewModel.ShopViewModel
{
    public class CreateShopViewModel
    {
        [Required(ErrorMessage = "Tên gian hàng là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên gian hàng không được vượt quá 100 ký tự")]
        [Display(Name = "Tên gian hàng")]
        public string ShopName { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        [Display(Name = "Mô tả gian hàng")]
        public string Description { get; set; }

        [Display(Name = "Ảnh đại diện gian hàng")]
        public IFormFile ProfileImage { get; set; }

        [StringLength(500, ErrorMessage = "Thông tin liên hệ không được vượt quá 500 ký tự")]
        [Display(Name = "Thông tin liên hệ")]
        public string ContactInfo { get; set; }
    }
}
