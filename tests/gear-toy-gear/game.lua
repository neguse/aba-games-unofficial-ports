local saved
lub={host={available=function()return false end},gfx={},audio={},io={save_text=function(path,text)assert(path=='save/gear-toy-gear/scores.txt');saved=text end}}
dofile(arg[1])
-- 保存は記録を読み終えてから (FrameHost.Begin が読む)
FrameHost.loaded=true
local function near(a,b,tolerance,label)
 assert(math.abs(a-b)<=tolerance,(label or '')..': '..tostring(a)..' != '..tostring(b))
end
local function tick(f,input,n)
 Pad.input=input
 for i=1,n or 1 do f:Update() end
end
local function snapshot(f,i)
 local a=f.actors;local p=a.player
 return {i,p.Pos.X,p.Pos.Y,Stage.GameSpeed,Stage.Rank,a.gameState.score,a.gameState.left,a.stage.stageCount,a.enemies:get_Count(),a.middleEnemies:get_Count(),a.bullets:get_Count()}
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
local f=GtgFrame.new();f:LoadContent();f:Seed(1);f:StartGame()
for i=1,3000 do
 tick(f,(i%240<120 and 1 or 8)|(i%600<400 and 512 or 256))
 if i%100==0 then
  local actual=snapshot(f,i)
  for j,expected in ipairs(reference[i//100])do near(actual[j],expected,j>=2 and j<=5 and .00001 or 0,'game '..i..' field '..j)end
 end
end
print('PASS original C# game: 3000 updates')
f:Seed(1);f:StartGame();local recording={}
for i=1,600 do tick(f,(i%240<120 and 1 or 8)|(i%600<400 and 512 or 256));recording[i]=snapshot(f,i)end
assert(#f.replay.data>0 and #f.replay.data<=600)
f:StartTitle();assert(f.replay:get_IsAvailable())
for i=1,600 do
 tick(f,0);local actual=snapshot(f,i)
 for j,expected in ipairs(recording[i])do near(actual[j],expected,.00001,'replay '..i..' field '..j)end
end
print('PASS 600-update replay including acceleration and braking')
f:StartGame();local a=f.actors;local p=a.player;local g=a.gameState;local stage=a.stage
local destroy=p.Destroy;p.Destroy=function()end
local bosses=0;local oldBoss=false
for i=1,6500 do
 tick(f,(i%240<120 and 1 or 8)|512)
 if stage.isBossStage and stage.stageTicks==420 then assert(a.middleEnemies:get_Count()>=3);bosses=bosses+1 end
 if oldBoss and not stage.isBossStage then assert(stage.stageTicks==120)end
 oldBoss=stage.isBossStage
 if stage.stageCount>=15 then break end
end
assert(stage.stageCount>=15 and bosses==2)
for kind=0,8 do
 a.pillars:Clear();stage.pillarType=kind;stage:AddPillars();assert(a.pillars.pool:get_Count()>0,'pillar '..kind)
end
p.Destroy=destroy
f:StartGame();tick(f,0,2);g=a.gameState
assert(g.left==2 and g.extendScore==1000000)
local score=g.score;g:set_Multiplier(10);g:AddScoreint(100);assert(g.score==score+1000)
g:set_Multiplier(1);g.score=999999;g:AddScoreint(1);assert(g.left==3 and g.extendScore==3000000)
g:AddScoreint(2000000);assert(g.left==4 and g.extendScore==6000000)
g.left=9;g:AddScoreint(3000000);assert(g.left==9 and g.extendScore==10000000)
local ticks=stage.ticks
tick(f,16384);assert(f.storedPauseTicks>=0)
tick(f,8,10);assert(stage.ticks==ticks)
tick(f,16384);assert(f.storedPauseTicks<0)
tick(f,0);g.score=7654321;g.left=0;p.invincibleTicks=-1;p:Destroy()
assert(g.storedIsInGameOver and g.left==-1)
tick(f,0,300);assert(f.state==0 and a.player.isInReplay and f.record.storedScores[1]==7654321)
assert(saved:sub(1,8)=='7654321,')
local record=Record.new();record:Load()
GtgPreference.Load(record,'7654321,90000,80000,70000,60000,50000,40000,30000,20000,10000')
assert(record.storedScores[1]==7654321 and record.storedScores[10]==10000)
GtgPreference.Load(record,'1,2,3,4,5,6,7,8,9,10');assert(record.storedScores[1]==7654321)
print('PASS two boss stages, nine pillar types, multipliers, extends, pause, game over and scores')
local pool=ActorPool.new(3,function()return MiddleEnemy.new()end,function()return true end,function()end,function()end)
local enemy=MiddleEnemy.new();enemy.Pos.X=10;enemy.TurretIndexes[1]=7
pool:AddT(enemy);enemy.Pos.X=20;enemy.TurretIndexes[1]=8;pool:AddT(enemy)
local copy=pool:Get(0);copy.Pos.X=99;copy.TurretIndexes[1]=99
assert(pool:Get(0).Pos.X==10 and pool:Get(0).TurretIndexes[1]==7)
pool:Remove(0);assert(pool:get_Count()==1 and pool:Get(0).Pos.X==20 and pool:Get(0).TurretIndexes[1]==8)
Letter.bars:Clear();Letter.AddstringVector3floatQuaternionfloat('GEAR TOY GEAR',Vector3.new(),1,Quaternion.get_Identity(),1)
assert(Letter.bars.pool:get_Count()>20)
local sound=Sound.new();local cue=sound:GetCue('HomingLaser')
local listener=AudioListener.new();listener.Position=Vector3.new(1,2,3)
local emitter=AudioEmitter.new();emitter.Position=Vector3.new(4,6,8)
cue:Apply3D(listener,emitter);cue:Play();local loop=FrameHost.loops[cue.id]
assert(loop.Sound==2);near(loop.Pan,3/math.sqrt(34),.00001,'loop pan')
emitter.Position.X=9;cue:Apply3D(listener,emitter);near(loop.Pan,8/math.sqrt(89),.00001,'moved loop pan')
cue:Stop(0);assert(FrameHost.loops[cue.id]==nil)
print('PASS actor value copies, fixed buffers, compaction, alphabet glyphs and spatial cue lifecycle')
print('RESULT 0')
