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

                // Update toast content
                toastBody.textContent = message;

                // Reset classes
                toastHeader.classList.remove("bg-success", "bg-danger", "text-white");

                // Set appropriate styling
                const headerClass = messageType === "success"
                    ? "bg-success"
                    : "bg-danger";
                toastHeader.classList.add(headerClass, "text-white");

                const toast = new bootstrap.Toast(toastMessageEl, {
                    autohide: true,
                    delay: 3000
                });
                // Show toast with Bootstrap
                //const toast = new bootstrap.Toast(toastMessageEl);
                toast.show();

                toastContainer.removeAttribute("data-message");
                toastContainer.removeAttribute("data-type");

                // Auto-hide after 3 seconds
                //setTimeout(() => toast.hide(), 3000);
            }
        }



        initializeToastNotifications();

    });

})(document);