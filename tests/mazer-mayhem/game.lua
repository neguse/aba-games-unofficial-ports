local saved
lub={host={available=function()return false end},gfx={},audio={},io={save_text=function(path,text)saved=text end}}
dofile(arg[1])
local function near(a,b,tolerance,label)
 assert(math.abs(a-b)<=tolerance, (label or '')..': '..tostring(a)..' != '..tostring(b))
end
local function tick(f,input,n)
 Pad.input=input
 for i=1,n or 1 do f:update();f.field:set_eye_position() end
end
local function snapshot(f)
 local p=f.player
 return {f.cnt,p.stored_pos.x,p.stored_pos.y,p.stored_pos.z,p.deg,f.balls:length(),f.bullets:length(),p.score,f.shots:length(),p.shot_cnt,p.left,p.bonus_cnt}
end
-- Original C# 0.14, seed 1, with SetEyePosition after each update.
local reference={
{100,143.26845,192.12083,3.5020006,-2.2249994,10,19,0,31,51,2,0},
{200,111.084946,218.47737,1,-2.2249994,10,22,0,19,101,2,0},
{300,103.7827,215.3911,1,-2.2249994,15,31,0,4,151,2,0},
{400,103.7827,204.92595,1,-3.2500033,15,35,0,12,201,2,0},
{500,113.71113,172.48282,1,-4.475008,18,37,206,38,251,2,1},
{600,161.70529,160.356,1,-4.475008,16,37,415,40,301,2,1},
{700,149.94687,111.758286,1,-4.475008,13,33,721,34,351,2,1},
{800,171.6495,128.05103,1,-6.5000157,17,43,827,36,401,2,1},
{900,191.30128,158.88249,1,-6.7250166,15,47,1036,36,451,2,1},
{1000,209.20271,190.02736,1,-6.7250166,15,58,1448,41,501,2,1},
{1100,200.1387,232.55354,1,-7.2500186,21,79,1448,22,551,2,1},
{1200,178.2797,255.88625,1,-8.975006,18,86,1754,38,601,2,1},
{1300,156.54202,210.85909,1,-8.975006,22,123,1768,38,651,2,1},
{1400,115.939865,219.91144,1,-8.975006,20,116,1995,27,701,2,1},
{1500,114.21045,202.2074,1,-10.499983,20,119,1995,39,751,2,1},
{1600,146.08298,190.27826,24,-11.224972,23,52,2204,20,0,1,0},
{1700,160.05992,176.59785,7.9299965,-11.224972,23,63,2205,48,24,1,0},
{1800,208.38963,187.36958,1,-11.249971,25,102,2308,35,78,1,0},
{1900,246.98978,184.88219,1,-13.474937,24,111,2413,36,128,1,1},
{2000,220.8913,217.27638,1,-13.474937,30,158,2439,33,178,1,1},
{2100,224.26639,253.31969,1,-13.474937,29,160,2952,15,228,1,1},
{2200,216.56831,255.10211,1,-14.499922,32,193,3055,20,278,1,1},
{2300,204.0845,225.4059,24,-15.724903,28,86,3502,1,0,0,0},
{2400,169.09831,224.31317,7.6239934,-15.724903,30,94,3706,61,36,0,1},
{2500,169.94518,174.3208,1,-15.724903,28,104,3912,39,86,0,1},
{2600,152.9877,135.53014,1,-17.749872,32,150,4016,32,136,0,1},
{2700,188.53108,139.86154,1,-17.974869,32,191,4230,38,186,0,1},
{2800,224.31276,143.04764,1,-17.974869,36,235,4239,22,236,0,1},
{2900,252.95137,122.47241,1.0172501,-18.49986,37,243,4249,4,286,0,1},
{3000,228.8884,150.50484,24,-20.224834,36,90,4380,12,0,-1,0},
}
local f=MmFrame.new();f:load_content();f:seed(1);f:start_in_game()
for i=1,3000 do
 tick(f,16|(i%240<120 and 1 or 8)|(i%360<90 and 32 or 0))
 if i%100==0 then
  local actual=snapshot(f)
  for j,expected in ipairs(reference[i//100])do near(actual[j],expected,j>=2 and j<=5 and .0001 or 0,'game '..i..' field '..j)end
 end
end
print('PASS original C# game: 3000 updates')
-- Original CircleParticle and SpringConstraint: collision, forces, rotation and spring correction.
local physics={
{59,-9.70379,-0.24060991,3.3077238,-11.504731,0.64389104,2.6885693,-0.11075115,0.05916795,-0.03711915,-0.2252779,-0.03068763,0.02578044,-0.56138927,-0.09331862,0.068037175,0.81945693},
{119,-1.7595336,1.1566838,2.6725006,-3.3111994,0.104988456,3.408686,0.5143833,-0.0008018017,0.007396698,0.505316,0.0053265765,0.0031068325,-0.85738677,-0.14135994,0.10280143,0.4840947},
{179,43.892094,0.3552808,3.6900806,45.82479,0.8540938,3.3409224,0.9187355,-0.001404196,0.009205818,0.9230652,0.0035582185,0.0057325363,-0.94796234,-0.15586708,0.113254376,0.25349975},
{239,85.08359,1.7163062,3.2562048,84.91555,0.06428392,4.4126053,0.2645111,0.0034601688,0.006560564,0.2575531,-0.0008723363,0.00959301,-0.97297984,-0.15976794,0.1160403,0.11974842},
{299,68.9749,1.7490801,3.7861,69.262764,0.10674848,4.935716,-0.7035141,0.002919197,0.0073537827,-0.69277954,-0.0011610761,0.01020956,-0.97905034,-0.16065128,0.11665595,0.045464925},
{359,20.170341,1.0067157,4.877218,22.169682,0.63685524,5.136127,-0.69311714,-0.0024558306,0.011344433,-0.6790104,0.002066791,0.008177757,-0.9800714,-0.16075704,0.116718546,0.004686686},
{419,5.2509966,1.4626456,5.139726,6.7618084,0.3521582,5.9170647,0.21545458,0.005661249,0.0057868958,0.21440935,-0.0035392344,0.012226582,-0.9799417,-0.16070202,0.116670825,-0.017633151},
{479,40.813927,0.49773285,6.4024367,38.839962,1.0059594,6.046669,0.73337555,-0.020887345,0.024438381,0.7303581,0.014053345,-1.9550323E-05,-0.9796634,-0.16063797,0.116620034,-0.029842649},
{539,74.50911,0.74538076,6.8194156,72.417046,0.8466188,6.7485447,0.28385925,-0.00014859438,0.009958744,0.2609253,0.00016927719,0.009736061,-0.9794499,-0.16059269,0.11658485,-0.036521383},
{599,68.44751,1.7341483,6.71927,67.7007,0.1905825,7.799744,-0.4167099,0.022436857,-0.0058293343,-0.3319931,-0.014919609,0.020318508,-0.97931314,-0.16056485,0.116563395,-0.040175036},
{659,42.30795,1.3933883,7.550689,44.075584,0.41946584,8.232438,-0.3455391,0.013512731,0.00042963028,-0.32712555,-0.008987755,0.016179085,-0.97923255,-0.1605487,0.116551,-0.042173926},
{719,34.445374,1.2052419,8.275909,36.367935,0.5458203,8.737462,0.04021454,-0.00801599,0.015507698,0.06833649,0.00535506,0.0061454773,-0.97918665,-0.16053963,0.11654401,-0.043267567},
{779,43.917484,0.8494606,9.118772,41.71587,0.7835085,9.164908,0.13272476,-0.10999167,0.08689499,0.066833496,0.0733338,-0.041438103,-0.97916114,-0.16053456,0.116540134,-0.043865923},
{839,42.67248,0.605633,9.883384,40.57377,0.9463336,9.644907,-0.096637726,-0.009631693,0.01663971,-0.12559128,0.006424427,0.005405426,-0.9791471,-0.16053173,0.11653798,-0.04419331},
{899,37.71628,1.0793399,10.145787,35.7729,0.6306735,10.459915,0.0270195,0.016034126,-0.0013256073,0.032794952,-0.01068759,0.017382622,-0.97913903,-0.1605305,0.116536744,-0.04437244},
{959,48.93958,0.6503458,11.040024,51.081474,0.916759,10.853513,0.34489822,0.07138455,-0.04007435,0.42485428,-0.047588527,0.04321289,-0.9791351,-0.1605296,0.11653628,-0.044470463},
{1019,71.51786,1.1505529,11.283874,73.505455,0.5833345,11.680922,0.2326355,0.04054284,-0.018481255,0.23085785,-0.027028084,0.028820992,-0.9791318,-0.16052942,0.116535895,-0.04452408},
{1079,66.148926,1.8196335,11.409567,66.32341,0.13730167,12.58721,-0.3651886,0.01919794,-0.003537178,-0.5130844,-0.012798265,0.01886177,-0.9791318,-0.16052942,0.116535895,-0.04455342},
{1139,28.402369,1.5264907,12.20888,26.923124,0.33274764,13.044493,-0.68639946,0.023112297,-0.0062761307,-0.66332436,-0.0154079795,0.020687103,-0.9791318,-0.16052942,0.116535895,-0.04456948},
{1199,7.172419,1.354056,12.923655,5.456725,0.44771773,13.558081,0.045547485,0.037426353,-0.016296387,0.08024502,-0.024950534,0.027365685,-0.9791318,-0.16052942,0.116535895,-0.044578277},
}
local a,b=CircleParticle.new(),CircleParticle.new();a:clear();b:clear()
a:set(Vector3.new(-1,0,3));b:set(Vector3.new(1,0,3));a:set_mass(2);b:set_mass(3)
for i=0,1199 do
 a:add_massless_force(Vector3.new(math.sin(i*.017)*2,.3,-.2));b:add_massless_force(Vector3.new(math.cos(i*.023)*-2,-.2,.15))
 a:check_collision(b);a:update();b:update()
 if i%3==0 then SpringConstraint.resolve(a.pos,b.pos,a:get_inv_mass(),b:get_inv_mass(),.5,2)end
 if i%60==59 then
  local av,bv=a:get_velocity(),b:get_velocity()
  local actual={i,a.pos.x,a.pos.y,a.pos.z,b.pos.x,b.pos.y,b.pos.z,av.x,av.y,av.z,bv.x,bv.y,bv.z,a.rotation.x,a.rotation.y,a.rotation.z,a.rotation.w}
  for j,expected in ipairs(physics[(i+1)//60])do near(actual[j],expected,.003,'physics '..i..' field '..j)end
 end
end
local random=Random.new(1)
for _,expected in ipairs({534011718,237820880,1002897798,1657007234,1412011072,929393559,760389092,2026928803,217468053,1379662799})do assert(random:next()==expected)end
for _,sample in ipairs({0,1073741823,2147483646})do
 random.next=function()return sample end
 for _,max in ipairs({2,3,6,10,180,360})do assert(random:nextint(max)==(sample==0 and 0 or sample==2147483646 and max-1 or (max-1)//2))end
end
local pool=ParticlePool.new(3,nil);local particle=Particle.new();particle.pos.x=10
pool:add(particle);particle.pos.x=20;pool:add(particle);particle.pos.x=30;pool:add(particle)
assert(pool:get(0).pos.x==10 and pool:get(1).pos.x==20)
local copy=pool:get(0);copy.pos.x=99;assert(pool:get(0).pos.x==10)
pool:removeint(0);pool:gc();assert(pool:length()==2 and pool:get(0):get_id()==0 and pool:get(0).pos.x==30)
particle.pos.x=40;pool:add(particle);assert(pool:get(0).pos.x==30 and pool:get(2).pos.x==40)
print('PASS physics, RNG boundaries, actor value copies and compaction')
f:seed(1);f:start_in_game();local recording={}
for i=1,600 do tick(f,16|(i%240<120 and 1 or 8)|(i%360<90 and 32 or 0));recording[i]=snapshot(f)end
assert(#f.replay.data==600)
f:start_title()
for i=1,600 do
 tick(f,0)
 local actual=snapshot(f)
 for j,expected in ipairs(recording[i])do near(actual[j],expected,.0001,'replay '..i..' field '..j)end
end
f:start_in_game();tick(f,0,2)
local p=f.player
for i=1,110 do p:get_bonus(p.stored_pos)end
assert(p.multiplier==100)
tick(f,96,3);assert(p.stored_is_in_hyper)
for i=1,1000 do p:get_bonus(p.stored_pos)end
assert(p.multiplier==999)
local score=p.score;p:add_scoreint(100);assert(p.score==score+99900)
p:end_hyper_mode();assert(not p.stored_is_in_hyper and p.multiplier==0)
p.score=999999;p:add_scoreint(1);assert(p.left==3 and p.next_extend_score==3000000)
p:add_scoreint(2000000);assert(p.left==3 and p.next_extend_score==6000000)
local x,y=p.stored_pos.x,p.stored_pos.y
tick(f,128);assert(f.stored_pause_cnt>=0)
tick(f,8,10);assert(p.stored_pos.x==x and p.stored_pos.y==y)
tick(f,128);assert(f.stored_pause_cnt<0)
tick(f,0);tick(f,17);assert(p.dash_cnt>0 and f.grenades:length()>0)
f.stage.appearance_wait_cnt=0;f.stage.appearance_cnt_dec=31;tick(f,0)
local boss
for i=1,f.balls:length()do if f.balls.actors[i].base_radius>5 then boss=i-1;break end end
assert(boss,'boss appearance')
f.balls.actors[boss+1].hardness=1e9
f.balls.actors[boss+1].burst_radius=f.balls.actors[boss+1].state.radius
f.balls:add_damageintfloat(boss,1)
assert(f.stage.appearance_wait_cnt==180 and f.stage.appearance_cnt_dec==10)
p.score=7654321;p.left=0;p:destroy();assert(p.gameover_cnt==1 and p.left==-1)
tick(f,0,601)
assert(f.state==0 and f.record.stored_scores[1]==7654321 and p.is_in_replay)
assert(saved:sub(1,8)=='7654321,')
local scores='7654321,1000000,900000,800000,700000,600000,500000,400000,300000,200000'
local record=Record.new();record:load();MmPreference.load(record,scores)
assert(record.stored_scores[1]==7654321 and record.stored_scores[10]==200000)
MmPreference.load(record,'1,2,3,4,5,6,7,8,9,10');assert(record.stored_scores[1]==7654321)
local sound=Sound.new();sound:play_bgm('Mm1');assert(FrameHost.music==11 and FrameHost.music_volume==1)
sound:fadeout_bgm()
for i=1,60 do sound:update()end
near(FrameHost.music_volume,.5,.0001,'fade midpoint');assert(FrameHost.music==11)
for i=1,60 do sound:update()end
assert(FrameHost.music==-1)
print('PASS replay, hyper, extends, pause, dash, boss progression, game over, scores and 120-update music fade')
print('RESULT 0')
