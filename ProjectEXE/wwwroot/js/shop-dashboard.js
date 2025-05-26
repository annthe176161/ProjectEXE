// ===== SHOP DASHBOARD JAVASCRIPT =====

(function () {
    'use strict';

    // Initialize dashboard
    function initDashboard() {
        initAnimations();
        initInteractions();
        handleAlerts();
    }

    // Initialize animations
    function initAnimations() {
        // Animate stat cards on scroll
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

        // Observe stat cards and action cards
        document.querySelectorAll('.stat-card, .quick-action-card').forEach(card => {
            observer.observe(card);
        });
    }

    // Initialize interactions
    function initInteractions() {
        // Avatar upload interaction
        const avatarElements = document.querySelectorAll('.avatar-wrapper, .avatar-placeholder');
        avatarElements.forEach(avatar => {
            avatar.addEventListener('click', () => {
                console.log('Avatar upload clicked');
                // Implement avatar upload functionality
            });
        });

        // Button loading states
        const buttons = document.querySelectorAll('.btn');
        buttons.forEach(button => {
            button.addEventListener('click', function (e) {
                if (this.href && !this.href.includes('#')) {
                    this.classList.add('loading');
                }
            });
        });
    }

    // Handle alert dismissal
    function handleAlerts() {
        const alerts = document.querySelectorAll('.alert');
        alerts.forEach(alert => {
            const closeBtn = alert.querySelector('.alert-close');
            if (closeBtn) {
                closeBtn.addEventListener('click', () => {
                    alert.style.animation = 'slideUpFade 0.3s ease forwards';
                    setTimeout(() => {
                        alert.remove();
                    }, 300);
                });
            }

            // Auto dismiss after 5 seconds
            setTimeout(() => {
                if (alert.parentNode) {
                    alert.style.animation = 'slideUpFade 0.3s ease forwards';
                    setTimeout(() => {
                        alert.remove();
                    }, 300);
                }
            }, 5000);
        });
    }

    // Add CSS animations
    const style = document.createElement('style');
    style.textContent = `
        @keyframes slideUpFade {
            to {
                opacity: 0;
                transform: translateX(-50%) translateY(-20px);
            }
        }
        
        .animate-in {
            animation: fadeInUp 0.6s ease forwards;
        }
        
        @keyframes fadeInUp {
            from {
                opacity: 0;
                transform: translateY(30px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }
    `;
    document.head.appendChild(style);

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initDashboard);
    } else {
        initDashboard();
    }

})();