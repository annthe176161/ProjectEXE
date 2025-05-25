document.addEventListener('DOMContentLoaded', function () {
    let currentStep = 1;
    const totalSteps = 3;

    // Step navigation
    function showStep(step) {
        // Hide all steps
        document.querySelectorAll('.form-step').forEach(s => s.classList.remove('active'));
        document.querySelectorAll('.step').forEach(s => {
            s.classList.remove('active', 'completed');
        });

        // Show current step
        document.querySelector(`[data-step="${step}"]`).classList.add('active');

        // Update progress
        for (let i = 1; i <= step; i++) {
            if (i < step) {
                document.querySelector(`.step[data-step="${i}"]`).classList.add('completed');
            } else if (i === step) {
                document.querySelector(`.step[data-step="${i}"]`).classList.add('active');
            }
        }

        currentStep = step;
    }

    // Next button handlers
    document.querySelectorAll('.btn-next').forEach(btn => {
        btn.addEventListener('click', function () {
            const nextStep = parseInt(this.dataset.next);
            if (validateCurrentStep()) {
                showStep(nextStep);
            }
        });
    });

    // Previous button handlers
    document.querySelectorAll('.btn-prev').forEach(btn => {
        btn.addEventListener('click', function () {
            const prevStep = parseInt(this.dataset.prev);
            showStep(prevStep);
        });
    });

    // Form validation for current step
    function validateCurrentStep() {
        const currentFormStep = document.querySelector('.form-step.active');
        const inputs = currentFormStep.querySelectorAll('input[required]');
        let isValid = true;

        inputs.forEach(input => {
            if (!input.value.trim()) {
                input.classList.add('is-invalid');
                isValid = false;
            } else {
                input.classList.remove('is-invalid');
            }
        });

        return isValid;
    }

    // Password toggle functionality
    window.togglePassword = function (inputId, iconId) {
        const passwordInput = document.getElementById(inputId);
        const passwordIcon = document.getElementById(iconId);

        if (passwordInput.type === 'password') {
            passwordInput.type = 'text';
            passwordIcon.classList.remove('fa-eye');
            passwordIcon.classList.add('fa-eye-slash');
        } else {
            passwordInput.type = 'password';
            passwordIcon.classList.remove('fa-eye-slash');
            passwordIcon.classList.add('fa-eye');
        }
    };

    // Password strength checker
    const passwordInput = document.getElementById('passwordInput');
    const confirmPasswordInput = document.getElementById('confirmPasswordInput');

    if (passwordInput) {
        passwordInput.addEventListener('input', function () {
            checkPasswordStrength(this.value);
            checkPasswordRequirements(this.value);
            checkPasswordMatch();
        });
    }

    if (confirmPasswordInput) {
        confirmPasswordInput.addEventListener('input', checkPasswordMatch);
    }

    function checkPasswordStrength(password) {
        const strengthFill = document.getElementById('strengthFill');
        const strengthText = document.getElementById('strengthText');

        if (!strengthFill || !strengthText) return;

        let strength = 0;
        const checks = [
            /.{8,}/, // At least 8 characters
            /[a-z]/, // Lowercase
            /[A-Z]/, // Uppercase
            /[0-9]/, // Numbers
            /[^A-Za-z0-9]/ // Special characters
        ];

        checks.forEach(check => {
            if (check.test(password)) strength++;
        });

        strengthFill.className = 'strength-fill';

        if (strength === 0) {
            strengthText.textContent = 'Độ mạnh mật khẩu';
        } else if (strength <= 2) {
            strengthFill.classList.add('weak');
            strengthText.textContent = 'Yếu';
        } else if (strength === 3) {
            strengthFill.classList.add('fair');
            strengthText.textContent = 'Trung bình';
        } else if (strength === 4) {
            strengthFill.classList.add('good');
            strengthText.textContent = 'Tốt';
        } else {
            strengthFill.classList.add('strong');
            strengthText.textContent = 'Rất mạnh';
        }
    }

    function checkPasswordRequirements(password) {
        const requirements = {
            length: password.length >= 8,
            uppercase: /[A-Z]/.test(password),
            lowercase: /[a-z]/.test(password),
            number: /[0-9]/.test(password)
        };

        Object.keys(requirements).forEach(req => {
            const element = document.querySelector(`[data-requirement="${req}"]`);
            if (element) {
                const icon = element.querySelector('i');
                if (requirements[req]) {
                    element.classList.add('valid');
                    icon.classList.remove('fa-times');
                    icon.classList.add('fa-check');
                } else {
                    element.classList.remove('valid');
                    icon.classList.remove('fa-check');
                    icon.classList.add('fa-times');
                }
            }
        });
    }

    function checkPasswordMatch() {
        const password = passwordInput.value;
        const confirmPassword = confirmPasswordInput.value;
        const matchIndicator = document.getElementById('passwordMatch');

        if (confirmPassword && password === confirmPassword) {
            matchIndicator.style.display = 'flex';
            confirmPasswordInput.classList.remove('is-invalid');
            confirmPasswordInput.classList.add('is-valid');
        } else if (confirmPassword) {
            matchIndicator.style.display = 'none';
            confirmPasswordInput.classList.remove('is-valid');
            confirmPasswordInput.classList.add('is-invalid');
        } else {
            matchIndicator.style.display = 'none';
            confirmPasswordInput.classList.remove('is-valid', 'is-invalid');
        }
    }

    // Form submission
    const registerForm = document.getElementById('registerForm');
    const registerButton = document.getElementById('registerButton');
    const registerText = document.getElementById('registerText');
    const btnLoader = document.getElementById('btnLoader');

    registerForm.addEventListener('submit', function (e) {
        if (!validateCurrentStep()) {
            e.preventDefault();
            return;
        }

        // Add loading state
        registerButton.classList.add('loading');
        registerButton.disabled = true;
        registerText.textContent = 'Đang tạo tài khoản...';
        btnLoader.style.display = 'block';
    });

    // Enhanced form validation feedback
    const inputs = document.querySelectorAll('.form-control');
    inputs.forEach(input => {
        input.addEventListener('blur', function () {
            if (this.value.trim() !== '') {
                this.classList.add('has-value');
            } else {
                this.classList.remove('has-value');
            }
        });

        input.addEventListener('focus', function () {
            this.classList.remove('is-invalid');
            const errorSpan = this.parentNode.querySelector('.field-validation-error');
            if (errorSpan) {
                errorSpan.style.display = 'none';
            }
        });
    });

    // Role selection animation
    const roleCards = document.querySelectorAll('.role-card');
    roleCards.forEach(card => {
        card.addEventListener('click', function () {
            // Add selection animation
            this.style.transform = 'scale(0.98)';
            setTimeout(() => {
                this.style.transform = '';
            }, 150);
        });
    });

    // Auto-focus first input
    const firstInput = document.querySelector('.form-step.active input');
    if (firstInput) {
        firstInput.focus();
    }

    // Keyboard navigation
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Enter' && e.ctrlKey) {
            if (currentStep < totalSteps) {
                document.querySelector('.btn-next').click();
            } else {
                registerForm.submit();
            }
        }
    });
});