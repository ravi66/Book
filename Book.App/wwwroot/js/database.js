const cacheName = 'SqliteWasmHelper';
const bookDbPath = '/data/cache/book.sqlite3';

export async function deleteDatabase() {

    window.sqlitedb = window.sqlitedb || {
        init: false,
        cache: await caches.open(cacheName)
    };

    const match = await window.sqlitedb.cache.match(bookDbPath);

    if (match && match.ok) {

        const deleteResp = await window.sqlitedb.cache.delete(bookDbPath);

        if (deleteResp) {
            return true;
        }
    }

    return false;
}

export async function generateDownloadUrl() {

    window.sqlitedb = window.sqlitedb || {
        init: false,
        cache: await caches.open(cacheName)
    };

    const match = await window.sqlitedb.cache.match(bookDbPath);

    if (match && match.ok) {
        const dbBlob = await match.blob();
        if (dbBlob) {
            return URL.createObjectURL(dbBlob);
        }
    }

    return '';
}

export async function uploadDatabase(blob) {

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

