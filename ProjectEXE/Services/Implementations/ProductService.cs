

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

        public async Task<List<CategoryViewModel>> GetCategories()
        {
            return await _context.Categories.Select(c => new CategoryViewModel
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                ParentCategoryId = c.ParentCategoryId,
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
        .Where(p => p.IsVisible && p.IsInStock);

            // Lọc theo danh mục
            if (filter.SelectedCategoryIds != null && filter.SelectedCategoryIds.Any())
            {
                // Sử dụng method mới để lấy tất cả category con
                var allCategoryIds = await GetAllChildCategoryIdsAsync(filter.SelectedCategoryIds);
                query = query.Where(p => allCategoryIds.Contains(p.CategoryId));
            }

            // Thêm lọc theo giới tính
            if (!string.IsNullOrEmpty(filter.Gender))
            {
                query = query.Where(p => p.Gender == filter.Gender);
            }

            // Sắp xếp theo lựa chọn của người dùng (mặc định: newest)
            switch (filter.SortBy?.ToLower())
            {
                case "price_asc":
                    query = query.OrderBy(p => p.Price);
                    break;

                case "price_desc":
                    query = query.OrderByDescending(p => p.Price);
                    break;

                case "newest":
                default:
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
            }

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

                var isNew = (DateTime.Now - product.CreatedAt).TotalDays <= 7;
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

            // THÊM: Lấy categories hierarchy và đánh dấu đã chọn
            filter.Categories = await GetCategoriesHierarchyAsync();
            if (filter.SelectedCategoryIds != null && filter.SelectedCategoryIds.Any())
            {
                MarkSelectedCategories(filter.Categories, filter.SelectedCategoryIds);
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

        public async Task<List<CategoryHierarchyViewModel>> GetCategoriesHierarchyAsync()
        {
            var allCategories = await _context.Categories
    .OrderBy(c => c.CategoryName)
    .ToListAsync();

            return BuildCategoryHierarchy(allCategories, null, 0);
        }

        private List<CategoryHierarchyViewModel> BuildCategoryHierarchy(
    List<Category> allCategories,
    int? parentId,
    int level)
        {
            return allCategories
                .Where(c => c.ParentCategoryId == parentId)
                .Select(c => new CategoryHierarchyViewModel
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    ParentCategoryId = c.ParentCategoryId,
                    Level = level,
                    Children = BuildCategoryHierarchy(allCategories, c.CategoryId, level + 1)
                })
                .ToList();
        }

        public async Task<List<int>> GetAllChildCategoryIdsAsync(List<int> parentCategoryIds)
        {
            var allChildIds = new HashSet<int>(parentCategoryIds);
            var categoriesToCheck = new Queue<int>(parentCategoryIds);

            while (categoriesToCheck.Count > 0)
            {
                var currentId = categoriesToCheck.Dequeue();
                var childCategories = await _context.Categories
                    .Where(c => c.ParentCategoryId == currentId)
                    .Select(c => c.CategoryId)
                    .ToListAsync();

                foreach (var childId in childCategories)
                {
                    if (allChildIds.Add(childId)) // Add returns false if item already exists
                    {
                        categoriesToCheck.Enqueue(childId);
                    }
                }
            }

            return allChildIds.ToList();
        }

        private void MarkSelectedCategories(List<CategoryHierarchyViewModel> categories, List<int> selectedIds)
        {
            foreach (var category in categories)
            {
                category.IsSelected = selectedIds.Contains(category.CategoryId);
                if (category.Children.Any())
                {
                    MarkSelectedCategories(category.Children, selectedIds);
                }
            }
        }

        public async Task<bool> MarkProductAsSoldAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            product.IsInStock = false;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
