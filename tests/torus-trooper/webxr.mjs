import assert from 'node:assert/strict';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { mkdir } from 'node:fs/promises';
import { mockXR } from '../webxr-mock.mjs';
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
    await page.addInitScript(mockXR);
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
