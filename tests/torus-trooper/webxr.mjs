import assert from 'node:assert/strict';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { mkdir } from 'node:fs/promises';
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
                            transform: { position: { x: eye + xrTest.offset, y: 1.6, z: 0 }, orientation: { x: 0, y: 0, z: 0, w: 1 }, inverse: { matrix: view } } };
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
    await page.waitForFunction(() => Math.abs(gameState[11] + .182) < .0001);
    await button(0, 5, true); await page.waitForFunction(() => Math.abs(gameState[11] + .032) < .0001); await button(0, 5, false);
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
    await page.keyboard.down('Escape'); await page.waitForFunction(() => gameState[0] === 0); await page.keyboard.up('Escape');
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
    console.log('WebXR input, scheduling, stereo pixels, gameplay, focus, tracking, exit, re-entry and failure recovery passed.');
} finally { await browser.close(); }
