import assert from 'node:assert/strict';
import { test } from 'node:test';
import { prepare, assets } from '../web/compiled.js';

test('direct save flush preserves the original score/replay keys and latest file contents', () => {
    const storage = new Map([['torus-trooper-scores-v1', 'original-score'], ['torus-trooper-replay-v1', 'original-replay'], ['other-game', 'unchanged']]);
    globalThis.localStorage = { getItem: key => storage.get(key), setItem: (key,value) => storage.set(key,value) };
    const files = new Map();
    const module = { ENV: {}, FS: {
        mkdirTree() {}, writeFile(path, data) { files.set(path,data); }, readFile(path) { return files.get(path); },
        rename(from,to) { files.set(to, files.get(from)); files.delete(from); }, close() {},
    } };
    const saves = [['/save/torus-trooper/scores.txt', 'torus-trooper-scores-v1'], ['/save/torus-trooper/replay.txt', 'torus-trooper-replay-v1']];
    prepare(module, [], saves);
    assert.equal(module.ENV.XDG_DATA_HOME, '/save');
    assert.equal(files.get(saves[0][0]), 'original-score');
    assert.equal(files.get(saves[1][0]), 'original-replay');
    module.FS.writeFile(saves[0][0], 'saved-on-quit');
    module.FS.writeFile(saves[1][0], 'replay-on-quit');
    module.persistSaves();
    assert.equal(storage.get(saves[0][1]), 'saved-on-quit');
    assert.equal(storage.get(saves[1][1]), 'replay-on-quit');
    assert.equal(storage.get('other-game'), 'unchanged');
    delete globalThis.localStorage;
});

test('asset manifest and individual downloads share the lifecycle abort signal', async () => {
    const nativeFetch = globalThis.fetch;
    const calls = [];
    const controller = new AbortController();
    globalThis.fetch = async (path, options) => {
        calls.push([path, options?.signal]);
        return { ok: true, json: async () => ['images/title.png'], arrayBuffer: async () => new ArrayBuffer(0) };
    };
    try { await assets({ signal: controller.signal }); }
    finally { globalThis.fetch = nativeFetch; }
    assert.deepEqual(calls.map(([path]) => path), ['assets.json', 'images/title.png']);
    assert.ok(calls.every(([,signal]) => signal === controller.signal));
});
