﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectEXE.DTO;
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
        private readonly IOrderEmailService _orderEmailService;
        private readonly RevaContext _context;
        private readonly IVourcherService _voucherService;

        public ProductController(
            IProductService productService,
            IOrderConfirmationService orderConfirmationService,
            IOrderService orderService,
            IOrderEmailService orderEmailService,
            ILogger<ProductController> logger,
            RevaContext context,
            IVourcherService voucherService)
        {
            _productService = productService;
            _orderConfirmationService = orderConfirmationService;
            _orderService = orderService;
            _orderEmailService = orderEmailService;
            _context = context;
            _voucherService = voucherService;
        }

        public async Task<IActionResult> Index(ProductFilterViewModel filter, int page = 1)
        {
            if (filter == null)
            {
                filter = new ProductFilterViewModel();
            }

            filter.Conditions = await _productService.GetAllConditionsAsync();
            var viewModel = await _productService.GetProductListAsync(filter, page);

            return View(viewModel);
        }

        public async Task<IActionResult> ProductList([FromQuery] List<int> SelectedCategoryIds, string gender, ProductFilterViewModel filter, int page = 1)
        {
            if (filter == null)
            {
                filter = new ProductFilterViewModel();
            }

            if (!string.IsNullOrEmpty(gender))
            {
                filter.Gender = gender;
            }

            if (SelectedCategoryIds != null && SelectedCategoryIds.Any())
            {
                filter.SelectedCategoryIds = SelectedCategoryIds;
            }

            filter.Conditions = await _productService.GetAllConditionsAsync();
            var viewModel = await _productService.GetProductListAsync(filter, page, 12);

            return View(viewModel);
        }

        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _productService.GetProductDetailByIdAsync(id);

            if (product == null)
            {
                TempData["Warning"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("ProductList", "Product");
            }

            product.RelatedProducts = await _productService.GetRelatedProductsAsync(id, product.Category, 4);

            return View(product);
        }

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

                var validationResult = await _orderConfirmationService.CanUserPurchaseAsync(id, buyerId.Value);
                if (!validationResult.IsValid)
                {
                    TempData["Error"] = validationResult.ErrorMessage;
                    return RedirectToAction("ProductDetails", new { id });
                }

                var viewModel = await _orderConfirmationService.GetOrderConfirmationDataAsync(id, buyerId.Value);
                if (viewModel == null)
                {
                    TempData["Error"] = "Không tìm thấy thông tin sản phẩm hoặc sản phẩm không còn khả dụng.";
                    return RedirectToAction("ProductDetails", new { id });
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra. Vui lòng thử lại.";
                return RedirectToAction("ProductDetails", new { id });
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPurchase(OrderConfirmationViewModel model)
        {
            try
            {
                var buyerId = GetCurrentUserId();
                if (buyerId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Lấy thông tin người dùng hiện tại
                var buyer = await _context.Users.FirstOrDefaultAsync(u => u.UserId == buyerId);
                if (buyer == null)
                {
                    TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                    return RedirectToAction("ProductDetails", new { id = model.ProductId });
                }

                // Kiểm tra thông tin liên hệ có đầy đủ không
                if (string.IsNullOrWhiteSpace(buyer.FullName) ||
                    string.IsNullOrWhiteSpace(buyer.PhoneNumber) ||
                    string.IsNullOrWhiteSpace(buyer.Address))
                {
                    TempData["Error"] = "Vui lòng cập nhật đầy đủ thông tin liên hệ trong trang cá nhân trước khi gửi yêu cầu mua hàng.";
                    return RedirectToAction("ConfirmPurchase", new { id = model.ProductId });
                }

                string? voucherId = null;
                if (!string.IsNullOrWhiteSpace(model.VoucherCode))
                {
                    voucherId = await _voucherService.GetVourcherIdByCode(model.VoucherCode);
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
                    BuyerAddress = buyer.Address,
                    VoucherId = voucherId,
                    DiscountAmount = model.DiscountAmount,
                    PayAmount = model.PayAmount,
                };

                var result = await _orderConfirmationService.CreateOrderAsync(createOrderModel);

                if (result.IsSuccess)
                {
                    try
                    {
                        // Lấy đầy đủ thông tin cần thiết cho email ngay tại đây
                        var orderDetails = await _context.Orders
                            .Include(o => o.Product)
                            .ThenInclude(p => p.Shop)
                            .ThenInclude(s => s.User)
                            .Include(o => o.Buyer)
                            .Include(o => o.Seller)
                            .FirstOrDefaultAsync(o => o.OrderId == result.OrderId);

                        if (orderDetails != null)
                        {
                            // Gửi email đồng bộ - đợi email được gửi xong
                            await _orderEmailService.SendOrderConfirmationNotificationAsync(
                                orderDetails.OrderId,
                                orderDetails.Product.ProductName,
                                orderDetails.Product.Price,
                                orderDetails.Product.Shop.ShopName,
                                orderDetails.Buyer.FullName,
                                orderDetails.Buyer.Email,
                                orderDetails.Seller.FullName,
                                orderDetails.Seller.Email
                            );

                            //giảm số lượng voucher
                            await _voucherService.ReduceQuantityVoucher(model.VoucherCode);

                        }
                        else
                        {
                            TempData["Error"] = $"Không tìm thấy thông tin đơn hàng #{result.OrderId} để gửi email";
                        }
                    }
                    catch (Exception emailEx)
                    {
                        TempData["Error"] = $"Lỗi gửi email thông báo đặt hàng cho đơn hàng #{result.OrderId}";
                    }

                    TempData["Success"] = "Yêu cầu mua hàng của bạn đã được gửi thành công. Người bán sẽ liên hệ với bạn sớm nhất có thể. Email thông báo đã được gửi!";
                    return RedirectToAction("OrderDetails", "Order", new { id = result.OrderId });
                }
                else
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("ConfirmPurchase", new { id = model.ProductId });
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi xử lý yêu cầu. Vui lòng thử lại.";
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

        [HttpGet]
        public async Task<IActionResult> QuickView(int id)
        {
            try
            {
                var product = await _productService.GetProductDetailByIdAsync(id);
                if (product == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
                }

                return PartialView("_QuickViewModal", product);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApplyVoucher([FromBody] ApplyVoucherRequest request)
        {
            var product = await _productService.GetProductById(request.ProductId);
            if (product == null)
                return NotFound(new { message = "Sản phẩm không tồn tại." });

            var result = await _voucherService.ApplyVoucher(request.VoucherCode, product.price);
            if (!result.IsSuccess)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                originalPrice = product.price,
                discountAmount = result.DiscountAmount,
                finalPrice = result.FinalPrice,
                message = result.Message
            });
        }
    }
}