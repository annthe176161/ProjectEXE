// Update total prices when quantity changes
document.querySelectorAll(".quantity").forEach((input) => {
  input.addEventListener("input", function () {
    const row = input.closest("tr");
    const price = parseFloat(
      row
        .querySelector(".price")
        .textContent.replace(" VND", "")
        .replace(",", "")
    );
    const quantity = parseInt(input.value);
    const total = price * quantity;
    row.querySelector(".total").textContent = total.toLocaleString() + " VND";
    updateCartTotal();
  });
});

// Remove product from cart
document.querySelectorAll(".remove-item").forEach((button) => {
  button.addEventListener("click", function () {
    const row = button.closest("tr");
    row.remove();
    updateCartTotal();
  });
});

// Update cart total
function updateCartTotal() {
  let total = 0;
  document.querySelectorAll(".total").forEach((cell) => {
    total += parseFloat(cell.textContent.replace(" VND", "").replace(",", ""));
  });
  const shipping = 50000; // Example shipping fee
  const discount =
    parseFloat(
      document
        .getElementById("discount")
        .textContent.replace(" VND", "")
        .replace(",", "")
    ) || 0;
  const finalTotal = total + shipping - discount;
  document.getElementById("total-products").textContent =
    total.toLocaleString() + " VND";
  document.getElementById("total-payment").textContent =
    finalTotal.toLocaleString() + " VND";
}

// Apply discount code
document
  .getElementById("apply-discount")
  .addEventListener("click", function () {
    const discountCode = document.getElementById("discount-code").value;
    let discountAmount = 0;

    if (discountCode === "SALE20") {
      discountAmount =
        0.2 *
        parseFloat(
          document
            .getElementById("total-products")
            .textContent.replace(" VND", "")
            .replace(",", "")
        );
    }

    document.getElementById("discount").textContent =
      discountAmount.toLocaleString() + " VND";
    updateCartTotal();
  });
