using Microsoft.EntityFrameworkCore;
using ProjectEXE.Models;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel.ShopOrderViewModel;

namespace ProjectEXE.Services.Implementations
{
    public class ShopOrderService : IShopOrderService
    {
        private readonly RevaContext _context;
        private readonly ILogger<ShopOrderService> _logger;

        public ShopOrderService(RevaContext context, ILogger<ShopOrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ShopOrderManagementViewModel> GetShopOrdersAsync(int sellerId, int page = 1, string statusFilter = null, string dateRange = null)
        {
            int pageSize = 10;

            try
            {
                // Base query
                var query = _context.Orders
                    .Include(o => o.Product)
                    .ThenInclude(p => p.ProductImages)
                    .Include(o => o.Buyer)
                    .Include(o => o.Status)
                    .Where(o => o.SellerId == sellerId)
                    .AsQueryable();

                // Apply status filter
                if (!string.IsNullOrEmpty(statusFilter) && int.TryParse(statusFilter, out int statusId))
                {
                    query = query.Where(o => o.StatusId == statusId);
                }

                // Apply date range filter
                if (!string.IsNullOrEmpty(dateRange))
                {
                    var dates = dateRange.Split(" - ");
                    if (dates.Length == 2 &&
                        DateTime.TryParse(dates[0], out DateTime startDate) &&
                        DateTime.TryParse(dates[1], out DateTime endDate))
                    {
                        endDate = endDate.AddDays(1);
                        query = query.Where(o => o.OrderDate >= startDate && o.OrderDate < endDate);
                    }
                }

                // Get total items for pagination
                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                // Get orders for the current page
                var orders = await query
                    .OrderByDescending(o => o.OrderDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Map to view model
                var orderItems = orders.Select(o => new ShopOrderItemViewModel
                {
                    OrderId = o.OrderId,
                    ProductId = o.ProductId,
                    ProductName = o.Product.ProductName,
                    Price = o.Product.Price,
                    ProductImageUrl = o.Product.ProductImages
                        .FirstOrDefault(pi => pi.IsMainImage)?.ImageUrl
                        ?? o.Product.ProductImages.FirstOrDefault()?.ImageUrl
                        ?? "/images/placeholder.png",
                    BuyerId = o.BuyerId,
                    BuyerName = o.Buyer.FullName,
                    BuyerPhone = o.Buyer.PhoneNumber,
                    BuyerAddress = o.Buyer.Address,
                    BuyerEmail = o.Buyer.Email,
                    StatusId = o.StatusId,
                    StatusName = o.Status.StatusName,
                    StatusClass = GetStatusClass(o.StatusId),
                    OrderDate = o.OrderDate,
                    UpdatedAt = o.UpdatedAt,
                    CancelReason = o.CancelReason
                }).ToList();

                // Create pagination
                var pagination = new PaginationViewModel
                {
                    CurrentPage = page,
                    ItemsPerPage = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };

                // Get status options for filter
                var statusOptions = await GetOrderStatusesAsync();

                return new ShopOrderManagementViewModel
                {
                    Orders = orderItems,
                    Pagination = pagination,
                    StatusOptions = statusOptions,
                    StatusFilter = statusFilter,
                    DateRangeFilter = dateRange
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách đơn hàng cho seller {SellerId}", sellerId);
                throw;
            }
        }

        public async Task<ShopOrderDetailViewModel> GetShopOrderDetailAsync(int orderId, int sellerId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Product)
                    .ThenInclude(p => p.ProductImages)
                    .Include(o => o.Product.Category)
                    .Include(o => o.Product.Condition)
                    .Include(o => o.Buyer)
                    .Include(o => o.Status)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId && o.SellerId == sellerId);

                if (order == null)
                {
                    _logger.LogWarning("Không tìm thấy đơn hàng {OrderId} cho seller {SellerId}", orderId, sellerId);
                    return null;
                }

                // Lấy danh sách trạng thái có thể chuyển đổi
                var availableStatuses = GetAvailableStatusTransitions(order.StatusId);

                var statusHistory = new List<OrderStatusHistoryViewModel>
                {
                    new OrderStatusHistoryViewModel
                    {
                        StatusId = order.StatusId,
                        StatusName = order.Status.StatusName,
                        UpdatedAt = order.UpdatedAt,
                        UpdatedBy = "System",
                        Notes = "Trạng thái hiện tại"
                    }
                };

                return new ShopOrderDetailViewModel
                {
                    OrderId = order.OrderId,
                    ProductId = order.ProductId,
                    ProductName = order.Product.ProductName,
                    ProductImageUrl = order.Product.ProductImages
                        .FirstOrDefault(pi => pi.IsMainImage)?.ImageUrl
                        ?? order.Product.ProductImages.FirstOrDefault()?.ImageUrl
                        ?? "/images/placeholder.png",
                    Price = order.Product.Price,
                    Category = order.Product.Category?.CategoryName ?? "Không xác định",
                    Condition = order.Product.Condition?.ConditionName ?? "Không xác định",
                    BuyerId = order.BuyerId,
                    BuyerName = order.Buyer.FullName,
                    BuyerPhone = order.Buyer.PhoneNumber,
                    BuyerEmail = order.Buyer.Email,
                    BuyerAddress = order.Buyer.Address,
                    StatusId = order.StatusId,
                    StatusName = order.Status.StatusName,
                    StatusClass = GetStatusClass(order.StatusId),
                    OrderDate = order.OrderDate,
                    UpdatedAt = order.UpdatedAt,
                    CancelReason = order.CancelReason,
                    AvailableStatuses = availableStatuses,
                    StatusHistory = statusHistory
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết đơn hàng {OrderId} cho seller {SellerId}", orderId, sellerId);
                throw;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusViewModel model, int sellerId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("Tìm đơn hàng {OrderId} cho seller {SellerId}", model.OrderId, sellerId);

                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.OrderId == model.OrderId && o.SellerId == sellerId);

                if (order == null)
                {
                    _logger.LogWarning("Không tìm thấy đơn hàng {OrderId} cho seller {SellerId}", model.OrderId, sellerId);
                    return false;
                }

                _logger.LogInformation("Tìm thấy đơn hàng, trạng thái hiện tại: {CurrentStatus}, trạng thái mới: {NewStatus}",
                    order.StatusId, model.StatusId);

                // Validate business rules
                if (!IsValidStatusTransition(order.StatusId, model.StatusId))
                {
                    _logger.LogWarning("Chuyển đổi trạng thái không hợp lệ từ {FromStatus} thành {ToStatus}",
                        order.StatusId, model.StatusId);
                    return false;
                }

                // Update order
                var oldStatus = order.StatusId;
                order.StatusId = model.StatusId;
                order.UpdatedAt = DateTime.Now;

                // Handle cancel reason
                if (model.StatusId == 5)
                {
                    order.CancelReason = model.CancelReason?.Trim();
                    _logger.LogInformation("Cập nhật lý do hủy: {CancelReason}", model.CancelReason);
                }
                else
                {
                    // Clear cancel reason if status is not cancelled
                    order.CancelReason = null;
                }

                _context.Orders.Update(order);

                var result = await _context.SaveChangesAsync();
                _logger.LogInformation("SaveChanges result: {Result}", result);

                if (result > 0)
                {
                    await transaction.CommitAsync();
                    _logger.LogInformation("Cập nhật thành công đơn hàng {OrderId} từ trạng thái {OldStatus} thành {NewStatus}",
                        model.OrderId, oldStatus, model.StatusId);
                    return true;
                }
                else
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning("Không có thay đổi nào được lưu cho đơn hàng {OrderId}", model.OrderId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái đơn hàng {OrderId}", model.OrderId);
                throw;
            }
        }

        public async Task<Dictionary<int, string>> GetOrderStatusesAsync()
        {
            try
            {
                var statuses = await _context.OrderStatuses
                    .OrderBy(s => s.StatusId)
                    .ToDictionaryAsync(s => s.StatusId, s => s.StatusName);
                return statuses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách trạng thái đơn hàng");
                // Return default statuses if database fails
                return new Dictionary<int, string>
                {
                    { 1, "Chờ xác nhận" },
                    { 2, "Đã xác nhận" },
                    { 3, "Đang giao hàng" },
                    { 4, "Đã giao - Hoàn thành" },
                    { 5, "Đã hủy" }
                };
            }
        }

        /// <summary>
        /// Lấy danh sách trạng thái có thể chuyển đổi từ trạng thái hiện tại
        /// </summary>
        private Dictionary<int, string> GetAvailableStatusTransitions(int currentStatusId)
        {
            var allStatuses = new Dictionary<int, string>
    {
        { 1, "Chờ xác nhận" },
        { 2, "Đã xác nhận" },
        { 3, "Đang giao hàng" },
        { 4, "Đã giao - Hoàn thành" },
        { 5, "Đã hủy" }
    };

            // Nếu đơn hàng đã hoàn thành hoặc đã hủy, chỉ hiển thị trạng thái hiện tại
            if (currentStatusId == 4 || currentStatusId == 5)
            {
                return new Dictionary<int, string>
        {
            { currentStatusId, allStatuses[currentStatusId] }
        };
            }

            // Hiển thị tất cả 5 trạng thái cho các trạng thái khác
            // Logic validation sẽ được xử lý ở phía Controller
            return allStatuses;
        }

        /// <summary>
        /// Kiểm tra xem việc chuyển đổi trạng thái có hợp lệ không
        /// </summary>
        private bool IsValidStatusTransition(int currentStatus, int newStatus)
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

        private string GetStatusClass(int statusId)
        {
            return statusId switch
            {
                1 => "bg-warning text-dark", // Chờ xác nhận
                2 => "bg-info text-white",   // Đã xác nhận
                3 => "bg-primary text-white", // Đang giao hàng
                4 => "bg-success text-white", // Đã giao - Hoàn thành
                5 => "bg-danger text-white",  // Đã hủy
                _ => "bg-secondary text-white"
            };
        }
    }
}
