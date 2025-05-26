using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectEXE.Models;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel.AccountViewModel;

namespace ProjectEXE.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly ISessionService _sessionService;
        private readonly RevaContext _context;

        public AccountController(IUserService userService, IEmailService emailService, ISessionService sessionService, RevaContext context)
        {
            _userService = userService;
            _emailService = emailService;
            _sessionService = sessionService;
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
                        // Generate a unique session ID instead of token
                        string sessionId = GenerateSessionId();

                        // Store email verification session
                        await _sessionService.StoreEmailVerificationAsync(sessionId, model.Email);

                        // Send verification email with session ID
                        await _emailService.SendVerificationEmailAsync(model.Email, sessionId);

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

        public async Task<IActionResult> VerifyEmail(string email, string sessionId)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(sessionId))
            {
                return BadRequest("Email hoặc session không hợp lệ");
            }

            // Validate session
            bool isValid = await _sessionService.ValidateAndGetEmailAsync(sessionId, email, "EmailVerify");

            if (isValid)
            {
                var user = await _userService.GetUserByEmailAsync(email);
                if (user != null && user.IsActive == 2) // Chưa verify
                {
                    user.IsActive = 1; // Kích hoạt tài khoản
                    await _userService.UpdateUserAsync(user);

                    // Tự động đăng nhập
                    var principal = _userService.CreateClaimsPrincipal(user);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    // Xóa session
                    await _sessionService.RemoveSessionAsync(sessionId, "EmailVerify");

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
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Kiểm tra email có tồn tại không
                var user = await _userService.GetUserByEmailAsync(model.Email);
                if (user == null)
                {
                    TempData["Warning"] = "Email này không tồn tại trong hệ thống.";
                    return View(model);
                }

                if (user.IsActive == 0)
                {
                    TempData["Warning"] = "Tài khoản đã bị vô hiệu hóa. Vui lòng liên hệ admin.";
                    return View(model);
                }

                if (user.IsActive == 2)
                {
                    TempData["Warning"] = "Tài khoản chưa được xác nhận email. Vui lòng xác nhận email trước khi reset mật khẩu.";
                    TempData["ShowResendLink"] = true; // Để hiển thị link gửi lại email xác nhận
                    return View(model);
                }


                if (user.IsActive == 1) // Chỉ user đã kích hoạt mới được reset password
                {
                    // Tạo session ID thay vì token
                    string sessionId = Guid.NewGuid().ToString("N");

                    // Lưu session cho password reset
                    await _sessionService.StorePasswordResetAsync(sessionId, model.Email);

                    // Gửi email reset password
                    await _emailService.SendPasswordResetEmailAsync(model.Email, sessionId);

                    Console.WriteLine($"✅ Đã tạo password reset session: {sessionId} cho email: {model.Email}");
                }

                // Luôn hiển thị thông báo thành công để bảo mật (không tiết lộ email có tồn tại hay không)
                TempData["Success"] = "Nếu email tồn tại trong hệ thống, chúng tôi đã gửi liên kết reset mật khẩu.";
                return RedirectToAction("ForgotPasswordConfirmation");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi ForgotPassword: {ex.Message}");
                TempData["Warning"] = "Có lỗi xảy ra. Vui lòng thử lại.";
                return View(model);
            }
        }
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string email, string sessionId)
        {
            Console.WriteLine($"🔑 ResetPassword GET: email={email}, sessionId={sessionId}");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(sessionId))
            {
                Console.WriteLine("❌ Email hoặc sessionId không hợp lệ");
                return BadRequest("Email hoặc session không hợp lệ");
            }

            // Validate session
            bool isValid = await _sessionService.ValidateAndGetEmailAsync(sessionId, email, "PasswordReset");

            if (!isValid)
            {
                Console.WriteLine($"❌ Session không hợp lệ: {sessionId}");
                TempData["Warning"] = "Liên kết reset mật khẩu không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction("ForgotPassword");
            }

            // Kiểm tra user có tồn tại không
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null || user.IsActive != 1)
            {
                Console.WriteLine($"❌ User không hợp lệ: {email}");
                TempData["Warning"] = "Tài khoản không hợp lệ.";
                return RedirectToAction("ForgotPassword");
            }

            var model = new ResetPasswordViewModel
            {
                Email = email,
                SessionId = sessionId
            };

            Console.WriteLine($"✅ Hiển thị form reset password cho: {email}");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                // Validate + Get user + Update password (như trên)
                if (!await _sessionService.ValidateAndGetEmailAsync(model.SessionId, model.Email, "PasswordReset"))
                {
                    TempData["Warning"] = "Liên kết không hợp lệ.";
                    return RedirectToAction("ForgotPassword");
                }

                _context.ChangeTracker.Clear();
                var user = await _context.Users.Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

                if (user == null)
                {
                    TempData["Warning"] = "Tài khoản không tồn tại.";
                    return RedirectToAction("ForgotPassword");
                }

                user.PasswordHash = _userService.HashPassword(model.NewPassword);
                user.IsActive = 1;
                await _context.SaveChangesAsync();

                // ✅ TỰ ĐỘNG ĐĂNG NHẬP LUÔN
                var principal = _userService.CreateClaimsPrincipal(user);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                await _sessionService.RemoveSessionAsync(model.SessionId, "PasswordReset");

                TempData["Success"] = "Mật khẩu đã được đặt lại và bạn đã được đăng nhập!";
                return RedirectToAction("Index", "Home"); // ✅ THẲNG VỀ HOME
            }
            catch (Exception ex)
            {
                TempData["Warning"] = "Có lỗi xảy ra.";
                return View(model);
            }
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

            if (user != null && user.IsActive == 2) // Chưa verify
            {
                // Tạo session ID mới
                string sessionId = Guid.NewGuid().ToString("N");

                // Lưu session mới
                await _sessionService.StoreEmailVerificationAsync(sessionId, model.Email);

                // Gửi email
                await _emailService.SendVerificationEmailAsync(model.Email, sessionId);

                TempData["Success"] = "Email xác nhận đã được gửi lại.";
            }
            else if (user != null && user.IsActive == 1)
            {
                TempData["Info"] = "Email đã được xác nhận trước đó.";
            }
            else
            {
                TempData["Warning"] = "Email không tồn tại.";
            }

            return View("ResendEmailConfirmationConfirmation");
        }


        private string GenerateSessionId()
        {
            return Guid.NewGuid().ToString("N") + DateTime.UtcNow.Ticks.ToString("x");
        }
        [HttpGet]
        public async Task<IActionResult> CheckUserStatus(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { error = "Email is required" });
            }

            _context.ChangeTracker.Clear();
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null)
            {
                return Json(new { error = "User not found" });
            }

            return Json(new
            {
                email = user.Email,
                isActive = user.IsActive,
                userId = user.UserId,
                createdAt = user.CreatedAt,
                roleId = user.RoleId
            });
        }
    }
}
