using ProjectEXE.ViewModel.ProductViewModel;
using System.ComponentModel.DataAnnotations;

namespace ProjectEXE.ViewModel.Shop
{
    public class ProductFormViewModel
    {
        public int? ProductId { get; set; } // Null nếu tạo mới, có giá trị nếu sửa

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(200, ErrorMessage = "Tên sản phẩm không được quá 200 ký tự")]
        [Display(Name = "Tên sản phẩm")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả sản phẩm")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá sản phẩm")]
        [Range(1000, 100000000, ErrorMessage = "Giá phải từ 1.000đ đến 100.000.000đ")]
        [Display(Name = "Giá")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tình trạng sản phẩm")]
        [Display(Name = "Tình trạng")]
        public int ConditionId { get; set; }

        [Display(Name = "Giới tính")]
        public string Gender { get; set; }

        [Display(Name = "Kích thước")]
        public string Size { get; set; }

        [Display(Name = "Màu sắc")]
        public string Color { get; set; }

        [Display(Name = "Thương hiệu")]
        public string Brand { get; set; }

        [Display(Name = "Chất liệu")]
        public string Material { get; set; }

        [Display(Name = "Còn hàng")]
        public bool IsInStock { get; set; } = true;

        [Display(Name = "Hiển thị sản phẩm")]
        public bool IsVisible { get; set; } = true;

        // Hình ảnh hiện tại (để hiển thị khi sửa)
        public List<ProductImageViewModel> ExistingImages { get; set; } = new List<ProductImageViewModel>();

        // Hình ảnh mới được tải lên
        [Display(Name = "Hình ảnh sản phẩm")]
        public List<IFormFile> NewImages { get; set; } = new List<IFormFile>();

        // Danh sách ID hình ảnh cần xóa (khi sửa)
        public List<int> ImageIdsToDelete { get; set; } = new List<int>();

        // ID của hình ảnh chính (dùng khi sửa)
        [Display(Name = "Hình ảnh chính")]
        public int? MainImageId { get; set; }

        // Dropdown options
        public List<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
        public List<ConditionViewModel> Conditions { get; set; } = new List<ConditionViewModel>();
        public List<string> GenderOptions { get; set; } = new List<string> { "Nam", "Nữ", "Unisex", "Trẻ em" };
    }
}
