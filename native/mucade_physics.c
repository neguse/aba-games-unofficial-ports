/* Native C ABI for games/mu-cade/OdeApi.cs. Physics and constants deliberately
 * match games/mu-cade/ode_host.c, including ODE 0.5's double-precision solver. */
#include <ode/ode.h>
#include <string.h>

#ifdef _WIN32
#define MCD_API __declspec(dllexport)
#else
#define MCD_API __attribute__((visibility("default")))
#endif
#ifdef __cplusplus
extern "C" {
#endif

MCD_API dWorldID mcd_world_create(void) { return dWorldCreate(); }
MCD_API void mcd_world_destroy(dWorldID a) { dWorldDestroy(a); }
MCD_API void mcd_world_configure(dWorldID a) { dWorldSetContactMaxCorrectingVel(a, 2.5); dWorldSetContactSurfaceLayer(a, .01); }
MCD_API void mcd_world_step(dWorldID a) { dWorldQuickStep(a, .05); }
MCD_API void mcd_world_gravity(dWorldID a, float x, float y, float z) { dWorldSetGravity(a, x, y, z); }
MCD_API dSpaceID mcd_space_create(void) { return dHashSpaceCreate(0); }
MCD_API void mcd_space_destroy(dSpaceID a) { dSpaceDestroy(a); }
MCD_API dJointGroupID mcd_group_create(void) { return dJointGroupCreate(1000); }
MCD_API void mcd_group_destroy(dJointGroupID a) { dJointGroupDestroy(a); }
MCD_API void mcd_group_empty(dJointGroupID a) { dJointGroupEmpty(a); }
MCD_API void mcd_seed(int seed) { dRandSetSeed(seed); }
MCD_API dBodyID mcd_body_create(dWorldID a) { return dBodyCreate(a); }
MCD_API void mcd_body_destroy(dBodyID a) { dBodyDestroy(a); }
MCD_API void mcd_body_gravity(dBodyID a, int enabled) { dBodySetGravityMode(a, enabled); }
MCD_API void mcd_body_position(dBodyID a, float x, float y, float z) { dBodySetPosition(a, x, y, z); }
MCD_API void mcd_body_force(dBodyID a, float x, float y, float z) { dBodyAddForce(a, x, y, z); }
MCD_API void mcd_body_rel_force(dBodyID a, float x, float y, float z) { dBodyAddRelForce(a, x, y, z); }
MCD_API void mcd_body_velocity(dBodyID a, float x, float y, float z) { dBodySetLinearVel(a, x, y, z); }
MCD_API void mcd_body_angular_velocity(dBodyID a, float x, float y, float z) { dBodySetAngularVel(a, x, y, z); }
MCD_API void mcd_body_set_force(dBodyID a, float x, float y, float z) { dBodySetForce(a, x, y, z); }
MCD_API void mcd_body_set_torque(dBodyID a, float x, float y, float z) { dBodySetTorque(a, x, y, z); }
MCD_API void mcd_body_force_at(dBodyID a, float x, float y, float z, float ox, float oy, float oz) { dBodyAddRelForceAtRelPos(a, x, y, z, ox, oy, oz); }
MCD_API void mcd_body_enable(dBodyID a) { dBodyEnable(a); }
MCD_API void mcd_body_disable(dBodyID a) { dBodyDisable(a); }
MCD_API void mcd_body_mass(dBodyID a, const dMass *m) { dBodySetMass(a, m); }
MCD_API dGeomID mcd_box(dSpaceID a, float x, float y, float z) { return dCreateBox(a, x, y, z); }
MCD_API dGeomID mcd_sphere(dSpaceID a, float r) { return dCreateSphere(a, r); }
MCD_API dGeomID mcd_plane(dSpaceID a, float x, float y, float z, float d) { return dCreatePlane(a, x, y, z, d); }
MCD_API dGeomID mcd_transform(dSpaceID a) { return dCreateGeomTransform(a); }
MCD_API void mcd_transform_geom(dGeomID a, dGeomID b) { dGeomTransformSetGeom(a, b); }
MCD_API void mcd_geom_destroy(dGeomID a) { dGeomDestroy(a); }
MCD_API void mcd_geom_position(dGeomID a, float x, float y, float z) { dGeomSetPosition(a, x, y, z); }
MCD_API void mcd_geom_body(dGeomID a, dBodyID b) { dGeomSetBody(a, b); }
MCD_API dBodyID mcd_get_body(dGeomID a) { return dGeomGetBody(a); }
MCD_API dJointID mcd_hinge(dWorldID a) { return dJointCreateHinge(a, 0); }
MCD_API void mcd_joint_destroy(dJointID a) { dJointDestroy(a); }
MCD_API void mcd_joint_attach(dJointID a, dBodyID b, dBodyID c) { dJointAttach(a, b, c); }
MCD_API void mcd_hinge_anchor(dJointID a, float x, float y, float z) { dJointSetHingeAnchor(a, x, y, z); }
MCD_API void mcd_hinge_axis(dJointID a, float x, float y, float z) { dJointSetHingeAxis(a, x, y, z); }
MCD_API void mcd_hinge_limit(dJointID a, int param, float value) { dJointSetHingeParam(a, param, value); }

/* dMass has 17 doubles (mass, padded center vector, padded inertia matrix).
 * Managed arrays own this storage, so temporary masses never allocate native
 * memory and require neither finalizers nor collectible-assembly callbacks. */
MCD_API int mcd_mass_size(void) { return (int)sizeof(dMass); }
MCD_API void mcd_mass_box(dMass *m, float x, float y, float z) { dMassSetBox(m, 1, x, y, z); }
MCD_API void mcd_mass_translate(dMass *m, float x, float y, float z) { dMassTranslate(m, x, y, z); }
MCD_API void mcd_mass_adjust(dMass *m, float value) { dMassAdjust(m, value); }
MCD_API void mcd_mass_add(dMass *m, const dMass *n) { dMassAdd(m, n); }

MCD_API void mcd_body_vector(dBodyID a, int kind, float *result) {
    const dReal *v = kind == 0 ? dBodyGetPosition(a) : kind == 1 ? dBodyGetLinearVel(a) : kind == 2 ? dBodyGetAngularVel(a) : kind == 3 ? dBodyGetForce(a) : dBodyGetRotation(a);
    int n = kind == 4 ? 12 : 3;
    for (int i = 0; i < n; i++) result[i] = (float)v[i];
}
MCD_API void mcd_body_rotation(dBodyID a, const float *rotation) {
    dMatrix3 m;
    for (int i = 0; i < 12; i++) m[i] = rotation[i];
    dBodySetRotation(a, m);
}
MCD_API void mcd_collide(dSpaceID a, dNearCallback *callback) { dSpaceCollide(a, 0, callback); }
MCD_API void mcd_collide_with(dGeomID a, dGeomID b, dNearCallback *callback) { dSpaceCollide2(a, b, 0, callback); }

static dJointFeedback feedbacks[100];
static int feedback_count;
MCD_API void mcd_reset_feedback(void) { feedback_count = 0; }
MCD_API int mcd_contacts(dWorldID world, dJointGroupID group, dGeomID a, dGeomID b, int feedback, dJointID *joints, float *positions) {
    dContact contact[4] = {0};
    int count = dCollide(a, b, 4, &contact[0].geom, sizeof(dContact));
    int kept = !feedback ? 0 : count < 100 - feedback_count ? count : 100 - feedback_count;
    for (int i = 0; i < count; i++) {
        dJointID joint = dJointCreateContact(world, group, &contact[i]);
        dJointAttach(joint, dGeomGetBody(a), dGeomGetBody(b));
        if (i >= kept) continue;
        dJointSetFeedback(joint, &feedbacks[feedback_count++]);
        joints[i] = joint;
        for (int k = 0; k < 3; k++) positions[i * 3 + k] = (float)contact[i].geom.pos[k];
    }
    return kept;
}
MCD_API int mcd_feedback(dJointID a, int body, float *result) {
    dJointFeedback *fb = dJointGetFeedback(a);
    if (!fb) return 0;
    const dReal *v = body == 1 ? fb->f1 : fb->f2;
    for (int i = 0; i < 3; i++) result[i] = (float)v[i];
    return 1;
}
MCD_API void mcd_shutdown(void) {
    feedback_count = 0;
    memset(feedbacks, 0, sizeof(feedbacks));
    dCloseODE();
}
#ifdef __cplusplus
}
#endif
