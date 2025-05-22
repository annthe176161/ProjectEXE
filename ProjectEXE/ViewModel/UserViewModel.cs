using System;
namespace ProjectEXE.ViewModel

{
    public class UserViewModel
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string RoleName { get; set; } // Sẽ lấy từ bảng Roles
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
