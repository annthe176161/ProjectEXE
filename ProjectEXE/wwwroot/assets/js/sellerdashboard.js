// Function to handle actions like editing and deleting products
document.querySelectorAll(".btn-warning").forEach((button) => {
  button.addEventListener("click", function () {
    alert("Chỉnh sửa sản phẩm");
  });
});

document.querySelectorAll(".btn-danger").forEach((button) => {
  button.addEventListener("click", function () {
    alert("Xóa sản phẩm");
  });
});

// Function to handle viewing order details
document.querySelectorAll(".btn-info").forEach((button) => {
  button.addEventListener("click", function () {
    alert("Xem chi tiết đơn hàng");
  });
});

// Function to handle replying to buyer messages
document.querySelector(".btn-primary").addEventListener("click", function () {
  alert("Trả lời tin nhắn từ người mua");
});
