using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectEXE.ViewModel
{
    public class CategoryOfAdminViewModel
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự.")]
        [Display(Name = "Tên danh mục")]
        public string CategoryName { get; set; }

        [Display(Name = "Danh mục cha")]
        public int? ParentCategoryId { get; set; } // Nullable vì có thể là danh mục gốc

        // Dùng để hiển thị tên danh mục cha trong danh sách
        public string ParentCategoryName { get; set; }

        // Dùng để tạo dropdown danh sách danh mục cha trong form Add/Edit
        public IEnumerable<SelectListItem> ParentCategoryList { get; set; }

        public CategoryOfAdminViewModel()
        {
            ParentCategoryList = new List<SelectListItem>();
        }
    }
}