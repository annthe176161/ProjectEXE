﻿using ProjectEXE.Models;
using ProjectEXE.ViewModel;
using ProjectEXE.ViewModel.ProductViewModel;
using ProjectEXE.ViewModel.Shop;
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
        Task<bool> UpdateShopAsync(CreateShopViewModel model, int shopId);
        Task<bool> CheckExpiryDate(int shopId);
        Task<bool> CanAddProductAsync(int shopId);
        Task<List<CategoryViewModel>> GetCategoriesAsync();
        Task<List<ConditionViewModel>> GetConditionsAsync();
        Task<bool> CreateProductAsync(ProductFormViewModel model, int shopId);
        Task<ProductFormViewModel> GetProductFormDataAsync(int? productId, int shopId);
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Product> GetProductByIdForEditAsync(int productId, int shopId);
        Task<IEnumerable<ProductCondition>> GetAllProductConditionsAsync();
        Task<bool> UpdateProductAsync(ProductEditViewModel model, int shopId);
        Task<(bool Success, string ErrorMessage)> DeleteProductAsync(int productId, int shopId);
        Task<IEnumerable<ShopProductViewModel>> GetProductsByShopIdAsync(int shopId);

        //tuan anh code phan thong ke 
        // Thêm vào interface IShopService (nếu chưa có)
        Task<int> GetTotalProductsByShopIdAsync(int shopId);
        Task<int> GetTotalOrdersByShopIdAsync(int shopId);


        // Doanh thu theo tháng
        Task<decimal> GetMonthlyRevenueByShopIdAsync(int shopId, int year, int month); 
        Task<decimal> GetCurrentMonthRevenueByShopIdAsync(int shopId);

        // Chinh sua shop
        Task<Shop> GetShopDetailByIdAsync(int shopId);
        Task<EditShopViewModel> GetShopForEditAsync(int shopId);
        Task<bool> UpdateShopAsync(EditShopViewModel model, int shopId);
    }
}
