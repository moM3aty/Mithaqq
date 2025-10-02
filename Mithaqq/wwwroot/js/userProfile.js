document.addEventListener('DOMContentLoaded', function() {


    initializeProfile();
    loadUserData();
});

function initializeProfile() {
  

    loadOrders();
    loadCourses();

    // Form handlers
    document.getElementById('passwordForm').addEventListener('submit', handlePasswordChange);
    document.getElementById('editProfileForm').addEventListener('submit', handleProfileUpdate);
}

const userOrders = [
    {
        id: 'ORD-001',
        date: '2025-01-15',
        items: [
            { name: 'Digital Marketing Package', price: 299, quantity: 1 }
        ],
        total: 299,
        status: 'Completed',
        paymentMethod: 'Credit Card',
        shippingAddress: '123 Main St, Dubai, UAE'
    },
    {
        id: 'ORD-002',
        date: '2025-01-10',
        items: [
            { name: 'Python Programming Course', price: 399, quantity: 1 }
        ],
        total: 399,
        status: 'Processing',
        paymentMethod: 'PayPal',
        shippingAddress: 'Digital Delivery'
    },
    {
        id: 'ORD-003',
        date: '2025-01-05',
        items: [
            { name: 'Travel Package Dubai', price: 799, quantity: 1 },
            { name: 'Travel Insurance', price: 50, quantity: 1 }
        ],
        total: 849,
        status: 'Completed',
        paymentMethod: 'Cash on Delivery',
        shippingAddress: '456 Business Bay, Dubai, UAE'
    }
];

const userCourses = [
    {
        id: 1,
        name: 'Digital Marketing Mastery',
        instructor: 'Sarah Johnson',
        progress: 75,
        totalLessons: 24,
        completedLessons: 18,
        enrollDate: '2024-12-01',
        lastAccessed: '2025-01-14',
        certificate: false,
        image: 'https://images.pexels.com/photos/196644/pexels-photo-196644.jpeg?auto=compress&cs=tinysrgb&w=400'
    },
    {
        id: 2,
        name: 'Python Programming Bootcamp',
        instructor: 'David Rodriguez',
        progress: 45,
        totalLessons: 36,
        completedLessons: 16,
        enrollDate: '2025-01-10',
        lastAccessed: '2025-01-15',
        certificate: false,
        image: 'https://images.pexels.com/photos/574071/pexels-photo-574071.jpeg?auto=compress&cs=tinysrgb&w=400'
    },
    {
        id: 3,
        name: 'Project Management Professional',
        instructor: 'Michael Chen',
        progress: 100,
        totalLessons: 18,
        completedLessons: 18,
        enrollDate: '2024-11-15',
        lastAccessed: '2024-12-20',
        certificate: true,
        image: 'https://images.pexels.com/photos/3184291/pexels-photo-3184291.jpeg?auto=compress&cs=tinysrgb&w=400'
    }
];





function loadOrders() {
    const container = document.getElementById('ordersContainer');
    
    if (userOrders.length === 0) {
        container.innerHTML = '<p class="text-muted text-center">No orders found.</p>';
        return;
    }

    container?.innerHTML = userOrders.map(order => `
        <div class="order-card mb-3">
            <div class="card">
                <div class="card-body">
                    <div class="row g-4 align-items-center">
                        <div class="col-md-3">
                            <h6 class="mb-1">${order.id}</h6>
                            <small class="text-muted">${order.date}</small>
                        </div>
                        <div class="col-md-4">
                            <div class="order-items">
                                ${order.items.map(item => `
                                    <div class="small">${item.name} x${item.quantity}</div>
                                `).join('')}
                            </div>
                        </div>
                        <div class="col-md-2">
                            <div class="fw-bold text-success">$${order.total}</div>
                        </div>
                        <div class="col-md-2">
                            <span class="badge badge-custome">
                                ${order.status}
                            </span>
                        </div>
                        <div class="col-md-1">
                            <button class="btn btn-outline-primary btn-sm" onclick="showOrderDetails('${order.id}')">
                                <i class="fas fa-eye"></i>
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `).join('');
}

function loadCourses() {
    const container = document.getElementById('coursesContainer');
    
    if (userCourses.length === 0) {
        container.innerHTML = '<div class="col-12"><p class="text-muted text-center">No courses enrolled.</p></div>';
        return;
    }

    container.innerHTML = userCourses.map(course => `
        <div class="col-md-6 mb-4">
            <div class="card h-100">
                <img src="${course.image}" class="card-img-top" alt="${course.name}" style="height: 200px; object-fit: cover;">
                <div class="card-body">
                    <h6 class="card-title">${course.name}</h6>
                    <p class="card-text text-muted small">Instructor: ${course.instructor}</p>
                    
                    <div class="progress mb-2" style="height: 8px;">
                        <div class="progress-bar bg-success" style="width: ${course.progress}%"></div>
                    </div>
                    <div class="d-flex justify-content-between align-items-center mb-2">
                        <small class="text-muted">${course.progress}% Complete</small>
                        <small class="text-muted">${course.completedLessons}/${course.totalLessons} lessons</small>
                    </div>
                    
                    ${course.certificate ? 
                        '<div class="alert alert-success py-2 mb-2"><i class="fas fa-certificate me-2"></i>Certificate Earned!</div>' : 
                        ''
                    }
                    
                    <div class="d-flex justify-content-between align-items-center">
                        <small class="text-muted">Last accessed: ${course.lastAccessed}</small>
                        <button class="btn btn-accent btn-sm" onclick="continueCourse(${course.id})">
                            <i class="fas fa-play me-1"></i>Continue
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `).join('');
}


function showOrderDetails(orderId) {
    const order = userOrders.find(o => o.id === orderId);
    if (!order) return;

    const modal = document.getElementById('orderDetailsModal');
    const body = document.getElementById('orderDetailsBody');

    body.innerHTML = `
        <div class="row g-4">
            <div class="col-md-6">
                <h6>Order Information</h6>
                <table class="table table-sm d-flex flex-column gap-4">
                    <tr class="mb-2 p-2 d-block"><td><strong>Order ID:</strong></td><td>${order.id}</td></tr>
                    <tr class="mb-2 p-2 d-block"><td><strong>Date:</strong></td><td>${order.date}</td></tr>
                    <tr class="mb-2 p-2 d-block"><td><strong>Status:</strong></td><td><span class="badge badge-custome">${order.status}</span></td></tr>
                    <tr class="mb-2 p-2 d-block"><td><strong>Payment:</strong></td><td>${order.paymentMethod}</td></tr>
                </table>
            </div>
            <div class="col-md-6">
                <h6>Shipping Address</h6>
                <p>${order.shippingAddress}</p>
            </div>
        </div>
        <hr>
        <h6>Order Items</h6>
        <div class="table-responsive">
            <table class="table">
                <thead>
                    <tr>
                        <th>Item</th>
                        <th>Quantity</th>
                        <th>Price</th>
                        <th>Total</th>
                    </tr>
                </thead>
                <tbody>
                    ${order.items.map(item => `
                        <tr>
                            <td>${item.name}</td>
                            <td>${item.quantity}</td>
                            <td>$${item.price}</td>
                            <td>$${item.price * item.quantity}</td>
                        </tr>
                    `).join('')}
                </tbody>
                <tfoot>
                    <tr>
                        <th colspan="3">Total</th>
                        <th>$${order.total}</th>
                    </tr>
                </tfoot>
            </table>
        </div>
    `;

    const bootstrapModal = new bootstrap.Modal(modal);
    bootstrapModal.show();
}

function showEditProfileModal() {
  

    const modal = new bootstrap.Modal(document.getElementById('editProfileModal'));
    modal.show();
}

function updateProfile() {
    const firstName = document.getElementById('editFirstName').value;
    const lastName = document.getElementById('editLastName').value;
    const phone = document.getElementById('editPhone').value;
    const address = document.getElementById('editAddress').value;

    const currentUser = AuthManager.getCurrentUser();
    if (currentUser) {
        currentUser.firstName = firstName;
        currentUser.lastName = lastName;
        currentUser.phone = phone;
        currentUser.address = address;
        
        localStorage.setItem('currentUser', JSON.stringify(currentUser));
        
        document.getElementById('userName').textContent = `${firstName} ${lastName}`;
        document.getElementById('profileName').textContent = `${firstName} ${lastName}`;
        
        const modal = bootstrap.Modal.getInstance(document.getElementById('editProfileModal'));
        modal.hide();
        
    }
}




function handlePasswordChange(e) {
    e.preventDefault();
    
    const currentPassword = document.getElementById('currentPassword').value;
    const newPassword = document.getElementById('newPassword').value;
    const confirmPassword = document.getElementById('confirmNewPassword').value;

   

    AuthManager.showNotification('Password changed successfully!', 'success');
    
    document.getElementById('passwordForm').reset();
}

function handleProfileUpdate(e) {
    e.preventDefault();
    updateProfile();
}