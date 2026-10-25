import assert from 'node:assert/strict';
import { mkdir } from 'node:fs/promises';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { waitSaved } from '../lub-page.mjs';
const { chromium } = await import(pathToFileURL(resolve(process.env.PLAYWRIGHT_MODULE || '.cache/browser/node_modules/playwright/index.mjs')));
const url=process.argv[2]||'http://127.0.0.1:8765/masashikun-hi/';
const browser=await chromium.launch({executablePath:process.env.CHROMIUM_PATH,headless:true,args:['--no-sandbox','--enable-unsafe-webgpu','--use-angle=swiftshader','--enable-features=Vulkan,WebGPU','--use-vulkan=swiftshader','--disable-vulkan-fallback-to-gl-for-testing']});
const errors=[];
try{
 await mkdir('build/screenshots',{recursive:true});
 const page=await browser.newPage({viewport:{width:1000,height:1000}});
 page.on('pageerror',e=>errors.push(e.message));page.on('console',m=>{if(m.type()==='error')errors.push(m.text());});
 // test commands: 0 score
 async function command(id,value=0){await page.evaluate(payload=>lubHost.queue.push({topic:'test',payload}),`${id},${value}`);}
 async function observe(){
  await page.locator('#status').waitFor({state:'hidden',timeout:60000});
  await page.evaluate(()=>{const onMessage=lubHost.onMessage;lubHost.onMessage=(topic,bytes)=>{if(topic==='test.state'){window.gameState=new TextDecoder().decode(bytes).split(',').map(Number);window.stateCount=(window.stateCount??0)+1;}else onMessage(topic,bytes);};});
  await page.waitForFunction(()=>window.gameState);
 }
 // The panel reads one key press per key each frame, so the next key waits until a frame after this one has run.
 async function tap(key){await page.keyboard.press(key);const n=await page.evaluate(()=>stateCount);await page.waitForFunction(n=>stateCount>n+1,n);}
 async function screenshot(name){
  const png=await page.locator('#canvas').screenshot({path:`build/screenshots/mas-${name}.png`});
  const bright=await page.evaluate(async data=>{
   const image=new Image();image.src='data:image/png;base64,'+data;await image.decode();const c=document.createElement('canvas');c.width=image.width;c.height=image.height;const ctx=c.getContext('2d');ctx.drawImage(image,0,0);const pixels=ctx.getImageData(0,0,c.width,c.height).data;let n=0;for(let i=0;i<pixels.length;i+=4)if(pixels[i]>120&&pixels[i+1]>120)n++;return n;
  },png.toString('base64'));assert.ok(bright>500,`${name} line drawing: ${bright}`);
 }
 await page.goto(url);await observe();await page.waitForTimeout(3000);await screenshot('title');
 // The menu (MasPanel) is drawn inside the game; F2 starts the five events from it.
 await tap('F2');await page.waitForFunction(()=>gameState[0]===2);
 // lub asks for pointer lock and the browser grants it on the next click.
 await page.mouse.click(300,300);await page.waitForFunction(()=>gameState[0]===3);await page.mouse.click(300,300);await page.waitForFunction(()=>gameState[0]===1);
 await page.waitForFunction(()=>document.pointerLockElement===document.querySelector('#canvas'));
 const previous=await page.evaluate(()=>gameState[5]);
 // Headless pointer lock reports a real move as a jump and its inverse, so the relative move is dispatched directly.
 await page.evaluate(()=>document.querySelector('#canvas').dispatchEvent(new PointerEvent('pointermove',{pointerType:'mouse',movementX:120,movementY:60,bubbles:true})));await page.waitForFunction(old=>gameState[5]>0&&gameState[5]!==old,previous);
 await tap('ShiftLeft');await page.waitForFunction(()=>gameState[5]===768);
 await page.waitForFunction(()=>gameState[1]>=48);
 const presses=await page.evaluate(()=>gameState[6]),holds=await page.evaluate(()=>gameState[7]);
 await page.keyboard.down('Space');await page.waitForFunction(n=>gameState[6]>n,presses);await page.waitForFunction(n=>gameState[7]>n,holds);await page.keyboard.up('Space');
 await screenshot('sprint');await tap('F3');await page.waitForFunction(()=>gameState[0]===-1);
 const count=await page.evaluate(()=>gameState[1]);await page.waitForTimeout(200);assert.equal(await page.evaluate(()=>gameState[1]),count);await screenshot('paused');
 await tap('F3');await page.waitForFunction(()=>gameState[0]===1);
 for(const [key,state,name]of[['F6',7,'vault'],['F7',10,'tug'],['F8',4,'throw'],['F9',13,'fire']]){
  await tap(key);await page.waitForFunction(s=>gameState[0]===s,state);
  for(let i=0;i<8;i++)await tap('ShiftLeft');
  await page.waitForTimeout(800);await screenshot(name);
 }
 await tap('Escape');await page.waitForFunction(()=>gameState[0]===-1);
 await tap('F5');await page.waitForFunction(()=>gameState[0]===1);
 // A new record opens the in-game name entry; Enter saves it.
 await command(0);await page.waitForFunction(()=>gameState[8]===1);await screenshot('name-entry');
 await page.keyboard.type('masa one');await tap('Enter');await page.waitForFunction(()=>gameState[8]===0);
 await waitSaved(page,'masashikun-hi/scores.txt','12345\tmasa one');
 await page.reload();await observe();assert.equal(await page.evaluate(()=>gameState[9]),12345);
 await tap('F10');await page.waitForTimeout(300);await screenshot('ranking');
 for(let n=0;n<6;n++)await tap('ArrowRight');
 await tap('Enter');
 const source=await page.request.get(new URL('source.tar.gz',url).href);assert.ok(source.ok());assert.ok((await source.body()).length>10000);
 const license=await page.request.get(new URL('LICENSE.txt',url).href);assert.match(await license.text(),/GNU GENERAL PUBLIC LICENSE/);
 // F10, Delete, Enter clears every record; Enter closes the ranking.
 await tap('F10');await tap('Delete');await tap('Enter');await tap('Enter');
 await waitSaved(page,'masashikun-hi/scores.txt','12345','excludes');
 await page.reload();await observe();assert.equal(await page.evaluate(()=>gameState[9]),99999);
 assert.deepEqual(errors,[]);console.log('PASS: five events, in-game menu, pointer lock/motion, Shift/Space, pause, name entry, six rankings, reload, clear and source download');
}finally{await browser.close();}
