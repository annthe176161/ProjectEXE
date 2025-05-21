using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel.Shop;
using System.Security.Claims;

namespace ProjectEXE.Controllers
{
    [Authorize(Roles = "Seller")]
    public class ShopProductController : Controller
    {
        private readonly IShopProductService _shopProductService;
        private readonly IShopService _shopService;

        public ShopProductController(IShopProductService shopProductService, IShopService shopService)
        {
            _shopProductService = shopProductService;
            _shopService = shopService;
        }

        private async Task<int> GetCurrentShopIdAsync()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var shop = await _shopService.GetShopByUserIdAsync(userId);
            return shop?.ShopId ?? 0;
        }

        // GET: ShopProduct/Create
        public async Task<IActionResult> AddProduct()
        {
            var shopId = await GetCurrentShopIdAsync();

            if (shopId == 0)
            {
                return RedirectToAction("Create", "Shop");
            }

            // Kiểm tra quyền tạo sản phẩm
            bool canCreate = await _shopProductService.CanCreateProductAsync(shopId);
            if (!canCreate)
            {
                TempData["ErrorMessage"] = "Bạn không thể đăng sản phẩm. Vui lòng kiểm tra gói dịch vụ của bạn.";
                return RedirectToAction("Index", "Shop");
            }

            var viewModel = await _shopProductService.GetProductFormDataAsync(null, shopId);
            return View("AddProduct", viewModel);
        }

        // POST: ShopProduct/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(ProductFormViewModel model)
        {
            var shopId = await GetCurrentShopIdAsync();

            if (shopId == 0)
            {
                return RedirectToAction("Create", "Shop");
            }

            // Kiểm tra quyền tạo sản phẩm
            bool canCreate = await _shopProductService.CanCreateProductAsync(shopId);
            if (!canCreate)
            {
                TempData["ErrorMessage"] = "Bạn không thể đăng sản phẩm. Vui lòng kiểm tra gói dịch vụ của bạn.";
                return RedirectToAction("Index", "ShopProfile");
            }

            if (ModelState.IsValid)
            {
                bool result = await _shopProductService.CreateProductAsync(model, shopId);

                if (result)
                {
                    TempData["SuccessMessage"] = "Sản phẩm đã được tạo thành công!";
                    return RedirectToAction("Index", "ShopProfile");
                }
                else
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo sản phẩm. Vui lòng thử lại.");
                }
            }

            // Nếu có lỗi, tải lại dữ liệu cho form
            model.Categories = await _shopProductService.GetCategoriesAsync();
            model.Conditions = await _shopProductService.GetConditionsAsync();
            return View("AddProduct", model);
        }

        // GET: ShopProduct/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var shopId = await GetCurrentShopIdAsync();

            if (shopId == 0)
            {
                return RedirectToAction("Create", "Shop");
            }

            var viewModel = await _shopProductService.GetProductFormDataAsync(id, shopId);

            if (viewModel.ProductId == null)
            {
                return NotFound();
            }

            return View("EditProduct", viewModel);
        }

        // POST: ShopProduct/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductFormViewModel model)
        {
            if (id != model.ProductId)
            {
                return NotFound();
            }

            var shopId = await GetCurrentShopIdAsync();

            if (shopId == 0)
            {
                return RedirectToAction("Create", "Shop");
            }

            if (ModelState.IsValid)
            {
                bool result = await _shopProductService.UpdateProductAsync(model, shopId);

                if (result)
                {
                    TempData["SuccessMessage"] = "Sản phẩm đã được cập nhật thành công!";
                    return RedirectToAction("Index", "Shop");
                }
                else
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật sản phẩm. Vui lòng thử lại.");
                }
            }

            // Nếu có lỗi, tải lại dữ liệu cho form
            model.Categories = await _shopProductService.GetCategoriesAsync();
            model.Conditions = await _shopProductService.GetConditionsAsync();
            return View("EditProduct", model);
        }

        // POST: ShopProduct/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var shopId = await GetCurrentShopIdAsync();

            if (shopId == 0)
            {
                return RedirectToAction("Create", "Shop");
            }

            bool result = await _shopProductService.DeleteProductAsync(id, shopId);

            if (result)
            {
                TempData["SuccessMessage"] = "Sản phẩm đã được xóa thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa sản phẩm.";
            }

            return RedirectToAction("Index", "Shop");
        }

        public async Task<IActionResult> Create()
        {
            return RedirectToAction("AddProduct");
        }
    }
}
