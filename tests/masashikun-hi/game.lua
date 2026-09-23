lub={config=function()end,host={available=function()return false end}}
local game=dofile(arg[1]);game.on_init()
local function hash(h,value)return(h~value)*16777619 end
local function pixels()
 local p={};for n=1,640*480 do p[n]=0 end
 for n=1,#MasForm.spans,5 do
  local x,y,w,h,c=table.unpack(MasForm.spans,n,n+4)
  for py=y,y+h-1 do for px=x,x+w-1 do assert(px>=0 and px<640 and py>=0 and py<480);p[py*640+px+1]=c end end
 end
 local h=-2128831035;for n=1,#p do h=hash(h,p[n])end;return h
end
local initials={MasTitle.inittitle,MasKak.initkak,MasTob.inittob,MasOok.initook,MasHng.inithng,MasGfi.initgfi}
local objects={MasTitle,MasKak,MasTob,MasOok,MasHng,MasGfi}
local fields={
{"bfmlspe","titlecou","demospe","td","tr","stn"},
{"myx","myy","myp","myd","myc","mychar","myvy","speed","tmpsp","mvsum","zm","kakcou","kakdemocou","time"},
{"myx","myy","myp","myc","mychar","mymy","mymx","speed","tmpsp","mvsum","tobtry","zm","tobcou","tobdemocou","tbc","tbcc"},
{"myx","myy","myp","myc","mychar","speed","mvsum","myhng","dist","endist","enhng","enspeed","plcou","enplcou","time","zm","ookcou","ookdemocou"},
{"myp","myx","myy","myd","mymx","mymy","mytd","speed","tmpsp","mypt","dist","flx","fly","flmx","flmy","zm","hngcou","hngdemocou","dmmbtcou","hngtry"},
{"myp","myx","maxmymy","myy","tpy","myd","mymy","myc","speed","tmpsp","dist","mvsum","mychar","flx","fly","kkx","zm","gficou","gfidemocou"},
}

-- Original Pascal, Delphi argument order and the two 1.11e adjacent-memory reads.
local expected={721291957,-299353891,2025337817,-1483790467,-214543261,153716629}
local raster={{-1516359561,1194163777,-1016830567,1351991218,1282498746},{-184272645,24323694,-1012357428,-1294942889,-1162818059},{514100574,-529365398,-43117824,-716159673,-445345398},{686293252,-1041335864,-1843232595,506457703,-844295022},{1427633240,1404584644,-58863044,-1658075133,-1091368258},{1665416618,852244286,-419671087,1317037097,-1241489979}}

for g=1,6 do
 MasMath.seed=12345;MasHira.clearhira();MasMain.sucf=false;initials[g]()
 local h=-2128831035;local snapshot=1
 for i=0,599 do
  MasForm.mousemv=(i*71)%1200;MasForm.mousebt=i%19==0 and 1 or 0
  MasMain.moveall();MasForm.spans={};MasMain.putall();MasScores.accept('')
  h=hash(hash(h,MasMain.mlspe),MasMath.seed)
  for _,field in ipairs(fields[g])do h=hash(h,objects[g][field])end
  if i==64 or i==127 or i==255 or i==511 or i==599 then
   assert(pixels()==raster[g][snapshot],'raster '..g..'/'..i);snapshot=snapshot+1
  end
 end
 assert(h==expected[g],'state '..g)
end
print('PASS: 3600 original Pascal updates and 30 exact rasters')
MasMath.seed=1;Game.start(0)
local seen={};local completed=false
for i=1,12000 do
 local state=MasMain.mlspe;seen[state]=true
 MasForm.mousemv=1500;MasForm.mousebt=i%19==0 and 1 or 0
 MasMain.moveall();MasForm.spans={};MasMain.putall();MasScores.accept('Player')
 if seen[16] and MasMain.mlspe==0 then completed=true;break end
end
assert(completed,'five-event competition completion')
for state=1,16 do assert(seen[state],'competition state '..state)end
local total=0;for _,score in ipairs(MasMain.score)do total=total+score end
assert(MasResult.ttlhsc[2].rec==total and total>0)
Game.start(1);MasKak.kakcou=100;Game.pause();assert(MasMain.mlspe==-1 and not MasForm.hidden)
for i=1,50 do MasMain.moveall()end;assert(MasKak.kakcou==100)
Game.pause();assert(MasMain.mlspe==1 and MasForm.hidden)
MasScores.clear();MasKak.setkakhiscore(12345);assert(MasScores.hscsf);MasScores.accept('A');MasKak.setkakhiscore(12000);MasScores.accept('B')
assert(MasKak.kakhsc[2].name=='B' and MasKak.kakhsc[3].name=='A' and MasKak.kakhsc[3].rec==12345)
MasTob.settobhiscore(123);MasScores.accept('Jump');MasHng.sethnghiscore(5000);MasScores.accept('Throw')
local saved;lub.host.available=function()return true end;lub.host.send=function(topic,text)if topic=='scores.save'then saved=text end end
MasScores.save();MasScores.clear();MasScores.load(saved)
assert(MasKak.kakhsc[2].rec==12000 and MasTob.tobhsc[2].name=='Jump' and MasHng.hnghsc[2].rec==5000)
MasScores.load(saved:gsub('12000','oops'));assert(MasKak.kakhsc[2].rec==12000)
print('PASS: all 16 competition states, three attempts, total score, pause and six-category score storage')
lub.host.available=function()return false end
MasMath.seed=12345;Game.start(6);seen={}
for i=1,10000 do
 seen[MasMain.mlspe]=true;MasForm.mousemv=0;MasForm.mousebt=0
 MasMain.moveall();MasForm.spans={};MasMain.putall()
 if seen[-2] and seen[-3] and seen[-4] and seen[-5] and seen[-6] then break end
end
for n=-6,-2 do assert(seen[n],'automatic demo '..n)end
local draw=MasForm.frame;MasForm.frame=function()end
local queue={};lub.host.available=function()return true end;lub.host.poll=function()local m=table.remove(queue,1);if m then return m[1],m[2]end end
local observed={};local move=MasMain.moveall;MasMain.moveall=function()observed[#observed+1]={MasForm.mousemv,MasForm.mousebt};move()end
Game.start(1);Game.elapsed=0
queue={{'move','3,0'},{'move','0,4'},{'button','1'},{'button','0'}}
Game.on_frame(.02);assert(#observed==0);Game.on_frame(.02);assert(observed[1][1]==5 and observed[1][2]==1)
Game.on_frame(.03);assert(observed[2][1]==0 and observed[2][2]==0)
queue={{'shift',''},{'button','1'}};Game.on_frame(.033);assert(observed[3][1]==768 and observed[3][2]==1)
Game.on_frame(.033);assert(observed[4][2]==2)
MasMain.moveall=move;MasForm.frame=draw
print('PASS: all five automatic demos, 33ms input accumulation, short click, held click and Shift')
print('RESULT 0')
