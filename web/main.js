const config = JSON.parse(document.querySelector("#game-config")?.textContent || "{}");
const canvas = document.querySelector('#canvas');
const status = document.querySelector('#status');
const decoder = new TextDecoder();
const queue = [];
const pressed = new Set();
const controls = new Map(config.controls || [
    ['ArrowUp', 1], ['KeyW', 1], ['ArrowDown', 2], ['KeyS', 2],
    ['ArrowLeft', 4], ['KeyA', 4], ['ArrowRight', 8], ['KeyD', 8],
    ['KeyZ', 16], ['ControlLeft', 16], ['Period', 16],
    ['KeyX', 32], ['AltLeft', 32], ['ShiftLeft', 32], ['ShiftRight', 32], ['Slash', 32],
    ['KeyP', 64], ['Escape', 128],
]);
let audio;
let volume;
let music;
let musicGain;
let musicVersion = 0;
let muted = false;
const buffers = new Map();
const channels = new Map();
const spatialSounds = new Map();
const soundVersions = new Map();
const musicNames = config.music || ['we_are_tumiki_fighters', 'just_over_the_horizon', 'panic_on_meadow', 'here_comes_a_gigantic_toy', 'battle_over_the_junk_city', 'return_to_home'];
const soundNames = config.sounds || ['ship_shot', 'stuck', 'stuck_bonus', 'stuck_destroyed', 'ship_destroyed', 'enemy_damaged', 'small_enemy_destroyed', 'enemy_destroyed', 'boss_destroyed', 'extend', 'warning', 'propeller', 'stuck_bonus_pushin'];
const soundChannels = config.channels || [0, 1, 2, 3, 2, 4, 5, 6, 6, 7, 7, 7, 2];

function send(topic, payload) { queue.push({ topic, payload }); }
function input() {
    let mask = 0;
    for (const key of pressed) mask |= controls.get(key) || 0;
    send('input', String(mask));
}
function initAudio() {
    if (audio) return;
    audio = new AudioContext();
    volume = audio.createGain();
    volume.gain.value = muted ? 0 : 1;
    volume.connect(audio.destination);
}
function unlockAudio() {
    initAudio();
    if (audio.state === 'suspended') audio.resume();
}
window.addEventListener('keydown', event => {
    if (!controls.has(event.code)) return;
    if (event.target instanceof HTMLButtonElement || event.target instanceof HTMLAnchorElement) return;
    event.preventDefault(); unlockAudio();
    if (!pressed.has(event.code)) { pressed.add(event.code); input(); }
});
window.addEventListener('keyup', event => {
    if (pressed.delete(event.code)) { event.preventDefault(); input(); }
});
window.addEventListener('blur', () => { pressed.clear(); input(); });
canvas.addEventListener('pointerdown', () => { unlockAudio(); canvas.focus(); });
if (config.pointer) {
    let pointer = [320, 240, 0];
    function point(event) {
        const rect = canvas.getBoundingClientRect();
        pointer = [Math.round(Math.max(0, Math.min(640, (event.clientX - rect.left) * 640 / rect.width))),
            Math.round(Math.max(0, Math.min(480, (event.clientY - rect.top) * 480 / rect.height))), event.buttons & 3];
        send('pointer', pointer.join(','));
    }
    canvas.addEventListener('pointerdown', event => { canvas.setPointerCapture(event.pointerId); point(event); });
    canvas.addEventListener('pointermove', point);
    canvas.addEventListener('pointerup', point);
    canvas.addEventListener('pointercancel', () => { pointer[2] = 0; send('pointer', pointer.join(',')); });
    canvas.addEventListener('contextmenu', event => event.preventDefault());
    window.addEventListener('blur', () => { pointer[2] = 0; send('pointer', pointer.join(',')); });
}
document.querySelector('#fullscreen').onclick = async () => { await canvas.requestFullscreen(); canvas.focus(); };
document.querySelector('#sound').onclick = event => {
    unlockAudio(); muted = !muted;
    event.currentTarget.textContent = muted ? '音：オフ' : '音：オン';
    event.currentTarget.setAttribute('aria-pressed', String(muted));
    volume.gain.value = muted ? 0 : 1;
    canvas.focus();
};
async function buffer(name, extension) {
    initAudio();
    const key = `${name}.${extension}`;
    if (!buffers.has(key)) buffers.set(key, (async () => {
        const response = await fetch(`audio/${key}`);
        if (!response.ok) throw new Error(`音源を読み込めません：${key}`);
        return audio.decodeAudioData(await response.arrayBuffer());
    })());
    return buffers.get(key);
}
function stopMusic(fade = false) {
    musicVersion++;
    if (!music) return;
    if (fade) {
        musicGain.gain.cancelScheduledValues(audio.currentTime);
        musicGain.gain.setValueAtTime(musicGain.gain.value, audio.currentTime);
        musicGain.gain.linearRampToValueAtTime(0, audio.currentTime + 1.28);
        music.stop(audio.currentTime + 1.28);
    } else music.stop();
    music = null;
}
async function playMusic(index, loop) {
    stopMusic();
    const version = musicVersion;
    const decoded = await buffer(musicNames[index], config.musicExtension || 'ogg');
    if (version !== musicVersion) return;
    music = audio.createBufferSource(); music.buffer = decoded; music.loop = loop;
    musicGain = audio.createGain();
    music.connect(musicGain).connect(volume); music.start();
}
function stopSound(channel) {
    soundVersions.set(channel, (soundVersions.get(channel) || 0) + 1);
    channels.get(channel)?.stop();
    channels.delete(channel);
}
async function playSound(index) {
    if (config.soundOverlap) {
        const decoded = await buffer(soundNames[index], 'wav');
        const source = audio.createBufferSource(); source.buffer = decoded;
        source.connect(volume); source.start();
        return;
    }
    const channel = soundChannels[index];
    stopSound(channel);
    const version = soundVersions.get(channel);
    const decoded = await buffer(soundNames[index], 'wav');
    if (version !== soundVersions.get(channel)) return;
    const source = audio.createBufferSource(); source.buffer = decoded;
    source.connect(volume);
    channels.set(channel, source); source.start();
    source.onended = () => { if (channels.get(channel) === source) channels.delete(channel); };
}
function positionSpatial(sound, position) {
    sound.position = position;
    if (!sound.source) return;
    const [x, y, z] = position;
    sound.panner.positionX.value = x; sound.panner.positionY.value = y; sound.panner.positionZ.value = z;
}
async function playSpatial(values) {
    const [id, index, loop, ...position] = values;
    stopSpatial(id);
    const sound = { position };
    spatialSounds.set(id, sound);
    const decoded = await buffer(soundNames[index], 'wav');
    if (spatialSounds.get(id) !== sound) return;
    const source = audio.createBufferSource(), panner = audio.createPanner();
    source.buffer = decoded; source.loop = Boolean(loop);
    panner.panningModel = 'equalpower'; panner.distanceModel = 'inverse';
    // Gtg.xap has no distance or pitch RPC; XACT's default volume curve is constant.
    panner.refDistance = 1; panner.rolloffFactor = 0;
    sound.source = source; sound.panner = panner; positionSpatial(sound, sound.position);
    source.connect(panner).connect(volume); source.start();
    source.onended = () => { if (spatialSounds.get(id) === sound) spatialSounds.delete(id); panner.disconnect(); };
}
function stopSpatial(id) {
    spatialSounds.get(id)?.source?.stop();
    spatialSounds.delete(id);
}
function fail(error) { status.hidden = false; status.textContent = String(error?.message || error); console.error(error); }
window.lubHost = { queue, onMessage(topic, bytes) {
    const text = decoder.decode(bytes);
    if (topic === 'ready') {
        status.hidden = true;
        if (!document.activeElement?.closest('.game-selection')) canvas.focus();
        if (config.seed) send('seed', String(crypto.getRandomValues(new Uint32Array(1))[0] & 0x7fffffff));
    }
    if (topic === 'scores.load') {
        try { send('scores', localStorage.getItem(config.scores || 'tumiki-scores-v1') || ''); } catch { send('scores', ''); }
    }
    if (topic === 'scores.save') {
        try { localStorage.setItem(config.scores || 'tumiki-scores-v1', text); } catch { }
    }
    if (topic === 'replay.load' && config.replay) {
        try { send('replay', localStorage.getItem(config.replay) || ''); } catch { send('replay', ''); }
    }
    if (topic === 'replay.save' && config.replay) {
        try { localStorage.setItem(config.replay, text); } catch { }
    }
    if (topic === 'music.loop' || topic === 'music.once') playMusic(Number(text), topic === 'music.loop').catch(fail);
    if (topic === 'music.stop') stopMusic();
    if (topic === 'music.fade') stopMusic(true);
    if (topic === 'music.volume' && music) musicGain.gain.value = Math.max(0, Math.min(1, Number(text)));
    if (topic === 'sound.play') playSound(Number(text)).catch(fail);
    if (topic === 'sound.stop') stopSound(soundChannels[Number(text)]);
    if (topic === 'spatial.play') playSpatial(text.split(',').map(Number)).catch(fail);
    if (topic === 'spatial.stop') stopSpatial(Number(text));
    if (topic === 'spatial.update') {
        const [id, ...position] = text.split(',').map(Number), sound = spatialSounds.get(id);
        if (sound) positionSpatial(sound, position);
    }
} };

async function boot() {
    const games = [
        ['/', 'TUMIKI Fighters'], ['/parsec47/', 'PARSEC47'], ['/gunroar/', 'Gunroar'],
        ['/titanion/', 'Titanion'], ['/a7xpg/', 'A7Xpg'], ['/torus-trooper/', 'Torus Trooper'],
        ['/rrootage/', 'rRootage'], ['/noiz2sa/', 'Noiz2sa'], ['/wok/', 'Wok'],
        ['/mazer-mayhem/', 'Mazer Mayhem'], ['/gear-toy-gear/', 'GearToyGear'],
        ['/mu-cade/', 'Mu-cade'], ['/masashikun-hi/', 'まさしくんハイ！'],
    ];
    const path = location.pathname.replace(/index\.html$/, '').replace(/\/?$/, '/');
    const menu = document.createElement('details');
    menu.className = 'game-selection';
    const summary = document.createElement('summary');
    summary.textContent = `ゲームを選ぶ · ${games.find(([href]) => href === path)?.[1] || document.title}`;
    const links = document.createElement('nav');
    links.setAttribute('aria-label', 'ゲーム選択');
    for (const [href, name] of games) {
        const link = document.createElement('a');
        link.href = href;
        link.textContent = name;
        if (href === path) link.setAttribute('aria-current', 'page');
        links.append(link);
    }
    menu.append(summary, links);
    menu.addEventListener('keydown', event => event.stopPropagation());
    menu.addEventListener('focusin', () => { pressed.clear(); input(); });
    canvas.before(menu);
    if (!navigator.gpu) throw new Error('このゲームにはWebGPU対応ブラウザが必要です。');
    const adapter = await navigator.gpu.requestAdapter();
    if (!adapter) throw new Error('WebGPUを初期化できませんでした。');
    const requiredFeatures = ['depth32float-stencil8', 'float32-filterable'].filter(feature => adapter.features.has(feature));
    const device = await adapter.requestDevice({ requiredFeatures });
    device.addEventListener('uncapturederror', event => fail(event.error));
    device.lost.then(info => fail(new Error(`描画が停止しました。ページを再読み込みしてください。 ${info.message}`)));
    const [codeResponse, shadersResponse] = await Promise.all([fetch('game.lua'), fetch('shaders.json')]);
    if (!codeResponse.ok || !shadersResponse.ok) throw new Error('ゲームデータを読み込めませんでした。');
    const code = await codeResponse.text();
    const shaders = await shadersResponse.json();
    window.slangCompile = async (_source, entry) => shaders[entry] || { error: `Unknown shader: ${entry}` };
    window._canvasWidth = 640; window._canvasHeight = 480;
    const module = {
        canvas, preinitializedWebGPUDevice: device, webgpuAdapter: adapter,
        locateFile: path => `${config.wasm || "wasm/"}${path}`,
        arguments: ['game.lua'],
        print: text => console.log(text),
        printErr: text => /error|failed|abort/i.test(text) ? fail(new Error(text)) : console.info(text),
        preRun: [() => {
            module.addRunDependency('game-files');
            const remove = module.removeRunDependency;
            module.removeRunDependency = function(id) {
                remove.call(module, id);
                if (id === 'datafile_lub.data') {
                    module.FS.writeFile('game.lua', code);
                    module.removeRunDependency = remove;
                    remove.call(module, 'game-files');
                }
            };
        }],
    };
    window.Module = module;
    const script = document.createElement('script'); script.src = `${config.wasm || 'wasm/'}lub.js`;
    script.onerror = () => fail(new Error('実行環境を読み込めませんでした。'));
    document.body.append(script);
}
boot().catch(fail);
