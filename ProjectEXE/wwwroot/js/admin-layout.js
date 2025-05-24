/* filepath: wwwroot/js/admin-layout.js */

document.addEventListener('DOMContentLoaded', function () {
    initializeAdminLayout();
});

function initializeAdminLayout() {
    setupSidebarToggle();
    setupDropdowns();
    setupActiveNavigation();
    setupNotifications();
    setupSearch();
    setupKeyboardShortcuts();
    setupLoadingStates();
}

function setupSidebarToggle() {
    const sidebar = document.getElementById('adminSidebar');
    const sidebarToggle = document.getElementById('sidebarToggle');
    const mobileToggle = document.getElementById('mobileToggle');
    const sidebarOverlay = document.getElementById('sidebarOverlay');

    // Desktop sidebar collapse
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function () {
            sidebar.classList.toggle('collapsed');
            localStorage.setItem('sidebarCollapsed', sidebar.classList.contains('collapsed'));
        });
    }

    // Mobile sidebar toggle
    if (mobileToggle) {
        mobileToggle.addEventListener('click', function () {
            sidebar.classList.add('active');
            sidebarOverlay.classList.add('active');
        });
    }

    // Close sidebar on overlay click
    if (sidebarOverlay) {
        sidebarOverlay.addEventListener('click', function () {
            sidebar.classList.remove('active');
            sidebarOverlay.classList.remove('active');
        });
    }

    // Close sidebar with sidebar toggle on mobile
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function () {
            if (window.innerWidth <= 1024) {
                sidebar.classList.remove('active');
                sidebarOverlay.classList.remove('active');
            }
        });
    }

    // Load saved sidebar state
    const savedState = localStorage.getItem('sidebarCollapsed');
    if (savedState === 'true' && window.innerWidth > 1024) {
        sidebar.classList.add('collapsed');
    }

    // Handle window resize
    window.addEventListener('resize', function () {
        if (window.innerWidth > 1024) {
            sidebar.classList.remove('active');
            sidebarOverlay.classList.remove('active');
        }
    });
}

function setupDropdowns() {
    // Search dropdown
    const searchToggle = document.getElementById('searchToggle');
    const searchDropdown = document.getElementById('searchDropdown');

    if (searchToggle && searchDropdown) {
        searchToggle.addEventListener('click', function (e) {
            e.stopPropagation();
            closeAllDropdowns();
            searchDropdown.classList.toggle('active');

            if (searchDropdown.classList.contains('active')) {
                const searchInput = searchDropdown.querySelector('.search-input');
                setTimeout(() => searchInput.focus(), 100);
            }
        });
    }

    // Notification dropdown
    const notificationToggle = document.getElementById('notificationToggle');
    const notificationDropdown = document.getElementById('notificationDropdown');

    if (notificationToggle && notificationDropdown) {
        notificationToggle.addEventListener('click', function (e) {
            e.stopPropagation();
            closeAllDropdowns();
            notificationDropdown.classList.toggle('active');

            if (notificationDropdown.classList.contains('active')) {
                markNotificationsAsRead();
            }
        });
    }

    // Profile dropdown
    const profileToggle = document.getElementById('profileToggle');
    const profileDropdown = document.getElementById('profileDropdown');

    if (profileToggle && profileDropdown) {
        profileToggle.addEventListener('click', function (e) {
            e.stopPropagation();
            closeAllDropdowns();
            profileDropdown.classList.toggle('active');
        });
    }

    // Close dropdowns when clicking outside
    document.addEventListener('click', function () {
        closeAllDropdowns();
    });

    // Prevent dropdown close when clicking inside
    document.querySelectorAll('.search-dropdown, .notification-dropdown, .profile-dropdown').forEach(dropdown => {
        dropdown.addEventListener('click', function (e) {
            e.stopPropagation();
        });
    });
}

function closeAllDropdowns() {
    document.querySelectorAll('.search-dropdown, .notification-dropdown, .profile-dropdown').forEach(dropdown => {
        dropdown.classList.remove('active');
    });
}

function setupActiveNavigation() {
    const currentPath = window.location.pathname;
    const navLinks = document.querySelectorAll('.nav-link');

    navLinks.forEach(link => {
        link.classList.remove('active');

        if (link.getAttribute('href') === currentPath) {
            link.classList.add('active');
        }
    });
}

function setupNotifications() {
    const markAllReadBtn = document.querySelector('.mark-all-read');
    const notificationItems = document.querySelectorAll('.notification-item');

    if (markAllReadBtn) {
        markAllReadBtn.addEventListener('click', function () {
            notificationItems.forEach(item => {
                item.classList.remove('unread');
            });

            updateNotificationBadge(0);
            showToast('Đã đánh dấu tất cả thông báo là đã đọc', 'success');
        });
    }

    // Mark individual notification as read
    notificationItems.forEach(item => {
        item.addEventListener('click', function () {
            this.classList.remove('unread');
            updateNotificationCount();
        });
    });
}

function markNotificationsAsRead() {
    // Simulate API call to mark notifications as read
    setTimeout(() => {
        const unreadItems = document.querySelectorAll('.notification-item.unread');
        if (unreadItems.length > 0) {
            updateNotificationBadge(Math.max(0, unreadItems.length - 2));
        }
    }, 1000);
}

function updateNotificationBadge(count) {
    const badge = document.querySelector('.notification-badge');
    if (badge) {
        if (count > 0) {
            badge.textContent = count;
            badge.style.display = 'flex';
        } else {
            badge.style.display = 'none';
        }
    }
}

function updateNotificationCount() {
    const unreadCount = document.querySelectorAll('.notification-item.unread').length;
    updateNotificationBadge(unreadCount);
}

function setupSearch() {
    const searchInput = document.querySelector('.search-input');
    const searchResults = document.querySelector('.search-results');

    if (searchInput && searchResults) {
        let searchTimeout;

        searchInput.addEventListener('input', function () {
            clearTimeout(searchTimeout);
            const query = this.value.trim();

            if (query.length > 0) {
                searchTimeout = setTimeout(() => {
                    performSearch(query);
                }, 300);
            } else {
                showEmptySearch();
            }
        });

        searchInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                const query = this.value.trim();
                if (query.length > 0) {
                    performSearch(query);
                }
            }
        });
    }
}

function performSearch(query) {
    const searchResults = document.querySelector('.search-results');

    // Show loading state
    searchResults.innerHTML = `
        <div class="search-loading">
            <div class="spinner" style="width: 20px; height: 20px; margin: 1rem auto;"></div>
            <div style="text-align: center; color: var(--gray-500); font-size: 0.875rem;">Đang tìm kiếm...</div>
        </div>
    `;

    // Simulate API call
    setTimeout(() => {
        // Mock search results
        const mockResults = [
            { title: 'Gói Premium', type: 'Dịch vụ', url: '#' },
            { title: 'Shop ABC', type: 'Shop', url: '#' },
            { title: 'Người dùng XYZ', type: 'Người dùng', url: '#' }
        ].filter(item => item.title.toLowerCase().includes(query.toLowerCase()));

        if (mockResults.length > 0) {
            searchResults.innerHTML = mockResults.map(result => `
                <div class="search-result-item" style="padding: 0.75rem; border-radius: var(--radius-sm); margin-bottom: 0.5rem; cursor: pointer; transition: var(--transition-fast);" 
                     onmouseover="this.style.background='var(--gray-50)'" 
                     onmouseout="this.style.background='transparent'">
                    <div style="font-weight: 500; color: var(--gray-900); margin-bottom: 0.25rem;">${result.title}</div>
                    <div style="font-size: 0.75rem; color: var(--gray-500);">${result.type}</div>
                </div>
            `).join('');
        } else {
            searchResults.innerHTML = `
                <div style="text-align: center; color: var(--gray-500); font-size: 0.875rem; padding: 2rem;">
                    <i class="fas fa-search" style="font-size: 2rem; margin-bottom: 0.5rem; display: block; color: var(--gray-300);"></i>
                    Không tìm thấy kết quả nào
                </div>
            `;
        }
    }, 500);
}

function showEmptySearch() {
    const searchResults = document.querySelector('.search-results');
    if (searchResults) {
        searchResults.innerHTML = `
            <div class="search-empty">
                <i class="fas fa-search"></i>
                <span>Nhập từ khóa để tìm kiếm</span>
            </div>
        `;
    }
}

function setupKeyboardShortcuts() {
    document.addEventListener('keydown', function (e) {
        // Ctrl/Cmd + K for search
        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            const searchToggle = document.getElementById('searchToggle');
            if (searchToggle) {
                searchToggle.click();
            }
        }

        // Escape to close dropdowns
        if (e.key === 'Escape') {
            closeAllDropdowns();
        }

        // Alt + S to toggle sidebar
        if (e.altKey && e.key === 's') {
            e.preventDefault();
            const sidebarToggle = document.getElementById('sidebarToggle');
            if (sidebarToggle) {
                sidebarToggle.click();
            }
        }
    });
}

function setupLoadingStates() {
    // Add loading states to navigation links
    const navLinks = document.querySelectorAll('.nav-link[href]:not([href="#"])');

    navLinks.forEach(link => {
        link.addEventListener('click', function () {
            if (!this.classList.contains('active')) {
                showLoadingOverlay();

                // Hide loading after navigation (fallback)
                setTimeout(() => {
                    hideLoadingOverlay();
                }, 3000);
            }
        });
    });

    // Hide loading when page loads
    window.addEventListener('load', function () {
        hideLoadingOverlay();
    });
}

function showLoadingOverlay() {
    const overlay = document.getElementById('loadingOverlay');
    if (overlay) {
        overlay.classList.add('active');
    }
}

function hideLoadingOverlay() {
    const overlay = document.getElementById('loadingOverlay');
    if (overlay) {
        overlay.classList.remove('active');
    }
}

function showToast(message, type = 'info') {
    const toast = document.createElement('div');
    toast.className = `toast-notification toast-${type}`;
    toast.innerHTML = `
        <div style="display: flex; align-items: center; gap: 0.75rem;">
            <i class="fas fa-${getToastIcon(type)}" style="color: var(--${type}-color);"></i>
            <span style="flex: 1;">${message}</span>
            <button onclick="this.parentElement.parentElement.remove()" style="background: none; border: none; color: var(--gray-400); cursor: pointer; padding: 0.25rem;">
                <i class="fas fa-times"></i>
            </button>
        </div>
    `;

    // Styles
    Object.assign(toast.style, {
        position: 'fixed',
        top: '1rem',
        right: '1rem',
        background: 'var(--white)',
        borderRadius: 'var(--radius)',
        boxShadow: 'var(--shadow-lg)',
        border: '1px solid var(--gray-200)',
        padding: '1rem',
        minWidth: '300px',
        zIndex: '9999',
        animation: 'slideInRight 0.3s ease-out',
        fontSize: '0.875rem'
    });

    document.body.appendChild(toast);

    // Auto remove
    setTimeout(() => {
        toast.style.animation = 'slideOutRight 0.3s ease-in';
        setTimeout(() => toast.remove(), 300);
    }, 4000);
}

function getToastIcon(type) {
    const icons = {
        success: 'check-circle',
        error: 'exclamation-triangle',
        warning: 'exclamation-circle',
        info: 'info-circle'
    };
    return icons[type] || 'info-circle';
}

// Add toast animations
const toastStyles = document.createElement('style');
toastStyles.textContent = `
    @keyframes slideInRight {
        from { transform: translateX(100%); opacity: 0; }
        to { transform: translateX(0); opacity: 1; }
    }
    @keyframes slideOutRight {
        from { transform: translateX(0); opacity: 1; }
        to { transform: translateX(100%); opacity: 0; }
    }
`;
document.head.appendChild(toastStyles);

// Initialize notification count on load
document.addEventListener('DOMContentLoaded', function () {
    updateNotificationCount();
});