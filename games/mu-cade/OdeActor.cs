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

public interface OdeActor
{
    public OdeBody getOde();
    public bool getExists();
    public void setExists(bool exists);
    public void collide_2(OdeActor actor, Collision flags);
    public void clearContactJoint();
    public void checkFeedbackForce();
    public void remove_0();
}

// The body and geoms of one actor. World registration, exists and clearContactJoint go through the owner, so an actor's own clearContactJoint runs from set_1.
public class OdeBody
{
    public OdeActor owner;
    public static bool collided;
    public World world;
    public OdeHandle _bodyId;
    public OdeHandle[] geomId = McdArrays.Make<OdeHandle>(8, () => null), transformedGeomId = McdArrays.Make<OdeHandle>(8, () => null);
    public int geomNum, trGeomNum, contactJointNum;
    public ContactJoint[] contactJoint;
    public bool bodyCreated;
    public static Vector3 vvct = new Vector3(), force = new Vector3();
    public OdeBody(OdeActor owner)
    {
        this.owner = owner;
    }

    public static void initFirst()
    {
    }

    public void setWorld(World w)
    {
        world = w;
    }

    public void init_1_Boolean(bool feedback = false)
    {
        if (feedback)
            contactJoint = McdArrays.Make(16, () => new ContactJoint());
        bodyCreated = false;
    }

    public void set_1(bool withBody = true)
    {
        if (withBody)
        {
            _bodyId = world.bodyCreate(owner);
            bodyCreated = true;
            McdPhysics.BodyGravity(_bodyId, 0);
        }
        else
            world.storeOdeActor(null, owner);
        geomNum = 0;
        trGeomNum = 0;
        owner.clearContactJoint();
        owner.setExists(true);
    }

    public void remove_0()
    {
        removeBodyAndGeom();
        removeExistence();
    }

    public void removeBodyAndGeom()
    {
        if (!(owner.getExists()))
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

    public void removeExistence()
    {
        owner.setExists(false);
    }

    public void setMass_1(OdeMass mass)
    {
        McdPhysics.BodyMass(_bodyId, mass);
    }

    public void addGeom_1(OdeHandle geom)
    {
        if (geomNum >= 8)
            return;
        if (bodyCreated)
            McdPhysics.GeomBody(geom, _bodyId);
        geomId[geomNum] = geom;
        geomNum++;
    }

    public void addTransformedGeom(OdeHandle geom)
    {
        if (trGeomNum < 8)
        {
            transformedGeomId[trGeomNum] = geom;
            trGeomNum++;
        }
    }

    public void addForce(float x = 0, float y = 0, float z = 0)
    {
        McdPhysics.BodyForce(_bodyId, x, y, z);
    }

    public void addRelForce(float x = 0, float y = 0, float z = 0)
    {
        McdPhysics.BodyRelForce(_bodyId, x, y, z);
    }

    public void addRelForceAtRelPos(float x, float y, float z, float ox, float oy, float oz)
    {
        McdPhysics.BodyForceAt(_bodyId, x, y, z, ox, oy, oz);
    }

    public void addTorque(float x = 0, float y = 0, float z = 0)
    {
        McdPhysics.BodyForce(_bodyId, x, y, z);
    }

    public Vector3 getForce()
    {
        var f = McdPhysics.BodyVector(_bodyId, 3);
        force.x = f[0];
        force.y = f[1];
        force.z = f[2];
        return force;
    }

    public void reset()
    {
        resetLinearVel();
        resetAngularVel();
        resetForce();
        resetTorque();
    }

    public float[] getLinearVel()
    {
        return McdPhysics.BodyVector(_bodyId, 1);
    }

    public void setLinearVel(float[] v)
    {
        McdPhysics.BodyVelocity(_bodyId, v[0], v[1], v[2]);
    }

    public float[] getAngularVel()
    {
        return McdPhysics.BodyVector(_bodyId, 2);
    }

    public void setAngularVel(float[] v)
    {
        McdPhysics.BodyAngularVelocity(_bodyId, v[0], v[1], v[2]);
    }

    public void resetLinearVel()
    {
        McdPhysics.BodyVelocity(_bodyId, 0, 0, 0);
    }

    public void resetAngularVel()
    {
        McdPhysics.BodyAngularVelocity(_bodyId, 0, 0, 0);
    }

    public void resetForce()
    {
        McdPhysics.BodySetForce(_bodyId, 0, 0, 0);
    }

    public void resetTorque()
    {
        McdPhysics.BodySetTorque(_bodyId, 0, 0, 0);
    }

    public void slowLinearVel(float ratio = .1f)
    {
        var v = getLinearVel();
        setLinearVel(new float[] { v[0] * (1 - ratio), v[1] * (1 - ratio), v[2] * (1 - ratio) });
    }

    public void slowAngularVel(float ratio = .1f)
    {
        var v = getAngularVel();
        setAngularVel(new float[] { v[0] * (1 - ratio), v[1] * (1 - ratio), v[2] * (1 - ratio) });
    }

    public void limitLinearVel(float max, float ratio = .1f)
    {
        var v = getLinearVel();
        float size = sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
        if (size > max)
        {
            float p = 1 + (max / size - 1) * ratio;
            setLinearVel(new float[] { v[0] * p, v[1] * p, v[2] * p });
        }
    }

    public void limitAngularVel(float max, float ratio = .1f)
    {
        var v = getAngularVel();
        float size = sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
        if (size > max)
        {
            float p = 1 + (max / size - 1) * ratio;
            setAngularVel(new float[] { v[0] * p, v[1] * p, v[2] * p });
        }
    }

    public void enableBody()
    {
        McdPhysics.BodyEnable(_bodyId);
    }

    public void disableBody()
    {
        McdPhysics.BodyDisable(_bodyId);
    }

    public void setDeg(float d)
    {
        McdPhysics.BodyRotation(_bodyId, new float[] { cos(d), -sin(d), 0, 0, sin(d), cos(d), 0, 0, 0, 0, 1, 0 });
    }

    public float getDeg()
    {
        var r = McdPhysics.BodyVector(_bodyId, 4);
        return (atan2(-r[1], r[0]) + atan2(r[4], r[5])) / 2;
    }

    public void getRot(float[] m)
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

    public void setRot(float[] r)
    {
        McdPhysics.BodyRotation(_bodyId, new float[] { r[0], r[4], r[8], 0, r[1], r[5], r[9], 0, r[2], r[6], r[10], 0 });
    }

    public void clearContactJoint()
    {
        contactJointNum = 0;
    }

    public void addContactJoint(float x, float y, float z, OdeHandle joint, int body)
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

    public void getFeedbackForce()
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

    public bool checkCollide()
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

    public void doCollide()
    {
        for (int i = 0; i < geomNum; i++)
            McdPhysics.CollideWith(geomId[i], world.space, (OdeHandle a,OdeHandle b)=>World.nearCallback(a,b));
    }

    public OdeHandle bodyId()
    {
        return _bodyId;
    }
}

public class OdeActorPool<T> where T : Actor, OdeActor
{
    public ActorPool<T> pool;
    public T[] actor;
    public OdeActorPool(int n, object[] args, Func<T> create)
    {
        pool = new ActorPool<T>(n, args, create);
        actor = pool.actor;
    }

    public T getInstance()
    {
        return pool.getInstance();
    }

    public T[] getMultipleInstances(int n)
    {
        return pool.getMultipleInstances(n);
    }

    public void move_0()
    {
        pool.move_0();
    }

    public void draw(float[] model, float[] tint, Gfx.Blend blend, string key, Mesh target = null)
    {
        pool.draw(model, tint, blend, key, target);
    }

    public void init_1_World(World world)
    {
        foreach (OdeActor a in actor)
            a.getOde().setWorld(world);
    }

    public void clearContactJoint()
    {
        foreach (OdeActor a in actor)
            if (a.getExists())
                a.clearContactJoint();
    }

    public void checkFeedbackForce()
    {
        foreach (OdeActor a in actor)
            if (a.getExists())
                a.checkFeedbackForce();
    }

    public void clear()
    {
        foreach (OdeActor a in actor)
            if (a.getExists())
                a.remove_0();
        pool.actorIdx = 0;
    }
}
