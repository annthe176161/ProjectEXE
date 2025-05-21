using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectEXE.Services.Interfaces;
using System.Security.Claims;

namespace ProjectEXE.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(int productId)
        {
            try
            {
                // Lấy ID người dùng từ claims
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                // Tạo đơn hàng
                var order = await _orderService.CreateOrderAsync(productId, userId);

                // Chuyển hướng đến trang chi tiết đơn hàng với thông báo thành công
                TempData["SuccessMessage"] = "Đơn hàng của bạn đã được tạo thành công và đang chờ người bán xác nhận.";
                return RedirectToAction("OrderDetails", new { id = order.OrderId });
            }
            catch (Exception ex)
            {
                // Nếu có lỗi, chuyển về trang chi tiết sản phẩm với thông báo lỗi
                TempData["ErrorMessage"] = $"Không thể tạo đơn hàng: {ex.Message}";
                return RedirectToAction("ProductDetails", "Product", new { id = productId });
            }
        }

        [HttpGet]
        public async Task<IActionResult> OrderList(int page = 1)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var orderList = await _orderService.GetBuyerOrdersAsync(userId, page);
            return View(orderList);
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var order = await _orderService.GetOrderDetailsAsync(id, userId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền xem đơn hàng này.";
                return RedirectToAction("OrderList");
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int orderId, string reason)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _orderService.CancelOrderAsync(orderId, userId, reason);

            if (result)
            {
                TempData["SuccessMessage"] = "Đơn hàng đã được hủy thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể hủy đơn hàng. Đơn hàng có thể đã được xác nhận hoặc đã hoàn thành.";
            }

            return RedirectToAction("OrderDetails", new { id = orderId });
        }
    }
}
