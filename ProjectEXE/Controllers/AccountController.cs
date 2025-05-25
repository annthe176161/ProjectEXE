using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
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
        private readonly RevaContext _context;

        public AccountController(IUserService userService, IEmailService emailService, RevaContext context)
        {
            _userService = userService;
            _emailService = emailService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null, bool clearCache = false)
        {
            Console.WriteLine($"LOGIN GET called with returnUrl: {returnUrl}, clearCache: {clearCache}");

            if (clearCache)
            {
                // Clear all caches when coming from password reset
                _context.ChangeTracker.Clear();

                // Clear cookies
                foreach (var cookie in Request.Cookies.Keys)
                {
                    Response.Cookies.Delete(cookie);
                }

                Console.WriteLine("Cache cleared after password reset");
            }

            if (User.Identity.IsAuthenticated)
            {
                Console.WriteLine("User already authenticated, redirecting to Home");
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            Console.WriteLine($"LOGIN POST called for email: {model?.Email}");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var user = await _userService.GetUserByEmailAsync(model.Email);

                if (user == null)
                {
                    TempData["Warning"] = "Email hoặc mật khẩu không chính xác";
                    return View(model);
                }

                // Kiểm tra trạng thái tài khoản
                switch (user.IsActive)
                {
                    case 0:
                        TempData["Warning"] = "Tài khoản đang bị vô hiệu hóa";
                        return View(model);
                    case 2:
                        TempData["Warning"] = "Vui lòng xác nhận email của bạn trước khi đăng nhập.";
                        return View(model);
                }

                // Validate password
                if (!await _userService.ValidatePasswordAsync(user, model.Password))
                {
                    TempData["Warning"] = "Email hoặc mật khẩu không chính xác";
                    return View(model);
                }

                // Login successful
                var principal = _userService.CreateClaimsPrincipal(user);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTime.UtcNow.AddDays(model.RememberMe ? 30 : 1)
                    });

                return !string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl)
                    ? Redirect(model.ReturnUrl)
                    : RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                TempData["Warning"] = "Có lỗi xảy ra. Vui lòng thử lại.";
                return View(model);
            }
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
                        TempData["Warning"] = "Có lỗi xảy ra khi tạo tài khoản. Vui lòng thử lại.";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Register error: {ex.Message}");
                    TempData["Warning"] = "Có lỗi xảy ra khi tạo tài khoản. Vui lòng thử lại.";
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

            bool isValid = await TokenStore.ValidateTokenAsync(email, token, "EmailVerification");

            if (isValid)
            {
                var user = await _userService.GetUserByEmailAsync(email);
                if (user != null && user.IsActive == 2)
                {
                    user.IsActive = 1;
                    await _userService.UpdateUserAsync(user);

                    var principal = _userService.CreateClaimsPrincipal(user);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal,
                        new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTime.UtcNow.AddDays(30)
                        });

                    await TokenStore.RemoveTokenAsync(email, "EmailVerification");

                    // Hiển thị trang thành công với auto redirect
                    ViewBag.RedirectToHome = true;
                    return View("EmailVerified");
                }
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
                try
                {
                    var user = await _userService.GetUserByEmailAsync(model.Email);

                    // Kiểm tra user có tồn tại không (đã được đăng ký chưa)
                    if (user == null)
                    {
                        // Không tiết lộ thông tin email không tồn tại để bảo mật
                        // Nhưng log lại để admin theo dõi
                        
                        TempData["Warning"] = "Email này chưa được đăng ký trong hệ thống.";
                        return View(model);
                    }

                    // Kiểm tra trạng thái tài khoản
                    if (user.IsActive == 0)
                    {
                        TempData["Warning"] = "Tài khoản này đã bị vô hiệu hóa. Vui lòng liên hệ quản trị viên.";
                        return View(model);
                    }

                    if (user.IsActive == 2)
                    {
                        TempData["Warning"] = "Email này chưa được xác nhận. Vui lòng xác nhận email trước khi đặt lại mật khẩu.";
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
                        try
                        {
                            await _emailService.SendPasswordResetEmailAsync(model.Email, token);

                            // Debug URL - chỉ log trong development
                            var baseUrl = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host;
                            var resetUrl = $"{baseUrl}/Account/ResetPassword?email={Uri.EscapeDataString(model.Email)}&token={Uri.EscapeDataString(token)}";
                            Console.WriteLine($"Password Reset URL: {resetUrl}");

                            return View("ForgotPasswordConfirmation");
                        }
                        catch (Exception emailEx)
                        {
                          
                            TempData["Warning"] = "Có lỗi xảy ra khi gửi email. Vui lòng thử lại sau.";
                            return View(model);
                        }
                    }
                }
                catch (Exception ex)
                {
                
                    TempData["Warning"] = "Có lỗi xảy ra. Vui lòng thử lại sau.";
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
                try
                {
                    bool isValid = await TokenStore.ValidateTokenAsync(
                        model.Email, model.Token, "PasswordReset");

                    if (isValid)
                    {
                        var user = await _userService.GetUserByEmailAsync(model.Email);
                        if (user == null || user.IsActive != 1)
                        {
                            TempData["Warning"] = "Tài khoản không hợp lệ.";
                            return View(model);
                        }

                        // FIX: Sử dụng SQL thuần để update password
                        var newPasswordHash = _userService.HashPassword(model.Password);

                        // Cập nhật trực tiếp bằng SQL để tránh cache
                        var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                            "UPDATE Users SET PasswordHash = {0} WHERE Email = {1}",
                            newPasswordHash, model.Email);

                        if (rowsAffected > 0)
                        {
                            _context.ChangeTracker.Clear();

                            // XÓA TOKEN SAU KHI SỬ DỤNG THÀNH CÔNG
                            await TokenStore.RemoveTokenAsync(model.Email, "PasswordReset");

                            TempData["Success"] = "Mật khẩu đã được đặt lại thành công. Vui lòng đăng nhập bằng mật khẩu mới.";
                            return RedirectToAction("Login");
                        }
                        else
                        {
                            TempData["Warning"] = "Không thể cập nhật mật khẩu. Vui lòng thử lại.";
                        }
                    }
                    else
                    {
                        TempData["Warning"] = "Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ResetPassword error: {ex.Message}");
                    TempData["Warning"] = "Có lỗi xảy ra. Vui lòng thử lại.";
                }
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

            // FIX: Sử dụng IsActive thay vì TokenStore.IsEmailVerifiedAsync
            if (user != null && user.IsActive == 2) // Chưa xác thực email
            {
                // Generate a new verification token
                string token = GenerateRandomToken();

                // Store the token
                await TokenStore.StoreTokenAsync(model.Email, token, "EmailVerification");

                // Send verification email
                await _emailService.SendVerificationEmailAsync(model.Email, token);

                TempData["Success"] = "Email xác nhận đã được gửi lại thành công.";
            }
            else if (user != null && user.IsActive == 1)
            {
                TempData["Info"] = "Email này đã được xác nhận trước đó.";
            }
            else
            {
                TempData["Warning"] = "Email không tồn tại trong hệ thống.";
            }

            // Always show success message
            return View("ResendEmailConfirmationConfirmation");
        }
    }
}
