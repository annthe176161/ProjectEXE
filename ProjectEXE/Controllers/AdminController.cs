using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectEXE.DTO;
using ProjectEXE.Models;
using ProjectEXE.Services.Implementations;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel;

namespace ProjectEXE.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IUserService _userService;
        public AdminController(IAdminService adminService, IUserService userService)
        {
            _adminService = adminService;
            _userService = userService;

        }

        public async Task<IActionResult> Dashboard()
        {
            var userViewModels = await _userService.GetAllUsersAsync();
            ViewData["TotalUsersCount"] = await _userService.GetTotalUsersCountAsync();
            ViewData["TotalShopsCount"] = await _userService.GetTotalShopsCountAsync();
            ViewData["TotalProductsCount"] = await _userService.GetTotalProductsCountAsync(); // <== LẤY TỔNG SỐ SẢN PHẨM

            ViewData["TotalRevenue"] = await _userService.GetTotalPackagePaymentsRevenueAsync(); // <== LẤY TỔNG DOANH THU
                                                                                                 // Lấy thêm danh sách thanh toán gần đây
            var recentPayments = await _adminService.GetRecentPackagePaymentsAsync(5); // Lấy 5 mục cho dashboard
            ViewBag.RecentPackagePayments = recentPayments;

            List<ServicePackage> services = await _adminService.getAllService();
            List<RBMDto> RBM = await _adminService.getRevenueByMonth();
            List<RBPDto> RBP = await _adminService.getRevenueByPackage();
            var viewModel = new DashboardViewModel
            {
                RBMDtos = RBM,
                ServicePackages = services,
                RBPDtos = RBP,
                userViewModels = userViewModels
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            bool success = await _adminService.deleteServiceById(id);

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPackage([FromBody] ServicePackageViewModel dto)
        {
            var newPackage = new ServicePackage
            {
                PackageName = dto.PackageName,
                ProductLimit = dto.ProductLimit,
                Price = dto.OriginalPrice,
                DiscountedPrice = dto.SalePrice
            };

            bool success = await _adminService.addPackage(newPackage);
            if (!success) return BadRequest();

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPackage([FromBody] ServicePackageViewModel dto)
        {
            var Package = new ServicePackage
            {
                PackageId = dto.PackageId,
                PackageName = dto.PackageName,
                ProductLimit = dto.ProductLimit,
                Price = dto.OriginalPrice,
                DiscountedPrice = dto.SalePrice
            };
            bool success = await _adminService.editPackage(Package);
            if(!success) return NotFound();

            return Ok();
        }


        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userService.GetUserDomainModelByIdAsync(id.Value);
            if (user == null)
            {
                return NotFound();
            }

            var allRoles = await _userService.GetAllRolesAsync();

            var viewModel = new UserEditViewModel
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                RoleId = user.RoleId,
                IsActive = user.IsActive,
                RolesList = allRoles.Select(r => new SelectListItem
                {
                    Value = r.RoleId.ToString(),
                    Text = r.RoleName,
                    Selected = r.RoleId == user.RoleId
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserEditViewModel model)
        {
            ModelState.Remove("RolesList");
            if (id != model.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var updateResult = await _userService.UpdateUserAsync(model);
                if (updateResult)
                {
                    TempData["SuccessMessage"] = "User updated successfully!";
                    return RedirectToAction(nameof(Dashboard));
                }
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            //
            //foreach (var state in ModelState)
            //{
            //    var fieldName = state.Key;
            //    var fieldErrors = state.Value.Errors;

            //    foreach (var error in fieldErrors)
            //    {
            //        Console.WriteLine($"Field: {fieldName}, Error: {error.ErrorMessage}");
            //    }
            //}

            // If ModelState is invalid, or update failed, re-populate RolesList
            var allRoles = await _userService.GetAllRolesAsync();
            model.RolesList = allRoles.Select(r => new SelectListItem
            {
                Value = r.RoleId.ToString(),
                Text = r.RoleName,
                Selected = r.RoleId == model.RoleId // Đảm bảo vai trò đã chọn vẫn được giữ lại
            }).ToList();

            return View(model);
        }

        // POST: Users/Ban/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ban(int id)
        {
            var result = await _userService.SetUserActiveStatusAsync(id, false);
            if (result)
            {
                TempData["SuccessMessage"] = "User has been banned successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to ban user. User not found or error occurred.";
            }
            return RedirectToAction(nameof(Dashboard));
        }

        // POST: Users/Unban/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unban(int id)
        {
            var result = await _userService.SetUserActiveStatusAsync(id, true);
            if (result)
            {
                TempData["SuccessMessage"] = "User has been unbanned successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to unban user. User not found or error occurred.";
            }
            return RedirectToAction(nameof(Dashboard));
        }

    }
}
