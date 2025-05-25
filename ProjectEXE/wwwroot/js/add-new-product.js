// ===== ADD PRODUCT SIMPLE JAVASCRIPT =====

(function () {
    'use strict';

    let selectedFiles = [];
    const maxImages = 5;

    // Initialize
    function init() {
        bindEvents();
        setupValidation();
    }

    // Bind events
    function bindEvents() {
        const fileInput = document.getElementById('NewImages');
        if (fileInput) {
            fileInput.addEventListener('change', handleFileSelect);
        }

        // Upload area click
        const uploadArea = document.querySelector('.image-upload-area');
        if (uploadArea) {
            uploadArea.addEventListener('click', () => fileInput.click());
            uploadArea.addEventListener('dragover', handleDragOver);
            uploadArea.addEventListener('drop', handleDrop);
        }

        // Form submit
        const form = document.getElementById('productForm');
        if (form) {
            form.addEventListener('submit', handleSubmit);
        }

        // Price formatting
        const priceInput = document.querySelector('input[name="Price"]');
        if (priceInput) {
            priceInput.addEventListener('input', formatPrice);
        }
    }

    // Handle file selection
    function handleFileSelect(e) {
        const files = Array.from(e.target.files);
        processFiles(files);
    }

    // Handle drag over
    function handleDragOver(e) {
        e.preventDefault();
        e.currentTarget.style.borderColor = 'var(--primary-green)';
    }

    // Handle drop
    function handleDrop(e) {
        e.preventDefault();
        const files = Array.from(e.dataTransfer.files);
        processFiles(files);
        e.currentTarget.style.borderColor = '#d1ecf1';
    }

    // Process files
    function processFiles(files) {
        const imageFiles = files.filter(file => file.type.startsWith('image/'));

        if (imageFiles.length === 0) {
            showMessage('Vui lòng chọn file hình ảnh!', 'error');
            return;
        }

        if (selectedFiles.length + imageFiles.length > maxImages) {
            showMessage(`Chỉ được phép tải lên tối đa ${maxImages} hình ảnh!`, 'error');
            return;
        }

        selectedFiles = selectedFiles.concat(imageFiles);
        updateFileInput();
        showPreview();
        showMessage(`Đã thêm ${imageFiles.length} hình ảnh`, 'success');
    }

    // Update file input
    function updateFileInput() {
        const fileInput = document.getElementById('NewImages');
        if (!fileInput) return;

        const dt = new DataTransfer();
        selectedFiles.forEach(file => dt.items.add(file));
        fileInput.files = dt.files;
    }

    // Show preview
    function showPreview() {
        let previewContainer = document.querySelector('.image-preview-container');

        if (previewContainer) {
            previewContainer.remove();
        }

        if (selectedFiles.length === 0) return;

        previewContainer = document.createElement('div');
        previewContainer.className = 'image-preview-container row';
        previewContainer.innerHTML = '<div class="col-12"><h6><i class="fas fa-images me-2"></i>Xem trước hình ảnh</h6></div>';

        selectedFiles.forEach((file, index) => {
            const col = document.createElement('div');
            col.className = 'col-lg-2 col-md-3 col-sm-4 col-6 preview-item';

            const reader = new FileReader();
            reader.onload = function (e) {
                col.innerHTML = `
                    <div class="preview-card">
                        <img src="${e.target.result}" class="preview-image" alt="Preview ${index + 1}">
                        <div class="preview-footer">
                            <div class="image-label ${index === 0 ? 'main-label' : ''}">
                                ${index === 0 ? 'Ảnh chính' : `Ảnh ${index + 1}`}
                            </div>
                        </div>
                    </div>
                `;
            };
            reader.readAsDataURL(file);

            previewContainer.appendChild(col);
        });

        const uploadArea = document.querySelector('.image-upload-area');
        uploadArea.parentNode.insertAdjacentElement('afterend', previewContainer);
    }

    // Format price
    function formatPrice(e) {
        let value = e.target.value.replace(/[^\d]/g, '');
        if (value) {
            value = parseInt(value).toLocaleString('vi-VN');
        }
        e.target.value = value;
    }

    // Handle form submit
    function handleSubmit(e) {
        if (!validateForm()) {
            e.preventDefault();
            return false;
        }

        const submitBtn = document.getElementById('submitBtn');
        if (submitBtn) {
            submitBtn.classList.add('btn-loading');
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';
        }
    }

    // Validate form
    function validateForm() {
        const requiredFields = [
            { name: 'ProductName', label: 'Tên sản phẩm' },
            { name: 'Description', label: 'Mô tả' },
            { name: 'Price', label: 'Giá bán' },
            { name: 'CategoryId', label: 'Danh mục' },
            { name: 'ConditionId', label: 'Tình trạng' }
        ];

        let isValid = true;

        requiredFields.forEach(field => {
            const input = document.querySelector(`[name="${field.name}"]`);
            if (input && !input.value.trim()) {
                showFieldError(input, `${field.label} là bắt buộc`);
                isValid = false;
            } else {
                clearFieldError(input);
            }
        });

        if (selectedFiles.length === 0) {
            showMessage('Vui lòng thêm ít nhất 1 hình ảnh!', 'error');
            isValid = false;
        }

        return isValid;
    }

    // Setup validation
    function setupValidation() {
        const inputs = document.querySelectorAll('input, textarea, select');
        inputs.forEach(input => {
            input.addEventListener('blur', () => {
                if (input.hasAttribute('required') || isRequiredField(input.name)) {
                    if (!input.value.trim()) {
                        showFieldError(input, 'Trường này là bắt buộc');
                    } else {
                        clearFieldError(input);
                    }
                }
            });

            input.addEventListener('input', () => clearFieldError(input));
        });
    }

    // Check if field is required
    function isRequiredField(name) {
        const required = ['ProductName', 'Description', 'Price', 'CategoryId', 'ConditionId'];
        return required.includes(name);
    }

    // Show field error
    function showFieldError(field, message) {
        field.classList.add('is-invalid');

        let errorElement = field.parentNode.querySelector('.field-error');
        if (!errorElement) {
            errorElement = document.createElement('div');
            errorElement.className = 'field-error text-danger';
            field.parentNode.appendChild(errorElement);
        }

        errorElement.textContent = message;
    }

    // Clear field error
    function clearFieldError(field) {
        field.classList.remove('is-invalid');

        const errorElement = field.parentNode.querySelector('.field-error');
        if (errorElement) {
            errorElement.remove();
        }
    }

    // Show message
    function showMessage(message, type) {
        // Remove existing messages
        const existing = document.querySelectorAll('.toast-message');
        existing.forEach(el => el.remove());

        // Create message
        const toast = document.createElement('div');
        toast.className = `toast-message toast-${type}`;
        toast.innerHTML = `
            <div class="toast-content">
                <i class="fas ${getIcon(type)}"></i>
                <span>${message}</span>
            </div>
        `;

        // Add to page
        document.body.appendChild(toast);

        // Show and auto hide
        setTimeout(() => toast.classList.add('show'), 100);
        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    }

    // Get icon for message type
    function getIcon(type) {
        const icons = {
            success: 'fa-check-circle',
            error: 'fa-exclamation-circle',
            warning: 'fa-exclamation-triangle'
        };
        return icons[type] || 'fa-info-circle';
    }

    // Initialize when DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();

// Toast message styles
const toastStyles = `
.toast-message {
    position: fixed;
    top: 20px;
    right: 20px;
    z-index: 9999;
    background: white;
    border-radius: 8px;
    box-shadow: 0 4px 12px rgba(0,0,0,0.15);
    padding: 12px 16px;
    transform: translateX(100%);
    transition: transform 0.3s ease;
    border-left: 4px solid #28a745;
}

.toast-message.show {
    transform: translateX(0);
}

.toast-message.toast-error {
    border-left-color: #dc3545;
}

.toast-content {
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 14px;
    font-weight: 500;
}

.toast-success .toast-content i { color: #28a745; }
.toast-error .toast-content i { color: #dc3545; }
.toast-warning .toast-content i { color: #fd7e14; }

@media (max-width: 576px) {
    .toast-message {
        left: 10px;
        right: 10px;
        transform: translateY(-100%);
    }
    .toast-message.show {
        transform: translateY(0);
    }
}
`;

// Inject styles
const styleSheet = document.createElement('style');
styleSheet.textContent = toastStyles;
document.head.appendChild(styleSheet);