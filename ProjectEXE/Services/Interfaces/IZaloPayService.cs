using ProjectEXE.ViewModel.Zalo;

namespace ProjectEXE.Services.Interfaces
{
    public interface IZaloPayService
    {
        Task<ZaloPayOrderResponse> CreateOrderAsync(string userId, string packageId, long amount);
        bool ValidateCallback(ZaloPayCallbackData data);
    }
}
