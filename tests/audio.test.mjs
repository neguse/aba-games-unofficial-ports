import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { test } from 'node:test';
import vm from 'node:vm';

async function harness() {
    const sources = [], gains = [], requests = new Map();
    const elements = new Map();
    const context = vm.createContext({
        TextDecoder, console, AbortController, DOMException, queueMicrotask,
        window: { addEventListener() {} },
        document: { querySelector(selector) {
            if (!elements.has(selector)) elements.set(selector, { addEventListener() {}, focus() {}, setAttribute() {} });
            return elements.get(selector);
        } },
        AudioContext: class {
            currentTime = 0;
            state = 'running';
            close() { this.state = 'closed'; return Promise.resolve(); }
            destination = {};
            decodeAudioData = async data => data;
            createGain() {
                const gain = { gain: { value: 1, cancelScheduledValues() {}, setValueAtTime() {}, linearRampToValueAtTime() {} },
                    connect(target) { this.target = target; return target; } };
                gains.push(gain); return gain;
            }
            createBufferSource() {
                const source = { starts: 0, stops: 0, connect(target) { this.target = target; return target; },
                    start() { this.starts++; }, stop() { this.stops++; } };
                sources.push(source); return source;
            }
        },
        fetch: url => new Promise(resolve => requests.set(url, () => resolve({ ok: true, arrayBuffer: async () => url }))),
    });
    const code = (await readFile('web/main.js', 'utf8')).replace('boot().catch(fail);', '').replace('setupLifecycle();', '');
    vm.runInContext(code, context);
    return { context, sources, gains, requests, elements, run: code => vm.runInContext(code, context) };
}

test('stopping a sound cancels a pending decode', async () => {
    const h = await harness();
    const pending = h.run('playSound(0)');
    h.run('stopSound(0)');
    h.requests.get('audio/ship_shot.wav')();
    await pending;
    assert.equal(h.sources.length, 0);
});

test('the newest sound on a shared channel wins regardless of fetch order', async () => {
    const h = await harness();
    const old = h.run('playSound(2)');
    const latest = h.run('playSound(4)');
    h.requests.get('audio/ship_destroyed.wav')();
    await latest;
    h.requests.get('audio/stuck_bonus.wav')();
    await old;
    assert.equal(h.sources.length, 1);
    assert.equal(h.sources[0].buffer, 'audio/ship_destroyed.wav');
    assert.equal(h.sources[0].starts, 1);
    assert.equal(h.sources[0].stops, 0);
});

test('mute affects fading music and effects through the same output', async () => {
    const h = await harness();
    const music = h.run('playMusic(0, true)');
    h.requests.get('audio/we_are_tumiki_fighters.ogg')();
    await music;
    h.run('stopMusic(true)');
    const sound = h.run('playSound(0)');
    h.requests.get('audio/ship_shot.wav')();
    await sound;
    const button = h.elements.get('#sound');
    button.onclick({ currentTarget: button });
    const output = h.gains[0];
    assert.equal(output.gain.value, 0);
    assert.equal(h.sources[0].target.target, output);
    assert.equal(h.sources[1].target, output);
    button.onclick({ currentTarget: button });
    assert.equal(output.gain.value, 1);
});

test('dispose synchronously shuts down and saves, then closes audio and GPU exactly once', async () => {
    const h = await harness();
    const order = [];
    h.context.window.Module = { _lub_tcs_shutdown: () => order.push('shutdown'), persistSaves: () => order.push('save') };
    h.context.window.Browser = { mainLoop: { pause: () => order.push('pause') } };
    h.context.window.miniaudio = { devices: [{ webaudio: { state: 'running', close: async () => order.push('native-audio') } }] };
    h.context.deviceStub = { destroy: () => order.push('gpu') };
    h.run('device = deviceStub; initAudio(); pressed.add("KeyZ"); send("input", "16")');
    await h.run('dispose()');
    await h.run('dispose()');
    assert.deepEqual(order, ['pause', 'shutdown', 'save', 'native-audio', 'gpu']);
    assert.equal(h.run('audio.state'), 'closed');
    assert.equal(h.run('queue.length'), 0);
    assert.equal(h.run('pressed.size'), 0);
    assert.equal(h.run('requests.signal.aborted'), true);
    h.run('send("input", "16"); unlockAudio()');
    assert.equal(h.run('queue.length'), 0);
});

test('audio completing after disposal never starts again', async () => {
    const h = await harness();
    const pending = h.run('playSound(0)').catch(error => error.name);
    await h.run('dispose()');
    h.requests.get('audio/ship_shot.wav')();
    assert.equal(await pending, 'AbortError');
    assert.equal(h.sources.length, 0);
});

test('on_quit legacy save messages survive shutdown; new playback messages do not', async () => {
    const h = await harness();
    const saves = new Map();
    h.context.localStorage = { setItem: (key, value) => saves.set(key, value) };
    h.context.bytes = new TextEncoder().encode('original-score-format');
    h.context.window.Module = { _lub_tcs_shutdown: () => h.run('window.lubHost.onMessage("scores.save", bytes)') };
    await h.run('dispose()');
    h.run('window.lubHost.onMessage("sound.play", new Uint8Array([48]))');
    assert.equal(saves.get('tumiki-scores-v1'), 'original-score-format');
    assert.equal(h.requests.size, 0);
});

test('XR sessions that resolve after disposal are ended before setup can continue', async () => {
    const h = await harness();
    let resolveSession, ends = 0;
    h.context.URLSearchParams = URLSearchParams;
    h.context.URL = URL;
    h.context.location = new URL('https://example.test/torus-trooper/?embedded=1');
    h.context.navigator = { xr: { requestSession: () => new Promise(resolve => { resolveSession = resolve; }) } };
    h.context.window.parent = { postMessage() {} };
    h.context.window.requestAnimationFrame = () => 1;
    h.context.window.cancelAnimationFrame = () => {};
    h.context.document.documentElement = { classList: { add() {} } };
    h.context.document.createElement = () => ({ addEventListener() {} });
    h.context.document.querySelectorAll = () => [];
    h.elements.get('#canvas').before = () => {};
    h.run('config.webxr = true; setupLifecycle()');
    const pending = h.run('navigator.xr.requestSession("immersive-vr")').catch(error => error.message);
    await h.run('dispose()');
    resolveSession({ end: async () => { ends++; }, addEventListener() {} });
    await pending;
    assert.equal(ends, 1);
});

test('original game quit returns to the embedded list once after the native callback unwinds', async () => {
    const h = await harness();
    const messages = [];
    h.context.location = new URL('https://example.test/collection/wok/?embedded=1');
    h.context.window.parent = { postMessage: (...args) => messages.push(args) };
    h.run('embedded = true; window.lubHost.onMessage("quit", new Uint8Array()); window.lubHost.onMessage("quit", new Uint8Array())');
    assert.equal(messages.length, 0, 'quit must not navigate synchronously inside Host.Send');
    await Promise.resolve();
    assert.equal(messages.length, 1);
    assert.equal(messages[0][0].type, 'aba:return');
    assert.equal(messages[0][1], 'https://example.test');
    h.run('window.lubHost.onMessage("quit", new Uint8Array())');
    await Promise.resolve();
    assert.equal(messages.length, 1, 'later duplicate quit must not schedule another transition');
});

test('standalone game quit defers shutdown and navigates to the collection root', async () => {
    const h = await harness();
    const order = [];
    h.context.URL = URL;
    h.context.location = {
        pathname: '/collection/mu-cade/',
        get href() { return 'https://example.test/collection/mu-cade/'; },
        set href(value) { order.push(['navigate', value]); },
    };
    h.context.window.Module = { _lub_tcs_shutdown: () => order.push(['shutdown']) };
    h.run('window.lubHost.onMessage("quit", new Uint8Array())');
    assert.deepEqual(order, [], 'native shutdown must wait until Host.Send returns');
    await Promise.resolve();
    assert.deepEqual(order, [['shutdown'], ['navigate', 'https://example.test/collection/']]);
    assert.equal(h.run('disposed'), true);
});

test('a queued original quit cannot revive navigation after disposal', async () => {
    const h = await harness();
    const messages = [];
    h.context.location = new URL('https://example.test/collection/wok/?embedded=1');
    h.context.window.parent = { postMessage: (...args) => messages.push(args) };
    h.run('embedded = true; window.lubHost.onMessage("quit", new Uint8Array()); dispose()');
    await Promise.resolve();
    assert.equal(messages.length, 0);
});
