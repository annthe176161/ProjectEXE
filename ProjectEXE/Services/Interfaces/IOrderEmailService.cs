namespace ProjectEXE.Services.Interfaces
{
    public interface IOrderEmailService
    {
        /// <summary>
        /// Gửi email thông báo đặt hàng thành công
        /// </summary>
        Task SendOrderConfirmationNotificationAsync(int orderId, string productName, decimal price, string shopName, string buyerName, string buyerEmail, string sellerName, string sellerEmail);

        /// <summary>
        /// Gửi email thông báo hủy đơn hàng
        /// </summary>
        Task SendOrderCancellationNotificationAsync(int orderId, string productName, decimal price, string shopName, string buyerName, string buyerEmail, string sellerName, string sellerEmail, string cancelReason);

        /// <summary>
        /// Gửi email thông báo cập nhật trạng thái đơn hàng
        /// </summary>
        Task SendOrderStatusUpdateNotificationAsync(int orderId, string productName, decimal price, string shopName, string buyerName, string buyerEmail, int oldStatusId, int newStatusId, string cancelReason = null);
    }
}
