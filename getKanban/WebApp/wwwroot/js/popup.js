function showMessage(message, isError = true) {
    const popup = document.createElement('div');
    popup.className = 'popup-card';
    popup.className += isError ? ' fail' : ' success';
    if (message.indexOf("\n") >= 0) {
        popup.innerHTML = message.replace(/(\r\n|\n|\r)/gm, "<br>");
    } else {
        popup.textContent = message;
    }

    document.getElementsByClassName('notification-container')[0].appendChild(popup);

    setTimeout(() => {
        popup.style.opacity = '0';
        setTimeout(() => {
            document.getElementsByClassName('notification-container')[0].removeChild(popup);
        }, 500);
    }, 5000);
}