using Microsoft.EntityFrameworkCore;
using ProjectEXE.Models;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel.ShopOrderViewModel;

namespace ProjectEXE.Services.Implementations
{
    public class ShopOrderService : IShopOrderService
    {
        private readonly RevaContext _context;

        public ShopOrderService(RevaContext context)
        {
            _context = context;
        }

        public async Task<ShopOrderManagementViewModel> GetShopOrdersAsync(int sellerId, int page = 1, string statusFilter = null, string dateRange = null)
        {
            int pageSize = 10;

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
                    endDate = endDate.AddDays(1); // Include the end date fully
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

        public async Task<ShopOrderDetailViewModel> GetShopOrderDetailAsync(int orderId, int sellerId)
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
                return null;

            // Trong thực tế bạn sẽ có bảng lịch sử trạng thái, tạm thời mô phỏng
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
                Category = order.Product.Category.CategoryName,
                Condition = order.Product.Condition.ConditionName,
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
                StatusHistory = statusHistory
            };
        }

        public async Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusViewModel model, int sellerId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == model.OrderId && o.SellerId == sellerId);

            if (order == null)
                return false;

            // Kiểm tra nếu trạng thái hợp lệ
            if (model.StatusId < 1 || model.StatusId > 5)
                return false;

            // Nếu hủy đơn hàng, lưu lý do
            if (model.StatusId == 5 && !string.IsNullOrEmpty(model.CancelReason))
            {
                order.CancelReason = model.CancelReason;
            }

            order.StatusId = model.StatusId;
            order.UpdatedAt = DateTime.Now;

            // Trong ứng dụng thực tế, bạn sẽ thêm vào bảng lịch sử trạng thái

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Dictionary<int, string>> GetOrderStatusesAsync()
        {
            var statuses = await _context.OrderStatuses.ToDictionaryAsync(s => s.StatusId, s => s.StatusName);
            return statuses;
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
