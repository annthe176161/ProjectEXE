using Microsoft.EntityFrameworkCore;
using ProjectEXE.Models;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ProjectEXE.Services.Implementations
{
    public class ShopServiceOfAdmin : IShopServiceOfAdmin
    {
        private readonly RevaContext _context;
        private readonly ILogger<ShopServiceOfAdmin> _logger;

        public ShopServiceOfAdmin(RevaContext context, ILogger<ShopServiceOfAdmin> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Shop> GetShopByUserIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation("GetShopByUserIdAsync called with userId: {UserId}", userId);

                if (_context.Shops == null)
                {
                    _logger.LogWarning("_context.Shops is null");
                    return null;
                }

                // Đảm bảo parameter được truyền đúng cách
                _logger.LogInformation("Querying database for shop with UserId: {UserId}", userId);

                var shop = await _context.Shops
                    .Where(s => s.UserId == userId)
                    .FirstOrDefaultAsync();

                if (shop != null)
                {
                    _logger.LogInformation("Found shop: ShopId={ShopId}, UserId={UserId}, ShopName={ShopName}",
                        shop.ShopId, shop.UserId, shop.ShopName);
                }
                else
                {
                    _logger.LogWarning("No shop found for UserId: {UserId}", userId);
                }

                return shop;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetShopByUserIdAsync for userId: {UserId}", userId);
                return null;
            }
        }

        public async Task<IEnumerable<ShopProductViewModel>> GetProductsByShopIdAsync(int shopId)
        {
            try
            {
                _logger.LogInformation("GetProductsByShopIdAsync called with shopId: {ShopId}", shopId);

                if (_context.Products == null)
                {
                    _logger.LogWarning("_context.Products is null");
                    return Enumerable.Empty<ShopProductViewModel>();
                }

                var products = await _context.Products
                    .Where(p => p.ShopId == shopId)
                    .Include(p => p.Category)
                    .Include(p => p.Condition)
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new ShopProductViewModel
                    {
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                        Description = p.Description,
                        Price = p.Price,
                        CategoryName = p.Category != null ? p.Category.CategoryName : "N/A",
                        Size = p.Size,
                        ConditionName = p.Condition != null ? p.Condition.ConditionName : "N/A",
                        Brand = p.Brand,
                        Color = p.Color,
                        Material = p.Material,
                        CreatedAt = p.CreatedAt,
                        IsVisible = p.IsVisible
                    })
                    .ToListAsync();

                _logger.LogInformation("Found {Count} products for shopId: {ShopId}", products.Count(), shopId);
                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetProductsByShopIdAsync for shopId: {ShopId}", shopId);
                return Enumerable.Empty<ShopProductViewModel>();
            }
        }

        public async Task<Product> GetProductByIdForEditAsync(int productId, int shopId)
        {
            try
            {
                _logger.LogInformation("GetProductByIdForEditAsync called with productId: {ProductId}, shopId: {ShopId}", productId, shopId);

                if (_context.Products == null)
                {
                    _logger.LogWarning("_context.Products is null");
                    return null;
                }

                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == productId && p.ShopId == shopId);

                if (product != null)
                {
                    _logger.LogInformation("Found product: {ProductName} for edit", product.ProductName);
                }
                else
                {
                    _logger.LogWarning("No product found with ProductId: {ProductId} and ShopId: {ShopId}", productId, shopId);
                }

                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetProductByIdForEditAsync for productId: {ProductId}, shopId: {ShopId}", productId, shopId);
                return null;
            }
        }

        public async Task<bool> AddProductAsync(ProductEditViewModel model, int shopId)
        {
            try
            {
                _logger.LogInformation("AddProductAsync called for shopId: {ShopId}, ProductName: {ProductName}", shopId, model?.ProductName);

                if (model == null || _context.Products == null)
                {
                    _logger.LogWarning("Model is null or _context.Products is null");
                    return false;
                }

                var product = new Product
                {
                    ProductName = model.ProductName,
                    Description = model.Description,
                    Price = model.Price,
                    CategoryId = model.CategoryId,
                    ConditionId = model.ConditionId,
                    Gender = model.Gender,
                    Size = model.Size,
                    Color = model.Color,
                    Brand = model.Brand,
                    Material = model.Material,
                    IsInStock = model.IsInStock,
                    IsVisible = model.IsVisible,
                    ShopId = shopId,
                    CreatedAt = DateTime.Now
                };

                _context.Products.Add(product);
                var result = await _context.SaveChangesAsync();

                _logger.LogInformation("Product added successfully. Affected rows: {Result}", result);
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product for shopId: {ShopId}", shopId);
                return false;
            }
        }

        public async Task<bool> UpdateProductAsync(ProductEditViewModel model, int shopId)
        {
            try
            {
                _logger.LogInformation("UpdateProductAsync called for productId: {ProductId}, shopId: {ShopId}", model?.ProductId, shopId);

                if (model == null || _context.Products == null)
                {
                    _logger.LogWarning("Model is null or _context.Products is null");
                    return false;
                }

                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == model.ProductId && p.ShopId == shopId);

                if (product == null)
                {
                    _logger.LogWarning("Product not found for ProductId: {ProductId}, ShopId: {ShopId}", model.ProductId, shopId);
                    return false;
                }

                product.ProductName = model.ProductName;
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;
                product.ConditionId = model.ConditionId;
                product.Gender = model.Gender;
                product.Size = model.Size;
                product.Color = model.Color;
                product.Brand = model.Brand;
                product.Material = model.Material;
                product.IsInStock = model.IsInStock;
                product.IsVisible = model.IsVisible;

                var result = await _context.SaveChangesAsync();

                _logger.LogInformation("Product updated successfully. Affected rows: {Result}", result);
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product for productId: {ProductId}, shopId: {ShopId}", model?.ProductId, shopId);
                return false;
            }
        }

        public async Task<(bool Success, string ErrorMessage)> DeleteProductAsync(int productId, int shopId)
        {
            try
            {
                _logger.LogInformation("DeleteProductAsync called for productId: {ProductId}, shopId: {ShopId}", productId, shopId);

                if (_context.Products == null)
                {
                    _logger.LogWarning("_context.Products is null");
                    return (false, "Không thể kết nối cơ sở dữ liệu sản phẩm.");
                }

                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == productId && p.ShopId == shopId);

                if (product == null)
                {
                    _logger.LogWarning("Product not found for deletion. ProductId: {ProductId}, ShopId: {ShopId}", productId, shopId);
                    return (false, "Không tìm thấy sản phẩm hoặc bạn không có quyền xóa sản phẩm này.");
                }

                _context.Products.Remove(product);
                var result = await _context.SaveChangesAsync();

                _logger.LogInformation("Product deleted successfully. Affected rows: {Result}", result);
                return (true, "Xóa sản phẩm thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product for productId: {ProductId}, shopId: {ShopId}", productId, shopId);
                return (false, "Đã có lỗi xảy ra khi xóa sản phẩm.");
            }
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            try
            {
                _logger.LogInformation("GetAllCategoriesAsync called");

                if (_context.Categories == null)
                {
                    _logger.LogWarning("_context.Categories is null");
                    return Enumerable.Empty<Category>();
                }

                var categories = await _context.Categories
                    .OrderBy(c => c.CategoryName)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} categories", categories.Count);
                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllCategoriesAsync");
                return Enumerable.Empty<Category>();
            }
        }

        public async Task<IEnumerable<ProductCondition>> GetAllProductConditionsAsync()
        {
            try
            {
                _logger.LogInformation("GetAllProductConditionsAsync called");

                if (_context.ProductConditions == null)
                {
                    _logger.LogWarning("_context.ProductConditions is null");
                    return Enumerable.Empty<ProductCondition>();
                }

                var conditions = await _context.ProductConditions
                    .OrderBy(pc => pc.ConditionName)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} product conditions", conditions.Count);
                return conditions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllProductConditionsAsync");
                return Enumerable.Empty<ProductCondition>();
            }
        }

        // Thêm method để test trực tiếp database connection
        public async Task<bool> TestDatabaseConnectionAsync()
        {
            try
            {
                _logger.LogInformation("Testing database connection");
                var count = await _context.Shops.CountAsync();
                _logger.LogInformation("Database connection successful. Total shops: {Count}", count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection test failed");
                return false;
            }
        }

        // Thêm method để debug shop query cụ thể
        public async Task<Shop> GetShopByUserIdWithDetailedLoggingAsync(int userId)
        {
            try
            {
                _logger.LogInformation("=== DETAILED SHOP QUERY DEBUG ===");
                _logger.LogInformation("Input userId: {UserId} (Type: {Type})", userId, userId.GetType().Name);

                // Test database connection
                var shopCount = await _context.Shops.CountAsync();
                _logger.LogInformation("Total shops in database: {Count}", shopCount);

                // List some shops for debugging
                var allShops = await _context.Shops.Take(5).ToListAsync();
                foreach (var s in allShops)
                {
                    _logger.LogInformation("Sample shop: ShopId={ShopId}, UserId={UserId}, ShopName={ShopName}",
                        s.ShopId, s.UserId, s.ShopName);
                }

                // Execute the actual query
                var shop = await _context.Shops
                    .Where(s => s.UserId == userId)
                    .FirstOrDefaultAsync();

                _logger.LogInformation("Query result: {Result}", shop != null ? $"Found shop {shop.ShopId}" : "No shop found");
                _logger.LogInformation("=== END DETAILED DEBUG ===");

                return shop;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in detailed shop query for userId: {UserId}", userId);
                return null;
            }
        }
    }
}