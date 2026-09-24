import assert from 'node:assert/strict';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
const { chromium } = await import(pathToFileURL(resolve(process.env.PLAYWRIGHT_MODULE || '.cache/browser/node_modules/playwright/index.mjs')));
const base = process.argv[2] || 'http://127.0.0.1:8765/';
const browser = await chromium.launch({ executablePath: process.env.CHROMIUM_PATH, headless: true,
    args: ['--no-sandbox', '--enable-unsafe-webgpu', '--use-angle=swiftshader', '--enable-features=Vulkan,WebGPU', '--use-vulkan=swiftshader', '--disable-vulkan-fallback-to-gl-for-testing'] });
try {
    for (const game of ['mu-cade']) {
        const page = await browser.newPage({ viewport: { width: 640, height: 900 } });
        const errors = [];
        page.on('pageerror', error => errors.push(error.message));
        page.on('console', message => { if (message.type() === 'error') errors.push(message.text()); });
        await page.route('**/game.lua', async route => {
            const response = await route.fetch();
            const code = (await response.text()).replace(/return Game\s*$/, `
local uploads, imageCount, frames = {}, 0, 0
local useTexture = lub.gfx.use_texture
lub.gfx.use_texture = function(key, w, h, format, data, version, ...)
 if key:sub(1,6)=='title-' then
  imageCount=imageCount+1
  if data and version==nil then uploads[key]=(uploads[key] or 0)+1 end
 end
 return useTexture(key,w,h,format,data,version,...)
end
local testImage = DrawImage.new()
testImage.key='title-test';testImage.width=2;testImage.height=2;testImage.atlas_height=3;testImage.levels=2
testImage.pixels={255,0,0,255,0,255,0,255,0,0,255,255,255,255,255,255,17,37,57,255,0,0,0,0}
local testMask = DrawImage.new()
for key,value in pairs(testImage) do testMask[key]=value end
testMask.key='title-test-mask';testMask.pixels={}
for i,value in ipairs(testImage.pixels) do testMask.pixels[i]=i%4==0 and 0 or value end
local quad=Mesh.new('image-quad')
quad:vertex(0,0,0,{0,0,0,1});quad:vertex(32,0,0,{1,0,0,1})
quad:vertex(32,32,0,{1,1,0,1});quad:vertex(0,32,0,{0,1,0,1});quad:quads(0,4)
local geometry=Mesh.new('image-test-geometry')
for _,p in ipairs({{200,40},{210,40},{210,50},{200,50}})do geometry:vertex(p[1],p[2],0,{0.1,0.2,0.3,1})end
geometry:quads(0,4)
local original = Game.on_frame
Game.on_frame = function(dt)
 imageCount=0;original(0.016)
 assert(imageCount>0,'original title must draw images')
 local ortho=Transform.ortho()
 local shader=Game.shader
 frames=frames+1
 lub.gfx.begin_pass({target=lub.gfx.main_tex,clear_color={0.2,0.3,0.4,1}})
 lub.gfx.draw(quad:get_count(),quad:bindings(Transform.translate(ortho,40,40,0),{1,0.5,0.25,1},1,true,0,testImage),
  {shader=shader,cull=lub.gfx.NONE,depth=false,blend=lub.gfx.ADDITIVE})
 lub.gfx.draw(quad:get_count(),quad:bindings(Transform.translate(ortho,100,40,0),nil,1,false,0,testMask),
  {shader=shader,cull=lub.gfx.NONE,depth=false,blend=lub.gfx.MULTIPLY})
 local small=Transform.scale(Transform.translate(ortho,160.25,40.25,0),0.5/32,0.5/32,1)
 lub.gfx.draw(quad:get_count(),quad:bindings(small,nil,1,true,0,testImage),
  {shader=shader,cull=lub.gfx.NONE,depth=false,blend=lub.gfx.ADDITIVE})
 lub.gfx.draw(geometry:get_count(),geometry:bindings(ortho),
  {shader=shader,cull=lub.gfx.NONE,depth=false,blend=lub.gfx.ADDITIVE})
 lub.gfx.end_pass()
 for key,count in pairs(uploads)do assert(count==1,'repeated texture upload: '..key)end
 lub.host.send('test.images',tostring(frames))
end
return Game`);
            await route.fulfill({ response, body: code });
        });
        await page.goto(new URL(`${game}/`, base).href);
        await page.locator('#status').waitFor({ state: 'hidden', timeout: 45000 }).catch(async error => { throw new Error(`${game}: ${await page.locator('#status').textContent()} ${errors.slice(0, 2).join('\n')} ${error.message}`); });
        await page.addStyleTag({ content: 'main {width:640px} canvas {position:fixed;left:0;top:0;width:640px;height:480px;max-height:none;margin:0;border:0;z-index:1000}' });
        await page.evaluate(() => {
            const original = lubHost.onMessage;
            lubHost.onMessage = (topic, bytes) => topic === 'test.images' ? window.imageFrames = Number(new TextDecoder().decode(bytes)) : original(topic, bytes);
        });
        await page.waitForFunction(() => window.imageFrames >= 5, null, { timeout: 15000 }).catch(error => { throw new Error(`${game}: ${errors.slice(0, 2).join('\n')} ${error.message}`); });
        const png = await page.locator('#canvas').screenshot({ path: `build/screenshots/${game}-texture-test.png` });
        const result = await page.evaluate(async png => {
            const image = new Image(); image.src = `data:image/png;base64,${png}`; await image.decode();
            const canvas = document.createElement('canvas'); canvas.width = 640; canvas.height = 480;
            const context = canvas.getContext('2d'); context.drawImage(image, 0, 0);
            const pixels = context.getImageData(0, 0, 640, 480).data;
            const sample = (x, y) => [...pixels.slice((y * 640 + x) * 4, (y * 640 + x) * 4 + 3)];
            let maxError = 0;
            const texels = [[255, 0, 0], [0, 255, 0], [0, 0, 255], [255, 255, 255]];
            for (const [left, multiply, tint] of [[40, false, [1, 0.5, 0.25]], [100, true, [1, 1, 1]]]) {
                for (let y = 40; y < 72; y++) for (let x = left; x < left + 32; x++) {
                    const u = (x + 0.5 - left) / 16 - 0.5, v = (y + 0.5 - 40) / 16 - 0.5;
                    const ix = Math.floor(u), iy = Math.floor(v), fx = u - ix, fy = v - iy;
                    for (let channel = 0; channel < 3; channel++) {
                        const at = (dx, dy) => texels[((iy + dy + 2) % 2) * 2 + (ix + dx + 2) % 2][channel];
                        const value = ((at(0, 0) * (1 - fx) + at(1, 0) * fx) * (1 - fy) + (at(0, 1) * (1 - fx) + at(1, 1) * fx) * fy) * tint[channel];
                        const background = [51, 76, 102][channel];
                        const expected = multiply ? background * value / 255 : Math.min(255, background + value);
                        maxError = Math.max(maxError, Math.abs(sample(x, y)[channel] - expected));
                    }
                }
            }
            return { maxError, mip: sample(160, 40), geometry: sample(205, 45), outside: sample(20, 20) };
        }, png.toString('base64'));
        assert.ok(result.maxError <= 2, `${game}: ${JSON.stringify(result)}`);
        for (const [actual, expected] of [[result.mip, [68, 113, 159]], [result.geometry, [77, 127, 179]], [result.outside, [51, 76, 102]]]) {
            actual.forEach((value, i) => assert.ok(Math.abs(value - expected[i]) <= 2, `${game}: ${JSON.stringify(result)}`));
        }
        assert.deepEqual(errors, []);
        console.log(`PASS ${game}: image quads, transforms, linear repeat, mip selection, tint, multiply, draw order, upload cache`);
        await page.close();
    }
} finally { await browser.close(); }
