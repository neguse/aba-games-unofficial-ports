import assert from 'node:assert/strict';
import { mkdir } from 'node:fs/promises';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';

const { chromium } = await import(pathToFileURL(resolve(process.env.PLAYWRIGHT_MODULE || '.cache/browser/node_modules/playwright/index.mjs')));
const url = process.argv[2] || 'http://127.0.0.1:8765';
const browser = await chromium.launch({
    executablePath: process.env.CHROMIUM_PATH,
    headless: true,
    args: ['--no-sandbox', '--enable-unsafe-webgpu', '--use-angle=swiftshader', '--enable-features=Vulkan,WebGPU',
        '--use-vulkan=swiftshader', '--disable-vulkan-fallback-to-gl-for-testing'],
});
const errors = [];
try {
    await mkdir('build/screenshots', { recursive: true });
    const page = await browser.newPage({ viewport: { width: 960, height: 850 } });
    page.on('pageerror', error => errors.push(error.message));
    page.on('console', message => { if (message.type() === 'error') errors.push(message.text()); });
    await page.addInitScript(() => {
        window.audioStarts = 0;
        const NativeAudioContext = window.AudioContext;
        window.AudioContext = class extends NativeAudioContext {
            createBufferSource() {
                const source = super.createBufferSource();
                const start = source.start.bind(source);
                source.start = (...args) => { window.audioStarts++; return start(...args); };
                return source;
            }
        };
    });
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
    async function command(id, value = 0) {
        await page.evaluate(payload => lubHost.queue.push({ topic: 'test', payload }), `${id},${value}`);
    }
    await page.goto(url);
    await observe();
    await page.waitForFunction(() => gameState[0] === 4);
    await page.screenshot({ path: 'build/screenshots/title.png' });
    await page.waitForTimeout(350);
    await page.keyboard.down('z');
    await page.waitForFunction(() => gameState[0] === 1, null, { timeout: 30000 });
    await page.keyboard.up('z');
    const x = await page.evaluate(() => gameState[3]);
    await page.keyboard.down('ArrowRight');
    await page.waitForFunction(x => gameState[3] > x + 0.5, x);
    await page.keyboard.up('ArrowRight');
    await page.keyboard.down('x');
    await page.waitForFunction(() => gameState[5] === 16);
    await page.keyboard.up('x');
    await page.waitForFunction(() => gameState[5] === 0);
    await page.keyboard.down('p');
    await page.waitForFunction(() => gameState[0] === 3);
    await page.keyboard.up('p');
    const paused = await page.evaluate(() => gameState.slice(2, 5));
    await page.waitForTimeout(400);
    assert.deepEqual(await page.evaluate(() => gameState.slice(2, 5)), paused);
    await page.keyboard.down('p');
    await page.waitForFunction(() => gameState[0] === 1);
    await page.keyboard.up('p');
    assert.ok(await page.evaluate(() => audioStarts > 0), 'game audio must start');
    await page.locator('#sound').click();
    await page.locator('#sound').click();
    await page.keyboard.down('ArrowRight');
    await page.evaluate(() => window.dispatchEvent(new Event('blur')));
    await page.keyboard.up('ArrowRight');
    for (let stage = 0; stage < 5; stage++) {
        await command(0, stage);
        await page.waitForFunction(stage => gameState[1] === stage && gameState[0] === 1, stage);
        await page.waitForTimeout(800);
        await page.screenshot({ path: `build/screenshots/stage-${stage + 1}.png` });
    }
    await command(1);
    await page.waitForFunction(() => localStorage.getItem('tumiki-scores-v1')?.startsWith('1234567,'));
    await page.reload();
    await observe();
    await page.waitForFunction(() => gameState[7] === 1234567);
    const decoded = await page.evaluate(async () => {
        const context = new AudioContext();
        const music = ['we_are_tumiki_fighters', 'just_over_the_horizon', 'panic_on_meadow', 'here_comes_a_gigantic_toy', 'battle_over_the_junk_city', 'return_to_home'];
        const effects = ['ship_shot', 'stuck', 'stuck_bonus', 'stuck_destroyed', 'ship_destroyed', 'enemy_damaged', 'small_enemy_destroyed', 'enemy_destroyed', 'boss_destroyed', 'extend', 'warning', 'propeller', 'stuck_bonus_pushin'];
        const files = [...music.map(name => `${name}.ogg`), ...effects.map(name => `${name}.wav`)];
        const durations = await Promise.all(files.map(async name => {
            const response = await fetch(`audio/${name}`);
            if (!response.ok) throw new Error(name);
            return (await context.decodeAudioData(await response.arrayBuffer())).duration;
        }));
        await context.close();
        return durations;
    });
    assert.equal(decoded.length, 19);
    assert.ok(decoded.every(duration => Number.isFinite(duration) && duration > 0));
    assert.deepEqual(errors, []);
    console.log('PASS: keyboard, retract, pause/resume, five stage renders, audio playback/19 decodes, score reload; no browser errors');
} finally {
    await browser.close();
}
