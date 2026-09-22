import assert from 'node:assert/strict';
import { mkdir } from 'node:fs/promises';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
const { chromium } = await import(pathToFileURL(resolve(process.env.PLAYWRIGHT_MODULE || '.cache/browser/node_modules/playwright/index.mjs')));
const url = process.argv[2] || 'http://127.0.0.1:8765/a7xpg/';
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
    await page.route('**/game.lua', async route => {
        const response = await route.fetch(); let code = await response.text();
        assert.match(code, /return Game\s*$/);
        code = code.replace('if topic == "scores" then', `if topic == "test.finish" then
 local g=Game.manager; g.score=7654321; g:start_gameover()
end
if topic == "test.gold" then
 local g=Game.manager; local gold=g.golds.actor[16]; gold.is_exist=true; gold.pos.x=g.ship.pos.x+math.sin(g.ship.deg)*0.4; gold.pos.y=g.ship.pos.y+math.cos(g.ship.deg)*0.4
end
if topic == "test.invincible" then Game.manager.ship.gauge=201 end
if topic == "scores" then`);
        code = code.replace(/return Game\s*$/, `local frame=Game.on_frame
function Game.on_frame(dt)
 frame(dt)
 local g=Game.manager
 lub.host.send('test.state',table.concat({g.state,g.stage,g.ship.pos.x,g.ship.pos.y,g.ship.speed,g.stage_timer,g.left_gold,g.ship.invincible and 1 or 0,g.pref_manager.hi_score},','))
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
    async function press(key) { await page.keyboard.down(key); await page.waitForTimeout(180); await page.keyboard.up(key); await page.waitForTimeout(220); }
    await page.goto(url); await observe();
    await page.evaluate(() => lubHost.queue.push({ topic: 'seed', payload: '12345' }));
    await page.screenshot({ path: 'build/screenshots/a7xpg-title.png' });
    await press('z'); await page.waitForFunction(() => gameState[0] === 1);
    await page.waitForTimeout(1200);
    const x = await page.evaluate(() => gameState[2]);
    await page.keyboard.down('ArrowRight'); await page.waitForFunction(x => gameState[2] > x + 0.5, x); await page.keyboard.up('ArrowRight');
    await page.keyboard.down('ArrowDown'); await page.keyboard.down('x'); await page.waitForFunction(() => gameState[4] > 0.5);
    await page.keyboard.up('x'); await page.keyboard.up('ArrowDown');
    const gold = await page.evaluate(() => gameState[6]);
    await page.evaluate(() => lubHost.queue.push({ topic: 'test.gold', payload: '' }));
    await page.waitForFunction(gold => gameState[6] < gold, gold);
    await page.evaluate(() => lubHost.queue.push({ topic: 'test.invincible', payload: '' }));
    await page.waitForFunction(() => gameState[7] === 1);
    await press('p'); await page.waitForFunction(() => gameState[0] === 4);
    const time = await page.evaluate(() => gameState[5]); await page.waitForTimeout(200);
    assert.equal(await page.evaluate(() => gameState[5]), time);
    const screenshot = await page.locator('#canvas').screenshot({ path: 'build/screenshots/a7xpg-game.png' });
    const colored = await page.evaluate(async png => {
        const image = new Image(); image.src = 'data:image/png;base64,' + png; await image.decode();
        const copy = document.createElement('canvas'); copy.width = image.width; copy.height = image.height;
        const context = copy.getContext('2d'); context.drawImage(image, 0, 0);
        const pixels = context.getImageData(0, 0, copy.width, copy.height).data;
        let count = 0;
        for (let i = 0; i < pixels.length; i += 4) if (Math.max(pixels[i], pixels[i+1], pixels[i+2]) - Math.min(pixels[i], pixels[i+1], pixels[i+2]) > 40) count++;
        return count;
    }, screenshot.toString('base64'));
    assert.ok(colored > 5000, `colored field and glow pixels: ${colored}`);
    await press('p'); await page.waitForFunction(() => gameState[0] === 1);
    assert.ok(await page.evaluate(() => audioStarts > 0));
    await page.evaluate(() => lubHost.queue.push({ topic: 'test.finish', payload: '' }));
    await page.waitForFunction(() => localStorage.getItem('a7xpg-scores-v1') === '7654321');
    await page.reload(); await observe();
    await page.waitForFunction(() => gameState[8] === 7654321);
    const count = await page.evaluate(async () => {
        const config = JSON.parse(document.querySelector('#game-config').textContent); const audio = new AudioContext();
        const names = [...config.music.map(n => `${n}.ogg`), ...config.sounds.map(n => `${n}.wav`)];
        for (const name of names) {
            const response = await fetch(`audio/${name}`); if (!response.ok) throw new Error(name);
            const decoded = await audio.decodeAudioData(await response.arrayBuffer()); if (!(decoded.duration > 0)) throw new Error(name);
        }
        await audio.close(); return names.length;
    });
    assert.equal(count, 15); assert.deepEqual(errors, []);
    console.log('PASS: movement, boost, gold, invulnerability, pause, audio/15 decodes, high score reload');
} finally { await browser.close(); }
