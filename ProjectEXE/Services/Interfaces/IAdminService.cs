using ProjectEXE.DTO;
using ProjectEXE.Models;

namespace ProjectEXE.Services.Interfaces
{
    public interface IAdminService
    {
        Task<List<ServicePackage>> getAllService();
        Task<List<RBMDto>> getRevenueByMonth();
        Task<List<RBPDto>> getRevenueByPackage();
        Task<bool> deleteServiceById(int id);
        Task<bool> addPackage(ServicePackage package);
        Task<bool> editPackage(ServicePackage package);
    }
}
