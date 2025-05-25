using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
// using Microsoft.AspNetCore.Http; // Nếu xử lý upload file

namespace ProjectEXE.ViewModel
{
    public class ProductEditViewModel
    {
        public int ProductId { get; set; } // Sẽ là 0 nếu là tạo mới

        [Required(ErrorMessage = "Tên sản phẩm không được để trống.")]
        [StringLength(200, ErrorMessage = "Tên sản phẩm không quá 200 ký tự.")]
        [Display(Name = "Tên sản phẩm")]
        public string ProductName { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        [StringLength(1000, ErrorMessage = "Mô tả không quá 1000 ký tự.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá không được để trống.")]
        [Range(1000, 999999999, ErrorMessage = "Giá phải từ 1,000 VNĐ đến 999,999,999 VNĐ.")]
        [Display(Name = "Giá (VNĐ)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }
        public IEnumerable<SelectListItem> CategoriesList { get; set; } = new List<SelectListItem>();

        [Required(ErrorMessage = "Vui lòng chọn tình trạng sản phẩm.")]
        [Display(Name = "Tình trạng sản phẩm")]
        public int ConditionId { get; set; }
        public IEnumerable<SelectListItem> ConditionsList { get; set; } = new List<SelectListItem>();

        [StringLength(20, ErrorMessage = "Giới tính không quá 20 ký tự.")]
        [Display(Name = "Giới tính")]
        public string Gender { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Kích thước không quá 50 ký tự.")]
        [Display(Name = "Kích thước")]
        public string Size { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Màu sắc không quá 50 ký tự.")]
        [Display(Name = "Màu sắc")]
        public string Color { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Thương hiệu không quá 100 ký tự.")]
        [Display(Name = "Thương hiệu")]
        public string Brand { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Chất liệu không quá 100 ký tự.")]
        [Display(Name = "Chất liệu")]
        public string Material { get; set; } = string.Empty;

        [Display(Name = "Còn hàng")]
        public bool IsInStock { get; set; } = true;

        [Display(Name = "Hiển thị sản phẩm")]
        public bool IsVisible { get; set; } = true;

        // Computed property để kiểm tra xem có đang tạo mới không
        public bool IsNew => ProductId == 0;

        // Computed property để hiển thị tiêu đề form
        public string FormTitle => IsNew ? "Thêm sản phẩm mới" : "Chỉnh sửa sản phẩm";

        // Các thuộc tính cho upload hình ảnh (sẽ xem xét ở bước nâng cao hơn)
        // public IFormFile MainImageFile { get; set; }
        // public List<IFormFile> OtherImageFiles { get; set; }
        // public List<string> ExistingImageUrls { get; set; }

        public ProductEditViewModel()
        {
            CategoriesList = new List<SelectListItem>();
            ConditionsList = new List<SelectListItem>();
            // ExistingImageUrls = new List<string>();
            // OtherImageFiles = new List<IFormFile>();
        }
    }
}