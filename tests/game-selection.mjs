import assert from 'node:assert/strict';
import { readFile, mkdir } from 'node:fs/promises';
import { resolve, extname } from 'node:path';
import { pathToFileURL } from 'node:url';
import { createServer } from 'node:http';
import { games } from '../web/game-catalog.js';

// --fixture tests the real shell/pages/lifecycle with a small instrumented Wasm stand-in.
// With a built dist URL, this same test exercises all 13 actual runtimes instead.
const serving = process.argv.includes('--serve');
const fixture = process.argv.includes('--fixture') || serving;
let server;
let url = process.argv.find(arg => /^https?:/.test(arg)) || 'http://127.0.0.1:8765';
if (fixture) {
    const runtime = `
const stats = { ticks: 0, shutdowns: 0, pauses: 0, title: document.title };
window.fixtureStats = stats;
parent.fixtureRuns.push(stats);
const files = new Map();
Module.FS = { mkdirTree() {}, writeFile(path, text) { files.set(path, text); }, readFile(path) { return files.get(path); }, close() {}, rename(a,b) { files.set(b, files.get(a)); } };
Module.ENV = {}; Module.ccall = () => {};
for (const callback of Module.preRun) callback();
Module.calledRun = true;
const settings = JSON.parse(document.querySelector('#game-config')?.textContent || '{}');
stats.context = new AudioContext(); window.miniaudio = { devices: [{ webaudio: stats.context }] };
window.Browser = { mainLoop: { pause() { stats.pauses++; } } };
Module._lub_tcs_shutdown = () => {
    stats.shutdowns++;
    const value = 'fixture-save-' + stats.title;
    for (const [path] of settings.saves || []) Module.FS.writeFile(path, value);
    if (!settings.saves) lubHost.onMessage('scores.save', new TextEncoder().encode(value));
};
if (Module.onRuntimeInitialized) Module.onRuntimeInitialized();
else lubHost.onMessage('ready', new Uint8Array());
function frame() { stats.ticks++; requestAnimationFrame(frame); }
requestAnimationFrame(frame);
`;
    server = createServer(async (request, response) => {
        try {
            const requestURL = new URL(request.url, 'http://localhost');
            const pathname = requestURL.pathname;
            if (pathname === '/__selector_test.js') {
                response.setHeader('Content-Type', 'text/javascript'); response.end(await readFile('tests/selector-in-browser.js')); return;
            }
            if (pathname.endsWith('/wasm/lub.js')) {
                response.setHeader('Content-Type', 'text/javascript'); response.end(runtime); return;
            }
            if (pathname.endsWith('/assets.json') || pathname.endsWith('/shaders.json')) {
                response.setHeader('Content-Type', 'application/json'); response.end('[]'); return;
            }
            if (pathname.endsWith('/xr.js')) {
                response.setHeader('Content-Type', 'text/javascript');
                response.end('export async function createXR() { return { ready() {} }; }'); return;
            }
            const path = pathname.endsWith('/') ? pathname + 'index.html' : pathname;
            const source = path.split('/').filter(Boolean).length === 1 ? 'web' + path : 'games' + path;
            response.setHeader('Content-Type', { '.html': 'text/html', '.js': 'text/javascript', '.css': 'text/css' }[extname(source)] || 'text/plain');
            let contents = await readFile(source, 'utf8');
            if (serving && source.endsWith('.html')) {
                const init = `<script>
window.fixtureRuns = [];
const fakeDevice = { lost: new Promise(() => {}), addEventListener() {}, destroy() { window.gpuDestroyed = true; } };
Object.defineProperty(navigator, 'gpu', { value: { requestAdapter: async () => ({ features: new Set(), requestDevice: async () => fakeDevice }) } });
Object.defineProperty(navigator, 'xr', { value: { requestSession: async () => {
    if (window.deferXR) await new Promise(resolve => { window.resolveXR = resolve; });
    return { addEventListener() {}, end: async () => { window.xrEnds = (window.xrEnds || 0) + 1; } };
} } });
</script>`;
                contents = contents.replace('<meta charset="utf-8">', '<meta charset="utf-8">' + init);
                if (source === 'web/index.html' && requestURL.searchParams.has('selftest'))
                    contents += '<script type="module" src="/__selector_test.js"></script>';
            }
            response.end(contents);
        } catch { response.writeHead(404); response.end('Not found'); }
    }).listen(0, '127.0.0.1');
    await new Promise(resolve => server.once('listening', resolve));
    url = `http://127.0.0.1:${server.address().port}`;
}
if (serving) { console.log(`Selector fixture server: ${url}/?selftest=1`); await new Promise(() => {}); }
const { chromium } = await import(pathToFileURL(resolve(process.env.PLAYWRIGHT_MODULE || '.cache/browser/node_modules/playwright/index.mjs')));
const browser = await chromium.launch({
    executablePath: process.env.CHROMIUM_PATH,
    headless: true,
    args: ['--no-sandbox', '--enable-unsafe-webgpu', '--use-angle=swiftshader', '--enable-features=Vulkan,WebGPU',
        '--use-vulkan=swiftshader', '--disable-vulkan-fallback-to-gl-for-testing'],
});
try {
    const page = await browser.newPage({ viewport: { width: 960, height: 850 } });
    const errors = [];
    page.on('pageerror', error => errors.push(error.message));
    page.on('console', message => { if (message.type() === 'error') errors.push(message.text()); });
    await page.addInitScript(isFixture => {
        if (window === parent) window.fixtureRuns = [];
        if (isFixture) {
            const device = { lost: new Promise(() => {}), addEventListener() {}, destroy() { window.gpuDestroyed = true; } };
            Object.defineProperty(navigator, 'gpu', { value: { requestAdapter: async () => ({ features: new Set(), requestDevice: async () => device }) } });
            Object.defineProperty(navigator, 'xr', { value: { requestSession: async () => {
                if (window.deferXR) await new Promise(resolve => { window.resolveXR = resolve; });
                return { addEventListener() {}, end: async () => { window.xrEnds = (window.xrEnds || 0) + 1; } };
            } } });
        }
    }, fixture);
    await page.goto(url);
    const list = page.locator('#selection');
    const back = page.locator('#return-to-list');
    const button = id => page.locator(`[data-game="${id}"]`);
    async function ready(id) {
        await page.locator('.game-frame').waitFor();
        const frame = await page.locator('.game-frame').elementHandle().then(handle => handle.contentFrame());
        await frame.locator('#status').waitFor({ state: 'hidden', timeout: 60000 });
        assert.equal(await page.locator('.game-frame').count(), 1);
        assert.ok(page.url().endsWith(`#game=${id}`));
        return frame;
    }
    async function returned(id) {
        await list.waitFor({ state: 'visible' });
        assert.equal(await page.locator('.game-frame').count(), 0);
        assert.equal(await button(id).evaluate(element => element === document.activeElement), true);
        if (fixture) {
            assert.equal(await page.evaluate(() => fixtureRuns.every(run => run.shutdowns === 1 && run.pauses === 1 && run.context.state === 'closed')), true);
            const ticks = await page.evaluate(() => fixtureRuns.map(run => run.ticks));
            await page.waitForTimeout(75);
            assert.deepEqual(await page.evaluate(() => fixtureRuns.map(run => run.ticks)), ticks);
        }
    }
    assert.equal(await page.locator('#game-list button').count(), 13);
    assert.equal(new Set(games.map(game => game.id)).size, 13);
    assert.equal(await page.locator('iframe').count(), 0, 'no runtime boots before selection');
    await button('tumiki').focus();
    await page.keyboard.press('ArrowRight');
    assert.equal(await button('parsec47').evaluate(element => element === document.activeElement), true);
    await page.keyboard.press('End');
    assert.equal(await button('masashikun-hi').evaluate(element => element === document.activeElement), true);
    await page.keyboard.press('Home');
    for (const game of games) {
        await button(game.id).click();
        const frame = await ready(game.id);
        await frame.locator('#canvas').click();
        if (fixture) {
            await page.waitForTimeout(40);
            assert.ok(await frame.evaluate(() => fixtureStats.ticks > 0));
            await page.evaluate(() => window.oldGame = document.querySelector('iframe').contentWindow);
        }
        await back.click();
        await returned(game.id);
        if (fixture) {
            assert.equal(await page.evaluate(() => oldGame.abaGame.disposed && oldGame.gpuDestroyed), true);
            const key = await frame.evaluate(() => '').catch(() => null); // Detached realms cannot run new input.
            assert.equal(key, null);
        }
    }
    // Repeated same-game launch starts a fresh realm; no Module/audio/input state is reused.
    await button('tumiki').click();
    let frame = await ready('tumiki');
    await frame.locator('#canvas').press('Alt+Escape');
    await returned('tumiki');
    await button('tumiki').click();
    await ready('tumiki');
    await page.goBack();
    await returned('tumiki');
    await page.goForward();
    await ready('tumiki');
    await back.click();
    await returned('tumiki');
    // In-game quit messages use the outer list instead of loading a nested selector.
    await button('wok').click();
    frame = await ready('wok');
    await frame.evaluate(() => lubHost.onMessage('quit', new Uint8Array()));
    await returned('wok');
    if (fixture) {
        // A live immersive session and a late permission resolution both end on disposal.
        await button('torus-trooper').click();
        frame = await ready('torus-trooper');
        await frame.evaluate(() => navigator.xr.requestSession('immersive-vr'));
        await page.evaluate(() => window.oldGame = document.querySelector('iframe').contentWindow);
        await back.click();
        await returned('torus-trooper');
        assert.equal(await page.evaluate(() => oldGame.xrEnds), 1);
        await button('torus-trooper').click();
        frame = await ready('torus-trooper');
        await frame.evaluate(() => { window.deferXR = true; window.pendingXR = navigator.xr.requestSession('immersive-vr').catch(() => {}); });
        await page.evaluate(() => window.oldGame = document.querySelector('iframe').contentWindow);
        await back.click();
        await returned('torus-trooper');
        await page.evaluate(async () => { oldGame.resolveXR(); await oldGame.pendingXR; });
        assert.equal(await page.evaluate(() => oldGame.xrEnds), 1);
        // Boot interrupted while asset fetch is pending never appends a Wasm runtime.
        let release;
        const held = new Promise(resolve => { release = resolve; });
        await page.route('**/assets.json', async route => { await held; await route.fulfill({ body: '[]', contentType: 'application/json' }).catch(() => {}); });
        const count = await page.evaluate(() => fixtureRuns.length);
        await button('parsec47').click();
        await page.frameLocator('iframe').locator('#game-return').waitFor();
        await back.click();
        await returned('parsec47');
        release();
        await page.unroute('**/assets.json');
        assert.equal(await page.evaluate(() => fixtureRuns.length), count);
        // Same-origin spoofed messages from the parent itself are ignored.
        await button('tumiki').click();
        await ready('tumiki');
        await page.evaluate(() => window.postMessage({ type: 'aba:return' }, location.origin));
        await page.waitForTimeout(50);
        assert.equal(await page.locator('iframe').count(), 1);
        await back.click();
        await returned('tumiki');
        // Saves remain isolated under original keys, including on_quit direct FS writes.
        const saved = await page.evaluate(() => Object.fromEntries(Object.entries(localStorage)));
        assert.equal(saved['tumiki-scores-v1'], 'fixture-save-TUMIKI Fighters');
        assert.equal(saved['torus-trooper-scores-v1'], 'fixture-save-Torus Trooper');
        assert.equal(saved['torus-trooper-replay-v1'], 'fixture-save-Torus Trooper');
        assert.equal(saved['gear-toy-gear-scores-v1'], 'fixture-save-GearToyGear');
        // Controller selection uses edges/repeat; held A on return cannot relaunch.
        await page.evaluate(() => {
            window.pad = { mapping: 'standard', axes: [0, 0], buttons: Array.from({ length: 16 }, () => ({ pressed: false })) };
            navigator.getGamepads = () => [pad];
        });
        await button('tumiki').focus();
        await page.waitForTimeout(50);
        await page.evaluate(() => { pad.buttons[15].pressed = true; });
        await page.waitForTimeout(50);
        assert.equal(await button('parsec47').evaluate(element => element === document.activeElement), true);
        await page.evaluate(() => { pad.buttons[15].pressed = false; pad.buttons[0].pressed = true; });
        await ready('parsec47');
        await back.click();
        await returned('parsec47');
        await page.waitForTimeout(100);
        assert.equal(await page.locator('iframe').count(), 0);
        await page.evaluate(() => { pad.buttons[0].pressed = false; });
    }
    await mkdir('build/screenshots', { recursive: true });
    await page.screenshot({ path: 'build/screenshots/game-selection.png' });
    await page.setViewportSize({ width: 375, height: 812 });
    assert.equal(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), true);
    await page.screenshot({ path: 'build/screenshots/game-selection-mobile.png' });
    assert.deepEqual(errors, []);
    console.log(`PASS: all 13 ${fixture ? 'fixture' : 'built'} games launch/return, keyboard/gamepad, history, quit, repeated lifecycle, save isolation, teardown and mobile width`);
} finally {
    await browser.close();
    server?.close();
}
