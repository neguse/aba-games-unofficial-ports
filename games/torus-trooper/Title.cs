// Copyright 2004 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public class TitleManager
{
    public const int REPLAY_CHANGE_DURATION = 30;
    public const int AUTO_REPEAT_START_TIME = 30;
    public const int AUTO_REPEAT_CNT = 5;
    public PrefManager prefManager;
    public Pad pad;
    public Ship ship;
    public GameManager gameManager;
    static int nextMesh;
    public Mesh[] meshes;
    public Mesh logo;
    public float cnt;
    public int grade, level;
    public bool dirPressed, btnPressed;
    public float keyRepeatCnt;
    public float replayCnt;
    public bool _replayMode;
    public float _replayChangeRatio;
    public TitleManager(PrefManager pm, Pad p, Ship s, GameManager gm)
    {
        prefManager = pm;
        pad = p;
        ship = s;
        gameManager = gm;
        createTorusShape();
    }

    public void close()
    {
        meshes = null;
    }

    public void start()
    {
        cnt = 0;
        grade = prefManager.prefData.selectedGrade;
        level = prefManager.prefData.selectedLevel;
        keyRepeatCnt = 0;
        {
            btnPressed = true;
            dirPressed = btnPressed;
        }

        replayCnt = 0;
        _replayMode = false;
    }

    public void move(bool hasReplayData)
    {
        int dir = pad.getDirState();
        if (!(replayMode))
        {
            if (((dir & (PadDir.RIGHT | PadDir.LEFT)) != 0))
            {
                if (!(dirPressed))
                {
                    dirPressed = true;
                    if (((dir & PadDir.RIGHT) != 0))
                    {
                        grade++;
                        if (grade >= Ship.GRADE_NUM)
                            grade = 0;
                    }

                    if (((dir & PadDir.LEFT) != 0))
                    {
                        grade--;
                        if (grade < 0)
                            grade = Ship.GRADE_NUM - 1;
                    }

                    if (level > prefManager.prefData.getMaxLevel(grade))
                        level = prefManager.prefData.getMaxLevel(grade);
                }
            }

            if (((dir & (PadDir.UP | PadDir.DOWN)) != 0))
            {
                int mv = 0;
                if (!(dirPressed))
                {
                    dirPressed = true;
                    mv = 1;
                }
                else
                {
                    keyRepeatCnt += SimulationTime.Step;
                    if (keyRepeatCnt >= AUTO_REPEAT_START_TIME)
                    {
                        if (SimulationTime.Period(keyRepeatCnt - SimulationTime.Step, AUTO_REPEAT_CNT))
                        {
                            mv = (GameMath.integer(keyRepeatCnt / AUTO_REPEAT_START_TIME)) * (GameMath.integer(keyRepeatCnt / AUTO_REPEAT_START_TIME));
                        }
                    }
                }

                if (((dir & PadDir.DOWN) != 0))
                {
                    level = level + (mv);
                    if (level > prefManager.prefData.getMaxLevel(grade))
                    {
                        if (keyRepeatCnt >= AUTO_REPEAT_START_TIME)
                            level = prefManager.prefData.getMaxLevel(grade);
                        else
                            level = 1;
                        keyRepeatCnt = 0;
                    }
                }

                if (((dir & PadDir.UP) != 0))
                {
                    level = level - (mv);
                    if (level < 1)
                    {
                        if (keyRepeatCnt >= AUTO_REPEAT_START_TIME)
                            level = 1;
                        else
                            level = prefManager.prefData.getMaxLevel(grade);
                        keyRepeatCnt = 0;
                    }
                }
            }
        }
        else
        {
            if (((dir & (PadDir.RIGHT | PadDir.LEFT)) != 0))
            {
                if (!(dirPressed))
                {
                    dirPressed = true;
                    if (((dir & PadDir.RIGHT) != 0))
                    {
                        Ship.cameraMode = false;
                    }

                    if (((dir & PadDir.LEFT) != 0))
                    {
                        Ship.cameraMode = true;
                    }
                }
            }

            if (((dir & (PadDir.UP | PadDir.DOWN)) != 0))
            {
                if (!(dirPressed))
                {
                    dirPressed = true;
                    if (((dir & PadDir.UP) != 0))
                    {
                        Ship.drawFrontMode = true;
                    }

                    if (((dir & PadDir.DOWN) != 0))
                    {
                        Ship.drawFrontMode = false;
                    }
                }
            }
        }

        if (dir == 0)
        {
            dirPressed = false;
            keyRepeatCnt = 0;
        }

        int btn = pad.getButtonState();
        if (((btn & PadButton.ANY) != 0))
        {
            if (!(btnPressed))
            {
                btnPressed = true;
                if (((btn & PadButton.A) != 0))
                {
                    if (!(replayMode))
                    {
                        prefManager.prefData.recordStartGame(grade, level);
                        gameManager.startInGame();
                    }
                }

                if (hasReplayData)
                    if (((btn & PadButton.B) != 0))
                        _replayMode = !(_replayMode);
            }
        }
        else
        {
            btnPressed = false;
        }

        cnt += SimulationTime.Step;
        if (_replayMode)
        {
            if (replayCnt < REPLAY_CHANGE_DURATION)
                replayCnt = Math.Min(REPLAY_CHANGE_DURATION, replayCnt + SimulationTime.Step);
        }
        else
        {
            if (replayCnt > 0)
                replayCnt = Math.Max(0, replayCnt - SimulationTime.Step);
        }

        _replayChangeRatio = (float)replayCnt / REPLAY_CHANGE_DURATION;
    }

    public void draw(float[] model, float[] tint, Gfx.Blend blend, Gfx.Cull cull, float lineWidth)
    {
        if (_replayChangeRatio >= 1.0f)
            return;
        model = Transform.Ortho();
        blend = Gfx.Blend.None;
        tint = new float[] { 0, 0, 0, 1 };
        float rcr = _replayChangeRatio * 2;
        if (rcr > 1)
            rcr = 1;
        var part1 = new Mesh("Title-draw-1");
        part1.Vertex(450 + (640 - 450) * rcr, 0, 0, tint);
        part1.Vertex(640, 0, 0, tint);
        part1.Vertex(640, 480, 0, tint);
        part1.Vertex(450 + (640 - 450) * rcr, 480, 0, tint);
        part1.Quads(0, part1.vertexCount - 0);
        if (part1.count > 0) TtRender.Draw(part1, model, tint, lineWidth, blend, cull);
        blend = Gfx.Blend.Additive;
        model = Transform.LookAt(Transform.Perspective(10000), 0, 0, -1, 0, 0, 0, 0, 1, 0);
        float[] parent2 = model;
        model = Transform.Translate(model, 3 - _replayChangeRatio * 2.4f, 1.8f, 3.5f - _replayChangeRatio * 1.5f);
        model = Transform.Rotate(model, 30, 1, 0, 0);
        model = Transform.Rotate(model, sin(cnt * 0.005f) * 12, 0, 1, 0);
        model = Transform.Rotate(model, cnt * 0.2f, 0, 0, 1);
        blend = Gfx.Blend.None;
        tint = new float[] { 0, 0, 0, 1 };
        { Mesh shape3 = meshes[1]; if (shape3.count > 0) TtRender.Draw(shape3, model, tint, lineWidth, blend, cull); }
        blend = Gfx.Blend.Additive;
        tint = new float[] { 1, 1, 1, 0.5f };
        { Mesh shape4 = meshes[0]; if (shape4.count > 0) TtRender.Draw(shape4, model, tint, lineWidth, blend, cull); }
        model = parent2;
    }

    public void drawFront(float[] model, float[] tint, Gfx.Blend blend, Gfx.Cull cull, float lineWidth)
    {
        if (_replayChangeRatio > 0)
            return;
        float[] parent1 = model;
        model = Transform.Translate(model, 508, 400, 0);
        model = Transform.Rotate(model, -20, 0, 0, 1);
        model = Transform.Scale(model, 128, 64, 1);
        lineWidth = 2;
        { Mesh shape2 = meshes[2]; if (shape2.count > 0) TtRender.Draw(shape2, model, tint, lineWidth, blend, cull); }
        lineWidth = 1;
        model = parent1;
        tint = new float[] { 1, 1, 1, 1 };
        if (logo == null) {
            logo = new Mesh("title-logo");
            logo.Vertex(470, 380, 0, new float[] { 0, 0, 0, 1 });
            logo.Vertex(598, 380, 0, new float[] { 1, 0, 0, 1 });
            logo.Vertex(598, 428, 0, new float[] { 1, 1, 0, 1 });
            logo.Vertex(470, 428, 0, new float[] { 0, 1, 0, 1 });
            logo.Quads(0, 4);
        }
        TtRender.Draw(logo, model, new float[] { 1, 1, 1, 1 }, lineWidth, blend, cull, TtData.title);
        float cx = 0, cy = 0;
        for (int i = 0; i < Ship.GRADE_NUM; i++)
        {
            lineWidth = 2;
            {
                Vector cursor = calcCursorPos(i, 1);
                cx = cursor.x;
                cy = cursor.y;
            }

            drawCursorRing(model, tint, blend, cull, lineWidth, cx, cy, 15);
            Letter.drawString(model, tint, blend, cull, lineWidth, Ship.GRADE_LETTER[i], cx - 4, cy - 10, 7);
            lineWidth = 1;
            int ml = prefManager.prefData.getMaxLevel(i);
            if (ml > 1)
            {
                float ecx = 0, ecy = 0;
                {
                    Vector cursor = calcCursorPos(i, ml);
                    ecx = cursor.x;
                    ecy = cursor.y;
                }

                drawCursorRing(model, tint, blend, cull, lineWidth, ecx, ecy, 15);
                Letter.drawNum(model, tint, blend, cull, lineWidth, ml, ecx + 7, ecy - 8, 6);
                float l2cx = 0, l2cy = 0;
                {
                    Vector cursor = calcCursorPos(i, 2);
                    l2cx = cursor.x;
                    l2cy = cursor.y;
                }

                var part3 = new Mesh("Title-drawFront-3" + "-" + i.ToString());
                part3.Vertex(cx - 29, cy + 7, 0, tint);
                part3.Vertex(l2cx - 29, l2cy + 7, 0, tint);
                part3.Vertex(l2cx - 29, l2cy + 7, 0, tint);
                part3.Vertex(ecx - 29, ecy + 7, 0, tint);
                part3.Vertex(cx + 29, cy - 7, 0, tint);
                part3.Vertex(l2cx + 29, l2cy - 7, 0, tint);
                part3.Vertex(l2cx + 29, l2cy - 7, 0, tint);
                part3.Vertex(ecx + 29, ecy - 7, 0, tint);
                for (int vi = 0; vi + 1 < part3.vertexCount; vi += 2) part3.Line(vi, vi + 1);
        if (part3.count > 0) TtRender.Draw(part3, model, tint, lineWidth, blend, cull);
            }
        }

        Letter.drawString(model, tint, blend, cull, lineWidth, Ship.GRADE_STR[grade], 560 - Ship.GRADE_STR[grade].Length * 19, 4, 9);
        Letter.drawNum(model, tint, blend, cull, lineWidth, level, 620, 10, 6);
        Letter.drawString(model, tint, blend, cull, lineWidth, "LV", 570, 10, 6);
        GradeData gd = prefManager.prefData.getGradeData(grade);
        Letter.drawNum(model, tint, blend, cull, lineWidth, gd.hiScore, 620, 45, 8);
        Letter.drawNum(model, tint, blend, cull, lineWidth, gd.startLevel, 408, 54, 5);
        Letter.drawNum(model, tint, blend, cull, lineWidth, gd.endLevel, 453, 54, 5);
        Letter.drawString(model, tint, blend, cull, lineWidth, "-", 423, 54, 5);
        {
            Vector cursor = calcCursorPos(grade, level);
            cx = cursor.x;
            cy = cursor.y;
        }

        drawCursorRing(model, tint, blend, cull, lineWidth, cx, cy, 18 + sin(cnt * 0.1f) * 3);
    }

    public Vector calcCursorPos(int gd, int lv)
    {
        float x = 460 + gd * 70;
        float y = 90;
        if (lv > 1)
        {
            y = y + (30 + lv);
            x = x - (lv * 0.33f);
        }

        return new Vector(x, y);
    }

    public void drawCursorRing(float[] model, float[] tint, Gfx.Blend blend, Gfx.Cull cull, float lineWidth, float x, float y, float s)
    {
        float[] parent1 = model;
        model = Transform.Translate(model, x, y, 0);
        model = Transform.Rotate(model, -20, 0, 0, 1);
        model = Transform.Scale(model, s * 2, s, 1);
        { Mesh shape2 = meshes[2]; if (shape2.count > 0) TtRender.Draw(shape2, model, tint, lineWidth, blend, cull); }
        model = parent1;
    }

    public void createTorusShape()
    {
        float[] model = Transform.Identity(); float[] tint = null; Mesh mesh = null; int meshIndex = 0;
        Vector3 cp = new Vector3();
        cp.z = 0;
        Vector3 ringOfs = new Vector3();
        float torusRad = 5;
        float ringRad = 0.7f;
        meshes = new Mesh[3];
        mesh = new Mesh("TitleManager-" + nextMesh.ToString()); nextMesh++; meshes[meshIndex] = mesh; model = Transform.Identity(); tint = null;
        float d1 = 0;
        for (int i = 0; i < 32; i++, d1 = d1 + (PI * 2 / 32))
        {
            float d2 = 0;
            for (int j = 0; j < 16; j++, d2 = d2 + (PI * 2 / 16))
            {
                cp.x = sin(d1) * torusRad;
                cp.y = cos(d1) * torusRad;
                int part1 = mesh.vertexCount;
                createRingOffset(ringOfs, cp, ringRad, d1, d2);
                mesh.Vertex(ringOfs.x, ringOfs.y, ringOfs.z, tint, model);
                createRingOffset(ringOfs, cp, ringRad, d1, d2 + PI * 2 / 16);
                mesh.Vertex(ringOfs.x, ringOfs.y, ringOfs.z, tint, model);
                cp.x = sin(d1 + PI * 2 / 32) * torusRad;
                cp.y = cos(d1 + PI * 2 / 32) * torusRad;
                createRingOffset(ringOfs, cp, ringRad, d1 + PI * 2 / 32, d2 + PI * 2 / 16);
                mesh.Vertex(ringOfs.x, ringOfs.y, ringOfs.z, tint, model);
                mesh.LineStrip(part1, mesh.vertexCount - part1);
            }
        }

        meshIndex++; mesh = new Mesh("TitleManager-" + nextMesh.ToString()); nextMesh++; meshes[meshIndex] = mesh; model = Transform.Identity(); tint = null;
        d1 = 0;
        int part2 = mesh.vertexCount;
        for (int i = 0; i < 32; i++, d1 = d1 + (PI * 2 / 32))
        {
            cp.x = sin(d1) * (torusRad + ringRad);
            cp.y = cos(d1) * (torusRad + ringRad);
            mesh.Vertex(cp.x, cp.y, cp.z, tint, model);
            cp.x = sin(d1) * (torusRad + ringRad * 10);
            cp.y = cos(d1) * (torusRad + ringRad * 10);
            mesh.Vertex(cp.x, cp.y, cp.z, tint, model);
            cp.x = sin(d1 + PI * 2 / 32) * (torusRad + ringRad * 10);
            cp.y = cos(d1 + PI * 2 / 32) * (torusRad + ringRad * 10);
            mesh.Vertex(cp.x, cp.y, cp.z, tint, model);
            cp.x = sin(d1 + PI * 2 / 32) * (torusRad + ringRad);
            cp.y = cos(d1 + PI * 2 / 32) * (torusRad + ringRad);
            mesh.Vertex(cp.x, cp.y, cp.z, tint, model);
        }

        d1 = 0;
        for (int i = 0; i < 32; i++, d1 = d1 + (PI * 2 / 32))
        {
            float d2 = 0;
            for (int j = 0; j < 16; j++, d2 = d2 + (PI * 2 / 16))
            {
                cp.x = sin(d1) * torusRad;
                cp.y = cos(d1) * torusRad;
                createRingOffset(ringOfs, cp, ringRad, d1, d2);
                mesh.Vertex(ringOfs.x, ringOfs.y, ringOfs.z, tint, model);
                createRingOffset(ringOfs, cp, ringRad, d1, d2 + PI * 2 / 16);
                mesh.Vertex(ringOfs.x, ringOfs.y, ringOfs.z, tint, model);
                cp.x = sin(d1 + PI * 2 / 32) * torusRad;
                cp.y = cos(d1 + PI * 2 / 32) * torusRad;
                createRingOffset(ringOfs, cp, ringRad, d1 + PI * 2 / 32, d2 + PI * 2 / 16);
                mesh.Vertex(ringOfs.x, ringOfs.y, ringOfs.z, tint, model);
                createRingOffset(ringOfs, cp, ringRad, d1 + PI * 2 / 32, d2);
                mesh.Vertex(ringOfs.x, ringOfs.y, ringOfs.z, tint, model);
            }
        }

        mesh.Quads(part2, mesh.vertexCount - part2);
        meshIndex++; mesh = new Mesh("TitleManager-" + nextMesh.ToString()); nextMesh++; meshes[meshIndex] = mesh; model = Transform.Identity(); tint = null;
        d1 = 0;
        tint = new float[] { 1, 1, 1, 1 };
        int part3 = mesh.vertexCount;
        for (int i = 0; i < 128; i++, d1 = d1 + (PI * 2 / 128))
        {
            cp.x = sin(d1);
            cp.y = cos(d1);
            mesh.Vertex(cp.x, cp.y, cp.z, tint, model);
        }

        mesh.LineStrip(part3, mesh.vertexCount - part3, true);
        tint = new float[] { 1, 1, 1, 0.3f };
        int part4 = mesh.vertexCount;
        mesh.Vertex(0, 0, 0, tint, model);
        for (int i = 0; i <= 128; i++, d1 = d1 + (PI * 2 / 128))
        {
            cp.x = sin(d1);
            cp.y = cos(d1);
            mesh.Vertex(cp.x, cp.y, cp.z, tint, model);
        }

        mesh.Fan(part4, mesh.vertexCount - part4);

    }

    public void createRingOffset(Vector3 ringOfs, Vector3 centerPos, float rad, float d1, float d2)
    {
        ringOfs.x = 0;
        ringOfs.y = rad;
        ringOfs.z = 0;
        ringOfs.rollX(d2);
        ringOfs.rollZ(-d1);
        ringOfs.opAddAssign(centerPos);
    }

    public bool replayMode
    {
        get
        {
            return _replayMode;
        }
    }

    public float replayChangeRatio
    {
        get
        {
            return _replayChangeRatio;
        }
    }
}
