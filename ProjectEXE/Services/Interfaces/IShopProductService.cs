using ProjectEXE.ViewModel.ProductViewModel;
using ProjectEXE.ViewModel.Shop;

namespace ProjectEXE.Services.Interfaces
{
    public interface IShopProductService
    {
        Task<bool> CanCreateProductAsync(int shopId);
        Task<ProductFormViewModel> GetProductFormDataAsync(int? productId, int shopId);
        Task<bool> CreateProductAsync(ProductFormViewModel model, int shopId);
        Task<bool> UpdateProductAsync(ProductFormViewModel model, int shopId);
        Task<bool> DeleteProductAsync(int productId, int shopId);
        Task<List<CategoryViewModel>> GetCategoriesAsync();
        Task<List<ConditionViewModel>> GetConditionsAsync();
        bool IsShopBuyPackage(int userId);
    }
}
