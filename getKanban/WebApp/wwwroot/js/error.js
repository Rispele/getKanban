function showError(message) {
    const errorCard = document.createElement('div');
    errorCard.className = 'error-card';
    errorCard.textContent = message;

    document.body.appendChild(errorCard);

    setTimeout(() => {
        errorCard.style.opacity = '0';
        setTimeout(() => {
            document.body.removeChild(errorCard);
        }, 500);
    }, 5000);
}