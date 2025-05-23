document.addEventListener('DOMContentLoaded', function () {
    initializeShopOrderDetails();
});

function initializeShopOrderDetails() {
    setupStatusSelect();
    setupFormValidation();
    setupImageModal();
    setupAlertAutoHide();
    setupLoadingStates();
}

function setupStatusSelect() {
    const statusSelect = document.getElementById('statusSelect');
    const cancelReasonGroup = document.getElementById('cancelReasonGroup');

    if (statusSelect && cancelReasonGroup) {
        statusSelect.addEventListener('change', function () {
            const statusId = this.value;

            if (statusId == 5) {
                cancelReasonGroup.classList.remove('d-none');
                cancelReasonGroup.querySelector('textarea').required = true;

                // Add animation
                cancelReasonGroup.style.animation = 'fadeInUp 0.4s ease forwards';
            } else {
                cancelReasonGroup.classList.add('d-none');
                cancelReasonGroup.querySelector('textarea').required = false;
                cancelReasonGroup.querySelector('textarea').value = '';
            }
        });
    }
}

function setupFormValidation() {
    const form = document.getElementById('statusUpdateForm');
    const updateButton = document.getElementById('updateButton');

    if (form) {
        form.addEventListener('submit', function (e) {
            const statusSelect = document.getElementById('statusSelect');
            const cancelReasonGroup = document.getElementById('cancelReasonGroup');
            const cancelReasonTextarea = cancelReasonGroup.querySelector('textarea');

            // Validate cancel reason if status is cancelled
            if (statusSelect.value == 5 && !cancelReasonTextarea.value.trim()) {
                e.preventDefault();
                showToast('Vui lòng nhập lý do hủy đơn hàng', 'error');
                cancelReasonTextarea.focus();
                return false;
            }

            // Show loading state
            if (updateButton) {
                const originalText = updateButton.innerHTML;
                updateButton.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Đang cập nhật...';
                updateButton.disabled = true;

                // Re-enable after 10 seconds as fallback
                setTimeout(() => {
                    if (updateButton.disabled) {
                        updateButton.innerHTML = originalText;
                        updateButton.disabled = false;
                    }
                }, 10000);
            }
        });
    }
}

function setupImageModal() {
    const productImage = document.querySelector('.product-image');

    if (productImage) {
        productImage.addEventListener('click', function () {
            showImageModal(this.src, this.alt);
        });
    }
}

function showImageModal(imageSrc, imageAlt) {
    const modalHtml = `
        <div class="modal fade" id="imageModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-lg modal-dialog-centered">
                <div class="modal-content" style="background: transparent; border: none;">
                    <div class="modal-body p-0">
                        <img src="${imageSrc}" alt="${imageAlt}" class="img-fluid rounded" style="width: 100%;">
                        <button type="button" class="btn-close position-absolute top-0 end-0 m-3" 
                                data-bs-dismiss="modal" aria-label="Close" 
                                style="background: white; border-radius: 50%; padding: 0.75rem; box-shadow: 0 2px 10px rgba(0,0,0,0.3);"></button>
                    </div>
                </div>
            </div>
        </div>
    `;

    // Remove existing modal if any
    const existingModal = document.getElementById('imageModal');
    if (existingModal) {
        existingModal.remove();
    }

    // Add new modal
    document.body.insertAdjacentHTML('beforeend', modalHtml);

    // Show modal
    const modal = new bootstrap.Modal(document.getElementById('imageModal'));
    modal.show();

    // Clean up after modal is hidden
    document.getElementById('imageModal').addEventListener('hidden.bs.modal', function () {
        this.remove();
    });
}

function setupAlertAutoHide() {
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        // Add progress bar
        const progressBar = document.createElement('div');
        progressBar.style.cssText = `
            position: absolute;
            bottom: 0;
            left: 0;
            height: 3px;
            background: currentColor;
            opacity: 0.3;
            animation: alertProgress 5s linear forwards;
        `;
        alert.style.position = 'relative';
        alert.appendChild(progressBar);

        // Auto hide after 5 seconds
        setTimeout(() => {
            if (alert.classList.contains('show')) {
                alert.classList.remove('show');
                setTimeout(() => {
                    if (alert.parentNode) {
                        alert.remove();
                    }
                }, 150);
            }
        }, 5000);
    });

    // Add CSS for progress animation if not exists
    if (!document.getElementById('alert-progress-style')) {
        const style = document.createElement('style');
        style.id = 'alert-progress-style';
        style.textContent = `
            @keyframes alertProgress {
                from { width: 100%; }
                to { width: 0%; }
            }
        `;
        document.head.appendChild(style);
    }
}

function setupLoadingStates() {
    // Setup loading states for quick action buttons
    const quickActionBtns = document.querySelectorAll('.quick-action-btn');

    quickActionBtns.forEach(btn => {
        if (btn.onclick && btn.onclick.toString().includes('print')) return; // Skip print button

        btn.addEventListener('click', function () {
            if (this.href && this.href.startsWith('tel:')) {
                showToast('Đang mở ứng dụng gọi điện...', 'info');
            } else if (this.href && this.href.startsWith('mailto:')) {
                showToast('Đang mở ứng dụng email...', 'info');
            }
        });
    });
}

function showToast(message, type = 'info') {
    const toastId = 'toast-' + Date.now();
    const iconClass = {
        'success': 'fas fa-check-circle',
        'error': 'fas fa-exclamation-triangle',
        'warning': 'fas fa-exclamation-circle',
        'info': 'fas fa-info-circle'
    }[type] || 'fas fa-info-circle';

    const bgClass = {
        'success': 'bg-success',
        'error': 'bg-danger',
        'warning': 'bg-warning',
        'info': 'bg-info'
    }[type] || 'bg-info';

    const toast = document.createElement('div');
    toast.id = toastId;
    toast.className = `toast align-items-center text-white ${bgClass} border-0`;
    toast.setAttribute('role', 'alert');
    toast.style.cssText = 'min-width: 300px;';

    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                <i class="${iconClass} me-2"></i>
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>
    `;

    // Add to page
    let toastContainer = document.querySelector('.toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        toastContainer.style.zIndex = '9999';
        document.body.appendChild(toastContainer);
    }

    toastContainer.appendChild(toast);

    // Show toast
    if (typeof bootstrap !== 'undefined' && bootstrap.Toast) {
        const bsToast = new bootstrap.Toast(toast, {
            autohide: true,
            delay: 4000
        });
        bsToast.show();

        // Remove after hiding
        toast.addEventListener('hidden.bs.toast', function () {
            this.remove();
        });
    } else {
        // Fallback without Bootstrap
        toast.style.display = 'block';
        setTimeout(() => {
            toast.style.transition = 'opacity 0.3s ease';
            toast.style.opacity = '0';
            setTimeout(() => toast.remove(), 300);
        }, 4000);
    }
}

// Quick action handlers
function callCustomer(phone) {
    window.location.href = `tel:${phone}`;
    showToast('Đang kết nối cuộc gọi...', 'info');
}

function emailCustomer(email) {
    window.location.href = `mailto:${email}`;
    showToast('Đang mở ứng dụng email...', 'info');
}

function printOrder() {
    // Add print-specific styles before printing
    const printStyle = document.createElement('style');
    printStyle.id = 'print-styles';
    printStyle.textContent = `
        @media print {
            body * { visibility: hidden; }
            .shop-order-details-page, .shop-order-details-page * { visibility: visible; }
            .page-header, .quick-actions, .form-actions, .btn, .alert { display: none !important; }
            .info-card { box-shadow: none; border: 1px solid #ddd; break-inside: avoid; page-break-inside: avoid; }
            .shop-order-details-page { background: white !important; }
            .container { max-width: none !important; margin: 0 !important; padding: 0 !important; }
        }
    `;
    document.head.appendChild(printStyle);

    // Print
    window.print();

    // Remove print styles after printing
    setTimeout(() => {
        const printStyleEl = document.getElementById('print-styles');
        if (printStyleEl) {
            printStyleEl.remove();
        }
    }, 1000);

    showToast('Đã gửi lệnh in đơn hàng', 'success');
}

// Add smooth scrolling for any anchor links
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            target.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
    });
});

// Add loading animation for external links
document.querySelectorAll('a[target="_blank"]').forEach(link => {
    link.addEventListener('click', function () {
        const icon = this.querySelector('i');
        if (icon) {
            const originalClass = icon.className;
            icon.className = 'fas fa-spinner fa-spin';
            setTimeout(() => {
                icon.className = originalClass;
            }, 2000);
        }
        showToast('Đang mở trang trong tab mới...', 'info');
    });
});