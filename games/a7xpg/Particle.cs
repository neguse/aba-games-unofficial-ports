// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Lub;

public class Particle : LuminousActor
{
    public const float GRAVITY = 0.2f;
    public Field field;
    public Rand rand;
    public Vector pos, ppos;
    public Vector vel;
    public float z, mz, pz;
    public float r, g, b;
    public float lumAlp;
    public int cnt;
    public override Actor newActor()
    {
        return new Particle();
    }

    public override void init(ActorInitializer ini)
    {
        ParticleInitializer pi = (ParticleInitializer)ini;
        field = pi.field;
        rand = pi.rand;
        pos = new Vector();
        ppos = new Vector();
        vel = new Vector();
    }

    public void set(Vector p, float d, float ofs, float speed, float r, float g, float b)
    {
        pos.x = p.x + sin(d) * ofs;
        pos.y = p.y + cos(d) * ofs;
        z = 0.5f;
        float sb = rand.nextFloat(0.5f) + 0.75f;
        vel.x = sin(d) * speed * sb;
        vel.y = cos(d) * speed * sb;
        mz = rand.nextFloat(1);
        this.r = r;
        this.g = g;
        this.b = b;
        cnt = 12 + rand.nextInt(48);
        lumAlp = 0.8f + rand.nextFloat(0.2f);
        isExist = true;
    }

    public override void move()
    {
        cnt--;
        if (cnt < 0)
        {
            isExist = false;
            return;
        }

        ppos.x = pos.x;
        ppos.y = pos.y;
        pz = z;
        pos.add(vel);
        vel.mul(0.98f);
        if (pos.x < -field.size.x || pos.x > field.size.x)
        {
            vel.x = vel.x * (-0.9f);
            pos.x = pos.x + (vel.x * 2);
        }

        if (pos.y < -field.size.y || pos.y > field.size.y)
        {
            vel.y = vel.y * (-0.9f);
            pos.y = pos.y + (vel.y * 2);
        }

        z = z + (mz);
        mz = mz - (GRAVITY);
        if (z < 0)
        {
            mz = mz * (-0.5f);
            vel.mul(0.8f);
            z = z + (mz * 2);
        }

        lumAlp = lumAlp * (0.98f);
    }

    public override void draw(float[] model, float[] tint, Gfx.Blend blend, Mesh target = null)
    {
        Mesh mesh = target; int first = mesh.vertexCount;
        tint = new float[] { r, g, b, 1 };
        mesh.Vertex(ppos.x, ppos.y, pz, tint);
        mesh.Vertex(pos.x, pos.y, z, tint);
        tint = new float[] { r, g, b, 0.7f };
        mesh.Vertex(ppos.x, ppos.y, -pz, tint);
        mesh.Vertex(pos.x, pos.y, -z, tint);

        for (int i = first; i + 1 < mesh.vertexCount; i += 2) mesh.Line(i, i + 1);
    }

    public override void drawLuminous(float[] model, float[] tint, Gfx.Blend blend, Mesh target = null)
    {
        Mesh mesh = target; int first = mesh.vertexCount;
        if (lumAlp < 0.2f)
            return;
        tint = new float[] { r, g, b, lumAlp };
        mesh.Vertex(ppos.x, ppos.y, pz, tint);
        mesh.Vertex(pos.x, pos.y, z, tint);

        for (int i = first; i + 1 < mesh.vertexCount; i += 2) mesh.Line(i, i + 1);
    }
}

public class ParticleInitializer : ActorInitializer
{
    public Field field;
    public Rand rand;
    public ParticleInitializer(Field field, Rand rand)
    {
        this.field = field;
        this.rand = rand;
    }
}
