// Price Filter Handling
document.addEventListener("DOMContentLoaded", function () {
  const priceRange = document.getElementById("priceRange");
  const minPrice = document.getElementById("minPrice");
  const maxPrice = document.getElementById("maxPrice");
  const applyPriceFilter = document.getElementById("applyPriceFilter");
  const resetFilters = document.getElementById("resetFilters");

  if (priceRange && minPrice && maxPrice) {
    // Update inputs when range slider changes
    priceRange.addEventListener("input", function () {
      maxPrice.value = this.value;
    });

    // Apply price filter
    applyPriceFilter.addEventListener("click", function () {
      // Thêm logic lọc sản phẩm theo giá ở đây
      console.log(
        `Filtering products between ${minPrice.value}đ and ${maxPrice.value}đ`
      );
      // Đây là nơi bạn sẽ thêm code để thực hiện lọc sản phẩm
    });

    // Reset filters
    resetFilters.addEventListener("click", function () {
      priceRange.value = 2000000;
      minPrice.value = "";
      maxPrice.value = "";

      // Reset condition checkboxes
      document.querySelectorAll(".form-check-input").forEach((checkbox) => {
        checkbox.checked = false;
      });

      // Reset size buttons
      document.querySelectorAll(".btn-check").forEach((btn) => {
        btn.checked = false;
      });

      console.log("All filters have been reset");
      // Đây là nơi bạn sẽ thêm code để hiển thị lại tất cả sản phẩm
    });
  }
});
