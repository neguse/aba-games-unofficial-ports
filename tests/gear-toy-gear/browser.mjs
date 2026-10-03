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
    // test commands: 0 boss, 1 finish, 2 cue, 3 stop
    async function command(id,value=0){await page.evaluate(payload=>lubHost.queue.push({topic:'test',payload}),`${id},${value}`);}
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
    await command(0);
    await page.waitForFunction(()=>gameState[8]>=3);await screenshot('boss');
    assert.ok(await page.evaluate(()=>window.miniaudio?.devices.some(device=>device?.webaudio.state==='running')));
    const loops=await page.evaluate(()=>gameState[12]);
    await command(2);await page.waitForFunction(n=>gameState[12]===n+1,loops);
    await command(3);await page.waitForFunction(n=>gameState[12]<=n,loops);
    await command(1);
    await page.waitForFunction(()=>gameState[0]===0&&gameState[7]===7654321&&gameState[9]===1);
    await page.waitForFunction(()=>localStorage.getItem('gear-toy-gear-scores-v1')?.startsWith('7654321,'));
    await screenshot('replay');
    await page.locator('#sound').click();assert.equal(await page.locator('#sound').getAttribute('aria-pressed'),'true');
    await page.reload();await observe();await page.waitForFunction(()=>gameState[7]===7654321);
    // A ranking stored by the earlier page, which kept the same text under the same key.
    await page.evaluate(()=>localStorage.setItem('gear-toy-gear-scores-v1','8000000,90000,80000,70000,60000,50000,40000,30000,20000,10000'));
    await page.reload();await observe();await page.waitForFunction(()=>gameState[7]===8000000);
    await page.keyboard.down('F1');await page.waitForFunction(()=>gameState[0]===1);await release('F1');
    await page.keyboard.down('Escape');await page.waitForFunction(()=>gameState[0]===0);await release('Escape');
    const count=await page.evaluate(async()=>{
        const config=JSON.parse(document.querySelector('#game-config').textContent),audio=new AudioContext();
        const names=[...config.music,...config.sounds];
        for(const name of names){const response=await fetch(`audio/${name}.wav`);if(!response.ok)throw new Error(name);const buffer=await audio.decodeAudioData(await response.arrayBuffer());if(!(buffer.duration>0))throw new Error(name);}
        await audio.close();return names.length;
    });
    assert.equal(count,11);assert.deepEqual(errors,[]);
    console.log('PASS: movement, automatic shots, acceleration/braking, pause, boss rendering, replay, audio, looped cues, mute, 11 audio decodes and ranking reload');
}finally{await browser.close();}
