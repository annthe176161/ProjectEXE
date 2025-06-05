using ProjectEXE.Services.Interfaces;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace ProjectEXE.Services.Implementations
{
    public class OrderEmailService : IOrderEmailService
    {
        private readonly IConfiguration _configuration;

        public OrderEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendOrderConfirmationNotificationAsync(int orderId, string productName, decimal price, string shopName, string buyerName, string buyerEmail, string sellerName, string sellerEmail)
        {
            Console.WriteLine($"🚀 Bắt đầu gửi email thông báo đặt hàng cho đơn #{orderId}");

            try
            {
                // Email cho người mua
                Console.WriteLine($"📧 Gửi email thông báo cho người mua: {buyerEmail}");
                var buyerSubject = $"[REVA] Đặt hàng thành công - Đơn hàng #{orderId}";
                var buyerBody = GenerateOrderConfirmationEmailForBuyer(orderId, productName, price, shopName, buyerName);
                await SendOrderEmailAsync(buyerEmail, buyerSubject, buyerBody);

                // Email cho người bán
                Console.WriteLine($"📧 Gửi email thông báo cho người bán: {sellerEmail}");
                var sellerSubject = $"[REVA] Có đơn hàng mới - Đơn hàng #{orderId}";
                var sellerBody = GenerateOrderConfirmationEmailForSeller(orderId, productName, price, buyerName, sellerName);
                await SendOrderEmailAsync(sellerEmail, sellerSubject, sellerBody);

                Console.WriteLine($"✅ Hoàn thành gửi email thông báo đặt hàng cho đơn #{orderId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi gửi email thông báo đặt hàng cho đơn #{orderId}: {ex.Message}");
            }
        }

        public async Task SendOrderCancellationNotificationAsync(int orderId, string productName, decimal price, string shopName, string buyerName, string buyerEmail, string sellerName, string sellerEmail, string cancelReason)
        {
            Console.WriteLine($"🚀 Bắt đầu gửi email thông báo hủy đơn cho đơn #{orderId}");

            try
            {
                // Email cho người mua
                Console.WriteLine($"📧 Gửi email thông báo hủy đơn cho người mua: {buyerEmail}");
                var buyerSubject = $"[REVA] Đơn hàng đã hủy - Đơn hàng #{orderId}";
                var buyerBody = GenerateOrderCancellationEmailForBuyer(orderId, productName, price, shopName, buyerName, cancelReason);
                await SendOrderEmailAsync(buyerEmail, buyerSubject, buyerBody);

                // Email cho người bán
                Console.WriteLine($"📧 Gửi email thông báo hủy đơn cho người bán: {sellerEmail}");
                var sellerSubject = $"[REVA] Đơn hàng bị hủy - Đơn hàng #{orderId}";
                var sellerBody = GenerateOrderCancellationEmailForSeller(orderId, productName, price, buyerName, sellerName, cancelReason);
                await SendOrderEmailAsync(sellerEmail, sellerSubject, sellerBody);

                Console.WriteLine($"✅ Hoàn thành gửi email thông báo hủy đơn cho đơn #{orderId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi gửi email thông báo hủy đơn cho đơn #{orderId}: {ex.Message}");
            }
        }

        // Phương thức private để gửi email - riêng cho OrderEmailService
        private async Task SendOrderEmailAsync(string email, string subject, string htmlBody)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"];

                // Log thêm thông tin để debug
                Console.WriteLine($"📧 Cấu hình gửi email: Host={smtpHost}, Port={smtpPort}, User={smtpUsername}");

                using var smtpClient = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword)
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true,
                    BodyEncoding = Encoding.UTF8,
                    SubjectEncoding = Encoding.UTF8
                };

                mailMessage.To.Add(email);

                // Thử đặt thời gian timeout lâu hơn
                smtpClient.Timeout = 30000; // 30 giây

                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine($"✅ Email đơn hàng sent successfully to {email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Email đơn hàng sending failed to {email}: {ex.Message}");
                Console.WriteLine($"Chi tiết lỗi: {ex}");
                // Không throw exception để không ảnh hưởng đến flow chính
            }
        }

        private string GenerateOrderConfirmationEmailForBuyer(int orderId, string productName, decimal price, string shopName, string buyerName)
        {
            var applicationUrl = _configuration["ApplicationUrl"];

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #28a745, #20c997); color: white; padding: 30px; text-align: center; }}
        .content {{ padding: 30px; }}
        .order-info {{ background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .footer {{ background-color: #6c757d; color: white; padding: 20px; text-align: center; }}
        .btn {{ background-color: #28a745; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block; margin-top: 15px; }}
        .highlight {{ color: #28a745; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Đặt hàng thành công!</h1>
            <p>Cảm ơn bạn đã sử dụng dịch vụ REVA</p>
        </div>
        
        <div class='content'>
            <h2>Xin chào {buyerName},</h2>
            <p>Đơn hàng của bạn đã được đặt thành công và đang chờ người bán xác nhận.</p>
            
            <div class='order-info'>
                <h3>📦 Thông tin đơn hàng</h3>
                <p><strong>Mã đơn hàng:</strong> <span class='highlight'>#{orderId}</span></p>
                <p><strong>Sản phẩm:</strong> {productName}</p>
                <p><strong>Giá:</strong> <span class='highlight'>{price:N0}₫</span></p>
                <p><strong>Cửa hàng:</strong> {shopName}</p>
                <p><strong>Ngày đặt:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                <p><strong>Trạng thái:</strong> <span style='color: #ffc107;'>Chờ xác nhận</span></p>
            </div>
            
            <h3>📋 Quy trình tiếp theo:</h3>
            <ol>
                <li>Người bán sẽ xem xét và xác nhận đơn hàng trong vòng 24 giờ</li>
                <li>Sau khi xác nhận, thông tin liên hệ sẽ được chia sẻ giữa hai bên</li>
                <li>Bạn và người bán sẽ thống nhất về phương thức thanh toán và giao nhận</li>
            </ol>
            
            <p style='background-color: #e7f3ff; padding: 15px; border-left: 4px solid #007bff; border-radius: 4px;'>
                <strong>💡 Lưu ý:</strong> REVA là nền tảng kết nối. Việc thanh toán và giao nhận sẽ được thực hiện trực tiếp giữa bạn và người bán.
            </p>
            
            <div style='text-align: center;'>
                <a href='{applicationUrl}/Order/OrderDetails/{orderId}' class='btn'>Xem chi tiết đơn hàng</a>
            </div>
        </div>
        
        <div class='footer'>
            <p>Đây là email tự động từ hệ thống REVA. Vui lòng không trả lời email này.</p>
            <p>© 2024 REVA - Nền tảng mua bán trực tuyến</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateOrderConfirmationEmailForSeller(int orderId, string productName, decimal price, string buyerName, string sellerName)
        {
            var applicationUrl = _configuration["ApplicationUrl"];

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #007bff, #0056b3); color: white; padding: 30px; text-align: center; }}
        .content {{ padding: 30px; }}
        .order-info {{ background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .footer {{ background-color: #6c757d; color: white; padding: 20px; text-align: center; }}
        .btn {{ background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block; margin-top: 15px; }}
        .highlight {{ color: #007bff; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🛍️ Đơn hàng mới!</h1>
            <p>Bạn có một đơn hàng mới cần xác nhận</p>
        </div>
        
        <div class='content'>
            <h2>Xin chào {sellerName},</h2>
            <p>Bạn có một đơn hàng mới từ khách hàng cần được xác nhận.</p>
            
            <div class='order-info'>
                <h3>📦 Thông tin đơn hàng</h3>
                <p><strong>Mã đơn hàng:</strong> <span class='highlight'>#{orderId}</span></p>
                <p><strong>Sản phẩm:</strong> {productName}</p>
                <p><strong>Giá:</strong> <span class='highlight'>{price:N0}₫</span></p>
                <p><strong>Khách hàng:</strong> {buyerName}</p>
                <p><strong>Ngày đặt:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
            </div>
            
            <h3>✅ Bước tiếp theo:</h3>
            <ol>
                <li>Đăng nhập vào hệ thống REVA để xem chi tiết đơn hàng</li>
                <li>Xác nhận hoặc từ chối đơn hàng</li>
                <li>Liên hệ trực tiếp với khách hàng để thống nhất giao dịch</li>
            </ol>
            
            <p style='background-color: #fff3cd; padding: 15px; border-left: 4px solid #ffc107; border-radius: 4px;'>
                <strong>⏰ Quan trọng:</strong> Vui lòng xác nhận đơn hàng trong vòng 24 giờ để đảm bảo trải nghiệm tốt cho khách hàng.
            </p>
            
            <div style='text-align: center;'>
                <a href='{applicationUrl}/Seller/OrderManagement' class='btn'>Quản lý đơn hàng</a>
            </div>
        </div>
        
        <div class='footer'>
            <p>Đây là email tự động từ hệ thống REVA. Vui lòng không trả lời email này.</p>
            <p>© 2024 REVA - Nền tảng mua bán trực tuyến</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateOrderCancellationEmailForBuyer(int orderId, string productName, decimal price, string shopName, string buyerName, string cancelReason)
        {
            var applicationUrl = _configuration["ApplicationUrl"];

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #dc3545, #c82333); color: white; padding: 30px; text-align: center; }}
        .content {{ padding: 30px; }}
        .order-info {{ background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .footer {{ background-color: #6c757d; color: white; padding: 20px; text-align: center; }}
        .btn {{ background-color: #28a745; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block; margin-top: 15px; }}
        .highlight {{ color: #dc3545; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>❌ Đơn hàng đã hủy</h1>
            <p>Thông báo hủy đơn hàng</p>
        </div>
        
        <div class='content'>
            <h2>Xin chào {buyerName},</h2>
            <p>Đơn hàng #{orderId} của bạn đã được hủy thành công.</p>
            
            <div class='order-info'>
                <h3>📦 Thông tin đơn hàng đã hủy</h3>
                <p><strong>Mã đơn hàng:</strong> <span class='highlight'>#{orderId}</span></p>
                <p><strong>Sản phẩm:</strong> {productName}</p>
                <p><strong>Giá:</strong> {price:N0}₫</p>
                <p><strong>Cửa hàng:</strong> {shopName}</p>
                <p><strong>Lý do hủy:</strong> {cancelReason}</p>
                <p><strong>Thời gian hủy:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
            </div>
            
            <p>Cảm ơn bạn đã sử dụng dịch vụ REVA. Hy vọng bạn sẽ tìm thấy sản phẩm phù hợp khác trên nền tảng của chúng tôi.</p>
            
            <div style='text-align: center;'>
                <a href='{applicationUrl}/Product/ProductList' class='btn'>Tiếp tục mua sắm</a>
            </div>
        </div>
        
        <div class='footer'>
            <p>Đây là email tự động từ hệ thống REVA. Vui lòng không trả lời email này.</p>
            <p>© 2024 REVA - Nền tảng mua bán trực tuyến</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateOrderCancellationEmailForSeller(int orderId, string productName, decimal price, string buyerName, string sellerName, string cancelReason)
        {
            var applicationUrl = _configuration["ApplicationUrl"];

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #ffc107, #e0a800); color: white; padding: 30px; text-align: center; }}
        .content {{ padding: 30px; }}
        .order-info {{ background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .footer {{ background-color: #6c757d; color: white; padding: 20px; text-align: center; }}
        .btn {{ background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block; margin-top: 15px; }}
        .highlight {{ color: #ffc107; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>⚠️ Đơn hàng bị hủy</h1>
            <p>Thông báo hủy đơn hàng từ khách hàng</p>
        </div>
        
        <div class='content'>
            <h2>Xin chào {sellerName},</h2>
            <p>Đơn hàng #{orderId} đã bị khách hàng hủy.</p>
            
            <div class='order-info'>
                <h3>📦 Thông tin đơn hàng bị hủy</h3>
                <p><strong>Mã đơn hàng:</strong> <span class='highlight'>#{orderId}</span></p>
                <p><strong>Sản phẩm:</strong> {productName}</p>
                <p><strong>Giá:</strong> {price:N0}₫</p>
                <p><strong>Khách hàng:</strong> {buyerName}</p>
                <p><strong>Lý do hủy:</strong> {cancelReason}</p>
                <p><strong>Thời gian hủy:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
            </div>
            
            <p>Sản phẩm của bạn sẽ tiếp tục được hiển thị trên REVA để các khách hàng khác có thể đặt mua.</p>
            
            <div style='text-align: center;'>
                <a href='{applicationUrl}/Seller/OrderManagement' class='btn'>Quản lý đơn hàng</a>
            </div>
        </div>
        
        <div class='footer'>
            <p>Đây là email tự động từ hệ thống REVA. Vui lòng không trả lời email này.</p>
            <p>© 2024 REVA - Nền tảng mua bán trực tuyến</p>
        </div>
    </div>
</body>
</html>";
        }

        public async Task SendOrderStatusUpdateNotificationAsync(int orderId, string productName, decimal price, string shopName, string buyerName, string buyerEmail, int oldStatusId, int newStatusId, string cancelReason = null)
        {
            Console.WriteLine($"🚀 Bắt đầu gửi email thông báo cập nhật trạng thái cho đơn #{orderId}: {oldStatusId} -> {newStatusId}");

            try
            {
                // Bỏ qua nếu trạng thái không thay đổi
                if (oldStatusId == newStatusId)
                {
                    Console.WriteLine($"⚠️ Không gửi email vì trạng thái không đổi (vẫn là {GetStatusName(newStatusId)})");
                    return;
                }

                // Chúng ta chỉ gửi email cho người mua
                Console.WriteLine($"📧 Gửi email thông báo cập nhật trạng thái cho người mua: {buyerEmail}");
                var subject = $"[REVA] Cập nhật đơn hàng #{orderId} - {GetStatusName(newStatusId)}";
                var body = GenerateOrderStatusUpdateEmail(orderId, productName, price, shopName, buyerName, oldStatusId, newStatusId, cancelReason);
                await SendOrderEmailAsync(buyerEmail, subject, body);

                Console.WriteLine($"✅ Hoàn thành gửi email thông báo cập nhật trạng thái cho đơn #{orderId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi gửi email thông báo cập nhật trạng thái cho đơn #{orderId}: {ex.Message}");
            }
        }

        private string GenerateOrderStatusUpdateEmail(int orderId, string productName, decimal price, string shopName, string buyerName, int oldStatusId, int newStatusId, string cancelReason = null)
        {
            var applicationUrl = _configuration["ApplicationUrl"];
            string statusUpdateMessage = GetStatusUpdateMessage(oldStatusId, newStatusId);
            string nextStepsMessage = GetNextStepsMessage(newStatusId);

            // Màu chủ đạo theo trạng thái mới
            string headerColor = GetStatusHeaderColor(newStatusId);
            string statusColor = GetStatusColor(newStatusId);
            string statusIcon = GetStatusIcon(newStatusId);

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background: {headerColor}; color: white; padding: 30px; text-align: center; }}
        .content {{ padding: 30px; }}
        .order-info {{ background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .footer {{ background-color: #6c757d; color: white; padding: 20px; text-align: center; }}
        .btn {{ background-color: {statusColor}; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block; margin-top: 15px; }}
        .highlight {{ color: {statusColor}; font-weight: bold; }}
        .status-timeline {{ margin: 20px 0; padding: 10px; background-color: #fff; border-radius: 8px; }}
        .status-step {{ padding: 10px 15px; margin: 5px 0; border-radius: 5px; position: relative; }}
        .status-inactive {{ background-color: #f5f5f5; color: #6c757d; }}
        .status-active {{ background-color: #e8f5e9; border-left: 4px solid {statusColor}; color: #333; }}
        .status-current {{ font-weight: bold; }}
        .status-icon {{ margin-right: 10px; }}
        .alert {{ padding: 15px; margin: 15px 0; border-radius: 5px; }}
        .alert-info {{ background-color: #e7f3fe; border-left: 4px solid #2196F3; }}
        .alert-warning {{ background-color: #fff3cd; border-left: 4px solid #ffc107; }}
        .alert-danger {{ background-color: #ffebee; border-left: 4px solid #f44336; }}
        .alert-success {{ background-color: #e8f5e9; border-left: 4px solid #4caf50; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{statusIcon} {GetStatusTitleForEmail(newStatusId)}</h1>
            <p>Trạng thái đơn hàng đã được cập nhật</p>
        </div>
        
        <div class='content'>
            <h2>Xin chào {buyerName},</h2>
            <p>{statusUpdateMessage}</p>
            
            <div class='order-info'>
                <h3>📦 Thông tin đơn hàng</h3>
                <p><strong>Mã đơn hàng:</strong> <span class='highlight'>#{orderId}</span></p>
                <p><strong>Sản phẩm:</strong> {productName}</p>
                <p><strong>Giá:</strong> <span class='highlight'>{price:N0}₫</span></p>
                <p><strong>Cửa hàng:</strong> {shopName}</p>
                <p><strong>Thời gian cập nhật:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                <p><strong>Trạng thái mới:</strong> <span class='highlight'>{GetStatusName(newStatusId)}</span></p>
            </div>
            
            <div class='status-timeline'>
                <h3>🔄 Quy trình đơn hàng:</h3>
                <div class='status-step {(newStatusId >= 1 ? "status-active" : "status-inactive")} {(newStatusId == 1 ? "status-current" : "")}'>
                    <span class='status-icon'>1️⃣</span> Chờ xác nhận
                    {(newStatusId == 1 ? "<span class='highlight'> (Hiện tại)</span>" : "")}
                </div>
                <div class='status-step {(newStatusId >= 2 ? "status-active" : "status-inactive")} {(newStatusId == 2 ? "status-current" : "")}'>
                    <span class='status-icon'>2️⃣</span> Đã xác nhận
                    {(newStatusId == 2 ? "<span class='highlight'> (Hiện tại)</span>" : "")}
                </div>
                <div class='status-step {(newStatusId >= 3 ? "status-active" : "status-inactive")} {(newStatusId == 3 ? "status-current" : "")}'>
                    <span class='status-icon'>3️⃣</span> Đang giao hàng
                    {(newStatusId == 3 ? "<span class='highlight'> (Hiện tại)</span>" : "")}
                </div>
                <div class='status-step {(newStatusId >= 4 ? "status-active" : "status-inactive")} {(newStatusId == 4 ? "status-current" : "")}'>
                    <span class='status-icon'>4️⃣</span> Đã giao - Hoàn thành
                    {(newStatusId == 4 ? "<span class='highlight'> (Hiện tại)</span>" : "")}
                </div>
                {(newStatusId == 5 ? $@"<div class='status-step status-active status-current' style='border-left: 4px solid #f44336;'>
                    <span class='status-icon'>❌</span> Đã hủy <span class='highlight'> (Hiện tại)</span>
                </div>" : "")}
            </div>
            
            {(newStatusId == 5 && !string.IsNullOrEmpty(cancelReason) ? $@"
            <div class='alert alert-danger'>
                <h4 style='color: #d32f2f;'>⚠️ Lý do hủy đơn hàng:</h4>
                <p>{cancelReason}</p>
            </div>" : "")}
            
            <div class='alert {GetAlertClassForStatus(newStatusId)}'>
                <h4>💬 {GetStatusAlertTitle(newStatusId)}</h4>
                <p>{nextStepsMessage}</p>
            </div>
            
            <div style='text-align: center;'>
                <a href='{applicationUrl}/Order/OrderDetails/{orderId}' class='btn'>Xem chi tiết đơn hàng</a>
            </div>
        </div>
        
        <div class='footer'>
            <p>Đây là email tự động từ hệ thống REVA. Vui lòng không trả lời email này.</p>
            <p>© {DateTime.Now.Year} REVA - Nền tảng mua bán trực tuyến</p>
        </div>
    </div>
</body>
</html>";
        }

        /// <summary>
        /// Lấy tên trạng thái đơn hàng
        /// </summary>
        private string GetStatusName(int statusId)
        {
            return statusId switch
            {
                1 => "Chờ xác nhận",
                2 => "Đã xác nhận",
                3 => "Đang giao hàng",
                4 => "Đã giao - Hoàn thành",
                5 => "Đã hủy",
                _ => "Không xác định"
            };
        }

        /// <summary>
        /// Lấy tiêu đề email theo trạng thái
        /// </summary>
        private string GetStatusTitleForEmail(int statusId)
        {
            return statusId switch
            {
                1 => "Đơn hàng đang chờ xác nhận",
                2 => "Đơn hàng đã được xác nhận",
                3 => "Đơn hàng đang được giao",
                4 => "Đơn hàng đã hoàn thành",
                5 => "Đơn hàng đã bị hủy",
                _ => "Cập nhật trạng thái đơn hàng"
            };
        }

        /// <summary>
        /// Lấy màu header email theo trạng thái
        /// </summary>
        private string GetStatusHeaderColor(int statusId)
        {
            return statusId switch
            {
                1 => "linear-gradient(135deg, #ff9800, #ff6d00)", // màu cam cho chờ xác nhận
                2 => "linear-gradient(135deg, #03a9f4, #039be5)", // màu xanh dương cho đã xác nhận
                3 => "linear-gradient(135deg, #3f51b5, #303f9f)", // màu indigo cho đang giao
                4 => "linear-gradient(135deg, #4caf50, #2e7d32)", // màu xanh lá cho hoàn thành
                5 => "linear-gradient(135deg, #f44336, #c62828)", // màu đỏ cho hủy đơn
                _ => "linear-gradient(135deg, #607d8b, #455a64)"  // màu xám mặc định
            };
        }

        /// <summary>
        /// Lấy màu cho text status
        /// </summary>
        private string GetStatusColor(int statusId)
        {
            return statusId switch
            {
                1 => "#ff9800", // cam
                2 => "#03a9f4", // xanh dương
                3 => "#3f51b5", // indigo
                4 => "#4caf50", // xanh lá
                5 => "#f44336", // đỏ
                _ => "#607d8b"  // xám
            };
        }

        /// <summary>
        /// Lấy icon cho trạng thái
        /// </summary>
        private string GetStatusIcon(int statusId)
        {
            return statusId switch
            {
                1 => "⏳",
                2 => "✅",
                3 => "🚚",
                4 => "🎉",
                5 => "❌",
                _ => "📋"
            };
        }

        /// <summary>
        /// Lấy class CSS cho alert dựa trên trạng thái
        /// </summary>
        private string GetAlertClassForStatus(int statusId)
        {
            return statusId switch
            {
                1 => "alert-warning",  // warning cho chờ xác nhận
                2 => "alert-info",     // info cho đã xác nhận
                3 => "alert-info",     // info cho đang giao
                4 => "alert-success",  // success cho hoàn thành
                5 => "alert-danger",   // danger cho hủy
                _ => "alert-info"
            };
        }

        /// <summary>
        /// Lấy tiêu đề cho thông báo
        /// </summary>
        private string GetStatusAlertTitle(int statusId)
        {
            return statusId switch
            {
                1 => "Thông tin quan trọng",
                2 => "Thông tin quan trọng",
                3 => "Thông tin giao hàng",
                4 => "Giao dịch thành công",
                5 => "Thông tin hủy đơn",
                _ => "Thông tin cập nhật"
            };
        }

        /// <summary>
        /// Lấy nội dung thông báo dựa trên sự thay đổi trạng thái
        /// </summary>
        private string GetStatusUpdateMessage(int oldStatusId, int newStatusId)
        {
            return (oldStatusId, newStatusId) switch
            {
                // Từ chờ xác nhận -> đã xác nhận
                (1, 2) => "Chúc mừng! Đơn hàng của bạn vừa được <span class='highlight'>xác nhận</span> bởi người bán. Họ đang chuẩn bị sản phẩm và sẽ liên hệ với bạn để sắp xếp giao hàng.",

                // Từ đã xác nhận -> đang giao hàng
                (2, 3) => "Thông báo vui! Đơn hàng của bạn đang trong quá trình <span class='highlight'>giao hàng</span>. Người bán đã bắt đầu quá trình giao sản phẩm đến cho bạn.",

                // Từ đang giao hàng -> hoàn thành
                (3, 4) => "Chúc mừng! Đơn hàng của bạn đã được đánh dấu là <span class='highlight'>hoàn thành</span>. Cảm ơn bạn đã sử dụng dịch vụ của REVA.",

                // Các trường hợp hủy đơn
                (1, 5) => "Đơn hàng #" + "{0}" + " của bạn đã <span class='highlight'>bị hủy</span> bởi người bán.",
                (2, 5) => "Đơn hàng #" + "{0}" + " của bạn đã <span class='highlight'>bị hủy</span> bởi người bán sau khi đã xác nhận.",
                (3, 5) => "Đơn hàng #" + "{0}" + " của bạn đã <span class='highlight'>bị hủy</span> bởi người bán trong quá trình giao hàng.",

                // Mặc định cho các trường hợp khác
                _ => $"Đơn hàng của bạn vừa được cập nhật từ trạng thái <span class='highlight'>{GetStatusName(oldStatusId)}</span> sang <span class='highlight'>{GetStatusName(newStatusId)}</span>."
            };
        }

        /// <summary>
        /// Lấy thông tin hướng dẫn tiếp theo dựa trên trạng thái mới
        /// </summary>
        private string GetNextStepsMessage(int newStatusId)
        {
            return newStatusId switch
            {
                1 => "Đơn hàng của bạn đang chờ người bán xác nhận. Quá trình này thường mất khoảng 24 giờ. Bạn sẽ nhận được email thông báo khi người bán xác nhận đơn hàng.",

                2 => "Người bán đã xác nhận đơn hàng và đang chuẩn bị sản phẩm. Họ sẽ sớm liên hệ với bạn để thảo luận về phương thức thanh toán và giao nhận. Bạn cũng có thể chủ động liên hệ với họ thông qua thông tin trong đơn hàng.",

                3 => "Đơn hàng của bạn đang được giao. Vui lòng đảm bảo rằng bạn có thể nhận hàng và liên lạc được qua số điện thoại đã đăng ký. Nếu có thắc mắc về quá trình giao hàng, bạn có thể liên hệ trực tiếp với người bán.",

                4 => "Đơn hàng của bạn đã hoàn thành! Chúng tôi rất vui khi bạn đã nhận được sản phẩm. Nếu bạn hài lòng với sản phẩm và dịch vụ, hãy đánh giá và cho chúng tôi biết trải nghiệm của bạn. Điều này sẽ giúp cải thiện dịch vụ và hỗ trợ cộng đồng REVA.",

                5 => "Đơn hàng của bạn đã bị hủy. Nếu bạn vẫn quan tâm đến sản phẩm tương tự, bạn có thể tìm kiếm các sản phẩm khác trên nền tảng REVA. Nếu bạn cần hỗ trợ thêm, vui lòng liên hệ với đội hỗ trợ khách hàng của chúng tôi.",

                _ => "Vui lòng kiểm tra trang chi tiết đơn hàng để biết thêm thông tin về đơn hàng của bạn."
            };
        }
    }
}
