using System;

public class OdeHandle
{
}

public class OdeMass
{
}

public class OdeContact
{
    public OdeHandle Joint;
    public float X, Y, Z;
}

public static class McdPhysics
{
    public static OdeHandle WorldCreate() => default;
    public static void WorldDestroy(OdeHandle a)
    {
    }

    public static void WorldConfigure(OdeHandle a)
    {
    }

    public static void WorldStep(OdeHandle a)
    {
    }

    public static void WorldGravity(OdeHandle a, float x, float y, float z)
    {
    }

    public static OdeHandle SpaceCreate() => default;
    public static void SpaceDestroy(OdeHandle a)
    {
    }

    public static OdeHandle GroupCreate() => default;
    public static void GroupDestroy(OdeHandle a)
    {
    }

    public static void GroupEmpty(OdeHandle a)
    {
    }

    public static void Seed(int seed)
    {
    }

    public static OdeHandle BodyCreate(OdeHandle a) => default;
    public static void BodyDestroy(OdeHandle a)
    {
    }

    public static void BodyGravity(OdeHandle a, int enabled)
    {
    }

    public static void BodyPosition(OdeHandle a, float x, float y, float z)
    {
    }

    public static void BodyForce(OdeHandle a, float x, float y, float z)
    {
    }

    public static void BodyRelForce(OdeHandle a, float x, float y, float z)
    {
    }

    public static void BodyVelocity(OdeHandle a, float x, float y, float z)
    {
    }

    public static void BodyAngularVelocity(OdeHandle a, float x, float y, float z)
    {
    }

    public static void BodySetForce(OdeHandle a, float x, float y, float z)
    {
    }

    public static void BodySetTorque(OdeHandle a, float x, float y, float z)
    {
    }

    public static void BodyForceAt(OdeHandle a, float x, float y, float z, float ox, float oy, float oz)
    {
    }

    public static void BodyEnable(OdeHandle a)
    {
    }

    public static void BodyDisable(OdeHandle a)
    {
    }

    public static void BodyMass(OdeHandle a, OdeMass m)
    {
    }

    public static OdeHandle Box(OdeHandle a, float x, float y, float z) => default;
    public static OdeHandle Sphere(OdeHandle a, float r) => default;
    public static OdeHandle Plane(OdeHandle a, float x, float y, float z, float d) => default;
    public static OdeHandle Transform(OdeHandle a) => default;
    public static void TransformGeom(OdeHandle a, OdeHandle b)
    {
    }

    public static void GeomDestroy(OdeHandle a)
    {
    }

    public static void GeomPosition(OdeHandle a, float x, float y, float z)
    {
    }

    public static void GeomBody(OdeHandle a, OdeHandle b)
    {
    }

    public static OdeHandle GetBody(OdeHandle a) => default;
    public static bool Same(OdeHandle a, OdeHandle b) => default;
    public static OdeHandle Hinge(OdeHandle a) => default;
    public static void JointDestroy(OdeHandle a)
    {
    }

    public static void JointAttach(OdeHandle a, OdeHandle b, OdeHandle c)
    {
    }

    public static void HingeAnchor(OdeHandle a, float x, float y, float z)
    {
    }

    public static void HingeAxis(OdeHandle a, float x, float y, float z)
    {
    }

    public static void HingeLimit(OdeHandle a, int param, float value)
    {
    }

    public static void MassBox(OdeMass m, float x, float y, float z)
    {
    }

    public static void MassTranslate(OdeMass m, float x, float y, float z)
    {
    }

    public static void MassAdjust(OdeMass m, float value)
    {
    }

    public static void MassAdd(OdeMass m, OdeMass n)
    {
    }

    public static OdeMass Mass() => default;
    public static float[] BodyVector(OdeHandle body, int kind) => default;
    public static void BodyRotation(OdeHandle body, float[] rotation)
    {
    }

    public static void Collide(OdeHandle space, Action<OdeHandle, OdeHandle> callback)
    {
    }

    public static void CollideWith(OdeHandle geom, OdeHandle space, Action<OdeHandle, OdeHandle> callback)
    {
    }

    public static void ResetFeedback()
    {
    }

    public static OdeContact[] Contacts(OdeHandle world, OdeHandle group, OdeHandle a, OdeHandle b, bool feedback) => default;
    public static float[] Feedback(OdeHandle joint, int body) => default;
}
