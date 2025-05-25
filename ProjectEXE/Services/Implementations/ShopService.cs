using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using ProjectEXE.Models;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel;
using ProjectEXE.ViewModel.ProductViewModel;
using ProjectEXE.ViewModel.Shop;
using ProjectEXE.ViewModel.ShopViewModel;
using System.Security.Claims;

namespace ProjectEXE.Services.Implementations
{
    public class ShopService : IShopService
    {
        private readonly RevaContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICloudinaryService _cloudinaryService;

        public ShopService(RevaContext context, IWebHostEnvironment webHostEnvironment, 
                    IHttpContextAccessor httpContextAccessor, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _cloudinaryService = cloudinaryService;
        }

        //====================hoapmhe173343====================================
        public async Task<bool> ActiveShop(ShopView shop, string imageUrl)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return false; // Không xác định được người dùng
            }

            int userId = int.Parse(userIdClaim.Value);

            if (_context.Shops.Any(n => n.ShopName == shop.ShopName))
            {
                return false;
            }
            var shopModel = new Shop
            {
                UserId = userId,
                ShopName = shop.ShopName,
                Description = shop.Description,
                ProfileImage = imageUrl,
                ContactInfo = shop.ContactInfo,
                CreatedAt = DateTime.Now,
            };

            await _context.Shops.AddAsync(shopModel);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> CreatePackageSubscription(int shopId, int packegeId)
        {
            var subscription = new PackageSubscription
            {
                ShopId = shopId,
                PackageId = packegeId,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(30),
                StatusId = 1, // Active
                CreatedAt = DateTime.Now
            };
            _context.PackageSubscriptions.Add(subscription);
            _context.SaveChanges();

            return subscription.SubscriptionId;
        }

        public async Task ActivePackagePayment(PackagePayment packagePayment)
        {
            await _context.PackagePayments.AddAsync(packagePayment);
            await _context.SaveChangesAsync();  
        }

        public async Task<int> GetShopIdByUserId(int userId)
        {
            var shop = await _context.Shops.FirstOrDefaultAsync(s => s.UserId == userId);
            return shop.ShopId;
        }

        public async Task<bool> CheckExpiryDate(int shopId)
        {
            var subscription = await _context.PackageSubscriptions
                .Where(p => p.ShopId == shopId)
                .OrderByDescending(p => p.EndDate) // lấy gói mới nhất
                .FirstOrDefaultAsync();

            if (subscription == null)
                return false; // không có gói nào => hết hạn

            return subscription.EndDate >= DateTime.UtcNow; // còn hạn thì trả true
        }

        public async Task<bool> CanAddProductAsync(int shopId)
        {
            // 1. Lấy gói hiện tại của shop (chưa hết hạn)
            var activeSubscription = await _context.PackageSubscriptions
                                    .Include(s => s.Package) 
                                    .Where(s => s.ShopId == shopId)
                                    .OrderByDescending(s => s.EndDate)
                                    .FirstOrDefaultAsync();

            int productLimit = activeSubscription.Package.ProductLimit;

            // Đếm số sản phẩm hiện có của shop
            int currentProductCount = await _context.Products
                .CountAsync(p => p.ShopId == shopId && p.IsInStock);

            // Kiểm tra xem còn slot để thêm sản phẩm không
            return currentProductCount < productLimit;
        }

        public async Task<List<CategoryViewModel>> GetCategoriesAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryViewModel
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    ParentCategoryId = c.ParentCategoryId
                })
                .ToListAsync();
        }

        public async Task<List<ConditionViewModel>> GetConditionsAsync()
        {
            return await _context.ProductConditions
                .Select(c => new ConditionViewModel
                {
                    ConditionId = c.ConditionId,
                    ConditionName = c.ConditionName
                })
                .ToListAsync();
        }

        public async Task<bool> CreateProductAsync(ProductFormViewModel model, int shopId)
        { 
            try
            {
                var product = new Product
                {
                    ShopId = shopId,
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
                    IsInStock = true,
                    IsVisible = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync(); // Để có ProductId cho ảnh

                // Upload ảnh song song
                var uploadTasks = model.NewImages.Select((file, index) => Task.Run(async () =>
                {
                    var uploadResult = await _cloudinaryService.UploadImageAsync(file);
                    return new ProductImage
                    {
                        ProductId = product.ProductId,
                        ImageUrl = uploadResult,
                        IsMainImage = (index == 0) // Ảnh đầu là ảnh chính
                    };
                })).ToList();

                var uploadedImages = await Task.WhenAll(uploadTasks);
                _context.ProductImages.AddRange(uploadedImages);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //======================================

        public async Task<bool> IsShopPremiumAsync(int shopId)
        {
            var activeSubscription = await _context.PackageSubscriptions
                .FirstOrDefaultAsync(s => s.ShopId == shopId &&
                                           s.StatusId == 1 && // Active
                                           s.EndDate > DateTime.Now);

            return activeSubscription != null;
        }

        public async Task<ShopBasicInfoViewModel> GetShopBasicInfoAsync(int shopId)
        {
            var shop = await _context.Shops
                .FirstOrDefaultAsync(s => s.ShopId == shopId);

            if (shop == null)
                return null;

            var isPremium = await IsShopPremiumAsync(shopId);

            return new ShopBasicInfoViewModel
            {
                ShopId = shop.ShopId,
                ShopName = shop.ShopName,
                IsPremium = isPremium
            };
        }

        public async Task<ShopDetailViewModel> GetShopDetailAsync(int shopId)
        {
            var shop = await _context.Shops
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.ShopId == shopId);

            if (shop == null)
                return null;

            var isPremium = await IsShopPremiumAsync(shopId);

            var productCount = await _context.Products
                .CountAsync(p => p.ShopId == shopId && p.IsVisible);

            return new ShopDetailViewModel
            {
                ShopId = shop.ShopId,
                ShopName = shop.ShopName,
                Description = shop.Description ?? "Chưa có mô tả",
                ProfileImage = !string.IsNullOrEmpty(shop.ProfileImage)
                    ? shop.ProfileImage
                    : "/images/shops/default-shop.jpg",
                ContactInfo = shop.ContactInfo,
                SellerName = shop.User.FullName,
                Location = shop.User.Address ?? "Việt Nam",
                CreatedAt = shop.CreatedAt,
                ProductCount = productCount,
                IsPremium = isPremium
            };
        }

        public async Task<ShopDetailViewModel> GetShopWithProductsAsync(int shopId, int page = 1, int pageSize = 6)
        {
            // Lấy thông tin chi tiết shop
            var shopDetail = await GetShopDetailAsync(shopId);

            if (shopDetail == null)
                return null;

            // Lấy danh sách sản phẩm của shop với phân trang
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Condition)
                .Include(p => p.ProductImages)
                .Where(p => p.ShopId == shopId && p.IsVisible)
                .OrderByDescending(p => p.CreatedAt);

            // Tổng số sản phẩm để phân trang
            var totalItems = await query.CountAsync();

            // Lấy sản phẩm theo trang hiện tại
            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Chuyển đổi sang view model
            var productViewModels = products.Select(p => new ProductViewModels
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Price = p.Price,
                Description = p.Description ?? "",
                MainImageUrl = p.ProductImages
                    .FirstOrDefault(pi => pi.IsMainImage)?.ImageUrl
                    ?? p.ProductImages.FirstOrDefault()?.ImageUrl
                    ?? "/images/placeholder.png",
                IsInStock = p.IsInStock,
                Condition = p.Condition.ConditionName,
                Category = p.Category.CategoryName,
                Shop = new ShopBasicInfoViewModel
                {
                    ShopId = shopId,
                    ShopName = shopDetail.ShopName,
                    IsPremium = shopDetail.IsPremium
                },
                IsNew = (DateTime.Now - p.CreatedAt).TotalDays <= 7
            }).ToList();

            // Cấu hình phân trang
            var pagination = new PaginationViewModel
            {
                TotalItems = totalItems,
                ItemsPerPage = pageSize,
                CurrentPage = page
            };

            // Gán danh sách sản phẩm và phân trang vào kết quả
            shopDetail.Products = productViewModels;
            shopDetail.Pagination = pagination;

            return shopDetail;
        }

        public async Task<Shop> GetShopByUserIdAsync(int userId)
        {
            return await _context.Shops
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }
        
        public async Task<bool> UpdateShopAsync(CreateShopViewModel model, int shopId)
        {
            var shop = await _context.Shops.FindAsync(shopId);

            if (shop == null)
                return false;

            // Kiểm tra tên shop đã tồn tại chưa (nếu đổi tên)
            if (shop.ShopName != model.ShopName &&
                await _context.Shops.AnyAsync(s => s.ShopName == model.ShopName))
                return false;

            // Xử lý upload ảnh nếu có
            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "shops");

                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Tạo tên file duy nhất
                string uniqueFileName = $"{Guid.NewGuid()}_{model.ProfileImage.FileName}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Lưu file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfileImage.CopyToAsync(fileStream);
                }

                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(shop.ProfileImage))
                {
                    string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, shop.ProfileImage.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                shop.ProfileImage = $"/images/shops/{uniqueFileName}";
            }

            // Cập nhật thông tin shop
            shop.ShopName = model.ShopName;
            shop.Description = model.Description;
            shop.ContactInfo = model.ContactInfo;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ProductFormViewModel> GetProductFormDataAsync(int? productId, int shopId)
        {
            var viewModel = new ProductFormViewModel
            {
                Categories = await GetCategoriesAsync(),
                Conditions = await GetConditionsAsync()
            };

            if (productId.HasValue)
            {
                var product = await _context.Products
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.ProductId == productId && p.ShopId == shopId);

                if (product != null)
                {
                    viewModel.ProductId = product.ProductId;
                    viewModel.ProductName = product.ProductName;
                    viewModel.Description = product.Description;
                    viewModel.Price = product.Price;
                    viewModel.CategoryId = product.CategoryId;
                    viewModel.ConditionId = product.ConditionId;
                    viewModel.Gender = product.Gender;
                    viewModel.Size = product.Size;
                    viewModel.Color = product.Color;
                    viewModel.Brand = product.Brand;
                    viewModel.Material = product.Material;
                    viewModel.IsInStock = product.IsInStock;
                    viewModel.IsVisible = product.IsVisible;

                    viewModel.ExistingImages = product.ProductImages.Select(pi => new ProductImageViewModel
                    {
                        ImageId = pi.ImageId,
                        ImageUrl = pi.ImageUrl,
                        IsMainImage = pi.IsMainImage
                    }).ToList();

                    var mainImage = product.ProductImages.FirstOrDefault(pi => pi.IsMainImage);
                    if (mainImage != null)
                    {
                        viewModel.MainImageId = mainImage.ImageId;
                    }
                }
            }

            return viewModel;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            try
            {
                if (_context.Categories == null)
                {
                    return Enumerable.Empty<Category>();
                }

                var categories = await _context.Categories
                    .OrderBy(c => c.CategoryName)
                    .ToListAsync();

                return categories;
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<Category>();
            }
        }

        public async Task<Product> GetProductByIdForEditAsync(int productId, int shopId)
        {
            try
            {
                if (_context.Products == null)
                {
                    return null;
                }

                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == productId && p.ShopId == shopId);
                return product;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IEnumerable<ProductCondition>> GetAllProductConditionsAsync()
        {
            try
            {
                if (_context.ProductConditions == null)
                {
                    return Enumerable.Empty<ProductCondition>();
                }

                var conditions = await _context.ProductConditions
                    .OrderBy(pc => pc.ConditionName)
                    .ToListAsync();

                return conditions;
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<ProductCondition>();
            }
        }

        public async Task<bool> UpdateProductAsync(ProductEditViewModel model, int shopId)
        {
            try
            {
                if (model == null || _context.Products == null)
                {
                    return false;
                }

                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == model.ProductId && p.ShopId == shopId);

                if (product == null)
                {
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

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<(bool Success, string ErrorMessage)> DeleteProductAsync(int productId, int shopId)
        {
            try
            {
                if (_context.Products == null)
                {
                    return (false, "Không thể kết nối cơ sở dữ liệu sản phẩm.");
                }

                var product = await _context.Products.Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.ProductId == productId && p.ShopId == shopId);

                if (product == null)
                {
                    return (false, "Không tìm thấy sản phẩm hoặc bạn không có quyền xóa sản phẩm này.");
                }
                _context.ProductImages.RemoveRange(product.ProductImages);

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return (true, "Xóa sản phẩm thành công.");
            }
            catch (Exception ex)
            {
                return (false, "Đã có lỗi xảy ra khi xóa sản phẩm.");
            }
        }
        public async Task<IEnumerable<ShopProductViewModel>> GetProductsByShopIdAsync(int shopId)
        {
            try
            {
                if (_context.Products == null)
                {
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

                return products;
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<ShopProductViewModel>();
            }
        }
    }
}
