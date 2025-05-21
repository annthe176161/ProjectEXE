using ProjectEXE.ViewModel.ProductViewModel;

namespace ProjectEXE.ViewModel.ShopOrderViewModel
{
    public class ShopOrderManagementViewModel
    {
        public List<ShopOrderItemViewModel> Orders { get; set; } = new List<ShopOrderItemViewModel>();
        public PaginationViewModel Pagination { get; set; }
        public Dictionary<int, string> StatusOptions { get; set; }
        public string StatusFilter { get; set; }
        public string DateRangeFilter { get; set; }
    }
}
