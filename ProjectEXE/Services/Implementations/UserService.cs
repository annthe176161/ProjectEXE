using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectEXE.Models;
using ProjectEXE.Services.Interfaces;
using System.Security.Claims;
using System.Security.Cryptography;

namespace ProjectEXE.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly RevaContext _context;

        public UserService(RevaContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);
        }

        public async Task<bool> ValidatePasswordAsync(User user, string password)
        {
            if (user == null)
                return false;

            return VerifyPassword(password, user.PasswordHash);
        }

        public async Task<User> CreateUserAsync(string email, string password, string fullName, string phoneNumber, string address, int roleId)
        {
            var passwordHash = HashPassword(password);

            var user = new User
            {
                Email = email,
                PasswordHash = passwordHash,
                FullName = fullName,
                PhoneNumber = phoneNumber,
                Address = address,
                RoleId = roleId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public ClaimsPrincipal CreateClaimsPrincipal(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null");
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(ClaimTypes.Name, user.FullName),
        new Claim(ClaimTypes.Email, user.Email)
    };

            // Kiểm tra null cho user.Role
            if (user.Role != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role.RoleName));
            }
            else
            {
                // Thêm claim mặc định hoặc lấy RoleName từ trường RoleId
                var roleName = GetRoleNameById(user.RoleId);
                claims.Add(new Claim(ClaimTypes.Role, roleName));
            }

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            return new ClaimsPrincipal(identity);
        }

        // Thêm phương thức lấy tên role từ ID
        private string GetRoleNameById(int roleId)
        {
            // Map roleId to roleName
            return roleId switch
            {
                1 => "Admin",
                2 => "Buyer",
                3 => "Seller",
                _ => "User" // Default role
            };
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            // For demo purposes, if the hash is the same as the password (unencrypted), return true
            if (passwordHash == "hashedpassword" && password == "admin123")
                return true;

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, passwordHash);
            }
            catch
            {
                return false;
            }
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);
        }
        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
