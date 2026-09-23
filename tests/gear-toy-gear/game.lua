lub={host={available=function()return false end}}
dofile(arg[1])
local function near(a,b,tolerance,label)
 assert(math.abs(a-b)<=tolerance,(label or '')..': '..tostring(a)..' != '..tostring(b))
end
local function tick(f,input,n)
 Pad.input=input
 for i=1,n or 1 do f:update() end
end
local function snapshot(f,i)
 local a=f.actors;local p=a.player
 return {i,p.pos.x,p.pos.y,Stage.game_speed,Stage.rank,a.game_state.score,a.game_state.left,a.stage.stage_count,a.enemies:get_count(),a.middle_enemies:get_count(),a.bullets:get_count()}
end
-- Original C# 0.1, seed 1, alternating movement, acceleration and braking.
local reference={
{100,0,90,1.6585149,0.55,1005,2,1,10,0,0},
{200,89.13616,12.439598,2.1826792,0.55,34059,2,1,1,0,3},
{300,22.80962,87.06159,2.5009437,0.55,38439,2,1,11,0,0},
{400,12.260886,89.16092,1.029773,0.595,42116,1,2,10,0,1},
{500,60.402454,66.71989,1.0000565,0.595,43216,1,2,0,0,0},
{600,5.684682,89.82028,1.0004987,0.595,43316,1,2,0,0,0},
{700,89.77985,6.291274,1.6651163,0.6355,44342,1,3,0,0,0},
{800,58.028004,68.79499,1.1353887,0.6355,44955,0,3,0,0,0},
{900,87.86851,19.471127,1.8378619,0.6355,46604,0,3,0,0,0},
{1000,42.36875,79.40333,2.2779148,0.67195,69812,0,4,5,1,1},
{1100,56.26398,70.24503,1.0092169,0.67195,77807,0,4,2,0,1},
{1200,89.77636,6.340746,1.0005523,0.67195,78107,0,4,4,0,0},
{1300,17.12627,88.35548,1.0170465,0.704755,82166,-1,5,5,0,0},
{1400,17.12627,88.35548,1.0000005,0.704755,82166,-1,5,11,0,0},
{1500,17.12627,88.35548,1.0000005,0.704755,82166,-1,5,2,0,1},
{1600,0,66,1.1566789,0.55,79,2,1,6,0,0},
{1700,36.61907,82.2134,1.8550198,0.55,1795,2,1,10,0,0},
{1800,89.89363,4.3743625,2.3023174,0.55,35347,2,1,7,0,0},
{1900,8.101899,89.63458,2.5734198,0.595,40031,2,2,10,0,10},
{2000,41.726597,79.74265,1.0018531,0.595,42148,1,2,10,0,0},
{2100,23.81449,86.79211,1.000011,0.595,43248,1,2,0,0,0},
{2200,72.71958,53.02699,1.1640145,0.6355,43400,1,3,0,0,0},
{2300,89.89972,4.2475452,1.0902082,0.6355,44837,0,3,0,0,0},
{2400,22.624159,87.10997,1.3787861,0.6355,45228,0,3,0,0,0},
{2500,89.73595,6.8890357,2.0084202,0.67195,47563,0,4,3,1,0},
{2600,15.631888,88.63207,1.3013709,0.67195,74286,0,4,4,1,2},
{2700,85.07693,29.358427,1.0017856,0.67195,77939,0,4,2,0,0},
{2800,52.71753,72.944244,1.1640251,0.704755,78191,0,5,8,0,0},
{2900,17.12627,88.35548,1.0005853,0.704755,82166,-1,5,7,0,1},
{3000,17.12627,88.35548,1.0000005,0.704755,82166,-1,5,9,0,0},
}
local f=GtgFrame.new();f:load_content();f:seed(1);f:start_game()
for i=1,3000 do
 tick(f,(i%240<120 and 1 or 8)|(i%600<400 and 512 or 256))
 if i%100==0 then
  local actual=snapshot(f,i)
  for j,expected in ipairs(reference[i//100])do near(actual[j],expected,j>=2 and j<=5 and .00001 or 0,'game '..i..' field '..j)end
 end
end
print('PASS original C# game: 3000 updates')
f:seed(1);f:start_game();local recording={}
for i=1,600 do tick(f,(i%240<120 and 1 or 8)|(i%600<400 and 512 or 256));recording[i]=snapshot(f,i)end
assert(#f.replay.data>0 and #f.replay.data<=600)
f:start_title();assert(f.replay:get_is_available())
for i=1,600 do
 tick(f,0);local actual=snapshot(f,i)
 for j,expected in ipairs(recording[i])do near(actual[j],expected,.00001,'replay '..i..' field '..j)end
end
print('PASS 600-update replay including acceleration and braking')
f:start_game();local a=f.actors;local p=a.player;local g=a.game_state;local stage=a.stage
local destroy=p.destroy;p.destroy=function()end
local bosses=0;local oldBoss=false
for i=1,6500 do
 tick(f,(i%240<120 and 1 or 8)|512)
 if stage.is_boss_stage and stage.stage_ticks==420 then assert(a.middle_enemies:get_count()>=3);bosses=bosses+1 end
 if oldBoss and not stage.is_boss_stage then assert(stage.stage_ticks==120)end
 oldBoss=stage.is_boss_stage
 if stage.stage_count>=15 then break end
end
assert(stage.stage_count>=15 and bosses==2)
for kind=0,8 do
 a.pillars:clear();stage.pillar_type=kind;stage:add_pillars();assert(a.pillars:get_count()>0,'pillar '..kind)
end
p.destroy=destroy
f:start_game();tick(f,0,2);g=a.game_state
assert(g.left==2 and g.extend_score==1000000)
local score=g.score;g:set_multiplier(10);g:add_scoreint(100);assert(g.score==score+1000)
g:set_multiplier(1);g.score=999999;g:add_scoreint(1);assert(g.left==3 and g.extend_score==3000000)
g:add_scoreint(2000000);assert(g.left==4 and g.extend_score==6000000)
g.left=9;g:add_scoreint(3000000);assert(g.left==9 and g.extend_score==10000000)
local ticks=stage.ticks
tick(f,16384);assert(f.stored_pause_ticks>=0)
tick(f,8,10);assert(stage.ticks==ticks)
tick(f,16384);assert(f.stored_pause_ticks<0)
tick(f,0);g.score=7654321;g.left=0;p.invincible_ticks=-1;p:destroy()
assert(g.stored_is_in_game_over and g.left==-1)
tick(f,0,300);assert(f.state==0 and a.player.is_in_replay and f.record.stored_scores[1]==7654321)
local record=Record.new();record:load()
GtgPreference.load(record,'7654321,90000,80000,70000,60000,50000,40000,30000,20000,10000')
assert(record.stored_scores[1]==7654321 and record.stored_scores[10]==10000)
GtgPreference.load(record,'1,2,3,4,5,6,7,8,9,10');assert(record.stored_scores[1]==7654321)
print('PASS two boss stages, nine pillar types, multipliers, extends, pause, game over and scores')
local pool=ActorPool.new(3,function()return MiddleEnemy.new()end)
local enemy=MiddleEnemy.new();enemy.pos.x=10;enemy.turret_indexes[1]=7
pool:add_t(enemy);enemy.pos.x=20;enemy.turret_indexes[1]=8;pool:add_t(enemy)
local copy=pool:get(0);copy.pos.x=99;copy.turret_indexes[1]=99
assert(pool:get(0).pos.x==10 and pool:get(0).turret_indexes[1]==7)
pool:remove(0);assert(pool:get_count()==1 and pool:get(0).pos.x==20 and pool:get(0).turret_indexes[1]==8)
Letter.bars:clear();Letter.addstring_vector3float_quaternionfloat('GEAR TOY GEAR',Vector3.new(),1,Quaternion.get_identity(),1)
assert(Letter.bars:get_count()>20)
local events={};lub.host.available=function()return true end;lub.host.send=function(topic,value)events[#events+1]={topic,value}end
local sound=Sound.new();local cue=sound:get_cue('HomingLaser')
local listener=AudioListener.new();listener.position=Vector3.new(1,2,3)
local emitter=AudioEmitter.new();emitter.position=Vector3.new(4,6,8)
cue:apply3_d(listener,emitter);cue:play();emitter.position.x=9;cue:apply3_d(listener,emitter);cue:stop(0)
local function fields(text)local a={};for v in text:gmatch('[^,]+')do a[#a+1]=tonumber(v)end;return a end
local play,update=fields(events[1][2]),fields(events[2][2])
assert(events[1][1]=='spatial.play' and play[2]==2 and play[3]==1 and play[4]==3 and play[5]==4 and play[6]==5)
assert(events[2][1]=='spatial.update' and update[2]==8 and update[3]==4 and update[4]==5)
assert(events[3][1]=='spatial.stop')
print('PASS actor value copies, fixed buffers, compaction, alphabet glyphs and spatial cue lifecycle')
print('RESULT 0')
