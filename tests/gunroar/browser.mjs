import assert from 'node:assert/strict';
import { mkdir } from 'node:fs/promises';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
const { chromium } = await import(pathToFileURL(resolve(process.env.PLAYWRIGHT_MODULE || '.cache/browser/node_modules/playwright/index.mjs')));
const url = process.argv[2] || 'http://127.0.0.1:8765/gunroar/';
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
    // BrowserHooks.Command ids: 0 = finish
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
    async function press(key) { await page.keyboard.down(key); await page.waitForTimeout(180); await page.keyboard.up(key); await page.waitForTimeout(220); }
    await page.goto(url); await observe();
    await page.evaluate(() => lubHost.queue.push({ topic: 'seed', payload: '12345' }));
    await page.screenshot({ path: 'build/screenshots/gunroar-title.png' });
    for (let mode = 0; mode < 4; mode++) {
        if (mode) { await press('x'); await page.waitForFunction(mode => gameState[1] === mode, mode); }
        await press('z'); await page.waitForFunction(mode => gameState[0] === 1 && gameState[2] === mode, mode);
        assert.equal(await page.evaluate(() => gameState[3]), mode === 2 ? 2 : 1);
        await page.waitForTimeout(2000);
        const x = await page.evaluate(() => gameState[4]);
        await page.keyboard.down('d'); await page.waitForFunction(x => gameState[4] > x + 0.5, x); await page.keyboard.up('d');
        if (mode === 0) {
            await page.keyboard.down('z'); await page.waitForFunction(() => gameState[6] > 0);
            await page.keyboard.down('x'); await page.waitForFunction(() => gameState[10] > 0); await page.keyboard.up('x');
        } else if (mode === 1) {
            await page.keyboard.down('i'); await page.waitForFunction(() => gameState[6] > 0);
        } else if (mode === 2) {
            const second = await page.evaluate(() => gameState[5]);
            await page.keyboard.down('a'); await page.keyboard.down('l');
            await page.waitForFunction(second => gameState[5] > second + 0.5 && gameState[6] > 0, second);
        } else {
            const rect = await page.locator('#canvas').boundingBox();
            await page.mouse.move(rect.x + rect.width * 0.7, rect.y + rect.height * 0.3);
            await page.mouse.down(); await page.waitForFunction(() => gameState[6] > 0); await page.mouse.up();
            await page.mouse.down({ button: 'right' }); await page.waitForFunction(() => gameState[9] > 0.2);
        }
        await press('p'); await page.waitForFunction(() => gameState[7] > 0);
        const time = await page.evaluate(() => gameState[8]); await page.waitForTimeout(200);
        assert.equal(await page.evaluate(() => gameState[8]), time);
        await page.screenshot({ path: `build/screenshots/gunroar-mode${mode}.png` });
        await press('p');
        for (const key of ['z', 'i', 'a', 'l']) await page.keyboard.up(key);
        await page.mouse.up({ button: 'right' });
        if (mode < 3) { await press('Escape'); await page.waitForFunction(() => gameState[0] === 0); }
    }
    assert.ok(await page.evaluate(() => audioStarts > 0));
    await page.evaluate(() => lubHost.queue.push({ topic: 'test', payload: '0,0' }));
    await page.waitForFunction(() => localStorage.getItem('gunroar-scores-v1')?.includes('7654321') && localStorage.getItem('gunroar-replay-v1'));
    await page.reload(); await observe();
    await page.waitForFunction(() => gameState[12] === 7654321 && gameState[1] === 3 && gameState[11] === 1);
    const count = await page.evaluate(async () => {
        const config = JSON.parse(document.querySelector('#game-config').textContent); const audio = new AudioContext();
        const names = [...config.music.map(n => `${n}.ogg`), ...config.sounds.map(n => `${n}.wav`)];
        for (const name of names) {
            const response = await fetch(`audio/${name}`); if (!response.ok) throw new Error(name);
            const decoded = await audio.decodeAudioData(await response.arrayBuffer()); if (!(decoded.duration > 0)) throw new Error(name);
        }
        await audio.close(); return names.length;
    });
    assert.equal(count, 14); assert.deepEqual(errors, []);
    console.log('PASS: four modes, movement, guns/lance, mouse spread, pause, audio/14 decodes, score and replay reload');
} finally { await browser.close(); }
