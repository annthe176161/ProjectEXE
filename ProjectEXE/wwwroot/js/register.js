// ===== REGISTER PAGE JAVASCRIPT =====

(function () {
    'use strict';

    let currentStep = 1;
    const totalSteps = 3;

    // Initialize
    function init() {
        bindEvents();
        setupValidation();
        updateUI();
    }

    // Bind events
    function bindEvents() {
        // Navigation buttons
        const nextBtn = document.getElementById('nextBtn');
        const prevBtn = document.getElementById('prevBtn');
        const submitBtn = document.getElementById('submitBtn');

        if (nextBtn) {
            nextBtn.addEventListener('click', handleNext);
        }

        if (prevBtn) {
            prevBtn.addEventListener('click', handlePrevious);
        }

        // Password toggle
        const toggleButtons = document.querySelectorAll('.toggle-password');
        toggleButtons.forEach(btn => {
            btn.addEventListener('click', handlePasswordToggle);
        });

        // Password strength
        const passwordInput = document.querySelector('input[name="Password"]');
        if (passwordInput) {
            passwordInput.addEventListener('input', checkPasswordStrength);
        }

        // Role selection
        const roleInputs = document.querySelectorAll('input[name="RoleId"]');
        roleInputs.forEach(input => {
            input.addEventListener('change', handleRoleChange);
        });

        // Form validation
        const inputs = document.querySelectorAll('input, select, textarea');
        inputs.forEach(input => {
            input.addEventListener('blur', validateField);
            input.addEventListener('input', clearFieldError);
        });
    }

    // Handle next step
    function handleNext() {
        if (validateCurrentStep()) {
            if (currentStep < totalSteps) {
                currentStep++;
                updateUI();
                animateStepTransition();
            }
        }
    }

    // Handle previous step
    function handlePrevious() {
        if (currentStep > 1) {
            currentStep--;
            updateUI();
            animateStepTransition();
        }
    }

    // Update UI for current step
    function updateUI() {
        // Update steps
        document.querySelectorAll('.form-step').forEach((step, index) => {
            step.classList.toggle('active', index + 1 === currentStep);
        });

        // Update progress
        document.querySelectorAll('.progress-step').forEach((step, index) => {
            const stepNum = index + 1;
            step.classList.toggle('active', stepNum === currentStep);
            step.classList.toggle('completed', stepNum < currentStep);
        });

        // Update progress lines
        document.querySelectorAll('.progress-line').forEach((line, index) => {
            line.classList.toggle('completed', index + 1 < currentStep);
        });

        // Update buttons
        const nextBtn = document.getElementById('nextBtn');
        const prevBtn = document.getElementById('prevBtn');
        const submitBtn = document.getElementById('submitBtn');

        if (prevBtn) {
            prevBtn.style.display = currentStep > 1 ? 'inline-flex' : 'none';
        }

        if (nextBtn && submitBtn) {
            if (currentStep === totalSteps) {
                nextBtn.style.display = 'none';
                submitBtn.style.display = 'inline-flex';
            } else {
                nextBtn.style.display = 'inline-flex';
                submitBtn.style.display = 'none';
            }
        }

        // Scroll to top
        document.querySelector('.register-card').scrollIntoView({
            behavior: 'smooth',
            block: 'start'
        });
    }

    // Animate step transition
    function animateStepTransition() {
        const activeStep = document.querySelector('.form-step.active');
        if (activeStep) {
            activeStep.style.opacity = '0';
            activeStep.style.transform = 'translateY(20px)';

            setTimeout(() => {
                activeStep.style.opacity = '1';
                activeStep.style.transform = 'translateY(0)';
            }, 100);
        }
    }

    // Validate current step
    function validateCurrentStep() {
        const currentStepElement = document.querySelector(`[data-step="${currentStep}"].form-step`);
        if (!currentStepElement) return true;

        const inputs = currentStepElement.querySelectorAll('input[required], select[required], textarea[required]');
        let isValid = true;

        inputs.forEach(input => {
            if (!validateField({ target: input })) {
                isValid = false;
            }
        });

        // Additional validation for step 2 (passwords)
        if (currentStep === 2) {
            const password = document.querySelector('input[name="Password"]').value;
            const confirmPassword = document.querySelector('input[name="ConfirmPassword"]').value;

            if (password !== confirmPassword) {
                showFieldError(document.querySelector('input[name="ConfirmPassword"]'), 'Mật khẩu xác nhận không khớp');
                isValid = false;
            }

            if (!isPasswordStrong(password)) {
                showFieldError(document.querySelector('input[name="Password"]'), 'Mật khẩu chưa đủ mạnh');
                isValid = false;
            }
        }

        return isValid;
    }

    // Setup validation
    function setupValidation() {
        // Remove default validation
        const form = document.getElementById('registerForm');
        if (form) {
            form.setAttribute('novalidate', 'true');
        }
    }

    // Validate field
    function validateField(e) {
        const field = e.target;
        const value = field.value.trim();
        let isValid = true;

        // Clear previous errors
        clearFieldError(field);

        // Required validation
        if (field.hasAttribute('required') || isRequiredField(field.name)) {
            if (!value) {
                showFieldError(field, 'Trường này là bắt buộc');
                isValid = false;
            }
        }

        // Email validation
        if (field.type === 'email' && value) {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(value)) {
                showFieldError(field, 'Email không hợp lệ');
                isValid = false;
            }
        }

        // Phone validation
        if (field.name === 'PhoneNumber' && value) {
            const phoneRegex = /^[0-9]{10,11}$/;
            if (!phoneRegex.test(value.replace(/\s/g, ''))) {
                showFieldError(field, 'Số điện thoại không hợp lệ');
                isValid = false;
            }
        }

        // Update field visual state
        if (isValid && value) {
            field.classList.add('is-valid');
            field.classList.remove('is-invalid');
        } else if (!isValid) {
            field.classList.add('is-invalid');
            field.classList.remove('is-valid');
        }

        return isValid;
    }

    // Check if field is required
    function isRequiredField(name) {
        const required = ['FullName', 'Email', 'PhoneNumber', 'Address', 'Password', 'ConfirmPassword'];
        return required.includes(name);
    }

    // Show field error
    function showFieldError(field, message) {
        field.classList.add('is-invalid');

        let errorElement = field.parentNode.querySelector('.field-error');
        if (!errorElement) {
            errorElement = field.parentNode.querySelector('.text-danger');
        }

        if (errorElement) {
            errorElement.textContent = message;
            errorElement.style.display = 'block';
        }
    }

    // Clear field error
    function clearFieldError(field) {
        field.classList.remove('is-invalid');

        const errorElement = field.parentNode.querySelector('.field-error');
        if (errorElement) {
            errorElement.textContent = '';
            errorElement.style.display = 'none';
        }
    }

    // Handle password toggle
    function handlePasswordToggle(e) {
        e.preventDefault();

        const targetName = e.currentTarget.getAttribute('data-target');
        const targetInput = document.querySelector(`input[name="${targetName}"]`);
        const icon = e.currentTarget.querySelector('i');

        if (targetInput.type === 'password') {
            targetInput.type = 'text';
            icon.className = 'fas fa-eye-slash';
        } else {
            targetInput.type = 'password';
            icon.className = 'fas fa-eye';
        }
    }

    // Check password strength
    function checkPasswordStrength(e) {
        const password = e.target.value;
        const strengthFill = document.querySelector('.strength-fill');
        const strengthText = document.querySelector('.strength-text');
        const requirements = document.querySelectorAll('.requirement-item');

        if (!strengthFill || !strengthText) return;

        // Check requirements
        const checks = {
            length: password.length >= 8,
            uppercase: /[A-Z]/.test(password),
            lowercase: /[a-z]/.test(password),
            number: /[0-9]/.test(password)
        };

        // Update requirement indicators
        requirements.forEach(req => {
            const type = req.getAttribute('data-requirement');
            if (checks[type]) {
                req.classList.add('met');
            } else {
                req.classList.remove('met');
            }
        });

        // Calculate strength
        const metCount = Object.values(checks).filter(Boolean).length;
        let strength = 'weak';
        let strengthLabel = 'Yếu';

        if (metCount === 4) {
            strength = 'strong';
            strengthLabel = 'Mạnh';
        } else if (metCount >= 3) {
            strength = 'good';
            strengthLabel = 'Tốt';
        } else if (metCount >= 2) {
            strength = 'fair';
            strengthLabel = 'Trung bình';
        }

        // Update UI
        strengthFill.className = `strength-fill ${strength}`;
        strengthText.textContent = `Độ mạnh mật khẩu: ${strengthLabel}`;
    }

    // Check if password is strong
    function isPasswordStrong(password) {
        return password.length >= 8 &&
            /[A-Z]/.test(password) &&
            /[a-z]/.test(password) &&
            /[0-9]/.test(password);
    }

    // Handle role change
    function handleRoleChange(e) {
        const roleCards = document.querySelectorAll('.role-card');
        roleCards.forEach(card => {
            card.classList.remove('selected');
        });

        const selectedCard = e.target.nextElementSibling;
        if (selectedCard) {
            selectedCard.classList.add('selected');
        }
    }

    // Initialize when DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();