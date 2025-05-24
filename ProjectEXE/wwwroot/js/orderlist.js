document.addEventListener('DOMContentLoaded', function () {
    initializeOrderList();
});

function initializeOrderList() {
    setupSearch();
    setupFilters();
    setupAnimations();
    setupAlertAutoHide();
}

function setupSearch() {
    const searchInput = document.getElementById('searchOrders');
    if (searchInput) {
        searchInput.addEventListener('input', debounce(filterOrders, 300));
    }
}

function setupFilters() {
    const statusFilter = document.getElementById('statusFilter');
    if (statusFilter) {
        statusFilter.addEventListener('change', filterOrders);
    }
}

function filterOrders() {
    const searchTerm = document.getElementById('searchOrders').value.toLowerCase();
    const statusFilter = document.getElementById('statusFilter').value;
    const orderCards = document.querySelectorAll('.order-card');

    orderCards.forEach(card => {
        const searchData = card.getAttribute('data-search').toLowerCase();
        const orderStatus = card.getAttribute('data-status');

        const matchesSearch = searchData.includes(searchTerm);
        const matchesStatus = !statusFilter || orderStatus === statusFilter;

        if (matchesSearch && matchesStatus) {
            card.style.display = 'block';
            card.style.animation = 'fadeInUp 0.4s ease forwards';
        } else {
            card.style.display = 'none';
        }
    });

    // Show empty message if no results
    updateEmptyMessage();
}

function updateEmptyMessage() {
    const visibleCards = document.querySelectorAll('.order-card[style*="display: block"], .order-card:not([style*="display: none"])');
    const container = document.getElementById('ordersContainer');

    let emptyMessage = container.querySelector('.filter-empty-message');

    if (visibleCards.length === 0) {
        if (!emptyMessage) {
            emptyMessage = document.createElement('div');
            emptyMessage.className = 'filter-empty-message text-center py-5';
            emptyMessage.innerHTML = `
                <div class="empty-icon mb-3">
                    <i class="fas fa-search" style="font-size: 3rem; color: var(--text-muted);"></i>
                </div>
                <h4>Không tìm thấy đơn hàng</h4>
                <p class="text-muted">Hãy thử điều chỉnh bộ lọc hoặc từ khóa tìm kiếm</p>
            `;
            container.appendChild(emptyMessage);
        }
        emptyMessage.style.display = 'block';
    } else if (emptyMessage) {
        emptyMessage.style.display = 'none';
    }
}

function setupAnimations() {
    // Stagger animation for order cards
    const orderCards = document.querySelectorAll('.order-card');
    orderCards.forEach((card, index) => {
        card.style.animationDelay = `${index * 0.1}s`;
    });

    // Hover effects for order cards
    orderCards.forEach(card => {
        card.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-6px) scale(1.02)';
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

function cancelOrder(orderId) {
    if (confirm('Bạn có chắc chắn muốn hủy đơn hàng này?')) {
        // Show loading state
        const button = event.target.closest('button');
        const originalText = button.innerHTML;
        button.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Đang hủy...';
        button.disabled = true;

        // Simulate API call (replace with actual implementation)
        setTimeout(() => {
            showToast('Đơn hàng đã được hủy thành công', 'success');
            // Reload page or update card status
            location.reload();
        }, 1500);
    }
}

function showToast(message, type = 'info') {
    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-white bg-${type} border-0`;
    toast.setAttribute('role', 'alert');
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                <i class="fas fa-${type === 'success' ? 'check-circle' : 'info-circle'} me-2"></i>
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

// Utility function for debouncing
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Image modal functionality
function showImageModal(imageSrc, imageAlt) {
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

// Setup image click handlers
document.addEventListener('click', function (e) {
    if (e.target.matches('.product-image')) {
        showImageModal(e.target.src, e.target.alt);
    }
});