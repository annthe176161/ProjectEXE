using Microsoft.EntityFrameworkCore;
using ProjectEXE.DTO;
using ProjectEXE.Models;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel.OrderViewModel;

namespace ProjectEXE.Services.Implementations
{
    public class OrderConfirmationService : IOrderConfirmationService
    {
        private readonly RevaContext _context;
        private readonly ILogger<OrderConfirmationService> _logger;

        public OrderConfirmationService(RevaContext context, ILogger<OrderConfirmationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<OrderConfirmationViewModel?> GetOrderConfirmationDataAsync(int productId, int buyerId)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Condition)
                .Include(p => p.ProductImages)
                .Include(p => p.Shop)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(p => p.ProductId == productId && p.IsVisible);

            if (product == null)
                return null;

            // Lấy thông tin người dùng từ bảng Users
            var buyer = await _context.Users.FirstOrDefaultAsync(u => u.UserId == buyerId);

            // Tính toán Premium status dựa trên số lượng sản phẩm và thời gian hoạt động
            var productCount = await _context.Products.CountAsync(p => p.ShopId == product.ShopId && p.IsVisible);
            var shopAge = DateTime.Now - product.Shop.CreatedAt;
            bool isPremium = productCount >= 20 && shopAge.TotalDays >= 90;

            var viewModel = new OrderConfirmationViewModel
            {
                ProductId = productId,
                BuyerId = buyerId,
                SellerId = product.Shop.UserId,

                // Thông tin người mua từ profile
                BuyerName = buyer?.FullName ?? string.Empty,
                BuyerPhone = buyer?.PhoneNumber ?? string.Empty,
                BuyerEmail = buyer?.Email ?? string.Empty,
                BuyerAddress = buyer?.Address ?? string.Empty,

                // Thông tin sản phẩm
                Product = new ProductSummaryViewModel
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    MainImageUrl = product.ProductImages?.FirstOrDefault()?.ImageUrl ?? "/images/no-image.jpg",
                    Brand = product.Brand,
                    Size = product.Size,
                    Color = product.Color,
                    Condition = product.Condition?.ConditionName ?? "Không xác định"
                },

                // Thông tin shop
                Shop = new ShopSummaryViewModel
                {
                    ShopId = product.ShopId,
                    ShopName = product.Shop.ShopName,
                    ProfileImage = product.Shop.ProfileImage ?? "/images/default-shop.jpg",
                    IsPremium = isPremium, // SỬA: Sử dụng logic tính toán thay vì thuộc tính
                    CreatedAt = product.Shop.CreatedAt,
                    ProductCount = productCount
                }
            };

            return viewModel;
        }

        // THÊM: Method mới cho CreateOrderRequestViewModel
        public async Task<OrderConfirmationResultViewModel> CreateOrderAsync(CreateOrderRequestViewModel model)
        {
            _logger.LogInformation("Creating order for ProductId: {ProductId}, BuyerId: {BuyerId}",
                model.ProductId, model.BuyerId);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Kiểm tra sản phẩm tồn tại
                var product = await _context.Products
                    .Include(p => p.Shop)
                    .FirstOrDefaultAsync(p => p.ProductId == model.ProductId && p.IsVisible && p.IsInStock);

                if (product == null)
                {
                    return new OrderConfirmationResultViewModel
                    {
                        IsSuccess = false,
                        Message = "Sản phẩm không còn khả dụng hoặc đã được bán."
                    };
                }

                // Kiểm tra người mua không phải là người bán
                if (model.BuyerId == product.Shop.UserId)
                {
                    return new OrderConfirmationResultViewModel
                    {
                        IsSuccess = false,
                        Message = "Bạn không thể mua sản phẩm của chính mình."
                    };
                }

                // Kiểm tra đã có đơn hàng chưa hoàn thành cho sản phẩm này
                var existingOrder = await _context.Orders
                    .FirstOrDefaultAsync(o => o.ProductId == model.ProductId &&
                                       o.BuyerId == model.BuyerId &&
                                       o.StatusId == 1); // Chờ xác nhận

                if (existingOrder != null)
                {
                    return new OrderConfirmationResultViewModel
                    {
                        IsSuccess = false,
                        Message = "Bạn đã có yêu cầu mua hàng cho sản phẩm này đang chờ xác nhận."
                    };
                }

                // Tạo đơn hàng mới
                var order = new Order
                {
                    BuyerId = model.BuyerId,
                    SellerId = model.SellerId,
                    ProductId = model.ProductId,
                    StatusId = 1, // Chờ xác nhận
                    OrderDate = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    VourcherId = model.VoucherId,
                    DiscountAmount = model.DiscountAmount,
                    PayAmount = model.PayAmount,
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Order created successfully with ID: {OrderId}", order.OrderId);

                return new OrderConfirmationResultViewModel
                {
                    IsSuccess = true,
                    OrderId = order.OrderId,
                    Message = "Yêu cầu mua hàng đã được gửi thành công! Người bán sẽ liên hệ với bạn sớm nhất."
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating order for ProductId: {ProductId}", model.ProductId);
                return new OrderConfirmationResultViewModel
                {
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi tạo yêu cầu mua hàng. Vui lòng thử lại."
                };
            }
        }

        // GIỮ: Method cũ để tương thích
        public async Task<OrderConfirmationResultViewModel> CreateOrderAsync(OrderConfirmationViewModel model)
        {
            // Convert sang CreateOrderRequestViewModel
            var createOrderModel = new CreateOrderRequestViewModel
            {
                ProductId = model.ProductId,
                BuyerId = model.BuyerId,
                SellerId = model.SellerId,
                BuyerName = model.BuyerName,
                BuyerPhone = model.BuyerPhone,
                BuyerEmail = model.BuyerEmail,
                BuyerAddress = model.BuyerAddress
            };

            return await CreateOrderAsync(createOrderModel);
        }

        public async Task<bool> IsProductAvailableAsync(int productId)
        {
            return await _context.Products
                .AnyAsync(p => p.ProductId == productId && p.IsVisible && p.IsInStock);
        }

        public async Task<PurchaseValidationResult> CanUserPurchaseAsync(int productId, int buyerId)
        {
            var product = await _context.Products
                .Include(p => p.Shop)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
                return PurchaseValidationResult.Fail("Sản phẩm không tồn tại.");

            if (product.Shop.UserId == buyerId)
                return PurchaseValidationResult.Fail("Bạn không thể mua sản phẩm của chính mình.");

            if (!product.IsVisible)
                return PurchaseValidationResult.Fail("Sản phẩm này hiện không bán. Liên hệ với chủ Shop để biết thêm thông tin.");

            if (!product.IsInStock)
                return PurchaseValidationResult.Fail("Sản phẩm đã hết hàng.");

            return PurchaseValidationResult.Success();
        }

    }
}
