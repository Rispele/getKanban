import * as signalR from "@microsoft/signalr";

document.addEventListener('DOMContentLoaded', async function () {
    const connection = new signalR.HubConnectionBuilder().withUrl("/lobbyHub").build();
    connection.on("NotifyJoined", function (userId, userName) {
        const playerElement = document.createElement("p");
        playerElement.setAttribute("class", "fs-6");
        const team = document.getElementsByClassName("commandEditorNamesContainer")[0];
        team.appendChild(playerElement);
        playerElement.textContent = userName;
    });
});