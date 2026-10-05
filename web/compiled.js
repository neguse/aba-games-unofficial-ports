export async function assets(options) {
    const response = await fetch('assets.json', options);
    if (!response.ok) throw new Error('ゲームデータを読み込めませんでした。');
    return Promise.all((await response.json()).map(async path => {
        const response = await fetch(path, options);
        if (!response.ok) throw new Error(`ゲームデータを読み込めません：${path}`);
        return [path, new Uint8Array(await response.arrayBuffer())];
    }));
}

export function prepare(module, files, saves) {
    const fs = module.FS;
    module.ENV.XDG_DATA_HOME = '/save';
    for (const [path, data] of files) {
        fs.mkdirTree('/' + path.split('/').slice(0, -1).join('/'));
        fs.writeFile('/' + path, data);
    }
    const keys = new Map(saves);
    for (const [path, key] of keys) {
        fs.mkdirTree(path.slice(0, path.lastIndexOf('/')));
        let text = '';
        try { text = localStorage.getItem(key) || ''; } catch {}
        fs.writeFile(path, text);
    }
    const persist = path => {
        if (keys.has(path)) {
            try { localStorage.setItem(keys.get(path), fs.readFile(path, { encoding: 'utf8' })); } catch {}
        }
    };
    module.persistSaves = () => { for (const path of keys.keys()) persist(path); };
    const rename = fs.rename;
    fs.rename = function(oldPath, newPath) {
        rename.call(fs, oldPath, newPath);
        persist(newPath);
    };
    const close = fs.close;
    fs.close = function(stream) {
        const path = stream.path, write = (stream.flags & 3) !== 0;
        close.call(fs, stream);
        if (write) persist(path);
    };
}

export function unlockAudio() {
    for (const device of window.miniaudio?.devices || []) device?.webaudio.resume();
}

export function volume(module, value) {
    if (module?.calledRun) module.ccall('lub_tcs_volume', null, ['number'], [value]);
}
