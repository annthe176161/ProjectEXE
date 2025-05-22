// Handle Edit Profile Form Submission
document
  .getElementById("edit-profile-form")
  .addEventListener("submit", function (e) {
    e.preventDefault();
    alert("Thông tin tài khoản đã được cập nhật!");
  });

// Handle Edit Address Form Submission
document
  .getElementById("edit-address-form")
  .addEventListener("submit", function (e) {
    e.preventDefault();
    alert("Địa chỉ giao hàng đã được cập nhật!");
  });

// Handle Change Password Form Submission
document
  .getElementById("change-password-form")
  .addEventListener("submit", function (e) {
    e.preventDefault();
    alert("Mật khẩu đã được thay đổi!");
  });
