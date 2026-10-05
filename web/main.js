const config = JSON.parse(document.querySelector("#game-config")?.textContent || "{}");
const canvas = document.querySelector('#canvas');
const status = document.querySelector('#status');
const decoder = new TextDecoder();
const queue = [];
const pressed = new Set();
const controls = new Map(config.controls || [
    ['ArrowUp', 1], ['KeyW', 1], ['ArrowDown', 2], ['KeyS', 2],
    ['ArrowLeft', 4], ['KeyA', 4], ['ArrowRight', 8], ['KeyD', 8],
    ['KeyZ', 16], ['ControlLeft', 16], ['Period', 16],
    ['KeyX', 32], ['AltLeft', 32], ['ShiftLeft', 32], ['ShiftRight', 32], ['Slash', 32],
    ['KeyP', 64], ['Escape', 128],
]);
let audio;
let volume;
let music;
let musicGain;
let musicVersion = 0;
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
    pressed.clear();
    stopFrames();
    window.Browser?.mainLoop?.pause?.();
    try { window.Module?._lub_tcs_shutdown?.(); } catch (error) { console.warn(error); }
    // on_quit above may send the game's normal score/replay save messages.
    try { window.Module?.persistSaves?.(); } catch (error) { console.warn(error); }
    if (window.Module) window.Module.noInitialRun = true;
    queue.length = 0;
    requests.abort();
    stopMusic();
    for (const channel of soundVersions.keys()) stopSound(channel);
    const closing = [];
    try { closing.push(...endSessions()); } catch (error) { console.warn(error); }
    const contexts = new Set([audio, ...(window.miniaudio?.devices || []).map(device => device?.webaudio)]);
    for (const context of contexts) {
        if (context && context.state !== 'closed') {
            try { closing.push(context.close()); } catch (error) { console.warn(error); }
        }
    }
    try { if (document.pointerLockElement) document.exitPointerLock(); } catch {}
    if (document.fullscreenElement) closing.push(document.exitFullscreen().catch(() => {}));
    device?.destroy();
    buffers.clear();
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
// A game with its own save files drives input, audio and storage through the runtime.
const direct = Boolean(config.saves);
const buffers = new Map();
const channels = new Map();
const soundVersions = new Map();
const musicNames = config.music || ['we_are_tumiki_fighters', 'just_over_the_horizon', 'panic_on_meadow', 'here_comes_a_gigantic_toy', 'battle_over_the_junk_city', 'return_to_home'];
const soundNames = config.sounds || ['ship_shot', 'stuck', 'stuck_bonus', 'stuck_destroyed', 'ship_destroyed', 'enemy_damaged', 'small_enemy_destroyed', 'enemy_destroyed', 'boss_destroyed', 'extend', 'warning', 'propeller', 'stuck_bonus_pushin'];
const soundChannels = config.channels || [0, 1, 2, 3, 2, 4, 5, 6, 6, 7, 7, 7, 2];

function send(topic, payload) { if (!disposed && !direct) queue.push({ topic, payload }); }
function input() {
    let mask = 0;
    for (const key of pressed) mask |= controls.get(key) || 0;
    send('input', String(mask));
}
function initAudio() {
    if (audio || disposed) return;
    audio = new AudioContext();
    volume = audio.createGain();
    volume.gain.value = muted ? 0 : 1;
    volume.connect(audio.destination);
}
function unlockAudio() {
    if (disposed) return;
    if (direct) { compiled?.unlockAudio(); return; }
    initAudio();
    if (audio.state === 'suspended') audio.resume();
}
window.addEventListener('keydown', event => {
    if (disposed || !controls.has(event.code)) return;
    if (event.target instanceof HTMLButtonElement || event.target instanceof HTMLAnchorElement) return;
    event.preventDefault(); unlockAudio();
    if (!pressed.has(event.code)) { pressed.add(event.code); input(); }
});
window.addEventListener('keyup', event => {
    if (pressed.delete(event.code)) { event.preventDefault(); input(); }
});
window.addEventListener('blur', () => { pressed.clear(); input(); });
canvas.addEventListener('pointerdown', () => { unlockAudio(); canvas.focus(); });
if (config.pointer) {
    let pointer = [320, 240, 0];
    function point(event) {
        const rect = canvas.getBoundingClientRect();
        pointer = [Math.round(Math.max(0, Math.min(640, (event.clientX - rect.left) * 640 / rect.width))),
            Math.round(Math.max(0, Math.min(480, (event.clientY - rect.top) * 480 / rect.height))), event.buttons & 3];
        send('pointer', pointer.join(','));
    }
    canvas.addEventListener('pointerdown', event => { canvas.setPointerCapture(event.pointerId); point(event); });
    canvas.addEventListener('pointermove', point);
    canvas.addEventListener('pointerup', point);
    canvas.addEventListener('pointercancel', () => { pointer[2] = 0; send('pointer', pointer.join(',')); });
    canvas.addEventListener('contextmenu', event => event.preventDefault());
    window.addEventListener('blur', () => { pointer[2] = 0; send('pointer', pointer.join(',')); });
}
document.querySelector('#fullscreen').onclick = async () => { try { await canvas.requestFullscreen(); canvas.focus(); } catch (error) { fail(error); } };
document.querySelector('#sound').onclick = event => {
    unlockAudio(); muted = !muted;
    event.currentTarget.textContent = muted ? '音：オフ' : '音：オン';
    event.currentTarget.setAttribute('aria-pressed', String(muted));
    if (direct) compiled?.volume(window.Module, muted ? 0 : 1);
    else volume.gain.value = muted ? 0 : 1;
    canvas.focus();
};
async function buffer(name, extension) {
    initAudio();
    const key = `${name}.${extension}`;
    if (!buffers.has(key)) buffers.set(key, (async () => {
        const response = await fetch(`audio/${key}`, { signal: requests.signal });
        if (!response.ok) throw new Error(`音源を読み込めません：${key}`);
        const bytes = await response.arrayBuffer();
        if (disposed) throw new DOMException('Game closed', 'AbortError');
        return audio.decodeAudioData(bytes);
    })());
    return buffers.get(key);
}
function stopMusic(fade = false) {
    musicVersion++;
    if (!music) return;
    if (fade) {
        musicGain.gain.cancelScheduledValues(audio.currentTime);
        musicGain.gain.setValueAtTime(musicGain.gain.value, audio.currentTime);
        musicGain.gain.linearRampToValueAtTime(0, audio.currentTime + 1.28);
        music.stop(audio.currentTime + 1.28);
    } else music.stop();
    music = null;
}
async function playMusic(index, loop) {
    if (disposed) return;
    stopMusic();
    const version = musicVersion;
    const decoded = await buffer(musicNames[index], config.musicExtension || 'ogg');
    if (disposed || version !== musicVersion) return;
    music = audio.createBufferSource(); music.buffer = decoded; music.loop = loop;
    musicGain = audio.createGain();
    music.connect(musicGain).connect(volume); music.start();
}
function stopSound(channel) {
    soundVersions.set(channel, (soundVersions.get(channel) || 0) + 1);
    channels.get(channel)?.stop();
    channels.delete(channel);
}
async function playSound(index) {
    if (disposed) return;
    const channel = soundChannels[index];
    stopSound(channel);
    const version = soundVersions.get(channel);
    const decoded = await buffer(soundNames[index], 'wav');
    if (disposed || version !== soundVersions.get(channel)) return;
    const source = audio.createBufferSource(); source.buffer = decoded;
    source.connect(volume);
    channels.set(channel, source); source.start();
    source.onended = () => { if (channels.get(channel) === source) channels.delete(channel); };
}
function fail(error) { if (disposed) return; status.hidden = false; status.textContent = String(error?.message || error); notify('aba:error', status.textContent); console.error(error); }
window.lubHost = { queue, onMessage(topic, bytes) {
    const text = decoder.decode(bytes);
    if (disposed && !['scores.save', 'replay.save'].includes(topic)) return;
    if (topic === 'quit' && !quitScheduled) {
        quitScheduled = true;
        // OnQuit destroys the native host. Never run it reentrantly inside a
        // Host.Send callback while the game's Wasm frame is still on the stack.
        queueMicrotask(() => { if (!disposed) returnToList(); });
    }
    if (topic === 'ready') {
        xr?.ready();
        status.hidden = true; notify('aba:ready');
        focusCanvasWhenReady();
        if (config.seed) send('seed', String(crypto.getRandomValues(new Uint32Array(1))[0] & 0x7fffffff));
    }
    if (topic === 'xr.present') xr?.present();
    if (topic === 'scores.load') {
        try { send('scores', localStorage.getItem(config.scores || 'tumiki-scores-v1') || ''); } catch { send('scores', ''); }
    }
    if (topic === 'scores.save') {
        try { localStorage.setItem(config.scores || 'tumiki-scores-v1', text); } catch { }
    }
    if (topic === 'replay.load' && config.replay) {
        try { send('replay', localStorage.getItem(config.replay) || ''); } catch { send('replay', ''); }
    }
    if (topic === 'replay.save' && config.replay) {
        try { localStorage.setItem(config.replay, text); } catch { }
    }
    if (topic === 'music.loop' || topic === 'music.once') playMusic(Number(text), topic === 'music.loop').catch(fail);
    if (topic === 'music.stop') stopMusic();
    if (topic === 'music.fade') stopMusic(true);
    if (topic === 'sound.play') playSound(Number(text)).catch(fail);
    if (topic === 'sound.stop') stopSound(soundChannels[Number(text)]);
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
    window.slangCompile = async (source, entry) => (Array.isArray(shaders)
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
        preRun: [() => { if (disposed) throw new Error('Game closed'); compiled.prepare(module, files, config.saves || []); }],
    };
    if (direct) module.onRuntimeInitialized = () => {
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
