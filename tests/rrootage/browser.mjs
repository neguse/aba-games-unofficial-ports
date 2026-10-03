import assert from 'node:assert/strict';
import { mkdir } from 'node:fs/promises';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
const { chromium } = await import(pathToFileURL(resolve(process.env.PLAYWRIGHT_MODULE || '.cache/browser/node_modules/playwright/index.mjs')));
const url = process.argv[2] || 'http://127.0.0.1:8765/rrootage/';
const browser = await chromium.launch({ executablePath: process.env.CHROMIUM_PATH, headless: true,
    args: ['--no-sandbox', '--enable-unsafe-webgpu', '--use-angle=swiftshader', '--enable-features=Vulkan,WebGPU', '--use-vulkan=swiftshader', '--disable-vulkan-fallback-to-gl-for-testing'] });
const errors = [];
try {
    await mkdir('build/screenshots', { recursive: true });
    const page = await browser.newPage({ viewport: { width: 960, height: 960 } });
    page.on('pageerror', error => errors.push(error.message));
    page.on('console', message => { if (message.type() === 'error') errors.push(message.text()); });
    await page.addInitScript(() => {
        window.audioStarts = 0;
        const Native = window.AudioContext;
        window.AudioContext = class extends Native {
            createBufferSource() {
                const source = super.createBufferSource(); const start = source.start.bind(source);
                source.start = (...args) => { window.audioStarts++; return start(...args); }; return source;
            }
        };
    });
    // test commands: 0 finish
    async function command(id, value = 0) { await page.evaluate(payload => lubHost.queue.push({ topic: 'test', payload }), `${id},${value}`); }
    async function observe() {
        await page.locator('#status').waitFor({ state: 'hidden', timeout: 45000 });
        await page.evaluate(() => {
            const original = lubHost.onMessage;
            lubHost.onMessage = (topic, bytes) => {
                if (topic === 'test.state') window.gameState = new TextDecoder().decode(bytes).split(',').map(Number);
                else original(topic, bytes);
            };
        });
        await page.waitForFunction(() => window.gameState);
    }
    async function press(key) { await page.keyboard.down(key); await page.waitForTimeout(150); await page.keyboard.up(key); await page.waitForTimeout(200); }
    await page.goto(url); await observe();
    await page.screenshot({ path: 'build/screenshots/rrootage-title.png' });
    await press('ArrowRight'); await page.waitForFunction(() => gameState[2] === 1);
    for (let mode = 0; mode < 4; mode++) {
        if (mode > 0) await press('x');
        await page.waitForFunction(mode => gameState[1] === mode, mode);
        await press('z'); await page.waitForFunction(() => gameState[0] === 1);
        const x = await page.evaluate(() => gameState[3]);
        await page.keyboard.down('ArrowRight'); await page.waitForFunction(x => gameState[3] > x + 2000, x); await page.keyboard.up('ArrowRight');
        await page.keyboard.down('z'); await page.waitForFunction(() => gameState[4] > 3); await page.keyboard.up('z');
        await page.waitForFunction(() => gameState[9] > 160);
        await page.keyboard.down('x');
        if (mode === 0) await page.waitForFunction(() => gameState[5] < 3);
        if (mode === 1) await page.waitForFunction(() => gameState[6] > 0);
        if (mode === 2) await page.waitForFunction(() => gameState[7] === 1);
        if (mode === 3) await page.waitForFunction(() => gameState[8] > 0);
        await page.keyboard.up('x');
        await press('p'); await page.waitForFunction(() => gameState[0] === 4);
        const tick = await page.evaluate(() => gameState[9]); await page.waitForTimeout(200);
        assert.equal(await page.evaluate(() => gameState[9]), tick);
        const screenshot = await page.locator('#canvas').screenshot({ path: `build/screenshots/rrootage-mode-${mode}.png` });
        const colored = await page.evaluate(async png => {
            const image = new Image(); image.src = 'data:image/png;base64,' + png; await image.decode();
            const copy = document.createElement('canvas'); copy.width = image.width; copy.height = image.height;
            const context = copy.getContext('2d'); context.drawImage(image, 0, 0);
            const pixels = context.getImageData(160, 0, 320, 480).data;
            let count = 0;
            for (let i = 0; i < pixels.length; i += 4) if (Math.max(pixels[i],pixels[i+1],pixels[i+2]) > 130) count++;
            return count;
        }, screenshot.toString('base64'));
        assert.ok(colored > 500, `gameplay pixels mode ${mode}: ${colored}`);
        await press('p'); await page.waitForFunction(() => gameState[0] === 1);
        if (mode < 3) { await press('Escape'); await page.waitForFunction(() => gameState[0] === 0); }
    }
    assert.ok(await page.evaluate(() => audioStarts > 0));
    await command(0);
    await page.waitForFunction(() => localStorage.getItem('rrootage-scores-v1')?.includes('7654321'));
    await page.reload(); await observe();
    await page.waitForFunction(() => gameState[0] === 0 && gameState[1] === 3 && gameState[2] === 1 && gameState[11] === 7654321 && gameState[12] === 1);
    const count = await page.evaluate(async () => {
        const config = JSON.parse(document.querySelector('#game-config').textContent); const audio = new AudioContext();
        const names = [...config.music.map(n => `${n}.ogg`), ...config.sounds.map(n => `${n}.wav`)];
        for (const name of names) {
            const response = await fetch(`audio/${name}`); if (!response.ok) throw new Error(name);
            const decoded = await audio.decodeAudioData(await response.arrayBuffer()); if (!(decoded.duration > 0)) throw new Error(name);
        }
        await audio.close(); return names.length;
    });
    assert.equal(count, 19); assert.deepEqual(errors, []);
    console.log('PASS: four modes, selection, movement, laser/bomb/rolling/polarity/reflector, rendering, pause, audio/19 decodes, score/clear reload');
} finally { await browser.close(); }
