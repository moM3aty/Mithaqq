document.addEventListener("DOMContentLoaded", () => {
    function updateCartItem(cartItemId, quantity) {
        // This would be an AJAX call in a real application
        console.log(`Updating item ${cartItemId} to quantity ${quantity}`);

        // Find the form and submit it
        const form = document.querySelector(`#update-form-${cartItemId}`);
        if (form) {
            const quantityInput = form.querySelector('input[name="quantity"]');
            quantityInput.value = quantity;
            form.submit();
        }
    }

    function removeItem(cartItemId) {
        // This would be an AJAX call in a real application
        console.log(`Removing item ${cartItemId}`);
        const form = document.querySelector(`#remove-form-${cartItemId}`);
        if (form) {
            form.submit();
        }
    }

    document.querySelectorAll(".qty-input").forEach(input => {
        input.addEventListener("change", (e) => {
            const cartItemId = e.target.closest('.cart-item').dataset.cartitemid;
            let quantity = parseInt(e.target.value);
            if (isNaN(quantity) || quantity < 1) {
                quantity = 1;
                e.target.value = 1;
            }
            updateCartItem(cartItemId, quantity);
        });
    });

    document.querySelectorAll(".btn-plus").forEach(btn => {
        btn.addEventListener("click", function () {
            const input = this.parentElement.querySelector(".qty-input");
            const cartItemId = this.closest('.cart-item').dataset.cartitemid;
            let value = parseInt(input.value) + 1;
            input.value = value;
            updateCartItem(cartItemId, value);
        });
    });

    document.querySelectorAll(".btn-minus").forEach(btn => {
        btn.addEventListener("click", function () {
            const input = this.parentElement.querySelector(".qty-input");
            const cartItemId = this.closest('.cart-item').dataset.cartitemid;
            let value = parseInt(input.value);
            if (value > 1) {
                value--;
                input.value = value;
                updateCartItem(cartItemId, value);
            }
        });
    });

    document.querySelectorAll(".btn-remove-item").forEach(btn => {
        btn.addEventListener("click", function () {
            const cartItemId = this.closest('.cart-item').dataset.cartitemid;
            removeItem(cartItemId);
        });
    });
});
