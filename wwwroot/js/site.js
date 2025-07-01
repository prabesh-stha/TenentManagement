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
            const phoneInputs = document.querySelectorAll('input[type="tel"]');

            phoneInputs.forEach(phoneInput => {
                // Format input as XXXX-XXXXXX
                phoneInput.addEventListener('input', function (e) {
                    const x = e.target.value.replace(/\D/g, '').match(/(\d{0,4})(\d{0,6})/);
                    e.target.value = !x[2] ? x[1] : x[1] + '-' + x[2];
                });

                // Remove hyphen before form submission
                const form = phoneInput.closest('form');
                if (form) {
                    form.addEventListener('submit', function () {
                        phoneInput.value = phoneInput.value.replace(/-/g, '');
                    });
                }
            });
        }



        initializePasswordToggles();
        initializeToastNotifications();
        initializePhoneInput();
        initializeFormSpinners();

    });

})(document);