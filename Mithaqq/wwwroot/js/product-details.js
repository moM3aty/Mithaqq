document.addEventListener("DOMContentLoaded", () => {
    const qtyInput = document.querySelector('.qty-input');
    const plusBtn = document.querySelector('.btn-plus');
    const minusBtn = document.querySelector('.btn-minus');

    if (plusBtn) {
        plusBtn.addEventListener('click', () => {
            let currentVal = parseInt(qtyInput.value);
            qtyInput.value = currentVal + 1;
        });
    }

    if (minusBtn) {
        minusBtn.addEventListener('click', () => {
            let currentVal = parseInt(qtyInput.value);
            if (currentVal > 1) {
                qtyInput.value = currentVal - 1;
            }
        });
    }
});
