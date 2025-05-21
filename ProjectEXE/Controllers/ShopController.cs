using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel.ShopViewModel;
using System.Security.Claims;

namespace ProjectEXE.Controllers
{
    [Authorize]
    public class ShopController : Controller
    {
        private readonly IShopService _shopService;

        public ShopController(IShopService shopService)
        {
            _shopService = shopService;
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

        public async Task<IActionResult> Create()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Kiểm tra xem người dùng đã có shop chưa
            if (await _shopService.HasShopAsync(userId))
            {
                return RedirectToAction("Index", "ShopProfile");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateShopViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Kiểm tra xem người dùng đã có shop chưa
            if (await _shopService.HasShopAsync(userId))
            {
                return RedirectToAction("Index", "ShopProfile");
            }

            var result = await _shopService.CreateShopAsync(model, userId);

            if (result)
            {
                TempData["SuccessMessage"] = "Gian hàng của bạn đã được tạo thành công!";
                return RedirectToAction("Index", "ShopProfile");
            }

            ModelState.AddModelError(string.Empty, "Không thể tạo gian hàng. Tên gian hàng có thể đã được sử dụng.");
            return View(model);
        }
    }
}
