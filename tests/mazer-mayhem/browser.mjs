import assert from 'node:assert/strict';
import { mkdir } from 'node:fs/promises';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
const { chromium } = await import(pathToFileURL(resolve(process.env.PLAYWRIGHT_MODULE || '.cache/browser/node_modules/playwright/index.mjs')));
const url = process.argv[2] || 'http://127.0.0.1:8765/mazer-mayhem/';
const browser = await chromium.launch({ executablePath: process.env.CHROMIUM_PATH, headless: true,
    args: ['--no-sandbox', '--enable-unsafe-webgpu', '--use-angle=swiftshader', '--enable-features=Vulkan,WebGPU', '--use-vulkan=swiftshader', '--disable-vulkan-fallback-to-gl-for-testing'] });
const errors=[];
try {
    await mkdir('build/screenshots',{recursive:true});
    const page=await browser.newPage({viewport:{width:960,height:960}});
    page.on('pageerror',e=>errors.push(e.message));page.on('console',m=>{if(m.type()==='error')errors.push(m.text());});
    await page.addInitScript(()=>{
        window.audioStarts=0;const Native=window.AudioContext;
        window.AudioContext=class extends Native {createBufferSource(){const source=super.createBufferSource(),start=source.start.bind(source);source.start=(...a)=>{window.audioStarts++;return start(...a);};return source;}};
    });
    await page.route('**/game.lua',async route=>{
        const response=await route.fetch();let code=await response.text();assert.match(code,/return Game\s*$/);
        code=code.replace('if topic == "scores" then',`if topic == "test.bonus" then for i=1,100 do Game.frame.player:get_bonus(Game.frame.player.stored_pos) end end
if topic == "test.boss" then Game.frame.stage.appearance_wait_cnt=0;Game.frame.stage.appearance_cnt_dec=31 end
if topic == "test.finish" then local p=Game.frame.player;p.score=7654321;p.left=0;p:destroy();p.gameover_cnt=599 end
if topic == "scores" then`);
        code=code.replace(/return Game\s*$/,`local frame=Game.on_frame
function Game.on_frame(dt)
 frame(dt)
 local f=Game.frame;local p=f.player;local boss=0
 for i=1,f.balls:length()do if f.balls.actors[i].base_radius>5 then boss=boss+1 end end
 lub.host.send('test.state',table.concat({f.state,p.stored_pos.x,p.stored_pos.y,p.deg,p.shot_cnt,p.dash_cnt,p.stored_is_in_hyper and 1 or 0,f.stored_pause_cnt,p.cnt,f.record.stored_scores[1],boss,p.is_in_replay and 1 or 0,Pad.input,f.grenades:length()},','))
end
return Game`);
        await route.fulfill({response,body:code});
    });
    async function observe(){
        await page.locator('#status').waitFor({state:'hidden',timeout:60000});
        await page.evaluate(()=>{const onMessage=lubHost.onMessage;lubHost.onMessage=(topic,bytes)=>{if(topic==='test.state')window.gameState=new TextDecoder().decode(bytes).split(',').map(Number);else onMessage(topic,bytes);};});
        await page.waitForFunction(()=>window.gameState);
    }
    async function release(key){await page.keyboard.up(key);await page.waitForFunction(()=>gameState[12]===0);}
    async function screenshot(name){
        const shot=await page.locator('#canvas').screenshot({path:`build/screenshots/mazer-${name}.png`});
        const count=await page.evaluate(async png=>{
            const image=new Image();image.src='data:image/png;base64,'+png;await image.decode();
            const copy=document.createElement('canvas');copy.width=image.width;copy.height=image.height;const ctx=copy.getContext('2d');ctx.drawImage(image,0,0);
            const pixels=ctx.getImageData(0,0,image.width,image.height).data;let count=0;
            for(let i=0;i<pixels.length;i+=4)if(Math.min(pixels[i],pixels[i+1],pixels[i+2])<180)count++;return count;
        },shot.toString('base64'));
        assert.ok(count>1000,`${name} visible pixels: ${count}`);
    }
    await page.goto(url);await observe();await screenshot('title');
    await page.keyboard.down('x');await page.waitForFunction(()=>gameState[0]===1&&gameState[4]>0);await release('x');
    const x=await page.evaluate(()=>gameState[1]);
    await page.keyboard.down('ArrowRight');await page.waitForFunction(x=>Math.abs(gameState[1]-x)>1,x);await release('ArrowRight');
    await page.keyboard.down('z');await page.waitForFunction(()=>gameState[3]<-.1);await release('z');
    const deg=await page.evaluate(()=>gameState[3]);
    await page.keyboard.down('c');await page.waitForFunction(d=>gameState[3]>d+.1,deg);await release('c');
    await page.keyboard.down('x');await page.waitForFunction(()=>gameState[5]>0&&gameState[13]>0);await release('x');
    await screenshot('playing');
    await page.keyboard.down('F1');await page.waitForFunction(()=>gameState[7]>=0);await release('F1');
    const paused=await page.evaluate(()=>gameState[8]);await page.waitForTimeout(350);assert.equal(await page.evaluate(()=>gameState[8]),paused);
    await screenshot('paused');await page.keyboard.down('F1');await page.waitForFunction(()=>gameState[7]<0);await release('F1');
    await page.evaluate(()=>lubHost.queue.push({topic:'test.bonus',payload:''}));
    await page.keyboard.down('z');await page.keyboard.down('c');await page.waitForFunction(()=>gameState[6]===1);
    await page.keyboard.up('z');await release('c');await screenshot('hyper');
    await page.evaluate(()=>lubHost.queue.push({topic:'test.boss',payload:''}));
    await page.waitForFunction(()=>gameState[10]>0);await screenshot('boss');
    await page.evaluate(()=>lubHost.queue.push({topic:'test.finish',payload:''}));
    await page.waitForFunction(()=>gameState[0]===0&&gameState[9]===7654321&&gameState[11]===1);
    await page.waitForFunction(()=>localStorage.getItem('mazer-mayhem-scores-v1')?.startsWith('7654321,'));
    await screenshot('replay');assert.ok(await page.evaluate(()=>audioStarts>0));
    await page.reload();await observe();await page.waitForFunction(()=>gameState[9]===7654321);
    await page.keyboard.down('F1');await page.waitForFunction(()=>gameState[0]===1);await release('F1');
    await page.keyboard.down('Escape');await page.waitForFunction(()=>gameState[0]===0);await release('Escape');
    const count=await page.evaluate(async()=>{
        const config=JSON.parse(document.querySelector('#game-config').textContent),audio=new AudioContext();
        const names=[...config.music,...config.sounds];
        for(const name of names){const response=await fetch(`audio/${name}.wav`);if(!response.ok)throw new Error(name);const buffer=await audio.decodeAudioData(await response.arrayBuffer());if(!(buffer.duration>0))throw new Error(name);}
        await audio.close();return names.length;
    });
    assert.equal(count,14);assert.deepEqual(errors,[]);
    console.log('PASS: movement, turn, shots/dash/grenade, pause, hyper, boss rendering, replay, 14 audio decodes and ranking reload');
}finally{await browser.close();}
