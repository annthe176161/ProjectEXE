using ProjectEXE.DTO;
using ProjectEXE.Models;
using ProjectEXE.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Humanizer;
using ProjectEXE.ViewModel;

namespace ProjectEXE.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly RevaContext _context;
        public AdminService(RevaContext context)
        {
            _context = context;
        }

        public async Task<bool> addPackage(ServicePackage package)
        {
            try
            {
                _context.ServicePackages.Add(package);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex) { }
            return false;
        }

        public async Task<bool> deleteServiceById(int id)
        {
            var item = _context.ServicePackages.Find(id);
            if (item != null)
            {
                _context.ServicePackages.Remove(item);
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public async Task<bool> editPackage(ServicePackage p)
        {
            var package = await _context.ServicePackages.FindAsync(p.PackageId);
            if (package == null) return false;

            package.PackageName = p.PackageName;
            package.ProductLimit = p.ProductLimit;
            package.Price = p.Price;
            package.DiscountedPrice = p.DiscountedPrice;

            _context.ServicePackages.Update(package);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ServicePackage>> getAllService()
        {
            return _context.ServicePackages.Include(s => s.PackageSubscriptions).ThenInclude(ps => ps.Status).ToList();
        }
        public async Task<List<RBMDto>> getRevenueByMonth()
        {
            var twelveMonthsAgo = DateTime.Now.AddMonths(-11);
            var firstDay = new DateTime(twelveMonthsAgo.Year, twelveMonthsAgo.Month, 1);

            var revenueData = await _context.PackagePayments
                .Where(p => p.CreatedAt >= firstDay && p.Status.StatusName == "Confirmed")
                .ToListAsync(); // Get data from database first

            var result = revenueData
                .GroupBy(p => p.CreatedAt.ToString("yyyy-MM"))
                .Select(g => new RBMDto
                {
                    RevenueMonth = g.Key,
                    TotalRevenue = g.Sum(p => p.Amount), // Remove division by 1000
                    FormattedRevenue = string.Format("{0:n0}", g.Sum(p => p.Amount)) // Format with thousand separators
                })
                .OrderBy(x => x.RevenueMonth)
                .ToList();

            return result;
        }

        public async Task<List<RBPDto>> getRevenueByPackage()
        {
            var twelveMonthsAgo = DateTime.Now.AddMonths(-11);
            var firstDay = new DateTime(twelveMonthsAgo.Year, twelveMonthsAgo.Month, 1);

            var revenueByPackage = await _context.PackagePayments
                .Where(p => p.CreatedAt >= firstDay && p.Status.StatusName == "Confirmed")
                .GroupBy(p => p.Package.PackageName)
                .Select(g => new RBPDto
                {
                    PackageName = g.Key,
                    TotalRevenue = g.Sum(p => p.Amount),  // Remove division by 1000
                    FormattedRevenue = string.Format("{0:n0}", g.Sum(p => p.Amount))  // Format with thousand separators
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToListAsync();

            return revenueByPackage;
        }


        public async Task<IEnumerable<RecentPackagePaymentDto>> GetRecentPackagePaymentsAsync(int count)
        {
            if (_context.PackagePayments == null)
            {
                return Enumerable.Empty<RecentPackagePaymentDto>();
            }

            var recentPayments = await _context.PackagePayments
                .AsNoTracking()
                .Include(pp => pp.User)
                .Include(pp => pp.Package)
                .Include(pp => pp.Status)
                .OrderByDescending(pp => pp.CreatedAt)
                .Take(count)
                .Select(pp => new RecentPackagePaymentDto // Đảm bảo đây là ProjectEXE.ViewModels.RecentPackagePaymentDto
                {
                    UserId = pp.UserId,
                    UserFullName = pp.User != null ? pp.User.FullName : "Không xác định",
                    PackageName = pp.Package != null ? pp.Package.PackageName : "Không xác định",
                    PricePaid = pp.Amount,
                    PaymentStatus = pp.Status != null ? pp.Status.StatusName : "Không xác định",
                    PaymentDate = pp.CreatedAt
                })
                .ToListAsync();

            return recentPayments;
        }


        // TRIỂN KHAI CÁC PHƯƠNG THỨC MỚI CHO CATEGORIES

        public async Task<IEnumerable<CategoryOfAdminViewModel>> GetAllCategoriesWithParentNameAsync()
        {
            if (_context.Categories == null)
            {
                return Enumerable.Empty<CategoryOfAdminViewModel>();
            }

            // Để lấy ParentCategoryName, chúng ta có thể join hoặc dùng sub-query.
            // Cách đơn giản là lấy hết rồi xử lý ở client, nhưng không hiệu quả cho lượng lớn.
            // Cách tốt hơn là dùng LINQ join.
            // Hoặc nếu Category model có navigation property ParentCategory:
            // await _context.Categories.Include(c => c.ParentCategory)...

            var categories = await _context.Categories
                .OrderBy(c => c.CategoryName)
                .Select(c => new CategoryOfAdminViewModel
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    ParentCategoryId = c.ParentCategoryId,
                    // Truy vấn tên của ParentCategory. Nếu không có ParentCategory (ParentCategoryId là null),
                    // thì ParentCategoryName cũng sẽ là null hoặc chuỗi rỗng.
                    ParentCategoryName = c.ParentCategoryId != null ? _context.Categories.FirstOrDefault(p => p.CategoryId == c.ParentCategoryId).CategoryName : null
                }).ToListAsync();

            // Cách tối ưu hơn nếu có navigation property ParentCategory trong Model Category:
            // var categories = await _context.Categories
            //     .Include(c => c.ParentCategory) // Giả sử có navigation property ParentCategory
            //     .OrderBy(c => c.CategoryName)
            //     .Select(c => new CategoryViewModel
            //     {
            //         CategoryId = c.CategoryId,
            //         CategoryName = c.CategoryName,
            //         ParentCategoryId = c.ParentCategoryId,
            //         ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.CategoryName : null
            //     }).ToListAsync();

            return categories;
        }


        public async Task<Category> GetCategoryByIdAsync(int categoryId)
        {
            if (_context.Categories == null) return null;
            return await _context.Categories.FindAsync(categoryId);
        }

        public async Task<IEnumerable<Category>> GetAllParentCategoriesAsync()
        {
            if (_context.Categories == null) return Enumerable.Empty<Category>();
            // Lấy tất cả các category có thể làm cha (ví dụ)
            return await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
        }

        public async Task<bool> AddCategoryAsync(CategoryOfAdminViewModel model)
        {
            if (_context.Categories == null) return false;

            var category = new Category
            {
                CategoryName = model.CategoryName,
                ParentCategoryId = model.ParentCategoryId == 0 ? null : model.ParentCategoryId // Nếu ParentCategoryId là 0 hoặc không chọn, gán null
            };

            try
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log lỗi (ex)
                Console.WriteLine($"Error adding category: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateCategoryAsync(CategoryOfAdminViewModel model)
        {
            if (_context.Categories == null) return false;

            var category = await _context.Categories.FindAsync(model.CategoryId);
            if (category == null)
            {
                return false; // Category not found
            }

            // Kiểm tra tránh tạo vòng lặp cha-con (ví dụ: không cho A làm con của B nếu B đã là con của A)
            // Hoặc không cho một category làm con của chính nó
            if (model.ParentCategoryId.HasValue && model.ParentCategoryId.Value == category.CategoryId)
            {
                // Hoặc trả về lỗi cụ thể hơn
                // ModelState.AddModelError("ParentCategoryId", "Không thể chọn danh mục hiện tại làm danh mục cha.");
                return false; // Cannot set category as its own parent
            }
            // Logic kiểm tra vòng lặp phức tạp hơn có thể cần thiết nếu cho phép thay đổi ParentCategoryId sâu.

            category.CategoryName = model.CategoryName;
            category.ParentCategoryId = model.ParentCategoryId == 0 ? null : model.ParentCategoryId;

            try
            {
                // _context.Categories.Update(category); // Không cần nếu entity đã được tracked
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log lỗi (ex)
                Console.WriteLine($"Error updating category: {ex.Message}");
                return false;
            }
        }

        public async Task<(bool Success, string ErrorMessage)> DeleteCategoryAsync(int categoryId)
        {
            if (_context.Categories == null) return (false, "Không thể kết nối tới danh mục.");

            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
            {
                return (false, "Không tìm thấy danh mục để xóa.");
            }

            // 1. Kiểm tra xem category này có là cha của category khác không
            bool hasChildren = await _context.Categories.AnyAsync(c => c.ParentCategoryId == categoryId);
            if (hasChildren)
            {
                return (false, "Không thể xóa danh mục này vì nó chứa các danh mục con. Vui lòng xóa hoặc di chuyển các danh mục con trước.");
            }

            // 2. Kiểm tra xem category này có đang được sử dụng bởi sản phẩm nào không
            // Giả định _context.Products tồn tại và Product model có CategoryId
            if (_context.Products != null)
            {
                bool isInUseByProducts = await _context.Products.AnyAsync(p => p.CategoryId == categoryId);
                if (isInUseByProducts)
                {
                    return (false, "Không thể xóa danh mục này vì đang được sử dụng bởi một hoặc nhiều sản phẩm.");
                }
            }


            try
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                return (true, "Xóa danh mục thành công.");
            }
            catch (DbUpdateException ex) // Bắt lỗi liên quan đến ràng buộc khóa ngoại nếu có
            {
                // Log lỗi (ex)
                Console.WriteLine($"Error deleting category (DbUpdateException): {ex.InnerException?.Message ?? ex.Message}");
                return (false, "Lỗi khi xóa danh mục. Có thể danh mục này vẫn còn liên kết dữ liệu.");
            }
            catch (Exception ex)
            {
                // Log lỗi (ex)
                Console.WriteLine($"Error deleting category: {ex.Message}");
                return (false, "Đã có lỗi xảy ra trong quá trình xóa danh mục.");
            }
        }
    }
}
