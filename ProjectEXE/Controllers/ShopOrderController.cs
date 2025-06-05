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
        private readonly IOrderEmailService _orderEmailService;

        public ShopOrderController(IShopOrderService shopOrderService, IOrderEmailService orderEmailService)
        {
            _shopOrderService = shopOrderService;
            _orderEmailService = orderEmailService;
        }

        public async Task<IActionResult> Index(int page = 1, string statusFilter = null, string dateRange = null)
        {
            try
            {
                var sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var model = await _shopOrderService.GetShopOrdersAsync(sellerId, page, statusFilter, dateRange);
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách đơn hàng";
                return View(new ShopOrderManagementViewModel());
            }
        }

        public async Task<IActionResult> OrderDetail(int id)
        {
            try
            {
                var sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var model = await _shopOrderService.GetShopOrderDetailAsync(id, sellerId);

                if (model == null)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền truy cập";
                    return RedirectToAction("Index");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi tải chi tiết đơn hàng";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(UpdateOrderStatusViewModel model)
        {
            try
            {
                var sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                // Lấy thông tin đơn hàng hiện tại để validate
                var currentOrder = await _shopOrderService.GetShopOrderDetailAsync(model.OrderId, sellerId);
                if (currentOrder == null)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền truy cập";
                    return RedirectToAction("Index");
                }

                // Lưu lại trạng thái cũ để so sánh sau khi cập nhật
                int oldStatusId = currentOrder.StatusId;

                // Validate status range
                if (model.StatusId < 1 || model.StatusId > 5)
                {
                    TempData["Error"] = "Trạng thái không hợp lệ";
                    return RedirectToAction("OrderDetail", new { id = model.OrderId });
                }

                // Kiểm tra logic chuyển đổi trạng thái
                if (!IsValidStatusTransitionForUI(currentOrder.StatusId, model.StatusId))
                {
                    string Error = GetStatusTransitionError(currentOrder.StatusId, model.StatusId);
                    TempData["Error"] = Error;
                    return RedirectToAction("OrderDetail", new { id = model.OrderId });
                }

                // Custom validation for cancel reason
                if (model.StatusId == 5 && string.IsNullOrWhiteSpace(model.CancelReason))
                {
                    TempData["Error"] = "Vui lòng nhập lý do hủy đơn hàng";
                    return RedirectToAction("OrderDetail", new { id = model.OrderId });
                }

                if (model.StatusId == 5 && !string.IsNullOrWhiteSpace(model.CancelReason) && model.CancelReason.Trim().Length < 10)
                {
                    TempData["Error"] = "Lý do hủy phải có ít nhất 10 ký tự";
                    return RedirectToAction("OrderDetail", new { id = model.OrderId });
                }

                var result = await _shopOrderService.UpdateOrderStatusAsync(model, sellerId);

                if (result)
                {
                    // Gửi email thông báo khi cập nhật trạng thái thành công
                    if (oldStatusId != model.StatusId) // Chỉ gửi khi trạng thái thực sự thay đổi
                    {
                        try
                        {
                            await _orderEmailService.SendOrderStatusUpdateNotificationAsync(
                                model.OrderId,
                                currentOrder.ProductName,
                                currentOrder.Price,
                                "Shop " + User.Identity.Name, // Sử dụng tên người dùng làm tên shop
                                currentOrder.BuyerName,
                                currentOrder.BuyerEmail,
                                oldStatusId,
                                model.StatusId,
                                model.StatusId == 5 ? model.CancelReason : null);
                        }
                        catch (Exception emailEx)
                        {
                            TempData["Error"] = $"Lỗi gửi email thông báo cập nhật trạng thái đơn hàng {model.OrderId}";
                        }
                    }

                    TempData["Success"] = GetSuccessMessage(model.StatusId, currentOrder.StatusId);
                }
                else
                {
                    TempData["Error"] = $"Không thể cập nhật trạng thái đơn hàng {model.OrderId}";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi cập nhật trạng thái đơn hàng {model.OrderId}";
            }

            return RedirectToAction("OrderDetail", new { id = model.OrderId });
        }
        

        private bool IsValidStatusTransitionForUI(int currentStatus, int newStatus)
        {
            // Không thể thay đổi từ trạng thái hoàn thành hoặc đã hủy
            if (currentStatus == 4 || currentStatus == 5)
            {
                return currentStatus == newStatus; // Chỉ cho phép giữ nguyên
            }

            return newStatus switch
            {
                1 => currentStatus == 1, // Chỉ có thể giữ nguyên "Chờ xác nhận"
                2 => currentStatus == 1 || currentStatus == 2, // Có thể xác nhận từ "Chờ xác nhận" hoặc giữ nguyên
                3 => currentStatus == 2 || currentStatus == 3, // Có thể giao hàng từ "Đã xác nhận" hoặc giữ nguyên
                4 => currentStatus == 3, // Chỉ có thể hoàn thành từ "Đang giao hàng"
                5 => currentStatus is 1 or 2 or 3 or 5, // Có thể hủy từ bất kỳ trạng thái nào trừ hoàn thành
                _ => false
            };
        }

        private string GetStatusTransitionError(int currentStatus, int newStatus)
        {
            if (currentStatus == 4)
            {
                return "❌ Không thể thay đổi trạng thái đơn hàng đã hoàn thành";
            }

            if (currentStatus == 5)
            {
                return "❌ Không thể thay đổi trạng thái đơn hàng đã hủy";
            }

            return (currentStatus, newStatus) switch
            {
                (1, 3) => "❌ Không thể chuyển từ 'Chờ xác nhận' sang 'Đang giao hàng'. Vui lòng xác nhận đơn hàng trước.",
                (1, 4) => "❌ Không thể chuyển từ 'Chờ xác nhận' sang 'Hoàn thành'. Vui lòng xác nhận và giao hàng trước.",
                (2, 1) => "❌ Không thể quay lại trạng thái 'Chờ xác nhận' sau khi đã xác nhận.",
                (2, 4) => "❌ Không thể chuyển từ 'Đã xác nhận' sang 'Hoàn thành'. Vui lòng giao hàng trước.",
                (3, 1) => "❌ Không thể quay lại trạng thái 'Chờ xác nhận' khi đang giao hàng.",
                (3, 2) => "❌ Không thể quay lại trạng thái 'Đã xác nhận' khi đang giao hàng.",
                (4, 5) => "❌ Không thể hủy đơn hàng đã hoàn thành.",
                _ => $"❌ Không thể chuyển từ trạng thái {GetStatusName(currentStatus)} sang {GetStatusName(newStatus)}"
            };
        }

        private string GetStatusName(int statusId)
        {
            return statusId switch
            {
                1 => "Chờ xác nhận",
                2 => "Đã xác nhận",
                3 => "Đang giao hàng",
                4 => "Đã giao - Hoàn thành",
                5 => "Đã hủy",
                _ => "Không xác định"
            };
        }

        private string GetSuccessMessage(int newStatusId, int oldStatusId)
        {
            if (newStatusId == oldStatusId)
            {
                return "Trạng thái đơn hàng không thay đổi";
            }

            return newStatusId switch
            {
                2 => "✅ Đã xác nhận đơn hàng thành công! Bạn có thể liên hệ với khách hàng để trao đổi thông tin giao hàng.",
                3 => "🚚 Đã cập nhật trạng thái 'Đang giao hàng'. Vui lòng theo dõi quá trình giao hàng.",
                4 => "🎉 Đơn hàng đã được hoàn thành thành công! Cảm ơn bạn đã sử dụng dịch vụ.",
                5 => "❌ Đã hủy đơn hàng. Lý do hủy đã được ghi nhận.",
                _ => "Cập nhật trạng thái đơn hàng thành công"
            };
        }
    }
}
