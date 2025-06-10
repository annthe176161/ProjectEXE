using System.Threading.Tasks;

namespace ProjectEXE.Services.Interfaces
{
    public interface IEmailService
    {
        /// <summary>
        /// Gửi email với nội dung tùy chỉnh
        /// </summary>
        /// <param name="email">Địa chỉ email của người nhận</param>
        /// <param name="subject">Tiêu đề email</param>
        /// <param name="message">Nội dung email (hỗ trợ HTML)</param>
        /// <returns>Task</returns>
        Task SendEmailAsync(string email, string subject, string message);

        /// <summary>
        /// Gửi email xác nhận địa chỉ email
        /// </summary>
        /// <param name="email">Địa chỉ email của người nhận</param>
        /// <param name="token">Token xác thực</param>
        /// <returns>Task</returns>
        Task SendVerificationEmailAsync(string email, string token);

        /// <summary>
        /// Gửi email đặt lại mật khẩu
        /// </summary>
        /// <param name="email">Địa chỉ email của người nhận</param>
        /// <param name="token">Token đặt lại mật khẩu</param>
        /// <returns>Task</returns>
        Task SendPasswordResetEmailAsync(string email, string token);

        /// <summary>
        /// Gửi email thông báo về voucher
        /// </summary>
        /// <param name="email">Địa chỉ email của người nhận</param>
        /// <param name="voucherCode">Mã voucher</param>
        /// <param name="discount">Phần trăm giảm giá</param>
        /// <param name="isNewUser">True nếu người nhận là người dùng mới đăng ký, False nếu là người đã giới thiệu</param>
        /// <returns>Task</returns>
        Task SendVoucherNotificationEmailAsync(string email, string voucherCode, int discount, bool isNewUser);
    }
}