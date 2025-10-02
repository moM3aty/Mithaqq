document.addEventListener("DOMContentLoaded", function () {
  /* ---------------------- BACK TO TOP & CART ---------------------- */
  const backToTop = document.getElementById("backToTop");
  const cartBtn = document.getElementById("cartBtn");

  if (backToTop) {
    window.addEventListener("scroll", () => {
      backToTop.style.display = window.scrollY > 300 ? "flex" : "none";
    });

    backToTop.addEventListener("click", () => {
      window.scrollTo({ top: 0, behavior: "smooth" });
    });
  }

  if (cartBtn) {
    cartBtn.addEventListener("click", () => {
      alert("Cart button clicked!");
    });
  }

  /* ---------------------- CONTACT FORM VALIDATION ---------------------- */
  const form = document.getElementById("contactForm");
  if (form) {
    const fields = {
      firstName: { el: document.getElementById("firstName"), regex: /^[A-Za-z]{2,}$/ },
      lastName: { el: document.getElementById("lastName"), regex: /^[A-Za-z]{2,}$/ },
      email: { el: document.getElementById("email"), regex: /^[^\s@]+@[^\s@]+\.[^\s@]+$/ },
      subject: { el: document.getElementById("subject"), regex: /^.{3,}$/ },
      message: { el: document.getElementById("message"), regex: /^.{5,}$/ }
    };

    Object.values(fields).forEach(({ el }) => {
      const errorDiv = el.nextElementSibling;
      if (errorDiv) errorDiv.style.display = "none";
      el.addEventListener("input", () => validateField(el));
    });

    function validateField(input) {
      const field = Object.values(fields).find(f => f.el === input);
      const errorDiv = input.nextElementSibling;
      if (!errorDiv) return true;

      if (!input.value.trim()) {
        showTypingError(errorDiv, "This field is required.");
        input.classList.add("is-invalid");
        return false;
      }

      if (field.regex && !field.regex.test(input.value.trim())) {
        showTypingError(errorDiv, "Invalid format.");
        input.classList.add("is-invalid");
        return false;
      }

      errorDiv.textContent = "";
      errorDiv.style.display = "none";
      input.classList.remove("is-invalid");
      input.classList.add("is-valid");
      return true;
    }

    function showTypingError(element, message) {
      element.textContent = "";
      element.style.display = "block";
      let i = 0;
      const typing = setInterval(() => {
        element.textContent += message.charAt(i++);
        if (i === message.length) clearInterval(typing);
      }, 40);
    }

    form.addEventListener("submit", e => {
      e.preventDefault();
      let isValid = true;

      Object.values(fields).forEach(({ el }) => {
        if (!validateField(el)) isValid = false;
      });

      if (!isValid) return;

      const phone = "01276832619";
      const whatsappUrl =
        `https://wa.me/${phone}?text=` +
        encodeURIComponent(
          `New Contact Message:
First Name: ${fields.firstName.el.value}
Last Name: ${fields.lastName.el.value}
Email: ${fields.email.el.value}
Phone: ${document.getElementById("phone").value || "N/A"}
Company: ${document.getElementById("company").value || "N/A"}
Department: ${document.getElementById("department").value || "N/A"}
Subject: ${fields.subject.el.value}
Message: ${fields.message.el.value}
Priority: ${document.getElementById("priority").value}`
        );

      window.open(whatsappUrl, "_blank");
      form.reset();
      Object.values(fields).forEach(({ el }) => el.classList.remove("is-valid"));
    });
  }

  /* ---------------------- NAVBAR SCROLL & ACTIVE LINKS ---------------------- */
  const nav = document.querySelector(".custom-navbar");
  const navbarLinks = document.querySelectorAll(".navbar-nav .nav-link");
  const navbarCollapse = document.getElementById("mainNav");
  const toggleButton = document.querySelector(".navbar-toggler");
  const sections = document.querySelectorAll("section[id]");

  if (nav && navbarLinks.length) {
    const isLargeScreen = () => window.innerWidth >= 992;

    const updateActiveLink = () => {
      const scrollPos = window.scrollY;

      if (scrollPos < 50) {
        navbarLinks.forEach(link => link.classList.remove("active"));
        const homeLink = document.querySelector('.navbar-nav .nav-link[href="#"]');
        if (homeLink) homeLink.classList.add("active");
        return;
      }

      let activeSet = false;
      sections.forEach(section => {
        if (
          scrollPos + nav.offsetHeight >= section.offsetTop &&
          scrollPos < section.offsetTop + section.offsetHeight
        ) {
          const id = section.getAttribute("id");
          navbarLinks.forEach(link => link.classList.remove("active"));
          const activeLink = document.querySelector(`.navbar-nav .nav-link[href="#${id}"]`);
          if (activeLink) activeLink.classList.add("active");
          activeSet = true;
        }
      });

      if (!activeSet) {
        navbarLinks.forEach(link => link.classList.remove("active"));
        const homeLink = document.querySelector('.navbar-nav .nav-link[href="#"]');
        if (homeLink) homeLink.classList.add("active");
      }
    };

    const updateNavbarBackground = () => {
      nav.classList.toggle("scrolled", window.scrollY > 50);
    };

    const updateNavbar = () => {
      updateNavbarBackground();
      updateActiveLink();
    };

    document.addEventListener("scroll", updateNavbar);
    window.addEventListener("resize", () => {
      updateNavbar();
      if (isLargeScreen()) navbarCollapse.style.backgroundColor = "";
    });

    navbarLinks.forEach(link => {
      link.addEventListener("click", function () {
        navbarLinks.forEach(l => l.classList.remove("active"));
        this.classList.add("active");
        if (!isLargeScreen()) new bootstrap.Collapse(navbarCollapse).hide();
      });
    });

   function handleResize() {
  if (isLargeScreen()) {
    navbarCollapse.classList.remove("navbar-dark-bg");
    nav.classList.remove("scrolled");
  }
}

if (toggleButton && navbarCollapse) {
  toggleButton.addEventListener("click", () => {
    if (!navbarCollapse.classList.contains("show")) {
      navbarCollapse.classList.add("navbar-dark-bg");
    } else {
      navbarCollapse.classList.remove("navbar-dark-bg");
      if (isLargeScreen() && window.scrollY < 50) {
        nav.classList.remove("scrolled");
      }
    }
  });

  window.addEventListener("resize", handleResize);
}


    updateNavbar();
  }

  /* ---------------------- AOS INIT ---------------------- */
  if (window.AOS) {
    AOS.init({
      duration: 1000,
      once: false,
      offset: 100
    });
  }
});
