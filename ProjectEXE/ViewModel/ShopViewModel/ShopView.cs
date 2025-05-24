using System.ComponentModel.DataAnnotations;

namespace ProjectEXE.ViewModel.ShopViewModel
{
    public class ShopView
    {
        public int UserId { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 0, ErrorMessage = "Độ dài phải từ 0 đến 100 ký tự.")]
        public string ShopName { get; set; }

        [StringLength(500, MinimumLength = 0, ErrorMessage = "Độ dài tối đa là 500 ký tự.")]
        public string? Description { get; set; }

        public string? ProfileImage { get; set; }
        public IFormFile? ImagePath { get; set; }

        public string? ContactInfo { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
