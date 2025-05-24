using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Net.payOS.Types;
using ProjectEXE.Hubs;
using ProjectEXE.Models;
using ProjectEXE.Services.Implementations;
using ProjectEXE.Services.Interfaces;
using System.Security.Claims;
using System.Text.Json;

namespace ProjectEXE.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPayOsService _payOsService;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<PaymentHub> _hubContext;
        private readonly IShopService _shopService;
        private readonly IPackageService _packageService;
        
        public PaymentController(IPayOsService payOsService, IConfiguration configuration, 
                                IHubContext<PaymentHub> hubContext, IShopService shopService,
                                IPackageService packageService)
        {
            _payOsService = payOsService;
            _configuration = configuration;
            _hubContext = hubContext;
            _shopService = shopService;
            _packageService = packageService;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(int packageId, string paymentMethod)
        {
            //id ng dung
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            // Lấy thông tin shop của người dùng
            var shop = await _shopService.GetShopByUserIdAsync(userId);
            if (shop == null)
            {
                TempData["Error"] = "Bạn cần có gian hàng để sử dụng tính năng này.";
                return RedirectToAction("ActiveShop", "Shop");
            }

            //lấy thông tin của gói thanh toán
            var package = await _packageService.GetPackageByIdAsync(packageId);

            //lưu thông tin của packageId
            TempData["PackageId"] = packageId;
            //lưu thông tin của tổng số tiền
            TempData["Amount"] = (int)(package.DiscountedPrice ?? 0);


            switch (paymentMethod)
            {
                case "PayOs":
                    var paymentLinkRequest = new PaymentData(
                                              orderCode: int.Parse(DateTimeOffset.Now.ToString("ffffff")),
                                              amount: (int)(package.DiscountedPrice ?? 0),
                                              description: $"Thanh toan gói dịch vụ",
                                              items: [new("Item test", 1, 2000)],
                                              returnUrl: _configuration["PayOS:PayOSReturnUrl"],
                                              cancelUrl: _configuration["PayOS:PayOSReturnUrl"]);
                    return RedirectToAction("CreatePaymentPayOS", "Payment", paymentLinkRequest);
                default:
                    return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> CreatePaymentPayOS(PaymentData data)
        {
            var url = await _payOsService.CreatePayOSPaymentUrl(data);
            return Redirect(url.checkoutUrl);
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallbackPayOS()
        {
            var response = _payOsService.ProcessReturnUrl(Request.Query);

            if (response == null || response.Code != "00")
            {
                return RedirectToAction("PaymentFail");
            }
            else if (response.Status == "PAID")
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var shopId = await _shopService.GetShopIdByUserId(userId);
                int packageId = int.Parse(TempData["PackageId"].ToString());
                string transactionCode = response.OrderCode.ToString();
                decimal amount = decimal.Parse(TempData["Amount"].ToString());

                var paymentpackageModel = new PackagePayment
                {
                    SubscriptionId = await _shopService.CreatePackageSubscription(shopId, packageId),
                    UserId = userId,
                    PackageId = packageId,
                    TransactionCode = transactionCode,
                    Amount = amount,
                    StatusId = 2,
                    ConfirmedByUserId = 7,
                    CreatedAt = DateTime.UtcNow,
                };

                await _shopService.ActivePackagePayment(paymentpackageModel);
                return RedirectToAction("PaymentSuccess");

            }
            return RedirectToAction("PaymentFail");

        }

        public IActionResult PaymentFail()
        {
            return View();
        }

        public IActionResult PaymentSuccess()
        {
            return View();
        }
    }
}
