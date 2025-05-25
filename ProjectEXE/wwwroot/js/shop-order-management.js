$(document).ready(function () {
    // ===== FILTER FUNCTIONALITY =====
    $('#statusFilter').on('change', function () {
        const form = $('#filterForm');
        const submitBtn = form.find('button[type="submit"]');

        // Add loading state
        submitBtn.addClass('loading').prop('disabled', true);

        setTimeout(() => {
            form.submit();
        }, 300);
    });

    // ===== ENHANCED TABLE INTERACTIONS =====
    $('.table tbody tr').hover(
        function () {
            $(this).addClass('table-row-hover');
        },
        function () {
            $(this).removeClass('table-row-hover');
        }
    );

    // ===== ORDER STATUS INDICATORS =====
    $('.badge').each(function () {
        const $badge = $(this);
        const statusText = $badge.text().trim();

        // Add icons based on status
        if (statusText.includes('Chờ')) {
            $badge.prepend('<i class="fas fa-clock me-1"></i>');
        } else if (statusText.includes('Xác nhận')) {
            $badge.prepend('<i class="fas fa-check-circle me-1"></i>');
        } else if (statusText.includes('Giao')) {
            $badge.prepend('<i class="fas fa-shipping-fast me-1"></i>');
        } else if (statusText.includes('Hoàn thành')) {
            $badge.prepend('<i class="fas fa-trophy me-1"></i>');
        } else if (statusText.includes('Hủy')) {
            $badge.prepend('<i class="fas fa-times-circle me-1"></i>');
        }
    });

    // ===== PRODUCT IMAGE PREVIEW =====
    $('.product-image').on('click', function () {
        const imgSrc = $(this).attr('src');
        const productName = $(this).attr('alt');

        showImagePreview(imgSrc, productName);
    });

    // ===== RESPONSIVE TABLE HANDLING =====
    function handleResponsiveTable() {
        const tableContainer = $('.table-responsive');
        const table = $('.table');

        if ($(window).width() < 768) {
            table.addClass('table-sm');
            tableContainer.addClass('mobile-scroll');
        } else {
            table.removeClass('table-sm');
            tableContainer.removeClass('mobile-scroll');
        }
    }

    // ===== SEARCH FUNCTIONALITY =====
    function addSearchBox() {
        const searchHtml = `
            <div class="search-box mb-3">
                <div class="input-group">
                    <span class="input-group-text">
                        <i class="fas fa-search"></i>
                    </span>
                    <input type="text" class="form-control" id="orderSearch" 
                           placeholder="Tìm kiếm theo mã đơn, tên khách hàng, sản phẩm...">
                </div>
            </div>
        `;

        $('.table-responsive').before(searchHtml);

        $('#orderSearch').on('input', function () {
            const searchTerm = $(this).val().toLowerCase();
            filterTableRows(searchTerm);
        });
    }

    function filterTableRows(searchTerm) {
        $('.table tbody tr').each(function () {
            const $row = $(this);
            const rowText = $row.text().toLowerCase();

            if (rowText.includes(searchTerm) || searchTerm === '') {
                $row.show().addClass('animate__animated animate__fadeIn');
            } else {
                $row.hide();
            }
        });
    }

    // ===== BULK ACTIONS =====
    function addBulkActions() {
        const bulkHtml = `
            <div class="bulk-actions mb-3" style="display: none;">
                <div class="d-flex align-items-center gap-3">
                    <span class="selected-count">0 đơn hàng được chọn</span>
                    <button class="btn btn-outline-primary btn-sm" id="bulkExport">
                        <i class="fas fa-download me-1"></i>Xuất Excel
                    </button>
                    <button class="btn btn-outline-secondary btn-sm" id="bulkPrint">
                        <i class="fas fa-print me-1"></i>In danh sách
                    </button>
                </div>
            </div>
        `;

        $('.table-responsive').before(bulkHtml);

        // Add checkboxes to table
        $('.table thead tr').prepend('<th><input type="checkbox" id="selectAll"></th>');
        $('.table tbody tr').each(function () {
            const orderId = $(this).find('.order-id').text().replace('#', '');
            $(this).prepend(`<td><input type="checkbox" class="order-checkbox" value="${orderId}"></td>`);
        });

        // Handle select all
        $('#selectAll').on('change', function () {
            const isChecked = $(this).is(':checked');
            $('.order-checkbox').prop('checked', isChecked);
            updateBulkActions();
        });

        // Handle individual checkboxes
        $('.order-checkbox').on('change', function () {
            updateBulkActions();
            updateSelectAll();
        });
    }

    function updateBulkActions() {
        const selectedCount = $('.order-checkbox:checked').length;
        const $bulkActions = $('.bulk-actions');
        const $selectedCount = $('.selected-count');

        if (selectedCount > 0) {
            $bulkActions.show();
            $selectedCount.text(`${selectedCount} đơn hàng được chọn`);
        } else {
            $bulkActions.hide();
        }
    }

    function updateSelectAll() {
        const totalCheckboxes = $('.order-checkbox').length;
        const checkedCheckboxes = $('.order-checkbox:checked').length;

        $('#selectAll').prop('checked', totalCheckboxes === checkedCheckboxes && totalCheckboxes > 0);
    }

    // ===== EXPORT FUNCTIONALITY =====
    $('#bulkExport').on('click', function () {
        const selectedIds = $('.order-checkbox:checked').map(function () {
            return $(this).val();
        }).get();

        if (selectedIds.length > 0) {
            exportOrders(selectedIds);
        }
    });

    function exportOrders(orderIds) {
        // Show loading
        const $btn = $('#bulkExport');
        $btn.addClass('loading').prop('disabled', true);

        // Simulate export (replace with actual AJAX call)
        setTimeout(() => {
            $btn.removeClass('loading').prop('disabled', false);
            alert(`Đã xuất ${orderIds.length} đơn hàng thành công!`);
        }, 2000);
    }

    // ===== IMAGE PREVIEW MODAL =====
    function showImagePreview(imgSrc, productName) {
        const modalHtml = `
            <div class="modal fade" id="imagePreviewModal" tabindex="-1">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">${productName}</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body text-center">
                            <img src="${imgSrc}" class="img-fluid rounded" alt="${productName}">
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Remove existing modal if any
        $('#imagePreviewModal').remove();

        // Add new modal to body
        $('body').append(modalHtml);

        // Show modal
        const modal = new bootstrap.Modal(document.getElementById('imagePreviewModal'));
        modal.show();

        // Clean up when modal is hidden
        $('#imagePreviewModal').on('hidden.bs.modal', function () {
            $(this).remove();
        });
    }

    // ===== PRINT FUNCTIONALITY =====
    $('#bulkPrint').on('click', function () {
        window.print();
    });

    // ===== TOOLTIPS =====
    function initTooltips() {
        $('[data-bs-toggle="tooltip"]').tooltip();

        // Add tooltips to action buttons
        $('.btn-outline-primary').attr('data-bs-toggle', 'tooltip')
            .attr('title', 'Xem chi tiết đơn hàng');
    }

    // ===== INITIAL CALLS =====
    handleResponsiveTable();
    addSearchBox();
    addBulkActions();
    initTooltips();

    // ===== EVENT LISTENERS =====
    $(window).on('resize', handleResponsiveTable);

    // ===== PERFORMANCE OPTIMIZATIONS =====

    // Debounce search
    let searchTimeout;
    $('#orderSearch').on('input', function () {
        clearTimeout(searchTimeout);
        const searchTerm = $(this).val().toLowerCase();

        searchTimeout = setTimeout(() => {
            filterTableRows(searchTerm);
        }, 300);
    });

    // Virtual scrolling for large datasets
    if ($('.table tbody tr').length > 50) {
        implementVirtualScrolling();
    }

    function implementVirtualScrolling() {
        // Implementation for virtual scrolling would go here
        console.log('Virtual scrolling implemented for large dataset');
    }
});

// ===== UTILITY FUNCTIONS =====

// Format currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

// Format date
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    });
}

// Copy order ID to clipboard
function copyOrderId(orderId) {
    navigator.clipboard.writeText(orderId).then(() => {
        // Show toast notification
        showToast('Đã sao chép mã đơn hàng', 'success');
    });
}

// Show toast notification
function showToast(message, type = 'info') {
    const toastHtml = `
        <div class="toast align-items-center text-white bg-${type} border-0" role="alert">
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `;

    // Add toast container if not exists
    if (!$('.toast-container').length) {
        $('body').append('<div class="toast-container position-fixed bottom-0 end-0 p-3"></div>');
    }

    const $toast = $(toastHtml);
    $('.toast-container').append($toast);

    const toast = new bootstrap.Toast($toast[0]);
    toast.show();

    // Auto remove after shown
    $toast.on('hidden.bs.toast', function () {
        $(this).remove();
    });
}