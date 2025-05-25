using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectEXE.Services.Interfaces;
using System.Security.Claims;

namespace ProjectEXE.Controllers
{
    [Authorize(Roles = "Seller")]
    public class PackageController : Controller
    {
        private readonly IPackageService _packageService;
        private readonly IShopService _shopService;

        public PackageController(IPackageService packageService, IShopService shopService)
        {
            _packageService = packageService;
            _shopService = shopService;
        }

        public async Task<IActionResult> Index()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Lấy thông tin shop của người dùng
            var shop = await _shopService.GetShopByUserIdAsync(userId);

            if (shop == null)
            {
                TempData["ErrorMessage"] = "Bạn cần có gian hàng để sử dụng tính năng này.";
                return RedirectToAction("Create", "Shop");
            }

            // Lấy danh sách gói dịch vụ
            var model = await _packageService.GetPackagesForShopAsync(shop.ShopId);

            return View(model);
        }

        
    }
}
