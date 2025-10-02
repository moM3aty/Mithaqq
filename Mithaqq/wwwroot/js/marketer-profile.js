let packages = [
  {
    id: "1",
    name: "Social Media Boost",
    description:
      "Complete social media management package with content creation and engagement strategies.",
    price: 299.99,
    category: "social-media",
    duration: "30 days",
    dateAdded: new Date(Date.now() - 86400000).toISOString(),
  },
  {
    id: "2",
    name: "SEO Optimization Pro",
    description:
      "Advanced SEO package including keyword research, on-page optimization, and backlink building.",
    price: 499.99,
    category: "seo",
    duration: "3 months",
    dateAdded: new Date(Date.now() - 172800000).toISOString(),
  },
  {
    id: "3",
    name: "Content Marketing Suite",
    description:
      "Complete content strategy with blog posts, infographics, and video content creation.",
    price: 399.99,
    category: "content",
    duration: "2 months",
    dateAdded: new Date(Date.now() - 259200000).toISOString(),
  },
];

document.addEventListener("DOMContentLoaded", () => {
  loadProfilePackages();
  updateStats();
});

function loadProfilePackages() {
  const packagesList = document.getElementById("packagesList");
  const noPackages = document.getElementById("noPackages");

  if (!packagesList) return;

  if (packages.length === 0) {
    packagesList.innerHTML = "";
    noPackages.style.display = "block";
    return;
  }

  noPackages.style.display = "none";
  packagesList.innerHTML = packages.map((pkg) => createPackageCard(pkg)).join("");
  updateStats();
}

function addPackage() {
  const form = document.getElementById("addPackageForm");

  if (!form.checkValidity()) {
    form.reportValidity();
    return;
  }

  const packageData = {
    id: Date.now().toString(),
    name: document.getElementById("packageName").value,
    description: document.getElementById("packageDescription").value,
    price: parseFloat(document.getElementById("packagePrice").value),
    category: document.getElementById("packageCategory").value,
    duration: document.getElementById("packageDuration").value,
    dateAdded: new Date().toISOString(),
  };

  packages.push(packageData);

  form.reset();
  const modal = bootstrap.Modal.getInstance(document.getElementById("addPackageModal"));
  modal.hide();

  loadProfilePackages();
  showNotification("Package added successfully!", "success");
}

function removePackage(packageId) {
  if (!confirm("Are you sure you want to remove this package?")) return;

  packages = packages.filter((p) => p.id !== packageId);
  loadProfilePackages();
  showNotification("Package removed successfully!", "success");
}


function createPackageCard(pkg) {
  const categoryColors = {
    "social-media": "info",
    seo: "success",
    content: "warning",
    ppc: "danger",
    email: "primary",
    analytics: "secondary",
  };

  const categoryNames = {
    "social-media": "Social Media",
    seo: "SEO",
    content: "Content Marketing",
    ppc: "PPC Advertising",
    email: "Email Marketing",
    analytics: "Analytics",
  };

  return `
    <div class="col-md-6 mb-4" data-package-id="${pkg.id}">
      <div class="card package-card h-100 shadow-sm">
        <div class="package-header">
          <div class="d-flex justify-content-between align-items-start">
            <h6 class="mb-0 flex-grow-1">${pkg.name}</h6>
            <button class="btn btn-link text-white p-0" onclick="removePackage('${pkg.id}')">
              <i class="fas fa-trash"></i>
            </button>
          </div>
        </div>
        <div class="card-body">
        <h3 class="mb-4">${pkg.name}</h3>
          <div class="d-flex justify-content-between align-items-center mb-2">
          
            <span class="package-price">$${pkg.price.toFixed(2)}</span>
            <span class="badge badge-custome">
              ${categoryNames[pkg.category] || pkg.category}
            </span>
          </div>
          ${
            pkg.description
              ? `<p class="card-text my-3 text-muted small">${pkg.description}</p>`
              : ""
          }
         
        </div>
        <div class="card-footer d-flex justify-content-between bg-transparent border-top">
        <div> ${
            pkg.duration
              ? `<div class="d-flex align-items-center text-muted small">
                   <i class="fas fa-clock me-1"></i>${pkg.duration}
                 </div>`
              : ""
          }
          </div>
          <small class="text-muted">
            <i class="fas fa-calendar me-1"></i>
            Added ${new Date(pkg.dateAdded).toLocaleDateString()}
          </small>
        </div>
      </div>
    </div>`;
}

function updateStats() {
  const profilePackageCount = document.getElementById("profilePackageCount");
  if (profilePackageCount) profilePackageCount.textContent = packages.length;
}

function showNotification(message, type = "info") {
  alert(`[${type.toUpperCase()}] ${message}`);
}

let marketerProfile = {
  name: "John Marketer",
  title: "Digital Marketing Specialist",
  avatar: "../images/company1.jpg"
};

document.addEventListener("DOMContentLoaded", () => {
  loadProfile();
});

function loadProfile() {
  document.getElementById("marketerName").innerHTML = marketerProfile.name;
  document.getElementById("marketerTitle").innerHTML = marketerProfile.title;
  document.getElementById("marketerAvatar").src = marketerProfile.avatar;
}

function saveProfile() {
  const form = document.getElementById("editProfileForm");
  if (!form.checkValidity()) {
    form.reportValidity();
    return;
  }

  marketerProfile.name = document.getElementById("editName").value;
  marketerProfile.title = document.getElementById("editTitle").value;

  const fileInput = document.getElementById("editAvatar");
  if (fileInput.files && fileInput.files[0]) {
    const reader = new FileReader();
    reader.onload = (e) => {
      marketerProfile.avatar = e.target.result; 
      loadProfile();
    };
    reader.readAsDataURL(fileInput.files[0]);
  } else {
    loadProfile();
  }
  const modal = bootstrap.Modal.getInstance(document.getElementById("editProfileModal"));
  modal.hide();

  showNotification("Profile updated successfully!", "success");
}
