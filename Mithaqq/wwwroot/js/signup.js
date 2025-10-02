document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("signupForm");
    const userName = document.getElementById("userName");
    const userEmail = document.getElementById("userEmail");
    const password = document.getElementById("password");
    const confirmPassword = document.getElementById("confirm-password");

    const patterns = {
        name: /^[a-zA-Z\s]{3,}$/,
        email: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
        password: /^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{8,}$/
    };

    function showError(input, message) {
        const errorMsg = input.closest(".col-12, .co-12").querySelector(".error-msg");
        if (errorMsg) errorMsg.textContent = message;
    }

    function clearError(input) {
        const errorMsg = input.closest(".col-12, .co-12").querySelector(".error-msg");
        if (errorMsg) errorMsg.textContent = "";
    }

    function validateField(input) {
        const value = input.value.trim();

        if (input.id === "userName") {
            if (!value) {
                showError(input, "Name is required");
                return false;
            } else if (!patterns.name.test(value)) {
                showError(input, "Name must be at least 3 letters");
                return false;
            }
        }

        if (input.id === "userEmail") {
            if (!value) {
                showError(input, "Email is required");
                return false;
            } else if (!patterns.email.test(value)) {
                showError(input, "Invalid email format");
                return false;
            }
        }

        if (input.id === "password") {
            if (!value) {
                showError(input, "Password is required");
                return false;
            } else if (!patterns.password.test(value)) {
                showError(input, "Password must be 8+ chars, include a letter, a number, and a special char");
                return false;
            }
        }

        if (input.id === "confirm-password") {
            if (!value) {
                showError(input, "Please confirm your password");
                return false;
            } else if (value !== password.value.trim()) {
                showError(input, "Passwords do not match");
                return false;
            }
        }


        clearError(input);
        return true;
    }

    if (form) {
        form.addEventListener("submit", function (e) {
            e.preventDefault();
            let isValid = true;

            [userName, userEmail, password, confirmPassword].forEach(input => {
                if (!validateField(input)) {
                    isValid = false;
                }
            });

            if (isValid) {
                const user = {
                    name: userName.value.trim(),
                    email: userEmail.value.trim(),
                    password: password.value.trim(),
                };

                let users = JSON.parse(localStorage.getItem("users")) || [];

                if (users.some(u => u.name === user.name || u.email === user.email)) {
                    alert("An account with this name or email already exists!");
                    return;
                }

                users.push(user);
                localStorage.setItem("users", JSON.stringify(users));

                alert("Signup successful!");
                window.location.href = "login.html";
            }
        });

        [userName, userEmail, password, confirmPassword].forEach(input => {
            input.addEventListener("input", () => validateField(input));
            input.addEventListener("change", () => validateField(input));
        });
    }
});
