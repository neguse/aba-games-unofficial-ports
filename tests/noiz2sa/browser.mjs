import assert from 'node:assert/strict';
import { mkdir } from 'node:fs/promises';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
const { chromium } = await import(pathToFileURL(resolve(process.env.PLAYWRIGHT_MODULE || '.cache/browser/node_modules/playwright/index.mjs')));
const url = process.argv[2] || 'http://127.0.0.1:8765/noiz2sa/';
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
        code = code.replace('if topic == "scores" then', `if topic == "test.select" then NrAttract.hi_score.stage=tonumber(payload);NrCore.init_title() end
if topic == "test.record" then NrAttract.score=123456;NrBarrage.scene=0;NrAttract.set_clear_score();NrPreference.save_preference() end
if topic == "test.finish" then NrAttract.score=7654321;NrCore.init_gameover();NrAttract.go_cnt=901 end
if topic == "test.palette" then test_palette=true end
if topic == "scores" then`);
        code = code.replace('NrRender.frame()', `if test_palette then
 local colors={0,1,16,31,32,47,48,63}
 for i,c in ipairs(colors) do NrScreen.buf:rect(16+(i-1)*36,220,32,32,c,0,0,0,0) end
end
NrRender.frame()`);
        code = code.replace(/return Game\s*$/, `local frame=Game.on_frame
function Game.on_frame(dt)
 frame(dt)
 local s=NrShip.ship;local shots=0
 for _,shot in ipairs(NrShot.shot) do if shot.cnt~=-999999 then shots=shots+1 end end
 lub.host.send('test.state',table.concat({NrCore.status,NrAttract.slc_stg,NrAttract.stage,s.pos.x,s.speed,s.cnt,shots,NrBarrage.endless,NrBarrage.insane,NrAttract.hi_score.stage_score[14],NrAttract.hi_score.scene_score[2][1],NrBarrage.scene},','))
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
    async function press(key) { await page.keyboard.down(key); await page.waitForTimeout(150); await page.keyboard.up(key); await page.waitForTimeout(200); }
    await page.goto(url); await observe();
    await page.screenshot({ path: 'build/screenshots/noiz2sa-title.png' });
    await press('ArrowDown'); await page.waitForFunction(() => gameState[1] === 1);
    const stages = [1, 9, 10, 11, 12, 13];
    for (const stage of stages) {
        console.log(`STAGE ${stage}`);
        if (stage !== 1) await page.evaluate(stage => lubHost.queue.push({ topic: 'test.select', payload: String(stage) }), stage);
        await page.waitForFunction(stage => gameState[0] === 0 && gameState[1] === stage, stage);
        await page.keyboard.down('z'); await page.waitForFunction(stage => gameState[0] === 1 && gameState[2] === stage, stage); await page.keyboard.up('z');
        assert.equal(await page.evaluate(() => gameState[7]), stage >= 10 ? 1 : 0);
        assert.equal(await page.evaluate(() => gameState[8]), stage === 13 ? 1 : 0);
        const x = await page.evaluate(() => gameState[3]);
        await page.keyboard.down('ArrowRight'); await page.waitForFunction(x => gameState[3] > x + 2000, x); await page.keyboard.up('ArrowRight');
        await page.keyboard.down('z'); await page.waitForFunction(() => gameState[6] > 0); await page.keyboard.up('z');
        await page.keyboard.down('x'); await page.waitForFunction(() => gameState[4] === 640); await page.keyboard.up('x');
        await page.waitForFunction(() => gameState[4] === 1280);
        await page.keyboard.down('p'); await page.waitForFunction(() => gameState[0] === 4); await page.keyboard.up('p');
        const tick = await page.evaluate(() => gameState[5]); await page.waitForTimeout(200);
        assert.equal(await page.evaluate(() => gameState[5]), tick);
        const screenshot = await page.locator('#canvas').screenshot({ path: `build/screenshots/noiz2sa-stage-${stage}.png` });
        const colored = await page.evaluate(async png => {
            const image = new Image(); image.src = 'data:image/png;base64,' + png; await image.decode();
            const copy = document.createElement('canvas'); copy.width = image.width; copy.height = image.height;
            const context = copy.getContext('2d'); context.drawImage(image, 0, 0);
            const pixels = context.getImageData(image.width/4, 0, image.width/2, image.height).data;
            let count = 0;
            for (let i = 0; i < pixels.length; i += 4) if (Math.max(pixels[i],pixels[i+1],pixels[i+2])-Math.min(pixels[i],pixels[i+1],pixels[i+2]) > 25) count++;
            return count;
        }, screenshot.toString('base64'));
        assert.ok(colored > 500, `gameplay pixels stage ${stage}: ${colored}`);
        await page.keyboard.down('p'); await page.waitForFunction(() => gameState[0] === 1); await page.keyboard.up('p');
        if (stage === 1) {
            await page.evaluate(() => lubHost.queue.push({ topic: 'test.record', payload: '' }));
            await page.waitForFunction(() => localStorage.getItem('noiz2sa-scores-v1')?.includes('123456'));
        }
        if (stage < 13) {
            await page.keyboard.down('Escape'); await page.waitForFunction(() => gameState[0] === 0);
            await page.keyboard.up('Escape'); await page.waitForTimeout(200);
        }
    }
    assert.ok(await page.evaluate(() => audioStarts > 0));
    await page.evaluate(() => lubHost.queue.push({ topic: 'test.finish', payload: '' }));
    await page.waitForFunction(() => localStorage.getItem('noiz2sa-scores-v1')?.includes('7654321'));
    await page.reload(); await observe();
    await page.waitForFunction(() => gameState[0] === 0 && gameState[1] === 13 && gameState[9] === 7654321 && gameState[10] === 123456);
    await page.evaluate(() => lubHost.queue.push({ topic: 'test.palette', payload: '' }));
    await page.waitForTimeout(500);
    const paletteImage = await page.locator('#canvas').screenshot();
    const palette = await page.evaluate(async png => {
        const image = new Image(); image.src = 'data:image/png;base64,' + png; await image.decode();
        const copy = document.createElement('canvas'); copy.width = image.width; copy.height = image.height;
        const context = copy.getContext('2d'); context.drawImage(image,0,0);
        return Array.from({length:8},(_,i) => Array.from(context.getImageData(Math.floor((192+i*36)*image.width/640),Math.floor(236*image.height/480),1,1).data).slice(0,3));
    }, paletteImage.toString('base64'));
    assert.deepEqual(palette,[[255,255,255],[196,196,196],[223,212,212],[223,53,53],[212,223,212],[53,223,53],[213,213,223],[66,66,223]]);
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
    console.log('PASS: normal/high stages, four endless modes, selection, movement/shots/slowdown, palette/trails, pause, audio/14 decodes, stage/scene score reload');
} finally { await browser.close(); }
