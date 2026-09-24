import assert from 'node:assert/strict';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
const { chromium } = await import(pathToFileURL(resolve(process.env.PLAYWRIGHT_MODULE || '.cache/browser/node_modules/playwright/index.mjs')));
const url = process.argv[2] || 'http://127.0.0.1:8765/gunroar/';
const browser = await chromium.launch({ executablePath: process.env.CHROMIUM_PATH, headless: true,
    args: ['--no-sandbox', '--enable-unsafe-webgpu', '--use-angle=swiftshader', '--enable-features=Vulkan,WebGPU', '--use-vulkan=swiftshader', '--disable-vulkan-fallback-to-gl-for-testing'] });
try {
    const page = await browser.newPage({ viewport: { width: 640, height: 800 } });
    const errors = [];
    page.on('pageerror', error => errors.push(error.message));
    page.on('console', message => { if (message.type() === 'error') errors.push(message.text()); });
    let reloadCode;
    await page.route('**/game.lua', async route => {
        const response = await route.fetch();
        const code = (await response.text()).replace(/return Game\s*$/, `
local init=Game.on_init
Game.on_init=function() init();lub.config({resource_sweep_after_frames=2}) end
Game.render_revision=function() return 1 end
local frames=0
local function quad(key,x,y,w,h,z,color)
 local m=Mesh.new(key)
 m:vertex(x,y,z,color);m:vertex(x+w,y,z,color)
 m:vertex(x+w,y+h,z,color);m:vertex(x,y+h,z,color);m:quads(0,4)
 return m
end
local function draw(mesh,model,color,width,blend,depth,image,cull)
 if mesh:get_count()==0 then return end
 lub.gfx.draw(mesh:get_count(),mesh:bindings(model,color,width or 1,blend==lub.gfx.ADDITIVE,0,image),
  {shader=Game.shader,depth=depth or false,cull=cull or lub.gfx.NONE,blend=blend or lub.gfx.ADDITIVE})
end
local uploads=0
local upload=lub.gfx.use_buffer
lub.gfx.use_buffer=function(key,kind,data,version)
 if key=='test-retained-vertices' and version==nil then uploads=uploads+1 end
 return upload(key,kind,data,version)
end
Game.on_frame=function(dt)
 frames=frames+1
 local revision=Game.render_revision()
 Game.shader=lub.gfx.use_shader('render-test',GameShaders.vertex,GameShaders.fragment,1)
 if Game.test_revision~=revision then
  Game.test_mesh=quad('test-retained',revision==1 and 0 or 5,0,20,20,0,nil)
  Game.test_revision=revision
 end
 local ortho=Transform.ortho()
 lub.gfx.begin_pass({target=lub.gfx.main_tex,clear_color={0,0,0,1}})
 local placement=Transform.scale(Transform.translate(ortho,20,20,0),2,1,1)
 draw(Game.test_mesh,placement,revision==1 and {1,0,0,1} or {0,1,0,1},1,lub.gfx.ADDITIVE)
 -- Replace the same resource key without relying on a resettable version counter.
 if frames==60 then Game.test_mesh=quad('test-retained',revision==1 and 0 or 5,0,20,20,0,nil) end
 draw(quad('alpha-red',240,20,40,40,0,{1,0,0,0.5}),ortho,nil,1,lub.gfx.ALPHA)
 draw(quad('alpha-blue',260,40,40,40,0,{0,0,1,0.5}),ortho,nil,1,lub.gfx.ALPHA)
 local line=Mesh.new('test-lines')
 for _,p in ipairs({{20,100},{60,100},{100,100},{120,100},{120,120},{160,100},{180,100},{180,120}}) do
  line:vertex(p[1],p[2],0,{0,1,0,1})
 end
 line:line(0,1);line:line_strip(2,3);line:line_strip(5,3,true)
 draw(line,ortho,nil,4,lub.gfx.ADDITIVE)
 draw(line,Transform.translate(ortho,0,120,0),nil,4,lub.gfx.ADDITIVE,false,nil,lub.gfx.FRONT)
 local fan=Mesh.new('test-fan')
 for _,p in ipairs({{210,30},{200,20},{220,20},{220,40},{200,40},{200,20}}) do fan:vertex(p[1],p[2],0,{1,1,0,1}) end
 fan:fan(0,6);draw(fan,ortho,nil,1,lub.gfx.ADDITIVE)
 local gradient=Mesh.new('test-gradient')
 gradient:vertex(400,20,0,{1,0,0,1});gradient:vertex(464,20,0,{0.5,0,0,0});gradient:vertex(400,84,0,{0.5,0,0,0})
 gradient:triangle(0,1,2);draw(gradient,ortho,nil,1,lub.gfx.ADDITIVE)
 draw(quad('near-red',20,140,40,40,0,{1,0,0,1}),ortho,nil,1,lub.gfx.NONE,true)
 draw(quad('far-blue',30,150,40,40,-0.5,{0,0,1,1}),ortho,nil,1,lub.gfx.NONE,true)
 draw(quad('near-green',40,160,40,40,0.5,{0,1,0,1}),ortho,nil,1,lub.gfx.NONE,true)
 local perspective=Transform.scale(Transform.rotate(Transform.translate(Transform.perspective(),0,0,-10),90,0,0,1),2,2,1)
 draw(quad('perspective',0,-1,1,1,0,{0,1,1,1}),perspective,nil,1,lub.gfx.ADDITIVE)
 local image=DrawImage.new()
 image.key='test-image';image.width=2;image.height=2;image.atlas_height=3;image.levels=2
 image.pixels={255,0,0,255,0,255,0,255,0,0,255,255,255,255,255,255,17,37,57,255,0,0,0,0}
 local sprite=Mesh.new('test-sprite')
 sprite:vertex(500,20,0,{0,0,0,1});sprite:vertex(532,20,0,{1,0,0,1})
 sprite:vertex(532,52,0,{1,1,0,1});sprite:vertex(500,52,0,{0,1,0,1});sprite:quads(0,4)
 draw(sprite,ortho,{1,0.5,0.25,1},1,lub.gfx.ADDITIVE,false,image)
 local small=Transform.scale(Transform.translate(ortho,560.25,20.25,0),0.5/32,0.5/32,1)
 small=Transform.translate(small,-500,-20,0)
 draw(sprite,small,nil,1,lub.gfx.ADDITIVE,false,image)
 lub.gfx.end_pass()
 assert(uploads <= (revision==1 and 2 or 4), 'unchanged mesh must reuse the runtime version')
 lub.host.send('render.frame',tostring(frames)..','..revision)
end
return Game`);
        reloadCode = code.replace('Game.render_revision=function() return 1 end', 'Game.render_revision=function() return 2 end');
        await route.fulfill({ response, body: code });
    });
    await page.goto(url);
    await page.waitForFunction(() => window.lubHost);
    await page.evaluate(() => {
        const original = lubHost.onMessage;
        lubHost.onMessage = (topic, bytes) => topic === 'render.frame' ? window.renderFrame = new TextDecoder().decode(bytes).split(',').map(Number) : original(topic, bytes);
    });
    await page.waitForFunction(() => window.renderFrame?.[0] >= 10);
    await page.evaluate(() => { Module.FS.mkdirTree('/samples/game/.lub'); Module.FS.writeFile('/samples/game/.lub/game.lua', Module.FS.readFile('/game.lua')); });
    await page.addStyleTag({ content: 'canvas {position:fixed;left:0;top:0;width:640px;height:480px;max-height:none;margin:0;border:0;z-index:1000}' });
    await page.waitForFunction(() => window.renderFrame?.[0] >= 100, null, { timeout: 45000 }).catch(error => { throw new Error(`${errors.slice(0,3).join('\n')} ${error.message}`); });
    await page.locator('#canvas').screenshot({ path: 'build/screenshots/gunroar-direct-pre-reload.png' });
    await page.evaluate(code => Module.FS.writeFile('/samples/game/.lub/game.lua', code), reloadCode);
    await page.waitForFunction(() => window.renderFrame?.[1] === 2 && window.renderFrame[0] >= 100, null, { timeout: 45000 }).catch(async error => { throw new Error(`${await page.locator('#status').textContent()} ${errors.join('\n')} ${error.message}`); });
    const png = await page.locator('#canvas').screenshot({ path: 'build/screenshots/gunroar-direct-render-test.png' });
    const points = [
        [40,218,[0,255,0]], [110,220,[0,255,0]], [120,230,[0,255,0]],
        [40,30,[0,255,0]], [19,30,[0,0,0]], [25,30,[0,0,0]], [60,30,[0,255,0]], [70,30,[0,0,0]],
        [210,30,[255,255,0]], [250,30,[128,0,0]], [270,50,[64,0,128]], [290,70,[0,0,128]],
        [40,98,[0,255,0]], [40,101,[0,255,0]], [40,97,[0,0,0]],
        [110,100,[0,255,0]], [120,110,[0,255,0]], [170,100,[0,255,0]], [180,110,[0,255,0]],
        [25,145,[255,0,0]], [35,155,[255,0,0]], [65,155,[0,0,255]], [45,165,[0,255,0]],
        [350,200,[0,255,255]], [416,36,[92,0,0]], [508,28,[240,4,2]], [560,20,[17,37,57]]];
    const samples = await page.evaluate(async ({ png, points }) => {
        const image = new Image(); image.src = `data:image/png;base64,${png}`; await image.decode();
        const canvas = document.createElement('canvas'); canvas.width = 640; canvas.height = 480;
        const context = canvas.getContext('2d'); context.drawImage(image,0,0);
        return points.map(([x,y]) => [...context.getImageData(x,y,1,1).data].slice(0,3));
    }, { png: png.toString('base64'), points });
    for (let i=0;i<points.length;i++) samples[i].forEach((value,channel) =>
        assert.ok(Math.abs(value-points[i][2][channel])<=1, `${points[i].slice(0,2)}: ${samples[i]} != ${points[i][2]}`));
    assert.deepEqual(errors, []);
    console.log('PASS: gfx transforms, retained mesh replacement/cache/sweep and live reload, fans/quads, alpha interpolation/order, lines, depth, title tint/mipmaps');
} finally { await browser.close(); }
