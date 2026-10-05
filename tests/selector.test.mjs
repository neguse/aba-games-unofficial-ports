import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { test } from 'node:test';
import vm from 'node:vm';
import { games } from '../web/game-catalog.js';

async function harness(hash = '') {
    const listeners = new Map(), frames = [], callbacks = new Map(), entries = [];
    let location = new URL('https://example.test/collection/' + hash), position = -1, sequence = 0;
    const document = { activeElement: null, title: '', hidden: false, hasFocus: () => true };
    const emit = (type, event = {}) => { for (const callback of listeners.get(type) || []) callback(event); };
    const window = { addEventListener(type, callback) { if (!listeners.has(type)) listeners.set(type, []); listeners.get(type).push(callback); } };
    class Element {
        hidden = false; disabled = false; dataset = {}; children = []; events = new Map();
        constructor(tag) { this.tagName = tag; }
        addEventListener(type, callback) { this.events.set(type, callback); }
        append(child) { child.parent = this; this.children.push(child); }
        remove() { this.parent.children = this.parent.children.filter(child => child !== this); }
        focus() { document.activeElement = this; this.events.get('focus')?.(); }
        contains(child) { return this.children.includes(child); }
        click() { if (!this.disabled) this.events.get('click')?.(); }
    }
    const elements = Object.fromEntries(['selection','player','game-list','game-container','launch-status','playing-title','return-to-list'].map(id => ['#'+id,new Element(id)]));
    elements['#player'].hidden = true;
    document.querySelector = selector => elements[selector];
    document.createElement = tag => {
        const element = new Element(tag);
        if (tag === 'iframe') {
            const run = { element, disposals: 0, closed: false };
            element.contentWindow = { location: { replace(url) { run.url = url; } }, abaGame: { dispose() {
                run.disposals++; run.closed = true;
                return run.wait || Promise.resolve();
            } } };
            frames.push(run);
        }
        return element;
    };
    const context = vm.createContext({ games, window, document, URL, URLSearchParams, console, setTimeout, clearTimeout,
        navigator: { getGamepads: () => [] },
        requestAnimationFrame(callback) { callbacks.set(++sequence, callback); return sequence; },
        cancelAnimationFrame(id) { callbacks.delete(id); },
        history: {
            pushState(state, unused, url) { entries.splice(position + 1); entries.push(String(url)); position++; location = new URL(url); },
            replaceState(state, unused, url) { if (position < 0) position = 0; entries[position] = String(url); location = new URL(url); },
        },
    });
    Object.defineProperty(context, 'location', { get: () => location });
    const code = (await readFile('web/selector.js', 'utf8')).replace("import { games } from './game-catalog.js';", '').replace('import.meta.url', JSON.stringify('https://example.test/collection/selector.js'));
    vm.runInContext(code, context);
    const flush = async () => { await vm.runInContext('transition', context); };
    await flush();
    const buttons = elements['#game-list'].children;
    return { context, elements, frames, buttons, emit, document, flush,
        click: id => buttons.find(button => button.dataset.game === id).click(),
        back: () => elements['#return-to-list'].click(),
        active: () => elements['#game-container'].children,
        message(data, source = frames.at(-1)?.element.contentWindow, origin = location.origin) { emit('message', { data, source, origin }); },
        navigate(delta) { position += delta; assert.ok(entries[position]); location = new URL(entries[position]); emit('popstate'); emit('hashchange'); },
        tick(time) { const pending = [...callbacks.values()]; callbacks.clear(); pending.forEach(callback => callback(time)); },
        key(code, extras = {}) { const event = { code, preventDefault() { this.prevented = true; }, ...extras }; emit('keydown', event); return event; },
        get url() { return location; },
    };
}

test('catalog has every existing game exactly once and all 13 use fresh disposable contexts', async () => {
    const h = await harness();
    assert.equal(h.buttons.length, 13);
    assert.equal(new Set(games.map(game => game.id)).size, 13);
    assert.equal(h.active().length, 0);
    for (const game of games) {
        h.click(game.id); await h.flush();
        assert.equal(h.active().length, 1);
        const run = h.frames.at(-1);
        assert.equal(run.url, `https://example.test/collection/${game.path}?embedded=1`);
        assert.equal(h.url.hash, `#game=${game.id}`);
        h.back(); await h.flush();
        assert.equal(run.disposals, 1);
        assert.equal(h.active().length, 0);
        assert.equal(h.document.activeElement.dataset.game, game.id);
        assert.equal(h.elements['#selection'].hidden, false);
    }
});

test('keyboard and gamepad navigation do not consume gameplay buttons or relaunch held A', async () => {
    const h = await harness();
    h.buttons[0].focus();
    assert.equal(h.key('ArrowRight').prevented, true);
    assert.equal(h.document.activeElement.dataset.game, 'parsec47');
    h.key('End'); assert.equal(h.document.activeElement.dataset.game, 'masashikun-hi');
    h.key('Home'); assert.equal(h.document.activeElement.dataset.game, 'tumiki');
    const pad = { mapping: 'standard', axes: [0, 0], buttons: Array.from({length:16}, () => ({pressed:false})) };
    h.context.navigator.getGamepads = () => [pad]; h.tick(0);
    pad.buttons[15].pressed = true; h.tick(10);
    assert.equal(h.document.activeElement.dataset.game, 'parsec47');
    h.tick(50); assert.equal(h.document.activeElement.dataset.game, 'parsec47');
    pad.buttons[15].pressed = false; pad.buttons[0].pressed = true; h.tick(60); await h.flush();
    assert.equal(h.active().length, 1);
    assert.equal(h.key('Escape').prevented, undefined, 'original Escape is untouched');
    assert.equal(h.key('KeyP').prevented, undefined, 'original pause is untouched');
    h.back(); await h.flush(); h.tick(1000); await h.flush();
    assert.equal(h.active().length, 0, 'held A cannot immediately relaunch');
    pad.buttons[0].pressed = false; h.tick(1100);
    pad.buttons[0].pressed = true; h.tick(1200); await h.flush();
    assert.equal(h.active().length, 1);
});

test('Back/Forward and bookmarked launches retain a list history entry', async () => {
    const h = await harness('#game=torus-trooper');
    assert.equal(h.active().length, 1);
    h.navigate(-1); await h.flush();
    assert.equal(h.active().length, 0);
    h.navigate(1); await h.flush();
    assert.equal(h.active().length, 1);
    assert.ok(h.url.hash.includes('torus-trooper'));
    assert.equal(h.frames[0].disposals, 1);
});

test('latest navigation wins during asynchronous disposal without reviving skipped games', async () => {
    const h = await harness();
    h.click('tumiki'); await h.flush();
    let release;
    h.frames[0].wait = new Promise(resolve => { release = resolve; });
    h.click('parsec47');
    await Promise.resolve();
    // Browser Back is available while the toolbar is disabled during disposal.
    h.navigate(-1);
    h.key('Escape', { altKey: true });
    release(); await h.flush();
    assert.equal(h.active().length, 0);
    assert.equal(h.frames.length, 1);
    assert.equal(h.frames[0].disposals, 1);
});

test('only active same-origin child messages control the shell', async () => {
    const h = await harness();
    h.click('tumiki'); await h.flush();
    h.message({ type: 'aba:return' }, {}, 'https://example.test'); await h.flush();
    assert.equal(h.active().length, 1);
    h.message({ type: 'aba:return' }, h.frames[0].element.contentWindow, 'https://attacker.test'); await h.flush();
    assert.equal(h.active().length, 1);
    h.message({ type: 'aba:ready' });
    assert.equal(h.elements['#launch-status'].hidden, true);
    h.message({ type: 'aba:return' }); await h.flush();
    assert.equal(h.active().length, 0);
});
