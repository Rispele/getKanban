function setupHubConnection(builder, sessionId, teamId) {
    const connection = builder.withUrl("/teamSessionHub").build();
    connection.start()
        .then(function () {
            connection.invoke("Join", sessionId);
        });
    connection.on("NotifyPageChange", function (page, stage) {
        window.location.href = `/${sessionId}/${teamId}/step/${page}/${stage}`;
    });
    return connection;
}