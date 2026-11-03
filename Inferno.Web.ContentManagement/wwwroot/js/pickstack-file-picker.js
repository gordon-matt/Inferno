// Bridges PageKrafter's file picker to Pickstack, whichever file manager the site registered with it.
// Pickstack's runtime is loaded by <PickstackScripts />, so wait for it rather than assuming it is ready.

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

// Resolves with { url, name } for the chosen file, or null when the dialog is dismissed without one.
export async function pick(request) {
    const files = await whenReady(5000);

    return new Promise(resolve => {
        let settled = false;
        const finish = file => {
            if (settled) {
                return;
            }

            settled = true;
            resolve(file && file.url ? { url: file.url, name: file.name || null } : null);
        };

        files.open({
            type: request.type,
            root: request.root || "",
            title: request.title || "",
            callback: finish,
            onClose: () => finish(null),
        });
    });
}
