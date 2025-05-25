using ProjectEXE.DTO;
using ProjectEXE.Models;
using ProjectEXE.ViewModel;

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

        Task<IEnumerable<RecentPackagePaymentDto>> GetRecentPackagePaymentsAsync(int count);

        // SỬA Ở ĐÂY: Cập nhật kiểu dữ liệu liên quan đến Category
        Task<IEnumerable<CategoryOfAdminViewModel>> GetAllCategoriesWithParentNameAsync();
        Task<Category> GetCategoryByIdAsync(int categoryId);
        Task<IEnumerable<Category>> GetAllParentCategoriesAsync();
        Task<bool> AddCategoryAsync(CategoryOfAdminViewModel model); // Sửa ở đây
        Task<bool> UpdateCategoryAsync(CategoryOfAdminViewModel model); // Sửa ở đây
        Task<(bool Success, string ErrorMessage)> DeleteCategoryAsync(int categoryId);

    }
}
