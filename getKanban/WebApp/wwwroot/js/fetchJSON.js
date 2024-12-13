async function fetchJSON(url, parameters=null, isText=false) {
    const response = await fetch(url, parameters);
    const responseText = await response.text();
    if (isText) { return responseText; }
    return responseText.length > 0 ? JSON.parse(responseText) : null;
}

async function fetchPostJSON(url, body = null) {
    const response = await fetch(url, {
        method: 'POST',
        headers: {"Content-Type": "application/json"},
        body: body === null ? {} : body
    });
    const responseText = await response.text();
    return responseText.length > 0 ? JSON.parse(responseText) : null;
}