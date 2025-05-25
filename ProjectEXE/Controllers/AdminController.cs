using CloudinaryDotNet.Actions; // Bạn có dùng Cloudinary không? Nếu không thì có thể không cần.
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; // Có thể không cần trực tiếp trong Controller nếu Service đã xử lý hết.
using ProjectEXE.DTO;
using ProjectEXE.Models;
// using ProjectEXE.Services.Implementations; // Không cần using Implementation trực tiếp ở đây.
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel; // Sử dụng namespace ViewModel (số ít) theo code bạn cung cấp.
using System; // Cho StringComparison, DateTime
using System.Collections.Generic; // Cho List
using System.Linq;
using System.Threading.Tasks;

namespace ProjectEXE.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IUserService _userService;
        private readonly IProductService _productService; // Giữ lại ProductService

        public AdminController(IAdminService adminService, IUserService userService, IProductService productService)
        {
            _adminService = adminService;
            _userService = userService;
            _productService = productService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userViewModels = await _userService.GetAllUsersAsync();
            ViewData["TotalUsersCount"] = await _userService.GetTotalUsersCountAsync();
            ViewData["TotalShopsCount"] = await _userService.GetTotalShopsCountAsync();
            ViewData["TotalProductsCount"] = await _userService.GetTotalProductsCountAsync();
            ViewData["TotalRevenue"] = await _userService.GetTotalPackagePaymentsRevenueAsync();

            var recentPayments = await _adminService.GetRecentPackagePaymentsAsync(5);
            ViewBag.RecentPackagePayments = recentPayments;

            List<ServicePackage> services = await _adminService.getAllService(); // Xem xét đổi tên phương thức trong IAdminService và AdminService thành GetAllServicePackagesAsync
            List<RBMDto> rbmData = await _adminService.getRevenueByMonth(); // Xem xét đổi tên phương thức
            List<RBPDto> rbpData = await _adminService.getRevenueByPackage(); // Xem xét đổi tên phương thức

            // Lấy danh sách Products từ ProductService
            List<Product> products = await _productService.GetProducts(); // Giả sử GetProducts trả về List<Product>
            ViewData["ProductPages"] = await _productService.getTotalPages(); // Giả sử phương thức này tồn tại

            // Lấy danh sách Categories từ AdminService để hiển thị trên Dashboard
            var categoriesList = await _adminService.GetAllCategoriesWithParentNameAsync();

            var viewModel = new DashboardViewModel
            {
                userViewModels = userViewModels,
                ServicePackages = services,
                RBMDtos = rbmData,
                RBPDtos = rbpData,
                Products = products,
                CategoriesList = categoriesList // Gán danh sách categories vào ViewModel
            };

            return View(viewModel);
        }

        // --- PRODUCT ACTIONS (giữ nguyên từ code của bạn) ---
        [HttpGet]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productService.GetProductById(id);
            var shops = await _productService.GetShops(); // Giả sử phương thức này tồn tại
            var categories = await _productService.GetCategories(); // Giả sử phương thức này tồn tại
            return Json(new
            {
                shops = shops,
                categories = categories,
                product = product
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(string search = "", int page = 1, int limit = 10)
        {
            var products = await _productService.GetProductsWithSearch(search, page, limit);
            var total = await _productService.getTotalPagesWithSearch(search);
            if (page < 1) page = 1;
            if (page > total && total > 0) page = total; // Sửa: chỉ gán page = total nếu total > 0
            else if (total == 0 && page > 1) page = 1; // Nếu không có sản phẩm nào thì page luôn là 1

            return Json(new
            {
                currentPage = page,
                totalPages = total,
                products = products
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Nên thêm ValidateAntiForgeryToken cho các action thay đổi dữ liệu
        public async Task<IActionResult> DeleteProduct(int id)
        {
            bool success = await _productService.deleteProductById(id);
            if (success)
            {
                TempData["Success"] = "Xóa sản phẩm thành công!";
            }
            else
            {
                TempData["Error"] = "Không thể xóa sản phẩm. Vui lòng thử lại.";
            }
            return RedirectToAction("Dashboard", new { fragment = "products-section" }); // Redirect tới khu vực product
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Thêm ValidateAntiForgeryToken
        public async Task<IActionResult> EditProduct([FromBody] ProductsViewModel dto) // Giả sử ProductsViewModel là ViewModel/DTO của bạn
        {
            if (!ModelState.IsValid)
            {
                // Trả về lỗi validation nếu cần
                return BadRequest(ModelState);
            }
            var productToUpdate = new Product // Tạo đối tượng Product từ DTO
            {
                ProductId = dto.productId,
                ProductName = dto.productName,
                Price = dto.price,
                IsVisible = dto.isVisible,
                ShopId = dto.shopId ?? 1, // Cân nhắc việc gán giá trị mặc định này
                CategoryId = dto.categoryId ?? 1 // Cân nhắc việc gán giá trị mặc định này
                // Các thuộc tính khác của Product cần được ánh xạ từ dto
            };
            bool success = await _productService.editProduct(productToUpdate); // editProduct nên nhận Product
            if (!success) return NotFound(new { message = "Không tìm thấy sản phẩm hoặc lỗi khi cập nhật." });

            return Ok(new { message = "Sản phẩm đã được cập nhật thành công." });
        }


        // --- SERVICE PACKAGE ACTIONS (giữ nguyên và thêm ValidateAntiForgeryToken) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id) // Đổi tên thành DeleteServicePackage để rõ ràng hơn
        {
            if (!_adminService.HasActivePackage(id))
            {
                await _adminService.DeleteServiceById(id);
                TempData["Success"] = "Xóa gói dịch vụ thành công!";
            }
            else
            {
                TempData["Error"] = "Gói dịch vụ này đã được đăng ký nên không thể xóa!";
            }
            return RedirectToAction("Dashboard", new { fragment = "packages-section" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPackage([FromBody] ServicePackageViewModel dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var newPackage = new ServicePackage
            {
                PackageName = dto.PackageName,
                ProductLimit = dto.ProductLimit,
                Price = dto.OriginalPrice, // Đảm bảo tên thuộc tính khớp
                DiscountedPrice = dto.SalePrice // Đảm bảo tên thuộc tính khớp
            };

            bool success = await _adminService.addPackage(newPackage); // addPackage nên là AddServicePackageAsync
            if (!success) return BadRequest(new { message = "Không thể thêm gói dịch vụ." });

            return Ok(new { message = "Gói dịch vụ đã được thêm thành công." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPackage([FromBody] ServicePackageViewModel dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var packageToUpdate = new ServicePackage // Đảm bảo ServicePackage có PackageId
            {
                PackageId = dto.PackageId, // ViewModel cần có PackageId
                PackageName = dto.PackageName,
                ProductLimit = dto.ProductLimit,
                Price = dto.OriginalPrice,
                DiscountedPrice = dto.SalePrice
            };
            bool success = await _adminService.editPackage(packageToUpdate); // editPackage nên là EditServicePackageAsync
            if (!success) return NotFound(new { message = "Không tìm thấy gói hoặc lỗi khi cập nhật." });

            return Ok(new { message = "Gói dịch vụ đã được cập nhật thành công." });
        }


        // --- USER MANAGEMENT ACTIONS (Edit, Ban, Unban - giữ nguyên từ code của bạn) ---
        // GET: Admin/Edit/5 (Đường dẫn có thể là Admin/EditUser/5 để rõ ràng hơn)
        public async Task<IActionResult> Edit(int? id) // Đổi tên thành EditUser để tránh trùng với EditCategory nếu có
        {
            if (id == null) return NotFound();

            ViewData["Title"] = "Chỉnh sửa người dùng"; // Thêm Title cho View
            var user = await _userService.GetUserDomainModelByIdAsync(id.Value);
            if (user == null) return NotFound();

            var allRoles = await _userService.GetAllRolesAsync();
            var viewModel = new UserEditViewModel
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                RoleId = user.RoleId,
                IsActive = user.IsActive,
                RolesList = allRoles.Select(r => new SelectListItem
                {
                    Value = r.RoleId.ToString(),
                    Text = r.RoleName,
                    Selected = r.RoleId == user.RoleId
                }).ToList()
            };
            return View(viewModel); // Trả về View "Edit.cshtml" (trong Views/Admin hoặc Views/Users)
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserEditViewModel model) // Đổi tên thành EditUser
        {
            ModelState.Remove("RolesList"); // Bỏ qua validation cho RolesList
            if (id != model.UserId) return NotFound();

            if (ModelState.IsValid)
            {
                var updateResult = await _userService.UpdateUserAsync(model);
                if (updateResult)
                {
                    TempData["SuccessMessage"] = "Cập nhật người dùng thành công!";
                    return RedirectToAction(nameof(Dashboard), new { fragment = "users-section" });
                }
                ModelState.AddModelError("", "Không thể lưu thay đổi. Vui lòng thử lại.");
            }

            var allRoles = await _userService.GetAllRolesAsync();
            model.RolesList = allRoles.Select(r => new SelectListItem
            {
                Value = r.RoleId.ToString(),
                Text = r.RoleName,
                Selected = r.RoleId == model.RoleId
            }).ToList();
            ViewData["Title"] = "Chỉnh sửa người dùng"; // Đặt lại Title
            return View(model);
        }

        // POST: Admin/Ban/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ban(int id)
        {
            var result = await _userService.SetUserActiveStatusAsync(id, 0);
            if (result) TempData["SuccessMessage"] = "Khóa người dùng thành công.";
            else TempData["ErrorMessage"] = "Không thể khóa người dùng.";
            return RedirectToAction(nameof(Dashboard), new { fragment = "users-section" });
        }

        // POST: Admin/Unban/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unban(int id)
        {
            var result = await _userService.SetUserActiveStatusAsync(id, 1);
            if (result) TempData["SuccessMessage"] = "Mở khóa người dùng thành công.";
            else TempData["ErrorMessage"] = "Không thể mở khóa người dùng.";
            return RedirectToAction(nameof(Dashboard), new { fragment = "users-section" });
        }

        // --- CATEGORY MANAGEMENT ACTIONS ---

        // GET: Admin/CreateCategory
        // --- CATEGORY MANAGEMENT ACTIONS ---

        // GET: Admin/CreateCategory
        public async Task<IActionResult> CreateCategory()
        {
            ViewData["Title"] = "Tạo Danh mục mới";
            var allCategories = await _adminService.GetAllParentCategoriesAsync();
            var viewModel = new CategoryOfAdminViewModel // Sửa ở đây
            {
                ParentCategoryList = allCategories.Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName
                }).ToList()
            };
            return View(viewModel);
        }

        // POST: Admin/CreateCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CategoryOfAdminViewModel model) // Sửa ở đây
        {
            ModelState.Remove("ParentCategoryList");
            ModelState.Remove("ParentCategoryName");
            if (ModelState.IsValid)
            {
                bool result = await _adminService.AddCategoryAsync(model);
                if (result)
                {
                    TempData["SuccessMessage"] = "Thêm danh mục mới thành công!";
                    return RedirectToAction(nameof(Dashboard), new { fragment = "categories-management-section" });
                }
                ModelState.AddModelError("", "Không thể thêm danh mục. Vui lòng thử lại.");
            }
            var allCategories = await _adminService.GetAllParentCategoriesAsync();
            model.ParentCategoryList = allCategories.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.CategoryName
            }).ToList();
            ViewData["Title"] = "Tạo Danh mục mới";
            return View(model);
        }

        // GET: Admin/EditCategory/5
        public async Task<IActionResult> EditCategory(int? id)
        {
            if (id == null) return NotFound();
            ViewData["Title"] = "Chỉnh sửa Danh mục";
            var categoryDomainModel = await _adminService.GetCategoryByIdAsync(id.Value);
            if (categoryDomainModel == null) return NotFound();

            var allCategories = await _adminService.GetAllParentCategoriesAsync();
            var viewModel = new CategoryOfAdminViewModel // Sửa ở đây
            {
                CategoryId = categoryDomainModel.CategoryId,
                CategoryName = categoryDomainModel.CategoryName,
                ParentCategoryId = categoryDomainModel.ParentCategoryId,
                ParentCategoryList = allCategories
                                        .Where(c => c.CategoryId != categoryDomainModel.CategoryId)
                                        .Select(c => new SelectListItem
                                        {
                                            Value = c.CategoryId.ToString(),
                                            Text = c.CategoryName,
                                            Selected = c.CategoryId == categoryDomainModel.ParentCategoryId
                                        }).ToList()
            };
            return View(viewModel);
        }

        // POST: Admin/EditCategory/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, CategoryOfAdminViewModel model) // Sửa ở đây
        {
            if (id != model.CategoryId) return NotFound();

            ModelState.Remove("ParentCategoryList");
            ModelState.Remove("ParentCategoryName");

            if (ModelState.IsValid)
            {
                if (model.ParentCategoryId.HasValue && model.ParentCategoryId.Value == model.CategoryId)
                {
                    ModelState.AddModelError("ParentCategoryId", "Không thể chọn danh mục hiện tại làm danh mục cha.");
                }
                else
                {
                    bool result = await _adminService.UpdateCategoryAsync(model);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
                        return RedirectToAction(nameof(Dashboard), new { fragment = "categories-management-section" });
                    }
                    ModelState.AddModelError("", "Không thể cập nhật danh mục. Vui lòng thử lại.");
                }
            }
            var allCategories = await _adminService.GetAllParentCategoriesAsync();
            model.ParentCategoryList = allCategories
                                        .Where(c => c.CategoryId != model.CategoryId)
                                        .Select(c => new SelectListItem
                                        {
                                            Value = c.CategoryId.ToString(),
                                            Text = c.CategoryName,
                                            Selected = c.CategoryId == model.ParentCategoryId
                                        }).ToList();
            ViewData["Title"] = "Chỉnh sửa Danh mục";
            return View(model);
        }

        // POST: Admin/DeleteCategory/5 (Giữ nguyên logic)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var (success, errorMessage) = await _adminService.DeleteCategoryAsync(id);
            if (success) TempData["SuccessMessage"] = errorMessage;
            else TempData["ErrorMessage"] = errorMessage;
            return RedirectToAction(nameof(Dashboard), new { fragment = "categories-management-section" });
        }

        // ... (các action Edit User, Ban, Unban, Product, Package giữ nguyên) ...
    }
}
