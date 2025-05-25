using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectEXE.ViewModel
{
    public class ShopProductViewModel
    {
        public int ProductId { get; set; }

        [Display(Name = "Tên sản phẩm")]
        public string ProductName { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Giá")]
        [DisplayFormat(DataFormatString = "{0:N0} VNĐ")]
        public decimal Price { get; set; }

        [Display(Name = "Danh mục")]
        public string CategoryName { get; set; } = "N/A";

        [Display(Name = "Kích thước")]
        public string Size { get; set; } = string.Empty;

        [Display(Name = "Tình trạng")]
        public string ConditionName { get; set; } = "N/A";

        [Display(Name = "Thương hiệu")]
        public string Brand { get; set; } = string.Empty;

        [Display(Name = "Màu sắc")]
        public string Color { get; set; } = string.Empty;

        [Display(Name = "Chất liệu")]
        public string Material { get; set; } = string.Empty;

        [Display(Name = "Ngày tạo")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Hiển thị")]
        public bool IsVisible { get; set; }

        [Display(Name = "Còn hàng")]
        public bool IsInStock { get; set; }

        // Computed properties for better display
        [Display(Name = "Trạng thái")]
        public string Status => IsVisible ? (IsInStock ? "Đang bán" : "Hết hàng") : "Ẩn";

        [Display(Name = "Mô tả ngắn")]
        public string ShortDescription => Description?.Length > 100
            ? Description.Substring(0, 100) + "..."
            : Description ?? string.Empty;

        [Display(Name = "Giá hiển thị")]
        public string FormattedPrice => $"{Price:N0} VNĐ";

        // Optional: thêm ảnh đại diện
        // public string MainImageUrl { get; set; } = string.Empty;

        // Optional: thêm số lượng đã bán
        // public int SoldCount { get; set; }

        // Optional: thêm đánh giá trung bình
        // public decimal AverageRating { get; set; }
    }
}