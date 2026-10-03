// tcs2c host for OdeApi.cs: the same ODE calls as ode.cpp. Include after the game source.
#include <ode/ode.h>

// An OdeMass carries its dMass behind the object, as the Lua userdata does.
typedef struct { Tcs_OdeMass object; dMass mass; } McdMass;
static void *ode(Tcs_OdeHandle *a) { return a ? (void *)(uintptr_t)a->host_value : NULL; }
static dMass *mass(Tcs_OdeMass *m) { return &((McdMass *)tcs_nonnull(m))->mass; }
static Tcs_OdeHandle *push(void *p) {
    if (!p) return NULL;
    Tcs_OdeHandle *a = tcs_new_OdeHandle();
    a->host_value = (uintptr_t)p;
    return a;
}
static TcsArray *vector(const dReal *v, int n) {
    TcsArray *result = tcs_typed(tcs_array_new(n, sizeof(float), NULL), TCS_TYPE_ARRAY_F32);
    for (int i = 0; i < n; i++) ((float *)result->data)[i] = (float)v[i];
    return result;
}

Tcs_OdeHandle *tcs_host_mcdphysics_world_create(void) { return push(dWorldCreate()); }
void tcs_host_mcdphysics_world_destroy(Tcs_OdeHandle *a) { dWorldDestroy(ode(a)); }
void tcs_host_mcdphysics_world_configure(Tcs_OdeHandle *a) { dWorldSetContactMaxCorrectingVel(ode(a), 2.5); dWorldSetContactSurfaceLayer(ode(a), .01); }
void tcs_host_mcdphysics_world_step(Tcs_OdeHandle *a) { dWorldQuickStep(ode(a), .05); }
void tcs_host_mcdphysics_world_gravity(Tcs_OdeHandle *a, float x, float y, float z) { dWorldSetGravity(ode(a), x, y, z); }
Tcs_OdeHandle *tcs_host_mcdphysics_space_create(void) { return push(dHashSpaceCreate(0)); }
void tcs_host_mcdphysics_space_destroy(Tcs_OdeHandle *a) { dSpaceDestroy(ode(a)); }
Tcs_OdeHandle *tcs_host_mcdphysics_group_create(void) { return push(dJointGroupCreate(1000)); }
void tcs_host_mcdphysics_group_destroy(Tcs_OdeHandle *a) { dJointGroupDestroy(ode(a)); }
void tcs_host_mcdphysics_group_empty(Tcs_OdeHandle *a) { dJointGroupEmpty(ode(a)); }
void tcs_host_mcdphysics_seed(int32_t seed) { dRandSetSeed(seed); }
Tcs_OdeHandle *tcs_host_mcdphysics_body_create(Tcs_OdeHandle *a) { return push(dBodyCreate(ode(a))); }
void tcs_host_mcdphysics_body_destroy(Tcs_OdeHandle *a) { dBodyDestroy(ode(a)); }
void tcs_host_mcdphysics_body_gravity(Tcs_OdeHandle *a, int32_t enabled) { dBodySetGravityMode(ode(a), enabled); }
void tcs_host_mcdphysics_body_position(Tcs_OdeHandle *a, float x, float y, float z) { dBodySetPosition(ode(a), x, y, z); }
void tcs_host_mcdphysics_body_force(Tcs_OdeHandle *a, float x, float y, float z) { dBodyAddForce(ode(a), x, y, z); }
void tcs_host_mcdphysics_body_rel_force(Tcs_OdeHandle *a, float x, float y, float z) { dBodyAddRelForce(ode(a), x, y, z); }
void tcs_host_mcdphysics_body_velocity(Tcs_OdeHandle *a, float x, float y, float z) { dBodySetLinearVel(ode(a), x, y, z); }
void tcs_host_mcdphysics_body_angular_velocity(Tcs_OdeHandle *a, float x, float y, float z) { dBodySetAngularVel(ode(a), x, y, z); }
void tcs_host_mcdphysics_body_set_force(Tcs_OdeHandle *a, float x, float y, float z) { dBodySetForce(ode(a), x, y, z); }
void tcs_host_mcdphysics_body_set_torque(Tcs_OdeHandle *a, float x, float y, float z) { dBodySetTorque(ode(a), x, y, z); }
void tcs_host_mcdphysics_body_force_at(Tcs_OdeHandle *a, float x, float y, float z, float ox, float oy, float oz) { dBodyAddRelForceAtRelPos(ode(a), x, y, z, ox, oy, oz); }
void tcs_host_mcdphysics_body_enable(Tcs_OdeHandle *a) { dBodyEnable(ode(a)); }
void tcs_host_mcdphysics_body_disable(Tcs_OdeHandle *a) { dBodyDisable(ode(a)); }
void tcs_host_mcdphysics_body_mass(Tcs_OdeHandle *a, Tcs_OdeMass *m) { dBodySetMass(ode(a), mass(m)); }
Tcs_OdeHandle *tcs_host_mcdphysics_box(Tcs_OdeHandle *a, float x, float y, float z) { return push(dCreateBox(ode(a), x, y, z)); }
Tcs_OdeHandle *tcs_host_mcdphysics_sphere(Tcs_OdeHandle *a, float r) { return push(dCreateSphere(ode(a), r)); }
Tcs_OdeHandle *tcs_host_mcdphysics_plane(Tcs_OdeHandle *a, float x, float y, float z, float d) { return push(dCreatePlane(ode(a), x, y, z, d)); }
Tcs_OdeHandle *tcs_host_mcdphysics_transform(Tcs_OdeHandle *a) { return push(dCreateGeomTransform(ode(a))); }
void tcs_host_mcdphysics_transform_geom(Tcs_OdeHandle *a, Tcs_OdeHandle *b) { dGeomTransformSetGeom(ode(a), ode(b)); }
void tcs_host_mcdphysics_geom_destroy(Tcs_OdeHandle *a) { dGeomDestroy(ode(a)); }
void tcs_host_mcdphysics_geom_position(Tcs_OdeHandle *a, float x, float y, float z) { dGeomSetPosition(ode(a), x, y, z); }
void tcs_host_mcdphysics_geom_body(Tcs_OdeHandle *a, Tcs_OdeHandle *b) { dGeomSetBody(ode(a), ode(b)); }
Tcs_OdeHandle *tcs_host_mcdphysics_get_body(Tcs_OdeHandle *a) { return push(dGeomGetBody(ode(a))); }
bool tcs_host_mcdphysics_same(Tcs_OdeHandle *a, Tcs_OdeHandle *b) { return ode(a) == ode(b); }
Tcs_OdeHandle *tcs_host_mcdphysics_hinge(Tcs_OdeHandle *a) { return push(dJointCreateHinge(ode(a), 0)); }
void tcs_host_mcdphysics_joint_destroy(Tcs_OdeHandle *a) { dJointDestroy(ode(a)); }
void tcs_host_mcdphysics_joint_attach(Tcs_OdeHandle *a, Tcs_OdeHandle *b, Tcs_OdeHandle *c) { dJointAttach(ode(a), ode(b), ode(c)); }
void tcs_host_mcdphysics_hinge_anchor(Tcs_OdeHandle *a, float x, float y, float z) { dJointSetHingeAnchor(ode(a), x, y, z); }
void tcs_host_mcdphysics_hinge_axis(Tcs_OdeHandle *a, float x, float y, float z) { dJointSetHingeAxis(ode(a), x, y, z); }
void tcs_host_mcdphysics_hinge_limit(Tcs_OdeHandle *a, int32_t param, float value) { dJointSetHingeParam(ode(a), param, value); }
void tcs_host_mcdphysics_mass_box(Tcs_OdeMass *m, float x, float y, float z) { dMassSetBox(mass(m), 1, x, y, z); }
void tcs_host_mcdphysics_mass_translate(Tcs_OdeMass *m, float x, float y, float z) { dMassTranslate(mass(m), x, y, z); }
void tcs_host_mcdphysics_mass_adjust(Tcs_OdeMass *m, float value) { dMassAdjust(mass(m), value); }
void tcs_host_mcdphysics_mass_add(Tcs_OdeMass *m, Tcs_OdeMass *n) { dMassAdd(mass(m), mass(n)); }
Tcs_OdeMass *tcs_host_mcdphysics_mass(void) {
    McdMass *m = tcs_gc_alloc(TCS_KIND_OBJECT, &tcs_layout_C_OdeMass, sizeof *m);
    tcs_init_OdeMass(&m->object);
    dMassSetZero(&m->mass);
    return &m->object;
}
TcsArray *tcs_host_mcdphysics_body_vector(Tcs_OdeHandle *a, int32_t k) {
    dBodyID b = ode(a);
    const dReal *v = k == 0 ? dBodyGetPosition(b) : k == 1 ? dBodyGetLinearVel(b) : k == 2 ? dBodyGetAngularVel(b) : k == 3 ? dBodyGetForce(b) : dBodyGetRotation(b);
    return vector(v, k == 4 ? 12 : 3);
}
void tcs_host_mcdphysics_body_rotation(Tcs_OdeHandle *a, TcsArray *rotation) {
    dMatrix3 m;
    for (int i = 0; i < 12; i++) m[i] = *(float *)tcs_array_at(rotation, i);
    dBodySetRotation(ode(a), m);
}
static void near_callback(void *data, dGeomID a, dGeomID b) {
    TcsClosure *c = data;
    ((void (*)(void **, Tcs_OdeHandle *, Tcs_OdeHandle *))c->fn)(c->cells, push(a), push(b));
}
void tcs_host_mcdphysics_collide(Tcs_OdeHandle *a, TcsClosure *callback) { dSpaceCollide(ode(a), tcs_nonnull(callback), near_callback); }
void tcs_host_mcdphysics_collide_with(Tcs_OdeHandle *a, Tcs_OdeHandle *b, TcsClosure *callback) { dSpaceCollide2(ode(a), ode(b), tcs_nonnull(callback), near_callback); }
static dJointFeedback feedbacks[100];
static int feedback_count;
void tcs_host_mcdphysics_reset_feedback(void) { feedback_count = 0; }
TcsArray *tcs_host_mcdphysics_contacts(Tcs_OdeHandle *world, Tcs_OdeHandle *group, Tcs_OdeHandle *ga, Tcs_OdeHandle *gb, bool feedback) {
    dGeomID a = ode(ga), b = ode(gb);
    dContact contact[4] = {0};
    int count = dCollide(a, b, 4, &contact[0].geom, sizeof(dContact));
    int kept = !feedback ? 0 : count < 100 - feedback_count ? count : 100 - feedback_count;
    TcsArray *result = tcs_array_new(kept, sizeof(Tcs_OdeContact *), &tcs_layout_ptr);
    for (int i = 0; i < count; i++) {
        dJointID joint = dJointCreateContact(ode(world), ode(group), &contact[i]);
        dJointAttach(joint, dGeomGetBody(a), dGeomGetBody(b));
        if (i >= kept) continue;
        dJointSetFeedback(joint, &feedbacks[feedback_count++]);
        Tcs_OdeContact *c = tcs_new_OdeContact();
        c->f_joint = push(joint);
        c->f_x = (float)contact[i].geom.pos[0];
        c->f_y = (float)contact[i].geom.pos[1];
        c->f_z = (float)contact[i].geom.pos[2];
        ((Tcs_OdeContact **)result->data)[i] = c;
    }
    return result;
}
TcsArray *tcs_host_mcdphysics_feedback(Tcs_OdeHandle *a, int32_t body) {
    dJointFeedback *fb = dJointGetFeedback(ode(a));
    return vector(body == 1 ? fb->f1 : fb->f2, 3);
}
