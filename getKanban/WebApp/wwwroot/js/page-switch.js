
function setSwitchEvent(elementId, connection, teamId, pageNumber, stageNumber) {
    document.getElementById(elementId).addEventListener('click', function () {
        connection.invoke("ChangePage", teamId, pageNumber, stageNumber)
            .then(function () {
                window.location.href = `/step/${pageNumber}/${stageNumber}`;
            });
    });
}
