// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using Lub;

public interface Shape
{
    public void draw_3(float[] model, float[] color, Gfx.Blend blend, Vector3 pos, float cd, float deg);
}

public interface MeshBuilder
{
    public void createMesh(Mesh mesh);
}

public class MeshShape : Shape
{
    static int nextMesh;
    public Mesh mesh;
    public MeshShape(MeshBuilder builder)
    {
        mesh = new Mesh("shape-" + nextMesh.ToString()); nextMesh++;
        builder.createMesh(mesh);
    }
    public void draw_0(float[] model, float[] color, Gfx.Blend blend)
    {
        foreach (MeshRange range in mesh.ranges)
        {
            Gfx.Blend material = range.material == 0 ? blend : (Gfx.Blend)range.material;
            Gfx.Draw(range.count, mesh.Bindings(model, color, 1, material == Gfx.Blend.Additive, range.first / 3),
                new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = material });
        }
    }
    public void draw_3(float[] model, float[] color, Gfx.Blend blend, Vector3 pos, float cd, float deg)
    {
        float[] parent1 = model; model = Transform.Translate(model, pos.x, pos.y, pos.z);
        model = Transform.Rotate(model, cd * 180 / PI, 0, 1, 0); model = Transform.Rotate(model, deg * 180 / PI, 0, 0, 1);
        draw_0(model, color, blend); model = parent1;
    }
    public void close() { mesh = null; }
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

public class PlayerShape : MeshBuilder
{
    public void createMesh(Mesh mesh)
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

public class PlayerLineShape : MeshBuilder
{
    public void createMesh(Mesh mesh)
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

public class ShotShape : MeshBuilder
{
    public void createMesh(Mesh mesh)
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

public static class TractorBeamShape
{
    public static void appendTractorBeam(Mesh mesh, float[] model, float[] color, int material, float r, float g, float b)
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

    public static void appendTractorBeamLine(Mesh mesh, float[] model, float[] color, int material, float r, float g, float b)
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

public class TractorBeamShapeRed : MeshBuilder
{
    public void createMesh(Mesh mesh)
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        TractorBeamShape.appendTractorBeam(mesh, model, color, material, 0.5f, 0.2f, 0.2f);
    }
}

public class TractorBeamShapeBlue : MeshBuilder
{
    public void createMesh(Mesh mesh)
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        TractorBeamShape.appendTractorBeam(mesh, model, color, material, 0.2f, 0.2f, 0.5f);
    }
}

public class TractorBeamShapePurple : MeshBuilder
{
    public void createMesh(Mesh mesh)
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        TractorBeamShape.appendTractorBeam(mesh, model, color, material, 0.5f, 0.2f, 0.5f);
    }
}

public class TractorBeamShapeDarkRed : MeshBuilder
{
    public void createMesh(Mesh mesh)
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        TractorBeamShape.appendTractorBeamLine(mesh, model, color, material, 0.4f, 0.1f, 0.1f);
    }
}

public class TractorBeamShapeDarkBlue : MeshBuilder
{
    public void createMesh(Mesh mesh)
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        TractorBeamShape.appendTractorBeamLine(mesh, model, color, material, 0.1f, 0.1f, 0.4f);
    }
}

public class TractorBeamShapeDarkPurple : MeshBuilder
{
    public void createMesh(Mesh mesh)
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        TractorBeamShape.appendTractorBeamLine(mesh, model, color, material, 0.4f, 0.1f, 0.4f);
    }
}

public class BulletShapeBase : Shape
{
    public MeshShape meshShape;
    public bool roll;
    public BulletShapeBase(MeshBuilder builder, bool roll)
    {
        meshShape = new MeshShape(builder);
        this.roll = roll;
    }

    public void draw_3(float[] model, float[] color, Gfx.Blend blend, Vector3 pos, float cd, float deg)
    {
        meshShape.draw_3(model, color, blend, pos, cd, deg);
    }

    public void draw_4(float[] model, float[] color, Gfx.Blend blend, Vector3 pos, float cd, float deg, float rd)
    {
        if (roll)
        {
            float[] parent1 = model;
            model = Transform.Translate(model, pos.x, pos.y, pos.z);
            model = Transform.Rotate(model, cd * 180 / PI, 0, 1, 0);
            model = Transform.Rotate(model, rd, 0, 0, 1);
            meshShape.draw_0(model, color, blend);
            model = parent1;
        }
        else
        {
            float[] parent1 = model;
            model = Transform.Translate(model, pos.x, pos.y, pos.z);
            model = Transform.Rotate(model, cd * 180 / PI, 0, 1, 0);
            model = Transform.Rotate(model, deg * 180 / PI, 0, 0, 1);
            model = Transform.Rotate(model, rd, 0, 1, 0);
            meshShape.draw_0(model, color, blend);
            model = parent1;
        }
    }

    public void close()
    {
        meshShape.close();
    }
}

public class BulletShape : MeshBuilder
{
    public void createMesh(Mesh mesh)
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

public class BulletLineShape : MeshBuilder
{
    public void createMesh(Mesh mesh)
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

public class MiddleBulletShape : MeshBuilder
{
    public void createMesh(Mesh mesh)
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

public class MiddleBulletLineShape : MeshBuilder
{
    public void createMesh(Mesh mesh)
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

public class CounterBulletShape : MeshBuilder
{
    public void createMesh(Mesh mesh)
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

public class CounterBulletLineShape : MeshBuilder
{
    public void createMesh(Mesh mesh)
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

public class EnemyShape : Shape
{
    public MeshShape meshShape;
    public EnemyShape(MeshBuilder builder)
    {
        meshShape = new MeshShape(builder);
    }

    public void draw_3(float[] model, float[] color, Gfx.Blend blend, Vector3 pos, float cd, float deg)
    {
        meshShape.draw_3(model, color, blend, pos, cd, deg);
    }

    public void draw_5(float[] model, float[] color, Gfx.Blend blend, Vector3 pos, float cd, float deg, float cnt, Vector size)
    {
        draw_6(model, color, blend, pos, cd, deg, cnt, size.x, size.y);
    }

    public void draw_6(float[] model, float[] color, Gfx.Blend blend, Vector3 pos, float cd, float deg, float cnt, float sx, float sy)
    {
        float[] parent1 = model;
        model = Transform.Translate(model, pos.x, pos.y, pos.z);
        model = Transform.Rotate(model, cd * 180 / PI, 0, 1, 0);
        model = Transform.Rotate(model, deg * 180 / PI, 0, 0, 1);
        model = Transform.Scale(model, sx, sy, 1);
        model = Transform.Rotate(model, cnt * 3.0f, 0, 1, 0);
        meshShape.draw_0(model, color, blend);
        model = parent1;
    }

    public void close()
    {
        meshShape.close();
    }
}

public class Enemy1Shape : MeshBuilder
{
    public void createMesh(Mesh mesh)
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

public class Enemy1TrailShape : MeshBuilder
{
    public void createMesh(Mesh mesh)
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

public class Enemy2Shape : MeshBuilder
{
    public void createMesh(Mesh mesh)
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

public class Enemy2TrailShape : MeshBuilder
{
    public void createMesh(Mesh mesh)
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

public class Enemy3Shape : MeshBuilder
{
    public void createMesh(Mesh mesh)
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

public class Enemy3TrailShape : MeshBuilder
{
    public void createMesh(Mesh mesh)
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

public class TriangleParticleShape : MeshBuilder
{
    public void createMesh(Mesh mesh)
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        int part1 = mesh.vertexCount; int face1 = mesh.count;
        mesh.Vertex(0, 0.5f, 0, color, model);
        mesh.Vertex(0.4f, -0.3f, 0, color, model);
        mesh.Vertex(-0.4f, -0.3f, 0, color, model);
        mesh.LineStrip(part1, mesh.vertexCount - part1, true); mesh.AddRange(face1, material);
    }
}

public class PillarShape
{
    public const float TICKNESS = 4.0f;
    public const float RADIUS_RATIO = 0.3f;
    public MeshShape meshShape;
    public PillarShape(MeshBuilder builder)
    {
        meshShape = new MeshShape(builder);
    }

    public static void appendPillar(Mesh mesh, float[] model, float[] color, int material, float r, float g, float b, bool outside = false)
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

    public void draw_2(float[] model, float[] color, Gfx.Blend blend, float y, float deg)
    {
        float[] parent1 = model;
        model = Transform.Translate(model, 0, y, 0);
        model = Transform.Rotate(model, deg * 180 / PI, 0, 1, 0);
        meshShape.draw_0(model, color, blend);
        model = parent1;
    }

    public void close()
    {
        meshShape.close();
    }
}

public class Pillar1Shape : MeshBuilder
{
    public void createMesh(Mesh mesh)
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        model = Transform.Scale(model, 0.6f, 1.0f, 0.6f);
        PillarShape.appendPillar(mesh, model, color, material, 0.5f, 0.4f, 0.4f);
    }
}

public class Pillar2Shape : MeshBuilder
{
    public void createMesh(Mesh mesh)
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        model = Transform.Scale(model, 0.8f, 1.0f, 0.8f);
        PillarShape.appendPillar(mesh, model, color, material, 0.6f, 0.3f, 0.3f);
    }
}

public class Pillar3Shape : MeshBuilder
{
    public void createMesh(Mesh mesh)
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        PillarShape.appendPillar(mesh, model, color, material, 0.5f, 0.5f, 0.4f);
    }
}

public class Pillar4Shape : MeshBuilder
{
    public void createMesh(Mesh mesh)
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        model = Transform.Scale(model, 1.1f, 1.0f, 1.1f);
        PillarShape.appendPillar(mesh, model, color, material, 0.5f, 0.4f, 0.5f);
    }
}

public class OutsidePillarShape : MeshBuilder
{
    public void createMesh(Mesh mesh)
    {
        float[] model = Transform.Identity(); float[] color = null; int material = 0;
        model = Transform.Scale(model, 7.0f, 3.0f, 7.0f);
        PillarShape.appendPillar(mesh, model, color, material, 0.2f, 0.2f, 0.3f, true);
    }
}
