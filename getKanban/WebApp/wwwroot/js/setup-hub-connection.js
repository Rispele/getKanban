function setupHubConnection(builder, sessionId, teamId, redirect=true) {
    const connection = builder.withUrl("/teamSessionHub").build();
    connection.start()
        .then(function () {
            connection.invoke("Join", sessionId, teamId);
        });
    if (redirect) {
        connection.on("NotifyPageChange", function (page, stage) {
            window.location.href = `/${sessionId}/${teamId}/step/${page}/${stage}`;
        });
    }
    return connection;
}