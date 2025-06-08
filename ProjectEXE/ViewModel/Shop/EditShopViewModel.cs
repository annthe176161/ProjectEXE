using System.ComponentModel.DataAnnotations;

namespace ProjectEXE.ViewModel.Shop
{
    public class EditShopViewModel
    {
        public int ShopId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên gian hàng")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên gian hàng phải từ 3-100 ký tự")]
        [Display(Name = "Tên gian hàng")]
        public string ShopName { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không vượt quá 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Hình ảnh đại diện")]
        public IFormFile ProfileImageFile { get; set; }

        public string ProfileImage { get; set; }

        [StringLength(500, ErrorMessage = "Thông tin liên hệ không vượt quá 500 ký tự")]
        [Display(Name = "Thông tin liên hệ")]
        public string ContactInfo { get; set; }
    }
}
