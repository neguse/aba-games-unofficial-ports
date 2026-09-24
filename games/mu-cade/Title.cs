// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public class TitleManager
{
    public Field field;
    public StageManager stageManager;
    public PrefManager prefManager;
    public int cnt;
    public TitleManager(Field field, StageManager stageManager, PrefManager prefManager)
    {
        this.field = field;
        this.stageManager = stageManager;
        this.prefManager = prefManager;
    }

    public virtual void start_0()
    {
        cnt = 0;
    }

    public virtual void move_0()
    {
        cnt++;
    }

    public virtual void draw(float[] model, float[] tint, Gfx.Blend blend, string key, Mesh target = null)
    {
        float x = 250, y = 50;
        float lsz = 50, lof = 40;
        for (int i = 0; i < 5; i++)
        {
            tint = new float[] { 1, 1, 1, 1 };
            bool mask = true;
            field.drawLetter_4(model, tint, blend, key + "-drawLetter_4-1" + "-" + i.ToString(), i, x, y, lsz, mask);
            blend = Gfx.Blend.Additive;
            tint = new float[] { 1, 1, 1, 1 };
            mask = false;
            field.drawLetter_4(model, tint, blend, key + "-drawLetter_4-2" + "-" + i.ToString(), i, x, y, lsz, mask);
            if (i == 0)
                x += lof * 1.0f;
            else
                x += lof * 0.9f;
        }

        blend = Gfx.Blend.Additive;
        if ((cnt % 120) < 60)
            Letter.drawString(model, tint, blend, key + "-drawString-1", "PUSH SHOT BUTTON TO START", 200, 430, 5);
        if ((cnt % 3600) == 0)
            stageManager.initRank();
        drawRanking(model, tint, blend, key + "-drawRanking-1");
    }

    public virtual void drawRanking(float[] model, float[] tint, Gfx.Blend blend, string key)
    {
        int rn = GameMath.integer((cnt - 60) / 40);
        if (rn > PrefData.RANKING_NUM)
            rn = PrefData.RANKING_NUM;
        float y = 120;
        for (int i = 0; i < rn; i++)
        {
            string rstr = default(string);
            switch (i)
            {
                case 0:
                    rstr = "1ST";
                    break;
                case 1:
                    rstr = "2ND";
                    break;
                case 2:
                    rstr = "3RD";
                    break;
                default:
                    rstr = (i + 1).ToString() + "TH";
                    break;
            }

            if (i < 9)
                Letter.drawString(model, tint, blend, key + "-drawString-1" + "-" + i.ToString(), rstr, 80, y, 7);
            else
                Letter.drawString(model, tint, blend, key + "-drawString-2" + "-" + i.ToString(), rstr, 66, y, 7);
            Letter.drawNum(model, tint, blend, key + "-drawNum-1" + "-" + i.ToString(), prefManager.prefData.highScore[i], 400, y, 6);
            Letter.drawTime(model, tint, blend, key + "-drawTime-1" + "-" + i.ToString(), prefManager.prefData.time[i], 600, y + 6, 6);
            y += 24;
        }
    }
}
