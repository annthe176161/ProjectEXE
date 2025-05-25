using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ProjectEXE.Controllers
{
    [Authorize(Roles = "Seller")]
    public class ShopProductOfAdminController : Controller
    {
        private readonly IShopServiceOfAdmin _shopService;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ShopProductOfAdminController(
            IShopServiceOfAdmin shopService,
            IUserService userService,
            IHttpContextAccessor httpContextAccessor)
        {
            _shopService = shopService;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }

        private async Task<int?> GetCurrentShopIdAsync()
        {
            try
            {
                // Sử dụng cách tương tự như ShopProductController
                var user = _httpContextAccessor.HttpContext?.User;
                var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim?.Value == null)
                {
                    return null;
                }

                var userId = int.Parse(userIdClaim.Value);

                // Kiểm tra người dùng trong database
                var currentUser = await _userService.GetUserDomainModelByIdAsync(userId);
                if (currentUser == null)
                {
                    return null;
                }

                if (currentUser.RoleId != 3) // RoleId = 3 là Seller
                {
                    return null;
                }

                // Lấy shop của seller
                var shop = await _shopService.GetShopByUserIdAsync(userId);
                if (shop == null)
                {
                    return null;
                }

                return shop.ShopId;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Alternative method như trong ShopProductController
        private async Task<int> GetCurrentShopIdAsyncSimple()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier);
            var userId = int.Parse(userIdClaim?.Value);
            var shop = await _shopService.GetShopByUserIdAsync(userId);
            return shop?.ShopId ?? 0;
        }

        // GET: ShopProductsOfAdmin/Index
        public async Task<IActionResult> Index()
        {
            var shopId = await GetCurrentShopIdAsync();
            if (shopId == null)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập, không phải là Seller, hoặc chưa đăng ký cửa hàng.";
                return RedirectToAction("AccessDenied", "Account");
            }

            ViewData["Title"] = "Sản phẩm của tôi";
            var products = await _shopService.GetProductsByShopIdAsync(shopId.Value);
            return View("~/Views/ShopProductOfAdmin/Index.cshtml", products);
        }

        // GET: ShopProductsOfAdmin/Create
        public async Task<IActionResult> Create()
        {
            var shopId = await GetCurrentShopIdAsync();
            if (shopId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần có cửa hàng để thêm sản phẩm hoặc không có quyền truy cập.";
                return RedirectToAction("AccessDenied", "Account");
            }

            // Kiểm tra thêm như trong ShopProductController
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier);
            var userId = int.Parse(userIdClaim?.Value);

            ViewData["Title"] = "Thêm sản phẩm mới";
            var categories = await _shopService.GetAllCategoriesAsync();
            var conditions = await _shopService.GetAllProductConditionsAsync();

            var viewModel = new ProductEditViewModel
            {
                CategoriesList = categories.Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName }).ToList(),
                ConditionsList = conditions.Select(c => new SelectListItem { Value = c.ConditionId.ToString(), Text = c.ConditionName }).ToList(),
                IsInStock = true,
                IsVisible = true
            };
            return View("~/Views/ShopProductOfAdmin/Create.cshtml", viewModel);
        }

        // POST: ShopProductsOfAdmin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductEditViewModel model)
        {
            var shopId = await GetCurrentShopIdAsync();
            if (shopId == null)
            {
                TempData["ErrorMessage"] = "Phiên làm việc hết hạn hoặc bạn không có quyền thực hiện.";
                return RedirectToAction("AccessDenied", "Account");
            }

            ModelState.Remove("CategoriesList");
            ModelState.Remove("ConditionsList");

            if (ModelState.IsValid)
            {
                bool result = await _shopService.AddProductAsync(model, shopId.Value);
                if (result)
                {
                    TempData["SuccessMessage"] = "Thêm sản phẩm mới thành công!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Không thể thêm sản phẩm. Vui lòng thử lại.");
            }

            var categories = await _shopService.GetAllCategoriesAsync();
            var conditions = await _shopService.GetAllProductConditionsAsync();
            model.CategoriesList = categories.Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName }).ToList();
            model.ConditionsList = conditions.Select(c => new SelectListItem { Value = c.ConditionId.ToString(), Text = c.ConditionName }).ToList();
            ViewData["Title"] = "Thêm sản phẩm mới";
            return View("~/Views/ShopProductOfAdmin/Create.cshtml", model);
        }

        // GET: ShopProductsOfAdmin/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var shopId = await GetCurrentShopIdAsync();
            if (shopId == null)
            {
                TempData["ErrorMessage"] = "Phiên làm việc hết hạn hoặc bạn không có quyền.";
                return RedirectToAction("AccessDenied", "Account");
            }

            var product = await _shopService.GetProductByIdForEditAsync(id.Value, shopId.Value);
            if (product == null) return NotFound();

            ViewData["Title"] = $"Chỉnh sửa sản phẩm: {product.ProductName}";
            var categories = await _shopService.GetAllCategoriesAsync();
            var conditions = await _shopService.GetAllProductConditionsAsync();

            var viewModel = new ProductEditViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId,
                ConditionId = product.ConditionId,
                Gender = product.Gender,
                Size = product.Size,
                Color = product.Color,
                Brand = product.Brand,
                Material = product.Material,
                IsInStock = product.IsInStock,
                IsVisible = product.IsVisible,
                CategoriesList = categories.Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName, Selected = c.CategoryId == product.CategoryId }).ToList(),
                ConditionsList = conditions.Select(c => new SelectListItem { Value = c.ConditionId.ToString(), Text = c.ConditionName, Selected = c.ConditionId == product.ConditionId }).ToList()
            };
            return View("~/Views/ShopProductOfAdmin/Edit.cshtml", viewModel);
        }

        // POST: ShopProductsOfAdmin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductEditViewModel model)
        {
            if (id != model.ProductId) return NotFound();

            var shopId = await GetCurrentShopIdAsync();
            if (shopId == null)
            {
                TempData["ErrorMessage"] = "Phiên làm việc hết hạn hoặc bạn không có quyền.";
                return RedirectToAction("AccessDenied", "Account");
            }

            ModelState.Remove("CategoriesList");
            ModelState.Remove("ConditionsList");

            if (ModelState.IsValid)
            {
                bool result = await _shopService.UpdateProductAsync(model, shopId.Value);
                if (result)
                {
                    TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Không thể cập nhật sản phẩm. Vui lòng thử lại.");
            }

            var categories = await _shopService.GetAllCategoriesAsync();
            var conditions = await _shopService.GetAllProductConditionsAsync();
            model.CategoriesList = categories.Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName, Selected = c.CategoryId == model.CategoryId }).ToList();
            model.ConditionsList = conditions.Select(c => new SelectListItem { Value = c.ConditionId.ToString(), Text = c.ConditionName, Selected = c.ConditionId == model.ConditionId }).ToList();
            ViewData["Title"] = $"Chỉnh sửa sản phẩm: {model.ProductName}";
            return View("~/Views/ShopProductOfAdmin/Edit.cshtml", model);
        }

        // POST: ShopProductsOfAdmin/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var shopId = await GetCurrentShopIdAsync();
            if (shopId == null)
            {
                TempData["ErrorMessage"] = "Phiên làm việc hết hạn hoặc bạn không có quyền.";
                return RedirectToAction("AccessDenied", "Account");
            }

            var (success, errorMessage) = await _shopService.DeleteProductAsync(id, shopId.Value);
            if (success)
            {
                TempData["SuccessMessage"] = errorMessage;
            }
            else
            {
                TempData["ErrorMessage"] = errorMessage;
            }
            return RedirectToAction(nameof(Index));
        }

        // Debug method để test
        [HttpGet]
        public IActionResult TestUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier);

            return Json(new
            {
                HasUser = user != null,
                IsAuthenticated = user?.Identity?.IsAuthenticated ?? false,
                UserIdClaim = userIdClaim?.Value,
                AllClaims = user?.Claims?.Select(c => new { c.Type, c.Value }).ToList()
            });
        }
    }
}