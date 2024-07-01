export async function downloadDb(cacheName, bookDbPath, fileName) {

    window.sqlitedb = window.sqlitedb || {
        init: false,
        cache: await caches.open(cacheName)
    };

    const match = await window.sqlitedb.cache.match(bookDbPath);

    if (match && match.ok) {

        const dbBlob = await match.blob();
        if (dbBlob) {

            const url = URL.createObjectURL(dbBlob);
            const anchorElement = document.createElement('a');
            anchorElement.href = url;
            anchorElement.download = fileName ?? '';
            anchorElement.click();
            anchorElement.remove();
            URL.revokeObjectURL(url);
        }
    }
}

export async function downloadJsonDb(fileName, contentStreamReference) {

    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
}

export async function uploadDatabase(cacheName, bookDbPath, blob) {

    window.sqlitedb = window.sqlitedb || {
        init: false,
        cache: await caches.open(cacheName)
    };

    const match = await window.sqlitedb.cache.match(bookDbPath);

    if (match && match.ok) {

        await window.sqlitedb.cache.delete(bookDbPath);
    }

    const headers = new Headers({
        'content-length': blob.size
    });

    const response = new Response(blob, {
        headers
    });

    await window.sqlitedb.cache.put(bookDbPath, response);
}