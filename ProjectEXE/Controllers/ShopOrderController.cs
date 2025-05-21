using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel.ShopOrderViewModel;
using System.Security.Claims;

namespace ProjectEXE.Controllers
{
    [Authorize(Roles = "Seller")]
    public class ShopOrderController : Controller
    {
        private readonly IShopOrderService _shopOrderService;

        public ShopOrderController(IShopOrderService shopOrderService)
        {
            _shopOrderService = shopOrderService;
        }

        public async Task<IActionResult> Index(int page = 1, string statusFilter = null, string dateRange = null)
        {
            var sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var model = await _shopOrderService.GetShopOrdersAsync(sellerId, page, statusFilter, dateRange);
            return View(model);
        }

        public async Task<IActionResult> OrderDetail(int id)
        {
            var sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var model = await _shopOrderService.GetShopOrderDetailAsync(id, sellerId);

            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(UpdateOrderStatusViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("OrderDetail", new { id = model.OrderId });
            }

            var sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _shopOrderService.UpdateOrderStatusAsync(model, sellerId);

            if (result)
            {
                TempData["SuccessMessage"] = "Cập nhật trạng thái đơn hàng thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể cập nhật trạng thái đơn hàng";
            }

            return RedirectToAction("OrderDetail", new { id = model.OrderId });
        }
    }
}
