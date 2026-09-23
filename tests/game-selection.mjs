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
try {
    const page = await browser.newPage({ viewport: { width: 960, height: 850 } });
    const errors = [];
    page.on('pageerror', error => errors.push(error.message));
    page.on('console', message => { if (message.type() === 'error') errors.push(message.text()); });
    await page.goto(url);
    const summary = page.locator('.game-selection summary');
    const nav = page.getByRole('navigation', { name: 'ゲーム選択' });
    await summary.focus();
    await summary.press('Enter');
    await page.locator('#status').waitFor({ state: 'hidden', timeout: 60000 });
    assert.equal(await summary.evaluate(element => element === document.activeElement), true);
    assert.equal(await nav.getByRole('link').count(), 13);
    const destinations = await nav.getByRole('link').evaluateAll(links => links.map(link => ({ name: link.textContent, href: link.href })));
    assert.equal(new Set(destinations.map(game => game.href)).size, 13);
    for (const game of [...destinations.slice(1), destinations[0]]) {
        if (!await nav.isVisible()) await summary.press('Enter');
        await nav.getByRole('link', { name: game.name, exact: true }).click();
        await page.waitForURL(game.href);
        await page.locator('#status').waitFor({ state: 'hidden', timeout: 60000 });
        await summary.focus();
        await summary.press('Space');
        await nav.waitFor({ state: 'visible' });
        assert.equal(await nav.locator('[aria-current="page"]').textContent(), game.name);
        assert.ok((await summary.textContent()).includes(game.name));
        const input = await page.evaluate(() => {
            const messages = [];
            const push = lubHost.queue.push;
            lubHost.queue.push = function(...items) { messages.push(...items); return push.apply(this, items); };
            for (const code of ['KeyZ', 'ArrowRight', 'Space', 'ShiftLeft', 'F2']) {
                document.activeElement.dispatchEvent(new KeyboardEvent('keydown', { code, bubbles: true }));
                document.activeElement.dispatchEvent(new KeyboardEvent('keyup', { code, bubbles: true }));
            }
            lubHost.queue.push = push;
            return messages.filter(message => !(['input', 'button'].includes(message.topic) && message.payload === '0'));
        });
        assert.deepEqual(input, [], 'menu keys must not control the game');
    }
    await mkdir('build/screenshots', { recursive: true });
    await page.screenshot({ path: 'build/screenshots/game-selection.png' });
    await page.setViewportSize({ width: 375, height: 812 });
    assert.equal(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), true);
    await page.screenshot({ path: 'build/screenshots/game-selection-mobile.png' });
    assert.deepEqual(errors, []);
    console.log('PASS: all 13 games reachable through the menu, current game, keyboard navigation/input isolation, mobile width; no browser errors');
} finally {
    await browser.close();
}
