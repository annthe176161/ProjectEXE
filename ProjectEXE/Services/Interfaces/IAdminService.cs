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

        // PHƯƠNG THỨC MỚI CHO CATEGORIES
        Task<IEnumerable<CategoryOfAdminViewModel>> GetAllCategoriesWithParentNameAsync();
        Task<Category> GetCategoryByIdAsync(int categoryId); // Trả về Model để xử lý logic
        Task<IEnumerable<Category>> GetAllParentCategoriesAsync(); // Lấy danh sách category làm cha
        Task<bool> AddCategoryAsync(CategoryOfAdminViewModel model);
        Task<bool> UpdateCategoryAsync(CategoryOfAdminViewModel model);
        Task<(bool Success, string ErrorMessage)> DeleteCategoryAsync(int categoryId);

    }
}
