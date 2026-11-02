// Bridges PageKrafter's file picker to Pickstack, whichever file manager the site registered with it.
// Pickstack's runtime is loaded by <PickstackScripts />, so wait for it rather than assuming it is ready.

let pending = null;

function whenReady(timeoutMs) {
    return new Promise((resolve, reject) => {
        const started = Date.now();
        const check = () => {
            if (window.Pickstack && window.Pickstack.files) {
                resolve(window.Pickstack.files);
            } else if (Date.now() - started > timeoutMs) {
                reject(new Error("Pickstack is not loaded on this page. Add <PickstackScripts /> to it."));
            } else {
                setTimeout(check, 50);
            }
        };
        check();
    });
}

// Resolves with { url, name } for the chosen file. Pickstack gives no signal when its dialog is closed without
// a choice, so a request that was left open is ended (as "no choice") when the next one starts.
export async function pick(request) {
    const files = await whenReady(5000);

    if (pending) {
        pending(null);
        pending = null;
    }

    return new Promise(resolve => {
        pending = resolve;
        files.open({
            type: request.type,
            root: request.root || "",
            title: request.title || "",
            callback: file => {
                if (pending === resolve) {
                    pending = null;
                }
                resolve(file && file.url ? { url: file.url, name: file.name || null } : null);
            },
        });
    });
}
