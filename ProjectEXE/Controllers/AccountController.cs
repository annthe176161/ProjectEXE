using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel.AccountViewModel;

namespace ProjectEXE.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userService.GetUserByEmailAsync(model.Email);
                var isPasswordValid = await _userService.ValidatePasswordAsync(user, model.Password);

                if (user != null && isPasswordValid)
                {
                    var principal = _userService.CreateClaimsPrincipal(user);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal,
                        new AuthenticationProperties
                        {
                            IsPersistent = model.RememberMe,
                            ExpiresUtc = DateTime.UtcNow.AddDays(model.RememberMe ? 30 : 1)
                        });

                    // Kiểm tra nếu là admin (RoleId = 1), chuyển hướng đến trang quản lý gói dịch vụ
                    if (user.RoleId == 1) // RoleId 1 là Admin
                    {
                        return RedirectToAction("Index", "AdminPackageManagement");
                    }

                    // Kiểm tra và xử lý ReturnUrl cho người dùng không phải admin
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        // Luôn chuyển hướng về trang chủ nếu không có ReturnUrl
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác");
            }

            // Chỉ hiển thị form đăng nhập lại nếu có lỗi
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _userService.IsEmailExistsAsync(model.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng");
                    return View(model);
                }

                var user = await _userService.CreateUserAsync(
                    model.Email,
                    model.Password,
                    model.FullName,
                    model.PhoneNumber,
                    model.Address,
                    model.RoleId
                );

                if (user != null)
                {
                    var principal = _userService.CreateClaimsPrincipal(user);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal,
                        new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTime.UtcNow.AddDays(30)
                        });

                    return RedirectToAction("Index", "Home");
                }
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Thêm thông báo debug nếu cần
            Console.WriteLine("Logging out user");

            // Đảm bảo đúng scheme xác thực
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Xóa tất cả cookies
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            // Thêm thông báo tạm thời để hiển thị sau khi đăng xuất
            TempData["SuccessMessage"] = "Bạn đã đăng xuất thành công!";

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        public IActionResult LogoutConfirm()
        {
            return View();
        }
    }
}
