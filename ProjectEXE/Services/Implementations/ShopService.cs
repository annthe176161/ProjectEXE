using Microsoft.EntityFrameworkCore;
using ProjectEXE.Models;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel.ProductViewModel;
using ProjectEXE.ViewModel.ShopViewModel;

namespace ProjectEXE.Services.Implementations
{
    public class ShopService : IShopService
    {
        private readonly RevaContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ShopService(RevaContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<bool> ActiveShop(ShopView shop, string imageUrl)
        {
            if (_context.Shops.Any(n => n.ShopName == shop.ShopName))
            {
                return false;
            }
            var shopModel = new Shop
            {
                UserId = 1, //tạm thời lấy user = 1
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

        public async Task<bool> HasShopAsync(int userId)
        {
            return await _context.Shops.AnyAsync(s => s.UserId == userId);
        }

        public async Task<bool> CreateShopAsync(CreateShopViewModel model, int userId)
        {
            // Kiểm tra xem người dùng đã có shop chưa
            if (await HasShopAsync(userId))
                return false;

            // Kiểm tra tên shop đã tồn tại chưa
            if (await _context.Shops.AnyAsync(s => s.ShopName == model.ShopName))
                return false;

            string profileImagePath = null;

            // Xử lý upload ảnh đại diện nếu có
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

                profileImagePath = $"/images/shops/{uniqueFileName}";
            }

            // Tạo shop mới
            var shop = new Shop
            {
                ShopName = model.ShopName,
                Description = model.Description,
                ProfileImage = profileImagePath,
                ContactInfo = model.ContactInfo,
                UserId = userId,
                CreatedAt = DateTime.Now
            };

            _context.Shops.Add(shop);
            await _context.SaveChangesAsync();

            // Cập nhật role user thành Seller nếu cần
            var user = await _context.Users.FindAsync(userId);
            if (user != null && user.RoleId != 3) // 3 = Seller role
            {
                user.RoleId = 3;
                await _context.SaveChangesAsync();
            }

            return true;
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
    }
}
