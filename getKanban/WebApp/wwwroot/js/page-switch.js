function setSwitchEvent(elementId, connection, sessionId, teamId, pageNumber, stageNumber) {
    document.getElementById(elementId).addEventListener('click', function () {
        connection.invoke("ChangePage", sessionId, teamId, pageNumber, stageNumber)
            .then(function () {
                window.location.href = `/${sessionId}/${teamId}/step/${pageNumber}/${stageNumber}`;
            });
    });
}

function setRollbackEvent(connection) {
    connection.on("NotifyRollbackToDay", function (sessionId, teamId, dayNumber) {
        window.location.href = `/${sessionId}/${teamId}/step/1/0`;
    });
}

async function checkPageAvailable(sessionId, teamId, event) {
    return await fetch(`/${sessionId}/${teamId}/api/check-available?event=${event}`)
        .then(available => available.json())
        .then(available => { return available; });
}
