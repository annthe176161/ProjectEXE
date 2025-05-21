using ProjectEXE.ViewModel.Admin;

namespace ProjectEXE.Services.Interfaces
{
    public interface IAdminPackageService
    {
        Task<PackagePaymentListViewModel> GetPaymentDashboardAsync();
        Task<PackagePaymentViewModel> GetPaymentDetailsAsync(int paymentId);
        Task<List<PackagePaymentViewModel>> GetPaymentsByStatusAsync(int statusId, int page = 1, int pageSize = 10);
        Task<bool> ConfirmPaymentAsync(int paymentId, int adminUserId);
        Task<bool> RejectPaymentAsync(int paymentId, int adminUserId, string reason);
    }
}
