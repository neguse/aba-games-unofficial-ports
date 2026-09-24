// Copyright 2002-2003 Kenta Cho. All rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;
using static RrConstants;
using static RrArrays;
using static RrRandom;
using static RrBarrage;
using static RrSound;
using static RrInput;
using static RrPreference;
using static RrCore;
using static RrAttract;
using static RrShip;
using static RrLaser;
using static RrShot;
using static RrFrag;
using static RrBackground;
using static RrBoss;
using static RrFoe;
using static RrScreen;
using static RrLetter;
using static RrAngles;
using static RrVector;

public static class RrScreen
{
    static Dictionary<string, Mesh> shapes = new Dictionary<string, Mesh>();
    static Mesh bulletPoint, boxFill, boxLine, fragmentLine;
    static Mesh[] sprites = new Mesh[3];
    public static float[] clearColor = new float[] { 0, 0, 0, 1 };
    public static float zoom = 15;
    public static int screenShakeCnt = 0;
    public static int screenShakeType = 0;
    public static float[] setEyepos()
    {
        float x = 0, y = 0;

        if (screenShakeCnt > 0)
        {
            switch (screenShakeType)
            {
                case 0:
                    x = (float)randNS2(256) / 5000.0f;
                    y = (float)randNS2(256) / 5000.0f;
                    break;
                default:
                    x = (float)randNS2(256) * screenShakeCnt / 21000.0f;
                    y = (float)randNS2(256) * screenShakeCnt / 21000.0f;
                    break;
            }

            return Transform.LookAt(Transform.Perspective(720, 1.8106602f), 0, 0, zoom, x, y, 0, 0.0f, 1.0f, 0.0f);
        }
        else
        {
            return Transform.LookAt(Transform.Perspective(720, 1.8106602f), 0, 0, zoom, 0, 0, 0, 0.0f, 1.0f, 0.0f);
        }
    }

    public static void setScreenShake(int type, int cnt)
    {
        screenShakeType = type;
        screenShakeCnt = cnt;
    }

    public static void moveScreenShake()
    {
        if (screenShakeCnt > 0)
        {
            screenShakeCnt--;
        }
    }





    public static void drawBox(float[] model, Gfx.Blend blend, string key, float x, float y, float width, float height, int r, int g, int b)
    {
        if (boxFill == null) {
            boxFill = new Mesh("box-fill"); boxLine = new Mesh("box-line");
            boxFill.Vertex(-1, -1, 0, null); boxFill.Vertex(1, -1, 0, null);
            boxFill.Vertex(1, 1, 0, null); boxFill.Vertex(-1, 1, 0, null); boxFill.Fan(0, 4);
            boxLine.Vertex(-1, -1, 0, null); boxLine.Vertex(1, -1, 0, null);
            boxLine.Vertex(1, 1, 0, null); boxLine.Vertex(-1, 1, 0, null); boxLine.LineStrip(0, 4, true);
        }
        model = Transform.Scale(Transform.Translate(model, x, y, 0), width, height, 1);
        float[] tint = new float[] { (r & 255) / 255f, (g & 255) / 255f, (b & 255) / 255f, blend == Gfx.Blend.None ? 1 : 128f / 255 };
        Gfx.Draw(boxFill.count, boxFill.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
        tint[3] = 1;
        Gfx.Draw(boxLine.count, boxLine.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }

    public static void drawLine(float[] model, Gfx.Blend blend, string key, float x1, float y1, float z1, float x2, float y2, float z2, int r, int g, int b, int a)
    {
        float[] tint = null;
        tint = new float[] { (r & 255) / 255f, (g & 255) / 255f, (b & 255) / 255f, blend == Gfx.Blend.None ? 1 : (a & 255) / 255f };
        var part1 = new Mesh(key + "-1");
        part1.Vertex(x1, y1, z1, tint);
        part1.Vertex(x2, y2, z2, tint);
        for (int vi = 0; vi + 1 < part1.vertexCount; vi += 2) part1.Line(vi, vi + 1);
        if (part1.count > 0) Gfx.Draw(part1.count, part1.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }

    public static void drawLinePart(float[] model, Gfx.Blend blend, string key, float x1, float y1, float z1, float x2, float y2, float z2, int r, int g, int b, int a, int len)
    {
        float[] tint = null;
        tint = new float[] { (r & 255) / 255f, (g & 255) / 255f, (b & 255) / 255f, blend == Gfx.Blend.None ? 1 : (a & 255) / 255f };
        var part1 = new Mesh(key + "-1");
        part1.Vertex(x1, y1, z1, tint);
        part1.Vertex(x1 + (x2 - x1) * len / 256, y1 + (y2 - y1) * len / 256, z1 + (z2 - z1) * len / 256, tint);
        for (int vi = 0; vi + 1 < part1.vertexCount; vi += 2) part1.Line(vi, vi + 1);
        if (part1.count > 0) Gfx.Draw(part1.count, part1.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }

    public static void drawRollLineAbs(float[] model, Gfx.Blend blend, string key, float x1, float y1, float z1, float x2, float y2, float z2, int r, int g, int b, int a, int d1)
    {
        float[] tint = null;
        float[] parent1 = model;
        model = Transform.Rotate(model, (float)d1 * 360 / 1024, 0, 0, 1);
        tint = new float[] { (r & 255) / 255f, (g & 255) / 255f, (b & 255) / 255f, blend == Gfx.Blend.None ? 1 : (a & 255) / 255f };
        var part2 = new Mesh(key + "-2");
        part2.Vertex(x1, y1, z1, tint);
        part2.Vertex(x2, y2, z2, tint);
        for (int vi = 0; vi + 1 < part2.vertexCount; vi += 2) part2.Line(vi, vi + 1);
        if (part2.count > 0) Gfx.Draw(part2.count, part2.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
        model = parent1;
    }

    public static void drawRollLine(float[] model, Gfx.Blend blend, string key, float x, float y, float z, float width, int r, int g, int b, int a, int d1, int d2)
    {
        float[] tint = null;
        float[] parent1 = model;
        model = Transform.Translate(model, x, y, z);
        model = Transform.Rotate(model, (float)d1 * 360 / 1024, 0, 0, 1);
        model = Transform.Rotate(model, (float)d2 * 360 / 1024, 1, 0, 0);
        tint = new float[] { (r & 255) / 255f, (g & 255) / 255f, (b & 255) / 255f, blend == Gfx.Blend.None ? 1 : (a & 255) / 255f };
        if (fragmentLine == null) {
            fragmentLine = new Mesh("fragment-line");
            fragmentLine.Vertex(0, -1, 0, null); fragmentLine.Vertex(0, 1, 0, null); fragmentLine.Line(0, 1);
        }
        model = Transform.Scale(model, 1, width, 1);
        if (fragmentLine.count > 0) Gfx.Draw(fragmentLine.count, fragmentLine.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
        model = parent1;
    }

    public static void drawSquare(float[] model, Gfx.Blend blend, string key, float x1, float y1, float z1, float x2, float y2, float z2, float x3, float y3, float z3, float x4, float y4, float z4, int r, int g, int b)
    {
        float[] tint = null;
        tint = new float[] { (r & 255) / 255f, (g & 255) / 255f, (b & 255) / 255f, blend == Gfx.Blend.None ? 1 : (64 & 255) / 255f };
        var part1 = new Mesh(key + "-1");
        part1.Vertex(x1, y1, z1, tint);
        part1.Vertex(x2, y2, z2, tint);
        part1.Vertex(x3, y3, z3, tint);
        part1.Vertex(x4, y4, z4, tint);
        part1.Fan(0, part1.vertexCount - 0);
        if (part1.count > 0) Gfx.Draw(part1.count, part1.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }

    public static void drawStar(float[] model, Gfx.Blend blend, string key, int f, float x, float y, float z, int r, int g, int b, float size)
    {
        float[] tint = null;
        int texture = f != 0 ? 0 : 1;
        if (sprites[texture] == null) {
            Mesh sprite = new Mesh("atlas-" + texture.ToString());
            sprite.Vertex(-1, -1, 0, new float[] { (1 + 0 * RrData.textureWidth[texture]) / (float)RrData.atlasWidth, (RrData.textureY[texture] + 1 + 1 * RrData.textureHeight[texture]) / (float)RrData.atlasHeight, 0, 1 });
            sprite.Vertex(1, -1, 0, new float[] { (1 + 1 * RrData.textureWidth[texture]) / (float)RrData.atlasWidth, (RrData.textureY[texture] + 1 + 1 * RrData.textureHeight[texture]) / (float)RrData.atlasHeight, 0, 1 });
            sprite.Vertex(1, 1, 0, new float[] { (1 + 1 * RrData.textureWidth[texture]) / (float)RrData.atlasWidth, (RrData.textureY[texture] + 1 + 0 * RrData.textureHeight[texture]) / (float)RrData.atlasHeight, 0, 1 });
            sprite.Vertex(-1, 1, 0, new float[] { (1 + 0 * RrData.textureWidth[texture]) / (float)RrData.atlasWidth, (RrData.textureY[texture] + 1 + 0 * RrData.textureHeight[texture]) / (float)RrData.atlasHeight, 0, 1 });
            sprite.Fan(0, 4); sprites[texture] = sprite;
        }
        model = Transform.Translate(model, x, y, z);
        model = Transform.Rotate(model, rand() % 360, 0, 0, 1);
        model = Transform.Scale(model, size, size, 1);
        tint = new float[] { (r & 255) / 255f, (g & 255) / 255f, (b & 255) / 255f, 1 };
        var bindings = sprites[texture].Bindings(model, tint);
        bindings["image"] = Game.atlas;
        var uniforms = (Dictionary<string, object>)bindings["uniforms"];
        uniforms["options"] = new float[] { 1, 0, 0, 2 };
        Gfx.Draw(sprites[texture].count, bindings, new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }

    public static void drawLaser(float[] model, Gfx.Blend blend, string key, float x, float y, float width, float height, int cc1, int cc2, int cc3, int cc4, int cnt, int type)
    {
        float[] tint = null;
        int i = 0, d = 0;
        float gx = 0, gy = 0;
        var part1 = new Mesh(key + "-1" + "-" + i.ToString());
        if (type != 0)
        {
            tint = new float[] { (cc1 & 255) / 255f, (cc1 & 255) / 255f, (cc1 & 255) / 255f, blend == Gfx.Blend.None ? 1 : (LASER_ALPHA & 255) / 255f };
            part1.Vertex(x - width, y, 0, tint);
        }

        tint = new float[] { (cc2 & 255) / 255f, (255 & 255) / 255f, (cc2 & 255) / 255f, blend == Gfx.Blend.None ? 1 : (LASER_ALPHA & 255) / 255f };
        part1.Vertex(x, y, 0, tint);
        tint = new float[] { (cc4 & 255) / 255f, (255 & 255) / 255f, (cc4 & 255) / 255f, blend == Gfx.Blend.None ? 1 : (LASER_ALPHA & 255) / 255f };
        part1.Vertex(x, y + height, 0, tint);
        tint = new float[] { (cc3 & 255) / 255f, (cc3 & 255) / 255f, (cc3 & 255) / 255f, blend == Gfx.Blend.None ? 1 : (LASER_ALPHA & 255) / 255f };
        part1.Vertex(x - width, y + height, 0, tint);
        part1.Fan(0, part1.vertexCount - 0);
        if (part1.count > 0) Gfx.Draw(part1.count, part1.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
        var part2 = new Mesh(key + "-2" + "-" + i.ToString());
        if (type != 0)
        {
            tint = new float[] { (cc1 & 255) / 255f, (cc1 & 255) / 255f, (cc1 & 255) / 255f, blend == Gfx.Blend.None ? 1 : (LASER_ALPHA & 255) / 255f };
            part2.Vertex(x + width, y, 0, tint);
        }

        tint = new float[] { (cc2 & 255) / 255f, (255 & 255) / 255f, (cc2 & 255) / 255f, blend == Gfx.Blend.None ? 1 : (LASER_ALPHA & 255) / 255f };
        part2.Vertex(x, y, 0, tint);
        tint = new float[] { (cc4 & 255) / 255f, (255 & 255) / 255f, (cc4 & 255) / 255f, blend == Gfx.Blend.None ? 1 : (LASER_ALPHA & 255) / 255f };
        part2.Vertex(x, y + height, 0, tint);
        tint = new float[] { (cc3 & 255) / 255f, (cc3 & 255) / 255f, (cc3 & 255) / 255f, blend == Gfx.Blend.None ? 1 : (LASER_ALPHA & 255) / 255f };
        part2.Vertex(x + width, y + height, 0, tint);
        part2.Fan(0, part2.vertexCount - 0);
        if (part2.count > 0) Gfx.Draw(part2.count, part2.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
        if (type == 2)
            return;
        tint = new float[] { (80 & 255) / 255f, (240 & 255) / 255f, (80 & 255) / 255f, blend == Gfx.Blend.None ? 1 : (LASER_LINE_ALPHA & 255) / 255f };
        var part3 = new Mesh(key + "-3" + "-" + i.ToString());
        d = (cnt * LASER_LINE_ROLL_SPEED) & (GameMath.integer(512 / 4) - 1);
        {
            i = 0;
            for (; i < 4; i++, d = d + ((GameMath.integer(512 / 4))))
            {
                d = d & (1023);
                gx = x + width * sctbl[(d + 256)] / 256.0f;
                if (type == 1)
                {
                    part3.Vertex(gx, y, 0, tint);
                }
                else
                {
                    part3.Vertex(x, y, 0, tint);
                }

                part3.Vertex(gx, y + height, 0, tint);
            }
        }

        if (type == 0)
        {
            for (int vi = 0; vi + 1 < part3.vertexCount; vi += 2) part3.Line(vi, vi + 1);
        if (part3.count > 0) Gfx.Draw(part3.count, part3.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
            return;
        }

        gy = y + (height / 4 / LASER_LINE_UP_SPEED) * (cnt & (LASER_LINE_UP_SPEED - 1));
        {
            i = 0;
            for (; i < 4; i++, gy = gy + (height / 4))
            {
                part3.Vertex(x - width, gy, 0, tint);
                part3.Vertex(x + width, gy, 0, tint);
            }
        }

        for (int vi = 0; vi + 1 < part3.vertexCount; vi += 2) part3.Line(vi, vi + 1);
        if (part3.count > 0) Gfx.Draw(part3.count, part3.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }

    public static void drawRing(float[] model, Gfx.Blend blend, string key, float x, float y, int d1, int d2, int r, int g, int b)
    {
        float[] tint = null;
        int i = 0, d = 0;
        float x1 = 0, y1 = 0, z1 = 0, x2 = 0, y2 = 0, z2 = 0, x3 = 0, y3 = 0, z3 = 0, x4 = 0, y4 = 0, z4 = 0;
        float[] parent1 = model;
        model = Transform.Translate(model, x, y, 0);
        model = Transform.Rotate(model, (float)d1 * 360 / 1024, 0, 0, 1);
        model = Transform.Rotate(model, (float)d2 * 360 / 1024, 1, 0, 0);
        tint = new float[] { (r & 255) / 255f, (g & 255) / 255f, (b & 255) / 255f, blend == Gfx.Blend.None ? 1 : (255 & 255) / 255f };
        {
            x2 = 0;
            x1 = x2;
        }

        {
            y4 = CORE_HEIGHT / 2;
            y1 = y4;
        }

        {
            y3 = -CORE_HEIGHT / 2;
            y2 = y3;
        }

        {
            z2 = CORE_RING_SIZE;
            z1 = z2;
        }

        {
            i = 0;
            d = 0;
            for (; i < 8; i++)
            {
                d = d + ((GameMath.integer(1024 / 8)));
                d = d & (1023);
                {
                    x4 = sctbl[(d + 256)] * CORE_RING_SIZE / 256;
                    x3 = x4;
                }

                {
                    z4 = sctbl[(d)] * CORE_RING_SIZE / 256;
                    z3 = z4;
                }

                drawSquare(model, blend, key + "-1280" + "-" + i.ToString(), x1, y1, z1, x2, y2, z2, x3, y3, z3, x4, y4, z4, r, g, b);
                x1 = x3;
                y1 = y3;
                z1 = z3;
                x2 = x4;
                y2 = y4;
                z2 = z4;
            }
        }

        model = parent1;
    }

    public static void drawCore(float[] model, Gfx.Blend blend, string key, float x, float y, int cnt, int r, int g, int b)
    {
        float[] tint = null;
        int i = 0;
        float cy = 0;
        float[] parent1 = model;
        model = Transform.Translate(model, x, y, 0);
        tint = new float[] { (r & 255) / 255f, (g & 255) / 255f, (b & 255) / 255f, blend == Gfx.Blend.None ? 1 : (255 & 255) / 255f };
        var part2 = new Mesh(key + "-2" + "-" + i.ToString());
        part2.Vertex(-SHAPE_POINT_SIZE_L, -SHAPE_POINT_SIZE_L, 0, tint);
        part2.Vertex(SHAPE_POINT_SIZE_L, -SHAPE_POINT_SIZE_L, 0, tint);
        part2.Vertex(SHAPE_POINT_SIZE_L, SHAPE_POINT_SIZE_L, 0, tint);
        part2.Vertex(-SHAPE_POINT_SIZE_L, SHAPE_POINT_SIZE_L, 0, tint);
        part2.Fan(0, part2.vertexCount - 0);
        if (part2.count > 0) Gfx.Draw(part2.count, part2.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
        model = parent1;
        cy = y - CORE_HEIGHT * 2.5f;
        {
            i = 0;
            for (; i < 4; i++, cy = cy + (CORE_HEIGHT))
            {
                drawRing(model, blend, key + "-1081" + "-" + i.ToString(), x, cy, (cnt * (4 + i)) & 1023, (GameMath.integer(sctbl[((cnt * (5 + i)) & 1023)] / 4)) & 1023, r, g, b);
            }
        }
    }

    public static void drawShipShape(float[] model, Gfx.Blend blend, string key, float x, float y, float d, int inv)
    {
        float[] tint = null;
        int i = 0;
        float[] parent1 = model;
        model = Transform.Translate(model, x, y, 0);
        tint = new float[] { (255 & 255) / 255f, (100 & 255) / 255f, (100 & 255) / 255f, blend == Gfx.Blend.None ? 1 : (255 & 255) / 255f };
        var part2 = new Mesh(key + "-2" + "-" + i.ToString());
        part2.Vertex(-SHAPE_POINT_SIZE_L, -SHAPE_POINT_SIZE_L, 0, tint);
        part2.Vertex(SHAPE_POINT_SIZE_L, -SHAPE_POINT_SIZE_L, 0, tint);
        part2.Vertex(SHAPE_POINT_SIZE_L, SHAPE_POINT_SIZE_L, 0, tint);
        part2.Vertex(-SHAPE_POINT_SIZE_L, SHAPE_POINT_SIZE_L, 0, tint);
        part2.Fan(0, part2.vertexCount - 0);
        if (part2.count > 0) Gfx.Draw(part2.count, part2.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
        if (((inv) != 0))
        {
            model = parent1;
            return;
        }

        model = Transform.Rotate(model, d, 0, 1, 0);
        tint = new float[] { (120 & 255) / 255f, (220 & 255) / 255f, (100 & 255) / 255f, blend == Gfx.Blend.None ? 1 : (150 & 255) / 255f };
        {
            i = 0;
            for (; i < 8; i++)
            {
                model = Transform.Rotate(model, 45, 0, 1, 0);
                var part3 = new Mesh(key + "-3" + "-" + i.ToString());
                part3.Vertex(-SHIP_DRUM_WIDTH, -SHIP_DRUM_HEIGHT, SHIP_DRUM_R, tint);
                part3.Vertex(SHIP_DRUM_WIDTH, -SHIP_DRUM_HEIGHT, SHIP_DRUM_R, tint);
                part3.Vertex(SHIP_DRUM_WIDTH, SHIP_DRUM_HEIGHT, SHIP_DRUM_R, tint);
                part3.Vertex(-SHIP_DRUM_WIDTH, SHIP_DRUM_HEIGHT, SHIP_DRUM_R, tint);
                part3.LineStrip(0, part3.vertexCount - 0, true);
        if (part3.count > 0) Gfx.Draw(part3.count, part3.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
            }
        }

        model = parent1;
    }

    public static void drawBomb(float[] model, Gfx.Blend blend, string key, float x, float y, float width, int cnt)
    {
        int i = 0, d = 0, od = 0, c = 0;
        float x1 = 0, y1 = 0, x2 = 0, y2 = 0;
        d = cnt * 48;
        d = d & (1023);
        c = 4 + (GameMath.signedShift(cnt, 3));
        if (c > 16)
            c = 16;
        od = GameMath.integer(1024 / c);
        x1 = (sctbl[(d)] * width) / 256 + x;
        y1 = (sctbl[(d + 256)] * width) / 256 + y;
        {
            i = 0;
            for (; i < c; i++)
            {
                d = d + (od);
                d = d & (1023);
                x2 = (sctbl[(d)] * width) / 256 + x;
                y2 = (sctbl[(d + 256)] * width) / 256 + y;
                drawLine(model, blend, key + "-623" + "-" + i.ToString(), x1, y1, 0, x2, y2, 0, 255, 255, 255, 255);
                x1 = x2;
                y1 = y2;
            }
        }
    }

    public static void drawCircle(float[] model, Gfx.Blend blend, string key, float x, float y, float width, int cnt, int r1, int g1, int b1, int r2, int b2, int g2)
    {
        float[] tint = null;
        int i = 0, d = 0;
        float x1 = 0, y1 = 0, x2 = 0, y2 = 0;
        if ((cnt & 1) == 0)
        {
            tint = new float[] { (r1 & 255) / 255f, (g1 & 255) / 255f, (b1 & 255) / 255f, blend == Gfx.Blend.None ? 1 : (64 & 255) / 255f };
        }
        else
        {
            tint = new float[] { (255 & 255) / 255f, (255 & 255) / 255f, (255 & 255) / 255f, blend == Gfx.Blend.None ? 1 : (64 & 255) / 255f };
        }

        var part1 = new Mesh(key + "-1" + "-" + i.ToString());
        part1.Vertex(x, y, 0, tint);
        d = cnt * 48;
        d = d & (1023);
        x1 = (sctbl[(d)] * width) / 256 + x;
        y1 = (sctbl[(d + 256)] * width) / 256 + y;
        tint = new float[] { (r2 & 255) / 255f, (g2 & 255) / 255f, (b2 & 255) / 255f, blend == Gfx.Blend.None ? 1 : (150 & 255) / 255f };
        {
            i = 0;
            for (; i < 16; i++)
            {
                d = d + (64);
                d = d & (1023);
                x2 = (sctbl[(d)] * width) / 256 + x;
                y2 = (sctbl[(d + 256)] * width) / 256 + y;
                part1.Vertex(x1, y1, 0, tint);
                part1.Vertex(x2, y2, 0, tint);
                x1 = x2;
                y1 = y2;
            }
        }

        part1.Fan(0, part1.vertexCount - 0);
        if (part1.count > 0) Gfx.Draw(part1.count, part1.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }

    public static void drawShape(float[] model, Gfx.Blend blend, string key, float x, float y, float size, int d, int cnt, int type, int r, int g, int b)
    {
        model = Transform.Translate(model, x, y, 0);
        float[] tint = new float[] { (r & 255) / 255f, (g & 255) / 255f, (b & 255) / 255f, 1 }; blend = Gfx.Blend.Additive;
        if (bulletPoint == null) {
            bulletPoint = new Mesh("bullet-point");
            bulletPoint.Vertex(-SHAPE_POINT_SIZE, -SHAPE_POINT_SIZE, 0, null);
            bulletPoint.Vertex(SHAPE_POINT_SIZE, -SHAPE_POINT_SIZE, 0, null);
            bulletPoint.Vertex(SHAPE_POINT_SIZE, SHAPE_POINT_SIZE, 0, null);
            bulletPoint.Vertex(-SHAPE_POINT_SIZE, SHAPE_POINT_SIZE, 0, null);
            bulletPoint.Fan(0, 4);
        }
        if (bulletPoint.count > 0) Gfx.Draw(bulletPoint.count, bulletPoint.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
        string geometryKey = "bullet-" + type.ToString() + "-" + r.ToString() + "-" + g.ToString() + "-" + b.ToString();
        if (!shapes.ContainsKey(geometryKey)) shapes[geometryKey] = createShape(geometryKey, type, r, g, b);
        Mesh mesh = shapes[geometryKey];
        model = Transform.Rotate(model, (float)((type == 1 ? cnt * 23 : type == 3 ? cnt * 37 : type == 4 ? cnt * 53 : type == 6 ? cnt * 13 : d) & 1023) * 360 / 1024, 0, 0, 1);
        model = Transform.Scale(model, size, size, 1);
        foreach (MeshRange range in mesh.ranges) {
            Gfx.Blend material = (Gfx.Blend)range.material;
            Gfx.Draw(range.count, mesh.Bindings(model, tint, 1, material == Gfx.Blend.Additive, range.first / 3),
                new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = material });
        }
    }

    public static int[][][] ikaClr = new int[][][]
    {
        new int[][]
        {
            new int[]
            {
                230,
                230,
                255
            },
            new int[]
            {
                100,
                100,
                200
            },
            new int[]
            {
                50,
                50,
                150
            }
        },
        new int[][]
        {
            new int[]
            {
                0,
                0,
                0
            },
            new int[]
            {
                200,
                0,
                0
            },
            new int[]
            {
                100,
                0,
                0
            }
        },
    };
    public static void drawShapeIka(float[] model, Gfx.Blend blend, string key, float x, float y, float size, int d, int cnt, int type, int c)
    {
        model = Transform.Translate(model, x, y, 0);
        float[] tint = new float[] { ikaClr[c][0][0] / 255f, ikaClr[c][0][1] / 255f, ikaClr[c][0][2] / 255f, 1 }; blend = Gfx.Blend.None;
        if (bulletPoint == null) {
            bulletPoint = new Mesh("bullet-point");
            bulletPoint.Vertex(-SHAPE_POINT_SIZE, -SHAPE_POINT_SIZE, 0, null);
            bulletPoint.Vertex(SHAPE_POINT_SIZE, -SHAPE_POINT_SIZE, 0, null);
            bulletPoint.Vertex(SHAPE_POINT_SIZE, SHAPE_POINT_SIZE, 0, null);
            bulletPoint.Vertex(-SHAPE_POINT_SIZE, SHAPE_POINT_SIZE, 0, null);
            bulletPoint.Fan(0, 4);
        }
        if (bulletPoint.count > 0) Gfx.Draw(bulletPoint.count, bulletPoint.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
        string geometryKey = "ika-" + type.ToString() + "-" + c.ToString();
        if (!shapes.ContainsKey(geometryKey)) shapes[geometryKey] = createShapeIka(geometryKey, type, c);
        Mesh mesh = shapes[geometryKey];
        model = Transform.Rotate(model, (float)((type == 1 ? cnt * 53 : d) & 1023) * 360 / 1024, 0, 0, 1);
        model = Transform.Scale(model, size, size, 1);
        foreach (MeshRange range in mesh.ranges) {
            Gfx.Blend material = (Gfx.Blend)range.material;
            Gfx.Draw(range.count, mesh.Bindings(model, tint, 1, material == Gfx.Blend.Additive, range.first / 3),
                new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = material });
        }
    }

    public static int[][][] shtClr = new int[][][]
    {
        new int[][]
        {
            new int[]
            {
                200,
                200,
                225
            },
            new int[]
            {
                50,
                50,
                200
            },
            new int[]
            {
                200,
                200,
                225
            }
        },
        new int[][]
        {
            new int[]
            {
                100,
                0,
                0
            },
            new int[]
            {
                100,
                0,
                0
            },
            new int[]
            {
                200,
                0,
                0
            }
        },
        new int[][]
        {
            new int[]
            {
                100,
                200,
                100
            },
            new int[]
            {
                50,
                100,
                50
            },
            new int[]
            {
                100,
                200,
                100
            }
        },
    };
    public static void drawShot(float[] model, Gfx.Blend blend, string key, float x, float y, float d, int c, float width, float height)
    {
        float[] tint = null;
        string geometryKey = "shot-" + c.ToString();
        if (!shapes.ContainsKey(geometryKey)) shapes[geometryKey] = createShot(geometryKey, c);
        Mesh mesh = shapes[geometryKey];
        model = Transform.Translate(model, x, y, 0);
        model = Transform.Rotate(model, d, 0, 0, 1);
        model = Transform.Scale(model, width, height, 1);
        foreach (MeshRange range in mesh.ranges) {
            Gfx.Blend material = (Gfx.Blend)range.material;
            Gfx.Draw(range.count, mesh.Bindings(model, tint, 1, material == Gfx.Blend.Additive, range.first / 3),
                new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = material });
        }
    }





    public static void drawBoard(float[] model, Gfx.Blend blend, string key, int x, int y, int width, int height)
    {
        float[] tint = null;
        tint = new float[] { (0 & 255) / 255f, (0 & 255) / 255f, (0 & 255) / 255f, blend == Gfx.Blend.None ? 1 : (255 & 255) / 255f };
        var part1 = new Mesh(key + "-1");
        part1.Vertex(x, y, 0, tint);
        part1.Vertex(x + width, y, 0, tint);
        part1.Vertex(x + width, y + height, 0, tint);
        part1.Vertex(x, y + height, 0, tint);
        part1.Quads(0, part1.vertexCount - 0);
        if (part1.count > 0) Gfx.Draw(part1.count, part1.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }

    public static void drawSideBoards(float[] model, Gfx.Blend blend, string key)
    {
        blend = Gfx.Blend.None;
        drawBoard(model, blend, key + "-41", 0, 0, 160, 480);
        drawBoard(model, blend, key + "-76", 480, 0, 160, 480);
        blend = Gfx.Blend.Additive;
        drawScore(model, blend, key + "-149");
        drawRPanel(model, blend, key + "-170");
    }

    public static void drawTitleBoard(float[] model, Gfx.Blend blend, string key)
    {
        float[] tint = null;
        int texture = 2;
        if (sprites[texture] == null) {
            Mesh sprite = new Mesh("atlas-" + texture.ToString());
            sprite.Vertex(350, 78, 0, new float[] { (1 + 0 * RrData.textureWidth[texture]) / (float)RrData.atlasWidth, (RrData.textureY[texture] + 1 + 0 * RrData.textureHeight[texture]) / (float)RrData.atlasHeight, 0, 1 });
            sprite.Vertex(470, 78, 0, new float[] { (1 + 1 * RrData.textureWidth[texture]) / (float)RrData.atlasWidth, (RrData.textureY[texture] + 1 + 0 * RrData.textureHeight[texture]) / (float)RrData.atlasHeight, 0, 1 });
            sprite.Vertex(470, 114, 0, new float[] { (1 + 1 * RrData.textureWidth[texture]) / (float)RrData.atlasWidth, (RrData.textureY[texture] + 1 + 1 * RrData.textureHeight[texture]) / (float)RrData.atlasHeight, 0, 1 });
            sprite.Vertex(350, 114, 0, new float[] { (1 + 0 * RrData.textureWidth[texture]) / (float)RrData.atlasWidth, (RrData.textureY[texture] + 1 + 1 * RrData.textureHeight[texture]) / (float)RrData.atlasHeight, 0, 1 });
            sprite.Fan(0, 4); sprites[texture] = sprite;
        }
        tint = new float[] { 1, 1, 1, 1 };
        var bindings = sprites[texture].Bindings(model, tint);
        bindings["image"] = Game.atlas;
        var uniforms = (Dictionary<string, object>)bindings["uniforms"];
        uniforms["options"] = new float[] { 1, 0, 0, 2 };
        Gfx.Draw(sprites[texture].count, bindings, new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
        tint = new float[] { (200 & 255) / 255f, (200 & 255) / 255f, (200 & 255) / 255f, blend == Gfx.Blend.None ? 1 : (255 & 255) / 255f };
        var part1 = new Mesh(key + "-1");
        part1.Vertex(350, 30, 0, tint);
        part1.Vertex(400, 30, 0, tint);
        part1.Vertex(380, 56, 0, tint);
        part1.Vertex(380, 80, 0, tint);
        part1.Vertex(350, 80, 0, tint);
        part1.Fan(0, part1.vertexCount - 0);
        if (part1.count > 0) Gfx.Draw(part1.count, part1.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
        var part2 = new Mesh(key + "-2");
        part2.Vertex(404, 80, 0, tint);
        part2.Vertex(404, 8, 0, tint);
        part2.Vertex(440, 8, 0, tint);
        part2.Vertex(440, 44, 0, tint);
        part2.Vertex(465, 80, 0, tint);
        part2.Fan(0, part2.vertexCount - 0);
        if (part2.count > 0) Gfx.Draw(part2.count, part2.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
        tint = new float[] { (255 & 255) / 255f, (255 & 255) / 255f, (255 & 255) / 255f, blend == Gfx.Blend.None ? 1 : (255 & 255) / 255f };
        var part3 = new Mesh(key + "-3");
        part3.Vertex(350, 30, 0, tint);
        part3.Vertex(400, 30, 0, tint);
        part3.Vertex(380, 56, 0, tint);
        part3.Vertex(380, 80, 0, tint);
        part3.Vertex(350, 80, 0, tint);
        part3.LineStrip(0, part3.vertexCount - 0, true);
        if (part3.count > 0) Gfx.Draw(part3.count, part3.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
        var part4 = new Mesh(key + "-4");
        part4.Vertex(404, 80, 0, tint);
        part4.Vertex(404, 8, 0, tint);
        part4.Vertex(440, 8, 0, tint);
        part4.Vertex(440, 44, 0, tint);
        part4.Vertex(465, 80, 0, tint);
        part4.LineStrip(0, part4.vertexCount - 0, true);
        if (part4.count > 0) Gfx.Draw(part4.count, part4.Bindings(model, tint, 1, blend == Gfx.Blend.Additive),
            new DrawOpts { Shader = Game.shader, Depth = false, Cull = Gfx.Cull.None, Blend = blend });
    }

    public static int drawNum(float[] model, Gfx.Blend blend, string key, int n, int x, int y, int s, int r, int g, int b)
    {
        for (;;)
        {
            drawLetter(model, blend, key + "-40" + "-" + x.ToString(), n % 10, x, y, s, 3, r, g, b);
            y = GameMath.integer(y + (s * 1.7f));
            n = GameMath.integer(n / (10));
            if (n <= 0)
                break;
        }

        return y;
    }

    public static int drawNumRight(float[] model, Gfx.Blend blend, string key, int n, int x, int y, int s, int r, int g, int b)
    {
        int d = 0, nd = 0, drawn = 0;
        {
            d = 100000000;
            for (; d > 0; d = GameMath.integer(d / (10)))
            {
                nd = GameMath.integer((GameMath.integer(n / d)));
                if ((nd > 0) || (((drawn) != 0)))
                {
                    n = n - (d * nd);
                    drawLetter(model, blend, key + "-340" + "-" + x.ToString(), nd % 10, x, y, s, 1, r, g, b);
                    y = GameMath.integer(y + (s * 1.7f));
                    drawn = 1;
                }
            }
        }

        if (!(((drawn) != 0)))
        {
            drawLetter(model, blend, key + "-567" + "-" + x.ToString(), 0, x, y, s, 1, r, g, b);
            y = GameMath.integer(y + (s * 1.7f));
        }

        return y;
    }

    public static int drawNumCenter(float[] model, Gfx.Blend blend, string key, int n, int x, int y, int s, int r, int g, int b)
    {
        for (;;)
        {
            drawLetter(model, blend, key + "-40" + "-" + x.ToString(), n % 10, x, y, s, 0, r, g, b);
            x = GameMath.integer(x - (s * 1.7f));
            n = GameMath.integer(n / (10));
            if (n <= 0)
                break;
        }

        return y;
    }

    public static int drawTimeCenter(float[] model, Gfx.Blend blend, string key, int n, int x, int y, int s, int r, int g, int b)
    {
        int i = 0;
        {
            i = 0;
            for (; i < 7; i++)
            {
                if (i != 4)
                {
                    drawLetter(model, blend, key + "-160" + "-" + i.ToString(), n % 10, x, y, s, 0, r, g, b);
                    n = GameMath.integer(n / (10));
                }
                else
                {
                    drawLetter(model, blend, key + "-330" + "-" + i.ToString(), n % 6, x, y, s, 0, r, g, b);
                    n = GameMath.integer(n / (6));
                }

                if (((i & 1) == 1) || (i == 0))
                {
                    switch (i)
                    {
                        case 3:
                            drawLetter(model, blend, key + "-619" + "-" + i.ToString(), 41, GameMath.integer(x + s * 1.16f), y, s, 0, r, g, b);
                            break;
                        case 5:
                            drawLetter(model, blend, key + "-781" + "-" + i.ToString(), 40, GameMath.integer(x + s * 1.16f), y, s, 0, r, g, b);
                            break;
                    }

                    x = GameMath.integer(x - (s * 1.7f));
                }
                else
                {
                    x = GameMath.integer(x - (s * 2.2f));
                }

                if (n <= 0)
                    break;
            }
        }

        return y;
    }

    static Mesh createShape(string key, int type, int r, int g, int b)
    {
        var mesh = new Mesh(key); float[] model = Transform.Identity(); float[] tint = new float[] { (r & 255) / 255f, (g & 255) / 255f, (b & 255) / 255f, 1 }; Gfx.Blend blend = Gfx.Blend.Additive; float size = 1;
        float sz = 0, sz2 = 0;
        switch (type)
        {
            case 0:
                sz = size / 2;
                blend = Gfx.Blend.None;
                int part1 = mesh.vertexCount; int face1 = mesh.count;
                mesh.Vertex(-sz, -sz, 0, tint, model);
                mesh.Vertex(sz, -sz, 0, tint, model);
                mesh.Vertex(0, size, 0, tint, model);
                mesh.LineStrip(part1, mesh.vertexCount - part1, true); mesh.AddRange(face1, (int)blend);
                blend = Gfx.Blend.Additive;
                tint = new float[] { (r & 255) / 255f, (g & 255) / 255f, (b & 255) / 255f, blend == Gfx.Blend.None ? 1 : (150 & 255) / 255f };
                int part2 = mesh.vertexCount; int face2 = mesh.count;
                mesh.Vertex(-sz, -sz, 0, tint, model);
                mesh.Vertex(sz, -sz, 0, tint, model);
                tint = new float[] { (SHAPE_BASE_COLOR_R & 255) / 255f, (SHAPE_BASE_COLOR_G & 255) / 255f, (SHAPE_BASE_COLOR_B & 255) / 255f, blend == Gfx.Blend.None ? 1 : (150 & 255) / 255f };
                mesh.Vertex(0, size, 0, tint, model);
                mesh.Fan(part2, mesh.vertexCount - part2); mesh.AddRange(face2, (int)blend);
                break;
            case 1:
                sz = size / 2;
                blend = Gfx.Blend.None;
                int part3 = mesh.vertexCount; int face3 = mesh.count;
                mesh.Vertex(0, -size, 0, tint, model);
                mesh.Vertex(sz, 0, 0, tint, model);
                mesh.Vertex(0, size, 0, tint, model);
                mesh.Vertex(-sz, 0, 0, tint, model);
                mesh.LineStrip(part3, mesh.vertexCount - part3, true); mesh.AddRange(face3, (int)blend);
                blend = Gfx.Blend.Additive;
                tint = new float[] { (r & 255) / 255f, (g & 255) / 255f, (b & 255) / 255f, blend == Gfx.Blend.None ? 1 : (180 & 255) / 255f };
                int part4 = mesh.vertexCount; int face4 = mesh.count;
                mesh.Vertex(0, -size, 0, tint, model);
                mesh.Vertex(sz, 0, 0, tint, model);
                tint = new float[] { (SHAPE_BASE_COLOR_R & 255) / 255f, (SHAPE_BASE_COLOR_G & 255) / 255f, (SHAPE_BASE_COLOR_B & 255) / 255f, blend == Gfx.Blend.None ? 1 : (150 & 255) / 255f };
                mesh.Vertex(0, size, 0, tint, model);
                mesh.Vertex(-sz, 0, 0, tint, model);
                mesh.Fan(part4, mesh.vertexCount - part4); mesh.AddRange(face4, (int)blend);
                break;
            case 2:
                sz = size / 4;
                sz2 = size / 3 * 2;
                blend = Gfx.Blend.None;
                int part5 = mesh.vertexCount; int face5 = mesh.count;
                mesh.Vertex(-sz, -sz2, 0, tint, model);
                mesh.Vertex(sz, -sz2, 0, tint, model);
                mesh.Vertex(sz, sz2, 0, tint, model);
                mesh.Vertex(-sz, sz2, 0, tint, model);
                mesh.LineStrip(part5, mesh.vertexCount - part5, true); mesh.AddRange(face5, (int)blend);
                blend = Gfx.Blend.Additive;
                tint = new float[] { (r & 255) / 255f, (g & 255) / 255f, (b & 255) / 255f, blend == Gfx.Blend.None ? 1 : (120 & 255) / 255f };
                int part6 = mesh.vertexCount; int face6 = mesh.count;
                mesh.Vertex(-sz, -sz2, 0, tint, model);
                mesh.Vertex(sz, -sz2, 0, tint, model);
                tint = new float[] { (SHAPE_BASE_COLOR_R & 255) / 255f, (SHAPE_BASE_COLOR_G & 255) / 255f, (SHAPE_BASE_COLOR_B & 255) / 255f, blend == Gfx.Blend.None ? 1 : (150 & 255) / 255f };
                mesh.Vertex(sz, sz2, 0, tint, model);
                mesh.Vertex(-sz, sz2, 0, tint, model);
                mesh.Fan(part6, mesh.vertexCount - part6); mesh.AddRange(face6, (int)blend);
                break;
            case 3:
                sz = size / 2;
                blend = Gfx.Blend.None;
                int part7 = mesh.vertexCount; int face7 = mesh.count;
                mesh.Vertex(-sz, -sz, 0, tint, model);
                mesh.Vertex(sz, -sz, 0, tint, model);
                mesh.Vertex(sz, sz, 0, tint, model);
                mesh.Vertex(-sz, sz, 0, tint, model);
                mesh.LineStrip(part7, mesh.vertexCount - part7, true); mesh.AddRange(face7, (int)blend);
                blend = Gfx.Blend.Additive;
                tint = new float[] { (r & 255) / 255f, (g & 255) / 255f, (b & 255) / 255f, blend == Gfx.Blend.None ? 1 : (180 & 255) / 255f };
                int part8 = mesh.vertexCount; int face8 = mesh.count;
                mesh.Vertex(-sz, -sz, 0, tint, model);
                mesh.Vertex(sz, -sz, 0, tint, model);
                tint = new float[] { (SHAPE_BASE_COLOR_R & 255) / 255f, (SHAPE_BASE_COLOR_G & 255) / 255f, (SHAPE_BASE_COLOR_B & 255) / 255f, blend == Gfx.Blend.None ? 1 : (150 & 255) / 255f };
                mesh.Vertex(sz, sz, 0, tint, model);
                mesh.Vertex(-sz, sz, 0, tint, model);
                mesh.Fan(part8, mesh.vertexCount - part8); mesh.AddRange(face8, (int)blend);
                break;
            case 4:
                sz = size / 2;
                blend = Gfx.Blend.None;
                int part9 = mesh.vertexCount; int face9 = mesh.count;
                mesh.Vertex(-sz / 2, -sz, 0, tint, model);
                mesh.Vertex(sz / 2, -sz, 0, tint, model);
                mesh.Vertex(sz, -sz / 2, 0, tint, model);
                mesh.Vertex(sz, sz / 2, 0, tint, model);
                mesh.Vertex(sz / 2, sz, 0, tint, model);
                mesh.Vertex(-sz / 2, sz, 0, tint, model);
                mesh.Vertex(-sz, sz / 2, 0, tint, model);
                mesh.Vertex(-sz, -sz / 2, 0, tint, model);
                mesh.LineStrip(part9, mesh.vertexCount - part9, true); mesh.AddRange(face9, (int)blend);
                blend = Gfx.Blend.Additive;
                tint = new float[] { (r & 255) / 255f, (g & 255) / 255f, (b & 255) / 255f, blend == Gfx.Blend.None ? 1 : (220 & 255) / 255f };
                int part10 = mesh.vertexCount; int face10 = mesh.count;
                mesh.Vertex(-sz / 2, -sz, 0, tint, model);
                mesh.Vertex(sz / 2, -sz, 0, tint, model);
                mesh.Vertex(sz, -sz / 2, 0, tint, model);
                mesh.Vertex(sz, sz / 2, 0, tint, model);
                tint = new float[] { (SHAPE_BASE_COLOR_R & 255) / 255f, (SHAPE_BASE_COLOR_G & 255) / 255f, (SHAPE_BASE_COLOR_B & 255) / 255f, blend == Gfx.Blend.None ? 1 : (150 & 255) / 255f };
                mesh.Vertex(sz / 2, sz, 0, tint, model);
                mesh.Vertex(-sz / 2, sz, 0, tint, model);
                mesh.Vertex(-sz, sz / 2, 0, tint, model);
                mesh.Vertex(-sz, -sz / 2, 0, tint, model);
                mesh.Fan(part10, mesh.vertexCount - part10); mesh.AddRange(face10, (int)blend);
                break;
            case 5:
                sz = size * 2 / 3;
                sz2 = size / 5;
                blend = Gfx.Blend.None;
                int part11 = mesh.vertexCount; int face11 = mesh.count;
                mesh.Vertex(-sz, -sz + sz2, 0, tint, model);
                mesh.Vertex(0, sz + sz2, 0, tint, model);
                mesh.Vertex(sz, -sz + sz2, 0, tint, model);
                mesh.LineStrip(part11, mesh.vertexCount - part11); mesh.AddRange(face11, (int)blend);
                blend = Gfx.Blend.Additive;
                tint = new float[] { (r & 255) / 255f, (g & 255) / 255f, (b & 255) / 255f, blend == Gfx.Blend.None ? 1 : (150 & 255) / 255f };
                int part12 = mesh.vertexCount; int face12 = mesh.count;
                mesh.Vertex(-sz, -sz + sz2, 0, tint, model);
                mesh.Vertex(sz, -sz + sz2, 0, tint, model);
                tint = new float[] { (SHAPE_BASE_COLOR_R & 255) / 255f, (SHAPE_BASE_COLOR_G & 255) / 255f, (SHAPE_BASE_COLOR_B & 255) / 255f, blend == Gfx.Blend.None ? 1 : (150 & 255) / 255f };
                mesh.Vertex(0, sz + sz2, 0, tint, model);
                mesh.Fan(part12, mesh.vertexCount - part12); mesh.AddRange(face12, (int)blend);
                break;
            case 6:
                sz = size / 2;
                blend = Gfx.Blend.None;
                int part13 = mesh.vertexCount; int face13 = mesh.count;
                mesh.Vertex(-sz, -sz, 0, tint, model);
                mesh.Vertex(0, -sz, 0, tint, model);
                mesh.Vertex(sz, 0, 0, tint, model);
                mesh.Vertex(sz, sz, 0, tint, model);
                mesh.Vertex(0, sz, 0, tint, model);
                mesh.Vertex(-sz, 0, 0, tint, model);
                mesh.LineStrip(part13, mesh.vertexCount - part13, true); mesh.AddRange(face13, (int)blend);
                blend = Gfx.Blend.Additive;
                tint = new float[] { (r & 255) / 255f, (g & 255) / 255f, (b & 255) / 255f, blend == Gfx.Blend.None ? 1 : (210 & 255) / 255f };
                int part14 = mesh.vertexCount; int face14 = mesh.count;
                mesh.Vertex(-sz, -sz, 0, tint, model);
                mesh.Vertex(0, -sz, 0, tint, model);
                mesh.Vertex(sz, 0, 0, tint, model);
                tint = new float[] { (SHAPE_BASE_COLOR_R & 255) / 255f, (SHAPE_BASE_COLOR_G & 255) / 255f, (SHAPE_BASE_COLOR_B & 255) / 255f, blend == Gfx.Blend.None ? 1 : (150 & 255) / 255f };
                mesh.Vertex(sz, sz, 0, tint, model);
                mesh.Vertex(0, sz, 0, tint, model);
                mesh.Vertex(-sz, 0, 0, tint, model);
                mesh.Fan(part14, mesh.vertexCount - part14); mesh.AddRange(face14, (int)blend);
                break;
        }



        return mesh;
    }

    static Mesh createShapeIka(string key, int type, int c)
    {
        var mesh = new Mesh(key); float[] model = Transform.Identity(); float[] tint = new float[] { ikaClr[c][0][0] / 255f, ikaClr[c][0][1] / 255f, ikaClr[c][0][2] / 255f, 1 }; Gfx.Blend blend = Gfx.Blend.None; float size = 1;
        float sz = 0, sz2 = 0, sz3 = 0;
        switch (type)
        {
            case 0:
                sz = size / 2;
                sz2 = sz / 3;
                sz3 = size * 2 / 3;
                int part1 = mesh.vertexCount; int face1 = mesh.count;
                mesh.Vertex(-sz, -sz3, 0, tint, model);
                mesh.Vertex(sz, -sz3, 0, tint, model);
                mesh.Vertex(sz2, sz3, 0, tint, model);
                mesh.Vertex(-sz2, sz3, 0, tint, model);
                mesh.LineStrip(part1, mesh.vertexCount - part1, true); mesh.AddRange(face1, (int)blend);
                blend = Gfx.Blend.Additive;
                tint = new float[] { (ikaClr[(c)][(1)][(0)] & 255) / 255f, (ikaClr[(c)][(1)][(1)] & 255) / 255f, (ikaClr[(c)][(1)][(2)] & 255) / 255f, blend == Gfx.Blend.None ? 1 : (250 & 255) / 255f };
                int part2 = mesh.vertexCount; int face2 = mesh.count;
                mesh.Vertex(-sz, -sz3, 0, tint, model);
                mesh.Vertex(sz, -sz3, 0, tint, model);
                tint = new float[] { (ikaClr[(c)][(2)][(0)] & 255) / 255f, (ikaClr[(c)][(2)][(1)] & 255) / 255f, (ikaClr[(c)][(2)][(2)] & 255) / 255f, blend == Gfx.Blend.None ? 1 : (250 & 255) / 255f };
                mesh.Vertex(sz2, sz3, 0, tint, model);
                mesh.Vertex(-sz2, sz3, 0, tint, model);
                mesh.Fan(part2, mesh.vertexCount - part2); mesh.AddRange(face2, (int)blend);
                break;
            case 1:
                sz = size / 2;
                int part3 = mesh.vertexCount; int face3 = mesh.count;
                mesh.Vertex(-sz / 2, -sz, 0, tint, model);
                mesh.Vertex(sz / 2, -sz, 0, tint, model);
                mesh.Vertex(sz, -sz / 2, 0, tint, model);
                mesh.Vertex(sz, sz / 2, 0, tint, model);
                mesh.Vertex(sz / 2, sz, 0, tint, model);
                mesh.Vertex(-sz / 2, sz, 0, tint, model);
                mesh.Vertex(-sz, sz / 2, 0, tint, model);
                mesh.Vertex(-sz, -sz / 2, 0, tint, model);
                mesh.LineStrip(part3, mesh.vertexCount - part3, true); mesh.AddRange(face3, (int)blend);
                blend = Gfx.Blend.Additive;
                tint = new float[] { (ikaClr[(c)][(1)][(0)] & 255) / 255f, (ikaClr[(c)][(1)][(1)] & 255) / 255f, (ikaClr[(c)][(1)][(2)] & 255) / 255f, blend == Gfx.Blend.None ? 1 : (250 & 255) / 255f };
                int part4 = mesh.vertexCount; int face4 = mesh.count;
                mesh.Vertex(-sz / 2, -sz, 0, tint, model);
                mesh.Vertex(sz / 2, -sz, 0, tint, model);
                mesh.Vertex(sz, -sz / 2, 0, tint, model);
                mesh.Vertex(sz, sz / 2, 0, tint, model);
                tint = new float[] { (ikaClr[(c)][(2)][(0)] & 255) / 255f, (ikaClr[(c)][(2)][(1)] & 255) / 255f, (ikaClr[(c)][(2)][(2)] & 255) / 255f, blend == Gfx.Blend.None ? 1 : (250 & 255) / 255f };
                mesh.Vertex(sz / 2, sz, 0, tint, model);
                mesh.Vertex(-sz / 2, sz, 0, tint, model);
                mesh.Vertex(-sz, sz / 2, 0, tint, model);
                mesh.Vertex(-sz, -sz / 2, 0, tint, model);
                mesh.Fan(part4, mesh.vertexCount - part4); mesh.AddRange(face4, (int)blend);
                break;
        }



        return mesh;
    }

    static Mesh createShot(string key, int c)
    {
        var mesh = new Mesh(key); float[] model = Transform.Identity(); float[] tint = null; Gfx.Blend blend = Gfx.Blend.Additive; float width = 1, height = 1;
        tint = new float[] { (shtClr[(c)][(0)][(0)] & 255) / 255f, (shtClr[(c)][(0)][(1)] & 255) / 255f, (shtClr[(c)][(0)][(2)] & 255) / 255f, blend == Gfx.Blend.None ? 1 : (240 & 255) / 255f };
        blend = Gfx.Blend.None; tint[3] = 1;
        int part1 = mesh.vertexCount; int face1 = mesh.count;
        mesh.Vertex(-width, -height, 0, tint, model);
        mesh.Vertex(-width, height, 0, tint, model);
        mesh.Vertex(width, -height, 0, tint, model);
        mesh.Vertex(width, height, 0, tint, model);
        for (int vi = part1; vi + 1 < mesh.vertexCount; vi += 2) mesh.Line(vi, vi + 1); mesh.AddRange(face1, (int)blend);
        blend = Gfx.Blend.Additive;
        tint = new float[] { (shtClr[(c)][(1)][(0)] & 255) / 255f, (shtClr[(c)][(1)][(1)] & 255) / 255f, (shtClr[(c)][(1)][(2)] & 255) / 255f, blend == Gfx.Blend.None ? 1 : (240 & 255) / 255f };
        int part2 = mesh.vertexCount; int face2 = mesh.count;
        mesh.Vertex(-width, -height, 0, tint, model);
        mesh.Vertex(width, -height, 0, tint, model);
        tint = new float[] { (shtClr[(c)][(2)][(0)] & 255) / 255f, (shtClr[(c)][(2)][(1)] & 255) / 255f, (shtClr[(c)][(2)][(2)] & 255) / 255f, blend == Gfx.Blend.None ? 1 : (240 & 255) / 255f };
        mesh.Vertex(width, height, 0, tint, model);
        mesh.Vertex(-width, height, 0, tint, model);
        mesh.Fan(part2, mesh.vertexCount - part2); mesh.AddRange(face2, (int)blend);

        return mesh;
    }
}
