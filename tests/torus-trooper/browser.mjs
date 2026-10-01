import assert from 'node:assert/strict';
import { mkdir } from 'node:fs/promises';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
const { chromium } = await import(pathToFileURL(resolve(process.env.PLAYWRIGHT_MODULE || '.cache/browser/node_modules/playwright/index.mjs')));
const url = process.argv[2] || 'http://127.0.0.1:8765/torus-trooper/';
const browser = await chromium.launch({ executablePath: process.env.CHROMIUM_PATH, headless: true,
    args: process.platform === 'win32' ? ['--enable-unsafe-webgpu', '--use-angle=d3d11'] : ['--no-sandbox', '--enable-unsafe-webgpu', '--use-angle=swiftshader', '--enable-features=Vulkan,WebGPU', '--use-vulkan=swiftshader', '--disable-vulkan-fallback-to-gl-for-testing'] });
const errors = [];
try {
    await mkdir('build/screenshots', { recursive: true });
    const page = await browser.newPage({ viewport: { width: 960, height: 960 } });
    page.on('pageerror', error => errors.push(error.message));
    page.on('console', message => { if (message.type() === 'error') errors.push(message.text()); });
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
    await page.screenshot({ path: 'build/screenshots/torus-title.png' });
    for (let grade = 0; grade < 3; grade++) {
        if (grade > 0) await press('ArrowRight');
        await page.waitForFunction(grade => gameState[1] === grade, grade);
        await press('z'); await page.waitForFunction(grade => gameState[0] === 1 && gameState[2] === grade, grade);
        await page.keyboard.down('ArrowUp'); await page.waitForFunction(() => gameState[10] > 0.15); await page.keyboard.up('ArrowUp');
        const x = await page.evaluate(() => gameState[3]);
        await page.keyboard.down('ArrowRight'); await page.waitForFunction(x => Math.abs(gameState[3] - x) > 0.05, x); await page.keyboard.up('ArrowRight');
        await page.keyboard.down('z'); await page.waitForFunction(() => gameState[4] > 0); await page.keyboard.up('z');
        await page.keyboard.down('x'); await page.waitForFunction(() => gameState[7] >= 30); await page.keyboard.up('x');
        await page.waitForFunction(() => gameState[7] === 0 && gameState[4] > 0);
        await press('p'); await page.waitForFunction(() => gameState[5] > 0);
        const time = await page.evaluate(() => gameState[6]); await page.waitForTimeout(200);
        assert.equal(await page.evaluate(() => gameState[6]), time);
        const screenshot = await page.locator('#canvas').screenshot({ path: `build/screenshots/torus-grade-${grade}.png` });
        const colored = await page.evaluate(async png => {
            const image = new Image(); image.src = 'data:image/png;base64,' + png; await image.decode();
            const copy = document.createElement('canvas'); copy.width = image.width; copy.height = image.height;
            const context = copy.getContext('2d'); context.drawImage(image, 0, 0);
            const pixels = context.getImageData(80, 100, 480, 300).data;
            let count = 0;
            for (let i = 0; i < pixels.length; i += 4) if (Math.max(pixels[i],pixels[i+1],pixels[i+2]) > 30) count++;
            return count;
        }, screenshot.toString('base64'));
        assert.ok(colored > 3000, `course pixels grade ${grade}: ${colored}`);
        await press('p'); await page.waitForFunction(() => gameState[5] === 0);
        if (grade < 2) { await press('Escape'); await page.waitForFunction(() => gameState[0] === 0); }
    }
    assert.ok(await page.evaluate(() => window.miniaudio?.devices.some(device => device?.webaudio.state === 'running')));
    await page.evaluate(() => lubHost.queue.push({ topic: 'test.finish', payload: '' }));
    await page.waitForFunction(() => localStorage.getItem('torus-trooper-scores-v1')?.includes('7654321'));
    await page.waitForFunction(() => gameState[13] > 65); await press('z');
    await page.waitForFunction(() => gameState[0] === 0 && !!localStorage.getItem('torus-trooper-replay-v1'));
    await page.reload(); await observe();
    await page.waitForFunction(() => gameState[9] === 7654321 && gameState[8] === 1);
    await press('x'); await page.waitForFunction(() => gameState[12] === 1);
    await press('ArrowRight'); await page.waitForFunction(() => gameState[14] === 0);
    await press('ArrowLeft'); await page.waitForFunction(() => gameState[14] === 1);
    await press('ArrowDown'); await page.waitForFunction(() => gameState[15] === 0);
    await press('ArrowUp'); await page.waitForFunction(() => gameState[15] === 1);
    await page.screenshot({ path: 'build/screenshots/torus-replay.png' });
    const count = await page.evaluate(async () => {
        const config = JSON.parse(document.querySelector('#game-config').textContent); const audio = new AudioContext();
        const names = [...config.music.map(n => `${n}.wav`), ...config.sounds.map(n => `${n}.wav`)];
        for (const name of names) {
            const response = await fetch(`audio/${name}`); if (!response.ok) throw new Error(name);
            const decoded = await audio.decodeAudioData(await response.arrayBuffer()); if (!(decoded.duration > 0)) throw new Error(name);
        }
        await audio.close(); return names.length;
    });
    assert.equal(count, 14); assert.deepEqual(errors, []);
    console.log('PASS: three grades, acceleration, movement, shot/charge, course rendering, pause, audio/14 decodes, score/replay reload');
} finally { await browser.close(); }
