let salesChartInstance = null;
let productsChartInstance = null;
let enrollmentsChartInstance = null;
let completionChartInstance = null;

document.addEventListener('DOMContentLoaded', function () {
    initializeEventListeners();
});

function initializeEventListeners() {
    document.getElementById('addProductBtn')?.addEventListener('click', () => showProductModal());
    document.getElementById('addCourseBtn')?.addEventListener('click', () => showCourseModal());
    document.getElementById('addShippingZoneBtn')?.addEventListener('click', () => showShippingZoneModal());
    document.getElementById('addCategoryBtn')?.addEventListener('click', () => showCategoryModal());
    document.getElementById('addTravelPackageBtn')?.addEventListener('click', () => showTravelPackageModal());
    document.getElementById('addBlogPostBtn')?.addEventListener('click', () => showBlogModal());
    document.getElementById('addUserBtn')?.addEventListener('click', () => showUserModal());

    document.getElementById('saveProductBtn')?.addEventListener('click', saveProduct);
    document.getElementById('saveCourseBtn')?.addEventListener('click', saveCourse);
    document.getElementById('saveShippingZoneBtn')?.addEventListener('click', saveShippingZone);
    document.getElementById('saveCategoryBtn')?.addEventListener('click', saveCategory);
    document.getElementById('saveTravelPackageBtn')?.addEventListener('click', saveTravelPackage);
    document.getElementById('saveBlogPostBtn')?.addEventListener('click', saveBlogPost);
    document.getElementById('saveUserBtn')?.addEventListener('click', saveUser);
    document.getElementById('updateStatusBtn')?.addEventListener('click', updateOrderStatus);
    document.getElementById('printInvoiceBtn')?.addEventListener('click', printInvoice);

    document.getElementById('productSearchInput')?.addEventListener('input', applyProductFilters);
    document.getElementById('productCategoryFilter')?.addEventListener('change', applyProductFilters);
    document.getElementById('courseSearchInput')?.addEventListener('input', applyCourseFilters);
    document.getElementById('courseCategoryFilter')?.addEventListener('change', applyCourseFilters);
    document.getElementById('courseCompanyFilter')?.addEventListener('change', applyCourseFilters);
    document.getElementById('userSearchInput')?.addEventListener('input', applyUserFilters);
    document.getElementById('travelSearchInput')?.addEventListener('input', applyTravelFilters);
    document.getElementById('blogSearchInput')?.addEventListener('input', applyBlogFilters);

    // Add event listener for the reviews tab
    document.querySelector('a[data-bs-target="#reviews"]')?.addEventListener('shown.bs.tab', loadReviews);
    document.querySelector('a[data-bs-target="#analytics"]')?.addEventListener('shown.bs.tab', loadAnalytics);


    setupImagePreview('productImageFile', 'productImagePreview');
    setupImagePreview('courseImageFile', 'courseImagePreview');
    setupImagePreview('blogImageFile', 'blogImagePreview');
    setupImagePreview('authorImageFile', 'authorImagePreview');
    setupImagePreview('travelImageFile', 'travelImagePreview');
}

function getModalInstance(modalId) {
    const modalEl = document.getElementById(modalId);
    if (!modalEl) {
        console.error(`Modal element with ID "${modalId}" not found.`);
        return null;
    }
    return bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
}

function filterOrders(status, button) {
    // Handle active class for buttons
    const buttons = button.parentElement.querySelectorAll('button');
    buttons.forEach(btn => btn.classList.remove('active'));
    button.classList.add('active');

    const rows = document.querySelectorAll('#ordersTable tr');
    rows.forEach(row => {
        if (status === 'all') {
            row.style.display = '';
        } else {
            const statusBadge = row.querySelector('.badge');
            if (statusBadge && statusBadge.textContent.trim() === status) {
                row.style.display = '';
            } else {
                row.style.display = 'none';
            }
        }
    });
}


function applyProductFilters() {
    const searchTerm = document.getElementById('productSearchInput').value;
    const categoryId = document.getElementById('productCategoryFilter').value;
    loadProducts(searchTerm, categoryId);
}

function applyCourseFilters() {
    const searchTerm = document.getElementById('courseSearchInput').value;
    const categoryId = document.getElementById('courseCategoryFilter').value;
    const companyId = document.getElementById('courseCompanyFilter').value;
    loadCourses(searchTerm, categoryId, companyId);
}

function applyUserFilters() {
    const searchTerm = document.getElementById('userSearchInput').value;
    loadUsers(searchTerm);
}

function applyTravelFilters() {
    const searchTerm = document.getElementById('travelSearchInput').value;
    loadTravelPackages(searchTerm);
}

function applyBlogFilters() {
    const searchTerm = document.getElementById('blogSearchInput').value;
    loadBlogPosts(searchTerm);
}

function setupImagePreview(inputId, previewId) {
    const inputFile = document.getElementById(inputId);
    if (inputFile) {
        inputFile.addEventListener('change', function () {
            const preview = document.getElementById(previewId);
            const file = this.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    preview.src = e.target.result;
                    preview.style.display = 'block';
                }
                reader.readAsDataURL(file);
            }
        });
    }
}

function showDashboardSelection() {
    document.getElementById('dashboardSelection').classList.remove('d-none');
    document.getElementById('ecommerceDashboard').classList.add('d-none');
    document.getElementById('lmsDashboard').classList.add('d-none');
}

async function showEcommerceDashboard() {
    document.getElementById('dashboardSelection').classList.add('d-none');
    document.getElementById('ecommerceDashboard').classList.remove('d-none');
    document.getElementById('lmsDashboard').classList.add('d-none');

    await loadDashboardStats();
    await loadAnalytics();
    await loadProducts();
    await loadOrders();
    await loadCategories();
    await loadShippingZones();
    await loadUsers();
    await loadReviews(); // Load reviews for the e-commerce dash
}

async function showLMSDashboard() {
    document.getElementById('dashboardSelection').classList.add('d-none');
    document.getElementById('ecommerceDashboard').classList.add('d-none');
    document.getElementById('lmsDashboard').classList.remove('d-none');

    await loadDashboardStats();
    await loadAnalytics();
    await loadCourses();
    await loadTravelPackages();
    await loadBlogPosts();
}

async function loadDashboardStats() {
    try {
        const stats = await fetchData('/admin/api/stats');
        document.getElementById('totalProductsStat').textContent = stats.totalProducts;
        document.getElementById('totalOrdersStat').textContent = stats.totalOrders;
        document.getElementById('totalMarketersStat').textContent = stats.totalMarketers;
        document.getElementById('monthlyRevenueStat').textContent = `$${stats.monthlyRevenue.toFixed(2)}`;

        const totalCoursesEl = document.getElementById('totalCoursesStat');
        const totalStudentsEl = document.getElementById('totalStudentsStat');
        if (totalCoursesEl) totalCoursesEl.textContent = stats.totalCourses;
        if (totalStudentsEl) totalStudentsEl.textContent = stats.totalStudents;

    } catch (error) {
        console.error('Failed to load dashboard stats:', error);
    }
}

async function loadProducts(searchTerm = '', categoryId = '') {
    const url = `/admin/api/products?searchTerm=${encodeURIComponent(searchTerm)}&categoryId=${encodeURIComponent(categoryId)}`;
    const products = await fetchData(url);
    const tbody = document.getElementById('productsTable');
    tbody.innerHTML = products.map(p => `
        <tr data-id="${p.id}">
            <td>
                <div class="d-flex align-items-center">
                    <img width="70px" src="${p.imageUrl || '/images/default-placeholder.png'}" alt="${p.name}" class="table-img me-2"/> 
                    <span>${p.name}</span>
                </div>
            </td>
            <td>${p.companyName || 'N/A'}</td>
            <td>${p.categoryName || 'N/A'}</td>
            <td>$${p.price.toFixed(2)}</td>
            <td>${p.stockQuantity}</td>
            <td>
                <button class="btn btn-sm btn-outline-primary edit-btn"><i class="fas fa-edit"></i></button>
                <button class="btn btn-sm btn-outline-danger delete-btn"><i class="fas fa-trash"></i></button>
            </td>
        </tr>`).join('');
    attachCrudEventListeners('#productsTable', showProductModal, deleteProduct);
}


function showProductModal(id = 0) {
    const modalInstance = getModalInstance('productModal');
    if (!modalInstance) return;
    openModal(document.getElementById('productForm'), 'productId', 'productModalLabel', id,
        id === 0 ? 'Add New Product' : 'Edit Product',
        `/api/products/${id}`,
        (data) => {
            document.getElementById('productName').value = data.name;
            document.getElementById('productDescription').value = data.description;
            document.getElementById('productPrice').value = data.price;
            document.getElementById('productSalePrice').value = data.salePrice;
            document.getElementById('productPackageCount').value = data.packageCount;
            document.getElementById('productPackagePriceForMarketer').value = data.packagePriceForMarketer;
            document.getElementById('productStock').value = data.stockQuantity;
            document.getElementById('productCompanyId').value = data.companyId;
            document.getElementById('productCategoryId').value = data.categoryId;

            const preview = document.getElementById('productImagePreview');
            if (data.imageUrl) {
                preview.src = data.imageUrl;
                preview.style.display = 'block';
            } else {
                preview.style.display = 'none';
                preview.src = '#';
            }
        },
        modalInstance);
}


async function saveProduct() {
    const form = document.getElementById('productForm');
    const formData = new FormData(form);
    await saveData('/admin/api/product', formData, 'productModal', loadProducts);
}

async function deleteProduct(id) {
    await deleteData(`/admin/api/product/${id}`, 'product', loadProducts);
}

async function loadCourses(searchTerm = '', categoryId = '', companyId = '') {
    const url = `/admin/api/courses?searchTerm=${encodeURIComponent(searchTerm)}&categoryId=${encodeURIComponent(categoryId)}&companyId=${encodeURIComponent(companyId)}`;
    const courses = await fetchData(url);
    const tbody = document.getElementById('coursesTable');
    tbody.innerHTML = courses.map(c => `
        <tr data-id="${c.id}">
            <td>
                <div class="d-flex align-items-center">
                    <img width="70px" src="${c.imageUrl || '/images/default-placeholder.png'}" alt="${c.name}" class="table-img me-2"/> 
                    <span>${c.name}</span>
                </div>
            </td>
            <td>${c.companyName || 'N/A'}</td>
            <td>${c.categoryName || 'N/A'}</td>
            <td>$${c.price.toFixed(2)}</td>
            <td>${c.instructorName}</td>
            <td>
                <a href="/Admin/EditCourse/${c.id}" class="btn btn-sm btn-outline-info"><i class="fas fa-list-alt"></i></a>
                <button class="btn btn-sm btn-outline-primary edit-btn"><i class="fas fa-edit"></i></button>
                <button class="btn btn-sm btn-outline-danger delete-btn"><i class="fas fa-trash"></i></button>
            </td>
        </tr>`).join('');
    attachCrudEventListeners('#coursesTable', showCourseModal, deleteCourse);
}


function showCourseModal(id = 0) {
    const modalInstance = getModalInstance('courseModal');
    if (!modalInstance) return;
    openModal(document.getElementById('courseForm'), 'courseId', 'courseModalLabel', id,
        id === 0 ? 'Add New Course' : 'Edit Course',
        `/api/courses/${id}`,
        (data) => {
            document.getElementById('courseName').value = data.name;
            document.getElementById('courseDescription').value = data.description;
            document.getElementById('coursePrice').value = data.price;
            document.getElementById('courseSalePrice').value = data.salePrice;
            document.getElementById('courseInstructor').value = data.instructorName;
            document.getElementById('courseCompanyId').value = data.companyId;
            document.getElementById('courseCategoryId').value = data.categoryId;

            const preview = document.getElementById('courseImagePreview');
            if (data.imageUrl) {
                preview.src = data.imageUrl;
                preview.style.display = 'block';
            } else {
                preview.style.display = 'none';
                preview.src = '#';
            }
        },
        modalInstance);
}

async function saveCourse() {
    const form = document.getElementById('courseForm');
    const isNewCourse = document.getElementById('courseId').value == '0';
    const formData = new FormData(form);

    try {
        const response = await fetch('/admin/api/course', {
            method: 'POST',
            body: formData
        });

        if (!response.ok) {
            const errorData = await response.json();
            const errorMessages = errorData.errors ? (Array.isArray(errorData.errors) ? errorData.errors.join('\n') : Object.values(errorData.errors).flat().join('\n')) : (errorData.message || 'Save operation failed');
            throw new Error(errorMessages);
        }

        const result = await response.json();
        const modalInstance = getModalInstance('courseModal');
        if (modalInstance) {
            modalInstance.hide();
        }

        if (isNewCourse && result.newCourseId) {
            window.location.href = `/Admin/EditCourse/${result.newCourseId}`;
        } else {
            await loadCourses();
        }
    } catch (error) {
        console.error('Save course failed:', error);
        Swal.fire('Error!', `Could not save the course. ${error.message}`, 'error');
    }
}


async function deleteCourse(id) {
    await deleteData(`/admin/api/course/${id}`, 'course', loadCourses);
}

async function loadCategories() {
    try {
        const categories = await fetchData('/admin/api/categories');
        const tbody = document.getElementById('categoriesTable');
        tbody.innerHTML = categories.map(c => `
        <tr data-id="${c.id}">
            <td>${c.id}</td>
            <td>${c.name}</td>
            <td>
                <button class="btn btn-sm btn-outline-primary edit-btn"><i class="fas fa-edit"></i></button>
                <button class="btn btn-sm btn-outline-danger delete-btn"><i class="fas fa-trash"></i></button>
            </td>
        </tr>`).join('');
        attachCrudEventListeners('#categoriesTable', showCategoryModal, deleteCategory);
    } catch (error) {
        console.error('Failed to load categories:', error);
    }
}

function showCategoryModal(id = 0) {
    const modalInstance = getModalInstance('categoryModal');
    if (!modalInstance) return;
    openModal(document.getElementById('categoryForm'), 'categoryId', 'categoryModalLabel', id,
        id === 0 ? 'Add New Category' : 'Edit Category',
        `/admin/api/category/${id}`,
        (data) => {
            document.getElementById('categoryName').value = data.name;
        },
        modalInstance);
}

async function saveCategory() {
    const data = {
        Id: parseInt(document.getElementById('categoryId').value) || 0,
        Name: document.getElementById('categoryName').value,
    };
    await saveData('/admin/api/category', data, 'categoryModal', loadCategories);
}

async function deleteCategory(id) {
    await deleteData(`/admin/api/category/${id}`, 'category', loadCategories);
}

async function loadShippingZones() {
    try {
        const zones = await fetchData('/admin/api/shippingzones');
        const tbody = document.getElementById('shippingZonesTable');
        tbody.innerHTML = zones.map(z => `
            <tr data-id="${z.id}">
                <td>${z.zoneName}</td>
                <td>${z.city}</td>
                <td>$${z.shippingCost.toFixed(2)}</td>
                <td>
                    <button class="btn btn-sm btn-outline-primary edit-btn"><i class="fas fa-edit"></i></button>
                    <button class="btn btn-sm btn-outline-danger delete-btn"><i class="fas fa-trash"></i></button>
                </td>
            </tr>`).join('');
        attachCrudEventListeners('#shippingZonesTable', showShippingZoneModal, deleteShippingZone);
    } catch (error) {
        console.error('Failed to load shipping zones:', error);
    }
}

function showShippingZoneModal(id = 0) {
    const modalInstance = getModalInstance('shippingZoneModal');
    if (!modalInstance) return;
    openModal(document.getElementById('shippingZoneForm'), 'shippingZoneId', 'shippingZoneModalLabel', id,
        id === 0 ? 'Add New Shipping Zone' : 'Edit Shipping Zone',
        `/admin/api/shippingzone/${id}`,
        (data) => {
            document.getElementById('shippingZoneName').value = data.zoneName;
            document.getElementById('shippingCity').value = data.city;
            document.getElementById('shippingCost').value = data.shippingCost;
        },
        modalInstance);
}

async function saveShippingZone() {
    const data = {
        Id: parseInt(document.getElementById('shippingZoneId').value) || 0,
        ZoneName: document.getElementById('shippingZoneName').value,
        City: document.getElementById('shippingCity').value,
        ShippingCost: parseFloat(document.getElementById('shippingCost').value)
    };
    await saveData('/admin/api/shippingzone', data, 'shippingZoneModal', loadShippingZones);
}


async function deleteShippingZone(id) {
    await deleteData(`/admin/api/shippingzone/${id}`, 'shipping zone', loadShippingZones);
}

async function loadTravelPackages(searchTerm = '') {
    const url = `/admin/api/travelpackages?searchTerm=${encodeURIComponent(searchTerm)}`;
    const packages = await fetchData(url);
    const tbody = document.getElementById('travelPackagesTable');
    tbody.innerHTML = packages.map(p => `
        <tr data-id="${p.id}">
            <td>${p.name}</td>
            <td>${p.destination}</td>
            <td>$${p.price.toFixed(2)}</td>
            <td>${p.durationDays}</td>
            <td>
                <button class="btn btn-sm btn-outline-primary edit-btn"><i class="fas fa-edit"></i></button>
                <button class="btn btn-sm btn-outline-danger delete-btn"><i class="fas fa-trash"></i></button>
            </td>
        </tr>`).join('');
    attachCrudEventListeners('#travelPackagesTable', showTravelPackageModal, deleteTravelPackage);
}


function showTravelPackageModal(id = 0) {
    const modalInstance = getModalInstance('travelPackageModal');
    if (!modalInstance) return;
    openModal(document.getElementById('travelPackageForm'), 'travelPackageId', 'travelPackageModalLabel', id,
        id === 0 ? 'Add New Travel Package' : 'Edit Travel Package',
        `/admin/api/travelpackage/${id}`,
        (pkg) => {
            document.getElementById('travelPackageName').value = pkg.name;
            document.getElementById('travelDestination').value = pkg.destination;
            document.getElementById('travelDescription').value = pkg.description;
            document.getElementById('travelPrice').value = pkg.price;
            document.getElementById('travelDuration').value = pkg.durationDays;
            document.getElementById('travelInclusions').value = pkg.inclusions;
            const preview = document.getElementById('travelImagePreview');
            if (pkg.imageUrl) {
                preview.src = pkg.imageUrl;
                preview.style.display = 'block';
            } else {
                preview.style.display = 'none';
                preview.src = '#';
            }
        },
        modalInstance);
}

async function saveTravelPackage() {
    const form = document.getElementById('travelPackageForm');
    const formData = new FormData(form);
    await saveData('/admin/api/travelpackage', formData, 'travelPackageModal', loadTravelPackages);
}


async function deleteTravelPackage(id) {
    await deleteData(`/admin/api/travelpackage/${id}`, 'travel package', loadTravelPackages);
}

async function loadBlogPosts(searchTerm = '') {
    const url = `/admin/api/blogposts?searchTerm=${encodeURIComponent(searchTerm)}`;
    const posts = await fetchData(url);
    const tbody = document.getElementById('blogPostsTable');
    if (!tbody) return;
    tbody.innerHTML = posts.map(post => `
        <tr data-id="${post.id}">
            <td>${post.title}</td>
            <td>${post.authorName}</td>
            <td>${new Date(post.publishDate).toLocaleDateString()}</td>
            <td>
                <button class="btn btn-sm btn-outline-primary edit-btn"><i class="fas fa-edit"></i></button>
                <button class="btn btn-sm btn-outline-danger delete-btn"><i class="fas fa-trash"></i></button>
            </td>
        </tr>`).join('');
    attachCrudEventListeners('#blogPostsTable', showBlogModal, deleteBlogPost);
}

function showBlogModal(id = 0) {
    const modalInstance = getModalInstance('blogModal');
    if (!modalInstance) return;
    openModal(document.getElementById('blogForm'), 'blogPostId', 'blogModalLabel', id,
        id === 0 ? 'Add New Blog Post' : 'Edit Blog Post',
        `/admin/api/blogpost/${id}`,
        (post) => {
            document.getElementById('blogTitle').value = post.title;
            document.getElementById('blogSubtitle').value = post.subtitle;
            document.getElementById('blogContent').value = post.content;
            document.getElementById('blogAuthorName').value = post.authorName;
            document.getElementById('blogAuthorTitle').value = post.authorTitle;

            const imgPreview = document.getElementById('blogImagePreview');
            if (post.imageUrl) {
                imgPreview.src = post.imageUrl;
                imgPreview.style.display = 'block';
            } else {
                imgPreview.style.display = 'none';
                imgPreview.src = '#';
            }

            const authorImgPreview = document.getElementById('authorImagePreview');
            if (post.authorImageUrl) {
                authorImgPreview.src = post.authorImageUrl;
                authorImgPreview.style.display = 'block';
            } else {
                authorImgPreview.style.display = 'none';
                authorImgPreview.src = '#';
            }
        },
        modalInstance);
}

async function saveBlogPost() {
    const form = document.getElementById('blogForm');
    const formData = new FormData(form);
    await saveData('/admin/api/blogpost', formData, 'blogModal', loadBlogPosts);
}

async function deleteBlogPost(id) {
    await deleteData(`/admin/api/blogpost/${id}`, 'blog post', loadBlogPosts);
}

async function loadUsers(searchTerm = '') {
    const url = `/admin/api/users?searchTerm=${encodeURIComponent(searchTerm)}`;
    try {
        const users = await fetchData(url);
        const tbody = document.getElementById('usersTable');
        if (!tbody) return;
        tbody.innerHTML = users.map(user => `
            <tr data-id="${user.id}">
                <td>${user.firstName} ${user.lastName}</td>
                <td>${user.email}</td>
                <td>${user.roles.map(r => `<span class="badge bg-secondary me-1">${r}</span>`).join(' ')}</td>
                <td>
                    <button class="btn btn-sm btn-outline-primary edit-btn"><i class="fas fa-edit"></i></button>
                    <button class="btn btn-sm btn-outline-danger delete-btn"><i class="fas fa-trash"></i></button>
                </td>
            </tr>`).join('');
        attachCrudEventListeners('#usersTable', showUserModal, deleteUser);
    } catch (error) {
        console.error('Failed to load users:', error);
    }
}


async function showUserModal(id = '') {
    const modalInstance = getModalInstance('userModal');
    if (!modalInstance) return;
    const form = document.getElementById('userForm');
    form.reset();
    document.getElementById('userId').value = id;
    document.getElementById('userModalLabel').textContent = id ? 'Edit User' : 'Add New User';

    document.querySelector('.password-fields').style.display = id ? 'none' : 'flex';
    document.getElementById('userPassword').required = !id;
    document.getElementById('userConfirmPassword').required = !id;

    document.querySelectorAll('#roles-checkbox-container input').forEach(cb => cb.checked = false);

    if (id) {
        try {
            const data = await fetchData(`/admin/api/user/${id}`);
            document.getElementById('userFirstName').value = data.firstName;
            document.getElementById('userLastName').value = data.lastName;
            document.getElementById('userEmail').value = data.email;
            document.getElementById('userType').value = data.userType;
            document.getElementById('userCompanyId').value = data.companyId || '';

            data.selectedRoles.forEach(roleName => {
                const checkbox = document.querySelector(`#roles-checkbox-container input[value="${roleName}"]`);
                if (checkbox) checkbox.checked = true;
            });

        } catch (error) {
            console.error(`Failed to fetch data for user ID ${id}:`, error);
        }
    }
    modalInstance.show();
}

async function saveUser() {
    const userId = document.getElementById('userId').value;
    const selectedRoles = Array.from(document.querySelectorAll('input[name="roles"]:checked')).map(cb => cb.value);

    const data = {
        Id: userId,
        FirstName: document.getElementById('userFirstName').value,
        LastName: document.getElementById('userLastName').value,
        Email: document.getElementById('userEmail').value,
        Password: document.getElementById('userPassword').value,
        ConfirmPassword: document.getElementById('userConfirmPassword').value,
        UserType: document.getElementById('userType').value,
        CompanyId: document.getElementById('userCompanyId').value ? parseInt(document.getElementById('userCompanyId').value) : null,
        SelectedRoles: selectedRoles,
    };

    if (!userId && data.Password !== data.ConfirmPassword) {
        alert("Passwords do not match.");
        return;
    }

    await saveData('/admin/api/user', data, 'userModal', loadUsers);
}

async function deleteUser(id) {
    await deleteData(`/admin/api/user/${id}`, 'user', loadUsers);
}

async function loadOrders() {
    const orders = await fetchData('/admin/api/orders');
    const tbody = document.getElementById('ordersTable');
    if (!tbody) return;
    tbody.innerHTML = orders.map(order => `
        <tr data-id="${order.id}">
            <td>${order.id}</td>
            <td>${order.customerName}</td>
            <td>${new Date(order.orderDate).toLocaleDateString()}</td>
            <td>$${order.orderTotal.toFixed(2)}</td>
            <td><span class="badge ${getStatusClass(order.status)}">${order.status}</span></td>
            <td>
                <button class="btn btn-sm btn-outline-primary view-btn"><i class="fas fa-eye"></i></button>
            </td>
        </tr>`).join('');

    const table = document.querySelector('#ordersTable');
    table.addEventListener('click', function (e) {
        const target = e.target.closest('button.view-btn');
        if (!target) return;

        const row = target.closest('tr');
        const id = row.dataset.id;
        viewOrderDetails(id);
    });
}

function getStatusClass(status) {
    switch (status.toLowerCase()) {
        case 'completed': return 'bg-success';
        case 'shipped': return 'bg-info';
        case 'processing': return 'bg-primary';
        case 'pending': return 'bg-warning text-dark';
        case 'pending approval': return 'bg-warning text-dark';
        case 'cancelled': return 'bg-danger';
        default: return 'bg-secondary';
    }
}

async function viewOrderDetails(id) {
    const modalInstance = getModalInstance('orderDetailModal');
    if (!modalInstance) return;
    try {
        const order = await fetchData(`/admin/api/order/${id}`);
        const invoiceContent = document.getElementById('invoice-content');

        const itemsHtml = order.items.map(item => `
            <tr>
                <td>${item.itemName}</td>
                <td>${item.quantity}</td>
                <td>$${item.unitPrice.toFixed(2)}</td>
                <td class="text-end">$${(item.quantity * item.unitPrice).toFixed(2)}</td>
            </tr>
        `).join('');

        invoiceContent.innerHTML = `
            <div class="p-4 border">
                <div class="row mb-4">
                    <div class="col-md-6">
                        <h4>Invoice #${order.id}</h4>
                        <p class="mb-1"><strong>Date:</strong> ${new Date(order.orderDate).toLocaleDateString()}</p>
                        <p><strong>Status:</strong> <span id="modalOrderStatus" class="badge ${getStatusClass(order.status)}">${order.status}</span></p>
                    </div>
                    <div class="col-md-6 text-md-end">
                        <h5>Mithaqq Inc.</h5>
                        <p class="mb-0">123 Business Rd.</p>
                        <p>Business City, 12345</p>
                    </div>
                </div>
                <div class="row mb-4">
                    <div class="col-md-6">
                        <h6>Bill To:</h6>
                        <p class="mb-0">${order.customerName}</p>
                        <p class="mb-0">${order.customerEmail}</p>
                        <p>${order.shippingAddress}</p>
                    </div>
                </div>
                <table class="table table-bordered">
                    <thead class="table-light">
                        <tr>
                            <th>Item</th>
                            <th>Quantity</th>
                            <th>Unit Price</th>
                            <th class="text-end">Total</th>
                        </tr>
                    </thead>
                    <tbody>${itemsHtml}</tbody>
                    <tfoot>
                        <tr>
                            <td colspan="3" class="text-end border-0"><strong>Subtotal</strong></td>
                            <td class="text-end border-0">$${order.orderTotal.toFixed(2)}</td>
                        </tr>
                        <tr>
                            <td colspan="3" class="text-end border-0"><strong>Tax (0%)</strong></td>
                            <td class="text-end border-0">$0.00</td>
                        </tr>
                        <tr>
                            <td colspan="3" class="text-end fw-bold"><strong>TOTAL</strong></td>
                            <td class="text-end fw-bold">$${order.orderTotal.toFixed(2)}</td>
                        </tr>
                    </tfoot>
                </table>
            </div>`;

        document.getElementById('orderStatusDropdown').value = order.status;
        document.getElementById('updateStatusBtn').dataset.orderId = order.id;

        modalInstance.show();
    } catch (error) {
        console.error('Failed to view order details:', error);
    }
}

async function updateOrderStatus() {
    const orderId = document.getElementById('updateStatusBtn').dataset.orderId;
    const newStatus = document.getElementById('orderStatusDropdown').value;

    try {
        const response = await fetch('/admin/api/order/updatestatus', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ orderId: parseInt(orderId), status: newStatus })
        });

        if (!response.ok) {
            throw new Error('Failed to update status');
        }

        const result = await response.json();
        if (result.success) {
            const statusBadge = document.getElementById('modalOrderStatus');
            statusBadge.textContent = newStatus;
            statusBadge.className = `badge ${getStatusClass(newStatus)}`;
            await loadOrders();
        }
    } catch (error) {
        console.error('Update status failed:', error);
        alert('Error updating status.');
    }
}

function printInvoice() {
    const invoiceContentEl = document.getElementById('invoice-content');
    if (!invoiceContentEl) return;

    // Extract data from the modal's content
    const orderId = invoiceContentEl.querySelector('h4')?.textContent.replace('Invoice #', '') || 'N/A';
    const orderDate = invoiceContentEl.querySelector('p:nth-of-type(1)')?.textContent.replace('Date:', '').trim() || 'N/A';
    const status = invoiceContentEl.querySelector('#modalOrderStatus')?.textContent.trim() || 'N/A';
    const customerName = invoiceContentEl.querySelector('h6:nth-of-type(1) + p')?.textContent.trim() || 'N/A';
    const customerEmail = invoiceContentEl.querySelector('h6:nth-of-type(1) + p + p')?.textContent.trim() || 'N/A';
    const shippingAddress = invoiceContentEl.querySelector('h6:nth-of-type(1) + p + p + p')?.textContent.trim() || 'N/A';
    const total = invoiceContentEl.querySelector('tfoot tr:last-child td:last-child')?.textContent.trim() || '$0.00';

    let itemsHtml = '';
    const itemRows = invoiceContentEl.querySelectorAll('tbody tr');
    itemRows.forEach(row => {
        const cells = row.querySelectorAll('td');
        itemsHtml += `
            <tr>
                <td>${cells[0].textContent}</td>
                <td>${cells[1].textContent}</td>
                <td>${cells[2].textContent}</td>
                <td class="text-right">${cells[3].textContent}</td>
            </tr>
        `;
    });

    const printWindow = window.open('', '', 'height=800,width=800');
    printWindow.document.write('<html><head><title>Invoice</title>');
    printWindow.document.write(`
        <style>
            body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 20px; background-color: #f9f9f9; color: #333; }
            .invoice-box { max-width: 800px; margin: auto; padding: 30px; border: 1px solid #eee; background: #fff; box-shadow: 0 0 10px rgba(0, 0, 0, 0.15); }
            .header { display: flex; justify-content: space-between; align-items: flex-start; padding-bottom: 20px; border-bottom: 2px solid #eee; }
            .header .company-details { text-align: left; }
            .header .invoice-details { text-align: right; }
            .header h1 { margin: 0; font-size: 24px; color: #1e3a8a; }
            .header p { margin: 2px 0; font-size: 14px; color: #555; }
            .addresses { display: flex; justify-content: space-between; margin-top: 40px; }
            .addresses h5 { font-size: 16px; margin-bottom: 10px; color: #1e3a8a; border-bottom: 1px solid #eee; padding-bottom: 5px;}
            .table-items { width: 100%; line-height: inherit; text-align: left; border-collapse: collapse; margin-top: 40px; }
            .table-items th { background: #1e3a8a; color: #fff; padding: 10px; text-align: left; }
            .table-items td { padding: 10px; border-bottom: 1px solid #eee; }
            .table-items tr:nth-child(even) { background: #f9f9f9; }
            .totals { margin-top: 30px; text-align: right; }
            .totals table { width: 40%; margin-left: auto; }
            .totals td { padding: 8px; }
            .totals .total td { font-weight: bold; font-size: 1.2em; color: #1e3a8a; border-top: 2px solid #333; }
            .footer { text-align: center; margin-top: 40px; font-size: 12px; color: #777; border-top: 1px solid #eee; padding-top: 15px; }
            .text-right { text-align: right; }
        </style>
    `);
    printWindow.document.write('</head><body>');
    printWindow.document.write(`
        <div class="invoice-box">
            <div class="header">
                <div class="company-details">
                    <img src="/images/logo-2.jpg" style="width:100%; max-width:150px;">
                    <p>Mithaqq Inc.</p>
                    <p>123 Business Road</p>
                    <p>Business City, 12345</p>
                </div>
                <div class="invoice-details">
                    <h1>INVOICE</h1>
                    <p><strong>Invoice #:</strong> ${orderId}</p>
                    <p><strong>Date:</strong> ${orderDate}</p>
                    <p><strong>Status:</strong> ${status}</p>
                </div>
            </div>
            <div class="addresses">
                <div>
                    <h5>BILL TO</h5>
                    <p>${customerName}</p>
                    <p>${customerEmail}</p>
                    <p>${shippingAddress}</p>
                </div>
            </div>
            <table class="table-items">
                <thead>
                    <tr>
                        <th>Item</th>
                        <th>Quantity</th>
                        <th class="text-right">Unit Price</th>
                        <th class="text-right">Total</th>
                    </tr>
                </thead>
                <tbody>
                    ${itemsHtml}
                </tbody>
            </table>
            <div class="totals">
                <table>
                    <tr class="total">
                        <td><strong>Total:</strong></td>
                        <td class="text-right"><strong>${total}</strong></td>
                    </tr>
                </table>
            </div>
            <div class="footer">
                Thank you for your business!
            </div>
        </div>
    `);
    printWindow.document.write('</body></html>');
    printWindow.document.close();
    printWindow.focus();
    setTimeout(() => { printWindow.print(); }, 500);
}

async function loadAnalytics() {
    try {
        const data = await fetchData('/admin/api/analytics');
        console.log("Analytics Data from server:", data); // Log the data to the console

        // Sales Chart (Line)
        const salesCtx = document.getElementById('salesChart');
        if (salesCtx) {
            if (salesChartInstance) {
                salesChartInstance.destroy();
            }
            salesChartInstance = new Chart(salesCtx, {
                type: 'line',
                data: {
                    labels: data.monthlySales.map(s => s.label),
                    datasets: [{
                        label: 'Sales',
                        data: data.monthlySales.map(s => s.value),
                        borderColor: '#16A085',
                        backgroundColor: 'rgba(22, 160, 133, 0.1)',
                        tension: 0.4,
                        fill: true,
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                callback: function (value) {
                                    return '$' + value.toLocaleString();
                                }
                            }
                        }
                    }
                }
            });
        }

        // Products Chart (Doughnut)
        const productsCtx = document.getElementById('productsChart');
        if (productsCtx) {
            if (productsChartInstance) {
                productsChartInstance.destroy();
            }
            productsChartInstance = new Chart(productsCtx, {
                type: 'doughnut',
                data: {
                    labels: data.salesByCategory.map(c => c.label),
                    datasets: [{
                        data: data.salesByCategory.map(c => c.value),
                        backgroundColor: ['#16A085', '#3498DB', '#F39C12', '#E74C3C', '#9B59B6', '#34495E']
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            position: 'bottom',
                        }
                    }
                }
            });
        }

        // Enrollments Chart (Bar)
        const enrollmentsCtx = document.getElementById('enrollmentsChart');
        if (enrollmentsCtx) {
            if (enrollmentsChartInstance) {
                enrollmentsChartInstance.destroy();
            }
            enrollmentsChartInstance = new Chart(enrollmentsCtx, {
                type: 'bar',
                data: {
                    labels: data.monthlyEnrollments.map(e => e.label),
                    datasets: [{
                        label: 'Enrollments',
                        data: data.monthlyEnrollments.map(e => e.value),
                        backgroundColor: '#27AE60'
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                stepSize: 1
                            }
                        }
                    }
                }
            });
        }

        // Completion Chart (Pie)
        const completionCtx = document.getElementById('completionChart');
        if (completionCtx) {
            if (completionChartInstance) {
                completionChartInstance.destroy();
            }
            completionChartInstance = new Chart(completionCtx, {
                type: 'pie',
                data: {
                    labels: data.completionRates.map(c => c.label),
                    datasets: [{
                        data: data.completionRates.map(c => c.value),
                        backgroundColor: ['#27AE60', '#F39C12', '#E74C3C']
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            position: 'bottom',
                        }
                    }
                }
            });
        }

        // Top Products List
        const topProductsList = document.getElementById('topProductsList');
        if (topProductsList) {
            if (data.topProducts.length > 0) {
                topProductsList.innerHTML = data.topProducts.map(p => `
                    <li class="list-group-item d-flex justify-content-between align-items-center">
                        ${p.name}
                        <span class="badge bg-primary rounded-pill">${p.quantity} sold</span>
                    </li>
                `).join('');
            } else {
                topProductsList.innerHTML = '<li class="list-group-item text-muted">No product sales data yet.</li>';
            }
        }

        // Top Courses List
        const topCoursesList = document.getElementById('topCoursesList');
        if (topCoursesList) {
            if (data.topCourses.length > 0) {
                topCoursesList.innerHTML = data.topCourses.map(c => `
                    <li class="list-group-item d-flex justify-content-between align-items-center">
                        ${c.name}
                        <span class="badge bg-success rounded-pill">${c.quantity} sold</span>
                    </li>
                `).join('');
            } else {
                topCoursesList.innerHTML = '<li class="list-group-item text-muted">No course sales data yet.</li>';
            }
        }

    } catch (error) {
        console.error('Failed to load analytics data:', error);
    }
}

// New function to load reviews
async function loadReviews() {
    const reviews = await fetchData('/admin/api/reviews');
    const tbody = document.getElementById('reviewsTable');
    if (!tbody) return;

    tbody.innerHTML = reviews.map(review => `
        <tr data-id="${review.id}">
            <td>${review.itemName}</td>
            <td><span class="badge bg-secondary">${review.itemType}</span></td>
            <td>${review.userName}</td>
            <td class="text-warning">${'<i class="fas fa-star"></i>'.repeat(review.stars)}</td>
            <td>${review.comment || '-'}</td>
            <td>${new Date(review.datePosted).toLocaleDateString()}</td>
            <td>
                <button class="btn btn-sm btn-outline-danger delete-review-btn"><i class="fas fa-trash"></i></button>
            </td>
        </tr>
    `).join('');

    // Attach event listeners for delete buttons
    document.querySelectorAll('.delete-review-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            const reviewId = e.target.closest('tr').dataset.id;
            deleteReview(reviewId);
        });
    });
}

// New function to delete a review
async function deleteReview(id) {
    await deleteData(`/admin/api/review/${id}`, 'review', loadReviews);
}


async function fetchData(url) {
    const response = await fetch(url);
    if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
    return await response.json();
}

async function saveData(url, data, modalId, refreshCallback) {
    const isFormData = data instanceof FormData;
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: isFormData ? {} : { 'Content-Type': 'application/json' },
            body: isFormData ? data : JSON.stringify(data)
        });
        if (!response.ok) {
            const errorData = await response.json();
            const errorMessages = errorData.errors ? (Array.isArray(errorData.errors) ? errorData.errors.join('\n') : Object.values(errorData.errors).flat().join('\n')) : (errorData.message || 'Save operation failed');
            throw new Error(errorMessages);
        }

        const modalInstance = getModalInstance(modalId);
        if (modalInstance) modalInstance.hide();

        await refreshCallback();

        Swal.fire(
            'Saved!',
            'Your changes have been saved.',
            'success'
        );

    } catch (error) {
        console.error('Save failed:', error);
        Swal.fire('Error!', `Could not save changes. ${error.message}`, 'error');
    }
}


async function deleteData(url, itemType, refreshCallback) {
    Swal.fire({
        title: `Are you sure you want to delete this ${itemType}?`,
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then(async (result) => {
        if (result.isConfirmed) {
            try {
                const response = await fetch(url, { method: 'DELETE' });
                if (!response.ok) {
                    throw new Error('Delete operation failed');
                }
                await refreshCallback();
                Swal.fire(
                    'Deleted!',
                    `The ${itemType} has been deleted.`,
                    'success'
                );
            } catch (error) {
                console.error('Delete failed:', error);
                Swal.fire(
                    'Error!',
                    `Could not delete the ${itemType}.`,
                    'error'
                );
            }
        }
    });
}

function attachCrudEventListeners(tableSelector, editCallback, deleteCallback) {
    const table = document.querySelector(tableSelector);
    if (!table) return;
    table.addEventListener('click', function (e) {
        const target = e.target.closest('button');
        if (!target) return;

        const row = target.closest('tr');
        const id = row.dataset.id;

        if (target.classList.contains('edit-btn')) {
            editCallback(id);
        } else if (target.classList.contains('delete-btn')) {
            deleteCallback(id);
        }
    });
}

function openModal(form, idField, titleId, id, title, fetchUrl, populateCallback, modalInstance) {
    if (!modalInstance) return;
    form.reset();
    document.getElementById(idField).value = id;
    document.getElementById(titleId).textContent = title;

    const previews = form.querySelectorAll('img[id$="Preview"]');
    previews.forEach(p => {
        p.src = '#';
        p.style.display = 'none';
    });

    if (id && id != '0') {
        fetchData(fetchUrl)
            .then(data => populateCallback(data))
            .catch(error => console.error(`Failed to fetch data for ID ${id}:`, error));
    }
    modalInstance.show();
}
// New function to load reviews
async function loadReviews() {
    const reviews = await fetchData('/admin/api/reviews');
    const tbody = document.getElementById('reviewsTable');
    if (!tbody) return;

    tbody.innerHTML = reviews.map(review => `
        <tr data-id="${review.id}">
            <td>${review.itemName}</td>
            <td><span class="badge bg-secondary">${review.itemType}</span></td>
            <td>${review.userName}</td>
            <td class="text-warning">${'<i class="fas fa-star"></i>'.repeat(review.stars)}</td>
            <td>${review.comment || '-'}</td>
            <td>${new Date(review.datePosted).toLocaleDateString()}</td>
            <td>
                <button class="btn btn-sm btn-outline-danger delete-review-btn"><i class="fas fa-trash"></i></button>
            </td>
        </tr>
    `).join('');

    // Attach event listeners for delete buttons
    document.querySelectorAll('.delete-review-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            const reviewId = e.target.closest('tr').dataset.id;
            deleteReview(reviewId);
        });
    });
}

// New function to delete a review
async function deleteReview(id) {
    await deleteData(`/admin/api/review/${id}`, 'review', loadReviews);
}


async function fetchData(url) {
    const response = await fetch(url);
    if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
    return await response.json();
}

async function saveData(url, data, modalId, refreshCallback) {
    const isFormData = data instanceof FormData;
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: isFormData ? {} : { 'Content-Type': 'application/json' },
            body: isFormData ? data : JSON.stringify(data)
        });
        if (!response.ok) {
            const errorData = await response.json();
            const errorMessages = errorData.errors ? (Array.isArray(errorData.errors) ? errorData.errors.join('\n') : Object.values(errorData.errors).flat().join('\n')) : (errorData.message || 'Save operation failed');
            throw new Error(errorMessages);
        }

        const modalInstance = getModalInstance(modalId);
        if (modalInstance) modalInstance.hide();

        await refreshCallback();

        Swal.fire(
            'Saved!',
            'Your changes have been saved.',
            'success'
        );

    } catch (error) {
        console.error('Save failed:', error);
        Swal.fire('Error!', `Could not save changes. ${error.message}`, 'error');
    }
}


async function deleteData(url, itemType, refreshCallback) {
    Swal.fire({
        title: `Are you sure you want to delete this ${itemType}?`,
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then(async (result) => {
        if (result.isConfirmed) {
            try {
                const response = await fetch(url, { method: 'DELETE' });
                if (!response.ok) {
                    throw new Error('Delete operation failed');
                }
                await refreshCallback();
                Swal.fire(
                    'Deleted!',
                    `The ${itemType} has been deleted.`,
                    'success'
                );
            } catch (error) {
                console.error('Delete failed:', error);
                Swal.fire(
                    'Error!',
                    `Could not delete the ${itemType}.`,
                    'error'
                );
            }
        }
    });
}

function attachCrudEventListeners(tableSelector, editCallback, deleteCallback) {
    const table = document.querySelector(tableSelector);
    if (!table) return;
    table.addEventListener('click', function (e) {
        const target = e.target.closest('button');
        if (!target) return;

        const row = target.closest('tr');
        const id = row.dataset.id;

        if (target.classList.contains('edit-btn')) {
            editCallback(id);
        } else if (target.classList.contains('delete-btn')) {
            deleteCallback(id);
        }
    });
}

function openModal(form, idField, titleId, id, title, fetchUrl, populateCallback, modalInstance) {
    if (!modalInstance) return;
    form.reset();
    document.getElementById(idField).value = id;
    document.getElementById(titleId).textContent = title;

    const previews = form.querySelectorAll('img[id$="Preview"]');
    previews.forEach(p => {
        p.src = '#';
        p.style.display = 'none';
    });

    if (id && id != '0') {
        fetchData(fetchUrl)
            .then(data => populateCallback(data))
            .catch(error => console.error(`Failed to fetch data for ID ${id}:`, error));
    }
    modalInstance.show();
}

