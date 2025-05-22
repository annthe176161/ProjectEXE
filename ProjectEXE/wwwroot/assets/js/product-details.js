document.addEventListener("DOMContentLoaded", function () {
  // Handle thumbnail clicks
  const thumbnails = document.querySelectorAll(".thumbnail-item");
  const mainImage = document.getElementById("mainProductImage");

  thumbnails.forEach((thumb) => {
    thumb.addEventListener("click", function () {
      // Change main image src
      mainImage.src = this.getAttribute("data-image");

      // Update active state
      thumbnails.forEach((t) => t.classList.remove("active"));
      this.classList.add("active");
    });
  });

  // Image zoom functionality
  if (mainImage) {
    const container = mainImage.parentElement;

    container.addEventListener("mousemove", function (e) {
      const { left, top, width, height } = container.getBoundingClientRect();
      const x = (e.clientX - left) / width;
      const y = (e.clientY - top) / height;

      mainImage.style.transformOrigin = `${x * 100}% ${y * 100}%`;
    });

    container.addEventListener("mouseenter", function () {
      mainImage.style.transform = "scale(2)";
    });

    container.addEventListener("mouseleave", function () {
      mainImage.style.transform = "scale(1)";
    });
  }

  // Handle size selection
  const sizeOptions = document.querySelectorAll(".size-option:not(.disabled)");
  const sizeValue = document.querySelector(
    ".product-sizes .selected-option strong"
  );

  sizeOptions.forEach((option) => {
    option.addEventListener("click", function () {
      sizeOptions.forEach((o) => o.classList.remove("active"));
      this.classList.add("active");
      sizeValue.textContent = this.getAttribute("data-size");
    });
  });

  // Handle color selection
  const colorOptions = document.querySelectorAll(".color-option");
  const colorValue = document.querySelector(
    ".product-colors .selected-option strong"
  );

  colorOptions.forEach((option) => {
    option.addEventListener("click", function () {
      colorOptions.forEach((o) => o.classList.remove("active"));
      this.classList.add("active");
      colorValue.textContent = this.getAttribute("data-color");
    });
  });

  // Quantity controls
  const quantityInput = document.querySelector(".quantity-input");
  const minusBtn = document.querySelector(".minus-btn");
  const plusBtn = document.querySelector(".plus-btn");

  minusBtn.addEventListener("click", function () {
    let currentValue = parseInt(quantityInput.value);
    if (currentValue > 1) {
      quantityInput.value = currentValue - 1;
    }
  });

  plusBtn.addEventListener("click", function () {
    let currentValue = parseInt(quantityInput.value);
    const max = parseInt(quantityInput.getAttribute("max"));
    if (currentValue < max) {
      quantityInput.value = currentValue + 1;
    }
  });
});

document.addEventListener("DOMContentLoaded", function () {
  // Toggle write review form
  const writeReviewBtn = document.getElementById("writeReviewBtn");
  const writeReviewForm = document.getElementById("writeReviewForm");
  const cancelReviewBtn = document.getElementById("cancelReviewBtn");

  if (writeReviewBtn && writeReviewForm && cancelReviewBtn) {
    writeReviewBtn.addEventListener("click", function () {
      writeReviewForm.style.display = "block";
      writeReviewBtn.style.display = "none";

      // Scroll to the form
      writeReviewForm.scrollIntoView({ behavior: "smooth", block: "start" });
    });

    cancelReviewBtn.addEventListener("click", function () {
      writeReviewForm.style.display = "none";
      writeReviewBtn.style.display = "inline-block";
    });
  }

  // Star rating functionality
  const ratingStars = document.querySelectorAll(".rating-star");
  if (ratingStars.length) {
    ratingStars.forEach((star) => {
      star.addEventListener("click", function () {
        const rating = this.getAttribute("data-rating");

        // Reset all stars
        ratingStars.forEach((s) => (s.className = "far fa-star rating-star"));

        // Fill stars up to selected one
        for (let i = 0; i < rating; i++) {
          ratingStars[i].className = "fas fa-star rating-star active";
        }
      });
    });
  }

  // Handle photo uploads
  const photoInput = document.getElementById("reviewPhotos");
  const previewContainer = document.getElementById("photoPreviewContainer");

  if (photoInput && previewContainer) {
    photoInput.addEventListener("change", function () {
      // Clear previous previews
      previewContainer.innerHTML = "";

      // Max 5 photos
      const files = Array.from(this.files).slice(0, 5);

      files.forEach((file) => {
        if (!file.type.match("image.*")) {
          return;
        }

        const reader = new FileReader();

        reader.onload = function (e) {
          const previewItem = document.createElement("div");
          previewItem.className = "preview-item";

          const img = document.createElement("img");
          img.src = e.target.result;

          const removeBtn = document.createElement("div");
          removeBtn.className = "remove-btn";
          removeBtn.innerHTML = '<i class="fas fa-times"></i>';

          removeBtn.addEventListener("click", function () {
            previewItem.remove();
          });

          previewItem.appendChild(img);
          previewItem.appendChild(removeBtn);
          previewContainer.appendChild(previewItem);
        };

        reader.readAsDataURL(file);
      });
    });
  }

  // Initialize lightbox for review photos
  const popupImages = document.querySelectorAll(".popup-image");
  if (popupImages.length) {
    // This would typically use a library like Magnific Popup
    // For this example, we'll just add a class to handle via CSS
    popupImages.forEach((image) => {
      image.addEventListener("click", function (e) {
        e.preventDefault();
        // Here you would typically open a lightbox
        console.log("Open lightbox for", this.href);
      });
    });
  }
});

// Related Products Carousel functionality
document.addEventListener("DOMContentLoaded", function () {
  const productRow = document.getElementById("relatedProductsRow");
  const prevBtn = document.getElementById("relatedPrevBtn");
  const nextBtn = document.getElementById("relatedNextBtn");

  let scrollAmount = 0;
  const scrollStep = 300; // Adjust scroll step as needed

  // Handle Next button click
  nextBtn.addEventListener("click", function () {
    scrollAmount += scrollStep;
    if (scrollAmount > productRow.scrollWidth - productRow.clientWidth) {
      scrollAmount = productRow.scrollWidth - productRow.clientWidth;
    }
    productRow.scrollTo({
      left: scrollAmount,
      behavior: "smooth",
    });
  });

  // Handle Previous button click
  prevBtn.addEventListener("click", function () {
    scrollAmount -= scrollStep;
    if (scrollAmount < 0) {
      scrollAmount = 0;
    }
    productRow.scrollTo({
      left: scrollAmount,
      behavior: "smooth",
    });
  });

  // Initialize tooltips
  const tooltipTriggerList = [].slice.call(
    document.querySelectorAll('[data-bs-toggle="tooltip"]')
  );
  tooltipTriggerList.map(function (tooltipTriggerEl) {
    return new bootstrap.Tooltip(tooltipTriggerEl);
  });
});
