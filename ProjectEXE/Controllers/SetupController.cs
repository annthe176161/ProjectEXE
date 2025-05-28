//using Microsoft.AspNetCore.Mvc;
//using ProjectEXE.Models;

//namespace ProjectEXE.Controllers
//{
//    public class SetupController : Controller
//    {
//        private readonly RevaContext _context;

//        public SetupController(RevaContext context)
//        {
//            _context = context;
//        }

//        // Thêm tham số secret để tránh người khác truy cập
//        // Ví dụ: /Setup/CreateAdmin?secret=your_secret_key
//        public IActionResult CreateAdmin(string secret)
//        {
//            if (secret != "abc123xyz")
//                return NotFound();

//            try
//            {
//                string email = "admin@gmail.com";
//                string password = "Thanhan123"; // Thay đổi mật khẩu mạnh hơn

//                // Kiểm tra nếu admin đã tồn tại
//                var existingAdmin = _context.Users.FirstOrDefault(u => u.Email == email);

//                if (existingAdmin != null)
//                {
//                    // Cập nhật mật khẩu nếu đã tồn tại
//                    existingAdmin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
//                    _context.SaveChanges();
//                    return Content($"Đã cập nhật mật khẩu cho admin {email}. Mật khẩu mới: {password}");
//                }

//                // Tạo admin mới nếu chưa tồn tại
//                var admin = new User
//                {
//                    Email = email,
//                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
//                    FullName = "Quản trị viên",
//                    PhoneNumber = "0987654321",
//                    RoleId = 1, // Role Admin
//                    IsActive = 1,
//                    CreatedAt = DateTime.Now
//                };

//                _context.Users.Add(admin);
//                _context.SaveChanges();

//                return Content($"Đã tạo tài khoản admin thành công! Email: {email}, Mật khẩu: {password}");
//            }
//            catch (Exception ex)
//            {
//                return Content($"Lỗi: {ex.Message}");
//            }
//        }
//    }
//}
