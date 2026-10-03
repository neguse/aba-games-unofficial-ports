import assert from 'node:assert/strict';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { mkdir } from 'node:fs/promises';
import { mockXR } from '../webxr-mock.mjs';
const { chromium } = await import(pathToFileURL(resolve(process.env.PLAYWRIGHT_MODULE || '.cache/browser/node_modules/playwright/index.mjs')));
const browser = await chromium.launch({ executablePath: process.env.CHROMIUM_PATH, headless: true,
    args: process.platform === 'win32' ? ['--enable-unsafe-webgpu', '--use-angle=d3d11'] :
        ['--no-sandbox', '--enable-unsafe-webgpu', '--use-angle=swiftshader', '--enable-features=Vulkan,WebGPU', '--use-vulkan=swiftshader'] });
const url = process.argv[2] || 'http://127.0.0.1:8765/gear-toy-gear/';
try {
    const page = await browser.newPage();
    const errors = [];
    page.on('pageerror', error => errors.push(error.message));
    page.on('console', message => { if (message.type() === 'error') errors.push(message.text()); });
    await page.addInitScript(mockXR);
    // gameState: 0 in game, 1-2 player position, 3 game speed, 5 pause ticks, 6 stage ticks, 13 XR session, 14 XR focus
    async function boot(target = url) {
        await page.goto(target);
        await page.locator('#status').waitFor({ state: 'hidden', timeout: 60000 });
        await page.evaluate(() => {
            const onMessage = lubHost.onMessage;
            lubHost.onMessage = (topic, bytes) => {
                if (topic === 'test.state') window.gameState = new TextDecoder().decode(bytes).split(',').map(Number);
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
    async function stick(x, y) {
        await page.evaluate(({ x, y }) => { const axes = xrTest.session.inputSources[0].gamepad.axes; axes[2] = x; axes[3] = y; }, { x, y });
    }
    await boot(); await vr.click();
    await page.waitForFunction(() => gameState[13] === 1 && xrTest.eyes.every(count => count > 100) && xrTest.difference > 50);
    assert.deepEqual(await page.evaluate(() => [canvas.width, canvas.height]), [512, 256]);
    await button(1, 4, true); await page.waitForFunction(() => gameState[0] === 1); await button(1, 4, false);
    await button(1, 0, true); await page.waitForFunction(() => gameState[3] > 1.1); await button(1, 0, false);
    const speed = await page.evaluate(() => gameState[3]);
    await button(0, 0, true); await page.waitForFunction(speed => gameState[3] < speed - .05, speed); await button(0, 0, false);
    const x = await page.evaluate(() => gameState[1]);
    await stick(.8, 0); await page.waitForFunction(x => Math.abs(gameState[1] - x) > 1, x); await stick(0, 0);
    await mkdir('build/screenshots', { recursive: true });
    await page.locator('#canvas').screenshot({ path: 'build/screenshots/gear-webxr-stereo.png' });
    assert.ok(await page.evaluate(() => xrTest.colored > 200), 'course is visible in both eyes');
    await page.evaluate(() => { xrTest.compare = true; });
    await page.waitForFunction(() => xrTest.copyDifferences !== undefined);
    assert.equal(await page.evaluate(() => xrTest.copyDifferences), 0, 'XR compositor receives both eyes with correct orientation and colors');
    await page.evaluate(() => { xrTest.session.visibilityState = 'visible-blurred'; xrTest.session.dispatchEvent(new Event('visibilitychange')); });
    await page.waitForFunction(() => gameState[14] === 0);
    const blurred = await page.evaluate(() => gameState[6]); await page.waitForTimeout(250);
    assert.equal(await page.evaluate(() => gameState[6]), blurred, 'system UI freezes simulation');
    await page.evaluate(() => { xrTest.session.visibilityState = 'visible'; xrTest.session.dispatchEvent(new Event('visibilitychange')); });
    await page.waitForFunction(ticks => gameState[14] === 1 && gameState[6] > ticks, blurred);
    await button(1, 5, true); await page.waitForTimeout(250);
    assert.equal(await page.evaluate(() => gameState[0]), 1, 'B leaves only a paused game');
    await button(1, 5, false);
    await button(0, 4, true); await page.waitForFunction(() => gameState[5] >= 0); await button(0, 4, false);
    const paused = await page.evaluate(() => gameState[6]); await page.waitForTimeout(250);
    assert.equal(await page.evaluate(() => gameState[6]), paused, 'X pauses');
    await button(1, 5, true); await page.waitForFunction(() => gameState[0] === 0); await button(1, 5, false);
    await page.evaluate(() => xrTest.session.end());
    await page.waitForFunction(() => gameState[13] === 0);
    assert.deepEqual(await page.evaluate(() => [canvas.width, canvas.height, _canvasWidth, _canvasHeight]), [640, 480, 640, 480]);
    await page.keyboard.down('x'); await page.waitForFunction(() => gameState[0] === 1); await page.keyboard.up('x');
    await vr.click(); await page.waitForFunction(() => gameState[13] === 1 && xrTest.eyes.every(count => count > 100));
    await page.evaluate(() => xrTest.session.end()); await page.waitForFunction(() => gameState[13] === 0);
    assert.deepEqual(errors, []);
    await page.evaluate(() => { xrTest.failure = 'request'; });
    await vr.click(); await page.waitForFunction(() => !document.querySelector('#vr').disabled);
    assert.equal(await vr.textContent(), 'VRで遊ぶ');
    assert.deepEqual(await page.evaluate(() => [canvas.width, canvas.height]), [640, 480]);
    await boot(url + '?unsupported');
    assert.equal(await vr.textContent(), 'VR非対応'); assert.equal(await vr.isDisabled(), true);
    console.log('WebXR stereo pixels, controller mapping, pause, focus, exit, re-entry and failure recovery passed.');
} finally { await browser.close(); }
