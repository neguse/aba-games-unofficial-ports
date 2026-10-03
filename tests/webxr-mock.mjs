// Page init script: an immersive session with two controllers whose frames read back what the
// game drew. `xrTest` holds the controls and the per-frame observations.
export function mockXR() {
    const request = window.requestAnimationFrame.bind(window);
    const identity = () => [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1];
    const control = handedness => ({ handedness, gamepad: { mapping: 'xr-standard', axes: [0, 0, 0, 0],
        buttons: Array.from({ length: 6 }, () => ({ value: 0, pressed: false })) } });
    window.xrTest = { supported: !location.search.includes('unsupported'), failure: '', offset: 0, tracking: true, eyes: [0, 0], difference: 0, frames: 0 };
    class Session extends EventTarget {
        visibilityState = 'visible'; inputSources = [control('left'), control('right')]; ended = false;
        updateRenderState(state) { this.renderState = state; }
        async requestReferenceSpace() {
            if (xrTest.failure === 'reference') throw new Error('reference failed');
            return new EventTarget();
        }
        async end() { this.ended = true; this.dispatchEvent(new Event('end')); }
        requestAnimationFrame(callback) {
            request(time => {
                if (this.ended) return;
                const views = [-.032, .032].map((eye, index) => {
                    const view = identity(); view[12] = -eye - xrTest.offset; view[13] = -1.6;
                    return { eye: index ? 'right' : 'left', projectionMatrix: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, -1.0002, -1, 0, 0, -.10001, 0],
                        transform: { position: { x: eye + xrTest.offset, y: 1.6, z: 0 }, orientation: { x: 0, y: 0, z: 0, w: 1 }, inverse: { matrix: view } } };
                });
                callback(time, { getViewerPose: () => xrTest.tracking ? { views, transform: {
                    position: { x: xrTest.offset, y: 1.6, z: 0 }, orientation: { x: 0, y: 0, z: 0, w: 1 },
                } } : null });
                const layer = this.renderState.baseLayer, gl = layer.gl, pixels = new Uint8Array(512 * 256 * 4);
                gl.bindFramebuffer(gl.FRAMEBUFFER, layer.framebuffer);
                gl.readPixels(0, 0, 512, 256, gl.RGBA, gl.UNSIGNED_BYTE, pixels);
                if (xrTest.compare) {
                    const copy = document.createElement('canvas'); copy.width = 512; copy.height = 256;
                    const context = copy.getContext('2d'); context.drawImage(document.querySelector('#canvas'), 0, 0);
                    const source = context.getImageData(0, 0, 512, 256).data;
                    let differences = 0;
                    for (let y = 0; y < 256; y++) for (let x = 0; x < 512; x++) for (let c = 0; c < 3; c++)
                        if (Math.abs(pixels[(y * 512 + x) * 4 + c] - source[((255 - y) * 512 + x) * 4 + c]) > 2) differences++;
                    xrTest.copyDifferences = differences; xrTest.compare = false;
                }
                xrTest.eyes = [0, 0]; xrTest.difference = 0; xrTest.colored = 0;
                for (let y = 0; y < 256; y++) for (let x = 0; x < 256; x++) {
                    const a = (y * 512 + x) * 4, b = a + 256 * 4;
                    if (Math.max(...pixels.slice(a, a + 3)) > 30) xrTest.eyes[0]++;
                    if (Math.max(...pixels.slice(b, b + 3)) > 30) xrTest.eyes[1]++;
                    if (pixels[a] !== pixels[b] || pixels[a + 1] !== pixels[b + 1]) xrTest.difference++;
                    if (Math.max(pixels[a], pixels[a + 1], pixels[a + 2]) - Math.min(pixels[a], pixels[a + 1], pixels[a + 2]) > 20) xrTest.colored++;
                }
                xrTest.frames++;
            });
        }
    }
    Object.defineProperty(navigator, 'xr', { value: {
        isSessionSupported: async () => xrTest.supported,
        requestSession: async () => {
            if (xrTest.failure === 'request') throw new Error('request denied');
            return xrTest.session = new Session();
        },
    } });
    window.XRWebGLLayer = class {
        constructor(session, gl) {
            this.gl = gl; this.framebuffer = gl.createFramebuffer();
            const texture = gl.createTexture(); gl.bindTexture(gl.TEXTURE_2D, texture);
            gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA8, 512, 256, 0, gl.RGBA, gl.UNSIGNED_BYTE, null);
            gl.bindFramebuffer(gl.FRAMEBUFFER, this.framebuffer);
            gl.framebufferTexture2D(gl.FRAMEBUFFER, gl.COLOR_ATTACHMENT0, gl.TEXTURE_2D, texture, 0);
            session.addEventListener('end', () => { gl.deleteTexture(texture); gl.deleteFramebuffer(this.framebuffer); });
        }
        getViewport(view) { return { x: view.eye === 'left' ? 0 : 256, y: 0, width: 256, height: 256 }; }
    };
    WebGL2RenderingContext.prototype.makeXRCompatible = async function() {};
}
