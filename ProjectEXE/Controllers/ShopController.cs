using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectEXE.Services.Implementations;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel;
using ProjectEXE.ViewModel.Shop;
using ProjectEXE.ViewModel.ShopViewModel;
using System.Composition;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjectEXE.Controllers
{
    //[Authorize]
    public class ShopController : Controller
    {
        private readonly IShopService _shopService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;

        public ShopController(IShopService shopService, ICloudinaryService cloudinaryService, 
                            IHttpContextAccessor httpContextAccessor, IUserService userService)
        {
            _shopService = shopService;
            _cloudinaryService = cloudinaryService;
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
        }

        public async Task<IActionResult> Details(int id, int page = 1)
        {
            var shopViewModel = await _shopService.GetShopWithProductsAsync(id, page);

            if (shopViewModel == null)
            {
                return NotFound();
            }

            return View("ShopView", shopViewModel);
        }

        public ActionResult ActiveShop(int id)
        {
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> ActiveShop(ShopView shopModel)
        {
            string imageUrl = await _cloudinaryService.UploadImageAsync(shopModel.ImagePath);
            if (await _shopService.ActiveShop(shopModel, imageUrl))
            {
                TempData["Success"] = "Bạn đã kích hoạt gian hàng thành công!";
            }
            else
            {
                TempData["Error"] = "Tên Shop đã tồn tại, hãy chọn tên khác";
                return View();
            }

            return RedirectToAction("Index", "Home");
        }


        public async Task<IActionResult> AddNewProduct()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            //lấy id của shop
            int shopId = await _shopService.GetShopIdByUserId(userId);
            //check shop đã đăng ký hay chưa
            if (shopId == null)
            {
                TempData["Error"] = "Shop của bạn chưa được kích hoạt, vui lòng kích hoạt";
                return RedirectToAction("ActiveShop", "Shop");
            }
            //check hạn gói đăng ký dịch vụ của shop
            if (!await _shopService.CheckExpiryDate(shopId))
            {
                TempData["Error"] = "Shop của bạn đã hết hạn gói dịch vụ, vui lòng gia hạn để tiếp tục sử dụng dịch vụ!!";
                return RedirectToAction("Index", "Package");
            }
            //kiểm tra xem còn số lượng thêm sản phẩm theo gói dịch vụ hay không
            if (!await _shopService.CanAddProductAsync(shopId))
            {
                TempData["Error"] = "Bạn đã đạt giới hạn sản phẩm của gói hiện tại. Nâng cấp gói để đăng thêm sản phẩm và tăng doanh thu!";
                return RedirectToAction("Index", "Package");
            }
            var viewModel = await _shopService.GetProductFormDataAsync(null, shopId);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewProduct(ProductFormViewModel model)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            //lấy id của shop
            int shopId = await _shopService.GetShopIdByUserId(userId);

            bool result = await _shopService.CreateProductAsync(model, shopId);
            if (result)
            {
                TempData["Success"] = "Sản phẩm đã được tạo thành công!";
                return RedirectToAction("Index", "ShopProfile");
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi tạo sản phẩm. Vui lòng thử lại.";
            }

            // Nếu có lỗi, tải lại dữ liệu cho form
            model.Categories = await _shopService.GetCategoriesAsync();
            model.Conditions = await _shopService.GetConditionsAsync();
            return View("AddNewProduct", model);
        }

        public async Task<IActionResult> ManageProduct()
        {
            var shopId = await GetCurrentShopIdAsync();
            if (shopId == null)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập, không phải là Seller, hoặc chưa đăng ký cửa hàng.";
                return RedirectToAction("AccessDenied", "Account");
            }

            ViewData["Title"] = "Sản phẩm của tôi";
            var products = await _shopService.GetProductsByShopIdAsync(shopId.Value);
            return View(products);
        }

        public async Task<IActionResult> EditProduct(int? id)
        {
            if (id == null) return NotFound();

            var shopId = await GetCurrentShopIdAsync();
            if (shopId == null)
            {
                TempData["Error"] = "Phiên làm việc hết hạn hoặc bạn không có quyền.";
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
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, ProductEditViewModel model)
        {
            if (id != model.ProductId) return NotFound();

            var shopId = await GetCurrentShopIdAsync();
            if (shopId == null)
            {
                TempData["Error"] = "Phiên làm việc hết hạn hoặc bạn không có quyền.";
                return RedirectToAction("AccessDenied", "Account");
            }

            ModelState.Remove("CategoriesList");
            ModelState.Remove("ConditionsList");

            if (ModelState.IsValid)
            {
                bool result = await _shopService.UpdateProductAsync(model, shopId.Value);
                if (result)
                {
                    TempData["Success"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToAction(nameof(ManageProduct));
                }
                ModelState.AddModelError("", "Không thể cập nhật sản phẩm. Vui lòng thử lại.");
            }

            var categories = await _shopService.GetAllCategoriesAsync();
            var conditions = await _shopService.GetAllProductConditionsAsync();
            model.CategoriesList = categories.Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName, Selected = c.CategoryId == model.CategoryId }).ToList();
            model.ConditionsList = conditions.Select(c => new SelectListItem { Value = c.ConditionId.ToString(), Text = c.ConditionName, Selected = c.ConditionId == model.ConditionId }).ToList();
            ViewData["Title"] = $"Chỉnh sửa sản phẩm: {model.ProductName}";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var shopId = await GetCurrentShopIdAsync();
            if (shopId == null)
            {
                TempData["Error"] = "Phiên làm việc hết hạn hoặc bạn không có quyền.";
                return RedirectToAction("AccessDenied", "Account");
            }

            var (success, errorMessage) = await _shopService.DeleteProductAsync(id, shopId.Value);
            if (success)
            {
                TempData["Success"] = errorMessage;
            }
            else
            {
                TempData["Error"] = errorMessage;
            }
            return RedirectToAction(nameof(ManageProduct));
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
    }
}
