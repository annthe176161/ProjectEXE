using Microsoft.Extensions.Configuration;
using ProjectEXE.Services.Interfaces;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ProjectEXE.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"];

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    EnableSsl = true
                };

                await client.SendMailAsync(mailMessage);
                Console.WriteLine($"Email sent successfully to {email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
                throw;
            }
        }

        public async Task SendVerificationEmailAsync(string email, string sessionId)
        {
            try
            {
                var baseUrl = _configuration["ApplicationUrl"];
                var verificationUrl = $"{baseUrl}/Account/VerifyEmail?email={Uri.EscapeDataString(email)}&sessionId={Uri.EscapeDataString(sessionId)}";

                Console.WriteLine($"Verification URL: {verificationUrl}");

                var subject = "Xác nhận địa chỉ email - REVA";
                var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='text-align: center; margin-bottom: 30px;'>
                    <h1 style='color: #007bff;'>REVA</h1>
                </div>
                
                <h2 style='color: #333;'>Xác nhận địa chỉ email</h2>
                
                <p>Xin chào,</p>
                
                <p>Cảm ơn bạn đã đăng ký tài khoản REVA. Để hoàn tất quá trình đăng ký, vui lòng xác nhận địa chỉ email của bạn bằng cách nhấp vào nút dưới đây:</p>
                
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{verificationUrl}' style='background-color: #007bff; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                        Xác nhận email
                    </a>
                </div>
                
                <p>Hoặc bạn có thể sao chép và dán liên kết sau vào trình duyệt:</p>
                <p style='word-break: break-all; background: #f8f9fa; padding: 10px; border-radius: 4px;'>
                    {verificationUrl}
                </p>
                
                <p><strong>Lưu ý:</strong> Liên kết này sẽ hết hạn sau 1 giờ vì lý do bảo mật.</p>
                
                <p>Nếu bạn không thực hiện yêu cầu này, bạn có thể bỏ qua email này.</p>
                
                <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                
                <p style='color: #666; font-size: 12px; text-align: center;'>
                    Email này được gửi tự động từ hệ thống REVA. Vui lòng không trả lời email này.
                </p>
            </div>
        ";

                await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending verification email: {ex.Message}");
                throw;
            }
        }

        public async Task SendPasswordResetEmailAsync(string email, string sessionId)
        {
            try
            {
                var baseUrl = _configuration["ApplicationUrl"];
                var resetUrl = $"{baseUrl}/Account/ResetPassword?email={Uri.EscapeDataString(email)}&sessionId={Uri.EscapeDataString(sessionId)}";

                Console.WriteLine($"📧 Password Reset URL: {resetUrl}");

                var subject = "Đặt lại mật khẩu - REVA";
                var body = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='text-align: center; margin-bottom: 30px;'>
                    <h1 style='color: #007bff;'>REVA</h1>
                </div>
                
                <h2 style='color: #333;'>Đặt lại mật khẩu</h2>
                
                <p>Xin chào,</p>
                
                <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn. Để tiếp tục, vui lòng nhấp vào nút dưới đây:</p>
                
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{resetUrl}' style='background-color: #28a745; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                        Đặt lại mật khẩu
                    </a>
                </div>
                
                <p>Hoặc bạn có thể sao chép và dán liên kết sau vào trình duyệt:</p>
                <p style='word-break: break-all; background: #f8f9fa; padding: 10px; border-radius: 4px;'>
                    {resetUrl}
                </p>
                
                <p><strong>Lưu ý quan trọng:</strong></p>
                <ul>
                    <li>Liên kết này chỉ có hiệu lực trong <strong>1 giờ</strong></li>
                    <li>Sau khi sử dụng, liên kết sẽ bị vô hiệu hóa</li>
                    <li>Nếu bạn không yêu cầu đặt lại mật khẩu, bạn có thể bỏ qua email này</li>
                </ul>
                
                <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                
                <p style='color: #666; font-size: 12px; text-align: center;'>
                    Email này được gửi tự động từ hệ thống REVA. Vui lòng không trả lời email này.<br>
                    Nếu bạn gặp vấn đề, vui lòng liên hệ với chúng tôi.
                </p>
            </div>
        ";

                await SendEmailAsync(email, subject, body);
                Console.WriteLine($"✅ Đã gửi email reset password tới: {email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi gửi email reset password: {ex.Message}");
                throw;
            }
        }

        // Thêm phương thức này vào EmailService.cs
        public async Task SendVoucherNotificationEmailAsync(string email, string voucherCode, int discount, bool isNewUser)
        {
            string subject = isNewUser
                ? $"Chào mừng bạn đến với REVA - Tặng voucher {discount}% cho đơn hàng đầu tiên"
                : $"Cảm ơn vì đã giới thiệu thành viên mới - Nhận voucher {discount}% từ REVA";

            string message = isNewUser
                ? $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='text-align: center; margin-bottom: 30px;'>
                    <h1 style='color: #007bff;'>REVA</h1>
                </div>
                
                <h2 style='color: #333;'>Chào mừng bạn đã đăng ký thành công!</h2>
                
                <p>Xin chào,</p>
                
                <p>Cảm ơn bạn đã đăng ký tài khoản tại REVA. Để chào mừng bạn, chúng tôi tặng bạn voucher giảm giá đặc biệt:</p>
                
                <div style='background-color: #f7f7f7; padding: 20px; border-radius: 10px; margin: 20px 0; text-align: center;'>
                    <h3 style='margin: 0 0 10px 0;'>MÃ VOUCHER CỦA BẠN</h3>
                    <div style='font-size: 24px; font-weight: bold; color: #28a745; padding: 10px; background: #ffffff; border-radius: 5px; border: 2px dashed #28a745;'>
                        {voucherCode}
                    </div>
                    <p style='margin-top: 15px;'>Giảm <strong>{discount}%</strong> cho đơn hàng đầu tiên!</p>
                    <p style='font-size: 12px; color: #666;'>Hạn sử dụng: {DateTime.Now.AddDays(14).ToString("dd/MM/yyyy")}</p>
                </div>
                
                <p>Khám phá ngay hàng ngàn sản phẩm chất lượng trên REVA và sử dụng voucher của bạn để nhận ưu đãi!</p>
                
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{_configuration["ApplicationUrl"]}' style='background-color: #007bff; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                        Khám phá ngay
                    </a>
                </div>
                
                <p>Xin cảm ơn và chúc bạn có trải nghiệm tuyệt vời tại REVA!</p>
                
                <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                
                <p style='color: #666; font-size: 12px; text-align: center;'>
                    Email này được gửi tự động từ hệ thống REVA. Vui lòng không trả lời email này.
                </p>
            </div>"
                : $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='text-align: center; margin-bottom: 30px;'>
                    <h1 style='color: #007bff;'>REVA</h1>
                </div>
                
                <h2 style='color: #333;'>Cảm ơn vì đã giới thiệu thành viên mới!</h2>
                
                <p>Xin chào,</p>
                
                <p>Chúc mừng! Một người dùng mới vừa đăng ký sử dụng mã giới thiệu của bạn. Để cảm ơn bạn, chúng tôi tặng bạn voucher đặc biệt:</p>
                
                <div style='background-color: #f7f7f7; padding: 20px; border-radius: 10px; margin: 20px 0; text-align: center;'>
                    <h3 style='margin: 0 0 10px 0;'>MÃ VOUCHER CỦA BẠN</h3>
                    <div style='font-size: 24px; font-weight: bold; color: #007bff; padding: 10px; background: #ffffff; border-radius: 5px; border: 2px dashed #007bff;'>
                        {voucherCode}
                    </div>
                    <p style='margin-top: 15px;'>Giảm <strong>{discount}%</strong> cho đơn hàng tiếp theo!</p>
                    <p style='font-size: 12px; color: #666;'>Hạn sử dụng: {DateTime.Now.AddDays(14).ToString("dd/MM/yyyy")}</p>
                </div>
                
                <p>Tiếp tục giới thiệu bạn bè để nhận thêm nhiều ưu đãi hấp dẫn!</p>
                
                <div style='text-align: center; margin: 30px 0;'>
                    <a href='{_configuration["ApplicationUrl"]}' style='background-color: #007bff; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                        Mua sắm ngay
                    </a>
                </div>
                
                <p>Xin cảm ơn vì đã là thành viên trung thành của REVA!</p>
                
                <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>
                
                <p style='color: #666; font-size: 12px; text-align: center;'>
                    Email này được gửi tự động từ hệ thống REVA. Vui lòng không trả lời email này.
                </p>
            </div>";

            await SendEmailAsync(email, subject, message);
        }
    }
}