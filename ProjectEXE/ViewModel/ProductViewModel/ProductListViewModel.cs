namespace ProjectEXE.ViewModel.ProductViewModel
{
    public class ProductListViewModel
    {
        public List<ProductViewModels> Products { get; set; } = new List<ProductViewModels>();
        public ProductFilterViewModel Filter { get; set; } = new ProductFilterViewModel();
        public PaginationViewModel Pagination { get; set; } = new PaginationViewModel();
    }
}
