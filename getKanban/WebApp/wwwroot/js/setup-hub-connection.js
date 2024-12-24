function setupHubConnection(builder, sessionId, teamId, redirect = true) {
    const connection = builder
        .withUrl("/teamSessionHub")
        .withAutomaticReconnect([0, 3000, 5000, 10000, 15000, 30000])
        .build();
    connection.onreconnected(async function () {
        window.location.reload();
    });
    connection.start()
        .then(async function () {
            await connection.invoke("Join", sessionId, teamId);
        });
    if (redirect) {
        connection.on("NotifyPageChange", function (page, stage) {
            window.location.href = `/${sessionId}/${teamId}/step/${page}/${stage}`;
        });
    }
    return connection;
}