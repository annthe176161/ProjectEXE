using ProjectEXE.Models;
using ProjectEXE.ViewModel.ServicePackages;

namespace ProjectEXE.Services.Interfaces
{
    public interface IPackageService
    {
        Task<List<ServicePackage>> GetAllPackagesAsync();
        Task<ServicePackage> GetPackageByIdAsync(int packageId);
        Task<ServicePackageListViewModel> GetPackagesForShopAsync(int shopId);
        Task<PaymentInformationViewModel> CreatePaymentRequestAsync(int packageId, int shopId, int userId);
        Task<PaymentCompletionViewModel> ConfirmPaymentAsync(string transactionCode, int userId);
        Task<bool> HasActiveSubscriptionAsync(int shopId);
        Task<DateTime?> GetSubscriptionEndDateAsync(int shopId);
    }
}
