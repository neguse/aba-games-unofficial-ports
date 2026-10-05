// Browser-hosted version for cloud browsers that cannot launch a separate Chromium process.
// Only served by tests/game-selection.mjs --serve, never shipped in web/ or dist/.
import { games } from '/game-catalog.js';
const report = document.createElement('pre');
report.id = 'test-report';
report.style.cssText = 'white-space:pre-wrap;padding:12px;max-width:90vw;background:#111;color:#eee';
const start = document.createElement('button');
start.textContent = 'Run selector lifecycle tests';
document.body.prepend(start, report);
const assert = (condition, message) => { if (!condition) throw new Error(message); };
const wait = async predicate => {
    const deadline = performance.now() + 8000;
    while (!predicate()) {
        if (performance.now() > deadline) throw new Error('Timed out: ' + predicate);
        await new Promise(resolve => setTimeout(resolve, 20));
    }
};
const tick = () => new Promise(resolve => setTimeout(resolve, 70));
const find = id => document.querySelector(`[data-game="${id}"]`);
const active = () => document.querySelector('iframe')?.contentWindow;
const launch = async id => {
    find(id).click();
    await wait(() => active()?.document.querySelector('#status')?.hidden);
    assert(document.querySelectorAll('iframe').length === 1, 'one runtime at a time');
    return active();
};
async function returned(id) {
    await wait(() => !document.querySelector('#selection').hidden && !document.querySelector('iframe'));
    assert(document.activeElement === find(id), 'focus restored: ' + id);
    assert(fixtureRuns.every(run => run.shutdowns === 1 && run.pauses === 1 && run.context.state === 'closed'), 'shutdown/audio closure exactly once');
    const ticks = fixtureRuns.map(run => run.ticks).join();
    await tick();
    assert(ticks === fixtureRuns.map(run => run.ticks).join(), 'no frame callbacks after teardown');
}
start.onclick = async () => {
    start.disabled = true;
    report.textContent = '';
    try {
        const log = text => { report.textContent += 'PASS: ' + text + '\n'; };
        for (const game of games) {
            const child = await launch(game.id);
            await tick();
            assert(child.fixtureStats.ticks > 0, 'fixture frame ran');
            document.querySelector('#return-to-list').click();
            await returned(game.id);
            assert(child.abaGame.disposed && child.gpuDestroyed, 'game and GPU disposed');
            log(game.name + ': launch, return, cleanup');
        }
        let child = await launch('tumiki');
        child.document.querySelector('#canvas').dispatchEvent(new KeyboardEvent('keydown', { code: 'Escape', altKey: true, bubbles: true }));
        await returned('tumiki');
        await launch('tumiki');
        history.back();
        await returned('tumiki');
        history.forward();
        await wait(() => active()?.document.querySelector('#status')?.hidden);
        document.querySelector('#return-to-list').click();
        await returned('tumiki');
        log('relaunch, Alt+Escape, browser Back and Forward');
        child = await launch('wok');
        child.lubHost.onMessage('quit', new Uint8Array());
        await returned('wok');
        log('original quit returns to outer list');
        child = await launch('torus-trooper');
        await child.navigator.xr.requestSession('immersive-vr');
        document.querySelector('#return-to-list').click();
        await returned('torus-trooper');
        assert(child.xrEnds === 1, 'active XR session ended');
        child = await launch('torus-trooper');
        child.deferXR = true;
        const pending = child.navigator.xr.requestSession('immersive-vr').catch(() => {});
        document.querySelector('#return-to-list').click();
        await returned('torus-trooper');
        child.resolveXR(); await pending;
        assert(child.xrEnds === 1, 'late XR permission result ended');
        log('active and pending XR teardown');
        assert(localStorage.getItem('tumiki-scores-v1') === 'fixture-save-TUMIKI Fighters', 'legacy on_quit save');
        assert(localStorage.getItem('torus-trooper-scores-v1') === 'fixture-save-Torus Trooper', 'direct score save');
        assert(localStorage.getItem('torus-trooper-replay-v1') === 'fixture-save-Torus Trooper', 'direct replay save');
        assert(localStorage.getItem('gear-toy-gear-scores-v1') === 'fixture-save-GearToyGear', 'separate save namespace');
        log('legacy/direct scores and replay remain isolated');
        find('tumiki').focus();
        find('tumiki').dispatchEvent(new KeyboardEvent('keydown', { code: 'ArrowRight', bubbles: true }));
        assert(document.activeElement === find('parsec47'), 'keyboard selection');
        const pad = { mapping: 'standard', axes: [0, 0], buttons: Array.from({ length: 16 }, () => ({ pressed: false })) };
        navigator.getGamepads = () => [pad];
        await tick();
        pad.buttons[15].pressed = true;
        await tick();
        assert(document.activeElement === find('gunroar'), 'gamepad selection');
        pad.buttons[15].pressed = false; pad.buttons[0].pressed = true;
        await wait(() => active()?.document.querySelector('#status')?.hidden);
        document.querySelector('#return-to-list').click();
        await returned('gunroar');
        await tick();
        assert(!document.querySelector('iframe'), 'held A must not relaunch after return');
        pad.buttons[0].pressed = false;
        log('keyboard/gamepad navigation; held-A return protection');
        report.textContent += `ALL PASS: ${fixtureRuns.length} isolated runtime launches\n`;
        document.title = 'PASS · Selector lifecycle tests';
    } catch (error) {
        report.textContent += 'FAIL: ' + error.stack;
        document.title = 'FAIL · Selector lifecycle tests';
        console.error(error);
    }
};
