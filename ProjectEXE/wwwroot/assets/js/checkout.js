// Confirm order
document.getElementById("confirm-order").addEventListener("click", function () {
  const fullName = document.getElementById("full-name").value;
  const phone = document.getElementById("phone").value;
  const address = document.getElementById("address").value;
  const paymentMethod = document.querySelector(
    'input[name="payment-method"]:checked'
  ).value;

  if (!fullName || !phone || !address) {
    alert("Vui lòng điền đầy đủ thông tin giao hàng!");
    return;
  }

  let message = `
        Đơn hàng đã được xác nhận!\n
        Người nhận: ${fullName}\n
        Số điện thoại: ${phone}\n
        Địa chỉ giao hàng: ${address}\n
        Phương thức thanh toán: ${paymentMethod}\n
        Tổng thanh toán: ${document.getElementById("total-payment").textContent}
    `;

  alert(message);
});

// Cancel order
document.getElementById("cancel-order").addEventListener("click", function () {
  if (confirm("Bạn chắc chắn muốn hủy đơn hàng?")) {
    window.location.href = "/"; // Redirect to home or cart page
  }
});
