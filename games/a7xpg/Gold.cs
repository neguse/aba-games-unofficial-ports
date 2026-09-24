// Copyright 2003 Kenta Cho. All rights reserved.
using System;
using static GameMath;
using static Lub;

public class Gold : LuminousActor
{
    public const float SIZE = 1;
    public static Mesh[] meshes;
    public Ship ship;
    public Field field;
    public Rand rand;
    public A7xGameManager manager;
    public Vector pos;
    public int cnt;
    public const float ROLL_DEG = 1.0f;
    public override Actor newActor()
    {
        return new Gold();
    }

    public override void init(ActorInitializer ini)
    {
        GoldInitializer gi = (GoldInitializer)ini;
        ship = gi.ship;
        field = gi.field;
        rand = gi.rand;
        manager = gi.manager;
        pos = new Vector();
    }

    public void set()
    {
        for (int i = 0; i < 8; i++)
        {
            pos.x = rand.nextFloat((field.size.x - SIZE) * 2) - field.size.x + SIZE;
            pos.y = rand.nextFloat((field.size.y - SIZE) * 2) - field.size.y + SIZE;
            if (pos.dist(ship.pos) > 8)
                break;
            if (i == 7)
            {
                pos.x = 0;
                pos.y = -field.size.y / 2;
            }
        }

        cnt = 0;
        isExist = true;
    }

    public override void move()
    {
        cnt++;
        if (ship.checkHit(pos.x, pos.y - SIZE, pos.x, pos.y + SIZE) || ship.checkHit(pos.x - SIZE, pos.y, pos.x + SIZE, pos.y))
        {
            isExist = false;
            for (int i = 0; i < 16; i++)
            {
                manager.addParticle(pos, rand.nextFloat(PI * 2), 0.5f, 0, 5, 0.8f, 0);
            }

            manager.getGold();
        }
    }

    public override void draw(float[] model, float[] tint, Gfx.Blend blend, Mesh target = null)
    {
        float[] parent1 = model;
        model = Transform.Translate(model, pos.x, pos.y, 0.5f);
        model = Transform.Rotate(model, ROLL_DEG * cnt, 0, 0, 1);
        { Mesh shape2 = meshes[0]; if (shape2.count > 0) Gfx.Draw(shape2.count, shape2.Bindings(model, tint, 1, blend == Gfx.Blend.Additive, 0, null, 640, 480),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
        { Mesh shape3 = meshes[1]; if (shape3.count > 0) Gfx.Draw(shape3.count, shape3.Bindings(model, tint, 1, blend == Gfx.Blend.Additive, 0, null, 640, 480),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
        model = Transform.Translate(model, 0, 0, -0.5f);
        model = Transform.Scale(model, 1, 1, -1);
        { Mesh shape4 = meshes[0]; if (shape4.count > 0) Gfx.Draw(shape4.count, shape4.Bindings(model, tint, 1, blend == Gfx.Blend.Additive, 0, null, 640, 480),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
        model = parent1;
    }

    public override void drawLuminous(float[] model, float[] tint, Gfx.Blend blend, Mesh target = null)
    {
        float[] parent1 = model;
        model = Transform.Translate(model, pos.x, pos.y, 0.5f);
        model = Transform.Rotate(model, ROLL_DEG * cnt, 0, 0, 1);
        { Mesh shape2 = meshes[1]; if (shape2.count > 0) Gfx.Draw(shape2.count, shape2.Bindings(model, tint, 1, blend == Gfx.Blend.Additive, 0, null, 128, 128),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend }); }
        model = parent1;
    }

    public static void createMeshes()
    {
        meshes = new Mesh[2]; Mesh mesh = null; float[] model = Transform.Identity(); float[] tint = null;
        mesh = new Mesh("Gold-" + (0).ToString()); meshes[0] = mesh;
        appendGold(mesh, model, tint, 1);

        mesh = new Mesh("Gold-" + (1).ToString()); meshes[1] = mesh;
        appendGoldLine(mesh, model, tint, 1);

    }

    public static void deleteMeshes() { meshes = null; }

    public static void appendGold(Mesh mesh, float[] model, float[] tint, float alpha)
    {
        int part1 = mesh.vertexCount;
        tint = new float[] { 1, 1, 0.3f, 0.9f * alpha };
        mesh.Vertex(0, 0, 1, tint, model);
        tint = new float[] { 0.2f, 0.5f, 0.4f, 0.8f * alpha };
        mesh.Vertex(1, 0, 0, tint, model);
        mesh.Vertex(0, 1, 0, tint, model);
        mesh.Vertex(-1, 0, 0, tint, model);
        mesh.Vertex(0, -1, 0, tint, model);
        mesh.Fan(part1, mesh.vertexCount - part1);
    }

    public static void appendGoldLine(Mesh mesh, float[] model, float[] tint, float alpha)
    {
        int part1 = mesh.vertexCount;
        tint = new float[] { 1, 1, 0.3f, 0.9f * alpha };
        mesh.Vertex(1, 0, 0, tint, model);
        mesh.Vertex(0, 1, 0, tint, model);
        mesh.Vertex(-1, 0, 0, tint, model);
        mesh.Vertex(0, -1, 0, tint, model);
        mesh.Vertex(1, 0, 0, tint, model);
        mesh.LineStrip(part1, mesh.vertexCount - part1);
    }
}

public class GoldInitializer : ActorInitializer
{
    public Ship ship;
    public Field field;
    public Rand rand;
    public A7xGameManager manager;
    public GoldInitializer(Ship ship, Field field, Rand rand, A7xGameManager manager)
    {
        this.ship = ship;
        this.field = field;
        this.rand = rand;
        this.manager = manager;
    }
}
