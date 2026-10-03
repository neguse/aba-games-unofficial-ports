lub={config=function()end,host={available=function()return false end}}
local draws=0
lub.png={load=function()return nil,0,0,0,0,0,"pending" end}
lub.gfx={ADDITIVE=1,ALPHA=2,MULTIPLY=3,NONE=0,use_buffer=function(key,kind,data)assert(#data>0,key);return {version=1}end,
 use_texture=function()return {version=1}end,draw=function(n)draws=draws+n end}
local game=dofile(arg[1]);game.on_init()
local g=Game.manager
local stageMove=g.stage_manager.move_0
local function near(a,b,t,label)assert(math.abs(a-b)<=t,(label or '')..': '..tostring(a)..' != '..tostring(b))end
local function tick(input,n)
 TwinStickPad.input=input
 for i=1,n or 1 do g:move_0()end
end
local function count(pool)local n=0;for _,a in ipairs(pool.actor)do if a.exists then n=n+1 end end;return n end
local function fresh()
 g.rand:set_seed(1);g:start_in_game();g.enemies:clear();g.stage_manager.move_0=function()end;tick(0,73)
end
assert(#g.ship.tails==63 and #g.stage_manager.appearances==32)
assert(g.state==0);tick(0,3);tick(256);assert(g.state==2)
fresh();assert(g.ship.restart_cnt<=0 and g.ship.exists)
local x=g.ship._pos.x;tick(8,20);assert(g.ship._pos.x>x+.1)
local angle=g.ship.deg;tick(256|1,10);near(g.ship.trg_deg,angle,.1,'held aim');assert(count(g.ship.shots)>0)
tick(128,10);near(g.ship.trg_deg,-math.pi/2,.0001,'second stick')
local time=g.time;tick(1024);tick(0,5);assert(g.paused and g.time==time);tick(1024);tick(0);assert(not g.paused and g.time>time)
fresh();for i=1,8 do g.ship:add_tail_1(1)end;assert(g.ship.tail_num==8 and g.ship:get_multiplier()>1)
tick(0,120);assert(g.ship.tail_num==8)
local mult=g.ship:get_multiplier();tick(512);assert(g.ship.tail_num==0 and g.ship.enhanced_shot_cnt==mult*5-1 and g.ship.bullet_disap_cnt>0)
tick(256,3);assert(count(g.ship.enhanced_shots)>0)
fresh();g.score=49999;g:add_score_1(1);assert(g.left==1 and g.next_extend_score==200000)
g.score=199999;g:add_score_1(1);assert(g.left==2 and g.next_extend_score==400000)
g.score=399999;g:add_score_1(1);assert(g.left==2 and g.next_extend_score==600000)
g.left=0;g.ship:destroyed_0();assert(g._is_game_over and g.left==-1)
g.pref_manager.pref_data:record_result(7654321,123456);assert(g.pref_manager.pref_data.high_score[1]==7654321 and g.pref_manager.pref_data.time[1]==123456)
local old=g.pref_manager.pref_data.high_score[1];g.pref_manager:load_text('bad');assert(g.pref_manager.pref_data.high_score[1]==old)
for _,kind in ipairs({CentHeadToAndFrom,CentHeadChase,CentHeadRoll})do
 for size=0,2 do
  fresh();local spec=kind.new(g.field,g.ship,g.bullets,g.world,30,size)
  local head=spec:set_jointed_enemies_5(g.enemies,0,10,0,0);assert(head and count(g.enemies)==spec.body_length)
  local most=0
  for i=1,240 do tick(0);most=math.max(most,count(g.bullets.simple_bullets))end
  assert(most>0,'enemy barrage');draws=0;g:draw();assert(draws>0)
 end
end
fresh();local before=g.score;local block=g.enemies:get_instance();assert(block:set_10(g.stage_manager._block_spec,0,0,0,0,1,1,1,1))
mcdphysics.body_position(block._body_id,30,0,-11);tick(0,2);assert(not block.exists and g.score>before)
draws=0;Letter.draw_string(Transform.ortho(),nil,lub.gfx.ADDITIVE,'test','ABC 123',10,20,3);assert(draws>100)
local lp=LinePoint.new(g.field);assert(#lp.pos==8 and #lp.pos_hist==40 and #lp.pos_hist[1]==8)
fresh();local spec=CentHeadChase.new(g.field,g.ship,g.bullets,g.world,30,1)
local head=spec:set_jointed_enemies_5(g.enemies,0,10,0,0);assert(head)
mcdphysics.body_position(head._body_id,30,0,-11);tick(0,2);assert(not head.exists and count(g.tail_particles)>0)
tick(0,180);assert(g.ship.tail_num>0 and g.ship:get_multiplier()==g.ship.tail_num+1)
fresh();g.stage_manager.move_0=stageMove;g.ship.destroyed_0=function()end
local maxEnemies,maxBullets=0,0
for i=1,4000 do
 tick(256|((i%240<120)and 4 or 8));maxEnemies=math.max(maxEnemies,count(g.enemies));maxBullets=math.max(maxBullets,count(g.bullets.simple_bullets))
 if i%120==0 then mcdphysics.body_position(g.ship._body_id,0,0,0);g.ship:reset()end
end
assert(maxEnemies>10 and maxBullets>0 and g.stage_manager.rank>0)
local o=mcdphysics
g:clear_all()
o.seed(1);local w=o.world_create();o.world_configure(w);local group=o.group_create();local floor=o.box(nil,40,40,1);o.geom_position(floor,0,0,-1)
local bodies,geoms={},{}
for i=1,2 do local b=o.body_create(w);bodies[i]=b;o.body_gravity(b,0);local m=o.mass();o.mass_box(m,1,1,1);o.mass_adjust(m,1);o.body_mass(b,m);o.body_position(b,(i-1)*4,0,2);geoms[i]=i==1 and o.box(nil,1,1,1)or o.sphere(nil,.5);o.geom_body(geoms[i],b)end
-- Original bundled ODE DLL: box/sphere contacts, feedback and falling off the floor.
local reference={
 {59,5,.571875,0,-.0100158456,4.571875,0,-.0100158457,0,0,9.99801936},
 {119,5,2.26875,0,-.01,6.26875,0,-.0100000001,0,0,10.0000001},
 {179,5,5.090625,0,-.01,9.090625,0,-.0100000001,0,0,10},
 {239,5,9.0375,0,-.00999999966,13.0375,0,-.0100000001,0,0,9.99999997},
 {299,5,14.109375,0,-.00999999951,18.109375,0,-.0100000001,0,0,9.99999998},
 {359,4,19.1625,0,-.0099999996,23.1643651,0,-3.75607769,0,0,7.99999995},
 {419,0,23.1046387,0,-4.89290368,27.0953596,0,-24.3692741,0,0,0},
 {479,0,25.9273244,0,-27.134076,29.901354,0,-62.9824706,0,0,0},
 {539,0,27.6250101,0,-67.3752484,31.5823484,0,-119.595667,0,0,0},
 {599,0,28.1976958,0,-125.616421,32.1383429,0,-194.208863,0,0,0}}
for t=0,599 do
 o.reset_feedback();local contacts={}
 for i=1,2 do o.body_force(bodies[i],t<300 and .125 or -.125,0,-2);for _,c in ipairs(o.contacts(w,group,geoms[i],floor,true))do contacts[#contacts+1]=c end end
 o.world_step(w)
 if t%60==59 then local row={t,#contacts};for _,b in ipairs(bodies)do for _,v in ipairs(o.body_vector(b,0))do row[#row+1]=v end end;local sum={0,0,0};for _,c in ipairs(contacts)do local f=o.feedback(c.joint,1);for i=1,3 do sum[i]=sum[i]+f[i]end end;for i=1,3 do row[#row+1]=sum[i]end;local expected=reference[(t+1)//60];assert(row[2]==expected[2]);for i=3,#row do near(row[i],expected[i],.00005,'ODE '..t..'/'..i)end end
 o.group_empty(group)
end
o.group_destroy(group);o.geom_destroy(floor);for _,geom in ipairs(geoms)do o.geom_destroy(geom)end;o.world_destroy(w)
local chainReference={{599,-0.980286337,-6.36093847,1.14291272,-0.0252237692,-6.60355634,1.50103316,0.949540308,-6.48335296,1.8859723,1.79248823,-5.98902652,2.31634236,2.41385808,-5.21811034,2.60966964,2.89063485,-4.33696082,2.68079432,3.32608604,-3.4332434,2.7032802,3.70564017,-2.50675861,2.80146187},{1199,0.75208507,-5.28502118,2.06679669,1.59299712,-4.74858353,2.21047471,2.39299397,-4.14850514,2.21822738,3.14603187,-3.49380011,2.3546222,3.74371646,-2.70343419,2.63334916,4.06937314,-1.77104964,2.97639083,4.1001714,-0.773927667,-3.04185101,3.86567287,0.198476356,-2.77326295}}
local ode=mcdphysics
ode.seed(1);local world=ode.world_create();ode.world_configure(world)
local bodies={}
for i=0,7 do
 local b=ode.body_create(world);bodies[i+1]=b;ode.body_gravity(b,0)
 local mass=ode.mass();ode.mass_box(mass,1,.5,1);ode.mass_adjust(mass,1);ode.body_mass(b,mass);ode.body_position(b,0,-i,0)
 if i>0 then local joint=ode.hinge(world);ode.joint_attach(joint,b,bodies[i]);ode.hinge_anchor(joint,0,-i+.5,0);ode.hinge_axis(joint,0,0,1);ode.hinge_limit(joint,0,-1);ode.hinge_limit(joint,1,1)end
end
for i=0,1199 do
 ode.body_force(bodies[1],math.sin(i*.031)*2,math.cos(i*.019)*2,0);ode.body_force_at(bodies[1],.3,0,0,0,.5,0)
 for _,b in ipairs(bodies)do local v=ode.body_vector(b,1);ode.body_velocity(b,v[1]*.99,v[2]*.99,v[3]*.99);v=ode.body_vector(b,2);ode.body_angular_velocity(b,v[1]*.9,v[2]*.9,v[3]*.9)end
 ode.world_step(world)
 if i==599 or i==1199 then local row=chainReference[i==599 and 1 or 2];for j,b in ipairs(bodies)do local p,r=ode.body_vector(b,0),ode.body_vector(b,4);near(p[1],row[(j-1)*3+2],.00005,'hinge x');near(p[2],row[(j-1)*3+3],.00005,'hinge y');near(math.atan(r[5],r[6]),row[(j-1)*3+4],.00005,'hinge angle')end end
end
ode.world_destroy(world)

print('PASS: movement, aiming, pause, eight tail joints, cutting, enhanced shots, extends, loss, ranking, nine enemy configurations, glyphs and original ODE contacts/feedback')
print('RESULT 0')
