using ProjectEXE.Models;
using ProjectEXE.ViewModel.ProductViewModel;
using ProjectEXE.ViewModel.ShopViewModel;
using System.Security.Claims;

namespace ProjectEXE.Services.Interfaces
{
    public interface IShopService
    {
        Task<bool> ActiveShop(ShopView shop, string imageUrl);
        Task<int> CreatePackageSubscription(int shopId, int packageId);
        Task<int> GetShopIdByUserId(int userId);
        Task ActivePackagePayment(PackagePayment packagePayment);
        Task<bool> IsShopPremiumAsync(int shopId);
        Task<ShopBasicInfoViewModel> GetShopBasicInfoAsync(int shopId);
        Task<ShopDetailViewModel> GetShopDetailAsync(int shopId);
        // Thêm phương thức mới để lấy chi tiết shop và sản phẩm
        Task<ShopDetailViewModel> GetShopWithProductsAsync(int shopId, int page = 1, int pageSize = 6);

        Task<Shop> GetShopByUserIdAsync(int userId);
        Task<bool> CreateShopAsync(CreateShopViewModel model, int userId);
        Task<bool> UpdateShopAsync(CreateShopViewModel model, int shopId);
        Task<bool> HasShopAsync(int userId);
        Task<bool> CheckExpiryDate(int shopId);
        Task<bool> CanAddProductAsync(int shopId);
        Task<List<CategoryViewModel>> GetCategoriesAsync();
        Task<List<ConditionViewModel>> GetConditionsAsync();
        Task<bool> CreateProductAsync(ViewModel.Shop.ProductFormViewModel model, int shopId);
    }
}
