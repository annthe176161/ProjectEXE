using ProjectEXE.Models;
using ProjectEXE.ViewModel;
using ProjectEXE.ViewModel.ProductViewModel;

namespace ProjectEXE.Services.Interfaces
{

    public interface IProductService
    {
        public Task<List<Product>> GetProducts(int page = 1, int limit = 10);
        public Task<List<ProductsViewModel>> GetProductsWithSearch(string search, int page = 1, int limit = 10);
        public Task<int> getTotalPages(int limit = 10);
        public Task<int> getTotalPagesWithSearch(string search, int limit = 10);
        public Task<bool> deleteProductById(int id);
        public Task<List<ShopsViewModel>> GetShops();
        public Task<List<CategoryViewModel>> GetCategories();
        public Task<ProductsViewModel> GetProductById(int id);
        Task<bool> editProduct(Product product);

        Task<ProductListViewModel> GetProductListAsync(ProductFilterViewModel filter, int page = 1, int pageSize = 12);
        Task<ProductViewModels> GetProductByIdAsync(int id);
        Task<List<CategoryViewModel>> GetAllCategoriesAsync();
        Task<List<CategoryHierarchyViewModel>> GetCategoriesHierarchyAsync();
        Task<List<int>> GetAllChildCategoryIdsAsync(List<int> parentCategoryIds);
        Task<List<ConditionViewModel>> GetAllConditionsAsync();
        // Product Detail
        Task<ProductDetailViewModel> GetProductDetailByIdAsync(int id);
        Task<List<ProductViewModels>> GetRelatedProductsAsync(int productId, string category, int count = 4);
        Task<bool> MarkProductAsSoldAsync(int productId);
    }
}

