(function () {
    document.addEventListener('click', async function (e) {
        const btn = e.target.closest('.btn-favorite');
        if (!btn) return;

        e.preventDefault();
        const propertyId = parseInt(btn.dataset.propertyId, 10);

        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        if (!tokenInput) {
            window.location.href = '/Account/Login';
            return;
        }

        const res = await fetch('/api/favorites/toggle', {
            method: 'POST',
            credentials: 'same-origin',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': tokenInput.value
            },
            body: JSON.stringify({propertyId})
        });

        if (res.status === 401) {
            window.location.href = '/Account/Login';
            return;
        }

        if (!res.ok) return;

        const data = await res.json();
        btn.classList.toggle('active', data.isFavorite);
        btn.setAttribute('aria-pressed', data.isFavorite ? 'true' : 'false');

        const count = document.getElementById('favorites-count');
        if (count && typeof data.count === 'number') {
            count.textContent = data.count.toString();
        }

        if (!data.isFavorite) {
            const favoriteCard = btn.closest('[data-favorite-card-id]');
            if (favoriteCard) {
                favoriteCard.remove();

                const grid = document.getElementById('favorites-grid');
                const emptyState = document.getElementById('favorites-empty-state');
                if (grid && emptyState && !grid.querySelector('[data-favorite-card-id]')) {
                    emptyState.hidden = false;
                }
            }
        }
    });
})();
