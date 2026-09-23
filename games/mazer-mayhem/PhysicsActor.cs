// Copyright 2008 Kenta Cho. Some rights reserved.
using System;

public class Engine
{
    public static float Damping = 0.99f;
    public static float DeltaTime = 0.01f;
}

public class CircleParticle
{
    public Vector3 Pos = new Vector3();
    public Vector3 PrevPos = new Vector3();
    public float Elasticity;
    public float Friction;
    public bool IsFixed;
    public Quaternion Rotation = new Quaternion();
    public Quaternion AngleRate = new Quaternion();
    public float Radius;
    private float storedInvMass;
    private Vector3 forces = new Vector3();
    public void Clear()
    {
        {
            Pos.Z = 0;
            Pos.Y = Pos.Z;
            Pos.X = Pos.Y;
        }

        {
            PrevPos.Z = 0;
            PrevPos.Y = PrevPos.Z;
            PrevPos.X = PrevPos.Y;
        }

        Elasticity = 0.75f;
        Friction = 0.01f;
        IsFixed = false;
        Rotation = (Quaternion.Identity).Copy();
        AngleRate = (Quaternion.Identity).Copy();
        Radius = 1.0f;
        storedInvMass = 1.0f;
        {
            forces.Z = 0;
            forces.Y = forces.Z;
            forces.X = forces.Y;
        }
    }

    public void Set(Vector3 p)
    {
        {
            PrevPos = (p).Copy();
            Pos = (PrevPos).Copy();
        }

        {
            forces.Z = 0;
            forces.Y = forces.Z;
            forces.X = forces.Y;
        }
    }

    public void Update()
    {
        if (IsFixed)
            return;
        Vector3 temp = (Pos).Copy();
        forces *= Engine.DeltaTime;
        Vector3 nv = (Velocity + forces).Copy();
        nv *= Engine.Damping;
        Pos += nv;
        PrevPos = (temp).Copy();
        {
            forces.Z = 0;
            forces.Y = forces.Z;
            forces.X = forces.Y;
        }

        Rotation *= AngleRate;
        AngleRate = (Quaternion.Lerp((AngleRate).Copy(), (Quaternion.Identity).Copy(), 0.01f)).Copy();
    }

    public void AddForce(Vector3 f)
    {
        forces += f * storedInvMass;
    }

    public void AddMasslessForce(Vector3 f)
    {
        forces += f;
    }

    public void CheckCollision(CircleParticle state)
    {
        float bd = Radius + state.Radius;
        if (Math.Abs(Pos.X - state.Pos.X) > bd || Math.Abs(Pos.Y - state.Pos.Y) > bd || Math.Abs(Pos.Z - state.Pos.Z) > bd)
            return;
        Vector3 collisionNormal = (Pos - state.Pos).Copy();
        float mag = collisionNormal.Length();
        float collisionDepth = (Radius + state.Radius) - mag;
        if (collisionDepth > 0)
        {
            collisionNormal /= mag;
            Resolve(state, (collisionNormal).Copy(), collisionDepth);
        }
    }

    private void Resolve(CircleParticle state, Vector3 normal, float depth)
    {
        if (!IsFixed)
            ResolveAngleRate((state.Velocity).Copy(), (normal).Copy());
        if (!state.IsFixed)
            state.ResolveAngleRate((Velocity).Copy(), (-normal).Copy());
        if (IsFixed && state.IsFixed)
            return;
        Vector3 mtd = (normal * depth).Copy();
        float te = Elasticity + state.Elasticity;
        float sumInvMass = InvMass + state.InvMass;
        float tf = Clamp(1 - (Friction + state.Friction), 0, 1);
        Collision ca = (GetComponents((normal).Copy())).Copy();
        Collision cb = (state.GetComponents((normal).Copy())).Copy();
        Vector3 vnA = ((cb.Vn * ((te + 1) * InvMass)) + (ca.Vn * (state.InvMass - te * InvMass))).Copy();
        vnA /= sumInvMass;
        Vector3 vnB = ((ca.Vn * ((te + 1) * state.InvMass)) + (cb.Vn * (InvMass - te * state.InvMass))).Copy();
        vnB /= sumInvMass;
        ca.Vt *= tf;
        cb.Vt *= tf;
        Vector3 mtdA = (mtd * (InvMass / sumInvMass)).Copy();
        Vector3 mtdB = (mtd * (-state.InvMass / sumInvMass)).Copy();
        vnA += ca.Vt;
        vnB += cb.Vt;
        if (!IsFixed)
            ResolveCollision((mtdA).Copy(), (vnA).Copy());
        if (!state.IsFixed)
            state.ResolveCollision((mtdB).Copy(), (vnB).Copy());
    }

    public void ResolveAngleRate(Vector3 vel, Vector3 normal)
    {
        AngleRate = (Quaternion.Lerp((AngleRate).Copy(), (Quaternion.CreateFromAxisAngle((vel).Copy(), Vector3.Dot((vel).Copy(), (normal).Copy()))).Copy(), 0.5f)).Copy();
    }

    private float Clamp(float input, float min, float max)
    {
        if (input > max)
            return max;
        if (input < min)
            return min;
        return input;
    }

    private Collision GetComponents(Vector3 collisionNormal)
    {
        Vector3 vel = (Velocity).Copy();
        float vdotn = Vector3.Dot((collisionNormal).Copy(), (vel).Copy());
        Collision c = new Collision();
        c.Vn = (collisionNormal * vdotn).Copy();
        c.Vt = (vel - c.Vn).Copy();
        return (c).Copy();
    }

    private void ResolveCollision(Vector3 mtd, Vector3 vel)
    {
        Pos += mtd;
        Velocity = (vel).Copy();
    }

    public void MulVelocityZ(float v)
    {
        PrevPos.Z = Pos.Z - (Pos.Z - PrevPos.Z) * v;
    }

    public Vector3 Velocity
    {
        get
        {
            return (Pos - PrevPos).Copy();
        }

        set
        {
            PrevPos = (Pos - value).Copy();
        }
    }

    public float Mass
    {
        get
        {
            return 1 / InvMass;
        }

        set
        {
            storedInvMass = 1 / value;
        }
    }

    public float InvMass
    {
        get
        {
            return (IsFixed) ? 0 : storedInvMass;
        }
    }

    public CircleParticle Copy()
    {
        return new CircleParticle
        {
            Pos = Pos.Copy(),
            PrevPos = PrevPos.Copy(),
            Elasticity = Elasticity,
            Friction = Friction,
            IsFixed = IsFixed,
            Rotation = Rotation.Copy(),
            AngleRate = AngleRate.Copy(),
            Radius = Radius,
            storedInvMass = storedInvMass,
            forces = forces.Copy()
        };
    }
}

public class Collision
{
    public Vector3 Vn = new Vector3();
    public Vector3 Vt = new Vector3();
    public Collision Copy()
    {
        return new Collision
        {
            Vn = Vn.Copy(),
            Vt = Vt.Copy()
        };
    }
}

public class SpringConstraint
{
    public static void Resolve(Vector3 p1, Vector3 p2, float invMass1, float invMass2, float stiffness, float restLength)
    {
        float deltaLength = Vector3.Distance((p1).Copy(), (p2).Copy());
        float diff = (deltaLength - restLength) / (deltaLength * (invMass1 + invMass2) + 0.0001f);
        Vector3 dmds = ((p1 - p2) * (diff * stiffness)).Copy();
        p1.Set(p1 - (dmds * invMass1));
        p2.Set(p2 + dmds * invMass2);
    }
}
