

using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using ProjectEXE.Models;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel;
using ProjectEXE.ViewModel.ProductViewModel;

namespace ProjectEXE.Services.Implementations
{

    public class ProductService : IProductService
    {
        private readonly RevaContext _context;
        private readonly IShopService _shopService;
        public ProductService(RevaContext context, IShopService shopService)
        {
            _context = context;
            _shopService = shopService;
        }
        public async Task<List<Product>> GetProducts(int page = 1, int limit = 10)
        {
            if (page < 1)
            {
                page = 1;
            }
            int TotalPages = (int)Math.Ceiling(_context.Products.Count() / (double)limit);
            if(TotalPages < page)
            {
                page = TotalPages;
            }
            return await _context.Products.Include(p => p.ProductImages).Include(p => p.Category).Include(p => p.Shop)
            .Skip((page - 1) * limit)
            .Take(limit).ToListAsync();
        }

        public async Task<bool> editProduct(Product p)
        {
            var product = await _context.Products.FindAsync(p.ProductId);
            if (product == null) return false;

            product.ProductName = p.ProductName;
            product.CategoryId = p.CategoryId;
            product.ShopId = p.ShopId;
            product.Price = p.Price;
            product.IsVisible = p.IsVisible;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> getTotalPages(int limit = 10)
        {
            int TotalPages = (int)Math.Ceiling(_context.Products.Count() / (double)limit);
            return TotalPages;
        }

        public async Task<List<ProductsViewModel>> GetProductsWithSearch(string search, int page = 1, int limit = 10)
        {
            if (page < 1)
            {
                page = 1;
            }
            int TotalPages = (int)Math.Ceiling(_context.Products.Count() / (double)limit);
            if (TotalPages < page)
            {
                page = TotalPages;
            }
            return await _context.Products.Include(p => p.ProductImages).Include(p => p.Category).Include(p => p.Shop)
            .Where(p => p.ProductName.Contains(search))
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(p => new ProductsViewModel
            {
                productId = p.ProductId,
                productName = p.ProductName,
                imageUrl = p.ProductImages.FirstOrDefault().ImageUrl,
                category = p.Category.CategoryName,
                shop = p.Shop.ShopName,
                price = p.Price,
                isVisible = p.IsVisible
            })
            .ToListAsync();
        }

        public async Task<List<CategorysViewModel>> GetCategories()
        {
            return await _context.Categories.Select(c => new CategorysViewModel
            {
                categoryId = c.CategoryId,
                categoryName = c.CategoryName,
            }).ToListAsync();
        }

        public async Task<List<ShopsViewModel>> GetShops()
        {
            return await _context.Shops.Select(s => new ShopsViewModel
            {
                shopId = s.ShopId,
                shopName = s.ShopName,
            }).ToListAsync();
        }

        public async Task<ProductsViewModel> GetProductById(int id)
        {
            return await _context.Products.Include(p => p.ProductImages).Include(p => p.Category).Include(p => p.Shop)
            .Where(p => p.ProductId == id)
            .Select(p => new ProductsViewModel
            {
                productId = p.ProductId,
                productName = p.ProductName,
                imageUrl = p.ProductImages.FirstOrDefault().ImageUrl,
                category = p.Category.CategoryName,
                categoryId = p.CategoryId,
                shopId = p.ShopId,
                shop = p.Shop.ShopName,
                price = p.Price,
                isVisible = p.IsVisible
            }).FirstOrDefaultAsync();
        } 

        public async Task<bool> deleteProductById(int id)
        {
            var item = _context.Products.Find(id);
            if (item != null)
            {
                _context.Products.Remove(item);
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public async Task<int> getTotalPagesWithSearch(string search, int limit = 10)
        {
            int TotalPages = (int)Math.Ceiling(_context.Products.Where(p => p.ProductName.Contains(search)).Count() / (double)limit);
            return TotalPages;
        }


        public async Task<ProductListViewModel> GetProductListAsync(ProductFilterViewModel filter, int page = 1, int pageSize = 6)
        {
            var query = _context.Products
        .Include(p => p.Shop)
        .Include(p => p.Category)
        .Include(p => p.Condition)
        .Include(p => p.ProductImages)
        .Where(p => p.IsVisible);

            // Apply filters
            if (filter.SelectedCategoryIds != null && filter.SelectedCategoryIds.Any())
            {
                // Lấy tất cả danh mục (bao gồm cả danh mục con) của các danh mục đã chọn
                var allCategoryIds = new HashSet<int>(filter.SelectedCategoryIds);

                // Thêm các danh mục con
                var childCategories = await _context.Categories
                    .Where(c => filter.SelectedCategoryIds.Contains(c.ParentCategoryId ?? 0))
                    .Select(c => c.CategoryId)
                    .ToListAsync();

                foreach (var childCategoryId in childCategories)
                {
                    allCategoryIds.Add(childCategoryId);
                }

                // Áp dụng filter
                query = query.Where(p => allCategoryIds.Contains(p.CategoryId));
            }

            if (filter.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);
            }

            if (filter.SelectedConditionIds != null && filter.SelectedConditionIds.Any())
            {
                query = query.Where(p => filter.SelectedConditionIds.Contains(p.ConditionId));
            }

            if (filter.ShowPremiumOnly)
            {
                // Get shops with active premium subscription
                var premiumShopIds = await _context.PackageSubscriptions
                    .Where(ps => ps.EndDate >= DateTime.Now && ps.StatusId == 1) // Assuming status 1 is 'Active'
                    .Select(ps => ps.ShopId)
                    .ToListAsync();

                query = query.Where(p => premiumShopIds.Contains(p.ShopId));
            }

            // Apply sorting
            query = filter.SortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "popular" => query.OrderByDescending(p => p.Orders.Count),
                _ => query.OrderByDescending(p => p.CreatedAt) // Default is "newest"
            };

            // Get total count for pagination
            var totalItems = await query.CountAsync();

            // Apply pagination
            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map to view models
            var productViewModels = new List<ProductViewModels>();
            foreach (var product in products)
            {
                var shopInfo = await _shopService.GetShopBasicInfoAsync(product.ShopId);

                var mainImage = product.ProductImages.FirstOrDefault(pi => pi.IsMainImage)?.ImageUrl
                                ?? product.ProductImages.FirstOrDefault()?.ImageUrl
                                ?? "/images/placeholder.png";

                // Determine if product is new (e.g., created in the last 7 days)
                var isNew = (DateTime.Now - product.CreatedAt).TotalDays <= 7;

                // Determine if product is bestseller (e.g., has more than 5 orders)
                var isBestSeller = product.Orders.Count > 5;

                productViewModels.Add(new ProductViewModels
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    Description = product.Description ?? "",
                    MainImageUrl = mainImage,
                    IsInStock = product.IsInStock,
                    Condition = product.Condition.ConditionName,
                    Category = product.Category.CategoryName,
                    Shop = shopInfo,
                    IsNew = isNew,
                    IsBestSeller = isBestSeller
                });
            }

            return new ProductListViewModel
            {
                Products = productViewModels,
                Filter = filter,
                Pagination = new PaginationViewModel
                {
                    TotalItems = totalItems,
                    CurrentPage = page,
                    ItemsPerPage = pageSize
                }
            };
        }

        public async Task<ProductViewModels> GetProductByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Shop)
                .Include(p => p.Category)
                .Include(p => p.Condition)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductId == id && p.IsVisible);

            if (product == null)
                return null;

            var shopInfo = await _shopService.GetShopBasicInfoAsync(product.ShopId);

            var mainImage = product.ProductImages.FirstOrDefault(pi => pi.IsMainImage)?.ImageUrl
                          ?? product.ProductImages.FirstOrDefault()?.ImageUrl
                          ?? "/images/placeholder.png";

            var isNew = (DateTime.Now - product.CreatedAt).TotalDays <= 7;
            var isBestSeller = product.Orders.Count > 5;

            return new ProductViewModels
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Price = product.Price,
                Description = product.Description ?? "",
                MainImageUrl = mainImage,
                IsInStock = product.IsInStock,
                Condition = product.Condition.ConditionName,
                Category = product.Category.CategoryName,
                Shop = shopInfo,
                IsNew = isNew,
                IsBestSeller = isBestSeller
            };
        }

        public async Task<List<CategoryViewModel>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryViewModel
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName
                })
                .ToListAsync();
        }

        public async Task<List<ConditionViewModel>> GetAllConditionsAsync()
        {
            return await _context.ProductConditions
                .Select(c => new ConditionViewModel
                {
                    ConditionId = c.ConditionId,
                    ConditionName = c.ConditionName,
                    Description = c.Description ?? ""
                })
                .ToListAsync();
        }

        public async Task<ProductDetailViewModel> GetProductDetailByIdAsync(int id)
        {
            var product = await _context.Products
        .Include(p => p.Shop)
        .Include(p => p.Category)
        .Include(p => p.Condition)
        .Include(p => p.ProductImages)
        .FirstOrDefaultAsync(p => p.ProductId == id && p.IsVisible);

            if (product == null)
                return null;

            var shopInfo = await _shopService.GetShopDetailAsync(product.ShopId);

            var productDetailViewModel = new ProductDetailViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Price = product.Price,
                Description = product.Description ?? "",
                ImageUrls = product.ProductImages
                    .OrderByDescending(pi => pi.IsMainImage)
                    .Select(pi => pi.ImageUrl)
                    .ToList(),
                IsInStock = product.IsInStock,
                Condition = product.Condition.ConditionName,
                ConditionDescription = product.Condition.Description,
                Category = product.Category.CategoryName,
                CategoryId = product.CategoryId.ToString(),
                Gender = product.Gender,
                Size = product.Size,
                Color = product.Color,
                Brand = product.Brand,
                Material = product.Material,

                // Thay vì gán ShopBasicInfoViewModel, tạo một ShopDetailViewModel
                Shop = new ShopDetailViewModel
                {
                    ShopId = shopInfo.ShopId,
                    ShopName = shopInfo.ShopName,
                    IsPremium = shopInfo.IsPremium,
                    ProfileImage = shopInfo.ProfileImage,
                    Location = shopInfo.Location,
                    CreatedAt = shopInfo.CreatedAt,
                    Description = shopInfo.Description,
                    ProductCount = shopInfo.ProductCount
                }
            };

            return productDetailViewModel;
        }

        public async Task<List<ProductViewModels>> GetRelatedProductsAsync(int productId, string category, int count = 4)
        {
            // Lấy ID danh mục từ tên danh mục
            var categoryId = await _context.Categories
                .Where(c => c.CategoryName == category)
                .Select(c => c.CategoryId)
                .FirstOrDefaultAsync();

            // Nếu không tìm thấy danh mục, trả về danh sách trống
            if (categoryId <= 0)
                return new List<ProductViewModels>();

            // Lấy sản phẩm liên quan từ cùng danh mục, loại trừ sản phẩm hiện tại
            var relatedProducts = await _context.Products
                .Include(p => p.Shop)
                .Include(p => p.Category)
                .Include(p => p.Condition)
                .Include(p => p.ProductImages)
                .Where(p => p.CategoryId == categoryId && p.ProductId != productId && p.IsVisible)
                .Take(count)
                .ToListAsync();

            // Map thành view models
            var productViewModels = new List<ProductViewModels>();
            foreach (var product in relatedProducts)
            {
                var shopInfo = await _shopService.GetShopBasicInfoAsync(product.ShopId);

                var mainImage = product.ProductImages
                    .FirstOrDefault(pi => pi.IsMainImage)?.ImageUrl
                    ?? product.ProductImages.FirstOrDefault()?.ImageUrl
                    ?? "/images/placeholder.png";

                productViewModels.Add(new ProductViewModels
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    Description = product.Description ?? "",
                    MainImageUrl = mainImage,
                    IsInStock = product.IsInStock,
                    Condition = product.Condition.ConditionName,
                    Category = product.Category.CategoryName,
                    Shop = shopInfo
                });
            }

            return productViewModels;
        }
    }
}
