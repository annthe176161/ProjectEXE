using ProjectEXE.ViewModel.OrderViewModel;

namespace ProjectEXE.Services.Interfaces
{
    public interface IOrderConfirmationService
    {
        Task<OrderConfirmationViewModel?> GetOrderConfirmationDataAsync(int productId, int buyerId);
        Task<OrderConfirmationResultViewModel> CreateOrderAsync(OrderConfirmationViewModel model);
        Task<bool> IsProductAvailableAsync(int productId);
        Task<bool> CanUserPurchaseAsync(int productId, int buyerId);
        Task<OrderConfirmationResultViewModel> CreateOrderAsync(CreateOrderRequestViewModel model);
    }
}
