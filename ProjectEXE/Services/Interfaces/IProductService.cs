using ProjectEXE.ViewModel.ProductViewModel;

namespace ProjectEXE.Services.Interfaces
{
    public interface IProductService
    {
        // Product List
        Task<ProductListViewModel> GetProductListAsync(ProductFilterViewModel filter, int page = 1, int pageSize = 12);
        Task<ProductViewModels> GetProductByIdAsync(int id);
        Task<List<CategoryViewModel>> GetAllCategoriesAsync();
        Task<List<ConditionViewModel>> GetAllConditionsAsync();
        // Product Detail
        Task<ProductDetailViewModel> GetProductDetailByIdAsync(int id);
        Task<List<ProductViewModels>> GetRelatedProductsAsync(int productId, string category, int count = 4);
    }
}
