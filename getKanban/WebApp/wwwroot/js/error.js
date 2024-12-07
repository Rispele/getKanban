function showMessage(message, isError = true) {
    const errorCard = document.createElement('div');
    errorCard.className = isError ? 'error-card' : 'message-card';
    errorCard.textContent = message;
    
    const cardBlock = document.createElement('div');
    cardBlock.className = 'card';
    cardBlock.appendChild(errorCard);

    document.body.appendChild(cardBlock);

    setTimeout(() => {
        errorCard.style.opacity = '0';
        setTimeout(() => {
            document.body.removeChild(cardBlock);
        }, 500);
    }, 5000);
}