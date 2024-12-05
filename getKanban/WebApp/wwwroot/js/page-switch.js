
function setSwitchEvent(elementId, connection, sessionId, teamId, pageNumber, stageNumber) {
    document.getElementById(elementId).addEventListener('click', function () {
        connection.invoke("ChangePage", sessionId, teamId, pageNumber, stageNumber)
            .then(function () {
                window.location.href = `/${sessionId}/${teamId}/step/${pageNumber}/${stageNumber}`;
            });
    });
}
