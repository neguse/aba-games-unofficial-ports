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
    await page.route('**/game.lua', async route => {
        const response = await route.fetch();
        const code = (await response.text()).replace(/return Game\s*$/, `
local frames,uploads,inner,outer=0,0,nil,nil
local upload=lub.gfx.use_buffer
lub.gfx.use_buffer=function(key,...)
 if key=='static-geometry' then uploads=uploads+1 end
 return upload(key,...)
end
local function quad(x,y,w,h,z)
 Drawing.gl_begin(Drawing.gl_quads)
 Drawing.gl_vertex3f(x,y,z);Drawing.gl_vertex3f(x+w,y,z)
 Drawing.gl_vertex3f(x+w,y+h,z);Drawing.gl_vertex3f(x,y+h,z)
 Drawing.gl_end()
end
local init=Game.on_init
Game.on_init=function() init();lub.config({resource_sweep_after_frames=2}) end
Game.on_frame=function(dt)
 frames=frames+1
 Drawing.begin_frame();Drawing.ortho=true;Drawing.viewport_ratio=1
 Drawing.gl_disable(Drawing.gl_depth_test);Drawing.gl_disable(Drawing.gl_cull_face)
 Drawing.gl_enable(Drawing.gl_blend);Drawing.gl_blend_func(Drawing.gl_src_alpha,Drawing.gl_one)
 if not inner then
  inner=Drawing.gl_gen_lists(1);Drawing.gl_new_list(inner,Drawing.gl_compile)
  quad(0,0,20,20,0);Drawing.gl_end_list()
  outer=Drawing.gl_gen_lists(1);Drawing.gl_new_list(outer,Drawing.gl_compile)
  Drawing.gl_translatef(5,7,0);Drawing.color(1,0,0,1);Drawing.gl_call_list(inner)
  Drawing.gl_end_list()
 end
 if frames==90 then
  Drawing.gl_delete_lists(outer,1)
  Drawing.gl_new_list(outer,Drawing.gl_compile)
  Drawing.gl_translatef(5,7,0);Drawing.color(0,0,1,1);Drawing.gl_call_list(inner)
  Drawing.gl_end_list()
 end
 Drawing.color(0,1,0,1)
 Drawing.gl_push_matrix();Drawing.gl_translatef(20,20,0);Drawing.gl_scalef(2,1,1)
 Drawing.gl_call_list(inner);Drawing.gl_pop_matrix()
 Drawing.gl_push_matrix();Drawing.gl_translatef(95,33,0)
 Drawing.gl_call_list(outer);Drawing.gl_pop_matrix()
 assert(Drawing.red==0 and Drawing.blue==1 or frames<90, 'list color side effect')
 Drawing.color(0,0,1,1);Drawing.gl_begin(Drawing.gl_triangle_strip)
 for _,p in ipairs({{160,20},{180,20},{160,40},{180,40}}) do Drawing.gl_vertex2f(p[1],p[2]) end
 Drawing.gl_end()
 Drawing.color(1,1,0,1);Drawing.gl_begin(Drawing.gl_triangle_fan)
 for _,p in ipairs({{210,30},{200,20},{220,20},{220,40},{200,40},{200,20}}) do Drawing.gl_vertex2f(p[1],p[2]) end
 Drawing.gl_end()
 Drawing.gl_blend_func(Drawing.gl_src_alpha,Drawing.gl_one_minus_src_alpha)
 Drawing.color(1,0,0,0.5);quad(240,20,40,40,0)
 Drawing.color(0,0,1,0.5);quad(260,40,40,40,0)
 Drawing.gl_blend_func(Drawing.gl_src_alpha,Drawing.gl_one)
 Drawing.color(0,1,0,1);Drawing.gl_line_width(4);Drawing.gl_begin(Drawing.gl_lines)
 Drawing.gl_vertex2f(20,100);Drawing.gl_vertex2f(60,100);Drawing.gl_end()
 Drawing.gl_begin(Drawing.gl_line_strip)
 Drawing.gl_vertex2f(100,100);Drawing.gl_vertex2f(120,100);Drawing.gl_vertex2f(120,120);Drawing.gl_end()
 Drawing.gl_begin(Drawing.gl_line_loop)
 Drawing.gl_vertex2f(160,100);Drawing.gl_vertex2f(180,100);Drawing.gl_vertex2f(180,120);Drawing.gl_end()
 Drawing.gl_line_width(1)
 Drawing.gl_begin(Drawing.gl_triangles)
 Drawing.color(1,0,0,1);Drawing.gl_vertex2f(400,20)
 Drawing.color(0.5,0,0,0);Drawing.gl_vertex2f(464,20);Drawing.gl_vertex2f(400,84)
 Drawing.gl_end()
 Drawing.gl_disable(Drawing.gl_blend);Drawing.gl_enable(Drawing.gl_depth_test)
 Drawing.color(1,0,0,1);quad(20,140,40,40,0)
 Drawing.color(0,0,1,1);quad(30,150,40,40,-0.5)
 Drawing.color(0,1,0,1);quad(40,160,40,40,0.5)
 Drawing.gl_disable(Drawing.gl_depth_test)
 Drawing.ortho=false;Drawing.gl_translatef(0,0,-10);Drawing.gl_rotatef(90,0,0,1);Drawing.gl_scalef(2,2,1)
 Drawing.color(0,1,1,1);quad(0,-1,1,1,0)
 Drawing.gl_begin(Drawing.gl_lines)
 Drawing.gl_vertex3f(0,0,11);Drawing.gl_vertex3f(1,1,11);Drawing.gl_end()
 local shader=lub.gfx.use_shader('render-test',GameShaders.vertex,GameShaders.fragment,1)
 lub.gfx.begin_pass({target=lub.gfx.main_tex,clear_color={0,0,0,1}})
 Drawing.render(shader);lub.gfx.end_pass()
 assert(uploads==(frames<90 and 1 or 2), 'static mesh uploaded without a geometry change')
 lub.host.send('render.frame',tostring(frames))
end
return Game`);
        await route.fulfill({ response, body: code });
    });
    await page.goto(url);
    await page.waitForFunction(() => window.lubHost);
    await page.evaluate(() => {
        const original = lubHost.onMessage;
        lubHost.onMessage = (topic, bytes) => topic === 'render.frame' ? window.renderFrame = Number(new TextDecoder().decode(bytes)) : original(topic, bytes);
    });
    await page.addStyleTag({ content: 'canvas {position:fixed;left:0;top:0;width:640px;height:480px;max-height:none;margin:0;border:0;z-index:1000}' });
    await page.waitForFunction(() => window.renderFrame >= 180, null, { timeout: 45000 }).catch(error => { throw new Error(`${errors.slice(0,3).join('\n')} ${error.message}`); });
    const png = await page.locator('#canvas').screenshot();
    const points = [[40,30,[0,255,0]], [19,30,[0,0,0]], [60,30,[0,0,0]], [110,50,[0,0,255]],
        [170,30,[0,0,255]], [210,30,[255,255,0]], [250,30,[128,0,0]], [270,50,[64,0,128]],
        [290,70,[0,0,128]], [40,98,[0,255,0]], [40,101,[0,255,0]], [40,97,[0,0,0]],
        [110,100,[0,255,0]], [120,110,[0,255,0]], [170,100,[0,255,0]], [180,110,[0,255,0]],
        [25,145,[255,0,0]], [35,155,[255,0,0]], [65,155,[0,0,255]], [45,165,[0,255,0]], [350,200,[0,255,255]], [416,36,[92,0,0]]];
    const samples = await page.evaluate(async ({ png, points }) => {
        const image = new Image(); image.src = `data:image/png;base64,${png}`; await image.decode();
        const canvas = document.createElement('canvas'); canvas.width = 640; canvas.height = 480;
        const context = canvas.getContext('2d'); context.drawImage(image,0,0);
        return points.map(([x,y]) => [...context.getImageData(x,y,1,1).data].slice(0,3));
    }, { png: png.toString('base64'), points });
    for (let i=0;i<points.length;i++) samples[i].forEach((value,channel) =>
        assert.ok(Math.abs(value-points[i][2][channel])<=1, `${points[i].slice(0,2)}: ${samples[i]} != ${points[i][2]}`));
    assert.deepEqual(errors, []);
    console.log('PASS: GPU transforms, nested mesh color inheritance, mesh replacement/cache/sweep, quads/strips/fans, alpha interpolation/order, line width/strips/loops, depth');
} finally { await browser.close(); }
