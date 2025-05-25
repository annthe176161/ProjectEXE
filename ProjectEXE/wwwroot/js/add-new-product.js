// ===== ADD PRODUCT SIMPLE JAVASCRIPT - FIXED =====

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

        // Upload area events - FIX: Only bind to specific clickable area
        const uploadPlaceholder = document.querySelector('.upload-placeholder');
        const uploadArea = document.querySelector('.image-upload-area');

        if (uploadPlaceholder && uploadArea && fileInput) {
            // Only bind click to placeholder, not the entire upload area
            uploadPlaceholder.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                fileInput.click();
            });

            // Drag and drop events
            uploadArea.addEventListener('dragover', handleDragOver);
            uploadArea.addEventListener('drop', handleDrop);
            uploadArea.addEventListener('dragleave', handleDragLeave);
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

    // Handle file selection - FIX: Prevent multiple triggers
    function handleFileSelect(e) {
        // Prevent event bubbling
        e.stopPropagation();

        const files = Array.from(e.target.files);

        // Only process if files are actually selected
        if (files.length > 0) {
            processFiles(files);
        }
    }

    // Handle drag over
    function handleDragOver(e) {
        e.preventDefault();
        e.stopPropagation();
        e.currentTarget.classList.add('drag-over');
    }

    // Handle drag leave
    function handleDragLeave(e) {
        e.preventDefault();
        e.stopPropagation();
        e.currentTarget.classList.remove('drag-over');
    }

    // Handle drop
    function handleDrop(e) {
        e.preventDefault();
        e.stopPropagation();
        e.currentTarget.classList.remove('drag-over');

        const files = Array.from(e.dataTransfer.files);
        if (files.length > 0) {
            processFiles(files);
        }
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
        previewContainer.className = 'image-preview-container';

        const previewHeader = document.createElement('div');
        previewHeader.className = 'preview-header';
        previewHeader.innerHTML = `
            <h6><i class="fas fa-images me-2"></i>Xem trước hình ảnh (${selectedFiles.length}/${maxImages})</h6>
            <button type="button" class="btn-clear-all" onclick="clearAllImages()">
                <i class="fas fa-trash"></i> Xóa tất cả
            </button>
        `;

        const previewGrid = document.createElement('div');
        previewGrid.className = 'preview-grid row';

        selectedFiles.forEach((file, index) => {
            const col = document.createElement('div');
            col.className = 'col-lg-2 col-md-3 col-sm-4 col-6 preview-item';

            const reader = new FileReader();
            reader.onload = function (e) {
                col.innerHTML = `
                    <div class="preview-card">
                        <img src="${e.target.result}" class="preview-image" alt="Preview ${index + 1}">
                        <div class="preview-overlay">
                            <button type="button" class="btn-remove" onclick="removeImage(${index})" title="Xóa ảnh">
                                <i class="fas fa-times"></i>
                            </button>
                        </div>
                        <div class="preview-footer">
                            <div class="image-label ${index === 0 ? 'main-label' : ''}">
                                ${index === 0 ? 'Ảnh chính' : `Ảnh ${index + 1}`}
                            </div>
                        </div>
                    </div>
                `;
            };
            reader.readAsDataURL(file);

            previewGrid.appendChild(col);
        });

        previewContainer.appendChild(previewHeader);
        previewContainer.appendChild(previewGrid);

        const uploadArea = document.querySelector('.image-upload-area');
        uploadArea.parentNode.insertAdjacentElement('afterend', previewContainer);
    }

    // Remove single image
    window.removeImage = function (index) {
        selectedFiles.splice(index, 1);
        updateFileInput();
        showPreview();
        showMessage('Đã xóa hình ảnh', 'success');
    };

    // Clear all images
    window.clearAllImages = function () {
        selectedFiles = [];
        updateFileInput();
        const previewContainer = document.querySelector('.image-preview-container');
        if (previewContainer) {
            previewContainer.remove();
        }
        showMessage('Đã xóa tất cả hình ảnh', 'success');
    };

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

// Enhanced toast message styles
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
    min-width: 280px;
}

.toast-message.show {
    transform: translateX(0);
}

.toast-message.toast-error {
    border-left-color: #dc3545;
}

.toast-message.toast-warning {
    border-left-color: #fd7e14;
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

/* Upload area states */
.image-upload-area.drag-over {
    border-color: var(--primary-green) !important;
    background-color: rgba(40, 167, 69, 0.05);
}

.upload-placeholder {
    cursor: pointer;
    transition: all 0.3s ease;
}

.upload-placeholder:hover {
    background-color: rgba(40, 167, 69, 0.02);
}

/* Preview styles */
.image-preview-container {
    margin-top: 20px;
    padding: 20px;
    background: #f8f9fa;
    border-radius: 12px;
    border: 1px solid #e9ecef;
}

.preview-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 15px;
    padding-bottom: 10px;
    border-bottom: 1px solid #dee2e6;
}

.preview-header h6 {
    margin: 0;
    color: var(--text-dark);
    font-weight: 600;
}

.btn-clear-all {
    background: #dc3545;
    color: white;
    border: none;
    padding: 6px 12px;
    border-radius: 6px;
    font-size: 12px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.3s ease;
}

.btn-clear-all:hover {
    background: #c82333;
    transform: translateY(-1px);
}

.preview-grid {
    gap: 15px;
}

.preview-item {
    margin-bottom: 15px;
}

.preview-card {
    position: relative;
    background: white;
    border-radius: 8px;
    overflow: hidden;
    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    transition: all 0.3s ease;
}

.preview-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 15px rgba(0,0,0,0.15);
}

.preview-image {
    width: 100%;
    height: 120px;
    object-fit: cover;
    display: block;
}

.preview-overlay {
    position: absolute;
    top: 5px;
    right: 5px;
    opacity: 0;
    transition: opacity 0.3s ease;
}

.preview-card:hover .preview-overlay {
    opacity: 1;
}

.btn-remove {
    background: rgba(220, 53, 69, 0.9);
    color: white;
    border: none;
    width: 24px;
    height: 24px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 12px;
    cursor: pointer;
    transition: all 0.3s ease;
}

.btn-remove:hover {
    background: #dc3545;
    transform: scale(1.1);
}

.preview-footer {
    padding: 8px;
    text-align: center;
}

.image-label {
    font-size: 11px;
    font-weight: 600;
    color: #6c757d;
    text-transform: uppercase;
    letter-spacing: 0.5px;
}

.main-label {
    color: var(--primary-green);
    background: rgba(40, 167, 69, 0.1);
    padding: 2px 6px;
    border-radius: 4px;
}

@media (max-width: 576px) {
    .toast-message {
        left: 10px;
        right: 10px;
        transform: translateY(-100%);
        min-width: auto;
    }
    .toast-message.show {
        transform: translateY(0);
    }
    
    .preview-header {
        flex-direction: column;
        gap: 10px;
        align-items: stretch;
    }
    
    .btn-clear-all {
        align-self: center;
        width: fit-content;
    }
    
    .preview-image {
        height: 100px;
    }
}
`;

// Inject styles
const styleSheet = document.createElement('style');
styleSheet.textContent = toastStyles;
document.head.appendChild(styleSheet);