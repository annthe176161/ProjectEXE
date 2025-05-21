using ProjectEXE.ViewModel.ShopOrderViewModel;

namespace ProjectEXE.Services.Interfaces
{
    public interface IShopOrderService
    {
        Task<ShopOrderManagementViewModel> GetShopOrdersAsync(int sellerId, int page = 1, string statusFilter = null, string dateRange = null);
        Task<ShopOrderDetailViewModel> GetShopOrderDetailAsync(int orderId, int sellerId);
        Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusViewModel model, int sellerId);
        Task<Dictionary<int, string>> GetOrderStatusesAsync();
    }
}
