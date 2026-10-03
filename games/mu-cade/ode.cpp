#include <ode/ode.h>
extern "C" {
#include <lua.h>
#include <lauxlib.h>
}
struct Handle {
 void* p;
 template<typename T> operator T*() const {return static_cast<T*>(p);}
};
static Handle handle(lua_State* L,int n){return {lua_touserdata(L,n)};}
static int push(lua_State* L,void* p){if(p)lua_pushlightuserdata(L,p);else lua_pushnil(L);return 1;}
static void vector(lua_State* L,const dReal* v,int n){lua_createtable(L,n,0);for(int i=0;i<n;i++){lua_pushnumber(L,v[i]);lua_rawseti(L,-2,i+1);}}
static int world_create(lua_State* L){return push(L,dWorldCreate());}
static int world_destroy(lua_State* L){auto a=handle(L,1);dWorldDestroy(a);return 0;}
static int world_configure(lua_State* L){auto a=handle(L,1);dWorldSetContactMaxCorrectingVel(a,2.5);dWorldSetContactSurfaceLayer(a,.01);return 0;}
static int world_step(lua_State* L){auto a=handle(L,1);dWorldQuickStep(a,.05);return 0;}
static int world_gravity(lua_State* L){auto a=handle(L,1);auto x=double(luaL_checknumber(L,2));auto y=double(luaL_checknumber(L,3));auto z=double(luaL_checknumber(L,4));dWorldSetGravity(a,x,y,z);return 0;}
static int space_create(lua_State* L){return push(L,dHashSpaceCreate(0));}
static int space_destroy(lua_State* L){auto a=handle(L,1);dSpaceDestroy(a);return 0;}
static int group_create(lua_State* L){return push(L,dJointGroupCreate(1000));}
static int group_destroy(lua_State* L){auto a=handle(L,1);dJointGroupDestroy(a);return 0;}
static int group_empty(lua_State* L){auto a=handle(L,1);dJointGroupEmpty(a);return 0;}
static int seed(lua_State* L){auto seed=int(luaL_checkinteger(L,1));dRandSetSeed(seed);return 0;}
static int body_create(lua_State* L){auto a=handle(L,1);return push(L,dBodyCreate(a));}
static int body_destroy(lua_State* L){auto a=handle(L,1);dBodyDestroy(a);return 0;}
static int body_gravity(lua_State* L){auto a=handle(L,1);auto enabled=int(luaL_checkinteger(L,2));dBodySetGravityMode(a,enabled);return 0;}
static int body_position(lua_State* L){auto a=handle(L,1);auto x=double(luaL_checknumber(L,2));auto y=double(luaL_checknumber(L,3));auto z=double(luaL_checknumber(L,4));dBodySetPosition(a,x,y,z);return 0;}
static int body_force(lua_State* L){auto a=handle(L,1);auto x=double(luaL_checknumber(L,2));auto y=double(luaL_checknumber(L,3));auto z=double(luaL_checknumber(L,4));dBodyAddForce(a,x,y,z);return 0;}
static int body_rel_force(lua_State* L){auto a=handle(L,1);auto x=double(luaL_checknumber(L,2));auto y=double(luaL_checknumber(L,3));auto z=double(luaL_checknumber(L,4));dBodyAddRelForce(a,x,y,z);return 0;}
static int body_velocity(lua_State* L){auto a=handle(L,1);auto x=double(luaL_checknumber(L,2));auto y=double(luaL_checknumber(L,3));auto z=double(luaL_checknumber(L,4));dBodySetLinearVel(a,x,y,z);return 0;}
static int body_angular_velocity(lua_State* L){auto a=handle(L,1);auto x=double(luaL_checknumber(L,2));auto y=double(luaL_checknumber(L,3));auto z=double(luaL_checknumber(L,4));dBodySetAngularVel(a,x,y,z);return 0;}
static int body_set_force(lua_State* L){auto a=handle(L,1);auto x=double(luaL_checknumber(L,2));auto y=double(luaL_checknumber(L,3));auto z=double(luaL_checknumber(L,4));dBodySetForce(a,x,y,z);return 0;}
static int body_set_torque(lua_State* L){auto a=handle(L,1);auto x=double(luaL_checknumber(L,2));auto y=double(luaL_checknumber(L,3));auto z=double(luaL_checknumber(L,4));dBodySetTorque(a,x,y,z);return 0;}
static int body_force_at(lua_State* L){auto a=handle(L,1);auto x=double(luaL_checknumber(L,2));auto y=double(luaL_checknumber(L,3));auto z=double(luaL_checknumber(L,4));auto ox=double(luaL_checknumber(L,5));auto oy=double(luaL_checknumber(L,6));auto oz=double(luaL_checknumber(L,7));dBodyAddRelForceAtRelPos(a,x,y,z,ox,oy,oz);return 0;}
static int body_enable(lua_State* L){auto a=handle(L,1);dBodyEnable(a);return 0;}
static int body_disable(lua_State* L){auto a=handle(L,1);dBodyDisable(a);return 0;}
static int body_mass(lua_State* L){auto a=handle(L,1);auto m=static_cast<dMass*>(lua_touserdata(L,2));dBodySetMass(a,m);return 0;}
static int box(lua_State* L){auto a=handle(L,1);auto x=double(luaL_checknumber(L,2));auto y=double(luaL_checknumber(L,3));auto z=double(luaL_checknumber(L,4));return push(L,dCreateBox(a,x,y,z));}
static int sphere(lua_State* L){auto a=handle(L,1);auto r=double(luaL_checknumber(L,2));return push(L,dCreateSphere(a,r));}
static int plane(lua_State* L){auto a=handle(L,1);auto x=double(luaL_checknumber(L,2));auto y=double(luaL_checknumber(L,3));auto z=double(luaL_checknumber(L,4));auto d=double(luaL_checknumber(L,5));return push(L,dCreatePlane(a,x,y,z,d));}
static int transform(lua_State* L){auto a=handle(L,1);return push(L,dCreateGeomTransform(a));}
static int transform_geom(lua_State* L){auto a=handle(L,1);auto b=handle(L,2);dGeomTransformSetGeom(a,b);return 0;}
static int geom_destroy(lua_State* L){auto a=handle(L,1);dGeomDestroy(a);return 0;}
static int geom_position(lua_State* L){auto a=handle(L,1);auto x=double(luaL_checknumber(L,2));auto y=double(luaL_checknumber(L,3));auto z=double(luaL_checknumber(L,4));dGeomSetPosition(a,x,y,z);return 0;}
static int geom_body(lua_State* L){auto a=handle(L,1);auto b=handle(L,2);dGeomSetBody(a,b);return 0;}
static int get_body(lua_State* L){auto a=handle(L,1);return push(L,dGeomGetBody(a));}
static int same(lua_State* L){lua_pushboolean(L,lua_touserdata(L,1)==lua_touserdata(L,2));return 1;}
static int hinge(lua_State* L){auto a=handle(L,1);return push(L,dJointCreateHinge(a,0));}
static int joint_destroy(lua_State* L){auto a=handle(L,1);dJointDestroy(a);return 0;}
static int joint_attach(lua_State* L){auto a=handle(L,1);auto b=handle(L,2);auto c=handle(L,3);dJointAttach(a,b,c);return 0;}
static int hinge_anchor(lua_State* L){auto a=handle(L,1);auto x=double(luaL_checknumber(L,2));auto y=double(luaL_checknumber(L,3));auto z=double(luaL_checknumber(L,4));dJointSetHingeAnchor(a,x,y,z);return 0;}
static int hinge_axis(lua_State* L){auto a=handle(L,1);auto x=double(luaL_checknumber(L,2));auto y=double(luaL_checknumber(L,3));auto z=double(luaL_checknumber(L,4));dJointSetHingeAxis(a,x,y,z);return 0;}
static int hinge_limit(lua_State* L){auto a=handle(L,1);auto param=int(luaL_checkinteger(L,2));auto value=double(luaL_checknumber(L,3));dJointSetHingeParam(a,param,value);return 0;}
static int mass_box(lua_State* L){auto m=static_cast<dMass*>(lua_touserdata(L,1));auto x=double(luaL_checknumber(L,2));auto y=double(luaL_checknumber(L,3));auto z=double(luaL_checknumber(L,4));dMassSetBox(m,1,x,y,z);return 0;}
static int mass_translate(lua_State* L){auto m=static_cast<dMass*>(lua_touserdata(L,1));auto x=double(luaL_checknumber(L,2));auto y=double(luaL_checknumber(L,3));auto z=double(luaL_checknumber(L,4));dMassTranslate(m,x,y,z);return 0;}
static int mass_adjust(lua_State* L){auto m=static_cast<dMass*>(lua_touserdata(L,1));auto value=double(luaL_checknumber(L,2));dMassAdjust(m,value);return 0;}
static int mass_add(lua_State* L){auto m=static_cast<dMass*>(lua_touserdata(L,1));auto n=static_cast<dMass*>(lua_touserdata(L,2));dMassAdd(m,n);return 0;}
static int mass(lua_State* L){auto* m=static_cast<dMass*>(lua_newuserdatauv(L,sizeof(dMass),0));dMassSetZero(m);return 1;}
static int body_vector(lua_State* L){dBodyID b=handle(L,1);int k=luaL_checkinteger(L,2);const dReal* v=k==0?dBodyGetPosition(b):k==1?dBodyGetLinearVel(b):k==2?dBodyGetAngularVel(b):k==3?dBodyGetForce(b):dBodyGetRotation(b);vector(L,v,k==4?12:3);return 1;}
static int body_rotation(lua_State* L){dMatrix3 m;for(int i=0;i<12;i++){lua_rawgeti(L,2,i+1);m[i]=luaL_checknumber(L,-1);lua_pop(L,1);}dBodySetRotation(handle(L,1),m);return 0;}
struct Callback {lua_State* L;int index;};
static void near_callback(void* data,dGeomID a,dGeomID b){auto* c=static_cast<Callback*>(data);lua_pushvalue(c->L,c->index);push(c->L,a);push(c->L,b);lua_call(c->L,2,0);}
static int collide(lua_State* L){Callback c{L,2};dSpaceCollide(handle(L,1),&c,near_callback);return 0;}
static int collide_with(lua_State* L){Callback c{L,3};dSpaceCollide2(handle(L,1),handle(L,2),&c,near_callback);return 0;}
static dJointFeedback feedbacks[100];
static int feedback_count;
static int reset_feedback(lua_State*){feedback_count=0;return 0;}
static int contacts(lua_State* L){
 dWorldID world=handle(L,1);dJointGroupID group=handle(L,2);dGeomID a=handle(L,3),b=handle(L,4);
 bool feedback=lua_toboolean(L,5);dContact contact[4]{};
 int count=dCollide(a,b,4,&contact[0].geom,sizeof(dContact));lua_createtable(L,count,0);
 for(int i=0;i<count;i++){
  auto joint=dJointCreateContact(world,group,&contact[i]);dJointAttach(joint,dGeomGetBody(a),dGeomGetBody(b));
  if(!feedback||feedback_count>=100)continue;
  dJointSetFeedback(joint,&feedbacks[feedback_count++]);
  lua_createtable(L,0,4);push(L,joint);lua_setfield(L,-2,"joint");
  const char* names[]={"x","y","z"};for(int j=0;j<3;j++){lua_pushnumber(L,contact[i].geom.pos[j]);lua_setfield(L,-2,names[j]);}
  lua_rawseti(L,-2,i+1);
 }
 return 1;
}
static int feedback(lua_State* L){auto* fb=dJointGetFeedback(handle(L,1));vector(L,luaL_checkinteger(L,2)==1?fb->f1:fb->f2,3);return 1;}
extern "C" int luaopen_mcd_ode(lua_State* L){
 const luaL_Reg functions[]={
{"world_create",world_create},
{"world_destroy",world_destroy},
{"world_configure",world_configure},
{"world_step",world_step},
{"world_gravity",world_gravity},
{"space_create",space_create},
{"space_destroy",space_destroy},
{"group_create",group_create},
{"group_destroy",group_destroy},
{"group_empty",group_empty},
{"seed",seed},
{"body_create",body_create},
{"body_destroy",body_destroy},
{"body_gravity",body_gravity},
{"body_position",body_position},
{"body_force",body_force},
{"body_rel_force",body_rel_force},
{"body_velocity",body_velocity},
{"body_angular_velocity",body_angular_velocity},
{"body_set_force",body_set_force},
{"body_set_torque",body_set_torque},
{"body_force_at",body_force_at},
{"body_enable",body_enable},
{"body_disable",body_disable},
{"body_mass",body_mass},
{"box",box},
{"sphere",sphere},
{"plane",plane},
{"transform",transform},
{"transform_geom",transform_geom},
{"geom_destroy",geom_destroy},
{"geom_position",geom_position},
{"geom_body",geom_body},
{"get_body",get_body},
{"same",same},
{"hinge",hinge},
{"joint_destroy",joint_destroy},
{"joint_attach",joint_attach},
{"hinge_anchor",hinge_anchor},
{"hinge_axis",hinge_axis},
{"hinge_limit",hinge_limit},
{"mass_box",mass_box},
{"mass_translate",mass_translate},
{"mass_adjust",mass_adjust},
{"mass_add",mass_add},
{"mass",mass},
{"body_vector",body_vector},
{"body_rotation",body_rotation},
{"collide",collide},
{"collide_with",collide_with},
{"reset_feedback",reset_feedback},
{"contacts",contacts},
{"feedback",feedback},
{nullptr,nullptr}};
 luaL_newlib(L,functions);return 1;
}
