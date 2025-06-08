document.addEventListener('DOMContentLoaded', function () {
    const profileImageUpload = document.getElementById('profileImageUpload');
    const shopImagePreview = document.getElementById('shopImagePreview');
    const submitButton = document.querySelector('.btn-save');
    const form = document.querySelector('form');

    // Xử lý tải lên và xem trước hình ảnh
    if (profileImageUpload) {
        profileImageUpload.addEventListener('change', function () {
            if (this.files && this.files[0]) {
                const file = this.files[0];

                // Validate kích thước file (max 2MB)
                if (file.size > 2 * 1024 * 1024) {
                    alert('Kích thước ảnh không được vượt quá 2MB');
                    this.value = '';
                    return;
                }

                // Validate loại file
                const validTypes = ['image/jpeg', 'image/png', 'image/jpg'];
                if (!validTypes.includes(file.type)) {
                    alert('Chỉ chấp nhận định dạng JPG, JPEG hoặc PNG');
                    this.value = '';
                    return;
                }

                const reader = new FileReader();

                reader.onload = function (e) {
                    // Nếu shopImagePreview là thẻ img
                    if (shopImagePreview.tagName === 'IMG') {
                        shopImagePreview.src = e.target.result;
                    }
                    // Nếu shopImagePreview là div (placeholder)
                    else {
                        // Xóa nội dung placeholder
                        while (shopImagePreview.firstChild) {
                            shopImagePreview.removeChild(shopImagePreview.firstChild);
                        }

                        // Tạo và thêm hình ảnh mới
                        const img = document.createElement('img');
                        img.src = e.target.result;
                        img.classList.add('shop-image');
                        shopImagePreview.appendChild(img);
                        shopImagePreview.classList.add('has-image');
                        shopImagePreview.classList.remove('no-image-placeholder');
                    }
                };

                reader.readAsDataURL(file);
            }
        });
    }

    // Xử lý gửi form với hiệu ứng loading
    if (form && submitButton) {
        form.addEventListener('submit', function () {
            if (this.checkValidity()) {
                submitButton.classList.add('loading');
                submitButton.innerHTML = '<span>Đang lưu...</span>';
            }
        });
    }

    // Hiệu ứng cho các phần form khi cuộn
    const sections = document.querySelectorAll('.edit-section');
    const fadeInSection = function () {
        sections.forEach(section => {
            const sectionTop = section.getBoundingClientRect().top;
            const windowHeight = window.innerHeight;

            if (sectionTop < windowHeight - 100) {
                section.style.opacity = '1';
                section.style.transform = 'translateY(0)';
            }
        });
    };

    // Thiết lập style ban đầu
    sections.forEach(section => {
        section.style.opacity = '0';
        section.style.transform = 'translateY(20px)';
        section.style.transition = 'opacity 0.5s ease, transform 0.5s ease';
    });

    // Thực thi khi tải
    setTimeout(fadeInSection, 200);

    // Thực thi khi cuộn
    window.addEventListener('scroll', fadeInSection);
});