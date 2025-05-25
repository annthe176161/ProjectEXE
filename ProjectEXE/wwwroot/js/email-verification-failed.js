// ===== EMAIL VERIFICATION FAILED PAGE JAVASCRIPT =====

(function () {
    'use strict';

    // Cache DOM elements
    let elements = {};

    // Initialize page
    function initializePage() {
        cacheElements();
        bindEvents();
        setupInitialState();
    }

    // Cache all DOM elements
    function cacheElements() {
        elements = {
            resendButton: document.getElementById('resendButton'),
            resendText: document.getElementById('resendText'),
            btnLoader: document.getElementById('btnLoader'),
            card: document.querySelector('.verification-failed-card'),
            floatingShapes: document.querySelectorAll('.floating-shape')
        };
    }

    // Bind all event listeners
    function bindEvents() {
        if (elements.resendButton) {
            elements.resendButton.addEventListener('click', handleResendClick);
        }
    }

    // Setup initial state
    function setupInitialState() {
        // Entrance animations
        if (elements.card) {
            setTimeout(function () {
                elements.card.classList.add('entrance-complete');
            }, 100);
        }

        // Floating shapes animation
        if (elements.floatingShapes) {
            elements.floatingShapes.forEach(function (shape, index) {
                setTimeout(function () {
                    shape.classList.add('animate');
                }, (index + 1) * 800);
            });
        }

        // Auto-hide success message after showing
        setTimeout(function () {
            showNotification('Trang đã tải thành công', 'info');
        }, 1000);
    }

    // Handle resend button click
    function handleResendClick(e) {
        e.preventDefault();

        // Add loading state
        setLoadingState(true);

        // Simulate sending email
        setTimeout(function () {
            // Reset loading state
            setLoadingState(false);

            // Show success notification
            showNotification('Email xác nhận đã được gửi lại!', 'success');

            // Redirect after delay
            setTimeout(function () {
                window.location.href = elements.resendButton.href;
            }, 2000);

        }, 2000);
    }

    // Set loading state for resend button
    function setLoadingState(isLoading) {
        if (!elements.resendButton) return;

        if (isLoading) {
            elements.resendButton.style.pointerEvents = 'none';
            elements.resendButton.style.opacity = '0.8';
            if (elements.resendText) {
                elements.resendText.textContent = 'Đang gửi...';
            }
            if (elements.btnLoader) {
                elements.btnLoader.style.display = 'block';
            }
        } else {
            elements.resendButton.style.pointerEvents = 'auto';
            elements.resendButton.style.opacity = '1';
            if (elements.resendText) {
                elements.resendText.textContent = 'Gửi lại email xác nhận';
            }
            if (elements.btnLoader) {
                elements.btnLoader.style.display = 'none';
            }
        }
    }

    // Show notification
    function showNotification(message, type) {
        // Remove existing notifications
        const existingNotifications = document.querySelectorAll('.notification');
        existingNotifications.forEach(function (notification) {
            notification.remove();
        });

        // Create notification element
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.innerHTML = `
            <div class="notification-content">
                <i class="fas ${getNotificationIcon(type)}"></i>
                <span>${message}</span>
            </div>
            <button class="notification-close">
                <i class="fas fa-times"></i>
            </button>
        `;

        // Add to page
        document.body.appendChild(notification);

        // Add click handler for close button
        const closeButton = notification.querySelector('.notification-close');
        if (closeButton) {
            closeButton.addEventListener('click', function () {
                hideNotification(notification);
            });
        }

        // Show notification
        setTimeout(function () {
            notification.classList.add('show');
        }, 100);

        // Auto hide after 5 seconds
        setTimeout(function () {
            hideNotification(notification);
        }, 5000);
    }

    // Hide notification
    function hideNotification(notification) {
        notification.classList.remove('show');
        setTimeout(function () {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 300);
    }

    // Get notification icon based on type
    function getNotificationIcon(type) {
        switch (type) {
            case 'success': return 'fa-check-circle';
            case 'error': return 'fa-exclamation-circle';
            case 'warning': return 'fa-exclamation-triangle';
            case 'info': default: return 'fa-info-circle';
        }
    }

    // Utility function for smooth scrolling
    function smoothScrollTo(element) {
        if (element) {
            element.scrollIntoView({
                behavior: 'smooth',
                block: 'center'
            });
        }
    }

    // Public API
    window.EmailVerificationFailedPage = {
        showNotification: showNotification,
        setLoadingState: setLoadingState
    };

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializePage);
    } else {
        initializePage();
    }

})();

// ===== NOTIFICATION STYLES (Dynamic CSS) =====
const notificationCSS = `
.notification {
    position: fixed;
    top: 20px;
    right: 20px;
    z-index: 9999;
    max-width: 400px;
    background: var(--white);
    border-radius: 12px;
    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.15);
    border: 1px solid #e9ecef;
    padding: 16px;
    transform: translateX(100%);
    transition: all 0.3s ease;
    opacity: 0;
}

.notification.show {
    transform: translateX(0);
    opacity: 1;
}

.notification-content {
    display: flex;
    align-items: center;
    gap: 12px;
    font-size: 14px;
    font-weight: 600;
    color: var(--text-dark);
}

.notification-close {
    position: absolute;
    top: 8px;
    right: 8px;
    background: none;
    border: none;
    color: var(--text-muted);
    font-size: 14px;
    cursor: pointer;
    padding: 4px;
    border-radius: 4px;
    transition: all 0.3s ease;
}

.notification-close:hover {
    color: var(--text-dark);
    background: rgba(0, 0, 0, 0.05);
}

.notification-success {
    border-left: 4px solid var(--success);
}

.notification-success .notification-content i {
    color: var(--success);
}

.notification-error {
    border-left: 4px solid var(--error);
}

.notification-error .notification-content i {
    color: var(--error);
}

.notification-warning {
    border-left: 4px solid var(--warning);
}

.notification-warning .notification-content i {
    color: var(--warning);
}

.notification-info {
    border-left: 4px solid var(--info);
}

.notification-info .notification-content i {
    color: var(--info);
}

@media (max-width: 576px) {
    .notification {
        top: 10px;
        right: 10px;
        left: 10px;
        max-width: none;
    }
}
`;

// Inject notification CSS
const styleSheet = document.createElement('style');
styleSheet.textContent = notificationCSS;
document.head.appendChild(styleSheet);