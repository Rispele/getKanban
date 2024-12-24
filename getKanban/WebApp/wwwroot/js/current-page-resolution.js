
async function resolveCurrentPage(connection, sessionId, teamId, ...events) {
    let allEventsAwaited = false;
    for (let event of events) {
        if (await checkPageAvailable(sessionId, teamId, event) === true) {
            allEventsAwaited = true;
            break;
        }
    }
    
    if (!allEventsAwaited) {
        const response = await fetchJSON(`/${sessionId}/${teamId}/api/get-current-step`, null, true);
        if (response !== `game-result`) {
            const pageNumber = parseInt(response.split('/')[0]);
            const stageNumber = parseInt(response.split('/')[1]);
            await connection.invoke("ChangePage", sessionId, teamId, pageNumber, stageNumber);
        }
        window.location.href = `/${sessionId}/${teamId}/step/${response}`;
    }
}