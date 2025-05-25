using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectEXE.Models;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.Services.TokenStorage;
using ProjectEXE.ViewModel.AccountViewModel;
using System.Security.Cryptography;

namespace ProjectEXE.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private  readonly RevaContext _context; // Thêm dòng này

        public AccountController(IUserService userService, IEmailService emailService, RevaContext context)
        {
            _userService = userService;
            _emailService = emailService;
            _context = context;
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

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác");
                    return View(model);
                }

                // Kiểm tra trạng thái tài khoản
                if (user.IsActive == 0) // Tài khoản bị vô hiệu hóa
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị vô hiệu hóa");
                    return View(model);
                }

                if (user.IsActive == 2) // Chưa xác thực email
                {
                    TempData["Warning"] = "Vui lòng xác nhận email của bạn trước khi đăng nhập.";
                    return View(model);
                }

                var isPasswordValid = await _userService.ValidatePasswordAsync(user, model.Password);

                if (isPasswordValid)
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

                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác");
            }


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
                    TempData["Warning"] = "Email này đã được sử dụng";
                    return View(model);
                }

                try
                {
                    // Create the user với RoleId từ form (default = 2)
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
                        // Generate a verification token
                        string token = GenerateRandomToken();

                        // Store the token
                        await TokenStore.StoreTokenAsync(model.Email, token, "EmailVerification");

                        // Send verification email
                        await _emailService.SendVerificationEmailAsync(model.Email, token);

                        // Redirect to confirmation page
                        return RedirectToAction("RegisterConfirmation");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Có lỗi xảy ra khi tạo tài khoản. Vui lòng thử lại.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Register error: {ex.Message}");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo tài khoản. Vui lòng thử lại.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult RegisterConfirmation()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return BadRequest("Email hoặc token không hợp lệ");
            }

            // 
            bool isValid = await TokenStore.ValidateTokenAsync(email, token, "EmailVerification");

            if (isValid)
            {
                // Lấy user và cập nhật trạng thái
                var user = await _userService.GetUserByEmailAsync(email);
                if (user != null && user.IsActive == 2) 
                {
                    user.IsActive = 1; 
                    await _userService.UpdateUserAsync(user);

                    // Đăng nhập tự động
                    var principal = _userService.CreateClaimsPrincipal(user);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal,
                        new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTime.UtcNow.AddDays(30)
                        });
                }

                // Xóa token sau khi sử dụng
                await TokenStore.RemoveTokenAsync(email, "EmailVerification");

                return View("EmailVerified");
            }

            return View("EmailVerificationFailed");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userService.GetUserByEmailAsync(model.Email);

                // Kiểm tra các điều kiện cần thiết
                if (user != null)
                {
                    // Kiểm tra tài khoản có bị vô hiệu hóa không
                    if (user.IsActive == 0)
                    {
                        ModelState.AddModelError("Email", "Tài khoản này đã bị vô hiệu hóa. Vui lòng liên hệ quản trị viên.");
                        return View(model);
                    }

                    // Kiểm tra email đã được xác nhận chưa
                    if (user.IsActive == 2)
                    {
                        ModelState.AddModelError("Email", "Email này chưa được xác nhận. Vui lòng xác nhận email trước khi đặt lại mật khẩu.");
                        return View(model);
                    }

                    // Chỉ gửi email reset nếu tài khoản hợp lệ (IsActive == 1)
                    if (user.IsActive == 1)
                    {
                        // Generate token
                        string token = GenerateRandomToken();

                        // Store the token
                        await TokenStore.StoreTokenAsync(model.Email, token, "PasswordReset");

                        // Send email
                        await _emailService.SendPasswordResetEmailAsync(model.Email, token);

                        // Debug URL
                        var baseUrl = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host;
                        var resetUrl = $"{baseUrl}/Account/ResetPassword?email={Uri.EscapeDataString(model.Email)}&token={Uri.EscapeDataString(token)}";
                        Console.WriteLine($"Password Reset URL: {resetUrl}");

                        return View("ForgotPasswordConfirmation");
                    }
                }
                else
                {
                    // Email không tồn tại
                    ModelState.AddModelError("Email", "Email này chưa được đăng ký trong hệ thống.");
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return BadRequest("Email hoặc token không hợp lệ");
            }

            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            
                if (ModelState.IsValid)
                {
                    bool isValid = await TokenStore.ValidateTokenAsync(
                        model.Email, model.Token, "PasswordReset");

                    if (isValid)
                    {
                        var user = await _userService.GetUserByEmailAsync(model.Email);

                        if (user != null)
                        {
                            // Cập nhật mật khẩu (dùng method có sẵn)
                            user.PasswordHash = _userService.HashPassword(model.Password);
                            await _userService.UpdateUserAsync(user);

                            // Clear cache để tránh vấn đề cũ
                            _context.Entry(user).State = EntityState.Detached;

                            // Xóa token
                            await TokenStore.RemoveTokenAsync(model.Email, "PasswordReset");

                            TempData["Success"] = "Mật khẩu đã được đặt lại thành công. Vui lòng đăng nhập bằng mật khẩu mới.";
                            return RedirectToAction("Login");
                        }
                    }

                    ModelState.AddModelError("", "Đặt lại mật khẩu không thành công. Token không hợp lệ hoặc đã hết hạn.");
                }

                return View(model);
            }
        

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Xóa tất cả cookies
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

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

        // Helper method to generate random token
        private string GenerateRandomToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        // Thêm phương thức để gửi lại email xác nhận
        [HttpGet]
        public IActionResult ResendEmailConfirmation()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendEmailConfirmation(EmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userService.GetUserByEmailAsync(model.Email);

            if (user != null && !await TokenStore.IsEmailVerifiedAsync(model.Email))
            {
                // Generate a new verification token
                string token = GenerateRandomToken();

                // Store the token
                await TokenStore.StoreTokenAsync(model.Email, token, "EmailVerification");

                // Send verification email
                await _emailService.SendVerificationEmailAsync(model.Email, token);
            }

            // Always show success message
            return View("ResendEmailConfirmationConfirmation");
        }
    }
}