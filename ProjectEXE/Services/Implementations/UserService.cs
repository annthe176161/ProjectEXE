using Microsoft.EntityFrameworkCore; // Cần cho Include và các phương thức của EF Core
using ProjectEXE.Models; // Chứa RevaContext và các class User, Role
using ProjectEXE.Services.Interfaces;
using ProjectEXE.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<User> GetUserDomainModelByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _context.Roles.OrderBy(r => r.RoleName).ToListAsync();
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

        public async Task<bool> SetUserActiveStatusAsync(int userId, bool isActive)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false; // User not found
            }

            user.IsActive = isActive;
            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }


        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _context.Users.CountAsync();
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

    }
}
