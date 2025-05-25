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

        public async Task SendVerificationEmailAsync(string email, string token)
        {
            try
            {
                var baseUrl = _configuration["ApplicationUrl"];
                var verificationUrl = $"{baseUrl}/Account/VerifyEmail?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

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

        public async Task SendPasswordResetEmailAsync(string email, string token)
        {
            try
            {
                var baseUrl = _configuration["ApplicationUrl"];
                var resetUrl = $"{baseUrl}/Account/ResetPassword?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

                Console.WriteLine($"Password Reset URL: {resetUrl}");

                var subject = "Đặt lại mật khẩu - REVA";
                var body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='text-align: center; margin-bottom: 30px;'>
                            <h1 style='color: #007bff;'>REVA</h1>
                        </div>
                        
                        <h2 style='color: #333;'>Đặt lại mật khẩu</h2>
                        
                        <p>Xin chào,</p>
                        
                        <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản của mình. Vui lòng nhấp vào nút dưới đây để đặt lại mật khẩu:</p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetUrl}' style='background-color: #dc3545; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                                Đặt lại mật khẩu
                            </a>
                        </div>
                        
                        <p>Hoặc bạn có thể sao chép và dán liên kết sau vào trình duyệt:</p>
                        <p style='word-break: break-all; background: #f8f9fa; padding: 10px; border-radius: 4px;'>
                            {resetUrl}
                        </p>
                        
                        <p><strong>Lưu ý quan trọng:</strong></p>
                        <ul>
                            <li>Liên kết này sẽ hết hạn sau 1 giờ vì lý do bảo mật</li>
                            <li>Liên kết chỉ sử dụng được một lần</li>
                            <li>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này</li>
                        </ul>
                        
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
                Console.WriteLine($"Error sending password reset email: {ex.Message}");
                throw;
            }
        }
    }
}