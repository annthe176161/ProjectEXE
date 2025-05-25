// ===== FORGOT PASSWORD PAGE JAVASCRIPT =====

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
            emailInput: document.getElementById('emailInput'),
            emailStatus: document.getElementById('emailStatus'),
            sendButton: document.getElementById('sendButton'),
            sendText: document.getElementById('sendText'),
            btnLoader: document.getElementById('btnLoader'),
            forgotForm: document.getElementById('forgotPasswordForm'),
            card: document.querySelector('.forgot-password-card'),
            steps: document.querySelectorAll('.step'),
            floatingShapes: document.querySelectorAll('.floating-shape')
        };
    }

    // Bind all event listeners
    function bindEvents() {
        if (elements.emailInput) {
            elements.emailInput.addEventListener('input', handleEmailInput);
            elements.emailInput.addEventListener('blur', handleEmailBlur);
            elements.emailInput.addEventListener('focus', handleEmailFocus);
        }

        if (elements.forgotForm) {
            elements.forgotForm.addEventListener('submit', handleFormSubmit);
        }
    }

    // Setup initial state
    function setupInitialState() {
        // Auto focus email input
        if (elements.emailInput) {
            elements.emailInput.focus();
        }

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
                }, (index + 1) * 500);
            });
        }
    }

    // Email validation
    function validateEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }

    // Handle email input events
    function handleEmailInput() {
        const email = this.value.trim();

        if (email && validateEmail(email)) {
            showEmailStatus(true);
            updateEmailFieldState('valid');
        } else if (email) {
            showEmailStatus(false);
            updateEmailFieldState('invalid');
        } else {
            showEmailStatus(false);
            updateEmailFieldState('neutral');
        }
    }

    // Handle email blur events
    function handleEmailBlur() {
        if (this.value.trim() !== '') {
            this.classList.add('has-value');
        } else {
            this.classList.remove('has-value');
        }
    }

    // Handle email focus events
    function handleEmailFocus() {
        this.classList.remove('invalid');
        hideValidationError();
    }

    // Show/hide email status
    function showEmailStatus(isValid) {
        if (!elements.emailStatus) return;

        if (isValid) {
            elements.emailStatus.style.display = 'flex';
        } else {
            elements.emailStatus.style.display = 'none';
        }
    }

    // Update email field visual state
    function updateEmailFieldState(state) {
        if (!elements.emailInput) return;

        elements.emailInput.classList.remove('valid', 'invalid');

        if (state === 'valid') {
            elements.emailInput.classList.add('valid');
        } else if (state === 'invalid') {
            elements.emailInput.classList.add('invalid');
        }
    }

    // Hide validation error messages
    function hideValidationError() {
        if (!elements.emailInput) return;

        const errorSpan = elements.emailInput.parentNode.querySelector('.field-validation-error');
        if (errorSpan) {
            errorSpan.style.display = 'none';
        }
    }

    // Handle form submission
    function handleFormSubmit(e) {
        const email = elements.emailInput ? elements.emailInput.value.trim() : '';

        if (!email || !validateEmail(email)) {
            e.preventDefault();
            updateEmailFieldState('invalid');
            if (elements.emailInput) {
                elements.emailInput.focus();
            }
            return false;
        }

        // Add loading state
        setLoadingState(true);

        // Simulate progress through steps
        setTimeout(function () {
            updateStepProgress(2);
        }, 1000);

        setTimeout(function () {
            updateSendButtonText('Đang xử lý...');
        }, 2000);

        return true;
    }

    // Set loading state
    function setLoadingState(isLoading) {
        if (!elements.sendButton) return;

        if (isLoading) {
            elements.sendButton.classList.add('loading');
            elements.sendButton.disabled = true;
            updateSendButtonText('Đang gửi...');
            showLoader(true);
        } else {
            elements.sendButton.classList.remove('loading');
            elements.sendButton.disabled = false;
            updateSendButtonText('Gửi liên kết khôi phục');
            showLoader(false);
        }
    }

    // Update send button text
    function updateSendButtonText(text) {
        if (elements.sendText) {
            elements.sendText.textContent = text;
        }
    }

    // Show/hide loader
    function showLoader(show) {
        if (elements.btnLoader) {
            elements.btnLoader.style.display = show ? 'block' : 'none';
        }
    }

    // Update step progress
    function updateStepProgress(step) {
        if (!elements.steps) return;

        elements.steps.forEach(function (stepEl, index) {
            stepEl.classList.remove('active', 'completed');

            if (index < step - 1) {
                stepEl.classList.add('completed');
            } else if (index === step - 1) {
                stepEl.classList.add('active');
            }
        });
    }

    // Utility functions for animations
    function animateElement(element, animationClass, duration) {
        if (!element) return;

        element.classList.add(animationClass);

        if (duration) {
            setTimeout(function () {
                element.classList.remove(animationClass);
            }, duration);
        }
    }

    // Public API for external access if needed
    window.ForgotPasswordPage = {
        validateEmail: validateEmail,
        updateStepProgress: updateStepProgress,
        setLoadingState: setLoadingState
    };

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializePage);
    } else {
        initializePage();
    }

})();

// ===== ADDITIONAL UTILITY FUNCTIONS =====

// Email format validation with more comprehensive rules
function isValidEmailFormat(email) {
    const emailRegex = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/;
    return emailRegex.test(email) && email.length <= 254;
}

// Common email domains for suggestions
const commonEmailDomains = [
    'gmail.com', 'yahoo.com', 'hotmail.com', 'outlook.com',
    'icloud.com', 'live.com', 'msn.com', 'aol.com'
];

// Email domain suggestion
function suggestEmailDomain(email) {
    if (!email || !email.includes('@')) return null;

    const [localPart, domain] = email.split('@');
    if (!domain) return null;

    const suggestion = commonEmailDomains.find(function (commonDomain) {
        return getLevenshteinDistance(domain.toLowerCase(), commonDomain) <= 2;
    });

    return suggestion ? localPart + '@' + suggestion : null;
}

// Calculate Levenshtein distance for email suggestions
function getLevenshteinDistance(str1, str2) {
    const matrix = [];

    for (let i = 0; i <= str2.length; i++) {
        matrix[i] = [i];
    }

    for (let j = 0; j <= str1.length; j++) {
        matrix[0][j] = j;
    }

    for (let i = 1; i <= str2.length; i++) {
        for (let j = 1; j <= str1.length; j++) {
            if (str2.charAt(i - 1) === str1.charAt(j - 1)) {
                matrix[i][j] = matrix[i - 1][j - 1];
            } else {
                matrix[i][j] = Math.min(
                    matrix[i - 1][j - 1] + 1,
                    matrix[i][j - 1] + 1,
                    matrix[i - 1][j] + 1
                );
            }
        }
    }

    return matrix[str2.length][str1.length];
}

// Debounce function for performance optimization
function debounce(func, wait, immediate) {
    let timeout;
    return function executedFunction() {
        const context = this;
        const args = arguments;

        const later = function () {
            timeout = null;
            if (!immediate) func.apply(context, args);
        };

        const callNow = immediate && !timeout;
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);

        if (callNow) func.apply(context, args);
    };
}

// Form validation enhancement
function enhanceFormValidation() {
    const form = document.getElementById('forgotPasswordForm');
    if (!form) return;

    // Add real-time validation
    const emailInput = document.getElementById('emailInput');
    if (emailInput) {
        const debouncedValidation = debounce(function (e) {
            const email = e.target.value.trim();
            if (email && !isValidEmailFormat(email)) {
                const suggestion = suggestEmailDomain(email);
                if (suggestion) {
                    showEmailSuggestion(suggestion);
                }
            }
        }, 500);

        emailInput.addEventListener('input', debouncedValidation);
    }
}

// Show email suggestion
function showEmailSuggestion(suggestion) {
    // Remove existing suggestion
    const existingSuggestion = document.querySelector('.email-suggestion');
    if (existingSuggestion) {
        existingSuggestion.remove();
    }

    // Create suggestion element
    const suggestionEl = document.createElement('div');
    suggestionEl.className = 'email-suggestion';
    suggestionEl.innerHTML = `
        <i class="fas fa-lightbulb"></i>
        <span>Có phải bạn muốn nhập: <strong>${suggestion}</strong>?</span>
        <button type="button" class="btn-suggestion">Sử dụng</button>
    `;

    // Insert after email input
    const emailInput = document.getElementById('emailInput');
    if (emailInput && emailInput.parentNode) {
        emailInput.parentNode.insertBefore(suggestionEl, emailInput.nextSibling);

        // Add click handler for suggestion button
        const btnSuggestion = suggestionEl.querySelector('.btn-suggestion');
        if (btnSuggestion) {
            btnSuggestion.addEventListener('click', function () {
                emailInput.value = suggestion;
                emailInput.dispatchEvent(new Event('input'));
                suggestionEl.remove();
            });
        }
    }
}

// Initialize enhanced features
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', enhanceFormValidation);
} else {
    enhanceFormValidation();
}