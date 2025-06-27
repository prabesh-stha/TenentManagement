// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
(document => {
    "use strict";

    // Initialize when DOM is loaded
    document.addEventListener("DOMContentLoaded", () => {


        // Toast notification handler
        function initializeToastNotifications() {
            console.log("initializing");
            const toastContainer = document.getElementById("toast-container");
            const toastMessageEl = document.getElementById("toastMessage");

            if (!toastContainer || !toastMessageEl) return;

            const message = toastContainer.dataset.message?.trim();
            const messageType = toastContainer.dataset.type;

            if (message) {
                const toastHeader = toastMessageEl.querySelector(".toast-header");
                const toastBody = document.getElementById("toast-body");

                toastBody.textContent = message;

                // Reset header styling
                toastHeader.classList.remove("bg-success", "bg-danger", "bg-warning", "bg-info", "text-white", "text-dark");

                let headerClass = "bg-success";
                let textClass = "text-white";

                switch (messageType) {
                    case "success":
                        headerClass = "bg-success";
                        textClass = "text-white";
                        break;
                    case "error":
                        headerClass = "bg-danger";
                        textClass = "text-white";
                        break;
                    case "warning":
                        headerClass = "bg-warning";
                        textClass = "text-dark";
                        break;
                    case "info":
                        headerClass = "bg-info";
                        textClass = "text-dark";
                        break;
                }

                toastHeader.classList.add(headerClass, textClass);

                const toast = new bootstrap.Toast(toastMessageEl, {
                    autohide: true,
                    delay: 3000
                });

                toast.show();

                toastContainer.removeAttribute("data-message");
                toastContainer.removeAttribute("data-type");
            }
        }
        //for password visibility toggle
        function initializePasswordToggles() {
            document.querySelectorAll('.toggle-password').forEach(button => {
                button.addEventListener('click', function () {
                    const input = this.previousElementSibling;
                    const type = input.type === 'password' ? 'text' : 'password';
                    input.type = type;
                    this.querySelector('i').classList.toggle('bi-eye');
                    this.querySelector('i').classList.toggle('bi-eye-slash');
                });
            });
        }

        // Form spinner handler
        function initializeFormSpinners() {
            document.querySelectorAll('form[data-enable-spinner="true"]').forEach(form => {
                const submitButton = form.querySelector('button[type="submit"]');
                if (!submitButton) return;

                // Preserve original state
                submitButton.dataset.originalHtml = submitButton.innerHTML;

                form.addEventListener("submit", function (e) {
                    if (this.checkValidity() && !submitButton.disabled) {
                        disableButton(submitButton);
                    }
                });
            });
        }
        function disableButton(button) {
            const loadingText = button.dataset.loadingText || "Submitting...";
            button.disabled = true;
            button.innerHTML = `
            <span class="spinner-border spinner-border-sm" 
                  role="status" 
                  aria-hidden="true"></span>
            ${loadingText}
        `;
        }


        // Phone Number Formatting
        function initializePhoneInput() {
            const phoneInput = document.querySelector('input[type="tel"]');
            if (phoneInput) {
                phoneInput.addEventListener('input', function (e) {
                    // Remove all non-digit characters
                    let cleaned = e.target.value.replace(/\D/g, '');
                    // Limit to 10 digits
                    if (cleaned.length > 10) {
                        cleaned = cleaned.slice(0, 10);
                    }
                    // Apply xxxx-xxxxxx format
                    if (cleaned.length > 4) {
                        e.target.value = cleaned.slice(0, 4) + '-' + cleaned.slice(4);
                    } else {
                        e.target.value = cleaned;
                    }
                });
            }
        }



        initializePasswordToggles();
        initializeToastNotifications();
        initializeFormSpinners();
        initializePhoneInput();

    });

})(document);