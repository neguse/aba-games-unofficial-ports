import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { test } from 'node:test';
import vm from 'node:vm';

async function harness() {
    const sources = [], gains = [], requests = new Map();
    const elements = new Map();
    const context = vm.createContext({
        TextDecoder, console,
        window: { addEventListener() {} },
        document: { querySelector(selector) {
            if (!elements.has(selector)) elements.set(selector, { addEventListener() {}, focus() {}, setAttribute() {} });
            return elements.get(selector);
        } },
        AudioContext: class {
            currentTime = 0;
            destination = {};
            decodeAudioData = async data => data;
            createGain() {
                const gain = { gain: { value: 1, cancelScheduledValues() {}, setValueAtTime() {}, linearRampToValueAtTime() {} },
                    connect(target) { this.target = target; return target; } };
                gains.push(gain); return gain;
            }
            createBufferSource() {
                const source = { starts: 0, stops: 0, connect(target) { this.target = target; return target; },
                    start() { this.starts++; }, stop() { this.stops++; } };
                sources.push(source); return source;
            }
        },
        fetch: url => new Promise(resolve => requests.set(url, () => resolve({ ok: true, arrayBuffer: async () => url }))),
    });
    const code = (await readFile('web/main.js', 'utf8')).replace('boot().catch(fail);', '');
    vm.runInContext(code, context);
    return { context, sources, gains, requests, elements, run: code => vm.runInContext(code, context) };
}

test('stopping a sound cancels a pending decode', async () => {
    const h = await harness();
    const pending = h.run('playSound(0)');
    h.run('stopSound(0)');
    h.requests.get('audio/ship_shot.wav')();
    await pending;
    assert.equal(h.sources.length, 0);
});

test('the newest sound on a shared channel wins regardless of fetch order', async () => {
    const h = await harness();
    const old = h.run('playSound(2)');
    const latest = h.run('playSound(4)');
    h.requests.get('audio/ship_destroyed.wav')();
    await latest;
    h.requests.get('audio/stuck_bonus.wav')();
    await old;
    assert.equal(h.sources.length, 1);
    assert.equal(h.sources[0].buffer, 'audio/ship_destroyed.wav');
    assert.equal(h.sources[0].starts, 1);
    assert.equal(h.sources[0].stops, 0);
});

test('mute affects fading music and effects through the same output', async () => {
    const h = await harness();
    const music = h.run('playMusic(0, true)');
    h.requests.get('audio/we_are_tumiki_fighters.ogg')();
    await music;
    h.run('stopMusic(true)');
    const sound = h.run('playSound(0)');
    h.requests.get('audio/ship_shot.wav')();
    await sound;
    const button = h.elements.get('#sound');
    button.onclick({ currentTarget: button });
    const output = h.gains[0];
    assert.equal(output.gain.value, 0);
    assert.equal(h.sources[0].target.target, output);
    assert.equal(h.sources[1].target, output);
    button.onclick({ currentTarget: button });
    assert.equal(output.gain.value, 1);
});


test('game music volume changes preserve the effect and mute output levels', async () => {
    const h = await harness();
    const music = h.run('playMusic(0, true)');
    h.requests.get('audio/we_are_tumiki_fighters.ogg')();
    await music;
    h.context.window.lubHost.onMessage('music.volume', new TextEncoder().encode('0.5'));
    assert.equal(h.sources[0].target.gain.value, 0.5);
    assert.equal(h.gains[0].gain.value, 1);
    h.run('stopMusic()');
    assert.equal(h.sources[0].stops, 1);
});


test('overlapping cues do not stop another instance of the same effect', async () => {
    const h = await harness();
    h.run('config.soundOverlap = true');
    const first = h.run('playSound(0)');
    const second = h.run('playSound(0)');
    h.requests.get('audio/ship_shot.wav')();
    await Promise.all([first, second]);
    assert.equal(h.sources.length, 2);
    assert.ok(h.sources.every(source => source.starts === 1 && source.stops === 0));
});
