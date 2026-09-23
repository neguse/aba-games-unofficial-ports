import assert from 'node:assert/strict';
import { mkdir } from 'node:fs/promises';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
const { chromium } = await import(pathToFileURL(resolve(process.env.PLAYWRIGHT_MODULE || '.cache/browser/node_modules/playwright/index.mjs')));
const url=process.argv[2]||'http://127.0.0.1:8765/masashikun-hi/';
const browser=await chromium.launch({executablePath:process.env.CHROMIUM_PATH,headless:true,args:['--no-sandbox','--enable-unsafe-webgpu','--use-angle=swiftshader','--enable-features=Vulkan,WebGPU','--use-vulkan=swiftshader','--disable-vulkan-fallback-to-gl-for-testing']});
const errors=[];
try{
 await mkdir('build/screenshots',{recursive:true});
 const page=await browser.newPage({viewport:{width:1000,height:1000}});
 page.on('pageerror',e=>errors.push(e.message));page.on('console',m=>{if(m.type()==='error')errors.push(m.text());});
 await page.route('**/game.lua',async route=>{
  const response=await route.fetch();let code=await response.text();assert.match(code,/return Game\s*$/);
  code=code.replace('if topic == "seed" then',`if topic == "test.score" then MasKak.setkakhiscore(12345) end
if topic == "seed" then`);
  code=code.replace(/return Game\s*$/,`local move=MasMain.moveall;local lastMotion,presses,holds=0,0,0
function MasMain.moveall()
 if MasForm.mousemv>0 then lastMotion=MasForm.mousemv end
 if MasForm.mousebt==1 then presses=presses+1 end
 if MasForm.mousebt==2 then holds=holds+1 end
 move()
end
local frame=Game.on_frame
function Game.on_frame(dt)
 frame(dt)
 lub.host.send('test.state',table.concat({MasMain.mlspe,MasKak.kakcou,MasKak.myx,MasKak.myp,MasKak.speed,lastMotion,presses,holds,MasScores.hscsf and 1 or 0,MasKak.kakhsc[2].rec},','))
end
return Game`);
  await route.fulfill({response,body:code});
 });
 async function observe(){
  await page.locator('#status').waitFor({state:'hidden',timeout:60000});
  await page.evaluate(()=>{const onMessage=lubHost.onMessage;lubHost.onMessage=(topic,bytes)=>{if(topic==='test.state')window.gameState=new TextDecoder().decode(bytes).split(',').map(Number);else onMessage(topic,bytes);};});
  await page.waitForFunction(()=>window.gameState);
 }
 async function screenshot(name){
  const png=await page.locator('#canvas').screenshot({path:`build/screenshots/mas-${name}.png`});
  const bright=await page.evaluate(async data=>{
   const image=new Image();image.src='data:image/png;base64,'+data;await image.decode();const c=document.createElement('canvas');c.width=image.width;c.height=image.height;const ctx=c.getContext('2d');ctx.drawImage(image,0,0);const pixels=ctx.getImageData(0,0,c.width,c.height).data;let n=0;for(let i=0;i<pixels.length;i+=4)if(pixels[i]>120&&pixels[i+1]>120)n++;return n;
  },png.toString('base64'));assert.ok(bright>500,`${name} line drawing: ${bright}`);
 }
 await page.goto(url);await observe();await page.waitForTimeout(3000);await screenshot('title');
 await page.locator('[data-event="0"]').click();await page.waitForFunction(()=>gameState[0]===2);
 await page.waitForFunction(()=>document.pointerLockElement===document.querySelector('#canvas'));
 await page.mouse.click(300,300);await page.waitForFunction(()=>gameState[0]===3);await page.mouse.click(300,300);await page.waitForFunction(()=>gameState[0]===1);
 const previous=await page.evaluate(()=>gameState[5]);await page.mouse.move(420,360);await page.waitForFunction(old=>gameState[5]>0&&gameState[5]!==old,previous);
 await page.keyboard.press('ShiftLeft');await page.waitForFunction(()=>gameState[5]===768);
 await page.waitForFunction(()=>gameState[1]>=48);
 const presses=await page.evaluate(()=>gameState[6]),holds=await page.evaluate(()=>gameState[7]);
 await page.keyboard.down('Space');await page.waitForFunction(n=>gameState[6]>n,presses);await page.waitForFunction(n=>gameState[7]>n,holds);await page.keyboard.up('Space');
 await screenshot('sprint');await page.keyboard.press('F3');await page.waitForFunction(()=>gameState[0]===-1);
 const count=await page.evaluate(()=>gameState[1]);await page.waitForTimeout(200);assert.equal(await page.evaluate(()=>gameState[1]),count);await screenshot('paused');
 await page.keyboard.press('F3');await page.waitForFunction(()=>gameState[0]===1);await page.waitForFunction(()=>document.pointerLockElement===document.querySelector('#canvas'));
 for(const [key,state,name]of[['F6',7,'vault'],['F7',10,'tug'],['F8',4,'throw'],['F9',13,'fire']]){
  await page.keyboard.press(key);await page.waitForFunction(s=>gameState[0]===s,state);
  for(let i=0;i<8;i++){await page.keyboard.press('ShiftLeft');await page.waitForTimeout(50);}
  await page.waitForTimeout(800);await screenshot(name);
 }
 await page.evaluate(()=>document.exitPointerLock());await page.waitForFunction(()=>gameState[0]===-1);
 await page.keyboard.press('F5');await page.waitForFunction(()=>gameState[0]===1);
 await page.evaluate(()=>lubHost.queue.push({topic:'test.score',payload:''}));await page.locator('#ranking').waitFor({state:'visible'});
 assert.equal(await page.locator('#name-row').isVisible(),true);await page.locator('#player-name').fill('まさし <one>');await page.locator('#close-rank').click();
 await page.waitForFunction(()=>localStorage.getItem('masashikun-hi-scores-v1')?.includes('12345\tまさし <one>'));
 await page.reload();await observe();assert.equal(await page.evaluate(()=>gameState[9]),12345);
 await page.locator('#rank').click();await page.locator('#ranking').waitFor({state:'visible'});assert.match(await page.locator('#records').textContent(),/まさし <one>/);
 for(let n=0;n<6;n++){await page.locator('#category').selectOption({index:n});await page.waitForTimeout(80);assert.equal(await page.locator('#records tr').count(),3);}
 await page.locator('#close-rank').click();
 const source=await page.request.get(new URL('source.tar.gz',url).href);assert.ok(source.ok());assert.ok((await source.body()).length>10000);
 const license=await page.request.get(new URL('LICENSE.txt',url).href);assert.match(await license.text(),/GNU GENERAL PUBLIC LICENSE/);
 await page.locator('#rank').click();page.once('dialog',d=>d.accept());await page.locator('#clear-scores').click();await page.locator('#close-rank').click();await page.reload();await observe();assert.equal(await page.evaluate(()=>gameState[9]),99999);
 assert.deepEqual(errors,[]);console.log('PASS: five events, instructions, pointer lock/motion, Shift/Space, pause, name entry, six rankings, reload, clear and source download');
}finally{await browser.close();}
