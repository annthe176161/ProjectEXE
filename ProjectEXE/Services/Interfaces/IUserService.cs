using ProjectEXE.Models;
using System.Security.Claims;

namespace ProjectEXE.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> GetUserByEmailAsync(string email);
        Task<bool> ValidatePasswordAsync(User user, string password);
        Task<User> CreateUserAsync(string email, string password, string fullName, string phoneNumber, string address, int roleId);
        Task<bool> IsEmailExistsAsync(string email);
        ClaimsPrincipal CreateClaimsPrincipal(User user);
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }
}
