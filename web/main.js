const config = JSON.parse(document.querySelector("#game-config")?.textContent || "{}");
const canvas = document.querySelector('#canvas');
const status = document.querySelector('#status');
// The game reads input, plays sound and saves through lub; the page only hosts its lifecycle.
const queue = [];
let muted = false;
let xr;
let compiled;
let device;
let disposed = false;
let disposal;
let quitScheduled = false;
let embedded = false;
let endSessions = () => [];
let stopFrames = () => {};
const requests = new AbortController();
function focusCanvasWhenReady() {
    if (embedded && window.parent.document.activeElement !== window.frameElement) return;
    if (!document.activeElement || document.activeElement === document.body || document.activeElement === canvas) canvas.focus();
}
function notify(type, message) {
    if (embedded) window.parent.postMessage({ type, message }, location.origin);
}
function returnToList() {
    if (embedded) notify('aba:return');
    else { dispose(); location.href = new URL(location.pathname.endsWith('/tumiki.html') ? './' : '../', location.href).href; }
}
function dispose() {
    if (disposed) return disposal || Promise.resolve();
    // Block late decode/fetch/XR completions before they can revive this game.
    disposed = true;
    stopFrames();
    window.Browser?.mainLoop?.pause?.();
    // OnQuit saves through lub (IndexedDB) before the runtime is released.
    try { window.Module?._lub_tcs_shutdown?.(); } catch (error) { console.warn(error); }
    if (window.Module) window.Module.noInitialRun = true;
    queue.length = 0;
    requests.abort();
    const closing = [];
    try { closing.push(...endSessions()); } catch (error) { console.warn(error); }
    const contexts = new Set((window.miniaudio?.devices || []).map(device => device?.webaudio));
    for (const context of contexts) {
        if (context && context.state !== 'closed') {
            try { closing.push(context.close()); } catch (error) { console.warn(error); }
        }
    }
    try { if (document.pointerLockElement) document.exitPointerLock(); } catch {}
    if (document.fullscreenElement) closing.push(document.exitFullscreen().catch(() => {}));
    device?.destroy();
    disposal = Promise.allSettled(closing);
    return disposal;
}
function setupLifecycle() {
    embedded = window.parent !== window && new URLSearchParams(location.search).get('embedded') === '1';
    window.abaGame = { dispose, returnToList, get disposed() { return disposed; } };
    if (embedded) document.documentElement.classList.add('embedded-game');
    const link = document.createElement('button');
    link.id = 'game-return'; link.type = 'button'; link.textContent = '← ゲーム一覧へ (Alt＋Esc)';
    link.addEventListener('click', returnToList);
    canvas.before(link);
    // Never let an old inter-game link navigate within the iframe and create a nested launcher.
    if (embedded) for (const anchor of document.querySelectorAll('nav a')) {
        const url = new URL(anchor.href);
        if (url.origin === location.origin && (url.pathname.endsWith('/') || url.pathname.endsWith('tumiki.html'))) anchor.hidden = true;
    }
    const request = window.requestAnimationFrame.bind(window);
    const cancel = window.cancelAnimationFrame.bind(window);
    const frames = new Set();
    window.requestAnimationFrame = callback => {
        if (disposed) return 0;
        const id = request(time => { frames.delete(id); if (!disposed) callback(time); });
        frames.add(id); return id;
    };
    window.cancelAnimationFrame = id => { frames.delete(id); cancel(id); };
    stopFrames = () => { frames.forEach(cancel); frames.clear(); };
    // Track sessions including a permission prompt that resolves after return to list.
    if (config.webxr && navigator.xr) {
        const sessions = new Set();
        const requestSession = navigator.xr.requestSession.bind(navigator.xr);
        navigator.xr.requestSession = async (...args) => {
            const session = await requestSession(...args);
            if (disposed) { await session.end(); throw new Error('ゲームは終了しました。'); }
            sessions.add(session);
            session.addEventListener('end', () => sessions.delete(session), { once: true });
            return session;
        };
        endSessions = () => [...sessions].map(session => { try { return session.end(); } catch (error) { return Promise.reject(error); } });
    }
    window.addEventListener('keydown', event => {
        if (event.altKey && event.code === 'Escape') {
            event.preventDefault(); event.stopImmediatePropagation(); returnToList();
        }
    }, { capture: true });
    window.addEventListener('pagehide', dispose);
    window.addEventListener('pageshow', event => { if (event.persisted && disposed && !embedded) location.reload(); });
}
setupLifecycle();
function unlockAudio() {
    if (!disposed) compiled?.unlockAudio();
}
window.addEventListener('keydown', unlockAudio);
canvas.addEventListener('pointerdown', () => { unlockAudio(); canvas.focus(); });
canvas.addEventListener('contextmenu', event => event.preventDefault());
document.querySelector('#fullscreen').onclick = async () => { try { await canvas.requestFullscreen(); canvas.focus(); } catch (error) { fail(error); } };
document.querySelector('#sound').onclick = event => {
    unlockAudio(); muted = !muted;
    event.currentTarget.textContent = muted ? '音：オフ' : '音：オン';
    event.currentTarget.setAttribute('aria-pressed', String(muted));
    compiled?.volume(window.Module, muted ? 0 : 1);
    canvas.focus();
};
function fail(error) { if (disposed) return; status.hidden = false; status.textContent = String(error?.message || error); notify('aba:error', status.textContent); console.error(error); }
window.lubHost = { queue, onMessage(topic) {
    if (disposed) return;
    if (topic === 'quit' && !quitScheduled) {
        quitScheduled = true;
        // OnQuit destroys the native host. Never run it reentrantly inside a
        // Host.Send callback while the game's Wasm frame is still on the stack.
        queueMicrotask(() => { if (!disposed) returnToList(); });
    }
    if (topic === 'xr.present') xr?.present();
} };

async function boot() {
    if (!navigator.gpu) throw new Error('このゲームにはWebGPU対応ブラウザが必要です。');
    const adapter = await navigator.gpu.requestAdapter();
    if (disposed) return;
    if (!adapter) throw new Error('WebGPUを初期化できませんでした。');
    const requiredFeatures = ['depth32float-stencil8', 'float32-filterable'].filter(feature => adapter.features.has(feature));
    device = await adapter.requestDevice({ requiredFeatures });
    if (disposed) { device.destroy(); return; }
    device.addEventListener('uncapturederror', event => fail(event.error));
    device.lost.then(info => fail(new Error(`描画が停止しました。ページを再読み込みしてください。 ${info.message}`)));
    const shadersResponse = await fetch('shaders.json', { signal: requests.signal });
    if (!shadersResponse.ok) throw new Error('ゲームデータを読み込めませんでした。');
    const shaders = await shadersResponse.json();
    if (disposed) return;
    window.slangCompile = (source, entry) => (Array.isArray(shaders)
        ? shaders.find(shader => shader.entry === entry && source.replaceAll('\r', '').endsWith(shader.source.replaceAll('\r', '')))
        : shaders[entry]) || { error: `Unknown shader: ${entry}` };
    compiled = await import('./compiled.js');
    if (disposed) return;
    const files = await compiled.assets({ signal: requests.signal });
    if (disposed) return;
    window._canvasWidth = 640; window._canvasHeight = 480;
    const module = {
        canvas, preinitializedWebGPUDevice: device, webgpuAdapter: adapter,
        locateFile: path => `${config.wasm || "wasm/"}${path}`,
        print: text => console.log(text),
        printErr: text => /error|failed|abort|fault/i.test(text) ? fail(new Error(text)) : console.info(text),
        preRun: [() => { if (disposed) throw new Error('Game closed'); compiled.prepare(module, files); }],
    };
    module.onRuntimeInitialized = () => {
        if (disposed) return;
        xr?.ready(); status.hidden = true; notify('aba:ready');
        compiled.volume(module, muted ? 0 : 1);
        focusCanvasWhenReady();
    };
    window.Module = module;
    if (config.webxr) {
        const { createXR } = await import(new URL('xr.js', location.href));
        if (disposed) return;
        xr = await createXR({ canvas, button: document.querySelector('#vr'), getModule: () => module,
            maxDimension: 1536, unlockAudio, report: fail });
    }
    if (disposed) return;
    const script = document.createElement('script'); script.src = `${config.wasm || 'wasm/'}lub.js`;
    script.onerror = () => fail(new Error('実行環境を読み込めませんでした。'));
    document.body.append(script);
}
boot().catch(fail);
