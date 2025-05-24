using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectEXE.Models;
using ProjectEXE.Services.Implementations;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel.OrderViewModel;
using ProjectEXE.ViewModel.ProductViewModel;
using System.Security.Claims;

namespace ProjectEXE.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderConfirmationService _orderConfirmationService;
        private readonly IOrderService _orderService;
        private readonly ILogger<ProductController> _logger;
        private readonly RevaContext _context;

        public ProductController(
            IProductService productService,
            IOrderConfirmationService orderConfirmationService,
            IOrderService orderService,
            ILogger<ProductController> logger,
            RevaContext context)
        {
            _productService = productService;
            _orderConfirmationService = orderConfirmationService;
            _orderService = orderService;
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(ProductFilterViewModel filter, int page = 1)
        {
            // Nếu filter chưa được khởi tạo, đặt giá trị mặc định
            if (filter == null)
            {
                filter = new ProductFilterViewModel();
            }

            // Lấy danh sách danh mục và điều kiện cho bộ lọc
            filter.Categories = await _productService.GetAllCategoriesAsync();
            filter.Conditions = await _productService.GetAllConditionsAsync();

            // Lấy danh sách sản phẩm theo bộ lọc và phân trang
            var viewModel = await _productService.GetProductListAsync(filter, page);

            return View(viewModel);
        }

        public async Task<IActionResult> ProductList([FromQuery] List<int> SelectedCategoryIds, ProductFilterViewModel filter, int page = 1)
        {
            if (filter == null)
            {
                filter = new ProductFilterViewModel();
            }

            // Gán giá trị đã chọn từ query string vào filter
            if (SelectedCategoryIds != null && SelectedCategoryIds.Any())
            {
                filter.SelectedCategoryIds = SelectedCategoryIds;
            }

            // Lấy danh sách danh mục và điều kiện cho bộ lọc
            filter.Categories = await _productService.GetAllCategoriesAsync();
            filter.Conditions = await _productService.GetAllConditionsAsync();

            // Lấy danh sách sản phẩm theo bộ lọc và phân trang
            // Truyền rõ giá trị 6 cho pageSize
            var viewModel = await _productService.GetProductListAsync(filter, page, 6);

            return View(viewModel);
        }

        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _productService.GetProductDetailByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            // Lấy sản phẩm liên quan
            product.RelatedProducts = await _productService.GetRelatedProductsAsync(id, product.Category, 4);

            return View(product);
        }

        // === THÊM CÁC ACTION CHO CHỨC NĂNG MUA HÀNG ===

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ConfirmPurchase(int id)
        {
            try
            {
                var buyerId = GetCurrentUserId();
                if (buyerId == null)
                {
                    return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("ConfirmPurchase", "Product", new { id }) });
                }

                // Check if user can purchase this product
                var canPurchase = await _orderConfirmationService.CanUserPurchaseAsync(id, buyerId.Value);
                if (!canPurchase)
                {
                    TempData["ErrorMessage"] = "Bạn không thể mua sản phẩm này.";
                    return RedirectToAction("ProductDetails", new { id });
                }

                var viewModel = await _orderConfirmationService.GetOrderConfirmationDataAsync(id, buyerId.Value);
                if (viewModel == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin sản phẩm hoặc sản phẩm không còn khả dụng.";
                    return RedirectToAction("ProductDetails", new { id });
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ConfirmPurchase for ProductId: {ProductId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra. Vui lòng thử lại.";
                return RedirectToAction("ProductDetails", new { id });
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPurchase(OrderConfirmationViewModel model)
        {
            _logger.LogInformation("=== CONFIRMPURCHASE POST METHOD CALLED ===");
            _logger.LogInformation("ProductId: {ProductId}", model?.ProductId);

            try
            {
                var buyerId = GetCurrentUserId();
                if (buyerId == null)
                {
                    _logger.LogWarning("BuyerId is null - redirecting to login");
                    return RedirectToAction("Login", "Account");
                }

                // Lấy thông tin người dùng hiện tại
                var buyer = await _context.Users.FirstOrDefaultAsync(u => u.UserId == buyerId);
                if (buyer == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng.";
                    return RedirectToAction("ProductDetails", new { id = model.ProductId });
                }

                // Kiểm tra thông tin liên hệ có đầy đủ không
                if (string.IsNullOrWhiteSpace(buyer.FullName) ||
                    string.IsNullOrWhiteSpace(buyer.PhoneNumber) ||
                    string.IsNullOrWhiteSpace(buyer.Address))
                {
                    TempData["ErrorMessage"] = "Vui lòng cập nhật đầy đủ thông tin liên hệ trong trang cá nhân trước khi gửi yêu cầu mua hàng.";
                    return RedirectToAction("ConfirmPurchase", new { id = model.ProductId });
                }

                // Tạo model để gửi đến service
                var createOrderModel = new CreateOrderRequestViewModel
                {
                    ProductId = model.ProductId,
                    BuyerId = buyerId.Value,
                    SellerId = model.SellerId,
                    BuyerName = buyer.FullName,
                    BuyerPhone = buyer.PhoneNumber,
                    BuyerEmail = buyer.Email,
                    BuyerAddress = buyer.Address
                };

                var result = await _orderConfirmationService.CreateOrderAsync(createOrderModel);

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Yêu cầu mua hàng của bạn đã được gửi thành công. Người bán sẽ liên hệ với bạn sớm nhất có thể.";
                    return RedirectToAction("OrderDetails", "Order", new { id = result.OrderId });
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction("ConfirmPurchase", new { id = model.ProductId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ConfirmPurchase POST for ProductId: {ProductId}", model?.ProductId);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý yêu cầu. Vui lòng thử lại.";
                return RedirectToAction("ConfirmPurchase", new { id = model.ProductId });
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}
