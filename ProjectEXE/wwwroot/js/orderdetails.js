document.addEventListener('DOMContentLoaded', function () {
    // Initialize page
    initializeOrderDetails();
});

function initializeOrderDetails() {
    // Enhanced cancel reason handler
    setupCancelReasonHandler();

    // Add smooth animations
    setupAnimations();

    // Auto-hide alerts
    setupAlertAutoHide();

    // Setup image modal/lightbox
    setupImageViewer();

    // Setup tooltips
    setupTooltips();
}

function setupCancelReasonHandler() {
    const reasonSelect = document.getElementById('reason');
    const otherReasonContainer = document.getElementById('otherReasonContainer');
    const otherReasonTextarea = document.getElementById('otherReason');

    if (reasonSelect) {
        reasonSelect.addEventListener('change', function () {
            if (this.value === 'Khác') {
                otherReasonContainer.classList.remove('d-none');
                otherReasonTextarea.setAttribute('required', '');
                otherReasonTextarea.focus();

                // Smooth scroll to textarea
                setTimeout(() => {
                    otherReasonTextarea.scrollIntoView({
                        behavior: 'smooth',
                        block: 'center'
                    });
                }, 100);
            } else {
                otherReasonContainer.classList.add('d-none');
                otherReasonTextarea.removeAttribute('required');
                otherReasonTextarea.value = '';
            }
        });
    }
}

function setupAnimations() {
    // Animate cards on load
    const cards = document.querySelectorAll('.info-card');
    cards.forEach((card, index) => {
        card.style.animationDelay = `${index * 0.1}s`;
        card.classList.add('animate__animated', 'animate__fadeInUp');
    });

    // Animate timeline steps
    const timelineSteps = document.querySelectorAll('.timeline-step');
    timelineSteps.forEach((step, index) => {
        setTimeout(() => {
            step.style.opacity = '0';
            step.style.transform = 'translateX(-20px)';
            step.style.transition = 'all 0.5s ease';

            setTimeout(() => {
                step.style.opacity = '1';
                step.style.transform = 'translateX(0)';
            }, 100);
        }, index * 200);
    });

    // Hover effects for cards
    cards.forEach(card => {
        card.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-4px) scale(1.02)';
        });

        card.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0) scale(1)';
        });
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

    // Add CSS for progress animation
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

function setupImageViewer() {
    const productImage = document.querySelector('.product-image');
    if (productImage) {
        productImage.addEventListener('click', function () {
            showImageModal(this.src, this.alt);
        });
    }
}

function showImageModal(imageSrc, imageAlt) {
    // Create modal dynamically
    const modalHtml = `
        <div class="modal fade" id="imageModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-lg modal-dialog-centered">
                <div class="modal-content" style="background: transparent; border: none;">
                    <div class="modal-body p-0">
                        <img src="${imageSrc}" alt="${imageAlt}" class="img-fluid rounded" style="width: 100%;">
                        <button type="button" class="btn-close position-absolute top-0 end-0 m-3" 
                                data-bs-dismiss="modal" aria-label="Close" 
                                style="background: white; border-radius: 50%; padding: 0.5rem;"></button>
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

function setupTooltips() {
    // Initialize Bootstrap tooltips if available
    if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }
}

// Form validation for cancel modal
function validateCancelForm() {
    const form = document.querySelector('#cancelOrderModal form');
    if (form) {
        form.addEventListener('submit', function (e) {
            const reason = document.getElementById('reason').value;
            const otherReason = document.getElementById('otherReason').value;

            if (!reason) {
                e.preventDefault();
                showValidationError('Vui lòng chọn lý do hủy đơn hàng');
                return false;
            }

            if (reason === 'Khác' && !otherReason.trim()) {
                e.preventDefault();
                showValidationError('Vui lòng mô tả chi tiết lý do hủy đơn');
                document.getElementById('otherReason').focus();
                return false;
            }

            // Show loading state
            const submitBtn = this.querySelector('button[type="submit"]');
            const originalText = submitBtn.innerHTML;
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Đang xử lý...';
            submitBtn.disabled = true;

            // Restore button if form submission fails
            setTimeout(() => {
                if (submitBtn.disabled) {
                    submitBtn.innerHTML = originalText;
                    submitBtn.disabled = false;
                }
            }, 10000);
        });
    }
}

function showValidationError(message) {
    // Create toast notification
    const toast = document.createElement('div');
    toast.className = 'toast align-items-center text-white bg-danger border-0';
    toast.setAttribute('role', 'alert');
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                <i class="fas fa-exclamation-circle me-2"></i>${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>
    `;

    // Add to page
    let toastContainer = document.querySelector('.toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        document.body.appendChild(toastContainer);
    }

    toastContainer.appendChild(toast);

    // Show toast
    if (typeof bootstrap !== 'undefined' && bootstrap.Toast) {
        const bsToast = new bootstrap.Toast(toast);
        bsToast.show();

        // Remove after hiding
        toast.addEventListener('hidden.bs.toast', function () {
            this.remove();
        });
    }
}

// Initialize form validation
document.addEventListener('DOMContentLoaded', function () {
    validateCancelForm();
});