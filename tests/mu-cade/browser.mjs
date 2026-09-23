import assert from 'node:assert/strict';
import {mkdir,readFile} from 'node:fs/promises';
import {resolve} from 'node:path';
import {pathToFileURL} from 'node:url';
const {chromium}=await import(pathToFileURL(resolve(process.env.PLAYWRIGHT_MODULE||'.cache/browser/node_modules/playwright/index.mjs')));
const browser=await chromium.launch({executablePath:process.env.CHROMIUM_PATH,headless:true,args:['--no-sandbox','--enable-unsafe-webgpu','--use-angle=swiftshader','--enable-features=Vulkan,WebGPU','--use-vulkan=swiftshader','--disable-vulkan-fallback-to-gl-for-testing']});
const url=process.argv[2]||'http://127.0.0.1:8765/mu-cade/';
const errors=[];
try{
 await mkdir('build/screenshots',{recursive:true});
 const page=await browser.newPage({viewport:{width:960,height:900}});
 page.on('pageerror',e=>errors.push(e.message));page.on('console',m=>{if(m.type()==='error')errors.push(m.text());});
 await page.addInitScript(()=>{window.audioStarts=0;const Native=window.AudioContext;window.AudioContext=class extends Native{createBufferSource(){const s=super.createBufferSource(),start=s.start.bind(s);s.start=(...args)=>{audioStarts++;return start(...args);};return s;}};});
 const tests=await readFile('tests/mu-cade/game.lua','utf8');
 const physics=tests.slice(tests.indexOf('local o=mcdphysics'),tests.indexOf("print('PASS:"));
 await page.route('**/game.lua',async route=>{
  const response=await route.fetch();let code=await response.text();
  code=code.replace('if topic == "scores" then',`if topic=='test.control' then Game.manager.enemies:clear();Game.manager.stage_manager.move_0=function()end end
if topic=='test.tail' then for i=1,8 do Game.manager.ship:add_tail_1(1)end end
if topic=='test.enemy' then local g=Game.manager;CentHeadChase.new(g.field,g.ship,g.bullets,g.world,30,2):set_jointed_enemies_5(g.enemies,0,10,0,0) end
if topic=='test.finish' then local g=Game.manager;g.score=7654321;g:start_game_over() end
if topic=='test.physics' then
 local g=Game.manager
 local function near(a,b,t,label)assert(math.abs(a-b)<=t,label)end
 ${physics}
 g:start_title();lub.host.send('test.physics.done','1')
end
if topic == "scores" then`);
  code=code.replace(/return Game\s*$/,`local frame=Game.on_frame
function Game.on_frame(dt)
 frame(dt);local g=Game.manager;local p=g.ship;local shots=0
 for _,s in ipairs(p.shots.actor)do if s.exists then shots=shots+1 end end
 lub.host.send('test.state',table.concat({g.state,p._pos.x,p._pos.y,p.tail_num,p.enhanced_shot_cnt,g.paused and 1 or 0,g.time,g.pref_manager.pref_data.high_score[1],TwinStickPad.input,shots,p.trg_deg,g._is_game_over and 1 or 0,g.score,p.restart_cnt},','))
end
return Game`);
  await route.fulfill({response,body:code});
 });
 async function observe(){await page.locator('#status').waitFor({state:'hidden',timeout:60000});await page.evaluate(()=>{const previous=lubHost.onMessage;lubHost.onMessage=(topic,bytes)=>{if(topic==='test.state')window.gameState=new TextDecoder().decode(bytes).split(',').map(Number);else if(topic==='test.physics.done')window.physicsDone=true;else previous(topic,bytes);};});await page.waitForFunction(()=>window.gameState);}
 async function send(topic){await page.evaluate(topic=>lubHost.queue.push({topic,payload:''}),topic);}
 async function release(key){await page.keyboard.up(key);await page.waitForFunction(()=>gameState[8]===0);}
 async function shot(name){await page.locator('#canvas').screenshot({path:`build/screenshots/mu-${name}.png`});}
 await page.goto(url);await observe();await shot('title');
 await page.keyboard.down('z');await page.waitForFunction(()=>gameState[0]===2&&gameState[13]<=0);await release('z');await send('test.control');
 const x=await page.evaluate(()=>gameState[1]);await page.keyboard.down('ArrowRight');await page.waitForFunction(x=>gameState[1]>x+.15,x);await release('ArrowRight');
 await page.keyboard.down('l');await page.waitForFunction(()=>Math.abs(gameState[10]+Math.PI/2)<.001&&gameState[9]>0);await release('l');
 await send('test.tail');await page.waitForFunction(()=>gameState[3]===8);await shot('tail');
 await page.keyboard.down('x');await page.waitForFunction(()=>gameState[3]===0&&gameState[4]>0);await release('x');
 await page.keyboard.down('p');await page.waitForFunction(()=>gameState[5]===1);await release('p');const time=await page.evaluate(()=>gameState[6]);await page.waitForTimeout(350);assert.equal(await page.evaluate(()=>gameState[6]),time);await shot('paused');
 await page.keyboard.down('p');await page.waitForFunction(()=>gameState[5]===0);await release('p');await send('test.enemy');await page.waitForTimeout(300);await shot('playing');
 await send('test.finish');await page.waitForFunction(()=>gameState[7]===7654321&&gameState[11]===1);await page.waitForFunction(()=>localStorage.getItem('mu-cade-scores-v1')?.startsWith('7654321,'));assert.ok(await page.evaluate(()=>audioStarts>0));
 await page.locator('#sound').click();assert.equal(await page.locator('#sound').getAttribute('aria-pressed'),'true');
 await page.reload();await observe();await page.waitForFunction(()=>gameState[7]===7654321);
 const count=await page.evaluate(async()=>{const c=JSON.parse(document.querySelector('#game-config').textContent),a=new AudioContext();for(const name of c.music){const r=await fetch(`audio/${name}.ogg`);assertResponse(r);const b=await a.decodeAudioData(await r.arrayBuffer());if(b.duration<=0)throw Error(name);}for(const name of c.sounds){const r=await fetch(`audio/${name}.wav`);assertResponse(r);const b=await a.decodeAudioData(await r.arrayBuffer());if(b.duration<=0)throw Error(name);}await a.close();return c.music.length+c.sounds.length;function assertResponse(r){if(!r.ok)throw Error(r.url);}});assert.equal(count,13);
 await send('test.physics');await page.waitForFunction(()=>window.physicsDone);assert.deepEqual(errors,[]);
 console.log('PASS: WebGPU, movement, IJKL aiming, tails/cutting, pause, enemy rendering, sound/mute, 13 audio decodes, ranking reload and original ODE contact/feedback trace');
}finally{await browser.close();}
