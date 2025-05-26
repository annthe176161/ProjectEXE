// ===== SERVICE PACKAGES JAVASCRIPT =====

(function () {
    'use strict';

    // Initialize the page
    function initServicePackages() {
        initAnimations();
        initInteractions();
        initAlerts();
    }

    // Initialize animations
    function initAnimations() {
        // Stagger animation for package cards
        const cards = document.querySelectorAll('.package-card');
        cards.forEach((card, index) => {
            card.style.animationDelay = `${index * 0.2}s`;
        });

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
        document.querySelectorAll('.benefit-item').forEach(item => {
            observer.observe(item);
        });
    }

    // Initialize interactions
    function initInteractions() {
        // Package card hover effects
        const packageCards = document.querySelectorAll('.package-card');
        packageCards.forEach(card => {
            card.addEventListener('mouseenter', function () {
                this.style.transform = 'translateY(-8px) scale(1.02)';
            });

            card.addEventListener('mouseleave', function () {
                if (this.classList.contains('recommended-package')) {
                    this.style.transform = 'scale(1.05)';
                } else {
                    this.style.transform = 'translateY(0) scale(1)';
                }
            });
        });

        // Button loading states
        const selectButtons = document.querySelectorAll('.btn-select-package');
        selectButtons.forEach(button => {
            button.addEventListener('click', function () {
                this.classList.add('loading');
                this.disabled = true;

                const originalContent = this.innerHTML;
                this.innerHTML = `
                    <i class="fas fa-spinner fa-spin"></i>
                    <span>Đang xử lý...</span>
                `;

                // Reset after 5 seconds as fallback
                setTimeout(() => {
                    this.classList.remove('loading');
                    this.disabled = false;
                    this.innerHTML = originalContent;
                }, 5000);
            });
        });

        // Smooth scroll for benefits section
        const benefitsSection = document.querySelector('.benefits-section');
        if (benefitsSection) {
            const triggerScroll = () => {
                benefitsSection.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            };

            // Auto scroll to benefits after 3 seconds
            setTimeout(triggerScroll, 3000);
        }
    }

    // Initialize alert handling
    function initAlerts() {
        const alerts = document.querySelectorAll('.custom-alert');
        alerts.forEach(alert => {
            // Auto dismiss after 5 seconds
            setTimeout(() => {
                alert.style.animation = 'fadeOut 0.5s ease forwards';
                setTimeout(() => {
                    alert.remove();
                }, 500);
            }, 5000);

            // Manual dismiss
            const closeBtn = alert.querySelector('.btn-close-custom');
            if (closeBtn) {
                closeBtn.addEventListener('click', () => {
                    alert.style.animation = 'fadeOut 0.5s ease forwards';
                    setTimeout(() => {
                        alert.remove();
                    }, 500);
                });
            }
        });
    }

    // Add additional CSS for loading and fade animations
    const style = document.createElement('style');
    style.textContent = `
        .btn-select-package.loading {
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
        
        .package-card {
            transform-origin: center;
        }
        
        .recommended-package .package-icon {
            animation: pulse 2s infinite;
        }
        
        @keyframes pulse {
            0% {
                box-shadow: 0 8px 24px rgba(40, 167, 69, 0.3);
            }
            50% {
                box-shadow: 0 12px 32px rgba(40, 167, 69, 0.5);
            }
            100% {
                box-shadow: 0 8px 24px rgba(40, 167, 69, 0.3);
            }
        }
    `;
    document.head.appendChild(style);

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initServicePackages);
    } else {
        initServicePackages();
    }

})();