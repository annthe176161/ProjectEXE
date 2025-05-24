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
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Email sending failed: {ex.Message}");
                throw;
            }
        }

        public async Task SendVerificationEmailAsync(string email, string token)
        {
            var baseUrl = _configuration["ApplicationUrl"];
            var verificationUrl = $"{baseUrl}/Account/VerifyEmail?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

            // Debug - hiển thị URL
            Console.WriteLine($"Verification URL: {verificationUrl}");

            var subject = "Xác nhận địa chỉ email";
            var body = $@"
                <h2>Xác nhận địa chỉ email</h2>
                <p>Vui lòng nhấp vào liên kết dưới đây để xác nhận địa chỉ email của bạn:</p>
                <p><a href='{verificationUrl}'>Xác nhận email</a></p>
                <p>Nếu bạn không thực hiện yêu cầu này, bạn có thể bỏ qua email này.</p>
            ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string email, string token)
        {
            var baseUrl = _configuration["ApplicationUrl"];
            var resetUrl = $"{baseUrl}/Account/ResetPassword?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

            // Debug - hiển thị URL
            Console.WriteLine($"Password Reset URL: {resetUrl}");

            var subject = "Đặt lại mật khẩu";
            var body = $@"
                <h2>Đặt lại mật khẩu</h2>
                <p>Bạn đã yêu cầu đặt lại mật khẩu. Vui lòng nhấp vào liên kết dưới đây để đặt lại mật khẩu của bạn:</p>
                <p><a href='{resetUrl}'>Đặt lại mật khẩu</a></p>
                <p>Nếu bạn không thực hiện yêu cầu này, bạn có thể bỏ qua email này.</p>
                <p>Liên kết này sẽ hết hạn sau 24 giờ.</p>
            ";

            await SendEmailAsync(email, subject, body);
        }
    }
}