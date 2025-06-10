using Microsoft.EntityFrameworkCore;
using ProjectEXE.Models;
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel;
using System.Security.Claims;

namespace ProjectEXE.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly RevaContext _context;
        private const int CONFIRMED_PAYMENT_STATUS_ID = 2;
        public UserService(RevaContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            
            return await _context.Users
            .Include(u => u.Role)
            .AsNoTracking()
             .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
                            
        }
        public async Task<bool> ValidatePasswordAsync(User user, string password)
        {
            if (user == null || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(user.PasswordHash))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Password verification error: {ex.Message}");
                return false;
            }
        }

        public async Task<User> GetUserDomainModelByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }
        public async Task<User> CreateUserAsync(string email, string password, string fullName, string phoneNumber, string address, int roleId, string referralCode, string referredBy)
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
                IsActive = 2, // 
                CreatedAt = DateTime.Now,
                ReferralCode = referralCode,
                ReferredBy = referredBy
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }
        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _context.Roles.OrderBy(r => r.RoleName).ToListAsync();
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

        public async Task UpdateUserAsync(User user)
        {
            try
            {
                _context.Entry(user).State = EntityState.Modified;
                var result = await _context.SaveChangesAsync();
                Console.WriteLine($"User update result: {result} records affected");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateUserAsync(UserEditViewModel model)
        {
            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null)
            {
                return false; // User not found
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.RoleId = model.RoleId;
            user.IsActive = model.IsActive;
            // Note: Password and CreatedAt are not updated here.
            // Password update should be a separate, secure process.

            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> SetUserActiveStatusAsync(int userId, int isActive)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.IsActive = isActive;
            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
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

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _context.Users.CountAsync(u => u.IsActive == 1);
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        public async Task<int> GetTotalShopsCountAsync()
        {
            // Giả định RevaContext của bạn có public DbSet<Shop> Shops { get; set; }
            // và bạn đã có model Shop.cs trong thư mục Models
            if (_context.Shops == null) // Kiểm tra phòng trường hợp DbSet chưa được khởi tạo
            {
                return 0;
            }
            return await _context.Shops.CountAsync();
        }


        public bool VerifyPassword(string password, string passwordHash)
        {
            // For demo purposes, if the hash is the same as the password (unencrypted), return true
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
                return false;

            // Bỏ điều kiện hardcoded đặc biệt vì có thể gây nhầm lẫn
            // if (passwordHash == "hashedpassword" && password == "admin123")
            //     return true;

            try
            {
                // Luôn sử dụng BCrypt để xác thực nhất quán
                return BCrypt.Net.BCrypt.Verify(password, passwordHash);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Password verification error: {ex.Message}");
                return false;
            }
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive == 1);
        }

        public async Task<decimal> GetTotalPackagePaymentsRevenueAsync()
        {
            if (_context.PackagePayments == null)
            {
                return 0m; // Trả về 0 kiểu decimal
            }
            // Tính tổng cột 'Amount' cho các thanh toán đã được xác nhận (StatusId = 2)
            return await _context.PackagePayments
                                 .Where(p => p.StatusId == CONFIRMED_PAYMENT_STATUS_ID)
                                 .SumAsync(p => p.Amount);
        }

        public async Task<IEnumerable<UserViewModel>> GetAllUsersAsync()
        {
            var usersData = await (from user in _context.Users
                                   join role in _context.Roles on user.RoleId equals role.RoleId // Join với bảng Roles
                                   orderby user.CreatedAt descending
                                   select new UserViewModel
                                   {
                                       UserId = user.UserId,
                                       Email = user.Email,
                                       FullName = user.FullName,
                                       RoleName = role.RoleName, // Lấy RoleName từ bảng Roles
                                       CreatedAt = user.CreatedAt,
                                       IsActive = user.IsActive
                                   }).ToListAsync();
            return usersData;
        }

        // PHƯƠNG THỨC MỚI ĐỂ ĐẾM TỔNG SỐ SẢN PHẨM
        public async Task<int> GetTotalProductsCountAsync()
        {
            // Giả định RevaContext của bạn có public DbSet<Product> Products { get; set; }
            // và bạn đã có model Product.cs trong thư mục Models
            if (_context.Products == null) // Kiểm tra phòng trường hợp DbSet chưa được khởi tạo
            {
                return 0;
            }
            return await _context.Products.CountAsync();
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return false;
                }

                // Xóa các liên kết với bảng khác nếu cần
                // Ví dụ: Xóa các liên kết đến vai trò, đơn hàng, v.v.

                _context.Users.Remove(user);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xóa user: {ex.Message}");
                return false;
            }
        }

       
        
        
    }
}
