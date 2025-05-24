document.addEventListener("DOMContentLoaded", function () {
  // Khởi tạo các biến và tham chiếu
  const form = document.getElementById("productUploadForm");
  const tabNavLinks = document.querySelectorAll(
    '.nav-link[data-bs-toggle="tab"]'
  );
  const nextButtons = document.querySelectorAll(".btn-next-step");
  const prevButtons = document.querySelectorAll(".btn-prev-step");
  const imageUploadInput = document.getElementById("productImages");
  const imagePreviewContainer = document.getElementById(
    "imagePreviewContainer"
  );
  const maxImages = 8;
  const allowedFileTypes = [
    "image/jpeg",
    "image/png",
    "image/webp",
    "image/gif",
  ];
  const maxFileSize = 5 * 1024 * 1024; // 5MB

  // Khởi tạo tab bootstrap nếu chưa được khởi tạo
  if (typeof bootstrap !== "undefined") {
    const tabElements = document.querySelectorAll(
      "#productUploadTabs .nav-link"
    );
    tabElements.forEach(function (tabElement) {
      new bootstrap.Tab(tabElement);
    });
  }

  // Xử lý tải lên và xem trước hình ảnh
  if (imageUploadInput) {
    imageUploadInput.addEventListener("change", function (event) {
      const files = event.target.files;
      const currentImages = document.querySelectorAll(
        ".image-preview-item"
      ).length;

      // Kiểm tra số lượng hình ảnh
      if (files.length + currentImages > maxImages) {
        alert(`Bạn chỉ có thể tải lên tối đa ${maxImages} hình ảnh.`);
        return;
      }

      // Xử lý từng tệp
      for (let i = 0; i < files.length; i++) {
        const file = files[i];

        // Kiểm tra loại tệp
        if (!allowedFileTypes.includes(file.type)) {
          alert(
            `Tệp "${file.name}" không phải là hình ảnh hợp lệ. Chỉ chấp nhận JPG, PNG, WEBP và GIF.`
          );
          continue;
        }

        // Kiểm tra kích thước tệp
        if (file.size > maxFileSize) {
          alert(`Tệp "${file.name}" quá lớn. Kích thước tối đa là 5MB.`);
          continue;
        }

        // Tạo xem trước
        createImagePreview(file);
      }

      // Cập nhật bộ đếm hình ảnh
      updateImageCounter();

      // Kiểm tra nút Tiếp theo
      checkImageTabValidation();
    });
  }

  // Hàm tạo xem trước hình ảnh
  function createImagePreview(file) {
    const reader = new FileReader();

    reader.onload = function (e) {
      const previewItem = document.createElement("div");
      previewItem.className = "image-preview-item";
      previewItem.innerHTML = `
                <div class="image-preview-wrapper">
                    <img src="${e.target.result}" alt="Preview">
                    <div class="image-preview-actions">
                        <button type="button" class="btn-remove-image" title="Xóa hình ảnh">
                            <i class="fas fa-trash"></i>
                        </button>
                        <button type="button" class="btn-primary-image" title="Đặt làm ảnh chính">
                            <i class="fas fa-star"></i>
                        </button>
                    </div>
                </div>
                <span class="image-name">${file.name}</span>
            `;

      // Thêm vào container
      imagePreviewContainer.appendChild(previewItem);

      // Xử lý các nút
      const removeButton = previewItem.querySelector(".btn-remove-image");
      removeButton.addEventListener("click", function () {
        previewItem.remove();
        updateImageCounter();
        checkImageTabValidation();
      });

      const primaryButton = previewItem.querySelector(".btn-primary-image");
      primaryButton.addEventListener("click", function () {
        // Xóa lớp active khỏi tất cả các mục
        document.querySelectorAll(".image-preview-item").forEach((item) => {
          item.classList.remove("is-primary");
        });
        // Thêm lớp active cho mục hiện tại
        previewItem.classList.add("is-primary");
      });

      // Đặt ảnh đầu tiên là chính
      if (document.querySelectorAll(".image-preview-item").length === 1) {
        previewItem.classList.add("is-primary");
      }
    };

    reader.readAsDataURL(file);
  }

  // Cập nhật bộ đếm hình ảnh
  function updateImageCounter() {
    const counter = document.getElementById("imageCounter");
    if (counter) {
      const currentCount = document.querySelectorAll(
        ".image-preview-item"
      ).length;
      counter.textContent = `${currentCount}/${maxImages}`;
    }
  }

  // Kiểm tra xem tab hình ảnh đã có hình ảnh chưa
  function checkImageTabValidation() {
    const nextButton = document.querySelector(
      "#images-tab-pane .btn-next-step"
    );
    if (nextButton) {
      const hasImages =
        document.querySelectorAll(".image-preview-item").length > 0;
      nextButton.disabled = !hasImages;

      // Cập nhật lớp cho tab
      const imagesTab = document.getElementById("images-tab");
      if (hasImages) {
        imagesTab.classList.add("has-images");
      } else {
        imagesTab.classList.remove("has-images");
      }
    }
  }

  // Xử lý điều hướng tab tiếp theo
  if (nextButtons) {
    nextButtons.forEach((button) => {
      button.addEventListener("click", function (e) {
        e.preventDefault();

        // Lấy tab hiện tại và tab tiếp theo
        const currentTab = this.closest(".tab-pane");
        const currentTabId = currentTab.id;
        const nextTabId = this.getAttribute("data-next-tab");

        // Kiểm tra tính hợp lệ của tab hiện tại
        if (validateTabInputs(currentTabId)) {
          // Chuyển sang tab tiếp theo
          const nextTab = document.querySelector(
            `[data-bs-target="#${nextTabId}"]`
          );
          if (nextTab) {
            const tab = new bootstrap.Tab(nextTab);
            tab.show();
          }
        }
      });
    });
  }

  // Xử lý điều hướng tab trước
  if (prevButtons) {
    prevButtons.forEach((button) => {
      button.addEventListener("click", function (e) {
        e.preventDefault();

        // Lấy tab trước
        const prevTabId = this.getAttribute("data-prev-tab");
        const prevTab = document.querySelector(
          `[data-bs-target="#${prevTabId}"]`
        );

        if (prevTab) {
          const tab = new bootstrap.Tab(prevTab);
          tab.show();
        }
      });
    });
  }

  // Kiểm tra tính hợp lệ của các trường nhập liệu trong tab
  function validateTabInputs(tabId) {
    switch (tabId) {
      case "images-tab-pane":
        // Kiểm tra có ít nhất 1 hình ảnh
        return document.querySelectorAll(".image-preview-item").length > 0;

      case "basic-info-tab-pane":
        // Kiểm tra thông tin cơ bản
        const productName = document.getElementById("productName").value;
        const category = document.getElementById("productCategory").value;
        const price = document.getElementById("productPrice").value;

        if (!productName || productName.trim() === "") {
          alert("Vui lòng nhập tên sản phẩm");
          return false;
        }

        if (!category || category === "default") {
          alert("Vui lòng chọn danh mục sản phẩm");
          return false;
        }

        if (!price || isNaN(parseFloat(price)) || parseFloat(price) <= 0) {
          alert("Vui lòng nhập giá hợp lệ");
          return false;
        }

        return true;

      case "details-tab-pane":
        // Kiểm tra thông tin chi tiết
        const condition = document.getElementById("productCondition").value;
        const size = document.getElementById("productSize").value;

        if (!condition || condition === "default") {
          alert("Vui lòng chọn tình trạng sản phẩm");
          return false;
        }

        if (!size || size === "default") {
          alert("Vui lòng chọn kích thước sản phẩm");
          return false;
        }

        return true;

      default:
        return true;
    }
  }

  // Form submission
  if (form) {
    form.addEventListener("submit", function (e) {
      e.preventDefault();

      // Kiểm tra toàn bộ form
      if (validateForm()) {
        // Hiển thị thông báo thành công thay vì gửi form
        // Trong thực tế, bạn sẽ gửi form thông qua AJAX
        showSuccessMessage();
      }
    });
  }

  // Kiểm tra toàn bộ form
  function validateForm() {
    // Kiểm tra tab hình ảnh
    if (!validateTabInputs("images-tab-pane")) {
      const imagesTab = document.querySelector(
        '[data-bs-target="#images-tab-pane"]'
      );
      if (imagesTab) {
        const tab = new bootstrap.Tab(imagesTab);
        tab.show();
      }
      return false;
    }

    // Kiểm tra tab thông tin cơ bản
    if (!validateTabInputs("basic-info-tab-pane")) {
      const basicInfoTab = document.querySelector(
        '[data-bs-target="#basic-info-tab-pane"]'
      );
      if (basicInfoTab) {
        const tab = new bootstrap.Tab(basicInfoTab);
        tab.show();
      }
      return false;
    }

    // Kiểm tra tab thông tin chi tiết
    if (!validateTabInputs("details-tab-pane")) {
      const detailsTab = document.querySelector(
        '[data-bs-target="#details-tab-pane"]'
      );
      if (detailsTab) {
        const tab = new bootstrap.Tab(detailsTab);
        tab.show();
      }
      return false;
    }

    return true;
  }

  // Hiển thị thông báo thành công
  function showSuccessMessage() {
    // Tạo modal thành công
    const successModal = `
            <div class="modal fade" id="successModal" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header bg-success text-white">
                            <h5 class="modal-title">Đăng sản phẩm thành công</h5>
                            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body text-center py-4">
                            <i class="fas fa-check-circle text-success" style="font-size: 3rem;"></i>
                            <h4 class="mt-3">Chúc mừng!</h4>
                            <p>Sản phẩm của bạn đã được đăng thành công và đang chờ phê duyệt.</p>
                        </div>
                        <div class="modal-footer">
                            <a href="sellerdashboard.html" class="btn btn-outline-secondary">Về trang quản lý</a>
                            <a href="sellerproductuploadpage.html" class="btn btn-success">Đăng sản phẩm khác</a>
                        </div>
                    </div>
                </div>
            </div>
        `;

    // Thêm modal vào trang
    document.body.insertAdjacentHTML("beforeend", successModal);

    // Hiển thị modal
    const modal = new bootstrap.Modal(document.getElementById("successModal"));
    modal.show();

    // Reset form
    form.reset();
    imagePreviewContainer.innerHTML = "";
    updateImageCounter();
  }

  // Khởi tạo khi trang tải xong
  updateImageCounter();
  checkImageTabValidation();

  // Xử lý thay đổi danh mục để hiển thị danh mục con
  const categorySelect = document.getElementById("productCategory");
  if (categorySelect) {
    categorySelect.addEventListener("change", function () {
      const subcategorySelect = document.getElementById("productSubcategory");
      const selectedCategory = this.value;

      // Xóa các tùy chọn danh mục con hiện tại
      while (subcategorySelect.options.length > 1) {
        subcategorySelect.remove(1);
      }

      // Hiển thị hoặc ẩn danh mục con tùy thuộc vào danh mục được chọn
      if (selectedCategory !== "default") {
        // Tạo dữ liệu danh mục con (trong thực tế sẽ lấy từ API)
        const subcategories = getSubcategories(selectedCategory);

        // Thêm tùy chọn danh mục con
        subcategories.forEach((subcategory) => {
          const option = document.createElement("option");
          option.value = subcategory.value;
          option.textContent = subcategory.text;
          subcategorySelect.appendChild(option);
        });

        // Hiển thị trường danh mục con
        document.getElementById("subcategoryGroup").classList.remove("d-none");
      } else {
        // Ẩn trường danh mục con
        document.getElementById("subcategoryGroup").classList.add("d-none");
      }
    });
  }

  // Hàm lấy danh mục con dựa trên danh mục (mẫu dữ liệu)
  function getSubcategories(category) {
    const subcategoryMap = {
      ao: [
        { value: "ao-thun", text: "Áo thun" },
        { value: "ao-somi", text: "Áo sơ mi" },
        { value: "ao-khoac", text: "Áo khoác" },
        { value: "ao-len", text: "Áo len" },
      ],
      quan: [
        { value: "quan-dai", text: "Quần dài" },
        { value: "quan-short", text: "Quần short" },
        { value: "quan-jean", text: "Quần jean" },
        { value: "quan-kaki", text: "Quần kaki" },
      ],
      vay: [
        { value: "vay-lien", text: "Váy liền" },
        { value: "chan-vay", text: "Chân váy" },
        { value: "dam-du-tiec", text: "Đầm dự tiệc" },
        { value: "dam-the-thao", text: "Đầm thể thao" },
      ],
      "do-the-thao": [
        { value: "ao-the-thao", text: "Áo thể thao" },
        { value: "quan-the-thao", text: "Quần thể thao" },
        { value: "do-bo-the-thao", text: "Đồ bộ thể thao" },
      ],
      "phu-kien": [
        { value: "tui-xach", text: "Túi xách" },
        { value: "giay-dep", text: "Giày dép" },
        { value: "non-mu", text: "Nón mũ" },
        { value: "that-lung", text: "Thắt lưng" },
        { value: "trang-suc", text: "Trang sức" },
      ],
    };

    return subcategoryMap[category] || [];
  }
});
