import assert from 'node:assert/strict';
import { mkdir } from 'node:fs/promises';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';

const { chromium } = await import(pathToFileURL(resolve(process.env.PLAYWRIGHT_MODULE || '.cache/browser/node_modules/playwright/index.mjs')));
const url = process.argv[2] || 'http://127.0.0.1:8765/parsec47/';
const browser = await chromium.launch({ executablePath: process.env.CHROMIUM_PATH, headless: true,
    args: ['--no-sandbox', '--enable-unsafe-webgpu', '--use-angle=swiftshader', '--enable-features=Vulkan,WebGPU',
        '--use-vulkan=swiftshader', '--disable-vulkan-fallback-to-gl-for-testing'] });
const errors = [];
try {
    await mkdir('build/screenshots', { recursive: true });
    const page = await browser.newPage({ viewport: { width: 960, height: 850 } });
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
    await page.route('**/game.lua', async route => {
        const response = await route.fetch(); let code = await response.text();
        assert.match(code, /return Game\s*$/);
        code = code.replace('if topic == "scores" then', `if topic == "test.progress" then
 Game.manager.ship.cnt = -30
 for i = 1, 900 do Game.manager.ship.cnt = -30; Game.manager:move() end
end
if topic == "test.gameover" then Game.manager.score = 1234567; Game.manager:start_gameover() end
if topic == "scores" then`);
        code = code.replace(/return Game\s*$/, `local frame = Game.on_frame
local renderFrames = 0
function Game.on_frame(dt)
 frame(dt)
 renderFrames = renderFrames + 1
 local g = Game.manager
 lub.host.send('test.state', table.concat({g.state,g.mode,g.title.mode,g.stage_manager.parsec,g.ship.pos.x,g.ship.roll_lock_cnt,g.cnt,g.pref_manager.hi_score[2][2][1],renderFrames}, ','))
end
return Game`);
        await route.fulfill({ response, body: code });
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
    async function press(key) {
        await page.keyboard.down(key);
        let frame = await page.evaluate(() => gameState[8]);
        await page.waitForFunction(frame => gameState[8] >= frame + 2, frame);
        await page.keyboard.up(key);
        frame = await page.evaluate(() => gameState[8]);
        await page.waitForFunction(frame => gameState[8] >= frame + 2, frame);
    }
    await page.goto(url); await observe();
    await page.waitForFunction(() => gameState[0] === 0);
    await page.screenshot({ path: 'build/screenshots/parsec47-title.png' });
    for (let mode = 0; mode < 2; mode++) {
        await page.waitForFunction(() => gameState[0] === 0 && gameState[6] > 9);
        if (mode) { await press('x'); await page.waitForFunction(() => gameState[2] === 1); }
        await press('z'); await page.waitForFunction(mode => gameState[0] === 1 && gameState[1] === mode, mode);
        await page.waitForTimeout(600);
        const x = await page.evaluate(() => gameState[4]);
        await page.keyboard.down('ArrowRight'); await page.waitForFunction(x => gameState[4] > x + 0.5, x); await page.keyboard.up('ArrowRight');
        await page.keyboard.down('x'); await page.waitForFunction(() => gameState[5] >= 20); await page.keyboard.up('x');
        await press('p'); await page.waitForFunction(() => gameState[0] === 3);
        const pos = await page.evaluate(() => gameState[4]); await page.waitForTimeout(300);
        assert.equal(await page.evaluate(() => gameState[4]), pos);
        await press('p'); await page.waitForFunction(() => gameState[0] === 1);
        await page.keyboard.down('z');
        await page.evaluate(() => lubHost.queue.push({ topic: 'test.progress', payload: '' }));
        await page.waitForFunction(() => gameState[3] >= 2);
        await page.screenshot({ path: `build/screenshots/parsec47-${mode ? 'lock' : 'roll'}.png` });
        await page.keyboard.up('z');
        if (!mode) { await press('Escape'); await page.waitForFunction(() => gameState[0] === 0); }
    }
    assert.ok(await page.evaluate(() => audioStarts > 0));
    await page.evaluate(() => lubHost.queue.push({ topic: 'test.gameover', payload: '' }));
    await page.waitForFunction(() => localStorage.getItem('parsec47-scores-v1')?.includes('1234567'));
    await page.reload(); await observe();
    await page.waitForFunction(() => gameState[7] === 1234567 && gameState[1] === 1);
    const count = await page.evaluate(async () => {
        const config = JSON.parse(document.querySelector('#game-config').textContent);
        const audio = new AudioContext();
        const names = [...config.music.map(n => `${n}.ogg`), ...config.sounds.map(n => `${n}.wav`)];
        for (const name of names) {
            const response = await fetch(`audio/${name}`);
            if (!response.ok) throw new Error(name);
            const decoded = await audio.decodeAudioData(await response.arrayBuffer());
            if (!(decoded.duration > 0)) throw new Error(name);
        }
        await audio.close(); return names.length;
    });
    assert.equal(count, 15); assert.deepEqual(errors, []);
    console.log('PASS: ROLL/LOCK, movement, special attack, pause, progress, audio/15 decodes, score and mode reload');
} finally { await browser.close(); }
