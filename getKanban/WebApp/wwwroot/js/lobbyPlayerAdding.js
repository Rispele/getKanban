import * as signalR from "@microsoft/signalr";

document.getElementById("join").addEventListener("click", async function () {
    const connection = new signalR.HubConnectionBuilder().withUrl("/lobbyHub").build();
    console.log("123")
    const userName = document.getElementById("userName").value;
    const sessionId = document.getElementById("sessionId").value;
    if (sessionId.length > 0 && userName.length > 0) {
        const isGameOpened = await fetch(`http://localhost:5046/game/check?sessionId=${sessionId}`);
        if ((await isGameOpened.text()) === 'true') {
            const userId = await fetch(`http://localhost:5046/session/user/create?name=${userName}`);
            connection.invoke("SendMessage", userId, userName).catch(function (err) {
                return console.error(err.toString());
            });
            window.location.href = `http://localhost:5046/lobby?sessionId=${sessionId}`
        } else {
        }
    }
});