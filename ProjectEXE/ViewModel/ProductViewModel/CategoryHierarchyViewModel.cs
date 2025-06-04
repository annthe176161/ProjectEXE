namespace ProjectEXE.ViewModel.ProductViewModel
{
    public class CategoryHierarchyViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public int? ParentCategoryId { get; set; }
        public int Level { get; set; } // Độ sâu của category (0 = root, 1 = level 1, etc.)
        public List<CategoryHierarchyViewModel> Children { get; set; } = new List<CategoryHierarchyViewModel>();
        public bool IsSelected { get; set; } // Để đánh dấu category đã được chọn
    }
}
