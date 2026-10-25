import assert from 'node:assert/strict';
import { test } from 'node:test';
import { assets } from '../web/compiled.js';

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
