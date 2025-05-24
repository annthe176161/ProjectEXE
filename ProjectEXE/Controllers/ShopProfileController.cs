using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectEXE.Services.Interfaces;
using System.Security.Claims;

namespace ProjectEXE.Controllers
{
    [Authorize]
    public class ShopProfileController : Controller
    {
        private readonly IShopService _shopService;

        public ShopProfileController(IShopService shopService)
        {
            _shopService = shopService;
        }

        public async Task<IActionResult> Index()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Kiểm tra xem người dùng đã có shop chưa
            var shop = await _shopService.GetShopByUserIdAsync(userId);

            if (shop == null)
            {
                // Nếu chưa có shop, chuyển hướng đến trang tạo shop
                return RedirectToAction("ActiveShop", "Shop");
            }

            // Hiển thị thông tin shop
            return View(shop);
        }
    }
}
