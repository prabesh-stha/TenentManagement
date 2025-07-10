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

        // Map Picker Initialization for any element with data-map-picker="true"
        function initializeMapPicker(config) {
            const map = L.map(config.mapElementId).setView([config.initialLat, config.initialLng], 13);
            let marker;

            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '© OpenStreetMap contributors'
            }).addTo(map);

            const hasInputs = config.latInputId && config.lngInputId && config.addressInputId;
            if (hasInputs && config.initialAddress) {
                document.getElementById(config.addressInputId).value = config.initialAddress;
            }
            // Only add search if editable
            if (hasInputs) {
                L.Control.geocoder({
                    defaultMarkGeocode: false
                })
                    .on('markgeocode', function (e) {
                        const latlng = e.geocode.center;
                        if (marker) map.removeLayer(marker);
                        marker = L.marker(latlng).addTo(map);
                        map.setView(latlng, 15);

                        document.getElementById(config.latInputId).value = latlng.lat;
                        document.getElementById(config.lngInputId).value = latlng.lng;
                        document.getElementById(config.addressInputId).value = e.geocode.name;
                    })
                    .addTo(map);
            }

            if (config.initialLat && config.initialLng) {
                marker = L.marker([config.initialLat, config.initialLng]).addTo(map);
            }

            // Only allow clicking map to set marker if inputs exist
            if (hasInputs) {
                map.on('click', function (e) {
                    const lat = e.latlng.lat;
                    const lng = e.latlng.lng;

                    if (marker) map.removeLayer(marker);
                    marker = L.marker([lat, lng]).addTo(map);

                    document.getElementById(config.latInputId).value = lat;
                    document.getElementById(config.lngInputId).value = lng;

                    fetch(`https://nominatim.openstreetmap.org/reverse?lat=${lat}&lon=${lng}&format=json`)
                        .then(res => res.json())
                        .then(data => {
                            document.getElementById(config.addressInputId).value = data.display_name;
                        });
                });
            }
        }

        function autoInitializeMapPickers() {
            const mapElements = document.querySelectorAll("[data-map-picker='true']");
            mapElements.forEach(mapElement => {
                initializeMapPicker({
                    mapElementId: mapElement.id,
                    latInputId: mapElement.dataset.latInput,
                    lngInputId: mapElement.dataset.lngInput,
                    addressInputId: mapElement.dataset.addressInput,
                    initialLat: parseFloat(mapElement.dataset.latitude) || 27.703233528817258,
                    initialLng: parseFloat(mapElement.dataset.longitude) || 85.315162539482131,
                    initialAddress: mapElement.dataset.address || "Tundikhel, Kanti Path, Kathmandu-22, Kathmandu Metropolitan City, Kathmandu, Bagamati Province, 44066, Nepal"
                });
            });
        }


        // Renter Username Validation on Input
        function initializeRenterUsernameValidation() {
            const usernameInput = document.getElementById("usernameInput");
            const iconContainer = document.getElementById("usernameStatusIcon");
            const statusIcon = iconContainer ? iconContainer.querySelector("i") : null;

            const renterIdField = document.getElementById("RenterId");

            if (!usernameInput || !statusIcon || !renterIdField) return;

            let debounceTimer;

            usernameInput.addEventListener("input", function () {
                clearTimeout(debounceTimer);
                const username = this.value.trim();

                statusIcon.className = "";

                if (username.length === 0) {
                    renterIdField.value = "";
                    return;
                }

                debounceTimer = setTimeout(() => {
                    fetch(`/Authentication/ValidateUsername?username=${encodeURIComponent(username)}`)
                        .then(res => res.json())
                        .then(data => {
                            if (data.success) {
                                statusIcon.className = "bi bi-check-circle-fill text-success";
                                renterIdField.value = data.id;
                            } else {
                                statusIcon.className = "bi bi-x-circle-fill text-danger";
                                renterIdField.value = "";
                            }
                        })
                        .catch(() => {
                            statusIcon.className = "bi bi-exclamation-circle-fill text-danger";
                            renterIdField.value = "";
                        });
                }, 500);
            });

            usernameInput.addEventListener("focus", function () {
                statusIcon.className = "";
            });
        }

        // Hides validation spans when the corresponding input is focused
        function initializeValidationSpanHiding() {
            document.querySelectorAll("input, textarea, select").forEach(input => {
                const name = input.getAttribute("name");
                if (!name) return;

                const span = document.querySelector(`span[data-valmsg-for="${CSS.escape(name)}"]`);
                if (span) {
                    input.addEventListener("focus", () => {
                        span.classList.add("d-none");
                    });
                }
            });
        }


        // Reusable SweetAlert delete confirmation
        function confirmDeletion(itemType, itemId, deleteUrlBase, onSuccessRedirectUrl = null) {
            Swal.fire({
                title: `Delete ${itemType}?`,
                text: `Are you sure you want to delete this ${itemType.toLowerCase()}? This action cannot be undone.`,
                icon: "warning",
                showCancelButton: true,
                confirmButtonColor: "#d33",
                cancelButtonColor: "#6c757d",
                confirmButtonText: "Yes, delete it!"
            }).then((result) => {
                if (result.isConfirmed) {
                    fetch(`${deleteUrlBase}/${itemId}`, {
                        method: 'POST'
                    })
                        .then(response => {
                            if (response.ok) {
                                Swal.fire(
                                    `${itemType} Deleted`,
                                    `The ${itemType.toLowerCase()} was successfully deleted.`,
                                    "success"
                                ).then(() => {
                                    if (onSuccessRedirectUrl) {
                                        window.location.href = onSuccessRedirectUrl;
                                    } else {
                                        location.reload(); // fallback
                                    }
                                });
                            } else {
                                return response.text().then(text => {
                                    throw new Error(text || "Deletion failed.");
                                });
                            }
                        })
                        .catch(error => {
                            Swal.fire(
                                "Error",
                                error.message || `Failed to delete the ${itemType.toLowerCase()}.`,
                                "error"
                            );
                        });
                }
            });
        }
        window.confirmDeletion = confirmDeletion;




        initializePasswordToggles();
        initializeToastNotifications();
        initializePhoneInput();
        initializeFormSpinners();
        autoInitializeMapPickers();

        initializeRenterUsernameValidation();
        initializeValidationSpanHiding();
    });

})(document);