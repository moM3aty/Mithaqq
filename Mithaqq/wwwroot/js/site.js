// Main Site Script for Mithaqq Project

document.addEventListener("DOMContentLoaded", function () {

    // Initialize AOS (Animate on Scroll) Library
    if (window.AOS) {
        AOS.init({
            duration: 1000,
            once: true, // Animate elements only once
            offset: 100
        });
    }

    // Initialize Navbar Scroll Behavior
    initializeNavbar();

    // Initialize Back to Top button
    initializeBackToTop();
});

function initializeNavbar() {
    const nav = document.querySelector(".custom-navbar");
    if (!nav) return;

    const updateNavbarBackground = () => {
        nav.classList.toggle("scrolled", window.scrollY > 50);
    };

    window.addEventListener("scroll", updateNavbarBackground);
    updateNavbarBackground(); // Run on page load
}

function initializeBackToTop() {
    const backToTopButton = document.getElementById("backToTop");
    if (!backToTopButton) return;

    window.addEventListener("scroll", () => {
        backToTopButton.style.display = (window.scrollY > 300) ? "flex" : "none";
    });

    backToTopButton.addEventListener("click", () => {
        window.scrollTo({ top: 0, behavior: "smooth" });
    });
}

// Password Toggle Visibility (Robust version using event delegation)
document.addEventListener('click', function (e) {
    if (e.target.classList.contains('toggle-password')) {
        const icon = e.target;
        const inputId = icon.getAttribute('data-input');
        if (inputId) {
            const input = document.getElementById(inputId);
            if (input) {
                const isPassword = input.type === 'password';
                input.type = isPassword ? 'text' : 'password';
                icon.classList.toggle('fa-eye', !isPassword);
                icon.classList.toggle('fa-eye-slash', isPassword);
            }
        }
    }
});
