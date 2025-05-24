
using ProjectEXE.Models;
using ProjectEXE.ViewModel;

namespace ProjectEXE.Services.Interfaces
{

    public interface IProductService
    {
        public Task<List<Product>> GetProducts(int page = 1, int limit = 10);
        public Task<List<ProductViewModel>> GetProductsWithSearch(string search, int page = 1, int limit = 10);
        public Task<int> getTotalPages(int limit = 10);
        public Task<int> getTotalPagesWithSearch(string search, int limit = 10);
        public Task<bool> deleteProductById(int id);
        public Task<List<ShopViewModel>> GetShops();
        public Task<List<CategoryViewModel>> GetCategories();
        public Task<ProductViewModel> GetProductById(int id);
        Task<bool> editProduct(Product product);
    }
}