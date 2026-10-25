import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { test } from 'node:test';
import vm from 'node:vm';

async function harness() {
    const elements = new Map();
    const context = vm.createContext({
        TextDecoder, console, AbortController, DOMException, queueMicrotask,
        window: { addEventListener() {} },
        document: { querySelector(selector) {
            if (!elements.has(selector)) elements.set(selector, { addEventListener() {}, focus() {}, setAttribute() {} });
            return elements.get(selector);
        } },
    });
    const code = (await readFile('web/main.js', 'utf8')).replace('boot().catch(fail);', '').replace('setupLifecycle();', '');
    vm.runInContext(code, context);
    return { context, elements, run: code => vm.runInContext(code, context) };
}

test('dispose synchronously shuts down the game, then closes audio and GPU exactly once', async () => {
    const h = await harness();
    const order = [];
    h.context.window.Module = { _lub_tcs_shutdown: () => order.push('shutdown') };
    h.context.window.Browser = { mainLoop: { pause: () => order.push('pause') } };
    h.context.window.miniaudio = { devices: [{ webaudio: { state: 'running', close: async () => order.push('native-audio') } }] };
    h.context.deviceStub = { destroy: () => order.push('gpu') };
    h.run('device = deviceStub; queue.push({ topic: "test", payload: "" })');
    await h.run('dispose()');
    await h.run('dispose()');
    assert.deepEqual(order, ['pause', 'shutdown', 'native-audio', 'gpu']);
    assert.equal(h.run('queue.length'), 0);
    assert.equal(h.run('requests.signal.aborted'), true);
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
