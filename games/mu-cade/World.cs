// Copyright 2006 Kenta Cho. Some rights reserved.
using System.Collections.Generic;

public class Collision
{
    public bool hit, feedback;
}

public class World
{
    public static OdeHandle world, contactGroup;
    public static Dictionary<OdeHandle, OdeActor> actor = new Dictionary<OdeHandle, OdeActor>();
    public static OdeActor staticActor;
    public OdeHandle space;
    public bool initialized;
    public virtual void init_0()
    {
        world = McdPhysics.WorldCreate();
        McdPhysics.WorldConfigure(world);
        space = McdPhysics.SpaceCreate();
        contactGroup = McdPhysics.GroupCreate();
        actor = new Dictionary<OdeHandle, OdeActor>();
        initialized = true;
    }

    public virtual void close()
    {
        if (!(initialized))
            return;
        McdPhysics.GroupDestroy(contactGroup);
        McdPhysics.SpaceDestroy(space);
        McdPhysics.WorldDestroy(world);
        initialized = false;
    }

    public virtual void move_1(float step)
    {
        McdPhysics.Collide(space, nearCallback);
        McdPhysics.WorldStep(world);
    }

    public virtual void resetJointFeedback()
    {
        McdPhysics.ResetFeedback();
    }

    public virtual void removeAllContactJoints()
    {
        McdPhysics.GroupEmpty(contactGroup);
    }

    public virtual void setGravity(float x, float y, float z)
    {
        McdPhysics.WorldGravity(world, x, y, z);
    }

    public virtual void createPlane(float a, float b, float c, float d)
    {
        McdPhysics.Plane(space, a, b, c, d);
    }

    public virtual OdeHandle bodyCreate(OdeActor a)
    {
        var id = McdPhysics.BodyCreate(world);
        storeOdeActor(id, a);
        return id;
    }

    public virtual void storeOdeActor(OdeHandle id, OdeActor a)
    {
        if (id == null)
            staticActor = a;
        else
            actor[id] = a;
    }

    public virtual void bodyDestroy(OdeHandle id)
    {
        actor.Remove(id);
        McdPhysics.BodyDestroy(id);
    }

    public static void nearCallback(OdeHandle o1, OdeHandle o2)
    {
        var b1 = McdPhysics.GetBody(o1);
        var b2 = McdPhysics.GetBody(o2);
        if (McdPhysics.Same(b1, b2))
            return;
        var a1 = b1 == null ? staticActor : actor[b1];
        var a2 = b2 == null ? staticActor : actor[b2];
        var f1 = new Collision();
        var f2 = new Collision();
        a1.collide_2(a2, f1);
        a2.collide_2(a1, f2);
        if (!(f1.hit) && !(f2.hit))
            return;
        foreach (var c in McdPhysics.Contacts(world, contactGroup, o1, o2, f1.feedback || f2.feedback))
        {
            if (f1.feedback)
                a1.addContactJoint(c.X, c.Y, c.Z, c.Joint, 1);
            if (f2.feedback)
                a2.addContactJoint(c.X, c.Y, c.Z, c.Joint, 2);
        }
    }
}
