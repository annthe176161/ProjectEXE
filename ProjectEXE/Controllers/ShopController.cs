using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel.ShopViewModel;
using System.Security.Claims;

namespace ProjectEXE.Controllers
{
    //[Authorize]
    public class ShopController : Controller
    {
        private readonly IShopService _shopService;
        private readonly ICloudinaryService _cloudinaryService;

        public ShopController(IShopService shopService, ICloudinaryService cloudinaryService)
        {
            _shopService = shopService;
            _cloudinaryService = cloudinaryService;
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
                TempData["Success"] = "Chuyển đổi thành người bán thành công";
            }
            else
            {
                TempData["Error"] = "Tên Shop đã tồn tại, hãy chọn tên khác";
                return View();
            }

            return RedirectToAction("Index", "Home");
        }
        
        [HttpGet]
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
