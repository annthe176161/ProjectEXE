// ===== PROFILE PAGE JAVASCRIPT =====

(function () {
    'use strict';

    // Initialize the page
    function initProfile() {
        initTabs();
        initAlerts();
        initAnimations();
        initInteractions();
    }

    // Initialize tab functionality
    function initTabs() {
        const tabButtons = document.querySelectorAll('.tab-button');
        const tabContents = document.querySelectorAll('.tab-content');

        tabButtons.forEach(button => {
            button.addEventListener('click', function () {
                const targetTab = this.getAttribute('data-tab');

                // Remove active class from all buttons and contents
                tabButtons.forEach(btn => btn.classList.remove('active'));
                tabContents.forEach(content => content.classList.remove('active'));

                // Add active class to clicked button and corresponding content
                this.classList.add('active');
                const targetContent = document.getElementById(targetTab);
                if (targetContent) {
                    targetContent.classList.add('active');
                }
            });
        });
    }

    // Initialize alert handling
    function initAlerts() {
        const alerts = document.querySelectorAll('.custom-alert');
        alerts.forEach(alert => {
            // Auto dismiss after 5 seconds
            setTimeout(() => {
                dismissAlert(alert);
            }, 5000);

            // Manual dismiss
            const closeBtn = alert.querySelector('.btn-close-custom');
            if (closeBtn) {
                closeBtn.addEventListener('click', () => {
                    dismissAlert(alert);
                });
            }
        });
    }

    // Dismiss alert with animation
    function dismissAlert(alert) {
        alert.style.animation = 'fadeOut 0.5s ease forwards';
        setTimeout(() => {
            alert.remove();
        }, 500);
    }

    // Initialize animations
    function initAnimations() {
        // Intersection Observer for scroll animations
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('fade-in');
                }
            });
        }, observerOptions);

        // Observe elements for animation
        document.querySelectorAll('.stat-item').forEach(item => {
            observer.observe(item);
        });

        // Stagger animation for info items
        const infoItems = document.querySelectorAll('.info-item');
        infoItems.forEach((item, index) => {
            item.style.animationDelay = `${index * 0.1}s`;
            item.classList.add('fade-in');
        });
    }

    // Initialize interactions
    function initInteractions() {
        // Avatar hover effect
        const avatar = document.querySelector('.avatar-placeholder');
        if (avatar) {
            avatar.addEventListener('mouseenter', function () {
                this.style.transform = 'scale(1.05) rotate(5deg)';
            });

            avatar.addEventListener('mouseleave', function () {
                this.style.transform = 'scale(1) rotate(0deg)';
            });
        }

        // Button loading states
        const actionButtons = document.querySelectorAll('.action-buttons .btn');
        actionButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                // Don't prevent navigation, just add loading state
                this.classList.add('loading');

                const originalContent = this.innerHTML;
                this.innerHTML = `
                    <i class="fas fa-spinner fa-spin"></i>
                    <span>Đang tải...</span>
                `;

                // Reset after navigation or timeout
                setTimeout(() => {
                    this.classList.remove('loading');
                    this.innerHTML = originalContent;
                }, 2000);
            });
        });

        // Info item hover enhancement
        const infoItems = document.querySelectorAll('.info-item');
        infoItems.forEach(item => {
            item.addEventListener('mouseenter', function () {
                const icon = this.querySelector('.info-icon');
                if (icon) {
                    icon.style.transform = 'scale(1.1) rotate(10deg)';
                }
            });

            item.addEventListener('mouseleave', function () {
                const icon = this.querySelector('.info-icon');
                if (icon) {
                    icon.style.transform = 'scale(1) rotate(0deg)';
                }
            });
        });

        // Stat items counter animation
        animateCounters();
    }

    // Animate counter numbers
    function animateCounters() {
        const counters = document.querySelectorAll('.stat-number');

        counters.forEach(counter => {
            const target = parseInt(counter.textContent);
            counter.textContent = '0';

            let current = 0;
            const increment = Math.max(1, Math.ceil(target / 50));

            const updateCounter = () => {
                if (current < target) {
                    current += increment;
                    if (current > target) current = target;
                    counter.textContent = current.toLocaleString();
                    requestAnimationFrame(updateCounter);
                }
            };

            // Start animation when element comes into view
            const observer = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        setTimeout(updateCounter, 500);
                        observer.unobserve(entry.target);
                    }
                });
            });

            observer.observe(counter);
        });
    }

    // Add additional CSS for loading and animations
    const style = document.createElement('style');
    style.textContent = `
        .btn.loading {
            pointer-events: none;
            opacity: 0.8;
        }
        
        @keyframes fadeOut {
            from {
                opacity: 1;
                transform: translateY(0);
            }
            to {
                opacity: 0;
                transform: translateY(-20px);
            }
        }
        
        .avatar-placeholder {
            transition: transform 0.3s ease;
        }
        
        .info-icon {
            transition: transform 0.3s ease;
        }
        
        .stat-item:nth-child(1) { animation-delay: 0.1s; }
        .stat-item:nth-child(2) { animation-delay: 0.2s; }
        .stat-item:nth-child(3) { animation-delay: 0.3s; }
        
        .info-item:nth-child(1) { animation-delay: 0.1s; }
        .info-item:nth-child(2) { animation-delay: 0.2s; }
        .info-item:nth-child(3) { animation-delay: 0.3s; }
        .info-item:nth-child(4) { animation-delay: 0.4s; }
    `;
    document.head.appendChild(style);

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initProfile);
    } else {
        initProfile();
    }

})();