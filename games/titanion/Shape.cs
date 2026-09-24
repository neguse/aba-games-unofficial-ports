// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public interface Shape
{
    public void draw_3(float[] model, float[] color, Gfx.Blend blend, Vector3 pos, float cd, float deg);
}

public abstract class MeshShape : Shape
{
    static int nextMesh;
    public Mesh mesh;
    public virtual void initializeShape()
    {
        mesh = new Mesh("shape-" + nextMesh.ToString()); nextMesh++;
        createMesh();
    }
    public abstract void createMesh();
    public virtual void draw_0(float[] model, float[] color, Gfx.Blend blend)
    {
        foreach (MeshRange range in mesh.ranges)
        {
            Gfx.Blend material = range.material == 0 ? blend : (Gfx.Blend)range.material;
            Gfx.Draw(range.count, mesh.Bindings(model, color, 1, material == Gfx.Blend.Additive, range.first / 3),
                new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = material });
        }
    }
    public virtual void draw_3(float[] model, float[] color, Gfx.Blend blend, Vector3 pos, float cd, float deg)
    {
        float[] parent1 = model; model = Transform.Translate(model, pos.x, pos.y, pos.z);
        model = Transform.Rotate(model, cd * 180 / PI, 0, 1, 0); model = Transform.Rotate(model, deg * 180 / PI, 0, 0, 1);
        draw_0(model, color, blend); model = parent1;
    }
    public virtual void close() { mesh = null; }
}

public class PyramidShape
{
    public static void append(Mesh mesh, float[] model, float[] color, int material)
    {
        int part1 = mesh.vertexCount; int face1 = mesh.count;
        mesh.Vertex(0, 0, 0, color, model);
        mesh.Vertex(1, 1, 1, color, model);
        mesh.Vertex(1, 1, -1, color, model);
        mesh.Vertex(-1, 1, -1, color, model);
        mesh.Vertex(-1, 1, 1, color, model);
        mesh.Vertex(1, 1, 1, color, model);
        mesh.Fan(part1, mesh.vertexCount - part1); mesh.AddRange(face1, material);
        color = new float[] { 0.1f, 0.1f, 0.1f, 0.5f };
        int part2 = mesh.vertexCount; int face2 = mesh.count;
        mesh.Vertex(0, 0, 0, color, model);
        mesh.Vertex(1, 1, 1, color, model);
        mesh.Vertex(1, 1, -1, color, model);
        mesh.Vertex(0, 0, 0, color, model);
        mesh.Vertex(-1, 1, -1, color, model);
        mesh.Vertex(-1, 1, 1, color, model);
        mesh.Vertex(0, 0, 0, color, model);
        mesh.LineStrip(part2, mesh.vertexCount - part2); mesh.AddRange(face2, material);
        int part3 = mesh.vertexCount; int face3 = mesh.count;
        mesh.Vertex(1, 1, 1, color, model);
        mesh.Vertex(-1, 1, 1, color, model);
        mesh.Vertex(1, 1, -1, color, model);
        mesh.Vertex(-1, 1, -1, color, model);
        for (int vi = part3; vi + 1 < mesh.vertexCount; vi += 2) mesh.Line(vi, vi + 1); mesh.AddRange(face3, material);
    }

    public static void appendShadow(Mesh mesh, float[] model, float[] color, int material, float r, float g, float b, bool noAlpha = false)
    {
        int part1 = mesh.vertexCount; int face1 = mesh.count;
        color = new float[] { r, g, b, 1 };
        mesh.Vertex(0, 0, 0, color, model);
        if (!((noAlpha)))
            color = new float[] { r * 0.75f, g * 0.75f, b * 0.75f, 0.33f };
        else
            color = new float[] { r * 0.75f, g * 0.75f, b * 0.75f, 0.75f };
        mesh.Vertex(1, 1, 1, color, model);
        mesh.Vertex(1, 1, -1, color, model);
        mesh.Vertex(-1, 1, -1, color, model);
        mesh.Vertex(-1, 1, 1, color, model);
        mesh.Vertex(1, 1, 1, color, model);
        mesh.Fan(part1, mesh.vertexCount - part1); mesh.AddRange(face1, material);
    }

    public static void appendPolygonShape(Mesh mesh, float[] model, float[] color, int material)
    {
        int part1 = mesh.vertexCount; int face1 = mesh.count;
        mesh.Vertex(0, 0, 0, color, model);
        mesh.Vertex(1, 1, 1, color, model);
        mesh.Vertex(1, 1, -1, color, model);
        mesh.Vertex(-1, 1, -1, color, model);
        mesh.Vertex(-1, 1, 1, color, model);
        mesh.Vertex(1, 1, 1, color, model);
        mesh.Fan(part1, mesh.vertexCount - part1); mesh.AddRange(face1, material);
    }

    public static void appendLineShape(Mesh mesh, float[] model, float[] color, int material)
    {
        int part1 = mesh.vertexCount; int face1 = mesh.count;
        mesh.Vertex(0, 0, 0, color, model);
        mesh.Vertex(1, 1, 1, color, model);
        mesh.Vertex(1, 1, -1, color, model);
        mesh.Vertex(0, 0, 0, color, model);
        mesh.Vertex(-1, 1, -1, color, model);
        mesh.Vertex(-1, 1, 1, color, model);
        mesh.Vertex(0, 0, 0, color, model);
        mesh.LineStrip(part1, mesh.vertexCount - part1); mesh.AddRange(face1, material);
        int part2 = mesh.vertexCount; int face2 = mesh.count;
        mesh.Vertex(1, 1, 1, color, model);
        mesh.Vertex(-1, 1, 1, color, model);
        mesh.Vertex(1, 1, -1, color, model);
        mesh.Vertex(-1, 1, -1, color, model);
        for (int vi = part2; vi + 1 < mesh.vertexCount; vi += 2) mesh.Line(vi, vi + 1); mesh.AddRange(face2, material);
    }
}

public class PlayerShape : MeshShape
{
    public PlayerShape()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        material = (int)Gfx.Blend.Alpha;
        float[] parent1 = model;
        model = Transform.Rotate(model, 180, 0, 0, 1);
        model = Transform.Translate(model, 0, -0.6f, 0);
        model = Transform.Scale(model, 0.4f, 1.3f, 0.4f);
        PyramidShape.appendShadow(mesh, model, color, material, 1, 0.5f, 0.5f, true);
        model = parent1;
        float[] parent2 = model;
        model = Transform.Rotate(model, 180, 0, 0, 1);
        model = Transform.Translate(model, 0.5f, -0.2f, 0);
        model = Transform.Scale(model, 0.3f, 0.9f, 0.3f);
        PyramidShape.appendShadow(mesh, model, color, material, 1, 1, 1, true);
        model = parent2;
        float[] parent3 = model;
        model = Transform.Rotate(model, 180, 0, 0, 1);
        model = Transform.Translate(model, -0.5f, -0.2f, 0);
        model = Transform.Scale(model, 0.3f, 0.9f, 0.3f);
        PyramidShape.appendShadow(mesh, model, color, material, 1, 1, 1, true);
        model = parent3;
        color = new float[] { 1, 0.5f, 0.5f, 1 };
        float[] parent4 = model;
        model = Transform.Rotate(model, 180, 0, 0, 1);
        model = Transform.Translate(model, 0, -0.6f, 0);
        model = Transform.Scale(model, 0.3f, 1.2f, 0.3f);
        PyramidShape.appendPolygonShape(mesh, model, color, material);
        model = parent4;
        color = new float[] { 1, 1, 1, 1 };
        float[] parent5 = model;
        model = Transform.Rotate(model, 180, 0, 0, 1);
        model = Transform.Translate(model, 0.5f, -0.2f, 0);
        model = Transform.Scale(model, 0.2f, 0.8f, 0.2f);
        PyramidShape.appendPolygonShape(mesh, model, color, material);
        model = parent5;
        color = new float[] { 1, 1, 1, 1 };
        float[] parent6 = model;
        model = Transform.Rotate(model, 180, 0, 0, 1);
        model = Transform.Translate(model, -0.5f, -0.2f, 0);
        model = Transform.Scale(model, 0.2f, 0.8f, 0.2f);
        PyramidShape.appendPolygonShape(mesh, model, color, material);
        model = parent6;
        material = (int)Gfx.Blend.Additive;
    }
}

public class PlayerLineShape : MeshShape
{
    public PlayerLineShape()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        material = (int)Gfx.Blend.Alpha;
        float[] parent1 = model;
        model = Transform.Rotate(model, 180, 0, 0, 1);
        model = Transform.Translate(model, 0, -0.6f, 0);
        model = Transform.Scale(model, 0.3f, 1.2f, 0.3f);
        PyramidShape.appendLineShape(mesh, model, color, material);
        model = parent1;
        float[] parent2 = model;
        model = Transform.Rotate(model, 180, 0, 0, 1);
        model = Transform.Translate(model, 0.5f, -0.2f, 0);
        model = Transform.Scale(model, 0.2f, 0.8f, 0.2f);
        PyramidShape.appendLineShape(mesh, model, color, material);
        model = parent2;
        float[] parent3 = model;
        model = Transform.Rotate(model, 180, 0, 0, 1);
        model = Transform.Translate(model, -0.5f, -0.2f, 0);
        model = Transform.Scale(model, 0.2f, 0.8f, 0.2f);
        PyramidShape.appendLineShape(mesh, model, color, material);
        model = parent3;
        material = (int)Gfx.Blend.Additive;
    }
}

public class ShotShape : MeshShape
{
    public ShotShape()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        float[] parent1 = model;
        model = Transform.Rotate(model, 180, 0, 0, 1);
        model = Transform.Translate(model, 0.5f, -0.5f, 0);
        model = Transform.Scale(model, 0.1f, 1.0f, 0.1f);
        color = new float[] { 0.4f, 0.2f, 0.8f, 1 };
        PyramidShape.appendLineShape(mesh, model, color, material);
        model = parent1;
        float[] parent2 = model;
        model = Transform.Rotate(model, 180, 0, 0, 1);
        model = Transform.Translate(model, -0.5f, -0.5f, 0);
        model = Transform.Scale(model, 0.1f, 1.0f, 0.1f);
        color = new float[] { 0.4f, 0.2f, 0.8f, 1 };
        PyramidShape.appendLineShape(mesh, model, color, material);
        model = parent2;
    }
}

public abstract class TractorBeamShape : MeshShape
{
    public virtual void appendTractorBeam(Mesh mesh, float[] model, float[] color, int material, float r, float g, float b)
    {
        color = new float[] { r, g, b, 0.5f };
        int part1 = mesh.vertexCount; int face1 = mesh.count;
        mesh.Vertex(-1, 0, -1, color, model);
        mesh.Vertex(1, 0, -1, color, model);
        mesh.Vertex(1, 0, 1, color, model);
        mesh.Vertex(-1, 0, 1, color, model);
        mesh.Quads(part1, mesh.vertexCount - part1); mesh.AddRange(face1, material);
        color = new float[] { r, g, b, 1 };
        int part2 = mesh.vertexCount; int face2 = mesh.count;
        mesh.Vertex(-1, 0, -1, color, model);
        mesh.Vertex(1, 0, -1, color, model);
        mesh.Vertex(1, 0, 1, color, model);
        mesh.Vertex(-1, 0, 1, color, model);
        mesh.LineStrip(part2, mesh.vertexCount - part2, true); mesh.AddRange(face2, material);
    }

    public virtual void appendTractorBeamLine(Mesh mesh, float[] model, float[] color, int material, float r, float g, float b)
    {
        color = new float[] { r, g, b, 1 };
        int part1 = mesh.vertexCount; int face1 = mesh.count;
        mesh.Vertex(-1, 0, -1, color, model);
        mesh.Vertex(1, 0, -1, color, model);
        mesh.Vertex(1, 0, 1, color, model);
        mesh.Vertex(-1, 0, 1, color, model);
        mesh.LineStrip(part1, mesh.vertexCount - part1, true); mesh.AddRange(face1, material);
    }
}

public class TractorBeamShapeRed : TractorBeamShape
{
    public TractorBeamShapeRed()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        appendTractorBeam(mesh, model, color, material, 0.5f, 0.2f, 0.2f);
    }
}

public class TractorBeamShapeBlue : TractorBeamShape
{
    public TractorBeamShapeBlue()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        appendTractorBeam(mesh, model, color, material, 0.2f, 0.2f, 0.5f);
    }
}

public class TractorBeamShapePurple : TractorBeamShape
{
    public TractorBeamShapePurple()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        appendTractorBeam(mesh, model, color, material, 0.5f, 0.2f, 0.5f);
    }
}

public class TractorBeamShapeDarkRed : TractorBeamShape
{
    public TractorBeamShapeDarkRed()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        appendTractorBeamLine(mesh, model, color, material, 0.4f, 0.1f, 0.1f);
    }
}

public class TractorBeamShapeDarkBlue : TractorBeamShape
{
    public TractorBeamShapeDarkBlue()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        appendTractorBeamLine(mesh, model, color, material, 0.1f, 0.1f, 0.4f);
    }
}

public class TractorBeamShapeDarkPurple : TractorBeamShape
{
    public TractorBeamShapeDarkPurple()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        appendTractorBeamLine(mesh, model, color, material, 0.4f, 0.1f, 0.4f);
    }
}

public abstract class BulletShapeBase : MeshShape
{
    public virtual void draw_4(float[] model, float[] color, Gfx.Blend blend, Vector3 pos, float cd, float deg, float rd)
    {
        float[] parent1 = model;
        model = Transform.Translate(model, pos.x, pos.y, pos.z);
        model = Transform.Rotate(model, cd * 180 / PI, 0, 1, 0);
        model = Transform.Rotate(model, deg * 180 / PI, 0, 0, 1);
        model = Transform.Rotate(model, rd, 0, 1, 0);
        draw_0(model, color, blend);
        model = parent1;
    }
}

public class BulletShape : BulletShapeBase
{
    public BulletShape()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        material = (int)Gfx.Blend.Alpha;
        color = new float[] { 0, 0, 0, 1 };
        int part1 = mesh.vertexCount; int face1 = mesh.count;
        mesh.Vertex(0, 0.5f, 0, color, model);
        mesh.Vertex(-0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(0, -0.3f, 0.4f, color, model);
        mesh.Vertex(-0.34f, -0.3f, -0.2f, color, model);
        mesh.Fan(part1, mesh.vertexCount - part1); mesh.AddRange(face1, material);
        int part2 = mesh.vertexCount; int face2 = mesh.count;
        mesh.Vertex(-0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(0, -0.3f, 0.4f, color, model);
        mesh.Fan(part2, mesh.vertexCount - part2); mesh.AddRange(face2, material);
        material = (int)Gfx.Blend.Additive;
        model = Transform.Scale(model, 1.2f, 1.2f, 1.2f);
        color = new float[] { 0.1f, 0.3f, 0.3f, 1 };
        int part3 = mesh.vertexCount; int face3 = mesh.count;
        mesh.Vertex(0, 0.5f, 0, color, model);
        mesh.Vertex(-0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(0, -0.3f, 0.4f, color, model);
        mesh.Vertex(-0.34f, -0.3f, -0.2f, color, model);
        mesh.Fan(part3, mesh.vertexCount - part3); mesh.AddRange(face3, material);
        int part4 = mesh.vertexCount; int face4 = mesh.count;
        mesh.Vertex(-0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(0, -0.3f, 0.4f, color, model);
        mesh.Fan(part4, mesh.vertexCount - part4); mesh.AddRange(face4, material);
    }
}

public class BulletLineShape : BulletShapeBase
{
    public BulletLineShape()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        model = Transform.Scale(model, 1.2f, 1.2f, 1.2f);
        int part1 = mesh.vertexCount; int face1 = mesh.count;
        mesh.Vertex(0, 0.5f, 0, color, model);
        mesh.Vertex(-0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(0, 0.5f, 0, color, model);
        mesh.Vertex(0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(0, 0.5f, 0, color, model);
        mesh.Vertex(0, -0.3f, 0.4f, color, model);
        for (int vi = part1; vi + 1 < mesh.vertexCount; vi += 2) mesh.Line(vi, vi + 1); mesh.AddRange(face1, material);
        int part2 = mesh.vertexCount; int face2 = mesh.count;
        mesh.Vertex(-0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(0, -0.3f, 0.4f, color, model);
        mesh.LineStrip(part2, mesh.vertexCount - part2, true); mesh.AddRange(face2, material);
    }
}

public class MiddleBulletShape : BulletShapeBase
{
    public MiddleBulletShape()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        material = (int)Gfx.Blend.Alpha;
        model = Transform.Scale(model, 1.1f, 1.0f, 1.1f);
        color = new float[] { 0, 0, 0, 1 };
        int part1 = mesh.vertexCount; int face1 = mesh.count;
        mesh.Vertex(-0.17f, 0.3f, -0.1f, color, model);
        mesh.Vertex(-0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(0.17f, 0.3f, -0.1f, color, model);
        mesh.Vertex(0.17f, 0.3f, -0.1f, color, model);
        mesh.Vertex(0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(0, -0.3f, 0.4f, color, model);
        mesh.Vertex(0, 0.3f, 0.2f, color, model);
        mesh.Vertex(0, 0.3f, 0.2f, color, model);
        mesh.Vertex(0, -0.3f, 0.4f, color, model);
        mesh.Vertex(-0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(-0.17f, 0.3f, -0.1f, color, model);
        mesh.Quads(part1, mesh.vertexCount - part1); mesh.AddRange(face1, material);
        int part2 = mesh.vertexCount; int face2 = mesh.count;
        mesh.Vertex(-0.17f, -0.3f, -0.1f, color, model);
        mesh.Vertex(0.17f, -0.3f, -0.1f, color, model);
        mesh.Vertex(0, -0.3f, 0.2f, color, model);
        mesh.Vertex(-0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(0, -0.3f, 0.4f, color, model);
        for (int vi = part2; vi + 2 < mesh.vertexCount; vi += 3) mesh.Triangle(vi, vi + 1, vi + 2); mesh.AddRange(face2, material);
        material = (int)Gfx.Blend.Additive;
        model = Transform.Scale(model, 1.4f, 1.3f, 1.4f);
        color = new float[] { 0.1f, 0.2f, 0.3f, 1 };
        int part3 = mesh.vertexCount; int face3 = mesh.count;
        mesh.Vertex(-0.17f, 0.3f, -0.1f, color, model);
        mesh.Vertex(-0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(0.17f, 0.3f, -0.1f, color, model);
        mesh.Vertex(0.17f, 0.3f, -0.1f, color, model);
        mesh.Vertex(0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(0, -0.3f, 0.4f, color, model);
        mesh.Vertex(0, 0.3f, 0.2f, color, model);
        mesh.Vertex(0, 0.3f, 0.2f, color, model);
        mesh.Vertex(0, -0.3f, 0.4f, color, model);
        mesh.Vertex(-0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(-0.17f, 0.3f, -0.1f, color, model);
        mesh.Quads(part3, mesh.vertexCount - part3); mesh.AddRange(face3, material);
        int part4 = mesh.vertexCount; int face4 = mesh.count;
        mesh.Vertex(-0.17f, 0.3f, -0.1f, color, model);
        mesh.Vertex(0.17f, 0.3f, -0.1f, color, model);
        mesh.Vertex(0, 0.3f, 0.2f, color, model);
        mesh.Vertex(-0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(0, -0.3f, 0.4f, color, model);
        for (int vi = part4; vi + 2 < mesh.vertexCount; vi += 3) mesh.Triangle(vi, vi + 1, vi + 2); mesh.AddRange(face4, material);
    }
}

public class MiddleBulletLineShape : BulletShapeBase
{
    public MiddleBulletLineShape()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        model = Transform.Scale(model, 1.4f, 1.3f, 1.4f);
        int part1 = mesh.vertexCount; int face1 = mesh.count;
        mesh.Vertex(-0.17f, 0.3f, -0.1f, color, model);
        mesh.Vertex(-0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(0.17f, 0.3f, -0.1f, color, model);
        mesh.Vertex(0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(0, 0.3f, 0.2f, color, model);
        mesh.Vertex(0, -0.3f, 0.4f, color, model);
        for (int vi = part1; vi + 1 < mesh.vertexCount; vi += 2) mesh.Line(vi, vi + 1); mesh.AddRange(face1, material);
        int part2 = mesh.vertexCount; int face2 = mesh.count;
        mesh.Vertex(-0.17f, 0.3f, -0.1f, color, model);
        mesh.Vertex(0.17f, 0.3f, -0.1f, color, model);
        mesh.Vertex(0, 0.3f, 0.2f, color, model);
        mesh.LineStrip(part2, mesh.vertexCount - part2, true); mesh.AddRange(face2, material);
        int part3 = mesh.vertexCount; int face3 = mesh.count;
        mesh.Vertex(-0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(0.34f, -0.3f, -0.2f, color, model);
        mesh.Vertex(0, -0.3f, 0.4f, color, model);
        mesh.LineStrip(part3, mesh.vertexCount - part3, true); mesh.AddRange(face3, material);
    }
}

public abstract class RollBulletShapeBase : BulletShapeBase
{
    public override void draw_4(float[] model, float[] color, Gfx.Blend blend, Vector3 pos, float cd, float deg, float rd)
    {
        float[] parent1 = model;
        model = Transform.Translate(model, pos.x, pos.y, pos.z);
        model = Transform.Rotate(model, cd * 180 / PI, 0, 1, 0);
        model = Transform.Rotate(model, rd, 0, 0, 1);
        draw_0(model, color, blend);
        model = parent1;
    }
}

public class CounterBulletShape : RollBulletShapeBase
{
    public CounterBulletShape()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        material = (int)Gfx.Blend.Alpha;
        color = new float[] { 0, 0, 0, 1 };
        int part1 = mesh.vertexCount; int face1 = mesh.count;
        mesh.Vertex(0, 0, 0.5f, color, model);
        mesh.Vertex(0.5f, 0, 0, color, model);
        mesh.Vertex(0, 0.5f, 0, color, model);
        mesh.Vertex(-0.5f, 0, 0, color, model);
        mesh.Vertex(0, -0.5f, 0, color, model);
        mesh.Vertex(0.5f, 0, 0, color, model);
        mesh.Fan(part1, mesh.vertexCount - part1); mesh.AddRange(face1, material);
        material = (int)Gfx.Blend.Additive;
        model = Transform.Scale(model, 1.2f, 1.2f, 1.2f);
        color = new float[] { 0.5f, 0.5f, 0.5f, 1 };
        int part2 = mesh.vertexCount; int face2 = mesh.count;
        mesh.Vertex(0, 0, 0.5f, color, model);
        mesh.Vertex(0.5f, 0, 0, color, model);
        mesh.Vertex(0, 0.5f, 0, color, model);
        mesh.Vertex(-0.5f, 0, 0, color, model);
        mesh.Vertex(0, -0.5f, 0, color, model);
        mesh.Vertex(0.5f, 0, 0, color, model);
        mesh.Fan(part2, mesh.vertexCount - part2); mesh.AddRange(face2, material);
    }
}

public class CounterBulletLineShape : RollBulletShapeBase
{
    public CounterBulletLineShape()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        model = Transform.Scale(model, 1.2f, 1.2f, 1.2f);
        int part1 = mesh.vertexCount; int face1 = mesh.count;
        mesh.Vertex(0.5f, 0, 0, color, model);
        mesh.Vertex(0, 0.5f, 0, color, model);
        mesh.Vertex(-0.5f, 0, 0, color, model);
        mesh.Vertex(0, -0.5f, 0, color, model);
        mesh.LineStrip(part1, mesh.vertexCount - part1, true); mesh.AddRange(face1, material);
        int part2 = mesh.vertexCount; int face2 = mesh.count;
        mesh.Vertex(0, 0, 0.5f, color, model);
        mesh.Vertex(0.5f, 0, 0, color, model);
        mesh.Vertex(0, 0, 0.5f, color, model);
        mesh.Vertex(0, 0.5f, 0, color, model);
        mesh.Vertex(0, 0, 0.5f, color, model);
        mesh.Vertex(-0.5f, 0, 0, color, model);
        mesh.Vertex(0, 0, 0.5f, color, model);
        mesh.Vertex(0, -0.5f, 0, color, model);
        for (int vi = part2; vi + 1 < mesh.vertexCount; vi += 2) mesh.Line(vi, vi + 1); mesh.AddRange(face2, material);
    }
}

public abstract class EnemyShape : MeshShape
{
    public virtual void draw_5(float[] model, float[] color, Gfx.Blend blend, Vector3 pos, float cd, float deg, float cnt, Vector size)
    {
        draw_6(model, color, blend, pos, cd, deg, cnt, size.x, size.y);
    }

    public virtual void draw_6(float[] model, float[] color, Gfx.Blend blend, Vector3 pos, float cd, float deg, float cnt, float sx, float sy)
    {
        float[] parent1 = model;
        model = Transform.Translate(model, pos.x, pos.y, pos.z);
        model = Transform.Rotate(model, cd * 180 / PI, 0, 1, 0);
        model = Transform.Rotate(model, deg * 180 / PI, 0, 0, 1);
        model = Transform.Scale(model, sx, sy, 1);
        model = Transform.Rotate(model, cnt * 3.0f, 0, 1, 0);
        draw_0(model, color, blend);
        model = parent1;
    }
}

public class Enemy1Shape : EnemyShape
{
    public Enemy1Shape()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        material = (int)Gfx.Blend.Alpha;
        float[] parent1 = model;
        model = Transform.Translate(model, 0, -0.6f, 0);
        model = Transform.Scale(model, 0.5f, 1.4f, 0.5f);
        PyramidShape.appendShadow(mesh, model, color, material, 0.5f, 0.5f, 0.3f);
        model = parent1;
        float[] parent2 = model;
        model = Transform.Rotate(model, 120, 0, 0, 1);
        model = Transform.Translate(model, 0.5f, -0.2f, 0);
        model = Transform.Scale(model, 0.4f, 1.0f, 0.4f);
        PyramidShape.appendShadow(mesh, model, color, material, 0.2f, 0.2f, 0.5f);
        model = parent2;
        color = new float[] { 0.2f, 0.2f, 0.5f, 1 };
        float[] parent3 = model;
        model = Transform.Rotate(model, 240, 0, 0, 1);
        model = Transform.Translate(model, -0.5f, -0.2f, 0);
        model = Transform.Scale(model, 0.4f, 1.0f, 0.4f);
        PyramidShape.appendShadow(mesh, model, color, material, 0.2f, 0.2f, 0.5f);
        model = parent3;
        color = new float[] { 1, 1, 0.6f, 1 };
        float[] parent4 = model;
        model = Transform.Translate(model, 0, -0.6f, 0);
        model = Transform.Scale(model, 0.3f, 1.2f, 0.3f);
        PyramidShape.append(mesh, model, color, material);
        model = parent4;
        color = new float[] { 0.5f, 0.5f, 1, 1 };
        float[] parent5 = model;
        model = Transform.Rotate(model, 120, 0, 0, 1);
        model = Transform.Translate(model, 0.5f, -0.2f, 0);
        model = Transform.Scale(model, 0.2f, 0.8f, 0.2f);
        PyramidShape.append(mesh, model, color, material);
        model = parent5;
        color = new float[] { 0.5f, 0.5f, 1, 1 };
        float[] parent6 = model;
        model = Transform.Rotate(model, 240, 0, 0, 1);
        model = Transform.Translate(model, -0.5f, -0.2f, 0);
        model = Transform.Scale(model, 0.2f, 0.8f, 0.2f);
        PyramidShape.append(mesh, model, color, material);
        model = parent6;
        material = (int)Gfx.Blend.Additive;
    }
}

public class Enemy1TrailShape : EnemyShape
{
    public Enemy1TrailShape()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        float[] parent1 = model;
        model = Transform.Translate(model, 0, -0.6f, 0);
        model = Transform.Scale(model, 0.3f, 1.2f, 0.3f);
        PyramidShape.appendLineShape(mesh, model, color, material);
        model = parent1;
        float[] parent2 = model;
        model = Transform.Rotate(model, 120, 0, 0, 1);
        model = Transform.Translate(model, 0.5f, -0.2f, 0);
        model = Transform.Scale(model, 0.2f, 0.8f, 0.2f);
        PyramidShape.appendLineShape(mesh, model, color, material);
        model = parent2;
        float[] parent3 = model;
        model = Transform.Rotate(model, 240, 0, 0, 1);
        model = Transform.Translate(model, -0.5f, -0.2f, 0);
        model = Transform.Scale(model, 0.2f, 0.8f, 0.2f);
        PyramidShape.appendLineShape(mesh, model, color, material);
        model = parent3;
    }
}

public class Enemy2Shape : EnemyShape
{
    public Enemy2Shape()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        material = (int)Gfx.Blend.Alpha;
        float[] parent1 = model;
        model = Transform.Translate(model, 0, -0.5f, 0);
        model = Transform.Scale(model, 0.5f, 1.2f, 0.5f);
        PyramidShape.appendShadow(mesh, model, color, material, 0.5f, 0.4f, 0.5f);
        model = parent1;
        float[] parent2 = model;
        model = Transform.Rotate(model, 60, 0, 0, 1);
        model = Transform.Translate(model, 0.6f, -0.7f, 0);
        model = Transform.Scale(model, 0.4f, 1.4f, 0.4f);
        PyramidShape.appendShadow(mesh, model, color, material, 0.9f, 0.6f, 0.5f);
        model = parent2;
        float[] parent3 = model;
        model = Transform.Rotate(model, 300, 0, 0, 1);
        model = Transform.Translate(model, -0.6f, -0.7f, 0);
        model = Transform.Scale(model, 0.4f, 1.4f, 0.4f);
        PyramidShape.appendShadow(mesh, model, color, material, 0.9f, 0.6f, 0.5f);
        model = parent3;
        color = new float[] { 1, 0.9f, 1.0f, 1 };
        float[] parent4 = model;
        model = Transform.Translate(model, 0, -0.5f, 0);
        model = Transform.Scale(model, 0.3f, 1.0f, 0.3f);
        PyramidShape.append(mesh, model, color, material);
        model = parent4;
        color = new float[] { 0.9f, 0.6f, 0.5f, 1 };
        float[] parent5 = model;
        model = Transform.Rotate(model, 60, 0, 0, 1);
        model = Transform.Translate(model, 0.6f, -0.7f, 0);
        model = Transform.Scale(model, 0.2f, 1.2f, 0.2f);
        PyramidShape.append(mesh, model, color, material);
        model = parent5;
        color = new float[] { 0.9f, 0.6f, 0.5f, 1 };
        float[] parent6 = model;
        model = Transform.Rotate(model, 300, 0, 0, 1);
        model = Transform.Translate(model, -0.6f, -0.7f, 0);
        model = Transform.Scale(model, 0.2f, 1.2f, 0.2f);
        PyramidShape.append(mesh, model, color, material);
        model = parent6;
        material = (int)Gfx.Blend.Additive;
    }
}

public class Enemy2TrailShape : EnemyShape
{
    public Enemy2TrailShape()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        float[] parent1 = model;
        model = Transform.Translate(model, 0, -0.5f, 0);
        model = Transform.Scale(model, 0.3f, 1.0f, 0.3f);
        PyramidShape.appendLineShape(mesh, model, color, material);
        model = parent1;
        float[] parent2 = model;
        model = Transform.Rotate(model, 60, 0, 0, 1);
        model = Transform.Translate(model, 0.6f, -0.7f, 0);
        model = Transform.Scale(model, 0.2f, 1.2f, 0.2f);
        PyramidShape.appendLineShape(mesh, model, color, material);
        model = parent2;
        float[] parent3 = model;
        model = Transform.Rotate(model, 300, 0, 0, 1);
        model = Transform.Translate(model, -0.6f, -0.7f, 0);
        model = Transform.Scale(model, 0.2f, 1.2f, 0.2f);
        PyramidShape.appendLineShape(mesh, model, color, material);
        model = parent3;
    }
}

public class Enemy3Shape : EnemyShape
{
    public Enemy3Shape()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        material = (int)Gfx.Blend.Alpha;
        float[] parent1 = model;
        model = Transform.Translate(model, 0, -0.4f, 0);
        model = Transform.Scale(model, 0.5f, 1.4f, 0.5f);
        PyramidShape.appendShadow(mesh, model, color, material, 0.5f, 0.5f, 0.3f);
        model = parent1;
        float[] parent2 = model;
        model = Transform.Rotate(model, 150, 0, 0, 1);
        model = Transform.Translate(model, 0.5f, 0.2f, 0);
        model = Transform.Scale(model, 0.4f, 1.0f, 0.4f);
        PyramidShape.appendShadow(mesh, model, color, material, 0.2f, 0.2f, 0.5f);
        model = parent2;
        color = new float[] { 0.2f, 0.2f, 0.5f, 1 };
        float[] parent3 = model;
        model = Transform.Rotate(model, 210, 0, 0, 1);
        model = Transform.Translate(model, -0.5f, 0.2f, 0);
        model = Transform.Scale(model, 0.4f, 1.0f, 0.4f);
        PyramidShape.appendShadow(mesh, model, color, material, 0.2f, 0.2f, 0.5f);
        model = parent3;
        color = new float[] { 1, 0.6f, 0.9f, 1 };
        float[] parent4 = model;
        model = Transform.Translate(model, 0, -0.4f, 0);
        model = Transform.Scale(model, 0.3f, 1.2f, 0.3f);
        PyramidShape.append(mesh, model, color, material);
        model = parent4;
        color = new float[] { 0.3f, 0.5f, 1, 1 };
        float[] parent5 = model;
        model = Transform.Rotate(model, 150, 0, 0, 1);
        model = Transform.Translate(model, 0.5f, 0.2f, 0);
        model = Transform.Scale(model, 0.2f, 0.8f, 0.2f);
        PyramidShape.append(mesh, model, color, material);
        model = parent5;
        color = new float[] { 0.3f, 0.5f, 1, 1 };
        float[] parent6 = model;
        model = Transform.Rotate(model, 210, 0, 0, 1);
        model = Transform.Translate(model, -0.5f, 0.2f, 0);
        model = Transform.Scale(model, 0.2f, 0.8f, 0.2f);
        PyramidShape.append(mesh, model, color, material);
        model = parent6;
        material = (int)Gfx.Blend.Additive;
    }
}

public class Enemy3TrailShape : EnemyShape
{
    public Enemy3TrailShape()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        float[] parent1 = model;
        model = Transform.Translate(model, 0, -0.4f, 0);
        model = Transform.Scale(model, 0.3f, 1.2f, 0.3f);
        PyramidShape.appendLineShape(mesh, model, color, material);
        model = parent1;
        float[] parent2 = model;
        model = Transform.Rotate(model, 150, 0, 0, 1);
        model = Transform.Translate(model, 0.5f, 0.2f, 0);
        model = Transform.Scale(model, 0.2f, 0.8f, 0.2f);
        PyramidShape.appendLineShape(mesh, model, color, material);
        model = parent2;
        float[] parent3 = model;
        model = Transform.Rotate(model, 210, 0, 0, 1);
        model = Transform.Translate(model, -0.5f, 0.2f, 0);
        model = Transform.Scale(model, 0.2f, 0.8f, 0.2f);
        PyramidShape.appendLineShape(mesh, model, color, material);
        model = parent3;
    }
}

public class TriangleParticleShape : MeshShape
{
    public TriangleParticleShape()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        int part1 = mesh.vertexCount; int face1 = mesh.count;
        mesh.Vertex(0, 0.5f, 0, color, model);
        mesh.Vertex(0.4f, -0.3f, 0, color, model);
        mesh.Vertex(-0.4f, -0.3f, 0, color, model);
        mesh.LineStrip(part1, mesh.vertexCount - part1, true); mesh.AddRange(face1, material);
    }
}

public abstract class PillarShape : MeshShape
{
    public const float TICKNESS = 4.0f;
    public const float RADIUS_RATIO = 0.3f;
    public virtual void appendPillar(Mesh mesh, float[] model, float[] color, int material, float r, float g, float b, bool outside = false)
    {
        material = (int)Gfx.Blend.Alpha;
        int part1 = mesh.vertexCount; int face1 = mesh.count;
        color = new float[] { r, g, b, 1 };
        for (int i = 0; i < 8; i++)
        {
            float d = PI * 2 * i / 8;
            mesh.Vertex(sin(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, TICKNESS, cos(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, color, model);
            d = d + (PI * 2 / 8);
            mesh.Vertex(sin(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, TICKNESS, cos(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, color, model);
            mesh.Vertex(sin(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, -TICKNESS, cos(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, color, model);
            d = d - (PI * 2 / 8);
            mesh.Vertex(sin(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, -TICKNESS, cos(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, color, model);
        }

        mesh.Quads(part1, mesh.vertexCount - part1); mesh.AddRange(face1, material);
        if (!((outside)))
        {
            color = new float[] { r, g, b, 1 };
            int part2 = mesh.vertexCount; int face2 = mesh.count;
            for (int i = 0; i < 8; i++)
            {
                float d = PI * 2 * i / 8;
                mesh.Vertex(0, TICKNESS, 0, color, model);
                mesh.Vertex(sin(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, TICKNESS, cos(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, color, model);
                d = d + (PI * 2 / 8);
                mesh.Vertex(sin(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, TICKNESS, cos(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, color, model);
                d = d - (PI * 2 / 8);
                mesh.Vertex(0, -TICKNESS, 0, color, model);
                mesh.Vertex(sin(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, -TICKNESS, cos(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, color, model);
                d = d + (PI * 2 / 8);
                mesh.Vertex(sin(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, -TICKNESS, cos(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, color, model);
            }

            for (int vi = part2; vi + 2 < mesh.vertexCount; vi += 3) mesh.Triangle(vi, vi + 1, vi + 2); mesh.AddRange(face2, material);
        }

        color = new float[] { 0.1f, 0.1f, 0.1f, 1 };
        for (int i = 0; i < 8; i++)
        {
            float d = PI * 2 * i / 8;
            int part3 = mesh.vertexCount; int face3 = mesh.count;
            mesh.Vertex(sin(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, TICKNESS, cos(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, color, model);
            d = d + (PI * 2 / 8);
            mesh.Vertex(sin(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, TICKNESS, cos(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, color, model);
            mesh.Vertex(sin(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, -TICKNESS, cos(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, color, model);
            d = d - (PI * 2 / 8);
            mesh.Vertex(sin(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, -TICKNESS, cos(d) * Field.CIRCLE_RADIUS * RADIUS_RATIO, color, model);
            mesh.LineStrip(part3, mesh.vertexCount - part3); mesh.AddRange(face3, material);
        }

        material = (int)Gfx.Blend.Additive;
    }

    public virtual void draw_2(float[] model, float[] color, Gfx.Blend blend, float y, float deg)
    {
        float[] parent1 = model;
        model = Transform.Translate(model, 0, y, 0);
        model = Transform.Rotate(model, deg * 180 / PI, 0, 1, 0);
        draw_0(model, color, blend);
        model = parent1;
    }
}

public class Pillar1Shape : PillarShape
{
    public Pillar1Shape()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        model = Transform.Scale(model, 0.6f, 1.0f, 0.6f);
        appendPillar(mesh, model, color, material, 0.5f, 0.4f, 0.4f);
    }
}

public class Pillar2Shape : PillarShape
{
    public Pillar2Shape()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        model = Transform.Scale(model, 0.8f, 1.0f, 0.8f);
        appendPillar(mesh, model, color, material, 0.6f, 0.3f, 0.3f);
    }
}

public class Pillar3Shape : PillarShape
{
    public Pillar3Shape()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        appendPillar(mesh, model, color, material, 0.5f, 0.5f, 0.4f);
    }
}

public class Pillar4Shape : PillarShape
{
    public Pillar4Shape()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        model = Transform.Scale(model, 1.1f, 1.0f, 1.1f);
        appendPillar(mesh, model, color, material, 0.5f, 0.4f, 0.5f);
    }
}

public class OutsidePillarShape : PillarShape
{
    public OutsidePillarShape()
    {
        initializeShape();
    }

    public override void createMesh()
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        model = Transform.Scale(model, 7.0f, 3.0f, 7.0f);
        appendPillar(mesh, model, color, material, 0.2f, 0.2f, 0.3f, true);
    }
}
