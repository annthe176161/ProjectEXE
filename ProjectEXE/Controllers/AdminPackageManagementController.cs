using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectEXE.Services.Interfaces;
using System.Security.Claims;

namespace ProjectEXE.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminPackageManagementController : Controller
    {
        private readonly IAdminPackageService _adminPackageService;

        public AdminPackageManagementController(IAdminPackageService adminPackageService)
        {
            _adminPackageService = adminPackageService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _adminPackageService.GetPaymentDashboardAsync();
            return View(model);
        }

        public async Task<IActionResult> PendingPayments(int page = 1)
        {
            var model = await _adminPackageService.GetPaymentsByStatusAsync(1, page);
            ViewBag.CurrentPage = page;
            return View(model);
        }

        public async Task<IActionResult> CompletedPayments(int page = 1)
        {
            var model = await _adminPackageService.GetPaymentsByStatusAsync(2, page);
            ViewBag.CurrentPage = page;
            return View(model);
        }

        public async Task<IActionResult> RejectedPayments(int page = 1)
        {
            var model = await _adminPackageService.GetPaymentsByStatusAsync(3, page);
            ViewBag.CurrentPage = page;
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var payment = await _adminPackageService.GetPaymentDetailsAsync(id);
            if (payment == null)
            {
                return NotFound();
            }
            return View(payment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            int adminUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _adminPackageService.ConfirmPaymentAsync(id, adminUserId);

            if (result)
            {
                TempData["SuccessMessage"] = "Thanh toán đã được xác nhận và gói dịch vụ đã được kích hoạt.";
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["ErrorMessage"] = "Có lỗi xảy ra khi xác nhận thanh toán. Vui lòng thử lại.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            int adminUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _adminPackageService.RejectPaymentAsync(id, adminUserId, reason);

            if (result)
            {
                TempData["SuccessMessage"] = "Thanh toán đã bị từ chối.";
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["ErrorMessage"] = "Có lỗi xảy ra khi từ chối thanh toán. Vui lòng thử lại.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
