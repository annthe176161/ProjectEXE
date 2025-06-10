using Microsoft.EntityFrameworkCore;
using ProjectEXE.Models;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel.OrderViewModel;

namespace ProjectEXE.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly RevaContext _context;
        private readonly IProductService _productService;
        private readonly IOrderEmailService _orderEmailService; 

        public OrderService(RevaContext context, IProductService productService, IOrderEmailService orderEmailService)
        {
            _context = context;
            _productService = productService;
            _orderEmailService = orderEmailService; 
        }

        public async Task<OrderDetailsViewModel> CreateOrderAsync(int productId, int buyerId)
        {
            // Lấy thông tin sản phẩm
            var product = await _context.Products
                .Include(p => p.Shop)
                .ThenInclude(s => s.User)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductId == productId && p.IsVisible && p.IsInStock);

            if (product == null)
                throw new Exception("Sản phẩm không tồn tại hoặc không khả dụng");

            // Lấy thông tin người mua
            var buyer = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == buyerId && u.IsActive == 1);

            if (buyer == null)
                throw new Exception("Người dùng không tồn tại hoặc không hoạt động");

            // Kiểm tra xem người mua có đang cố mua sản phẩm của chính mình không
            if (product.Shop.UserId == buyerId)
                throw new Exception("Bạn không thể mua sản phẩm của chính mình");

            // Tạo đơn hàng mới với trạng thái "Chờ xác nhận"
            var order = new Order
            {
                BuyerId = buyerId,
                SellerId = product.Shop.UserId,
                ProductId = productId,
                StatusId = 1, // Chờ xác nhận
                OrderDate = DateTime.Now,
                UpdatedAt = DateTime.Now,
                //VourcherId = ,
                //DiscountAmount = ,
                //PayAmount = ,
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            try
            {
                // Gửi email đồng bộ, không dùng Task.Run
                await _orderEmailService.SendOrderConfirmationNotificationAsync(
                    order.OrderId,
                    product.ProductName,
                    product.Price,
                    product.Shop.ShopName,
                    buyer.FullName,
                    buyer.Email,
                    product.Shop.User.FullName,
                    product.Shop.User.Email
                );
                Console.WriteLine($"✅ Đã gửi email thông báo đặt hàng thành công cho đơn hàng #{order.OrderId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi gửi email thông báo đặt hàng: {ex.Message}");
            }

            // Lấy thông tin đơn hàng đầy đủ để trả về
            var mainImage = product.ProductImages
                .FirstOrDefault(pi => pi.IsMainImage)?.ImageUrl
                ?? product.ProductImages.FirstOrDefault()?.ImageUrl
                ?? "/images/placeholder.png";

            return new OrderDetailsViewModel
            {
                OrderId = order.OrderId,
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Price = product.Price,
                ProductImageUrl = mainImage,
                SellerName = product.Shop.User.FullName,
                ShopName = product.Shop.ShopName,
                BuyerName = buyer.FullName,
                BuyerPhoneNumber = buyer.PhoneNumber,
                BuyerAddress = buyer.Address,
                OrderStatus = "Chờ xác nhận",
                OrderStatusId = 1,
                OrderDate = order.OrderDate,
                UpdatedAt = order.UpdatedAt
            };
        }

        public async Task<bool> CancelOrderAsync(int orderId, int buyerId, string reason)
        {
            var order = await _context.Orders
                .Include(o => o.Product)
                .ThenInclude(p => p.Shop)
                .ThenInclude(s => s.User)
                .Include(o => o.Buyer)
                .Include(o => o.Seller)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.BuyerId == buyerId);

            if (order == null)
                return false;

            // Chỉ cho phép hủy đơn hàng khi đang ở trạng thái "Chờ xác nhận"
            if (order.StatusId != 1)
                return false;

            order.StatusId = 5; // Đã hủy
            order.CancelReason = reason;
            order.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // Gửi email thông báo hủy đơn hàng (chạy bất đồng bộ)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _orderEmailService.SendOrderCancellationNotificationAsync(
                        order.OrderId,
                        order.Product.ProductName,
                        order.Product.Price,
                        order.Product.Shop.ShopName,
                        order.Buyer.FullName,
                        order.Buyer.Email,
                        order.Seller.FullName,
                        order.Seller.Email,
                        reason
                    );
                }
                catch (Exception ex)
                {
                    // Log lỗi nhưng không làm fail transaction
                    Console.WriteLine($"Lỗi gửi email thông báo hủy đơn: {ex.Message}");
                }
            });

            return true;
        }

        // Các method khác giữ nguyên...
        public async Task<OrderListViewModel> GetBuyerOrdersAsync(int buyerId, int page = 1, int pageSize = 10)
        {
            var query = _context.Orders
                .Include(o => o.Product)
                .ThenInclude(p => p.ProductImages)
                .Include(o => o.Seller)
                .Include(o => o.Status)
                .Include(o => o.Product.Shop)
                .Where(o => o.BuyerId == buyerId)
                .OrderByDescending(o => o.OrderDate);

            var totalOrders = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);

            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var orderViewModels = orders.Select(o => new OrderDetailsViewModel
            {
                OrderId = o.OrderId,
                ProductId = o.ProductId,
                ProductName = o.Product.ProductName,
                Price = o.Product.Price,
                ProductImageUrl = o.Product.ProductImages
                    .FirstOrDefault(pi => pi.IsMainImage)?.ImageUrl
                    ?? o.Product.ProductImages.FirstOrDefault()?.ImageUrl
                    ?? "/images/placeholder.png",
                SellerName = o.Seller.FullName,
                ShopName = o.Product.Shop.ShopName,
                OrderStatus = o.Status.StatusName,
                OrderStatusId = o.StatusId,
                OrderDate = o.OrderDate,
                UpdatedAt = o.UpdatedAt,
                CancelReason = o.CancelReason
            }).ToList();

            return new OrderListViewModel
            {
                Orders = orderViewModels,
                TotalOrders = totalOrders,
                CurrentPage = page,
                TotalPages = totalPages
            };
        }

        public async Task<OrderDetailsViewModel> GetOrderDetailsAsync(int orderId, int userId)
        {
            var order = await _context.Orders
                .Include(o => o.Product)
                .ThenInclude(p => p.ProductImages)
                .Include(o => o.Seller)
                .Include(o => o.Buyer)
                .Include(o => o.Status)
                .Include(o => o.Product.Shop)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && (o.BuyerId == userId || o.SellerId == userId));

            if (order == null)
                return null;

            // Chỉ hiển thị thông tin liên hệ khi đơn hàng đã được xác nhận
            string shopContactInfo = null;
            string buyerPhoneNumber = null;
            string buyerAddress = null;

            if (order.StatusId >= 2) // Đã xác nhận hoặc các trạng thái sau
            {
                shopContactInfo = order.Product.Shop.ContactInfo;
                buyerPhoneNumber = order.Buyer.PhoneNumber;
                buyerAddress = order.Buyer.Address;
            }

            return new OrderDetailsViewModel
            {
                OrderId = order.OrderId,
                ProductId = order.ProductId,
                ProductName = order.Product.ProductName,
                Price = order.Product.Price,
                ProductImageUrl = order.Product.ProductImages
                    .FirstOrDefault(pi => pi.IsMainImage)?.ImageUrl
                    ?? order.Product.ProductImages.FirstOrDefault()?.ImageUrl
                    ?? "/images/placeholder.png",
                SellerName = order.Seller.FullName,
                ShopName = order.Product.Shop.ShopName,
                ShopContactInfo = shopContactInfo,
                BuyerName = order.Buyer.FullName,
                BuyerPhoneNumber = buyerPhoneNumber,
                BuyerAddress = buyerAddress,
                OrderStatus = order.Status.StatusName,
                OrderStatusId = order.StatusId,
                OrderDate = order.OrderDate,
                UpdatedAt = order.UpdatedAt,
                CancelReason = order.CancelReason
            };
        }
    }
}