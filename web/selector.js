import { games } from './game-catalog.js';

const selection = document.querySelector('#selection');
const player = document.querySelector('#player');
const list = document.querySelector('#game-list');
const container = document.querySelector('#game-container');
const status = document.querySelector('#launch-status');
const title = document.querySelector('#playing-title');
const back = document.querySelector('#return-to-list');
const base = new URL('.', import.meta.url);
const buttons = [];
let selected = 0;
let current = null;
let revision = 0;
let transition = Promise.resolve();
let animation;
let held = new Set();
let repeatAt = 0;
let gamepadReady = true;

for (const [index, game] of games.entries()) {
    const button = document.createElement('button');
    button.type = 'button';
    button.dataset.game = game.id;
    button.textContent = game.name;
    button.tabIndex = index === selected ? 0 : -1;
    button.addEventListener('focus', () => select(index, false));
    button.addEventListener('click', () => launch(game));
    buttons.push(button);
    list.append(button);
}
function select(index, focus = true) {
    selected = (index + games.length) % games.length;
    buttons.forEach((button, i) => { button.tabIndex = i === selected ? 0 : -1; });
    if (focus) buttons[selected].focus();
}
function fromHash() {
    const id = new URLSearchParams(location.hash.slice(1)).get('game');
    return games.find(game => game.id === id) || null;
}
function route(game, replace = false) {
    const url = new URL(location.href);
    url.hash = game ? `game=${game.id}` : '';
    history[replace ? 'replaceState' : 'pushState']({ abaGame: game?.id || null }, '', url);
    render(game);
}
function launch(game) {
    if (current?.game === game && !current.closing) return;
    route(game);
}
function returnToList() {
    // Replacing the current entry avoids accumulating a new history entry on every return.
    route(null, true);
}
async function dispose(session) {
    if (!session) return;
    session.closing = true;
    // The same-origin child does synchronous shutdown/save work before its first await.
    // Then wait for XR/audio cleanup before destroying its complete JS/Wasm realm.
    try {
        const cleanup = session.frame.contentWindow?.abaGame?.dispose();
        let timeout;
        try {
            await Promise.race([cleanup, new Promise(resolve => { timeout = setTimeout(resolve, 1500); })]);
        } finally { clearTimeout(timeout); }
    } catch (error) { console.warn('Game cleanup:', error); }
    session.frame.remove();
    if (current === session) current = null;
}
function render(game) {
    const version = ++revision;
    if (current) current.closing = true;
    back.disabled = true;
    status.hidden = false;
    status.textContent = current ? 'ゲームを終了しています…' : '読み込み中…';
    transition = transition.then(async () => {
        await dispose(current);
        if (version !== revision) return;
        if (!game) {
            selection.hidden = false;
            player.hidden = true;
            document.title = 'ABA Games · ゲームを選ぶ';
            gamepadReady = false;
            select(selected);
            return;
        }
        selected = games.indexOf(game);
        selection.hidden = true;
        player.hidden = false;
        title.textContent = game.name;
        document.title = `${game.name} · ABA Games`;
        status.textContent = '読み込み中…';
        back.disabled = false; // Return works while shaders/assets/Wasm are still loading.
        const frame = document.createElement('iframe');
        frame.className = 'game-frame';
        frame.title = `${game.name} ゲーム画面`;
        frame.allow = 'autoplay; fullscreen; xr-spatial-tracking; gamepad';
        frame.allowFullscreen = true;
        current = { frame, game, closing: false };
        container.append(frame);
        const url = new URL(game.path, base);
        url.searchParams.set('embedded', '1');
        // Replace the initial about:blank entry rather than add iframe history entries.
        frame.contentWindow.location.replace(url.href);
        frame.focus();
    }).catch(error => {
        console.error(error);
        status.textContent = 'ゲームを起動できませんでした。ゲーム一覧へ戻って再試行してください。';
        status.hidden = false;
        back.disabled = false;
    });
}
back.addEventListener('click', returnToList);
window.addEventListener('popstate', () => render(fromHash()));
window.addEventListener('hashchange', () => {
    const game = fromHash();
    if (current?.game !== game || current?.closing) render(game);
});
window.addEventListener('message', event => {
    if (!current || current.closing || event.source !== current.frame.contentWindow || event.origin !== base.origin) return;
    if (event.data?.type === 'aba:return') returnToList();
    if (event.data?.type === 'aba:ready') status.hidden = true;
    if (event.data?.type === 'aba:error') {
        status.textContent = String(event.data.message);
        status.hidden = false;
    }
});
window.addEventListener('keydown', event => {
    if (event.altKey && event.code === 'Escape' && current) {
        event.preventDefault(); returnToList(); return;
    }
    if (selection.hidden || !list.contains(document.activeElement) || event.altKey || event.ctrlKey || event.metaKey) return;
    let offset = 0;
    if (event.code === 'ArrowRight' || event.code === 'ArrowDown') offset = 1;
    if (event.code === 'ArrowLeft' || event.code === 'ArrowUp') offset = -1;
    if (offset) { event.preventDefault(); select(selected + offset); }
    if (event.code === 'Home') { event.preventDefault(); select(0); }
    if (event.code === 'End') { event.preventDefault(); select(games.length - 1); }
});
function pollGamepads(time) {
    // Do not consume gameplay buttons. In-game return has a separate UI/chord.
    if (!selection.hidden && !document.hidden && document.hasFocus()) {
        const actions = new Set();
        for (const pad of navigator.getGamepads?.() || []) {
            if (!pad || pad.mapping !== 'standard') continue;
            if (pad.buttons[0]?.pressed) actions.add('start');
            if (pad.buttons[12]?.pressed || pad.buttons[14]?.pressed || pad.axes[0] < -.55 || pad.axes[1] < -.55) actions.add('previous');
            if (pad.buttons[13]?.pressed || pad.buttons[15]?.pressed || pad.axes[0] > .55 || pad.axes[1] > .55) actions.add('next');
        }
        if (!gamepadReady && actions.size === 0) gamepadReady = true;
        if (!gamepadReady) actions.clear();
        for (const action of ['previous', 'next']) {
            if (actions.has(action) && (!held.has(action) || time >= repeatAt)) {
                select(selected + (action === 'next' ? 1 : -1));
                repeatAt = time + (held.has(action) ? 150 : 400);
            }
        }
        if (actions.has('start') && !held.has('start')) launch(games[selected]);
        held = actions;
    }
    animation = requestAnimationFrame(pollGamepads);
}
window.addEventListener('pagehide', () => {
    ++revision;
    cancelAnimationFrame(animation);
    // Synchronous shutdown is important even when the browser cannot await navigation.
    current?.frame.contentWindow?.abaGame?.dispose();
});
window.addEventListener('pageshow', event => {
    if (event.persisted) { animation = requestAnimationFrame(pollGamepads); render(fromHash()); }
});
// A bookmarked game still gets a list entry behind it, so browser Back returns here.
const initial = fromHash();
if (initial) { route(null, true); route(initial); }
else { route(null, true); }
animation = requestAnimationFrame(pollGamepads);
