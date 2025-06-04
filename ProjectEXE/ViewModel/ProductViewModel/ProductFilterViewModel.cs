namespace ProjectEXE.ViewModel.ProductViewModel
{
    public class ProductFilterViewModel
    {
        public List<CategoryHierarchyViewModel> Categories { get; set; } = new List<CategoryHierarchyViewModel>();
        public List<int> SelectedCategoryIds { get; set; } = new List<int>();
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<ConditionViewModel> Conditions { get; set; } = new List<ConditionViewModel>();
        public List<int> SelectedConditionIds { get; set; } = new List<int>();
        public string SortBy { get; set; } = "newest";
        public bool ShowPremiumOnly { get; set; } = false;
        public string? Gender { get; set; }
    }
}
