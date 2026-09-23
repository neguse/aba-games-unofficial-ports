// Copyright 2004 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Drawing;

public class TitleManager
{
    public const int REPLAY_CHANGE_DURATION = 30;
    public const int AUTO_REPEAT_START_TIME = 30;
    public const int AUTO_REPEAT_CNT = 5;
    public PrefManager prefManager;
    public Pad pad;
    public Ship ship;
    public GameManager gameManager;
    public DisplayList displayList;
    public int cnt;
    public int grade, level;
    public bool dirPressed, btnPressed;
    public int keyRepeatCnt;
    public int replayCnt;
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
        displayList.close();
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
                    keyRepeatCnt++;
                    if (keyRepeatCnt >= AUTO_REPEAT_START_TIME)
                    {
                        if (keyRepeatCnt % AUTO_REPEAT_CNT == 0)
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

        cnt++;
        if (_replayMode)
        {
            if (replayCnt < REPLAY_CHANGE_DURATION)
                replayCnt++;
        }
        else
        {
            if (replayCnt > 0)
                replayCnt--;
        }

        _replayChangeRatio = (float)replayCnt / REPLAY_CHANGE_DURATION;
    }

    public void draw()
    {
        if (_replayChangeRatio >= 1.0f)
            return;
        glPopMatrix();
        TtScreen.viewOrthoFixed();
        glDisable(GL_BLEND);
        TtScreen.setColor(0, 0, 0);
        float rcr = _replayChangeRatio * 2;
        if (rcr > 1)
            rcr = 1;
        glBegin(GL_QUADS);
        glVertex3f(450 + (640 - 450) * rcr, 0, 0);
        glVertex3f(640, 0, 0);
        glVertex3f(640, 480, 0);
        glVertex3f(450 + (640 - 450) * rcr, 480, 0);
        glEnd();
        glEnable(GL_BLEND);
        TtScreen.viewPerspective();
        glPushMatrix();
        TtScreen.lookAt(0, 0, -1, 0, 0, 0, 0, 1, 0);
        glPushMatrix();
        glTranslatef(3 - _replayChangeRatio * 2.4f, 1.8f, 3.5f - _replayChangeRatio * 1.5f);
        glRotatef(30, 1, 0, 0);
        glRotatef(sin(cnt * 0.005f) * 12, 0, 1, 0);
        glRotatef(cnt * 0.2f, 0, 0, 1);
        glDisable(GL_BLEND);
        TtScreen.setColor(0, 0, 0);
        displayList.call(1);
        glEnable(GL_BLEND);
        TtScreen.setColor(1, 1, 1, 0.5f);
        displayList.call(0);
        glPopMatrix();
    }

    public void drawFront()
    {
        if (_replayChangeRatio > 0)
            return;
        glPushMatrix();
        glTranslatef(508, 400, 0);
        glRotatef(-20, 0, 0, 1);
        glScalef(128, 64, 1);
        glLineWidth(2);
        displayList.call(2);
        glLineWidth(1);
        glPopMatrix();
        TtScreen.setColor(1, 1, 1);
        TtData.drawTitle();
        float cx = 0, cy = 0;
        for (int i = 0; i < Ship.GRADE_NUM; i++)
        {
            glLineWidth(2);
            {
                Vector cursor = calcCursorPos(i, 1);
                cx = cursor.x;
                cy = cursor.y;
            }

            drawCursorRing(cx, cy, 15);
            Letter.drawString(Ship.GRADE_LETTER[i], cx - 4, cy - 10, 7);
            glLineWidth(1);
            int ml = prefManager.prefData.getMaxLevel(i);
            if (ml > 1)
            {
                float ecx = 0, ecy = 0;
                {
                    Vector cursor = calcCursorPos(i, ml);
                    ecx = cursor.x;
                    ecy = cursor.y;
                }

                drawCursorRing(ecx, ecy, 15);
                Letter.drawNum(ml, ecx + 7, ecy - 8, 6);
                float l2cx = 0, l2cy = 0;
                {
                    Vector cursor = calcCursorPos(i, 2);
                    l2cx = cursor.x;
                    l2cy = cursor.y;
                }

                glBegin(GL_LINES);
                glVertex3f(cx - 29, cy + 7, 0);
                glVertex3f(l2cx - 29, l2cy + 7, 0);
                glVertex3f(l2cx - 29, l2cy + 7, 0);
                glVertex3f(ecx - 29, ecy + 7, 0);
                glVertex3f(cx + 29, cy - 7, 0);
                glVertex3f(l2cx + 29, l2cy - 7, 0);
                glVertex3f(l2cx + 29, l2cy - 7, 0);
                glVertex3f(ecx + 29, ecy - 7, 0);
                glEnd();
            }
        }

        Letter.drawString(Ship.GRADE_STR[grade], 560 - Ship.GRADE_STR[grade].Length * 19, 4, 9);
        Letter.drawNum(level, 620, 10, 6);
        Letter.drawString("LV", 570, 10, 6);
        GradeData gd = prefManager.prefData.getGradeData(grade);
        Letter.drawNum(gd.hiScore, 620, 45, 8);
        Letter.drawNum(gd.startLevel, 408, 54, 5);
        Letter.drawNum(gd.endLevel, 453, 54, 5);
        Letter.drawString("-", 423, 54, 5);
        {
            Vector cursor = calcCursorPos(grade, level);
            cx = cursor.x;
            cy = cursor.y;
        }

        drawCursorRing(cx, cy, 18 + sin(cnt * 0.1f) * 3);
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

    public void drawCursorRing(float x, float y, float s)
    {
        glPushMatrix();
        glTranslatef(x, y, 0);
        glRotatef(-20, 0, 0, 1);
        glScalef(s * 2, s, 1);
        displayList.call(2);
        glPopMatrix();
    }

    public void createTorusShape()
    {
        Vector3 cp = new Vector3();
        cp.z = 0;
        Vector3 ringOfs = new Vector3();
        float torusRad = 5;
        float ringRad = 0.7f;
        displayList = new DisplayList(3);
        displayList.beginNewList();
        float d1 = 0;
        for (int i = 0; i < 32; i++, d1 = d1 + (PI * 2 / 32))
        {
            float d2 = 0;
            for (int j = 0; j < 16; j++, d2 = d2 + (PI * 2 / 16))
            {
                cp.x = sin(d1) * torusRad;
                cp.y = cos(d1) * torusRad;
                glBegin(GL_LINE_STRIP);
                createRingOffset(ringOfs, cp, ringRad, d1, d2);
                TtScreen.glVertex(ringOfs);
                createRingOffset(ringOfs, cp, ringRad, d1, d2 + PI * 2 / 16);
                TtScreen.glVertex(ringOfs);
                cp.x = sin(d1 + PI * 2 / 32) * torusRad;
                cp.y = cos(d1 + PI * 2 / 32) * torusRad;
                createRingOffset(ringOfs, cp, ringRad, d1 + PI * 2 / 32, d2 + PI * 2 / 16);
                TtScreen.glVertex(ringOfs);
                glEnd();
            }
        }

        displayList.nextNewList();
        d1 = 0;
        glBegin(GL_QUADS);
        for (int i = 0; i < 32; i++, d1 = d1 + (PI * 2 / 32))
        {
            cp.x = sin(d1) * (torusRad + ringRad);
            cp.y = cos(d1) * (torusRad + ringRad);
            TtScreen.glVertex(cp);
            cp.x = sin(d1) * (torusRad + ringRad * 10);
            cp.y = cos(d1) * (torusRad + ringRad * 10);
            TtScreen.glVertex(cp);
            cp.x = sin(d1 + PI * 2 / 32) * (torusRad + ringRad * 10);
            cp.y = cos(d1 + PI * 2 / 32) * (torusRad + ringRad * 10);
            TtScreen.glVertex(cp);
            cp.x = sin(d1 + PI * 2 / 32) * (torusRad + ringRad);
            cp.y = cos(d1 + PI * 2 / 32) * (torusRad + ringRad);
            TtScreen.glVertex(cp);
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
                TtScreen.glVertex(ringOfs);
                createRingOffset(ringOfs, cp, ringRad, d1, d2 + PI * 2 / 16);
                TtScreen.glVertex(ringOfs);
                cp.x = sin(d1 + PI * 2 / 32) * torusRad;
                cp.y = cos(d1 + PI * 2 / 32) * torusRad;
                createRingOffset(ringOfs, cp, ringRad, d1 + PI * 2 / 32, d2 + PI * 2 / 16);
                TtScreen.glVertex(ringOfs);
                createRingOffset(ringOfs, cp, ringRad, d1 + PI * 2 / 32, d2);
                TtScreen.glVertex(ringOfs);
            }
        }

        glEnd();
        displayList.nextNewList();
        d1 = 0;
        TtScreen.setColor(1, 1, 1);
        glBegin(GL_LINE_LOOP);
        for (int i = 0; i < 128; i++, d1 = d1 + (PI * 2 / 128))
        {
            cp.x = sin(d1);
            cp.y = cos(d1);
            TtScreen.glVertex(cp);
        }

        glEnd();
        TtScreen.setColor(1, 1, 1, 0.3f);
        glBegin(GL_TRIANGLE_FAN);
        glVertex3f(0, 0, 0);
        for (int i = 0; i <= 128; i++, d1 = d1 + (PI * 2 / 128))
        {
            cp.x = sin(d1);
            cp.y = cos(d1);
            TtScreen.glVertex(cp);
        }

        glEnd();
        displayList.endNewList();
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
