export async function assets(options) {
    const response = await fetch('assets.json', options);
    if (!response.ok) throw new Error('ゲームデータを読み込めませんでした。');
    return Promise.all((await response.json()).map(async path => {
        const response = await fetch(path, options);
        if (!response.ok) throw new Error(`ゲームデータを読み込めません：${path}`);
        return [path, new Uint8Array(await response.arrayBuffer())];
    }));
}

export function prepare(module, files) {
    const fs = module.FS;
    for (const [path, data] of files) {
        fs.mkdirTree('/' + path.split('/').slice(0, -1).join('/'));
        fs.writeFile('/' + path, data);
    }
}

export function unlockAudio() {
    for (const device of window.miniaudio?.devices || []) device?.webaudio.resume();
}

export function volume(module, value) {
    if (module?.calledRun) module.ccall('lub_tcs_volume', null, ['number'], [value]);
}
