import assert from 'node:assert/strict';
import { mkdir } from 'node:fs/promises';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
const { chromium } = await import(pathToFileURL(resolve(process.env.PLAYWRIGHT_MODULE || '.cache/browser/node_modules/playwright/index.mjs')));
const url = process.argv[2] || 'http://127.0.0.1:8765/titanion/';
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
 local g=Game.manager
 g.game_state.score=7654321; g.game_state.left=0; g.game_state:destroyed_player()
 g:save_last_replay(); g:start_title()
end
if topic == "scores" then`);
        code = code.replace(/return Game\s*$/, `local frame=Game.on_frame
function Game.on_frame(dt)
 frame(dt)
 local g=Game.manager
 local shots=0
 for _,s in ipairs(g.player_spec.shots.actors) do if s.exists then shots=shots+1 end end
 lub.host.send('test.state',table.concat({g.game_state:is_in_game() and 1 or 0,g.title.cursor_idx,g.game_state:mode_0(),g.player.state.pos.x,shots,g.game_state:paused() and 1 or 0,g.stage.phase_time,g.player_spec.tractor_beam.length,g.player.state.is_invincible and 1 or 0,g.player.state.replay_mode and 1 or 0,g.preference.high_score[3][1],g.player.state.captured_enemy_width},','))
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
    await page.screenshot({ path: 'build/screenshots/titanion-title.png' });
    for (let mode = 0; mode < 3; mode++) {
        await press('ArrowDown'); await page.waitForFunction(mode => gameState[1] === mode, mode);
        await press('z'); await page.waitForFunction(mode => gameState[0] === 1 && gameState[2] === mode, mode);
        await page.waitForTimeout(1200);
        const x = await page.evaluate(() => gameState[3]);
        await page.keyboard.down('d'); await page.waitForFunction(x => gameState[3] > x + 0.5, x); await page.keyboard.up('d');
        await page.keyboard.down('z'); await page.waitForFunction(() => gameState[4] > 0); await page.keyboard.up('z');
        await page.keyboard.down('x'); await page.waitForFunction(() => gameState[7] > 0.5);
        if (mode === 0) assert.equal(await page.evaluate(() => gameState[8]), 1);
        if (mode === 2) {
            await page.keyboard.down('z'); await page.waitForFunction(() => gameState[4] > 0 && gameState[11] < 0.9);
            await page.keyboard.up('z');
        }
        await press('p'); await page.waitForFunction(() => gameState[5] === 1);
        const time = await page.evaluate(() => gameState[6]); await page.waitForTimeout(200);
        assert.equal(await page.evaluate(() => gameState[6]), time);
        await page.screenshot({ path: `build/screenshots/titanion-mode${mode}.png` });
        await press('p'); await page.keyboard.up('x');
        if (mode < 2) { await press('Escape'); await page.waitForFunction(() => gameState[0] === 0); }
    }
    assert.ok(await page.evaluate(() => audioStarts > 0));
    await page.evaluate(() => lubHost.queue.push({ topic: 'test.finish', payload: '' }));
    await page.waitForFunction(() => localStorage.getItem('titanion-scores-v1')?.includes('7654321') && localStorage.getItem('titanion-replay-v1'));
    await page.reload(); await observe();
    await page.waitForFunction(() => gameState[10] === 7654321 && gameState[1] === 2 && gameState[9] === 1);
    const count = await page.evaluate(async () => {
        const config = JSON.parse(document.querySelector('#game-config').textContent); const audio = new AudioContext();
        const names = [...config.music.map(n => `${n}.ogg`), ...config.sounds.map(n => `${n}.wav`)];
        for (const name of names) {
            const response = await fetch(`audio/${name}`); if (!response.ok) throw new Error(name);
            const decoded = await audio.decodeAudioData(await response.arrayBuffer()); if (!(decoded.duration > 0)) throw new Error(name);
        }
        await audio.close(); return names.length;
    });
    assert.equal(count, 12); assert.deepEqual(errors, []);
    console.log('PASS: three modes, movement, shots, capture/provocation beams, focus, pause, audio/12 decodes, score and replay reload');
} finally { await browser.close(); }
