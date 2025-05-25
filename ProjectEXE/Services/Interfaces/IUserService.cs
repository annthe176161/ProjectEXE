using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectEXE.Models;
using ProjectEXE.ViewModel;
using System.Security.Claims;


namespace ProjectEXE.Services.Interfaces
{
    
        public interface IUserService
        {
        Task<IEnumerable<UserViewModel>> GetAllUsersAsync(); // Existing method

        Task<User> GetUserDomainModelByIdAsync(int userId); // To get the raw User entity for editing
        Task<IEnumerable<Role>> GetAllRolesAsync(); // To populate roles dropdown

        Task<bool> UpdateUserAsync(UserEditViewModel model);
        Task<bool> SetUserActiveStatusAsync(int userId, bool isActive);

        Task<int> GetTotalUsersCountAsync(); // <== PHƯƠNG THỨC MỚI


        Task<int> GetTotalShopsCountAsync(); // <== PHƯƠNG THỨC MỚI

        Task<int> GetTotalProductsCountAsync(); // <== PHƯƠNG THỨC MỚI


        Task<decimal> GetTotalPackagePaymentsRevenueAsync();
        Task<User> GetUserByEmailAsync(string email);
        Task<bool> ValidatePasswordAsync(User user, string password);
        Task<User> CreateUserAsync(string email, string password, string fullName, string phoneNumber, string address, int roleId);
        Task<bool> IsEmailExistsAsync(string email);
        Task<User> GetUserByIdAsync(int userId);
        ClaimsPrincipal CreateClaimsPrincipal(User user);
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
        Task UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> IsEmailVerifiedAsync(string email);
        Task<bool> IsEmailPendingVerificationAsync(string email);
    }
    
}
