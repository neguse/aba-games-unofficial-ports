// What the browser tests read from a game page whose game uses lub directly:
// saves in lub's save directory (the Emscripten FS /save, synced to IndexedDB) and lub's audio device.

// Waits until /save/<file> has the expected text and lub has finished writing it to IndexedDB.
// how is 'includes', 'excludes', 'startsWith' or 'equals'.
export async function waitSaved(page, file, expected, how = 'includes') {
    await page.waitForFunction(([file, expected, how]) => {
        const module = window.Module, state = module?.lubSave;
        if (!state || state.busy || !module.FS.analyzePath('/save/' + file).exists) return false;
        const text = module.FS.readFile('/save/' + file, { encoding: 'utf8' });
        if (how === 'equals') return text === expected;
        if (how === 'startsWith') return text.startsWith(expected);
        return text.includes(expected) === (how !== 'excludes');
    }, [file, expected, how]);
}

// Replaces /save/<file> and writes it to IndexedDB, so that the next page load reads it.
export async function writeSave(page, file, text) {
    await page.evaluate(async ([file, text]) => {
        const fs = window.Module.FS;
        fs.mkdirTree('/save/' + file.slice(0, file.lastIndexOf('/')));
        fs.writeFile('/save/' + file, text);
        await new Promise((resolve, reject) => fs.syncfs(false, error => error ? reject(error) : resolve()));
    }, [file, text]);
}

export async function audioRunning(page) {
    return page.evaluate(() => window.miniaudio?.devices.some(device => device?.webaudio.state === 'running') === true);
}

// Decodes every sound the game loads (the audio/ entries of assets.json) and returns how many there are.
export async function decodeAudio(page) {
    return page.evaluate(async () => {
        const files = (await (await fetch('assets.json')).json()).filter(path => path.startsWith('audio/'));
        const audio = new AudioContext();
        for (const file of files) {
            const response = await fetch(file);
            if (!response.ok) throw new Error(file);
            const decoded = await audio.decodeAudioData(await response.arrayBuffer());
            if (!(decoded.duration > 0)) throw new Error(file);
        }
        await audio.close();
        return files.length;
    });
}
