using ProjectEXE.ViewModel.OrderViewModel;

namespace ProjectEXE.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDetailsViewModel> CreateOrderAsync(int productId, int buyerId);
        Task<OrderListViewModel> GetBuyerOrdersAsync(int buyerId, int page = 1, int pageSize = 10);
        Task<OrderDetailsViewModel> GetOrderDetailsAsync(int orderId, int userId);
        Task<bool> CancelOrderAsync(int orderId, int buyerId, string reason);
    }
}
