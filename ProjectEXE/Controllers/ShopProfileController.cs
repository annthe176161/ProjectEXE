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
            // DEBUG: Log shopId
            //Console.WriteLine($"ShopId: {shop.ShopId}");

            // Lấy thống kê cho shop
            var statistics = await GetShopStatisticsAsync(shop.ShopId);

            // DEBUG: Log statistics
            //Console.WriteLine($"TotalProducts: {statistics.TotalProducts}");
            //Console.WriteLine($"TotalOrders: {statistics.TotalOrders}");

            // Truyền thống kê qua ViewBag
            ViewBag.TotalProducts = statistics.TotalProducts;
            ViewBag.TotalOrders = statistics.TotalOrders;

            // DEBUG: Confirm ViewBag
            //Console.WriteLine($"ViewBag.TotalProducts: {ViewBag.TotalProducts}");
            //Console.WriteLine($"ViewBag.TotalOrders: {ViewBag.TotalOrders}");

            // Hiển thị thông tin shop

            // Hiển thị thông tin shop
            return View(shop);
        }
        // THÊM METHOD NÀY VÀO CONTROLLER
        private async Task<ShopStatistics> GetShopStatisticsAsync(int shopId)
        {
            try
            {
                var totalProducts = await _shopService.GetTotalProductsByShopIdAsync(shopId);
                var totalOrders = await _shopService.GetTotalOrdersByShopIdAsync(shopId);

                Console.WriteLine($"From Service - TotalProducts: {totalProducts}, TotalOrders: {totalOrders}");

                return new ShopStatistics
                {
                    TotalProducts = totalProducts,
                    TotalOrders = totalOrders
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetShopStatisticsAsync: {ex.Message}");
                return new ShopStatistics { TotalProducts = 0, TotalOrders = 0 };
            }
        }
    }

    // THÊM CLASS NÀY VÀO CUỐI FILE (NGOÀI CONTROLLER)
    public class ShopStatistics
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
    }
}


