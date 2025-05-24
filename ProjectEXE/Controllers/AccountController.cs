using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel.AccountViewModel;
using ProjectEXE.Services;
using ProjectEXE.Services.TokenStorage;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ProjectEXE.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;

        public AccountController(IUserService userService, IEmailService emailService)
        {
            _userService = userService;
            _emailService = emailService;
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
                    // Kiểm tra email đã được xác thực chưa
                    //if (await TokenStore.IsEmailVerifiedAsync(user.Email) == false)
                    //{
                    //    TempData["Warning"] = "Vui lòng xác nhận email của bạn trước khi đăng nhập.";
                    //    return View(model);
                    //}

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

                // Create the user
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

            // Validate the token
            bool isValid = await TokenStore.ValidateTokenAsync(email, token, "EmailVerification");

            if (isValid)
            {
                // Mark the email as verified
                await TokenStore.MarkEmailAsVerifiedAsync(email);

                // Get the user and sign in automatically
                var user = await _userService.GetUserByEmailAsync(email);
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
                }

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

                if (user != null)
                {
                    // Generate token
                    string token = GenerateRandomToken();

                    // Store the token
                    await TokenStore.StoreTokenAsync(model.Email, token, "PasswordReset");

                    // Send email
                    await _emailService.SendPasswordResetEmailAsync(model.Email, token);
                }

                // Always show success page (even if email doesn't exist) to prevent email enumeration
                return View("ForgotPasswordConfirmation");
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
                // Validate token
                bool isValid = await TokenStore.ValidateTokenAsync(
                    model.Email, model.Token, "PasswordReset");

                if (isValid)
                {
                    // Get the user
                    var user = await _userService.GetUserByEmailAsync(model.Email);

                    if (user != null)
                    {
                        // Update password
                        user.PasswordHash = _userService.HashPassword(model.Password);
                        await _userService.UpdateUserAsync(user);

                        return View("ResetPasswordConfirmation");
                    }
                }

                // If we get here, something failed
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