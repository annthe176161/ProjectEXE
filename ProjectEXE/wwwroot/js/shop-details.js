/* filepath: wwwroot/js/shop-details.js */

document.addEventListener('DOMContentLoaded', function () {
    initializeShopDetails();
});

function initializeShopDetails() {
    setupViewToggle();
    setupProductCards();
    setupQuickActions();
    setupScrollAnimations();
}

function setupViewToggle() {
    const toggleBtns = document.querySelectorAll('.toggle-btn');
    const productsContainer = document.getElementById('productsContainer');

    toggleBtns.forEach(btn => {
        btn.addEventListener('click', function () {
            const view = this.dataset.view;

            // Update active state
            toggleBtns.forEach(b => b.classList.remove('active'));
            this.classList.add('active');

            // Toggle view
            if (view === 'list') {
                productsContainer.classList.add('list-view');
            } else {
                productsContainer.classList.remove('list-view');
            }

            // Save preference
            localStorage.setItem('shopProductView', view);
        });
    });

    // Load saved preference
    const savedView = localStorage.getItem('shopProductView');
    if (savedView === 'list') {
        document.querySelector('[data-view="list"]').click();
    }
}

function setupProductCards() {
    const productCards = document.querySelectorAll('.product-card');

    productCards.forEach(card => {
        // Add loading state for images
        const img = card.querySelector('.product-image');
        if (img) {
            img.addEventListener('load', function () {
                this.style.opacity = '1';
            });

            img.addEventListener('error', function () {
                this.src = '/images/placeholder-product.jpg';
            });
        }

        // Add hover effects
        card.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-8px)';
        });

        card.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0)';
        });
    });
}

function setupQuickActions() {
    // Quick view buttons
    const quickViewBtns = document.querySelectorAll('.quick-view');
    quickViewBtns.forEach(btn => {
        btn.addEventListener('click', function () {
            const productId = this.dataset.id;
            showQuickView(productId);
        });
    });

    // Add to cart buttons
    const addToCartBtns = document.querySelectorAll('.add-to-cart, .btn-cart');
    addToCartBtns.forEach(btn => {
        btn.addEventListener('click', function () {
            const productId = this.dataset.id;
            addToCart(productId);
        });
    });
}

function showQuickView(productId) {
    // Show loading state
    showToast('Đang tải thông tin sản phẩm...', 'info');

    // Simulate loading (replace with actual API call)
    setTimeout(() => {
        // You would typically fetch product details and show in a modal
        showToast('Tính năng xem nhanh sẽ được cập nhật sớm!', 'info');
    }, 1000);
}

function addToCart(productId) {
    const button = document.querySelector(`[data-id="${productId}"].btn-cart, [data-id="${productId}"].add-to-cart`);

    if (button) {
        // Show loading state
        const originalText = button.innerHTML;
        button.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Đang thêm...';
        button.disabled = true;

        // Simulate API call
        setTimeout(() => {
            button.innerHTML = '<i class="fas fa-check me-1"></i>Đã thêm';
            button.classList.add('btn-success');
            button.classList.remove('btn-primary');

            showToast('Đã thêm sản phẩm vào giỏ hàng!', 'success');

            // Reset button after 2 seconds
            setTimeout(() => {
                button.innerHTML = originalText;
                button.disabled = false;
                button.classList.remove('btn-success');
                button.classList.add('btn-primary');
            }, 2000);
        }, 1000);
    }
}

function setupScrollAnimations() {
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('animate__animated', 'animate__fadeInUp');
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    // Observe product cards
    const productCards = document.querySelectorAll('.product-card-wrapper');
    productCards.forEach((card, index) => {
        card.style.animationDelay = `${index * 0.1}s`;
        observer.observe(card);
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
    toast.style.cssText = 'min-width: 300px; z-index: 9999;';

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
        toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
        toastContainer.style.zIndex = '9999';
        document.body.appendChild(toastContainer);
    }

    toastContainer.appendChild(toast);

    // Show toast
    if (typeof bootstrap !== 'undefined' && bootstrap.Toast) {
        const bsToast = new bootstrap.Toast(toast, {
            autohide: true,
            delay: 3000
        });
        bsToast.show();

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
        }, 3000);
    }
}

// Smooth scroll for anchor links
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

// Lazy loading for product images
if ('IntersectionObserver' in window) {
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                img.src = img.dataset.src || img.src;
                img.classList.remove('lazy');
                observer.unobserve(img);
            }
        });
    });

    document.querySelectorAll('img[data-src]').forEach(img => {
        imageObserver.observe(img);
    });
}