import assert from 'node:assert/strict';
import { mkdir } from 'node:fs/promises';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
const { chromium } = await import(pathToFileURL(resolve(process.env.PLAYWRIGHT_MODULE || '.cache/browser/node_modules/playwright/index.mjs')));
const url = process.argv[2] || 'http://127.0.0.1:8765/torus-trooper/';
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
 g.in_game_state.score=7654321; g.in_game_state.next_extend=2147483647; g.in_game_state.time=0; g.ship.is_game_over=false
 g.ship._speed=0; g.ship.target_speed=0; g.ship.regenerative_charge=0
end
if topic == "scores" then`);
        code = code.replace(/return Game\s*$/, `local frame=Game.on_frame
function Game.on_frame(dt)
 frame(dt)
 local g=Game.manager
 local shots=0
 for _,s in ipairs(g.shots.actor) do if s.exists then shots=shots+1 end end
 lub.host.send('test.state',table.concat({g.state==g.in_game_state and 1 or 0,g.title_manager.grade,g.ship.grade,g.ship:get_pos().x,shots,g.in_game_state.pause_cnt,g.in_game_state.time,g.ship.charging_shot and g.ship.charging_shot.charge_cnt or 0,Ship.replay_mode and 1 or 0,g.pref_manager.pref_data.grade_data[3].hi_score,g.ship:get_speed(),g.ship.cnt,g.title_manager:get_replay_mode() and 1 or 0,g.in_game_state.game_over_cnt,Ship.camera_mode and 1 or 0,Ship.draw_front_mode and 1 or 0},','))
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
    assert.ok(await page.evaluate(() => audioStarts > 0));
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
        const names = [...config.music.map(n => `${n}.ogg`), ...config.sounds.map(n => `${n}.wav`)];
        for (const name of names) {
            const response = await fetch(`audio/${name}`); if (!response.ok) throw new Error(name);
            const decoded = await audio.decodeAudioData(await response.arrayBuffer()); if (!(decoded.duration > 0)) throw new Error(name);
        }
        await audio.close(); return names.length;
    });
    assert.equal(count, 14); assert.deepEqual(errors, []);
    console.log('PASS: three grades, acceleration, movement, shot/charge, course rendering, pause, audio/14 decodes, score/replay reload');
} finally { await browser.close(); }
