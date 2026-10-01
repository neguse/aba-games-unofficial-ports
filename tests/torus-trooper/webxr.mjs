import assert from 'node:assert/strict';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { mkdir } from 'node:fs/promises';
import { anchorPose, controllerInput, eyeMatrix, frameScheduler, multiply } from '../../web/torus-webxr.js';

const identity = [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1];
const anchor = anchorPose({ position: { x: 2, y: 1.6, z: -3 }, orientation: { x: 0, y: 0, z: 0, w: 1 } });
assert.deepEqual([...anchor.slice(12, 15)], [2, Math.fround(1.6), -3]);
assert.ok(multiply(identity, anchor).every((value, index) => value === anchor[index]));
const projection = [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, -1.002, -1, 0, 0, -.2002, 0];
const matrix = eyeMatrix({ projectionMatrix: projection, transform: { inverse: { matrix: identity } } }, identity);
const nearDepth = (matrix[10] * -.1 + matrix[14]) / .1;
assert.ok(Math.abs(nearDepth) < .000001, 'WebGL near plane becomes WebGPU depth zero');
const yawAnchor = anchorPose({ position: { x: 0, y: 0, z: 0 }, orientation: { x: 0, y: Math.SQRT1_2, z: 0, w: Math.SQRT1_2 } });
const inverseYaw = [0, 0, 1, 0, 0, 1, 0, 0, -1, 0, 0, 0, 0, 0, 0, 1];
const recentered = eyeMatrix({ projectionMatrix: projection, transform: { inverse: { matrix: inverseYaw } } }, yawAnchor);
assert.ok(recentered.every((value, index) => Math.abs(value - matrix[index]) < .000001), 'recenter cancels initial headset yaw');
const pad = { mapping: 'xr-standard', axes: [0, 0, -.8, -.8], buttons: Array.from({ length: 6 }, () => ({ value: 0, pressed: false })) };
pad.buttons[0] = { value: .8, pressed: true };
assert.equal(controllerInput([{ handedness: 'left', gamepad: pad }]).mask, 1 | 4 | 32);
pad.axes[2] = -.3; pad.axes[3] = 0; pad.buttons[0].value = 0;
assert.equal(controllerInput([{ handedness: 'left', gamepad: pad }], 4).mask, 4, 'stick hysteresis');
assert.equal(controllerInput([{ handedness: 'left', gamepad: pad }]).mask, 0);
assert.equal(controllerInput([], 255).mask, 0, 'disconnected controls release');
const native = new Map(); let id = 0;
const host = { requestAnimationFrame(callback) { native.set(++id, callback); return id; }, cancelAnimationFrame(id) { native.delete(id); } };
const scheduler = frameScheduler(host), calls = [];
host.requestAnimationFrame(() => { calls.push(1); host.requestAnimationFrame(() => calls.push(2)); });
scheduler.immersive(true); assert.equal(native.size, 0);
scheduler.flush(10); assert.deepEqual(calls, [1], 'new callbacks wait for next XR frame');
scheduler.flush(20); assert.deepEqual(calls, [1, 2]);
const cancelled = host.requestAnimationFrame(() => calls.push(3)); host.cancelAnimationFrame(cancelled);
scheduler.flush(30); assert.deepEqual(calls, [1, 2]);
host.requestAnimationFrame(() => calls.push(4)); scheduler.immersive(false);
assert.equal(native.size, 1, 'desktop animation resumes after XR');

const { chromium } = await import(pathToFileURL(resolve(process.env.PLAYWRIGHT_MODULE || '.cache/browser/node_modules/playwright/index.mjs')));
const browser = await chromium.launch({ executablePath: process.env.CHROMIUM_PATH, headless: true,
    args: process.platform === 'win32' ? ['--enable-unsafe-webgpu', '--use-angle=d3d11'] :
        ['--no-sandbox', '--enable-unsafe-webgpu', '--use-angle=swiftshader', '--enable-features=Vulkan,WebGPU', '--use-vulkan=swiftshader'] });
const url = process.argv[2] || 'http://127.0.0.1:8765/torus-trooper/';
try {
    const page = await browser.newPage();
    const errors = [];
    page.on('pageerror', error => errors.push(error.message));
    page.on('console', message => { if (message.type() === 'error') errors.push(message.text()); });
    await page.addInitScript(() => {
        const request = window.requestAnimationFrame.bind(window);
        const identity = () => [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1];
        const control = handedness => ({ handedness, gamepad: { mapping: 'xr-standard', axes: [0, 0, 0, 0],
            buttons: Array.from({ length: 6 }, () => ({ value: 0, pressed: false })) } });
        window.xrTest = { supported: !location.search.includes('unsupported'), failure: '', offset: 0, tracking: true, eyes: [0, 0], difference: 0, frames: 0 };
        class Session extends EventTarget {
            visibilityState = 'visible'; inputSources = [control('left'), control('right')]; ended = false;
            updateRenderState(state) { this.renderState = state; }
            async requestReferenceSpace() {
                if (xrTest.failure === 'reference') throw new Error('reference failed');
                return new EventTarget();
            }
            async end() { this.ended = true; this.dispatchEvent(new Event('end')); }
            requestAnimationFrame(callback) {
                request(time => {
                    if (this.ended) return;
                    const views = [-.032, .032].map((eye, index) => {
                        const view = identity(); view[12] = -eye - xrTest.offset; view[13] = -1.6;
                        return { eye: index ? 'right' : 'left', projectionMatrix: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, -1.0002, -1, 0, 0, -.10001, 0],
                            transform: { inverse: { matrix: view } } };
                    });
                    callback(time, { getViewerPose: () => xrTest.tracking ? { views, transform: {
                        position: { x: xrTest.offset, y: 1.6, z: 0 }, orientation: { x: 0, y: 0, z: 0, w: 1 },
                    } } : null });
                    const layer = this.renderState.baseLayer, gl = layer.gl, pixels = new Uint8Array(512 * 256 * 4);
                    gl.bindFramebuffer(gl.FRAMEBUFFER, layer.framebuffer);
                    gl.readPixels(0, 0, 512, 256, gl.RGBA, gl.UNSIGNED_BYTE, pixels);
                    if (xrTest.compare) {
                        const copy = document.createElement('canvas'); copy.width = 512; copy.height = 256;
                        const context = copy.getContext('2d'); context.drawImage(document.querySelector('#canvas'), 0, 0);
                        const source = context.getImageData(0, 0, 512, 256).data;
                        let differences = 0;
                        for (let y = 0; y < 256; y++) for (let x = 0; x < 512; x++) for (let c = 0; c < 3; c++)
                            if (Math.abs(pixels[(y * 512 + x) * 4 + c] - source[((255 - y) * 512 + x) * 4 + c]) > 2) differences++;
                        xrTest.copyDifferences = differences; xrTest.compare = false;
                    }
                    xrTest.eyes = [0, 0]; xrTest.difference = 0; xrTest.colored = 0;
                    for (let y = 0; y < 256; y++) for (let x = 0; x < 256; x++) {
                        const a = (y * 512 + x) * 4, b = a + 256 * 4;
                        if (Math.max(...pixels.slice(a, a + 3)) > 30) xrTest.eyes[0]++;
                        if (Math.max(...pixels.slice(b, b + 3)) > 30) xrTest.eyes[1]++;
                        if (pixels[a] !== pixels[b] || pixels[a + 1] !== pixels[b + 1]) xrTest.difference++;
                        if (Math.max(pixels[a], pixels[a + 1], pixels[a + 2]) - Math.min(pixels[a], pixels[a + 1], pixels[a + 2]) > 20) xrTest.colored++;
                    }
                    xrTest.frames++;
                });
            }
        }
        Object.defineProperty(navigator, 'xr', { value: {
            isSessionSupported: async () => xrTest.supported,
            requestSession: async () => {
                if (xrTest.failure === 'request') throw new Error('request denied');
                return xrTest.session = new Session();
            },
        } });
        window.XRWebGLLayer = class {
            constructor(session, gl) {
                this.gl = gl; this.framebuffer = gl.createFramebuffer();
                const texture = gl.createTexture(); gl.bindTexture(gl.TEXTURE_2D, texture);
                gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA8, 512, 256, 0, gl.RGBA, gl.UNSIGNED_BYTE, null);
                gl.bindFramebuffer(gl.FRAMEBUFFER, this.framebuffer);
                gl.framebufferTexture2D(gl.FRAMEBUFFER, gl.COLOR_ATTACHMENT0, gl.TEXTURE_2D, texture, 0);
                session.addEventListener('end', () => { gl.deleteTexture(texture); gl.deleteFramebuffer(this.framebuffer); });
            }
            getViewport(view) { return { x: view.eye === 'left' ? 0 : 256, y: 0, width: 256, height: 256 }; }
        };
        WebGL2RenderingContext.prototype.makeXRCompatible = async function() {};
    });
    await page.route('**/game.lua', async route => {
        const response = await route.fetch();
        const code = (await response.text()).replace(/return Game\s*$/, `local frame=Game.on_frame
function Game.on_frame(dt)
 frame(dt)
 local g=Game.manager
 local shots=0
 for _,s in ipairs(g.shots.actor) do if s.exists then shots=shots+1 end end
 lub.host.send('test.xr',table.concat({g.state==g.in_game_state and 1 or 0,g.in_game_state.time,g.pad.directions,g.pad.buttons,g.in_game_state.pause_cnt,shots,g.ship.charging_shot and g.ship.charging_shot.charge_cnt or 0,TtRender.active and 1 or 0,TtRender.focused and 1 or 0,Game.elapsed,TtRender.first_person and 1 or 0,TtRender.projection[1][13] or 0},','))
end
return Game`);
        await route.fulfill({ response, body: code });
    });
    async function boot(target = url) {
        await page.goto(target);
        await page.locator('#status').waitFor({ state: 'hidden', timeout: 45000 });
        await page.evaluate(() => {
            const onMessage = lubHost.onMessage;
            lubHost.onMessage = (topic, bytes) => {
                if (topic === 'test.xr') window.gameState = new TextDecoder().decode(bytes).split(',').map(Number);
                else onMessage(topic, bytes);
            };
        });
        await page.waitForFunction(() => window.gameState);
    }
    const vr = page.locator('#vr');
    async function button(hand, index, pressed) {
        await page.evaluate(({ hand, index, pressed }) => {
            xrTest.session.inputSources[hand].gamepad.buttons[index] = { pressed, value: pressed ? 1 : 0 };
        }, { hand, index, pressed });
    }
    await boot(); await vr.click();
    await page.waitForFunction(() => xrTest.eyes.every(count => count > 100) && xrTest.difference > 50);
    assert.deepEqual(await page.evaluate(() => [canvas.width, canvas.height]), [512, 256]);
    await button(1, 4, true); await page.waitForFunction(() => gameState[0] === 1); await button(1, 4, false);
    await button(1, 0, true); await page.waitForFunction(() => gameState[5] > 0); await button(1, 0, false);
    await button(0, 0, true); await page.waitForFunction(() => gameState[6] > 24); await button(0, 0, false);
    await page.waitForFunction(() => gameState[6] === 0);
    await button(0, 4, true); await page.waitForFunction(() => gameState[4] > 0); await button(0, 4, false);
    await mkdir('build/screenshots', { recursive: true });
    await page.locator('#canvas').screenshot({ path: 'build/screenshots/torus-webxr-third-person.png' });
    assert.ok(await page.evaluate(() => xrTest.colored > 200), 'course is visible in third person');
    await page.evaluate(() => { xrTest.compare = true; });
    await page.waitForFunction(() => xrTest.copyDifferences !== undefined);
    assert.equal(await page.evaluate(() => xrTest.copyDifferences), 0, 'XR compositor receives both eyes with correct orientation and colors');
    const viewBefore = await page.evaluate(() => gameState[10]);
    await button(1, 3, true); await page.waitForFunction(value => gameState[10] !== value, viewBefore);
    await page.waitForTimeout(150); assert.notEqual(await page.evaluate(() => gameState[10]), viewBefore, 'held stick toggles once');
    await button(1, 3, false);
    await page.evaluate(() => { xrTest.offset = .15; });
    await page.waitForFunction(() => Math.abs(gameState[11] + .118) < .0001);
    await button(0, 5, true); await page.waitForFunction(() => Math.abs(gameState[11] - .032) < .0001); await button(0, 5, false);
    await page.locator('#canvas').screenshot({ path: 'build/screenshots/torus-webxr-stereo.png' });
    assert.ok(await page.evaluate(() => xrTest.colored > 200), 'course is visible in first person');
    await button(0, 4, true); await page.waitForFunction(() => gameState[4] === 0); await button(0, 4, false);
    await page.evaluate(() => { xrTest.session.inputSources[0].gamepad.axes[2] = .8; xrTest.offset = .15; });
    await page.waitForFunction(() => gameState[2] === 8 && xrTest.difference > 50);
    await page.evaluate(() => { xrTest.session.inputSources = []; });
    await page.waitForFunction(() => gameState[2] === 0 && gameState[3] === 0);
    await page.evaluate(() => { xrTest.session.visibilityState = 'visible-blurred'; xrTest.session.dispatchEvent(new Event('visibilitychange')); });
    await page.waitForFunction(() => gameState[8] === 0);
    const paused = await page.evaluate(() => gameState[1]); await page.waitForTimeout(250);
    assert.equal(await page.evaluate(() => gameState[1]), paused, 'system UI freezes simulation');
    await page.evaluate(() => { xrTest.session.visibilityState = 'visible'; });
    await page.waitForFunction(time => gameState[1] < time, paused);
    await page.evaluate(() => { xrTest.tracking = false; }); await page.waitForFunction(() => gameState[8] === 0);
    await page.evaluate(() => { xrTest.tracking = true; }); await page.waitForFunction(() => gameState[8] === 1);
    await page.evaluate(() => xrTest.session.end());
    await page.waitForFunction(() => gameState[7] === 0);
    assert.deepEqual(await page.evaluate(() => [canvas.width, canvas.height, _canvasWidth, _canvasHeight]), [640, 480, 640, 480]);
    await page.keyboard.press('Escape'); await page.waitForFunction(() => gameState[0] === 0);
    await vr.click(); await page.waitForFunction(() => gameState[7] === 1 && xrTest.eyes.every(count => count > 100));
    await page.evaluate(() => xrTest.session.end()); await page.waitForFunction(() => gameState[7] === 0);
    assert.deepEqual(errors, []);
    for (const failure of ['request', 'reference']) {
        await page.evaluate(failure => { xrTest.failure = failure; }, failure);
        await vr.click(); await page.waitForFunction(() => !document.querySelector('#vr').disabled);
        assert.equal(await vr.textContent(), 'VRで遊ぶ');
        assert.deepEqual(await page.evaluate(() => [canvas.width, canvas.height]), [640, 480]);
        if (failure === 'reference') assert.equal(await page.evaluate(() => xrTest.session.ended), true);
    }
    await boot(url + '?unsupported');
    assert.equal(await vr.textContent(), 'VR非対応'); assert.equal(await vr.isDisabled(), true);
    await page.keyboard.down('z'); await page.waitForFunction(() => gameState[0] === 1); await page.keyboard.up('z');
    console.log('WebXR math, input, scheduling, stereo pixels, gameplay, focus, tracking, exit, re-entry and failure recovery passed.');
} finally { await browser.close(); }
