/* filepath: wwwroot/js/admin-package-payments.js */

document.addEventListener('DOMContentLoaded', function () {
    initializePackagePayments();
});

function initializePackagePayments() {
    setupTableInteractions();
    setupStatCards();
    setupRefreshTimer();
    setupActionButtons();
    setupSearch();
}

function setupTableInteractions() {
    const tableRows = document.querySelectorAll('.table-row');

    tableRows.forEach(row => {
        row.addEventListener('click', function (e) {
            // Don't trigger row click if clicking on buttons
            if (e.target.closest('.btn')) {
                return;
            }

            // Add selection state
            tableRows.forEach(r => r.classList.remove('selected'));
            this.classList.add('selected');
        });
    });
}

function setupStatCards() {
    const statCards = document.querySelectorAll('.stat-card');

    statCards.forEach(card => {
        card.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-4px) scale(1.02)';
        });

        card.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(-2px) scale(1)';
        });
    });
}

function setupRefreshTimer() {
    const refreshInterval = 30000; // 30 seconds

    setInterval(() => {
        refreshPendingCount();
    }, refreshInterval);
}

function refreshPendingCount() {
    // Simulate API call to get updated pending count
    fetch('/Admin/PackagePayment/GetPendingCount')
        .then(response => response.json())
        .then(data => {
            updatePendingDisplay(data.count);
        })
        .catch(error => {
            console.log('Failed to refresh pending count:', error);
        });
}

function updatePendingDisplay(count) {
    const pendingElements = document.querySelectorAll('.stat-number');
    if (pendingElements.length > 0) {
        const currentCount = parseInt(pendingElements[0].textContent);
        if (currentCount !== count) {
            // Animate the change
            animateNumber(pendingElements[0], currentCount, count);

            // Show notification if there are new pending payments
            if (count > currentCount) {
                showNotification(`Có ${count - currentCount} thanh toán mới cần xử lý!`, 'info');
            }
        }
    }
}

function animateNumber(element, from, to) {
    const duration = 1000;
    const increment = (to - from) / (duration / 16);
    let current = from;

    const timer = setInterval(() => {
        current += increment;
        if ((increment > 0 && current >= to) || (increment < 0 && current <= to)) {
            current = to;
            clearInterval(timer);
        }
        element.textContent = Math.round(current);
    }, 16);
}

function setupActionButtons() {
    const actionButtons = document.querySelectorAll('.btn');

    actionButtons.forEach(btn => {
        btn.addEventListener('click', function (e) {
            if (this.classList.contains('loading')) {
                e.preventDefault();
                return;
            }

            // Add loading state for navigation buttons
            if (this.href && !this.href.includes('#')) {
                this.classList.add('loading');
                this.style.pointerEvents = 'none';

                setTimeout(() => {
                    this.classList.remove('loading');
                    this.style.pointerEvents = '';
                }, 2000);
            }
        });
    });
}

function setupSearch() {
    // Add search functionality if needed
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        let searchTimeout;

        searchInput.addEventListener('input', function () {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                performSearch(this.value);
            }, 300);
        });
    }
}

function performSearch(query) {
    const rows = document.querySelectorAll('.table-row');

    rows.forEach(row => {
        const text = row.textContent.toLowerCase();
        const matches = text.includes(query.toLowerCase());

        row.style.display = matches ? '' : 'none';

        if (matches && query) {
            // Highlight matching text
            highlightText(row, query);
        } else {
            // Remove highlights
            removeHighlights(row);
        }
    });
}

function highlightText(element, query) {
    // Simple text highlighting implementation
    const walker = document.createTreeWalker(
        element,
        NodeFilter.SHOW_TEXT,
        null,
        false
    );

    const textNodes = [];
    let node;

    while (node = walker.nextNode()) {
        textNodes.push(node);
    }

    textNodes.forEach(textNode => {
        const parent = textNode.parentNode;
        if (parent.tagName === 'MARK') return; // Already highlighted

        const text = textNode.textContent;
        const regex = new RegExp(`(${query})`, 'gi');

        if (regex.test(text)) {
            const highlightedHTML = text.replace(regex, '<mark>$1</mark>');
            const wrapper = document.createElement('span');
            wrapper.innerHTML = highlightedHTML;
            parent.replaceChild(wrapper, textNode);
        }
    });
}

function removeHighlights(element) {
    const marks = element.querySelectorAll('mark');
    marks.forEach(mark => {
        const parent = mark.parentNode;
        parent.replaceChild(document.createTextNode(mark.textContent), mark);
        parent.normalize();
    });
}

function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.innerHTML = `
        <div class="notification-content">
            <i class="fas fa-${getNotificationIcon(type)} me-2"></i>
            <span>${message}</span>
        </div>
        <button class="notification-close" onclick="this.parentElement.remove()">
            <i class="fas fa-times"></i>
        </button>
    `;

    // Add styles
    Object.assign(notification.style, {
        position: 'fixed',
        top: '1rem',
        right: '1rem',
        zIndex: '9999',
        background: 'var(--white)',
        borderRadius: 'var(--radius)',
        boxShadow: 'var(--shadow-xl)',
        padding: '1rem',
        minWidth: '300px',
        border: `1px solid var(--${type === 'info' ? 'info' : type}-color)`,
        borderLeft: `4px solid var(--${type === 'info' ? 'info' : type}-color)`,
        animation: 'slideInRight 0.3s ease-out'
    });

    document.body.appendChild(notification);

    // Auto remove after 5 seconds
    setTimeout(() => {
        notification.style.animation = 'slideOutRight 0.3s ease-in';
        setTimeout(() => {
            if (notification.parentNode) {
                notification.remove();
            }
        }, 300);
    }, 5000);
}

function getNotificationIcon(type) {
    const icons = {
        'success': 'check-circle',
        'error': 'exclamation-triangle',
        'warning': 'exclamation-circle',
        'info': 'info-circle'
    };
    return icons[type] || 'info-circle';
}

// Add notification animations
const style = document.createElement('style');
style.textContent = `
    @keyframes slideInRight {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    @keyframes slideOutRight {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(100%);
            opacity: 0;
        }
    }
    
    .notification-content {
        display: flex;
        align-items: center;
        margin-bottom: 0.5rem;
    }
    
    .notification-close {
        background: none;
        border: none;
        cursor: pointer;
        padding: 0.25rem;
        border-radius: var(--radius-sm);
        color: var(--gray-400);
        transition: var(--transition);
        position: absolute;
        top: 0.5rem;
        right: 0.5rem;
    }
    
    .notification-close:hover {
        background: var(--gray-100);
        color: var(--gray-600);
    }
    
    .table-row.selected {
        background: var(--primary-color);
        color: var(--white);
    }
    
    .table-row.selected .user-email,
    .table-row.selected .time,
    .table-row.selected .stat-description {
        color: rgba(255, 255, 255, 0.8);
    }
`;
document.head.appendChild(style);

// Handle page visibility changes
document.addEventListener('visibilitychange', function () {
    if (!document.hidden) {
        // Refresh data when page becomes visible
        refreshPendingCount();
    }
});

// Keyboard shortcuts
document.addEventListener('keydown', function (e) {
    // Ctrl/Cmd + R to refresh
    if ((e.ctrlKey || e.metaKey) && e.key === 'r') {
        e.preventDefault();
        refreshPendingCount();
        showNotification('Đang làm mới dữ liệu...', 'info');
    }

    // Escape to clear selection
    if (e.key === 'Escape') {
        const selectedRows = document.querySelectorAll('.table-row.selected');
        selectedRows.forEach(row => row.classList.remove('selected'));
    }
});