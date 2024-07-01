export async function uploadDemoData(cacheName, bookDbPath, blob) {

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