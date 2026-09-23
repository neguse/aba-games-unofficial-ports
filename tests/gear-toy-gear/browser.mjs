import assert from 'node:assert/strict';
import { mkdir } from 'node:fs/promises';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
const { chromium } = await import(pathToFileURL(resolve(process.env.PLAYWRIGHT_MODULE || '.cache/browser/node_modules/playwright/index.mjs')));
const url = process.argv[2] || 'http://127.0.0.1:8765/gear-toy-gear/';
const browser = await chromium.launch({ executablePath: process.env.CHROMIUM_PATH, headless: true,
    args: ['--no-sandbox', '--enable-unsafe-webgpu', '--use-angle=swiftshader', '--enable-features=Vulkan,WebGPU', '--use-vulkan=swiftshader', '--disable-vulkan-fallback-to-gl-for-testing'] });
const errors=[];
try {
    await mkdir('build/screenshots',{recursive:true});
    const page=await browser.newPage({viewport:{width:960,height:960}});
    page.on('pageerror',e=>errors.push(e.message));page.on('console',m=>{if(m.type()==='error')errors.push(m.text());});
    await page.addInitScript(()=>{
        window.audioStarts=0;window.spatialStarts=0;window.loopStops=0;const Native=window.AudioContext;
        window.AudioContext=class extends Native {
            createBufferSource(){const source=super.createBufferSource(),start=source.start.bind(source),stop=source.stop.bind(source);source.start=(...a)=>{window.audioStarts++;return start(...a);};source.stop=(...a)=>{if(source.loop)window.loopStops++;return stop(...a);};return source;}
            createPanner(){window.spatialStarts++;return super.createPanner();}
        };
    });
    await page.route('**/game.lua',async route=>{
        const response=await route.fetch();let code=await response.text();assert.match(code,/return Game\s*$/);
        code=code.replace('if topic == "scores" then',`if topic == "test.boss" then local a=Game.frame.actors;a.stage.stage_count=6;a.stage.stage_ticks=0;a.stage:go_to_next_stage();a.stage.stage_ticks=421 end
if topic == "test.finish" then local a=Game.frame.actors;a.game_state.score=7654321;a.game_state.left=0;a.player.invincible_ticks=-1;a.player:destroy();a.game_state.game_over_ticks=2 end
if topic == "test.cue" then local s=Game.frame.sound;Game.test_cue=s:get_cue('HomingLaser');Game.test_cue:apply3_d(AudioListener.new(),AudioEmitter.new());Game.test_cue:play() end
if topic == "test.stop" then Game.test_cue:stop(0) end
if topic == "scores" then`);
        code=code.replace(/return Game\s*$/,`local frame=Game.on_frame
function Game.on_frame(dt)
 frame(dt)
 local f=Game.frame;local a=f.actors;local p=a.player
 lub.host.send('test.state',table.concat({f.state,p.pos.x,p.pos.y,Stage.game_speed,a.shots:get_count(),f.stored_pause_ticks,a.stage.ticks,f.record.stored_scores[1],a.middle_enemies:get_count(),p.is_in_replay and 1 or 0,Pad.input,a.player_homing_lasers:get_count()},','))
end
return Game`);
        await route.fulfill({response,body:code});
    });
    async function observe(){
        await page.locator('#status').waitFor({state:'hidden',timeout:60000});
        await page.evaluate(()=>{const onMessage=lubHost.onMessage;lubHost.onMessage=(topic,bytes)=>{if(topic==='test.state')window.gameState=new TextDecoder().decode(bytes).split(',').map(Number);else onMessage(topic,bytes);};});
        await page.waitForFunction(()=>window.gameState);
    }
    async function release(key){await page.keyboard.up(key);await page.waitForFunction(()=>gameState[10]===0);}
    async function screenshot(name){
        const shot=await page.locator('#canvas').screenshot({path:`build/screenshots/gear-${name}.png`});
        const count=await page.evaluate(async png=>{
            const image=new Image();image.src='data:image/png;base64,'+png;await image.decode();
            const copy=document.createElement('canvas');copy.width=image.width;copy.height=image.height;const ctx=copy.getContext('2d');ctx.drawImage(image,0,0);
            const pixels=ctx.getImageData(0,0,image.width,image.height).data;let color=0,white=0;
            for(let i=0;i<pixels.length;i+=4){if(Math.max(pixels[i],pixels[i+1],pixels[i+2])-Math.min(pixels[i],pixels[i+1],pixels[i+2])>30)color++;if(Math.min(pixels[i],pixels[i+1],pixels[i+2])>180)white++;}return {color,white};
        },shot.toString('base64'));
        assert.ok(count.color>1000&&count.white>100,`${name} visible pixels: ${JSON.stringify(count)}`);
    }
    await page.goto(url);await observe();await screenshot('title');
    await page.keyboard.down('x');await page.waitForFunction(()=>gameState[0]===1&&gameState[3]>1.1&&gameState[4]>0);await release('x');
    const speed=await page.evaluate(()=>gameState[3]);
    await page.keyboard.down('z');await page.waitForFunction(s=>gameState[3]<s-.05,speed);await release('z');
    const x=await page.evaluate(()=>gameState[1]);
    await page.keyboard.down('ArrowRight');await page.waitForFunction(x=>Math.abs(gameState[1]-x)>1,x);await release('ArrowRight');
    await screenshot('playing');
    await page.keyboard.down('p');await page.waitForFunction(()=>gameState[5]>=0);await release('p');
    const paused=await page.evaluate(()=>gameState[6]);await page.waitForTimeout(350);assert.equal(await page.evaluate(()=>gameState[6]),paused);
    await screenshot('paused');await page.keyboard.down('p');await page.waitForFunction(()=>gameState[5]<0);await release('p');
    await page.evaluate(()=>lubHost.queue.push({topic:'test.boss',payload:''}));
    await page.waitForFunction(()=>gameState[8]>=3);await screenshot('boss');
    await page.evaluate(()=>lubHost.queue.push({topic:'test.cue',payload:''}));
    await page.waitForFunction(()=>spatialStarts>0);await page.waitForTimeout(300);
    await page.evaluate(()=>lubHost.queue.push({topic:'test.stop',payload:''}));await page.waitForFunction(()=>loopStops>0);
    await page.evaluate(()=>lubHost.queue.push({topic:'test.finish',payload:''}));
    await page.waitForFunction(()=>gameState[0]===0&&gameState[7]===7654321&&gameState[9]===1);
    await page.waitForFunction(()=>localStorage.getItem('gear-toy-gear-scores-v1')?.startsWith('7654321,'));
    await screenshot('replay');assert.ok(await page.evaluate(()=>audioStarts>0));
    await page.locator('#sound').click();assert.equal(await page.locator('#sound').getAttribute('aria-pressed'),'true');
    await page.reload();await observe();await page.waitForFunction(()=>gameState[7]===7654321);
    await page.keyboard.down('F1');await page.waitForFunction(()=>gameState[0]===1);await release('F1');
    await page.keyboard.down('Escape');await page.waitForFunction(()=>gameState[0]===0);await release('Escape');
    const count=await page.evaluate(async()=>{
        const config=JSON.parse(document.querySelector('#game-config').textContent),audio=new AudioContext();
        const names=[...config.music,...config.sounds];
        for(const name of names){const response=await fetch(`audio/${name}.wav`);if(!response.ok)throw new Error(name);const buffer=await audio.decodeAudioData(await response.arrayBuffer());if(!(buffer.duration>0))throw new Error(name);}
        await audio.close();return names.length;
    });
    assert.equal(count,11);assert.deepEqual(errors,[]);
    console.log('PASS: movement, automatic shots, acceleration/braking, pause, boss rendering, replay, spatial loops, mute, 11 audio decodes and ranking reload');
}finally{await browser.close();}
