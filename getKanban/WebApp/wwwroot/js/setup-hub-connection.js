
function setupHubConnection(builder) {
    const connection = builder.withUrl("/lobbyHub").build();
    connection.start();
    connection.on("NotifyPageChange", function (page, stage) {
        window.location.href = `/step/${page}/${stage}`;
    });
    return connection;
}