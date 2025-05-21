using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectEXE.Models;
using ProjectEXE.ViewModel;


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
    }
    
}
