import assert from 'node:assert/strict';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
const { chromium } = await import(pathToFileURL(resolve(process.env.PLAYWRIGHT_MODULE || '.cache/browser/node_modules/playwright/index.mjs')));
const base = process.argv[2] || 'http://127.0.0.1:8765/';
const browser = await chromium.launch({ executablePath: process.env.CHROMIUM_PATH, headless: true,
    args: ['--no-sandbox', '--enable-unsafe-webgpu', '--use-angle=swiftshader', '--enable-features=Vulkan,WebGPU', '--use-vulkan=swiftshader', '--disable-vulkan-fallback-to-gl-for-testing'] });
try {
    for (const game of ['a7xpg', 'parsec47', 'gunroar', 'titanion', 'torus-trooper', 'mu-cade']) {
        const page = await browser.newPage({ viewport: { width: 640, height: 900 } });
        const errors = [];
        page.on('pageerror', error => errors.push(error.message));
        page.on('console', message => { if (message.type() === 'error') errors.push(message.text()); });
        await page.route('**/game.lua', async route => {
            const response = await route.fetch();
            const code = (await response.text()).replace(/return Game\s*$/, `
local uploads, imageCount, frames = {}, 0, 0
local useTexture = lub.gfx.use_texture
lub.gfx.use_texture = function(key, w, h, format, data, ...)
 if data and key:sub(1, 6) == 'title-' then uploads[key] = (uploads[key] or 0) + 1 end
 return useTexture(key, w, h, format, data, ...)
end
${game === 'gunroar' ? '' : `local emit = Drawing.emit_image
Drawing.emit_image = function(part, transform)
 emit(part, transform)
 assert(#Drawing.batches[#Drawing.batches].vertices == 48, 'image must be two triangles')
 imageCount = imageCount + 1
end`}
local testImage = DrawImage.new()
testImage.key='title-test'; testImage.width=2; testImage.height=2; testImage.atlas_height=${game === 'a7xpg' ? 2 : 3}; testImage.levels=${game === 'a7xpg' ? 1 : 2}
testImage.pixels={255,0,0,255, 0,255,0,255, 0,0,255,255, 255,255,255,255${game === 'a7xpg' ? '' : ', 17,37,57,255, 0,0,0,0'}}
local testMask = DrawImage.new()
for key,value in pairs(testImage) do testMask[key]=value end
testMask.key='title-test-mask'; testMask.pixels={}
for i,value in ipairs(testImage.pixels) do testMask.pixels[i]=i%4==0 and 0 or value end
local list, outer
local original = Game.on_frame
Game.on_frame = function(dt)
 imageCount=0
 original(0.016)
 ${game === 'gunroar' ? `for _,batch in ipairs(Drawing.batches) do
  if batch.image then
   imageCount=imageCount+1
   assert(batch.count == 6, 'image must be two triangles')
  end
 end` : ''}
 assert(imageCount > 0, 'original title must draw images')
 Drawing.begin_frame(); Drawing.ortho=true; Drawing.viewport_ratio=1
 Drawing.gl_disable(Drawing.gl_depth_test); Drawing.gl_disable(Drawing.gl_cull_face); Drawing.gl_enable(Drawing.gl_blend)
 Drawing.gl_blend_func(Drawing.gl_src_alpha, Drawing.gl_one)
 if not list then
  list=Drawing.gl_gen_lists(1); Drawing.gl_new_list(list, Drawing.gl_compile)
  Drawing.image(testImage,0,0,32,32,1,0.5,0.25,1,false); Drawing.gl_end_list()
  outer=Drawing.gl_gen_lists(1); Drawing.gl_new_list(outer, Drawing.gl_compile)
  Drawing.gl_translatef(5,7,0); Drawing.gl_call_list(list); Drawing.gl_end_list()
 end
 Drawing.gl_push_matrix(); Drawing.gl_translatef(35,33,0); Drawing.gl_call_list(outer); Drawing.gl_pop_matrix()
 Drawing.image(testMask,100,40,32,32,1,1,1,1,true)
 Drawing.image(testImage,160.25,40.25,0.5,0.5,1,1,1,1,false)
 Drawing.color(0.1,0.2,0.3,1); Drawing.gl_begin(Drawing.gl_quads)
 Drawing.gl_vertex2f(200,40); Drawing.gl_vertex2f(210,40); Drawing.gl_vertex2f(210,50); Drawing.gl_vertex2f(200,50); Drawing.gl_end()
 assert(#Drawing.batches==4, 'texture and geometry batch ordering')
 local shader=lub.gfx.use_shader('title-test-shader',GameShaders.vertex,GameShaders.fragment,1)
 local blank=lub.gfx.use_texture('test-blank',1,1,lub.gfx.RGBA8,{0,0,0,0},1)
 frames=frames+1
 lub.gfx.begin_pass({target=lub.gfx.main_tex,clear_color={0.2,0.3,0.4,1}})
 ${game === 'gunroar' ? 'Drawing.render(shader)' : `for index,batch in ipairs(Drawing.batches) do
  local buffer=lub.gfx.use_buffer('title-test-geometry'..index,lub.gfx.STORAGE,batch.vertices,frames)
  local bindings=TextureDrawing.bindings(buffer,batch,index+100,frames);${game === 'a7xpg' ? 'bindings.glow=blank' : ''}
  lub.gfx.draw(#batch.vertices//8,bindings,{shader=shader,cull=lub.gfx.NONE,depth=false,depth_write=false,blend=batch.multiply and lub.gfx.MULTIPLY or lub.gfx.ADDITIVE})
 end`}
 lub.gfx.end_pass()
 for key,count in pairs(uploads) do assert(count==1, 'repeated texture upload: '..key) end
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
        for (const [actual, expected] of [[result.mip, game === 'a7xpg' ? [179, 204, 230] : [68, 113, 159]], [result.geometry, [77, 127, 179]], [result.outside, [51, 76, 102]]]) {
            actual.forEach((value, i) => assert.ok(Math.abs(value - expected[i]) <= 2, `${game}: ${JSON.stringify(result)}`));
        }
        assert.deepEqual(errors, []);
        console.log(`PASS ${game}: image quads, nested display lists, linear repeat, mip selection, tint, multiply, batch order, upload cache`);
        await page.close();
    }
} finally { await browser.close(); }
