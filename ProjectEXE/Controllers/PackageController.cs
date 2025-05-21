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

        public async Task<IActionResult> Select(int id)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Lấy thông tin shop của người dùng
            var shop = await _shopService.GetShopByUserIdAsync(userId);

            if (shop == null)
            {
                TempData["ErrorMessage"] = "Bạn cần có gian hàng để sử dụng tính năng này.";
                return RedirectToAction("Create", "Shop");
            }

            try
            {
                // Tạo yêu cầu thanh toán
                var paymentInfo = await _packageService.CreatePaymentRequestAsync(id, shop.ShopId, userId);
                return View(paymentInfo);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment(string transactionCode)
        {
            if (string.IsNullOrEmpty(transactionCode))
            {
                TempData["ErrorMessage"] = "Mã giao dịch không hợp lệ.";
                return RedirectToAction("Index");
            }

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                // Xác nhận thanh toán
                var result = await _packageService.ConfirmPaymentAsync(transactionCode, userId);
                return View(result);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}
