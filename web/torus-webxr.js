export function multiply(a, b) {
    const result = new Float32Array(16);
    for (let column = 0; column < 4; column++)
        for (let row = 0; row < 4; row++)
            for (let k = 0; k < 4; k++) result[column * 4 + row] += a[k * 4 + row] * b[column * 4 + k];
    return result;
}

export function anchorPose(transform) {
    const { x, y, z, w } = transform.orientation;
    const yaw = Math.atan2(2 * (x * z + y * w), 1 - 2 * (x * x + y * y));
    const c = Math.cos(yaw), s = Math.sin(yaw), p = transform.position;
    return new Float32Array([c, 0, -s, 0, 0, 1, 0, 0, s, 0, c, 0, p.x, p.y, p.z, 1]);
}

export function eyeMatrix(view, anchor) {
    const matrix = multiply(view.projectionMatrix, multiply(view.transform.inverse.matrix, anchor));
    // WebXR/WebGL uses -w..w depth; the game's WebGPU renderer uses 0..w.
    for (let column = 0; column < 4; column++) matrix[column * 4 + 2] = (matrix[column * 4 + 2] + matrix[column * 4 + 3]) * .5;
    return matrix;
}

export function controllerInput(sources, previous = 0) {
    let mask = 0, view = false, recenter = false;
    const held = (value, bit) => value >= ((previous & bit) ? .25 : .4);
    for (const source of sources) {
        const pad = source.gamepad;
        if (!pad || pad.mapping !== 'xr-standard') continue;
        const button = index => pad.buttons[index]?.pressed || false;
        if (source.handedness === 'left') {
            const x = pad.axes[2] || 0, y = pad.axes[3] || 0;
            if (held(-x, 4)) mask |= 4;
            if (held(x, 8)) mask |= 8;
            if (held(-y, 1)) mask |= 1;
            if (held(y, 2)) mask |= 2;
            if (held(pad.buttons[0]?.value || 0, 32)) mask |= 32;
            if (button(4)) mask |= 64;
            recenter = button(5);
        } else if (source.handedness === 'right') {
            if (held(pad.buttons[0]?.value || 0, 16)) mask |= 16;
            if (button(4)) mask |= 256;
            if (button(5)) mask |= 128;
            view = button(3);
        }
    }
    return { mask, view, recenter };
}

export function frameScheduler(host) {
    const request = host.requestAnimationFrame.bind(host), cancel = host.cancelAnimationFrame.bind(host);
    const pending = new Map();
    let sequence = 0, native = null, immersive = false;
    function schedule() {
        if (!immersive && native === null && pending.size) native = request(time => {
            native = null; flush(time); schedule();
        });
    }
    function flush(time) {
        const callbacks = [...pending.entries()];
        for (const [id, callback] of callbacks) {
            if (!pending.delete(id)) continue;
            callback(time);
        }
    }
    // SDL's Emscripten loop must run inside the XR frame, before copying the stereo canvas.
    host.requestAnimationFrame = callback => { const id = ++sequence; pending.set(id, callback); schedule(); return id; };
    host.cancelAnimationFrame = id => pending.delete(id);
    return {
        flush,
        immersive(value) {
            immersive = value;
            if (native !== null) { cancel(native); native = null; }
            schedule();
        },
    };
}

function presenter(gl) {
    const shaders = [];
    const program = gl.createProgram(), texture = gl.createTexture(), vao = gl.createVertexArray();
    function dispose() {
        shaders.forEach(shader => gl.deleteShader(shader));
        gl.deleteProgram(program); gl.deleteTexture(texture); gl.deleteVertexArray(vao);
    }
    try {
        for (const [type, source] of [
            [gl.VERTEX_SHADER, `#version 300 es
                out vec2 uv;
                void main() {
                    vec2 p = vec2(float((gl_VertexID << 1) & 2), float(gl_VertexID & 2));
                    uv = p; gl_Position = vec4(p * 2.0 - 1.0, 0.0, 1.0);
                }`],
            [gl.FRAGMENT_SHADER, `#version 300 es
                precision highp float;
                uniform sampler2D scene;
                uniform float eye;
                in vec2 uv;
                out vec4 color;
                void main() { color = texture(scene, vec2((uv.x + eye) * 0.5, uv.y)); }`],
        ]) {
            const shader = gl.createShader(type); shaders.push(shader);
            gl.shaderSource(shader, source); gl.compileShader(shader);
            if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) throw new Error(gl.getShaderInfoLog(shader));
            gl.attachShader(program, shader);
        }
        gl.linkProgram(program);
        if (!gl.getProgramParameter(program, gl.LINK_STATUS)) throw new Error(gl.getProgramInfoLog(program));
        const eye = gl.getUniformLocation(program, 'eye');
        gl.bindTexture(gl.TEXTURE_2D, texture);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
        gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, true);
        return {
            dispose,
            draw(canvas, layer, views, rendered) {
                gl.bindFramebuffer(gl.FRAMEBUFFER, layer.framebuffer);
                gl.clearColor(0, 0, 0, 1); gl.clear(gl.COLOR_BUFFER_BIT);
                if (!rendered) return;
                gl.useProgram(program); gl.bindVertexArray(vao);
                gl.bindTexture(gl.TEXTURE_2D, texture);
                gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, canvas);
                for (const view of views) {
                    const viewport = layer.getViewport(view);
                    gl.viewport(viewport.x, viewport.y, viewport.width, viewport.height);
                    gl.uniform1f(eye, view.eye === 'left' ? 0 : 1);
                    gl.drawArrays(gl.TRIANGLES, 0, 3);
                }
            },
        };
    } catch (error) { dispose(); throw error; }
}

export async function createTorusXR({ canvas, button, send, unlockAudio, report }) {
    let ready = false, session = null, pending = false, output, rendered = false, scheduler;
    let savedSize, mask = 0, firstPerson = false, previousView = false, previousRecenter = false;
    const api = { ready() { ready = true; if (scheduler) button.disabled = false; }, present() { rendered = true; } };
    button.hidden = false;
    try {
        if (!globalThis.isSecureContext || !navigator.xr || !globalThis.XRWebGLLayer ||
            !await navigator.xr.isSessionSupported('immersive-vr')) {
            button.textContent = 'VR非対応'; return api;
        }
    } catch { button.textContent = 'VR非対応'; return api; }
    scheduler = frameScheduler(window);
    button.textContent = 'VRで遊ぶ';
    function cleanup(ended) {
        if (session !== ended) return;
        session = null;
        output?.dispose(); output = null;
        if (savedSize) {
            [canvas.width, canvas.height, window._canvasWidth, window._canvasHeight] = savedSize;
            savedSize = null;
        }
        mask = 0; previousView = false; previousRecenter = false;
        send('xr.end', ''); send('input', '0');
        scheduler.immersive(false);
        button.textContent = 'VRで遊ぶ'; button.disabled = !ready;
        canvas.focus();
    }
    button.onclick = async () => {
        if (pending) return;
        pending = true; button.disabled = true;
        try {
            if (session) { await session.end(); return; }
            unlockAudio();
            const current = await navigator.xr.requestSession('immersive-vr', { requiredFeatures: ['local'] });
            session = current;
            current.addEventListener('end', () => cleanup(current), { once: true });
            const surface = document.createElement('canvas');
            const gl = surface.getContext('webgl2', { xrCompatible: true, alpha: false, antialias: false });
            if (!gl) throw new Error('VR用の描画を初期化できませんでした。');
            surface.addEventListener('webglcontextlost', event => {
                event.preventDefault(); report(new Error('VRの描画が停止しました。'));
                current.end().catch(report);
            });
            await gl.makeXRCompatible();
            if (session !== current) return;
            const layer = new XRWebGLLayer(current, gl, { alpha: false, antialias: false, depth: false, stencil: false });
            current.updateRenderState({ baseLayer: layer, depthNear: .05, depthFar: 500 });
            const reference = await current.requestReferenceSpace('local');
            if (session !== current) return;
            output = presenter(gl);
            let anchor = null;
            reference.addEventListener('reset', () => { anchor = null; });
            savedSize = [canvas.width, canvas.height, window._canvasWidth, window._canvasHeight];
            scheduler.immersive(true); send('xr.wait', '');
            current.addEventListener('visibilitychange', () => {
                if (current.visibilityState !== 'visible') { mask = 0; send('xr.wait', ''); }
            });
            button.textContent = 'VRを終了'; button.disabled = false;
            function frame(time, xrFrame) {
                if (session !== current) return;
                try {
                    const pose = xrFrame.getViewerPose(reference);
                    const views = pose && ['left', 'right'].map(eye => pose.views.find(view => view.eye === eye));
                    const focused = current.visibilityState === 'visible' && views?.every(Boolean);
                    rendered = false;
                    if (focused) {
                        const controls = controllerInput(current.inputSources, mask); mask = controls.mask;
                        if (controls.view && !previousView) firstPerson = !firstPerson;
                        if (!anchor || controls.recenter && !previousRecenter) anchor = anchorPose(pose.transform);
                        previousView = controls.view; previousRecenter = controls.recenter;
                        const viewport = layer.getViewport(views[0]);
                        const scale = Math.min(1, 1536 / Math.max(viewport.width, viewport.height));
                        const width = Math.max(1, Math.round(viewport.width * scale)), height = Math.max(1, Math.round(viewport.height * scale));
                        if (canvas.width !== width * 2 || canvas.height !== height) {
                            canvas.width = window._canvasWidth = width * 2; canvas.height = window._canvasHeight = height;
                        }
                        send('xr.frame', [width, height, mask, firstPerson ? 1 : 0, ...eyeMatrix(views[0], anchor), ...eyeMatrix(views[1], anchor)].join(','));
                    } else { mask = 0; send('xr.wait', ''); }
                    scheduler.flush(time);
                    output.draw(canvas, layer, focused ? views : [], rendered && focused);
                    current.requestAnimationFrame(frame);
                } catch (error) { report(error); current.end().catch(report); }
            }
            current.requestAnimationFrame(frame);
        } catch (error) {
            const current = session;
            if (current) {
                try { await current.end(); } catch (endError) { report(endError); }
                cleanup(current);
            }
            report(error);
        } finally { pending = false; button.disabled = !ready; }
    };
    return api;
}
