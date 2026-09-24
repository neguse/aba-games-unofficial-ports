using static Lub;
// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;

public class ContactJoint
{
    public OdeHandle jointID;
    public int bodyIdx;
    public Vector3 pos = new Vector3(), feedbackForce = new Vector3();
}

public class OdeActor : Actor
{
    public static bool collided;
    public World world;
    public OdeHandle _bodyId;
    public OdeHandle[] geomId = McdArrays.Make<OdeHandle>(8, () => null), transformedGeomId = McdArrays.Make<OdeHandle>(8, () => null);
    public int geomNum, trGeomNum, contactJointNum;
    public ContactJoint[] contactJoint;
    public bool bodyCreated;
    public static Vector3 vvct = new Vector3(), force = new Vector3();
    public static void initFirst()
    {
    }

    public virtual void setWorld(World w)
    {
        world = w;
    }

    public virtual void init_1_Boolean(bool feedback = false)
    {
        if (feedback)
            contactJoint = McdArrays.Make(16, () => new ContactJoint());
        bodyCreated = false;
    }

    public override void init_1_(object[] args)
    {
    }

    public override void move_0()
    {
    }

    public override void draw(float[] model, float[] tint, Gfx.Blend blend, string key, Mesh target = null)
    {
    }

    public virtual void set_1(bool withBody = true)
    {
        if (withBody)
        {
            _bodyId = world.bodyCreate(this);
            bodyCreated = true;
            McdPhysics.BodyGravity(_bodyId, 0);
        }
        else
            world.storeOdeActor(null, this);
        geomNum = 0;
        trGeomNum = 0;
        clearContactJoint();
        exists = true;
    }

    public virtual void remove_0()
    {
        removeBodyAndGeom();
        removeExistence();
    }

    public virtual void removeBodyAndGeom()
    {
        if (!(exists))
            return;
        for (int i_0 = 0; i_0 < trGeomNum; i_0++)
            McdPhysics.GeomDestroy(transformedGeomId[i_0]);
        for (int i_1 = 0; i_1 < geomNum; i_1++)
            McdPhysics.GeomDestroy(geomId[i_1]);
        if (bodyCreated)
        {
            world.bodyDestroy(_bodyId);
            bodyCreated = false;
        }
    }

    public virtual void removeExistence()
    {
        exists = false;
    }

    public virtual void setMass_1(OdeMass mass)
    {
        McdPhysics.BodyMass(_bodyId, mass);
    }

    public virtual void addGeom_1(OdeHandle geom)
    {
        if (geomNum >= 8)
            return;
        if (bodyCreated)
            McdPhysics.GeomBody(geom, _bodyId);
        geomId[geomNum] = geom;
        geomNum++;
    }

    public virtual void addTransformedGeom(OdeHandle geom)
    {
        if (trGeomNum < 8)
        {
            transformedGeomId[trGeomNum] = geom;
            trGeomNum++;
        }
    }

    public virtual void addForce(float x = 0, float y = 0, float z = 0)
    {
        McdPhysics.BodyForce(_bodyId, x, y, z);
    }

    public virtual void addRelForce(float x = 0, float y = 0, float z = 0)
    {
        McdPhysics.BodyRelForce(_bodyId, x, y, z);
    }

    public virtual void addRelForceAtRelPos(float x, float y, float z, float ox, float oy, float oz)
    {
        McdPhysics.BodyForceAt(_bodyId, x, y, z, ox, oy, oz);
    }

    public virtual void addTorque(float x = 0, float y = 0, float z = 0)
    {
        McdPhysics.BodyForce(_bodyId, x, y, z);
    }

    public virtual Vector3 getForce()
    {
        var f = McdPhysics.BodyVector(_bodyId, 3);
        force.x = f[0];
        force.y = f[1];
        force.z = f[2];
        return force;
    }

    public virtual void reset()
    {
        resetLinearVel();
        resetAngularVel();
        resetForce();
        resetTorque();
    }

    public virtual float[] getLinearVel()
    {
        return McdPhysics.BodyVector(_bodyId, 1);
    }

    public virtual void setLinearVel(float[] v)
    {
        McdPhysics.BodyVelocity(_bodyId, v[0], v[1], v[2]);
    }

    public virtual float[] getAngularVel()
    {
        return McdPhysics.BodyVector(_bodyId, 2);
    }

    public virtual void setAngularVel(float[] v)
    {
        McdPhysics.BodyAngularVelocity(_bodyId, v[0], v[1], v[2]);
    }

    public virtual void resetLinearVel()
    {
        McdPhysics.BodyVelocity(_bodyId, 0, 0, 0);
    }

    public virtual void resetAngularVel()
    {
        McdPhysics.BodyAngularVelocity(_bodyId, 0, 0, 0);
    }

    public virtual void resetForce()
    {
        McdPhysics.BodySetForce(_bodyId, 0, 0, 0);
    }

    public virtual void resetTorque()
    {
        McdPhysics.BodySetTorque(_bodyId, 0, 0, 0);
    }

    public virtual void slowLinearVel(float ratio = .1f)
    {
        var v = getLinearVel();
        setLinearVel(new float[] { v[0] * (1 - ratio), v[1] * (1 - ratio), v[2] * (1 - ratio) });
    }

    public virtual void slowAngularVel(float ratio = .1f)
    {
        var v = getAngularVel();
        setAngularVel(new float[] { v[0] * (1 - ratio), v[1] * (1 - ratio), v[2] * (1 - ratio) });
    }

    public virtual void limitLinearVel(float max, float ratio = .1f)
    {
        var v = getLinearVel();
        float size = sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
        if (size > max)
        {
            float p = 1 + (max / size - 1) * ratio;
            setLinearVel(new float[] { v[0] * p, v[1] * p, v[2] * p });
        }
    }

    public virtual void limitAngularVel(float max, float ratio = .1f)
    {
        var v = getAngularVel();
        float size = sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
        if (size > max)
        {
            float p = 1 + (max / size - 1) * ratio;
            setAngularVel(new float[] { v[0] * p, v[1] * p, v[2] * p });
        }
    }

    public virtual void enableBody()
    {
        McdPhysics.BodyEnable(_bodyId);
    }

    public virtual void disableBody()
    {
        McdPhysics.BodyDisable(_bodyId);
    }

    public virtual void setDeg(float d)
    {
        McdPhysics.BodyRotation(_bodyId, new float[] { cos(d), -sin(d), 0, 0, sin(d), cos(d), 0, 0, 0, 0, 1, 0 });
    }

    public virtual float getDeg()
    {
        var r = McdPhysics.BodyVector(_bodyId, 4);
        return (atan2(-r[1], r[0]) + atan2(r[4], r[5])) / 2;
    }

    public virtual void getRot(float[] m)
    {
        var r = McdPhysics.BodyVector(_bodyId, 4);
        m[0] = r[0];
        m[1] = r[4];
        m[2] = r[8];
        m[3] = 0;
        m[4] = r[1];
        m[5] = r[5];
        m[6] = r[9];
        m[7] = 0;
        m[8] = r[2];
        m[9] = r[6];
        m[10] = r[10];
        m[11] = 0;
        m[12] = 0;
        m[13] = 0;
        m[14] = 0;
        m[15] = 1;
    }

    public virtual void setRot(float[] r)
    {
        McdPhysics.BodyRotation(_bodyId, new float[] { r[0], r[4], r[8], 0, r[1], r[5], r[9], 0, r[2], r[6], r[10], 0 });
    }

    public virtual void collide_2(OdeActor actor, Collision flags)
    {
    }

    public virtual void clearContactJoint()
    {
        contactJointNum = 0;
    }

    public virtual void addContactJoint(float x, float y, float z, OdeHandle joint, int body)
    {
        if (contactJointNum >= 16)
            return;
        var c = contactJoint[contactJointNum];
        contactJointNum++;
        c.pos.x = x;
        c.pos.y = y;
        c.pos.z = z;
        c.jointID = joint;
        c.bodyIdx = body;
    }

    public virtual void checkFeedbackForce()
    {
    }

    public virtual void getFeedbackForce()
    {
        for (int i = 0; i < contactJointNum; i++)
        {
            var c = contactJoint[i];
            var f = McdPhysics.Feedback(c.jointID, c.bodyIdx);
            c.feedbackForce.x = f[0];
            c.feedbackForce.y = f[1];
            c.feedbackForce.z = f[2];
        }
    }

    public virtual bool checkCollide()
    {
        collided = false;
        for (int i = 0; i < geomNum; i++)
        {
            McdPhysics.CollideWith(geomId[i], world.space, (OdeHandle a, OdeHandle b) =>
            {
                collided = true;
            });
            if (collided)
                break;
        }

        return collided;
    }

    public virtual void doCollide()
    {
        for (int i = 0; i < geomNum; i++)
            McdPhysics.CollideWith(geomId[i], world.space, (OdeHandle a,OdeHandle b)=>World.nearCallback(a,b));
    }

    public virtual OdeHandle bodyId()
    {
        return _bodyId;
    }
}

public class OdeActorPool<T> : ActorPool<T> where T : OdeActor
{
    public OdeActorPool(int n, object[] args, Func<T> create) : base(n, args, create)
    {
    }

    public virtual void init_1_World(World world)
    {
        foreach (T a in actor)
            a.setWorld(world);
    }

    public virtual void clearContactJoint()
    {
        foreach (T a in actor)
            if (a.exists)
                a.clearContactJoint();
    }

    public virtual void checkFeedbackForce()
    {
        foreach (T a in actor)
            if (a.exists)
                a.checkFeedbackForce();
    }

    public override void clear()
    {
        foreach (T a in actor)
            if (a.exists)
                a.remove_0();
        actorIdx = 0;
    }
}
