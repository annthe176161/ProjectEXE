using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectEXE.Models;
using ProjectEXE.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectEXE.Services.Interfaces
{
    public interface IShopServiceOfAdmin
    {
        // Methods gốc
        Task<Shop> GetShopByUserIdAsync(int userId);
        Task<IEnumerable<ShopProductViewModel>> GetProductsByShopIdAsync(int shopId);
        Task<Product> GetProductByIdForEditAsync(int productId, int shopId);
        Task<bool> AddProductAsync(ProductEditViewModel model, int shopId);
        Task<bool> UpdateProductAsync(ProductEditViewModel model, int shopId);
        Task<(bool Success, string ErrorMessage)> DeleteProductAsync(int productId, int shopId);
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<IEnumerable<ProductCondition>> GetAllProductConditionsAsync();

        // Thêm methods debug và testing
        Task<bool> TestDatabaseConnectionAsync();
        Task<Shop> GetShopByUserIdWithDetailedLoggingAsync(int userId);
    }
}