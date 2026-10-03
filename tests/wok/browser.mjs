import assert from 'node:assert/strict';
import { mkdir } from 'node:fs/promises';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
const { chromium } = await import(pathToFileURL(resolve(process.env.PLAYWRIGHT_MODULE || '.cache/browser/node_modules/playwright/index.mjs')));
const url = process.argv[2] || 'http://127.0.0.1:8765/wok/';
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
    // test commands: 0 generators(kind), 1 finish, 2 music
    async function command(id,value=0){await page.evaluate(payload=>lubHost.queue.push({topic:'test',payload}),`${id},${value}`);}
    async function observe(){
        await page.locator('#status').waitFor({state:'hidden',timeout:60000});
        await page.evaluate(()=>{const onMessage=lubHost.onMessage;lubHost.onMessage=(topic,bytes)=>{if(topic==='test.state')window.gameState=new TextDecoder().decode(bytes).split(',').map(Number);else onMessage(topic,bytes);};});
        await page.waitForFunction(()=>window.gameState);
    }
    await page.goto(url);await observe();await page.screenshot({path:'build/screenshots/wok-title.png'});
    const box=await page.locator('#canvas').boundingBox();
    async function start(){
        await page.mouse.click(box.x+520/640*box.width,box.y+315/480*box.height);
        await page.waitForFunction(()=>gameState[0]===1);
        await page.waitForFunction(()=>document.pointerLockElement===document.querySelector('#canvas'));
    }
    await start();
    // Headless pointer lock reports each real move as a jump and its inverse, so relative moves are dispatched directly.
    const nudge=(x,y)=>page.evaluate(([x,y])=>document.querySelector('#canvas').dispatchEvent(new PointerEvent('pointermove',{movementX:x,movementY:y})),[x,y]);
    // A late jump from the lock can undo a nudge, so each nudge repeats until it shows.
    async function nudged(x,y,reached,argument){
        for(let attempt=0;;attempt++){
            await nudge(x,y);
            try{await page.waitForFunction(reached,argument,{timeout:2000});return;}
            catch(error){if(attempt===4)throw error;}
        }
    }
    await nudged(50,0,()=>gameState[1]>100);
    const x=await page.evaluate(()=>gameState[1]);
    await nudged(-20,0,x=>gameState[1]<x-10,x);
    await nudge(-2,-2);
    await page.waitForFunction(()=>Math.abs(gameState[3])>0.01);
    await page.screenshot({path:'build/screenshots/wok-playing.png'});
    await page.keyboard.down('Escape');await page.waitForFunction(()=>gameState[0]===0);await page.keyboard.up('Escape');
    await page.waitForFunction(()=>document.pointerLockElement===null);
    await page.waitForTimeout(1300);await start();
    for(let kind=0;kind<6;kind++){
        await command(0,kind);
        await page.waitForFunction(()=>gameState[6]>0);await page.waitForTimeout(300);
        const shot=await page.locator('#canvas').screenshot({path:`build/screenshots/wok-generator-${kind}.png`});
        const colored=await page.evaluate(async png=>{
            const image=new Image();image.src='data:image/png;base64,'+png;await image.decode();
            const copy=document.createElement('canvas');copy.width=image.width;copy.height=image.height;const ctx=copy.getContext('2d');ctx.drawImage(image,0,0);
            const pixels=ctx.getImageData(0,0,image.width*0.9,image.height).data;let count=0;
            for(let i=0;i<pixels.length;i+=4)if(Math.min(pixels[i],pixels[i+1],pixels[i+2])<200)count++;return count;
        },shot.toString('base64'));
        assert.ok(colored>500,`sprite pixels ${kind}: ${colored}`);
    }
    await command(2);
    await page.waitForFunction(()=>gameState[7]===1);assert.ok(await page.evaluate(()=>audioStarts>0));
    await command(1);
    await page.waitForFunction(()=>gameState[0]===3&&gameState[5]===7654321);
    await page.waitForFunction(()=>localStorage.getItem('wok-scores-v1')==='7654321');
    await page.screenshot({path:'build/screenshots/wok-gameover.png'});
    await page.reload();await observe();await page.waitForFunction(()=>gameState[0]===0&&gameState[5]===7654321);
    const count=await page.evaluate(async()=>{
        const config=JSON.parse(document.querySelector('#game-config').textContent),audio=new AudioContext();
        const names=[...config.music.map(n=>`${n}.${config.musicExtension}`),...config.sounds.map(n=>`${n}.wav`)];
        for(const name of names){const response=await fetch(`audio/${name}`);if(!response.ok)throw new Error(name);const buffer=await audio.decodeAudioData(await response.arrayBuffer());if(!(buffer.duration>0))throw new Error(name);}
        await audio.close();return names.length;
    });
    assert.equal(count,5);assert.deepEqual(errors,[]);
    console.log('PASS: click start, relative mouse/pan tilt, Escape/unlock/restart, six generator sprites, music change, five audio decodes, game over/high score reload');
}finally{await browser.close();}
