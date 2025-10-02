let productModal, courseModal, categoryModal, shippingZoneModal, travelPackageModal;

document.addEventListener('DOMContentLoaded', function () {
    // Safely get modal instances
    const pModal = document.getElementById('companyProductModal');
    if (pModal) productModal = new bootstrap.Modal(pModal);

    const cModal = document.getElementById('companyCourseModal');
    if (cModal) courseModal = new bootstrap.Modal(cModal);

    const catModal = document.getElementById('companyCategoryModal');
    if (catModal) categoryModal = new bootstrap.Modal(catModal);

    const shipModal = document.getElementById('companyShippingZoneModal');
    if (shipModal) shippingZoneModal = new bootstrap.Modal(shipModal);

    const travelModalEl = document.getElementById('companyTravelPackageModal');
    if (travelModalEl) travelPackageModal = new bootstrap.Modal(travelModalEl);

    // Initialize event listeners only if the corresponding elements exist
    initializeEventListeners();

    // Initial Load only if we are on the dashboard page
    if (document.getElementById('companyDashboard')) {
        loadDashboardStats();
        loadProducts();
        loadCourses();
        loadCategories();
        loadShippingZones();
        loadTravelPackages();
    }
});

function initializeEventListeners() {
    const addSafeListener = (id, event, handler) => {
        const element = document.getElementById(id);
        if (element) {
            element.addEventListener(event, handler);
        }
    };

    // Modal buttons
    addSafeListener('addCompanyProductBtn', 'click', () => showProductModal());
    addSafeListener('addCompanyCourseBtn', 'click', () => showCourseModal());
    addSafeListener('addCategoryBtn', 'click', () => showCategoryModal());
    addSafeListener('addShippingZoneBtn', 'click', () => showShippingZoneModal());
    addSafeListener('addTravelPackageBtn', 'click', () => showTravelPackageModal());

    // Save buttons
    addSafeListener('saveProductBtn', 'click', saveProduct);
    addSafeListener('saveCourseBtn', 'click', saveCourse);
    addSafeListener('saveCategoryBtn', 'click', saveCategory);
    addSafeListener('saveShippingZoneBtn', 'click', saveShippingZone);
    addSafeListener('saveTravelPackageBtn', 'click', saveTravelPackage);

    // Search Filters
    addSafeListener('productSearchInput', 'input', (e) => loadProducts(e.target.value));
    addSafeListener('courseSearchInput', 'input', (e) => loadCourses(e.target.value));
    addSafeListener('categorySearchInput', 'input', (e) => loadCategories(e.target.value));
    addSafeListener('shippingSearchInput', 'input', (e) => loadShippingZones(e.target.value));
    addSafeListener('travelSearchInput', 'input', (e) => loadTravelPackages(e.target.value));

    // Image Previews
    setupImagePreview('productImageFile', 'productImagePreview');
    setupImagePreview('courseImageFile', 'courseImagePreview');
    setupImagePreview('travelImageFile', 'travelImagePreview');
}

async function loadDashboardStats() {
    try {
        const stats = await fetchData('/companyadmin/api/stats');
        document.getElementById('companyTotalProducts').textContent = stats.totalProducts;
        document.getElementById('companyTotalCourses').textContent = stats.totalCourses;
        document.getElementById('companyTotalOrders').textContent = stats.totalOrders;
    } catch (error) {
        console.error('Failed to load dashboard stats:', error);
    }
}

async function loadProducts(searchTerm = '') {
    const products = await fetchData(`/companyadmin/api/products?searchTerm=${encodeURIComponent(searchTerm)}`);
    const tbody = document.getElementById('companyProductsTable');
    if (!tbody) return;
    tbody.innerHTML = products.map(p => `
        <tr data-id="${p.id}">
            <td><img src="${p.imageUrl || '/images/default-placeholder.png'}" class="table-img me-2"/> ${p.name}</td>
            <td>${p.categoryName || 'N/A'}</td>
            <td>$${p.price.toFixed(2)}</td>
            <td>${p.stockQuantity}</td>
            <td>
                <button class="btn btn-sm btn-outline-primary edit-btn"><i class="fas fa-edit"></i></button>
                <button class="btn btn-sm btn-outline-danger delete-btn"><i class="fas fa-trash"></i></button>
            </td>
        </tr>`).join('');
    attachCrudEventListeners('#companyProductsTable', showProductModal, deleteProduct);
}

async function loadCourses(searchTerm = '') {
    const courses = await fetchData(`/companyadmin/api/courses?searchTerm=${encodeURIComponent(searchTerm)}`);
    const tbody = document.getElementById('companyCoursesTable');
    if (!tbody) return;
    tbody.innerHTML = courses.map(c => `
        <tr data-id="${c.id}">
            <td><img src="${c.imageUrl || '/images/default-placeholder.png'}" class="table-img me-2"/> ${c.name}</td>
            <td>${c.categoryName || 'N/A'}</td>
            <td>$${c.price.toFixed(2)}</td>
            <td>${c.instructorName}</td>
            <td>
                <a href="/CompanyAdmin/EditCourse/${c.id}" class="btn btn-sm btn-outline-info"><i class="fas fa-list-alt"></i></a>
                <button class="btn btn-sm btn-outline-primary edit-btn"><i class="fas fa-edit"></i></button>
                <button class="btn btn-sm btn-outline-danger delete-btn"><i class="fas fa-trash"></i></button>
            </td>
        </tr>`).join('');
    attachCrudEventListeners('#companyCoursesTable', showCourseModal, deleteCourse);
}

async function loadCategories(searchTerm = '') {
    const categories = await fetchData(`/companyadmin/api/categories?searchTerm=${encodeURIComponent(searchTerm)}`);
    const tbody = document.getElementById('companyCategoriesTable');
    if (!tbody) return;
    tbody.innerHTML = categories.map(c => `<tr><td>${c.id}</td><td>${c.name}</td></tr>`).join('');
}

async function loadShippingZones(searchTerm = '') {
    const zones = await fetchData(`/companyadmin/api/shippingzones?searchTerm=${encodeURIComponent(searchTerm)}`);
    const tbody = document.getElementById('companyShippingZonesTable');
    if (!tbody) return;
    tbody.innerHTML = zones.map(z => `
        <tr data-id="${z.id}">
            <td>${z.zoneName}</td>
            <td>${z.city}</td>
            <td>$${z.shippingCost.toFixed(2)}</td>
        </tr>`).join('');
}

async function loadTravelPackages(searchTerm = '') {
    try {
        const packages = await fetchData(`/companyadmin/api/travelpackages?searchTerm=${encodeURIComponent(searchTerm)}`);
        const tbody = document.getElementById('companyTravelPackagesTable');
        if (!tbody) return;
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
        attachCrudEventListeners('#companyTravelPackagesTable', showTravelPackageModal, deleteTravelPackage);
    } catch (error) {
        // This can fail if the user is not an EasyWay admin, which is expected.
        // console.log("Could not load travel packages, this may be expected.");
    }
}

function showProductModal(id = 0) {
    openModal(document.getElementById('companyProductForm'), 'productId', 'productModalLabel', id,
        id == 0 ? 'Add New Product' : 'Edit Product',
        `/companyadmin/api/product/${id}`,
        (data) => {
            document.getElementById('productName').value = data.name;
            document.getElementById('productDescription').value = data.description;
            document.getElementById('productPrice').value = data.price;
            document.getElementById('productSalePrice').value = data.salePrice;
            document.getElementById('productStock').value = data.stockQuantity;
            document.getElementById('productCategoryId').value = data.categoryId;

            const preview = document.getElementById('productImagePreview');
            preview.src = data.imageUrl || '#';
            preview.style.display = data.imageUrl ? 'block' : 'none';
        },
        productModal);
}

async function saveProduct() {
    const form = document.getElementById('companyProductForm');
    const formData = new FormData(form);
    await saveData('/companyadmin/api/product', formData, productModal, loadProducts);
}

async function deleteProduct(id) {
    await deleteData(`/companyadmin/api/product/${id}`, 'product', loadProducts);
}

function showCourseModal(id = 0) {
    openModal(document.getElementById('companyCourseForm'), 'courseId', 'courseModalLabel', id,
        id == 0 ? 'Add New Course' : 'Edit Course',
        `/companyadmin/api/course/${id}`,
        (data) => {
            document.getElementById('courseName').value = data.name;
            document.getElementById('courseDescription').value = data.description;
            document.getElementById('coursePrice').value = data.price;
            document.getElementById('courseSalePrice').value = data.salePrice;
            document.getElementById('courseInstructor').value = data.instructorName;
            document.getElementById('courseCategoryId').value = data.categoryId;

            const preview = document.getElementById('courseImagePreview');
            preview.src = data.imageUrl || '#';
            preview.style.display = data.imageUrl ? 'block' : 'none';
        },
        courseModal);
}

async function saveCourse() {
    const form = document.getElementById('companyCourseForm');
    const isNew = document.getElementById('courseId').value == '0';
    const formData = new FormData(form);
    const result = await saveData('/companyadmin/api/course', formData, courseModal, loadCourses, true);
    if (isNew && result && result.success && result.newCourseId) {
        window.location.href = `/CompanyAdmin/EditCourse/${result.newCourseId}`;
    }
}

async function deleteCourse(id) {
    await deleteData(`/companyadmin/api/course/${id}`, 'course', loadCourses);
}

function showCategoryModal() {
    const form = document.getElementById('companyCategoryForm');
    form.reset();
    categoryModal.show();
}

async function saveCategory() {
    const data = { Name: document.getElementById('categoryName').value };
    await saveData('/companyadmin/api/category', data, categoryModal, loadCategories);
}

function showShippingZoneModal(id = 0) {
    openModal(document.getElementById('companyShippingZoneForm'), 'shippingZoneId', 'shippingZoneModalLabel', id,
        id == 0 ? 'Add Shipping Zone' : 'Edit Shipping Zone',
        `/companyadmin/api/shippingzone/${id}`,
        (data) => {
            document.getElementById('shippingZoneName').value = data.zoneName;
            document.getElementById('shippingCity').value = data.city;
            document.getElementById('shippingCost').value = data.shippingCost;
        },
        shippingZoneModal);
}

async function saveShippingZone() {
    const data = {
        Id: parseInt(document.getElementById('shippingZoneId').value) || 0,
        ZoneName: document.getElementById('shippingZoneName').value,
        City: document.getElementById('shippingCity').value,
        ShippingCost: parseFloat(document.getElementById('shippingCost').value)
    };
    await saveData('/companyadmin/api/shippingzone', data, shippingZoneModal, loadShippingZones);
}

async function deleteShippingZone(id) {
    await deleteData(`/companyadmin/api/shippingzone/${id}`, 'shipping zone', loadShippingZones);
}

function showTravelPackageModal(id = 0) {
    openModal(document.getElementById('companyTravelPackageForm'), 'travelPackageId', 'travelPackageModalLabel', id,
        id == 0 ? 'Add Travel Package' : 'Edit Travel Package',
        `/companyadmin/api/travelpackage/${id}`,
        (data) => {
            document.getElementById('travelPackageName').value = data.name;
            document.getElementById('travelDestination').value = data.destination;
            document.getElementById('travelDescription').value = data.description;
            document.getElementById('travelPrice').value = data.price;
            document.getElementById('travelDuration').value = data.durationDays;
            document.getElementById('travelInclusions').value = data.inclusions;
            const preview = document.getElementById('travelImagePreview');
            preview.src = data.imageUrl || '#';
            preview.style.display = data.imageUrl ? 'block' : 'none';
        },
        travelPackageModal);
}

async function saveTravelPackage() {
    const form = document.getElementById('companyTravelPackageForm');
    const formData = new FormData(form);
    await saveData('/companyadmin/api/travelpackage', formData, travelPackageModal, loadTravelPackages);
}

async function deleteTravelPackage(id) {
    await deleteData(`/companyadmin/api/travelpackage/${id}`, 'travel package', loadTravelPackages);
}


async function fetchData(url) {
    try {
        const response = await fetch(url);
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`HTTP error! status: ${response.status} - ${errorText}`);
        }
        return await response.json();
    } catch (error) {
        console.error('Fetch error:', error);
        throw error;
    }
}

async function saveData(url, data, modalInstance, refreshCallback, returnJson = false) {
    const isFormData = data instanceof FormData;
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: isFormData ? {} : { 'Content-Type': 'application/json' },
            body: isFormData ? data : JSON.stringify(data)
        });
        if (!response.ok) {
            let errorData;
            try { errorData = await response.json(); } catch (e) { throw new Error(await response.text()); }
            const errorMessages = errorData.errors ? Object.values(errorData.errors).flat().join('\n') : (errorData.message || 'Save operation failed');
            throw new Error(errorMessages);
        }
        modalInstance.hide();
        await refreshCallback();
        if (returnJson) return await response.json();
        return { success: true };
    } catch (error) {
        console.error('Save failed:', error);
        Swal.fire('Error!', `Could not save changes. ${error.message}`, 'error');
        return { success: false };
    }
}


async function deleteData(url, itemType, refreshCallback) {
    const result = await Swal.fire({
        title: `Are you sure you want to delete this ${itemType}?`,
        text: "You won't be able to revert this!",
        icon: 'warning', showCancelButton: true, confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33', confirmButtonText: 'Yes, delete it!'
    });

    if (result.isConfirmed) {
        try {
            const response = await fetch(url, { method: 'DELETE' });
            if (!response.ok) throw new Error('Delete operation failed');
            await refreshCallback();
            Swal.fire('Deleted!', `The ${itemType} has been deleted.`, 'success');
        } catch (error) {
            console.error('Delete failed:', error);
            Swal.fire('Error!', `Could not delete the ${itemType}.`, 'error');
        }
    }
}

function attachCrudEventListeners(tableSelector, editCallback, deleteCallback) {
    const table = document.querySelector(tableSelector);
    if (!table) return;
    table.addEventListener('click', function (e) {
        const link = e.target.closest('a');
        if (link && link.href.includes('EditCourse')) return;

        const target = e.target.closest('button');
        if (!target) return;

        e.preventDefault();

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
    if (!form || !modalInstance) return;
    form.reset();
    document.getElementById(idField).value = id;
    document.getElementById(titleId).textContent = title;

    const previews = form.querySelectorAll('img[id$="Preview"]');
    previews.forEach(p => { p.src = '#'; p.style.display = 'none'; });

    const fileInputs = form.querySelectorAll('input[type="file"]');
    fileInputs.forEach(input => input.value = '');

    if (id && id != '0') {
        fetchData(fetchUrl)
            .then(data => populateCallback(data))
            .catch(error => console.error(`Failed to fetch data for ID ${id}:`, error));
    }
    modalInstance.show();
}

function setupImagePreview(inputId, previewId) {
    const inputFile = document.getElementById(inputId);
    if (inputFile) {
        inputFile.addEventListener('change', function () {
            const preview = document.getElementById(previewId);
            if (this.files && this.files[0]) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    preview.src = e.target.result;
                    preview.style.display = 'block';
                }
                reader.readAsDataURL(this.files[0]);
            }
        });
    }
}

