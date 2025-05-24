/* filepath: wwwroot/js/shop-order-details.js */

document.addEventListener('DOMContentLoaded', function () {
    initializeOrderDetails();
});

function initializeOrderDetails() {
    setupStatusChangeHandler();
    setupFormValidation();
    setupLoadingStates();
    showStatusTransitionInfo();
    setupAlertAutoHide();
}

function setupStatusChangeHandler() {
    const statusSelect = document.getElementById('statusSelect');
    const cancelReasonGroup = document.getElementById('cancelReasonGroup');
    const cancelReasonInput = document.getElementById('cancelReasonInput');
    const updateButton = document.getElementById('updateButton');

    if (statusSelect && cancelReasonGroup && cancelReasonInput && updateButton) {
        // Set initial state
        toggleCancelReasonField(statusSelect.value);
        updateButtonText(statusSelect.value);
        updateHelpText(statusSelect.value);

        statusSelect.addEventListener('change', function () {
            toggleCancelReasonField(this.value);
            updateButtonText(this.value);
            updateHelpText(this.value);
            showTransitionWarning(this.value);
        });
    }
}

function toggleCancelReasonField(statusValue) {
    const cancelReasonGroup = document.getElementById('cancelReasonGroup');
    const cancelReasonInput = document.getElementById('cancelReasonInput');

    if (statusValue === '5') {
        // Hiển thị trường lý do hủy
        cancelReasonGroup.classList.remove('d-none');
        cancelReasonInput.required = true;

        // Focus vào textarea sau một chút để animation mượt
        setTimeout(() => {
            cancelReasonInput.focus();
        }, 100);

        // Add animation
        cancelReasonGroup.style.opacity = '0';
        setTimeout(() => {
            cancelReasonGroup.style.transition = 'opacity 0.3s ease';
            cancelReasonGroup.style.opacity = '1';
        }, 50);
    } else {
        // Ẩn trường lý do hủy
        cancelReasonGroup.classList.add('d-none');
        cancelReasonInput.required = false;
        if (statusValue !== '5') {
            cancelReasonInput.value = '';
        }
        clearValidationError('cancelReasonInput');
    }
}

function updateButtonText(statusValue) {
    const updateButton = document.getElementById('updateButton');
    if (!updateButton) return;

    const buttonTexts = {
        '1': '<i class="fas fa-clock me-2"></i>Giữ trạng thái chờ',
        '2': '<i class="fas fa-check-circle me-2"></i>Xác nhận đơn hàng',
        '3': '<i class="fas fa-shipping-fast me-2"></i>Bắt đầu giao hàng',
        '4': '<i class="fas fa-trophy me-2"></i>Hoàn thành đơn hàng',
        '5': '<i class="fas fa-times-circle me-2"></i>Hủy đơn hàng'
    };

    updateButton.innerHTML = buttonTexts[statusValue] || '<i class="fas fa-save me-2"></i>Cập nhật trạng thái';

    // Thay đổi màu button theo trạng thái
    updateButton.className = 'btn btn-lg w-100 ' + getButtonClass(statusValue);
}

function updateHelpText(statusValue) {
    const helpTextElement = document.getElementById('statusHelpText');
    if (!helpTextElement) return;

    const helpTexts = {
        '1': 'Bạn có thể giữ nguyên trạng thái này, xác nhận đơn hàng hoặc hủy đơn hàng',
        '2': 'Bạn có thể bắt đầu giao hàng hoặc hủy đơn hàng',
        '3': 'Bạn có thể hoàn thành đơn hàng khi khách hàng đã nhận hàng',
        '4': 'Hoàn thành đơn hàng - trạng thái cuối cùng',
        '5': 'Hủy đơn hàng - vui lòng nhập lý do hủy'
    };

    helpTextElement.textContent = helpTexts[statusValue] || 'Chọn trạng thái mới cho đơn hàng';
}

function getButtonClass(statusValue) {
    return {
        '1': 'btn-warning',
        '2': 'btn-info',
        '3': 'btn-primary',
        '4': 'btn-success',
        '5': 'btn-danger'
    }[statusValue] || 'btn-primary-custom';
}

function showTransitionWarning(statusValue) {
    // Remove existing warnings
    const existingWarning = document.querySelector('.status-transition-warning');
    if (existingWarning) {
        existingWarning.remove();
    }

    const currentStatus = getCurrentStatus();

    // Kiểm tra xem có phải là chuyển đổi không hợp lệ không
    if (!isValidTransition(currentStatus, parseInt(statusValue))) {
        const warningDiv = document.createElement('div');
        warningDiv.className = 'alert alert-warning alert-dismissible fade show status-transition-warning mt-3';
        warningDiv.innerHTML = `
            <div class="d-flex align-items-center">
                <i class="fas fa-exclamation-triangle me-2"></i>
                <div>${getTransitionWarningMessage(currentStatus, parseInt(statusValue))}</div>
            </div>
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        const form = document.getElementById('statusUpdateForm');
        form.appendChild(warningDiv);
        return;
    }

    // Hiển thị thông báo hướng dẫn cho các chuyển đổi hợp lệ
    const warningMessages = {
        '2': {
            type: 'info',
            message: '📞 Sau khi xác nhận, bạn có thể liên hệ trực tiếp với khách hàng để trao đổi thông tin giao hàng.'
        },
        '3': {
            type: 'warning',
            message: '🚚 Đảm bảo bạn đã chuẩn bị hàng và sẵn sàng giao hàng cho khách hàng.'
        },
        '4': {
            type: 'success',
            message: '🎉 Xác nhận hoàn thành giao dịch. Trạng thái này không thể thay đổi sau khi cập nhật.'
        },
        '5': {
            type: 'danger',
            message: '⚠️ Hủy đơn hàng là hành động không thể hoàn tác. Vui lòng cân nhắc kỹ trước khi thực hiện.'
        }
    };

    const warning = warningMessages[statusValue];
    if (warning && statusValue !== currentStatus.toString()) {
        const warningDiv = document.createElement('div');
        warningDiv.className = `alert alert-${warning.type} alert-dismissible fade show status-transition-warning mt-3`;
        warningDiv.innerHTML = `
            <div class="d-flex align-items-center">
                <i class="fas fa-info-circle me-2"></i>
                <div>${warning.message}</div>
            </div>
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        const form = document.getElementById('statusUpdateForm');
        form.appendChild(warningDiv);

        // Auto hide after 8 seconds
        setTimeout(() => {
            if (warningDiv.parentNode) {
                warningDiv.classList.remove('show');
            }
        }, 8000);
    }
}

function getCurrentStatus() {
    const statusSelect = document.getElementById('statusSelect');
    return parseInt(statusSelect.dataset.currentStatus || statusSelect.value);
}

function isValidTransition(currentStatus, newStatus) {
    // Logic validation theo business rules
    if (currentStatus === 4 || currentStatus === 5) {
        return currentStatus === newStatus;
    }

    switch (newStatus) {
        case 1: return currentStatus === 1;
        case 2: return currentStatus === 1 || currentStatus === 2;
        case 3: return currentStatus === 2 || currentStatus === 3;
        case 4: return currentStatus === 3;
        case 5: return [1, 2, 3, 5].includes(currentStatus);
        default: return false;
    }
}

function getTransitionWarningMessage(currentStatus, newStatus) {
    const statusNames = {
        1: 'Chờ xác nhận',
        2: 'Đã xác nhận',
        3: 'Đang giao hàng',
        4: 'Đã giao - Hoàn thành',
        5: 'Đã hủy'
    };

    if (currentStatus === 4) {
        return '❌ Không thể thay đổi trạng thái đơn hàng đã hoàn thành';
    }

    if (currentStatus === 5) {
        return '❌ Không thể thay đổi trạng thái đơn hàng đã hủy';
    }

    switch (`${currentStatus}-${newStatus}`) {
        case '1-3': return '❌ Không thể chuyển từ "Chờ xác nhận" sang "Đang giao hàng". Vui lòng xác nhận đơn hàng trước.';
        case '1-4': return '❌ Không thể chuyển từ "Chờ xác nhận" sang "Hoàn thành". Vui lòng xác nhận và giao hàng trước.';
        case '2-1': return '❌ Không thể quay lại trạng thái "Chờ xác nhận" sau khi đã xác nhận.';
        case '2-4': return '❌ Không thể chuyển từ "Đã xác nhận" sang "Hoàn thành". Vui lòng giao hàng trước.';
        case '3-1': return '❌ Không thể quay lại trạng thái "Chờ xác nhận" khi đang giao hàng.';
        case '3-2': return '❌ Không thể quay lại trạng thái "Đã xác nhận" khi đang giao hàng.';
        case '4-5': return '❌ Không thể hủy đơn hàng đã hoàn thành.';
        default: return `❌ Không thể chuyển từ "${statusNames[currentStatus]}" sang "${statusNames[newStatus]}"`;
    }
}

function setupFormValidation() {
    const form = document.getElementById('statusUpdateForm');

    if (form) {
        form.addEventListener('submit', function (e) {
            const isValid = validateForm();

            if (!isValid) {
                e.preventDefault();
                return false;
            }

            // Show confirmation for critical actions
            const statusSelect = document.getElementById('statusSelect');
            const currentStatus = getCurrentStatus();
            const newStatus = parseInt(statusSelect.value);

            if ((newStatus === 4 || newStatus === 5) && newStatus !== currentStatus) {
                const actionText = newStatus === 4 ? 'hoàn thành' : 'hủy';
                const confirmMessage = `Bạn có chắc chắn muốn ${actionText} đơn hàng này? Hành động này không thể hoàn tác.`;

                if (!confirm(confirmMessage)) {
                    e.preventDefault();
                    return false;
                }
            }

            // Show loading state
            showLoadingState();

            return true;
        });

        // Real-time validation for cancel reason
        const cancelReasonInput = document.getElementById('cancelReasonInput');
        if (cancelReasonInput) {
            cancelReasonInput.addEventListener('input', function () {
                validateCancelReason();
                updateCharacterCounter();
            });

            cancelReasonInput.addEventListener('blur', function () {
                validateCancelReason();
            });
        }
    }
}

function validateForm() {
    let isValid = true;

    const statusSelect = document.getElementById('statusSelect');
    const cancelReasonInput = document.getElementById('cancelReasonInput');
    const currentStatus = getCurrentStatus();
    const newStatus = parseInt(statusSelect.value);

    // Clear previous errors
    clearAllValidationErrors();

    // Validate status selection
    if (!statusSelect.value) {
        showValidationError('statusSelect', 'Vui lòng chọn trạng thái');
        isValid = false;
    }

    // Validate status transition
    if (!isValidTransition(currentStatus, newStatus)) {
        showValidationError('statusSelect', getTransitionWarningMessage(currentStatus, newStatus));
        isValid = false;
    }

    // Validate cancel reason if status is 5 (cancelled)
    if (statusSelect.value === '5') {
        if (!cancelReasonInput.value.trim()) {
            showValidationError('cancelReasonInput', 'Vui lòng nhập lý do hủy đơn hàng');
            isValid = false;
        } else if (cancelReasonInput.value.trim().length < 10) {
            showValidationError('cancelReasonInput', 'Lý do hủy phải có ít nhất 10 ký tự');
            isValid = false;
        } else if (cancelReasonInput.value.length > 500) {
            showValidationError('cancelReasonInput', 'Lý do hủy không được vượt quá 500 ký tự');
            isValid = false;
        }
    }

    return isValid;
}

function validateCancelReason() {
    const cancelReasonInput = document.getElementById('cancelReasonInput');
    const statusSelect = document.getElementById('statusSelect');

    if (statusSelect.value !== '5') return true;

    const value = cancelReasonInput.value.trim();

    if (!value) {
        showValidationError('cancelReasonInput', 'Vui lòng nhập lý do hủy đơn hàng');
        return false;
    }

    if (value.length < 10) {
        showValidationError('cancelReasonInput', 'Lý do hủy phải có ít nhất 10 ký tự');
        return false;
    }

    if (value.length > 500) {
        showValidationError('cancelReasonInput', 'Lý do hủy không được vượt quá 500 ký tự');
        return false;
    }

    clearValidationError('cancelReasonInput');
    return true;
}

function updateCharacterCounter() {
    const cancelReasonInput = document.getElementById('cancelReasonInput');
    let counterDiv = document.getElementById('cancelReasonCounter');

    if (!counterDiv) {
        counterDiv = document.createElement('div');
        counterDiv.className = 'form-text text-end';
        counterDiv.id = 'cancelReasonCounter';
        cancelReasonInput.parentNode.appendChild(counterDiv);
    }

    const current = cancelReasonInput.value.length;
    const max = 500;
    counterDiv.textContent = `${current}/${max} ký tự`;

    if (current > max * 0.9) {
        counterDiv.className = 'form-text text-end text-warning';
    } else if (current > max) {
        counterDiv.className = 'form-text text-end text-danger';
    } else {
        counterDiv.className = 'form-text text-end text-muted';
    }
}

function showValidationError(fieldId, message) {
    const field = document.getElementById(fieldId);
    if (!field) return;

    field.classList.add('is-invalid');

    // Find or create error element
    const errorElementId = fieldId + 'Error';
    let errorElement = document.getElementById(errorElementId);

    if (!errorElement) {
        errorElement = document.createElement('div');
        errorElement.id = errorElementId;
        errorElement.className = 'invalid-feedback';
        field.parentNode.appendChild(errorElement);
    }

    errorElement.textContent = message;
    errorElement.style.display = 'block';

    // Scroll to error
    field.scrollIntoView({ behavior: 'smooth', block: 'center' });
}

function clearValidationError(fieldId) {
    const field = document.getElementById(fieldId);
    if (!field) return;

    field.classList.remove('is-invalid');

    const errorElement = document.getElementById(fieldId + 'Error');
    if (errorElement) {
        errorElement.style.display = 'none';
    }
}

function clearAllValidationErrors() {
    const invalidFields = document.querySelectorAll('.is-invalid');
    invalidFields.forEach(field => {
        field.classList.remove('is-invalid');
    });

    const errorElements = document.querySelectorAll('.invalid-feedback');
    errorElements.forEach(element => {
        element.style.display = 'none';
    });
}

function showLoadingState() {
    const submitButton = document.getElementById('updateButton');
    if (submitButton) {
        submitButton.disabled = true;
        submitButton.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Đang cập nhật...';
    }
}

function setupLoadingStates() {
    // Reset loading state if page reloads due to error
    const submitButton = document.getElementById('updateButton');
    if (submitButton) {
        submitButton.disabled = false;
        // Button text will be updated by updateButtonText function
    }
}

function showStatusTransitionInfo() {
    // Show helpful information about status transitions
    const statusSelect = document.getElementById('statusSelect');
    if (statusSelect) {
        updateButtonText(statusSelect.value);
        updateHelpText(statusSelect.value);
    }
}

function setupAlertAutoHide() {
    // Handle alert auto-hide
    const alerts = document.querySelectorAll('.alert:not(.status-transition-warning)');
    alerts.forEach(alert => {
        // Auto hide success/info alerts
        if (alert.classList.contains('alert-success-custom')) {
            setTimeout(() => {
                if (alert.parentNode) {
                    alert.style.transition = 'opacity 0.3s ease';
                    alert.style.opacity = '0';
                    setTimeout(() => {
                        alert.remove();
                    }, 300);
                }
            }, 5000);
        }
    });
}