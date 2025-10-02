function initializeRatingForm(itemData) {
    const form = document.getElementById('review-form');
    if (!form) return;

    const starsContainer = form.querySelector('.interactive-stars');
    const ratingInput = document.getElementById('star-rating-value');
    const stars = starsContainer.querySelectorAll('i');
    const commentInput = document.getElementById('review-comment');
    const messageEl = document.getElementById('review-message');

    starsContainer.addEventListener('mouseover', (e) => {
        if (e.target.tagName === 'I') {
            const hoverValue = parseInt(e.target.dataset.value);
            updateStars(hoverValue);
        }
    });

    starsContainer.addEventListener('mouseout', () => {
        const selectedRating = parseInt(ratingInput.value);
        updateStars(selectedRating);
    });

    starsContainer.addEventListener('click', (e) => {
        if (e.target.tagName === 'I') {
            const clickValue = parseInt(e.target.dataset.value);
            ratingInput.value = clickValue;
            updateStars(clickValue);
        }
    });

    function updateStars(value) {
        stars.forEach(star => {
            if (parseInt(star.dataset.value) <= value) {
                star.classList.remove('far');
                star.classList.add('fas');
            } else {
                star.classList.remove('fas');
                star.classList.add('far');
            }
        });
    }

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const rating = parseInt(ratingInput.value);
        const comment = commentInput.value;

        if (rating === 0) {
            showMessage('Please select a star rating.', true);
            return;
        }

        const reviewData = {
            stars: rating,
            comment: comment,
            ...itemData
        };

        try {
            const response = await fetch('/api/reviews', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(reviewData)
            });

            if (!response.ok) {
                const errorData = await response.text();
                throw new Error(errorData || 'Failed to submit review.');
            }

            showMessage('Thank you! Your review has been submitted.', false);
            form.style.display = 'none';
            // Optionally, reload the page to see the new review and updated average
            setTimeout(() => window.location.reload(), 2000);

        } catch (error) {
            showMessage(error.message, true);
        }
    });

    function showMessage(message, isError) {
        messageEl.textContent = message;
        messageEl.className = isError ? 'alert alert-danger mt-3' : 'alert alert-success mt-3';
    }
}
