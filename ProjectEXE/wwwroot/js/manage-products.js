// ===== MANAGE PRODUCTS JAVASCRIPT =====

(function () {
    'use strict';

    // Initialize the page
    function initManageProducts() {
        initFilters();
        initTableInteractions();
        initAnimations();
    }

    // Initialize filter functionality
    function initFilters() {
        const searchInput = document.getElementById('searchInput');
        const statusFilter = document.getElementById('statusFilter');
        const stockFilter = document.getElementById('stockFilter');

        if (searchInput) {
            searchInput.addEventListener('input', debounce(filterTable, 300));
        }

        if (statusFilter) {
            statusFilter.addEventListener('change', filterTable);
        }

        if (stockFilter) {
            stockFilter.addEventListener('change', filterTable);
        }
    }

    // Filter table function
    function filterTable() {
        const searchTerm = document.getElementById('searchInput')?.value.toLowerCase() || '';
        const statusFilter = document.getElementById('statusFilter')?.value || '';
        const stockFilter = document.getElementById('stockFilter')?.value || '';
        const rows = document.querySelectorAll('.product-row');

        let visibleCount = 0;

        rows.forEach((row, index) => {
            const productName = row.getAttribute('data-product-name') || '';
            const category = row.getAttribute('data-category') || '';
            const visible = row.getAttribute('data-visible') || '';
            const stock = row.getAttribute('data-stock') || '';

            let showRow = true;

            // Search filter
            if (searchTerm && !productName.includes(searchTerm) && !category.includes(searchTerm)) {
                showRow = false;
            }

            // Status filter
            if (statusFilter) {
                if (statusFilter === 'visible' && visible !== 'true') showRow = false;
                if (statusFilter === 'hidden' && visible !== 'false') showRow = false;
            }

            // Stock filter
            if (stockFilter && stock) {
                if (stockFilter === 'instock' && stock !== 'true') showRow = false;
                if (stockFilter === 'outofstock' && stock !== 'false') showRow = false;
            }

            if (showRow) {
                row.style.display = '';
                row.style.animationDelay = `${visibleCount * 0.1}s`;
                visibleCount++;
            } else {
                row.style.display = 'none';
            }
        });

        // Show no results message if needed
        showNoResultsMessage(visibleCount === 0);
    }

    // Show/hide no results message
    function showNoResultsMessage(show) {
        let noResultsRow = document.querySelector('.no-results-row');

        if (show && !noResultsRow) {
            const tbody = document.querySelector('.products-table tbody');
            noResultsRow = document.createElement('tr');
            noResultsRow.className = 'no-results-row';
            noResultsRow.innerHTML = `
                <td colspan="8" class="text-center py-5">
                    <div class="no-results">
                        <i class="fas fa-search-minus fa-3x text-muted mb-3"></i>
                        <h4 class="text-muted">Không tìm thấy sản phẩm</h4>
                        <p class="text-muted">Thử thay đổi bộ lọc hoặc từ khóa tìm kiếm</p>
                    </div>
                </td>
            `;
            tbody.appendChild(noResultsRow);
        } else if (!show && noResultsRow) {
            noResultsRow.remove();
        }
    }

    // Initialize table interactions
    function initTableInteractions() {
        // Add hover effects
        const rows = document.querySelectorAll('.product-row');
        rows.forEach(row => {
            row.addEventListener('mouseenter', function () {
                this.style.transform = 'translateX(4px)';
            });

            row.addEventListener('mouseleave', function () {
                this.style.transform = 'translateX(0)';
            });
        });

        // Add loading states to action buttons
        const actionButtons = document.querySelectorAll('.btn-action');
        actionButtons.forEach(button => {
            button.addEventListener('click', function () {
                if (this.href && !this.href.includes('#')) {
                    this.classList.add('loading');
                    this.style.pointerEvents = 'none';

                    // Remove loading after 3 seconds as fallback
                    setTimeout(() => {
                        this.classList.remove('loading');
                        this.style.pointerEvents = '';
                    }, 3000);
                }
            });
        });
    }

    // Initialize animations
    function initAnimations() {
        // Animate stats cards on scroll
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.style.animationDelay = `${Math.random() * 0.3}s`;
                    entry.target.classList.add('animate-in');
                }
            });
        }, observerOptions);

        // Observe stat cards
        document.querySelectorAll('.stat-card').forEach(card => {
            observer.observe(card);
        });
    }

    // Delete confirmation function
    window.confirmDelete = function (productId, productName) {
        const modal = document.getElementById('deleteModal');
        const productNameElement = document.getElementById('productNameToDelete');
        const deleteForm = document.getElementById('deleteForm');

        if (productNameElement) {
            productNameElement.textContent = productName;
        }

        if (deleteForm) {
            // Assuming you have a delete action URL pattern
            deleteForm.action = `/Shop/DeleteProduct/${productId}`;
        }

        // Show modal using Bootstrap
        if (window.bootstrap && window.bootstrap.Modal) {
            const bsModal = new bootstrap.Modal(modal);
            bsModal.show();
        }
    };

    // Utility function: debounce
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

    // Add loading animation CSS
    const style = document.createElement('style');
    style.textContent = `
        .btn-action.loading {
            position: relative;
            pointer-events: none;
            opacity: 0.7;
        }
        
        .btn-action.loading::after {
            content: "";
            position: absolute;
            width: 16px;
            height: 16px;
            margin: auto;
            border: 2px solid transparent;
            border-top-color: currentColor;
            border-radius: 50%;
            animation: spin 1s linear infinite;
        }
        
        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }
        
        .animate-in {
            animation: fadeInUp 0.6s ease forwards;
        }
        
        .no-results {
            padding: 2rem;
        }
        
        .no-results i {
            opacity: 0.5;
        }
    `;
    document.head.appendChild(style);

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initManageProducts);
    } else {
        initManageProducts();
    }

})();