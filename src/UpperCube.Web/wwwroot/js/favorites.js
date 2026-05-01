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
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': tokenInput.value
            },
            body: JSON.stringify({ propertyId })
        });

        if (res.status === 401) {
            window.location.href = '/Account/Login';
            return;
        }

        if (!res.ok) return;

        const data = await res.json();
        btn.classList.toggle('active', data.isFavorite);
    });
})();