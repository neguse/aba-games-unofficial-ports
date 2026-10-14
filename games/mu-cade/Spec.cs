// Copyright 2006 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public class CentBarrage
{
    public int type;
    public float speedRank;
    public int interval;
    public string wayMorphBml;
    public float wayMorphRank;
    public string barMorphBml;
    public float barMorphRank;
    public void set_4_BulletPool_Ship_EnemyState_Single(BulletPool bullets, Ship ship, EnemyState state, float orderRatio = 0)
    {
        state.barrage.clear();
        switch (type)
        {
            case CentBarrageBasicBarrageType.AIM:
            case CentBarrageBasicBarrageType.FRONT:
            case CentBarrageBasicBarrageType.PLUMB:
            case CentBarrageBasicBarrageType.AIM_IN_ORDER:
                state.barrage.addBml_4("basic", "straight.xml", 1, speedRank);
                break;
            case CentBarrageBasicBarrageType.SIDE:
                state.barrage.addBml_4("basic", "side.xml", 1, speedRank);
                break;
            case CentBarrageBasicBarrageType.ONE_SIDE:
                state.barrage.addBml_4("basic", "sideoneway.xml", 1, speedRank);
                break;
        }

        if ((wayMorphBml) != null)
            state.barrage.addBml_4("waymorph", wayMorphBml, wayMorphRank, speedRank);
        if ((barMorphBml) != null)
            state.barrage.addBml_4("barmorph", barMorphBml, barMorphRank, speedRank);
        int sc = 0;
        if (type == CentBarrageBasicBarrageType.AIM_IN_ORDER)
            sc = GameMath.integer((interval * orderRatio));
        state.barrage.setWait(sc, interval);
        state.topBullet = state.barrage.addTopBullet_3(bullets, ship);
        if (!((state.topBullet) != null))
            return;
        state.topBullet.activated = false;
        switch (type)
        {
            case CentBarrageBasicBarrageType.AIM:
            case CentBarrageBasicBarrageType.AIM_IN_ORDER:
                break;
            case CentBarrageBasicBarrageType.FRONT:
            case CentBarrageBasicBarrageType.SIDE:
            case CentBarrageBasicBarrageType.ONE_SIDE:
                state.setTopBulletDirection = true;
                state.topBullet.unsetAimTop();
                break;
            case CentBarrageBasicBarrageType.PLUMB:
                state.setPlumbDirection = true;
                state.topBullet.unsetAimTop();
                break;
        }
    }
}

public static class CentHeadToAndFrom
{
    public static void initLengthAndSize(CentHead h, int size)
    {
        float ss = default(float);
        switch (size)
        {
            case 0:
                h.bodyLength = 4 + EnemySpecBase.rand.nextInt(4);
                ss = 0.9f + EnemySpecBase.rand.nextFloat(0.3f);
                break;
            case 1:
                h.bodyLength = 5 + EnemySpecBase.rand.nextInt(3);
                ss = 1.3f + EnemySpecBase.rand.nextFloat(0.2f);
                break;
            case 2:
                h.bodyLength = 7 + EnemySpecBase.rand.nextInt(2);
                ss = 1.6f + EnemySpecBase.rand.nextFloat(0.1f);
                break;
        }

        h.baseSize = size;
        h.sizeScale = new Vector3(ss, ss, 1);
        h.massScale = ss * ss;
    }

    public static float calcBarrageRank(float br)
    {
        if (br < 0.01f)
            return 0;
        else
            return 1 - 1 / sqrt(br);
    }

    public static float[] calcBarrageSpeedAndInterval(float rank, float br, float minInterval = 20, float maxInterval = 120)
    {
        float speed = 0;
        int interval = 0;
        float sr = br * (0.5f + EnemySpecBase.rand.nextSignedFloat(0.2f));
        float ir = br - sr;
        if (sr < 1)
        {
            sr = 1;
        }
        else
        {
            rank -= sr;
            sr = 2.5f - 1.5f / sqrt(sr);
        }

        speed = sr;
        if (ir < 1)
            ir = 1;
        else
            rank -= ir;
        interval = GameMath.integer((minInterval + (maxInterval - minInterval) / ir));
        return new float[]
        {
            rank,
            speed,
            interval
        };
    }

    public static float[] calcForwardForceAndSlowVelocity(CentHead h, float rank, int size)
    {
        float forwardForceScale = 0;
        float slowVelocityRatio = 0;
        forwardForceScale = 1;
        if (EnemySpecBase.rand.nextInt(5) == 0)
        {
            float ff = rank * (0.1f + EnemySpecBase.rand.nextFloat(0.3f - size * 0.1f));
            if (ff < 0)
                ff = 0;
            forwardForceScale = 1 + sqrt(ff);
            rank -= ff * 0.2f;
        }

        forwardForceScale *= (float)h.bodyLength / 5;
        slowVelocityRatio = 1;
        if (size >= 1)
        {
            float sv = rank * (0.1f + EnemySpecBase.rand.nextFloat(0.1f)) * size;
            if (sv < 0)
                sv = 0;
            slowVelocityRatio = 1 + sv * 3;
            rank -= sv * 0.2f;
        }

        return new float[]
        {
            rank,
            forwardForceScale,
            slowVelocityRatio
        };
    }

    public const float COLOR_R = 0.5f;
    public const float COLOR_G = 0.25f;
    public const float COLOR_B = 1.0f;
    public static readonly string[] BAR_MORPH_BML = new string[]
    {
        "baraccel.xml",
        "whip.xml",
        "slidebar.xml",
        "slidebaraccel.xml",
        "slidewhip.xml"
    };
    public static CentHead create(Field field, Ship ship, BulletPool bullets, World world, float rank, int size)
    {
        CentHead h = new CentHead(field, ship, bullets, world, CentHeadCentType.TO_AND_FROM);
        h.headSpec = h;
        h.bodySpec = new CentBody(field, ship, bullets, world, h, CentBodyCentType.TO_AND_FROM, COLOR_R, COLOR_G, COLOR_B);
        initLengthAndSize(h, size);
        float rk = rank;
        if (size <= 0)
        {
            h.bodyBarrage = null;
        }
        else
        {
            h.bodyBarrage = new CentBarrage();
            if (EnemySpecBase.rand.nextInt(2) == 0)
                h.bodyBarrage.type = CentBarrageBasicBarrageType.PLUMB;
            else
                h.bodyBarrage.type = CentBarrageBasicBarrageType.SIDE;
            h.bodyBarrage.wayMorphBml = null;
            float br_0 = rk * (0.1f * size + EnemySpecBase.rand.nextFloat(0.1f));
            float brv_0 = calcBarrageRank(br_0);
            if (brv_0 >= 0.1f)
            {
                h.bodyBarrage.barMorphBml = BAR_MORPH_BML[EnemySpecBase.rand.nextInt(BAR_MORPH_BML.Length)];
                h.bodyBarrage.barMorphRank = brv_0;
                rk -= br_0 * 2;
            }
            else
            {
                h.bodyBarrage.barMorphBml = null;
            }

            float sp_0 = default(float);
            int iv_0 = default(int);
            {
                float[] values_0 = calcBarrageSpeedAndInterval(rk, rk * 0.25f, 20, 150);
                rk = values_0[0];
                sp_0 = values_0[1];
                iv_0 = integer(values_0[2]);
            }

            h.bodyBarrage.speedRank = sp_0;
            h.bodyBarrage.interval = iv_0;
        }

        h.headBarrage = new CentBarrage();
        h.headBarrage.type = CentBarrageBasicBarrageType.FRONT;
        h.headBarrage.wayMorphBml = null;
        float br_1 = rk * (0.1f * size + EnemySpecBase.rand.nextFloat(0.3f));
        float brv_1 = calcBarrageRank(br_1);
        if (brv_1 >= 0.1f)
        {
            h.headBarrage.barMorphBml = BAR_MORPH_BML[EnemySpecBase.rand.nextInt(BAR_MORPH_BML.Length)];
            h.headBarrage.barMorphRank = brv_1;
            rk -= br_1 * 2;
        }
        else
        {
            h.headBarrage.barMorphBml = null;
        }

        float ffs = default(float), svr = default(float);
        {
            float[] values_1 = calcForwardForceAndSlowVelocity(h, rk, size);
            rk = values_1[0];
            ffs = values_1[1];
            svr = values_1[2];
        }

        h.forwardForceScale = ffs;
        h.slowVelocityRatio = svr;
        float sp_1 = default(float);
        int iv_1 = default(int);
        switch (size)
        {
            case 0:
            {
                float[] values_2 = calcBarrageSpeedAndInterval(rk, rk);
                rk = values_2[0];
                sp_1 = values_2[1];
                iv_1 = integer(values_2[2]);
            }

                break;
            case 1:
            {
                float[] values_3 = calcBarrageSpeedAndInterval(rk, rk, 10, 90);
                rk = values_3[0];
                sp_1 = values_3[1];
                iv_1 = integer(values_3[2]);
            }

                break;
            case 2:
            {
                float[] values_4 = calcBarrageSpeedAndInterval(rk, rk, 5, 60);
                rk = values_4[0];
                sp_1 = values_4[1];
                iv_1 = integer(values_4[2]);
            }

                break;
        }

        h.headBarrage.speedRank = sp_1;
        h.headBarrage.interval = iv_1;
        h._colorR = COLOR_R;
        h._colorG = COLOR_G;
        h._colorB = COLOR_B;
        return h;
    }
}

public static class CentHeadChase
{
    public static void initLengthAndSize(CentHead h, int size)
    {
        float ss = default(float);
        switch (size)
        {
            case 0:
                h.bodyLength = 4 + EnemySpecBase.rand.nextInt(4);
                ss = 0.9f + EnemySpecBase.rand.nextFloat(0.3f);
                break;
            case 1:
                h.bodyLength = 5 + EnemySpecBase.rand.nextInt(3);
                ss = 1.3f + EnemySpecBase.rand.nextFloat(0.2f);
                break;
            case 2:
                h.bodyLength = 7 + EnemySpecBase.rand.nextInt(2);
                ss = 1.6f + EnemySpecBase.rand.nextFloat(0.1f);
                break;
        }

        h.baseSize = size;
        h.sizeScale = new Vector3(ss, ss, 1);
        h.massScale = ss * ss;
    }

    public static float calcBarrageRank(float br)
    {
        if (br < 0.01f)
            return 0;
        else
            return 1 - 1 / sqrt(br);
    }

    public static float[] calcBarrageSpeedAndInterval(float rank, float br, float minInterval = 20, float maxInterval = 120)
    {
        float speed = 0;
        int interval = 0;
        float sr = br * (0.5f + EnemySpecBase.rand.nextSignedFloat(0.2f));
        float ir = br - sr;
        if (sr < 1)
        {
            sr = 1;
        }
        else
        {
            rank -= sr;
            sr = 2.5f - 1.5f / sqrt(sr);
        }

        speed = sr;
        if (ir < 1)
            ir = 1;
        else
            rank -= ir;
        interval = GameMath.integer((minInterval + (maxInterval - minInterval) / ir));
        return new float[]
        {
            rank,
            speed,
            interval
        };
    }

    public static float[] calcForwardForceAndSlowVelocity(CentHead h, float rank, int size)
    {
        float forwardForceScale = 0;
        float slowVelocityRatio = 0;
        forwardForceScale = 1;
        if (EnemySpecBase.rand.nextInt(5) == 0)
        {
            float ff = rank * (0.1f + EnemySpecBase.rand.nextFloat(0.3f - size * 0.1f));
            if (ff < 0)
                ff = 0;
            forwardForceScale = 1 + sqrt(ff);
            rank -= ff * 0.2f;
        }

        forwardForceScale *= (float)h.bodyLength / 5;
        slowVelocityRatio = 1;
        if (size >= 1)
        {
            float sv = rank * (0.1f + EnemySpecBase.rand.nextFloat(0.1f)) * size;
            if (sv < 0)
                sv = 0;
            slowVelocityRatio = 1 + sv * 3;
            rank -= sv * 0.2f;
        }

        return new float[]
        {
            rank,
            forwardForceScale,
            slowVelocityRatio
        };
    }

    public const float COLOR_R = 0.75f;
    public const float COLOR_G = 0.25f;
    public const float COLOR_B = 0.75f;
    public static readonly string[] WAY_MORPH_BML = new string[]
    {
        "accnway.xml",
        "decnway.xml",
        "nway.xml"
    };
    public static readonly string[] BAR_MORPH_BML = new string[]
    {
        "bar.xml",
        "baraccel.xml",
        "whip.xml"
    };
    public static CentHead create(Field field, Ship ship, BulletPool bullets, World world, float rank, int size)
    {
        CentHead h = new CentHead(field, ship, bullets, world, CentHeadCentType.CHASE);
        h.headSpec = h;
        h.bodySpec = new CentBody(field, ship, bullets, world, h, CentBodyCentType.CHASE, COLOR_R, COLOR_G, COLOR_B);
        initLengthAndSize(h, size);
        float rk = rank;
        if (size <= 1)
        {
            h.bodyBarrage = null;
        }
        else
        {
            h.bodyBarrage = new CentBarrage();
            h.bodyBarrage.type = CentBarrageBasicBarrageType.AIM_IN_ORDER;
            h.bodyBarrage.wayMorphBml = null;
            float br_0 = rk * (0.1f + EnemySpecBase.rand.nextFloat(0.1f));
            float brv_0 = calcBarrageRank(br_0);
            if (brv_0 >= 0.1f)
            {
                h.bodyBarrage.barMorphBml = BAR_MORPH_BML[EnemySpecBase.rand.nextInt(BAR_MORPH_BML.Length)];
                h.bodyBarrage.barMorphRank = brv_0;
                rk -= br_0 * 2;
            }
            else
            {
                h.bodyBarrage.barMorphBml = null;
            }

            float sp_0 = default(float);
            int iv_0 = default(int);
            {
                float[] values_0 = calcBarrageSpeedAndInterval(rk, rk * 0.2f);
                rk = values_0[0];
                sp_0 = values_0[1];
                iv_0 = integer(values_0[2]);
            }

            h.bodyBarrage.speedRank = sp_0;
            h.bodyBarrage.interval = iv_0;
        }

        h.headBarrage = new CentBarrage();
        h.headBarrage.type = CentBarrageBasicBarrageType.FRONT;
        float wr = default(float);
        switch (size)
        {
            case 0:
                wr = 0;
                break;
            case 1:
                wr = rk * (0.2f + EnemySpecBase.rand.nextFloat(0.1f));
                break;
            case 2:
                wr = rk * (0.1f + EnemySpecBase.rand.nextFloat(0.1f));
                break;
        }

        float br_1 = rk * EnemySpecBase.rand.nextFloat(0.3f);
        float wrv = calcBarrageRank(wr);
        float brv_1 = calcBarrageRank(br_1);
        if (wrv >= 0.2f)
        {
            h.headBarrage.wayMorphBml = WAY_MORPH_BML[EnemySpecBase.rand.nextInt(WAY_MORPH_BML.Length)];
            h.headBarrage.wayMorphRank = wrv;
            rk -= wr * 2;
        }
        else
        {
            h.headBarrage.wayMorphBml = null;
        }

        if (brv_1 >= 0.1f)
        {
            h.headBarrage.barMorphBml = BAR_MORPH_BML[EnemySpecBase.rand.nextInt(BAR_MORPH_BML.Length)];
            h.headBarrage.barMorphRank = brv_1;
            rk -= br_1 * 2;
        }
        else
        {
            h.headBarrage.barMorphBml = null;
        }

        float ffs = default(float), svr = default(float);
        {
            float[] values_1 = calcForwardForceAndSlowVelocity(h, rk, size);
            rk = values_1[0];
            ffs = values_1[1];
            svr = values_1[2];
        }

        h.forwardForceScale = ffs;
        h.slowVelocityRatio = svr;
        float sp_1 = default(float);
        int iv_1 = default(int);
        if (size <= 0)
        {
            float[] values_2 = calcBarrageSpeedAndInterval(rk, rk);
            rk = values_2[0];
            sp_1 = values_2[1];
            iv_1 = integer(values_2[2]);
        }
        else
        {
            float[] values_3 = calcBarrageSpeedAndInterval(rk, rk, 10, 90);
            rk = values_3[0];
            sp_1 = values_3[1];
            iv_1 = integer(values_3[2]);
        }

        h.headBarrage.speedRank = sp_1;
        h.headBarrage.interval = iv_1;
        h._colorR = COLOR_R;
        h._colorG = COLOR_G;
        h._colorB = COLOR_B;
        return h;
    }
}

public static class CentHeadRoll
{
    public static void initLengthAndSize(CentHead h, int size)
    {
        float ss = default(float);
        switch (size)
        {
            case 0:
                h.bodyLength = 4 + EnemySpecBase.rand.nextInt(4);
                ss = 0.9f + EnemySpecBase.rand.nextFloat(0.3f);
                break;
            case 1:
                h.bodyLength = 5 + EnemySpecBase.rand.nextInt(3);
                ss = 1.3f + EnemySpecBase.rand.nextFloat(0.2f);
                break;
            case 2:
                h.bodyLength = 7 + EnemySpecBase.rand.nextInt(2);
                ss = 1.6f + EnemySpecBase.rand.nextFloat(0.1f);
                break;
        }

        h.baseSize = size;
        h.sizeScale = new Vector3(ss, ss, 1);
        h.massScale = ss * ss;
    }

    public static float calcBarrageRank(float br)
    {
        if (br < 0.01f)
            return 0;
        else
            return 1 - 1 / sqrt(br);
    }

    public static float[] calcBarrageSpeedAndInterval(float rank, float br, float minInterval = 20, float maxInterval = 120)
    {
        float speed = 0;
        int interval = 0;
        float sr = br * (0.5f + EnemySpecBase.rand.nextSignedFloat(0.2f));
        float ir = br - sr;
        if (sr < 1)
        {
            sr = 1;
        }
        else
        {
            rank -= sr;
            sr = 2.5f - 1.5f / sqrt(sr);
        }

        speed = sr;
        if (ir < 1)
            ir = 1;
        else
            rank -= ir;
        interval = GameMath.integer((minInterval + (maxInterval - minInterval) / ir));
        return new float[]
        {
            rank,
            speed,
            interval
        };
    }

    public static float[] calcForwardForceAndSlowVelocity(CentHead h, float rank, int size)
    {
        float forwardForceScale = 0;
        float slowVelocityRatio = 0;
        forwardForceScale = 1;
        if (EnemySpecBase.rand.nextInt(5) == 0)
        {
            float ff = rank * (0.1f + EnemySpecBase.rand.nextFloat(0.3f - size * 0.1f));
            if (ff < 0)
                ff = 0;
            forwardForceScale = 1 + sqrt(ff);
            rank -= ff * 0.2f;
        }

        forwardForceScale *= (float)h.bodyLength / 5;
        slowVelocityRatio = 1;
        if (size >= 1)
        {
            float sv = rank * (0.1f + EnemySpecBase.rand.nextFloat(0.1f)) * size;
            if (sv < 0)
                sv = 0;
            slowVelocityRatio = 1 + sv * 3;
            rank -= sv * 0.2f;
        }

        return new float[]
        {
            rank,
            forwardForceScale,
            slowVelocityRatio
        };
    }

    public const float COLOR_R = 0.25f;
    public const float COLOR_G = 0.75f;
    public const float COLOR_B = 0.75f;
    public static readonly string[] WAY_MORPH_BML = new string[]
    {
        "nway.xml",
        "round.xml"
    };
    public static readonly string[] BAR_MORPH_BML = new string[]
    {
        "bar.xml",
        "baraccel.xml",
        "whip.xml",
        "slidebar.xml",
        "slidebaraccel.xml",
        "slidewhip.xml"
    };
    public static CentHead create(Field field, Ship ship, BulletPool bullets, World world, float rank, int size)
    {
        CentHead h = new CentHead(field, ship, bullets, world, CentHeadCentType.ROLL);
        h.headSpec = h;
        h.bodySpec = new CentBody(field, ship, bullets, world, h, CentBodyCentType.ROLL, COLOR_R, COLOR_G, COLOR_B);
        initLengthAndSize(h, size);
        h.bodyLength = 6;
        float rk = rank;
        h.bodyBarrage = new CentBarrage();
        h.bodyBarrage.type = CentBarrageBasicBarrageType.ONE_SIDE;
        h.bodyBarrage.wayMorphBml = null;
        float br_0 = rk * (0.1f * size + EnemySpecBase.rand.nextFloat(0.1f));
        float brv_0 = calcBarrageRank(br_0);
        if (brv_0 >= 0.1f)
        {
            h.bodyBarrage.barMorphBml = BAR_MORPH_BML[EnemySpecBase.rand.nextInt(BAR_MORPH_BML.Length)];
            h.bodyBarrage.barMorphRank = brv_0;
            rk -= br_0 * 2;
        }
        else
        {
            h.bodyBarrage.barMorphBml = null;
        }

        float sp_0 = default(float);
        int iv_0 = default(int);
        switch (size)
        {
            case 0:
            {
                float[] values_0 = calcBarrageSpeedAndInterval(rk, rk * 0.5f, 10, 60);
                rk = values_0[0];
                sp_0 = values_0[1];
                iv_0 = integer(values_0[2]);
            }

                break;
            case 1:
            {
                float[] values_1 = calcBarrageSpeedAndInterval(rk, rk * 0.3f, 5, 40);
                rk = values_1[0];
                sp_0 = values_1[1];
                iv_0 = integer(values_1[2]);
            }

                break;
            case 2:
            {
                float[] values_2 = calcBarrageSpeedAndInterval(rk, rk * 0.3f, 3, 24);
                rk = values_2[0];
                sp_0 = values_2[1];
                iv_0 = integer(values_2[2]);
            }

                break;
        }

        h.bodyBarrage.speedRank = sp_0;
        h.bodyBarrage.interval = iv_0;
        if (size <= 0)
        {
            h.headBarrage = h.bodyBarrage;
            float ffs_0 = default(float), svr_0 = default(float);
            {
                float[] values_3 = calcForwardForceAndSlowVelocity(h, rk, size);
                rk = values_3[0];
                ffs_0 = values_3[1];
                svr_0 = values_3[2];
            }

            h.forwardForceScale = ffs_0;
            h.slowVelocityRatio = svr_0;
        }
        else
        {
            h.headBarrage = new CentBarrage();
            h.headBarrage.type = CentBarrageBasicBarrageType.FRONT;
            float wr = rk * (size * 0.2f + EnemySpecBase.rand.nextFloat(0.2f));
            float br_1 = rk * EnemySpecBase.rand.nextFloat(0.2f);
            float wrv = calcBarrageRank(wr);
            float brv_1 = calcBarrageRank(br_1);
            if (wrv >= 0.2f)
            {
                h.headBarrage.wayMorphBml = WAY_MORPH_BML[EnemySpecBase.rand.nextInt(WAY_MORPH_BML.Length)];
                h.headBarrage.wayMorphRank = wrv;
                rk -= wr * 2;
            }
            else
            {
                h.headBarrage.wayMorphBml = null;
            }

            if (brv_1 >= 0.1f)
            {
                h.headBarrage.barMorphBml = BAR_MORPH_BML[EnemySpecBase.rand.nextInt(BAR_MORPH_BML.Length)];
                h.headBarrage.barMorphRank = brv_1;
                rk -= br_1 * 2;
            }
            else
            {
                h.headBarrage.barMorphBml = null;
            }

            float ffs_1 = default(float), svr_1 = default(float);
            {
                float[] values_4 = calcForwardForceAndSlowVelocity(h, rk, size);
                rk = values_4[0];
                ffs_1 = values_4[1];
                svr_1 = values_4[2];
            }

            h.forwardForceScale = ffs_1;
            h.slowVelocityRatio = svr_1;
            float sp_1 = default(float);
            int iv_1 = default(int);
            {
                float[] values_5 = calcBarrageSpeedAndInterval(rk, rk, 10, 60);
                rk = values_5[0];
                sp_1 = values_5[1];
                iv_1 = integer(values_5[2]);
            }

            h.headBarrage.speedRank = sp_1;
            h.headBarrage.interval = iv_1;
        }

        h._colorR = COLOR_R;
        h._colorG = COLOR_G;
        h._colorB = COLOR_B;
        return h;
    }
}

public class CentHead : EnemySpec, JointedEnemySpec, ConnectedParticlesBodyAddable
{
    public static readonly int[][] TURN_TRG_DEG = new int[][]
    {
        new int[]
        {
            5,
            4,
            3,
            -1,
            -1,
            -1,
            3,
            4
        },
        new int[]
        {
            5,
            6,
            7,
            6,
            5,
            -1,
            -1,
            -1
        },
        new int[]
        {
            -1,
            -1,
            7,
            0,
            1,
            0,
            7,
            -1
        },
        new int[]
        {
            1,
            -1,
            -1,
            -1,
            1,
            2,
            3,
            2
        }
    };
    public const float SIZE = 1.0f;
    public const float WIDTH = 0.25f;
    public const float MASS = 1.0f;
    public const float FORWARD_FORCE_BASE = 5;
    public const float ANGULAR_FORCE_BASE = 10;
    public const float SLOW_VELOCITY_RATIO_CONST = 0.01f;
    public const float SLOW_ANGULAR_RATIO = 0.1f;
    public CentHead headSpec;
    public int bodyLength, baseSize;
    public int centType;
    public Field field;
    public Ship ship;
    public BulletPool bullets;
    public World world;
    public ShapeGroup shape;
    public Drawable subShape = null;
    public bool _rotate2d = true;
    public bool _collideBullet = false;
    public float _colorR, _colorG, _colorB;
    public bool move_2(Enemy enemy, EnemyState state)
    {
        if (field.checkInField_1_Vector3(state.pos))
        {
            if (state.isHead)
                enemy.addRelForce(0, FORWARD_FORCE_BASE * state.forwardForceScale);
            if ((state.topBullet) != null)
                state.topBullet.activated = true;
            state.linePoint.enableSpectrumColor(true);
            switch (state.type)
            {
                case CentHeadCentType.TO_AND_FROM:
                    enemy.slowLinearVel(CentHead.SLOW_VELOCITY_RATIO_CONST * state.slowVelocityRatio * (1 + (state.turnCnt * 0.1f)));
                    break;
                case CentHeadCentType.CHASE:
                    enemy.slowLinearVel(CentHead.SLOW_VELOCITY_RATIO_CONST * state.slowVelocityRatio * 0.25f);
                    break;
                case CentHeadCentType.ROLL:
                    enemy.slowLinearVel(CentHead.SLOW_VELOCITY_RATIO_CONST * state.slowVelocityRatio * 5);
                    break;
            }
        }
        else
        {
            if ((state.topBullet) != null)
                state.topBullet.activated = false;
            state.linePoint.enableSpectrumColor(false);
        }

        enemy.slowAngularVel(CentHead.SLOW_ANGULAR_RATIO);
        if (!(state.isHead))
            return true;
        if (field.checkInField_1_Vector3(state.pos))
            enemy.slowLinearVel(CentHead.SLOW_VELOCITY_RATIO_CONST * state.slowVelocityRatio);
        if (state.turnCnt > 0)
            state.turnCnt--;
        float ad = default(float);
        float angularForce = ANGULAR_FORCE_BASE;
        switch (state.type)
        {
            case CentHeadCentType.TO_AND_FROM:
                int ti = -1;
                if (state.pos.x > field.size().x * 0.5f)
                    ti = 3;
                else if (state.pos.x < -field.size().x * 0.5f)
                    ti = 1;
                if (ti >= 0 && TURN_TRG_DEG[ti][state.trgDeg] >= 0)
                {
                    state.trgDeg = TURN_TRG_DEG[ti][state.trgDeg];
                    if (state.turnCnt <= 0)
                        state.turnCnt = 60;
                }
                else
                {
                    if (state.pos.y < -field.size().y * 0.5f)
                        ti = 2;
                    else if (state.pos.y > field.size().y * 0.5f)
                        ti = 0;
                    if (ti >= 0 && TURN_TRG_DEG[ti][state.trgDeg] >= 0)
                    {
                        state.trgDeg = TURN_TRG_DEG[ti][state.trgDeg];
                        if (state.turnCnt <= 0)
                            state.turnCnt = 60;
                    }
                }

                ad = state.trgDeg * PI / 4;
                break;
            case CentHeadCentType.CHASE:
                ad = atan2(-ship.pos().x + state.pos.x, ship.pos().y - state.pos.y);
                break;
            case CentHeadCentType.ROLL:
                if ((state.nextJointedEnemy) != null)
                {
                    Enemy e_0 = state.nextJointedEnemy;
                    EnemyState s_0 = default(EnemyState);
                    for (;;)
                    {
                        s_0 = e_0.getState_0();
                        if (!((s_0.nextJointedEnemy) != null) || !(s_0.nextJointedEnemy.exists))
                            break;
                        e_0 = s_0.nextJointedEnemy;
                    }

                    ad = atan2(-s_0.pos.x + state.pos.x, s_0.pos.y - state.pos.y);
                }
                else
                {
                    ad = state.deg + PI / 5;
                }

                float sd = atan2(-ship.pos().x + state.pos.x, ship.pos().y - state.pos.y);
                float sf = FORWARD_FORCE_BASE * state.forwardForceScale * 0.16f;
                enemy.addForce(-sin(sd) * sf, cos(sd) * sf);
                break;
        }

        ad = normalizeDeg(ad);
        ad -= state.deg;
        ad = normalizeDeg(ad);
        float f = ad;
        if (f > 1)
            f = 1;
        else if (f < -1)
            f = -1;
        enemy.addRelForceAtRelPos(0, state.sizeScale.x * SIZE / 2, 0, angularForce * f, 0, 0);
        Enemy e_1 = enemy;
        for (;;)
        {
            EnemyState s_1 = e_1.getState_0();
            e_1 = s_1.nextJointedEnemy;
            if (!((e_1) != null) || !(e_1.exists))
                break;
            s_1.trgDeg = state.trgDeg;
            s_1.moveFlag = state.moveFlag;
        }

        enemy.addForce(0, 0, -Field.GRAVITY * state.massScale);
        return true;
    }

    public void remove_1(Enemy enemy)
    {
        Enemy e = enemy;
        for (;;)
        {
            EnemyState s = e.getState_0();
            e = s.nextJointedEnemy;
            if (!((e) != null) || !(e.exists))
                break;
            e.remove_0();
        }
    }

    public void destroyed_2(Enemy enemy, EnemyState state)
    {
        if (state.isHead)
        {
            Enemy e = enemy;
            int idx = 0;
            for (;;)
            {
                EnemyState s = e.getState_0();
                e = s.nextJointedEnemy;
                if (!((e) != null) || !(e.exists))
                    break;
                e.removeAsTail(idx);
                idx++;
            }

            addConnectedParticlesHead(enemy, state);
            enemy.addScore_2((baseSize + 1) * 50 + bodyLength * 10);
        }
    }

    public void addConnectedParticlesHead(Enemy enemy, EnemyState state)
    {
        float d = atan2(state.pos.x, -state.pos.y);
        for (int i = 0; i < 6; i++)
            enemy.addConnectedParticles_3_Single_Single_Boolean(d + EnemySpecBase.rand.nextSignedFloat(PI / 4), 6.0f, false);
    }

    public void addConnectedParticlesBody(Enemy enemy, EnemyState state)
    {
        enemy.addConnectedParticles_3_Single_Single_Boolean(state.deg + PI / 2, 1.5f * state.sizeScale.x);
        enemy.addConnectedParticles_3_Single_Single_Boolean(state.deg - PI / 2, 1.5f * state.sizeScale.x);
    }

    public void drawSubShape(float[] model, float[] tint, Gfx.Blend blend, string key, EnemyState state)
    {
        if (state.isHead)
        {
            float[] parent1 = model;
            model = Transform.Translate(model, state.pos.x, state.pos.y, state.pos.z);
            model = Transform.Multiply(model, state.rot);
            model = Transform.Scale(model, state.sizeScale.x, state.sizeScale.y, state.sizeScale.z);
            subShape.draw(model, tint, blend, key + "-draw-1");
            model = parent1;
        }
    }

    public EnemySpec bodySpec;
    public Vector3 sizeScale;
    public float massScale;
    public float forwardForceScale;
    public float slowVelocityRatio;
    public CentBarrage headBarrage, bodyBarrage;
    public CentHead(Field field, Ship ship, BulletPool bullets, World world, int centType)
    {
        this.centType = centType;
        this.field = field;
        this.ship = ship;
        this.bullets = bullets;
        this.world = world;
        shape = new ShapeGroup();
        shape.addShape(new Square(world, MASS, 0, 0, SIZE * WIDTH, SIZE));
        subShape = new EyeShape();
    }

    public void initState(Enemy enemy, EnemyState state)
    {
        state.type = centType;
        EnemySpecBase.initState(shape, world, enemy, state);
        setHeadBarrage(state);
        if (state.pos.x < 0)
        {
            if (state.pos.y < 0)
                state.trgDeg = 7;
            else
                state.trgDeg = 5;
        }
        else
        {
            if (state.pos.y < 0)
                state.trgDeg = 1;
            else
                state.trgDeg = 3;
        }

        state.turnCnt = 0;
        state.moveFlag = 0;
        state.forwardForceScale = forwardForceScale;
        state.slowVelocityRatio = slowVelocityRatio;
        state.isHead = true;
        state.linePoint.setSpectrumParams(_colorR, _colorG, _colorB, 1.0f);
    }

    public void setHeadBarrage(EnemyState state)
    {
        if ((state.topBullet) != null)
        {
            state.topBullet.removeForced();
            state.topBullet = null;
        }

        if ((headBarrage) != null)
            headBarrage.set_4_BulletPool_Ship_EnemyState_Single(bullets, ship, state);
    }

    public Enemy setJointedEnemies_5(EnemyPool enemies, float x, float y, float z, float deg)
    {
        if (bodyLength <= 0)
            return null;
        Enemy[] je = enemies.getMultipleInstances(bodyLength);
        if (!((je) != null))
            return null;
        bool app = true;
        bool isFirst = true;
        float ex = x, ey = y;
        foreach (Enemy e in je)
        {
            if (isFirst)
            {
                app = e.set_8_EnemySpec_Single_Single_Single_Single_Vector3_Single_Int32(this, ex, ey, z, deg, sizeScale, massScale, 1) && app;
                isFirst = false;
            }
            else
            {
                app = e.set_8_EnemySpec_Single_Single_Single_Single_Vector3_Single_Int32(bodySpec, ex, ey, z, deg, sizeScale, massScale, 1) && app;
            }

            ex += sin(deg) * sizeScale.x * SIZE * 1.1f;
            ey += -cos(deg) * sizeScale.x * SIZE * 1.1f;
        }

        if (!(app))
        {
            foreach (Enemy e in je)
                e.remove_0();
            return null;
        }

        for (int i_0 = 0; i_0 < je.Length - 1; i_0++)
        {
            OdeHandle jid = McdPhysics.Hinge(World.world);
            McdPhysics.HingeLimit(jid, 0, -1);
            McdPhysics.HingeLimit(jid, 1, 1);
            McdPhysics.JointAttach(jid, je[i_0].bodyId(), je[i_0 + 1].bodyId());
            McdPhysics.HingeAnchor(jid, (je[i_0].pos().x + je[i_0 + 1].pos().x) / 2, (je[i_0].pos().y + je[i_0 + 1].pos().y) / 2, (je[i_0].pos().z + je[i_0 + 1].pos().z) / 2);
            McdPhysics.HingeAxis(jid, 0, 0, 1);
            OdeHandle[] joints = null;
            joints = McdArrays.Append(joints, jid);
            je[i_0].setJoints(joints);
        }

        for (int i_1 = 0; i_1 < je.Length; i_1++)
        {
            Enemy pe = null, ne = null;
            if (i_1 > 0)
                pe = je[i_1 - 1];
            if (i_1 < je.Length - 1)
                ne = je[i_1 + 1];
            je[i_1].setJointedEnemiesPrevNext(pe, ne);
            je[i_1].setJointedEnemies_1(je);
        }

        for (int i_2 = 1; i_2 < je.Length; i_2++)
        {
            EnemyState s = je[i_2].getState_0();
            if ((bodyBarrage) != null)
                bodyBarrage.set_4_BulletPool_Ship_EnemyState_Single(bullets, ship, s, (float)i_2 / je.Length / 2);
            s.forwardForceScale = forwardForceScale;
        }

        return je[0];
    }

    public void collide_3(Enemy enemy, EnemyState state, OdeActor actor)
    {
    }

    public void recordLinePoints_2(EnemyState state, LinePoint lp)
    {
        EnemySpecBase.recordLinePoints_2(shape, state, lp);
    }

    public void drawShadow_1(float[] model, float[] tint, Gfx.Blend blend, string key, LinePoint lp)
    {
        shape.drawShadow_1(model, tint, blend, key + "-drawShadow_1-1", lp);
    }

    public bool rotate2d()
    {
        return _rotate2d;
    }

    public bool collideBullet()
    {
        return _collideBullet;
    }

    public float colorR()
    {
        return _colorR;
    }

    public float colorG()
    {
        return _colorG;
    }

    public float colorB()
    {
        return _colorB;
    }
}

public class CentBody : EnemySpec, ConnectedParticlesBodyAddable
{
    public static readonly int[][] TURN_TRG_DEG = new int[][]
    {
        new int[]
        {
            5,
            4,
            3,
            -1,
            -1,
            -1,
            3,
            4
        },
        new int[]
        {
            5,
            6,
            7,
            6,
            5,
            -1,
            -1,
            -1
        },
        new int[]
        {
            -1,
            -1,
            7,
            0,
            1,
            0,
            7,
            -1
        },
        new int[]
        {
            1,
            -1,
            -1,
            -1,
            1,
            2,
            3,
            2
        }
    };
    public const float SIZE = 1.0f;
    public const float WIDTH = 0.25f;
    public const float MASS = 1.0f;
    public const float FORWARD_FORCE_BASE = 5;
    public const float ANGULAR_FORCE_BASE = 10;
    public const float SLOW_VELOCITY_RATIO = 0.01f;
    public const float SLOW_ANGULAR_RATIO = 0.1f;
    public CentHead headSpec;
    public int bodyLength, baseSize;
    public int centType;
    public float centColorR, centColorG, centColorB;
    public Field field;
    public Ship ship;
    public BulletPool bullets;
    public World world;
    public ShapeGroup shape;
    public Drawable subShape = null;
    public bool _rotate2d = true;
    public bool _collideBullet = false;
    public float _colorR, _colorG, _colorB;
    public bool move_2(Enemy enemy, EnemyState state)
    {
        if (field.checkInField_1_Vector3(state.pos))
        {
            if (state.isHead)
                enemy.addRelForce(0, FORWARD_FORCE_BASE * state.forwardForceScale);
            if ((state.topBullet) != null)
                state.topBullet.activated = true;
            state.linePoint.enableSpectrumColor(true);
            switch (state.type)
            {
                case CentBodyCentType.TO_AND_FROM:
                    enemy.slowLinearVel(CentHead.SLOW_VELOCITY_RATIO_CONST * state.slowVelocityRatio * (1 + (state.turnCnt * 0.1f)));
                    break;
                case CentBodyCentType.CHASE:
                    enemy.slowLinearVel(CentHead.SLOW_VELOCITY_RATIO_CONST * state.slowVelocityRatio * 0.25f);
                    break;
                case CentBodyCentType.ROLL:
                    enemy.slowLinearVel(CentHead.SLOW_VELOCITY_RATIO_CONST * state.slowVelocityRatio * 5);
                    break;
            }
        }
        else
        {
            if ((state.topBullet) != null)
                state.topBullet.activated = false;
            state.linePoint.enableSpectrumColor(false);
        }

        enemy.slowAngularVel(CentHead.SLOW_ANGULAR_RATIO);
        if (!(state.isHead))
            return true;
        if (field.checkInField_1_Vector3(state.pos))
            enemy.slowLinearVel(CentHead.SLOW_VELOCITY_RATIO_CONST * state.slowVelocityRatio);
        if (state.turnCnt > 0)
            state.turnCnt--;
        float ad = default(float);
        float angularForce = ANGULAR_FORCE_BASE;
        switch (state.type)
        {
            case CentBodyCentType.TO_AND_FROM:
                int ti = -1;
                if (state.pos.x > field.size().x * 0.5f)
                    ti = 3;
                else if (state.pos.x < -field.size().x * 0.5f)
                    ti = 1;
                if (ti >= 0 && TURN_TRG_DEG[ti][state.trgDeg] >= 0)
                {
                    state.trgDeg = TURN_TRG_DEG[ti][state.trgDeg];
                    if (state.turnCnt <= 0)
                        state.turnCnt = 60;
                }
                else
                {
                    if (state.pos.y < -field.size().y * 0.5f)
                        ti = 2;
                    else if (state.pos.y > field.size().y * 0.5f)
                        ti = 0;
                    if (ti >= 0 && TURN_TRG_DEG[ti][state.trgDeg] >= 0)
                    {
                        state.trgDeg = TURN_TRG_DEG[ti][state.trgDeg];
                        if (state.turnCnt <= 0)
                            state.turnCnt = 60;
                    }
                }

                ad = state.trgDeg * PI / 4;
                break;
            case CentBodyCentType.CHASE:
                ad = atan2(-ship.pos().x + state.pos.x, ship.pos().y - state.pos.y);
                break;
            case CentBodyCentType.ROLL:
                if ((state.nextJointedEnemy) != null)
                {
                    Enemy e_0 = state.nextJointedEnemy;
                    EnemyState s_0 = default(EnemyState);
                    for (;;)
                    {
                        s_0 = e_0.getState_0();
                        if (!((s_0.nextJointedEnemy) != null) || !(s_0.nextJointedEnemy.exists))
                            break;
                        e_0 = s_0.nextJointedEnemy;
                    }

                    ad = atan2(-s_0.pos.x + state.pos.x, s_0.pos.y - state.pos.y);
                }
                else
                {
                    ad = state.deg + PI / 5;
                }

                float sd = atan2(-ship.pos().x + state.pos.x, ship.pos().y - state.pos.y);
                float sf = FORWARD_FORCE_BASE * state.forwardForceScale * 0.16f;
                enemy.addForce(-sin(sd) * sf, cos(sd) * sf);
                break;
        }

        ad = normalizeDeg(ad);
        ad -= state.deg;
        ad = normalizeDeg(ad);
        float f = ad;
        if (f > 1)
            f = 1;
        else if (f < -1)
            f = -1;
        enemy.addRelForceAtRelPos(0, state.sizeScale.x * SIZE / 2, 0, angularForce * f, 0, 0);
        Enemy e_1 = enemy;
        for (;;)
        {
            EnemyState s_1 = e_1.getState_0();
            e_1 = s_1.nextJointedEnemy;
            if (!((e_1) != null) || !(e_1.exists))
                break;
            s_1.trgDeg = state.trgDeg;
            s_1.moveFlag = state.moveFlag;
        }

        enemy.addForce(0, 0, -Field.GRAVITY * state.massScale);
        return true;
    }

    public void remove_1(Enemy enemy)
    {
        Enemy e = enemy;
        for (;;)
        {
            EnemyState s = e.getState_0();
            e = s.nextJointedEnemy;
            if (!((e) != null) || !(e.exists))
                break;
            e.remove_0();
        }
    }

    public void destroyed_2(Enemy enemy, EnemyState state)
    {
        if (state.isHead)
        {
            Enemy e = enemy;
            int idx = 0;
            for (;;)
            {
                EnemyState s = e.getState_0();
                e = s.nextJointedEnemy;
                if (!((e) != null) || !(e.exists))
                    break;
                e.removeAsTail(idx);
                idx++;
            }

            addConnectedParticlesHead(enemy, state);
            enemy.addScore_2((baseSize + 1) * 50 + bodyLength * 10);
        }
    }

    public void addConnectedParticlesHead(Enemy enemy, EnemyState state)
    {
        float d = atan2(state.pos.x, -state.pos.y);
        for (int i = 0; i < 6; i++)
            enemy.addConnectedParticles_3_Single_Single_Boolean(d + EnemySpecBase.rand.nextSignedFloat(PI / 4), 6.0f, false);
    }

    public void addConnectedParticlesBody(Enemy enemy, EnemyState state)
    {
        enemy.addConnectedParticles_3_Single_Single_Boolean(state.deg + PI / 2, 1.5f * state.sizeScale.x);
        enemy.addConnectedParticles_3_Single_Single_Boolean(state.deg - PI / 2, 1.5f * state.sizeScale.x);
    }

    public void drawSubShape(float[] model, float[] tint, Gfx.Blend blend, string key, EnemyState state)
    {
        if (state.isHead)
        {
            float[] parent1 = model;
            model = Transform.Translate(model, state.pos.x, state.pos.y, state.pos.z);
            model = Transform.Multiply(model, state.rot);
            model = Transform.Scale(model, state.sizeScale.x, state.sizeScale.y, state.sizeScale.z);
            subShape.draw(model, tint, blend, key + "-draw-1");
            model = parent1;
        }
    }

    public CentBody(Field field, Ship ship, BulletPool bullets, World world, CentHead headSpec, int centType, float centColorR, float centColorG, float centColorB)
    {
        this.centType = centType;
        this.centColorR = centColorR;
        this.centColorG = centColorG;
        this.centColorB = centColorB;
        this.field = field;
        this.ship = ship;
        this.bullets = bullets;
        this.world = world;
        this.headSpec = headSpec;
        shape = new ShapeGroup();
        shape.addShape(new Square(world, MASS, 0, 0, SIZE / 4, SIZE));
        subShape = new EyeShape();
    }

    public void initState(Enemy enemy, EnemyState state)
    {
        state.type = centType;
        EnemySpecBase.initState(shape, world, enemy, state);
        state.trgDeg = 0;
        state.turnCnt = 0;
        state.moveFlag = 0;
        state.isHead = false;
        state.destroyable = state.isHead;
        _colorR = centColorR;
        _colorG = centColorG;
        _colorB = centColorB;
        state.linePoint.setSpectrumParams(_colorR, _colorG, _colorB, 1.0f);
    }

    public void collide_3(Enemy enemy, EnemyState state, OdeActor actor)
    {
    }

    public void recordLinePoints_2(EnemyState state, LinePoint lp)
    {
        EnemySpecBase.recordLinePoints_2(shape, state, lp);
    }

    public void drawShadow_1(float[] model, float[] tint, Gfx.Blend blend, string key, LinePoint lp)
    {
        shape.drawShadow_1(model, tint, blend, key + "-drawShadow_1-1", lp);
    }

    public bool rotate2d()
    {
        return _rotate2d;
    }

    public bool collideBullet()
    {
        return _collideBullet;
    }

    public float colorR()
    {
        return _colorR;
    }

    public float colorG()
    {
        return _colorG;
    }

    public float colorB()
    {
        return _colorB;
    }
}

public interface ConnectedParticlesBodyAddable
{
    void addConnectedParticlesBody(Enemy enemy, EnemyState state);
}

public class Block : EnemySpec
{
    public const float SLOW_VELOCITY_RATIO = 0.022f;
    public const float SLOW_ANGULAR_RATIO = 0.022f;
    public Field field;
    public Ship ship;
    public BulletPool bullets;
    public World world;
    public ShapeGroup shape;
    public Drawable subShape = null;
    public bool _rotate2d = true;
    public bool _collideBullet = false;
    public float _colorR, _colorG, _colorB;
    public Block(Field field, Ship ship, BulletPool bullets, World world)
    {
        this.field = field;
        this.ship = ship;
        this.bullets = bullets;
        this.world = world;
        shape = new ShapeGroup();
        shape.addShape(new Box(world, 1, 0, 0, 0, 1, 1, 1));
        _rotate2d = false;
        _collideBullet = true;
    }

    public void initState(Enemy enemy, EnemyState state)
    {
        EnemySpecBase.initState(shape, world, enemy, state);
        _colorR = 0;
        _colorG = 0;
        _colorB = 1;
        state.linePoint.setSpectrumParams(_colorR, _colorG, _colorB, 0.7f);
        state.type = -1;
    }

    public bool move_2(Enemy enemy, EnemyState state)
    {
        if (field.checkInField_1_Vector3(state.pos))
        {
            enemy.slowLinearVel(SLOW_VELOCITY_RATIO * state.massScale);
            state.linePoint.enableSpectrumColor(true);
        }
        else
        {
            state.linePoint.enableSpectrumColor(false);
        }

        enemy.slowAngularVel(SLOW_ANGULAR_RATIO);
        enemy.addForce(0, 0, -Field.GRAVITY * 0.25f * state.massScale);
        return true;
    }

    public void destroyed_2(Enemy enemy, EnemyState state)
    {
        float d = atan2(state.pos.x, -state.pos.y);
        for (int i = 0; i < 5; i++)
            enemy.addConnectedParticles_3_Single_Single_Boolean(d + EnemySpecBase.rand.nextSignedFloat(PI / 4), 4.0f * state.sizeScale.x);
        enemy.addScore_2(100, 0.3f);
    }

    public void collide_3(Enemy enemy, EnemyState state, OdeActor actor)
    {
    }

    public void recordLinePoints_2(EnemyState state, LinePoint lp)
    {
        EnemySpecBase.recordLinePoints_2(shape, state, lp);
    }

    public void drawShadow_1(float[] model, float[] tint, Gfx.Blend blend, string key, LinePoint lp)
    {
        shape.drawShadow_1(model, tint, blend, key + "-drawShadow_1-1", lp);
    }

    public bool rotate2d()
    {
        return _rotate2d;
    }

    public bool collideBullet()
    {
        return _collideBullet;
    }

    public float colorR()
    {
        return _colorR;
    }

    public float colorG()
    {
        return _colorG;
    }

    public float colorB()
    {
        return _colorB;
    }

    public void drawSubShape(float[] model, float[] tint, Gfx.Blend blend, string key, EnemyState state)
    {
    }
}

public static class CentBarrageBasicBarrageType
{
    public const int AIM = 0;
    public const int FRONT = 1;
    public const int PLUMB = 2;
    public const int SIDE = 3;
    public const int ONE_SIDE = 4;
    public const int AIM_IN_ORDER = 5;
}

public static class CentHeadCentType
{
    public const int TO_AND_FROM = 0;
    public const int CHASE = 1;
    public const int ROLL = 2;
}

public static class CentBodyCentType
{
    public const int TO_AND_FROM = 0;
    public const int CHASE = 1;
    public const int ROLL = 2;
}
