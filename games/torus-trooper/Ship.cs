// Copyright 2004 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;
using static GameMath;
using static Lub;

public class Ship : BulletTarget
{
    public const float IN_SIGHT_DEPTH_DEFAULT = 35;
    public const float RELPOS_MAX_Y = 10;
    public const int GRADE_NUM = 3;
    public static string[] GRADE_LETTER = new string[]
    {
        "N",
        "H",
        "E"
    };
    public static string[] GRADE_STR = new string[]
    {
        "NORMAL",
        "HARD",
        "EXTREME"
    };
    public static bool replayMode, cameraMode, drawFrontMode;
    public bool isGameOver;
    public const int RESTART_CNT = 268;
    public const int INVINCIBLE_CNT = 228;
    public const float HIT_WIDTH = 0.00025f;
    public const float EYE_HEIGHT = 0.8f;
    public const float LOOKAT_HEIGHT = 0.9f;
    public Rand rand;
    public RecordablePad pad;
    public Tunnel tunnel;
    public ShotPool shots;
    public ParticlePool particles;
    public InGameState gameState;
    public Vector _pos;
    public Vector _relPos;
    public Vector _eyePos;
    public Vector rocketPos;
    public float d1, d2;
    public int grade;
    public float nextStarAppDist;
    public Vector starPos;
    public int lap;
    public static float[] SPEED_DEFAULT = new float[] { 0.4f, 0.6f, 0.8f };
    public static float[] SPEED_MAX = new float[] { 0.8f, 1.2f, 1.6f };
    public static float[] ACCEL_RATIO = new float[] { 0.002f, 0.003f, 0.004f };
    public float targetSpeed;
    public float _speed;
    public float _inSightDepth;
    public static float[] BANK_MAX_DEFAULT = new float[] { 0.8f, 1.0f, 1.2f };
    public const float OUT_OF_COURSE_BANK = 1.0f;
    public const float RELPOS_Y_MOVE = 0.1f;
    public float bank;
    public float bankMax;
    public float tunnelOfs;
    public Vector3 pos3;
    public ShipShape _shape;
    public Vector3 epos;
    public Shot chargingShot;
    public const int FIRE_INTERVAL = 2;
    public const int STAR_SHELL_INTERVAL = 7;
    public float regenerativeCharge;
    public float fireCnt;
    public const float GUNPOINT_WIDTH = 0.05f;
    public int fireShotCnt;
    public float sideFireCnt;
    public int sideFireShotCnt;
    public Vector gunpointPos;
    public int rank;
    public int bossAppRank, bossAppNum, zoneEndRank;
    public bool _inBossMode;
    public bool _isBossModeEnd;
    public float cnt;
    public float screenShakeCnt;
    public float screenShakeIntense;
    public Camera camera;
    public bool btnPressed;
    public Ship(Pad pad, Tunnel tunnel)
    {
        rand = new Rand();
        this.pad = (RecordablePad)pad;
        this.tunnel = tunnel;
        _pos = new Vector();
        _relPos = new Vector();
        _eyePos = new Vector();
        rocketPos = new Vector();
        starPos = new Vector();
        pos3 = new Vector3();
        epos = new Vector3();
        _shape = new ShipShape(1);
        shape.create(ShipShapeType.SMALL);
        gunpointPos = new Vector();
        camera = new Camera(this);
        drawFrontMode = true;
        cameraMode = true;
    }

    public void setParticles(ParticlePool particles)
    {
        this.particles = particles;
    }

    public void setShots(ShotPool shots)
    {
        this.shots = shots;
    }

    public void setGameState(InGameState gameState)
    {
        this.gameState = gameState;
    }

    public void start(int grd, int seed)
    {
        rand.setSeed(seed);
        grade = grd;
        tunnelOfs = 0;
        {
            _pos.y = 0;
            _pos.x = _pos.y;
        }

        {
            _relPos.y = 0;
            _relPos.x = _relPos.y;
        }

        {
            _eyePos.y = 0;
            _eyePos.x = _eyePos.y;
        }

        bank = 0;
        _speed = 0;
        {
            d2 = 0;
            d1 = d2;
        }

        cnt = -INVINCIBLE_CNT;
        {
            sideFireShotCnt = 0;
            fireShotCnt = sideFireShotCnt;
        }

        _inSightDepth = IN_SIGHT_DEPTH_DEFAULT;
        rank = 0;
        bankMax = BANK_MAX_DEFAULT[grade];
        nextStarAppDist = 0;
        lap = 1;
        isGameOver = false;
        restart();
        if (replayMode)
            camera.start();
        btnPressed = true;
    }

    public void restart()
    {
        targetSpeed = 0;
        fireCnt = 0;
        sideFireCnt = 99999;
        if ((chargingShot != null))
        {
            chargingShot.remove();
            chargingShot = null;
        }

        regenerativeCharge = 0;
    }

    public void close()
    {
        _shape.close();
    }

    public void move()
    {
        cnt += SimulationTime.Step;
        int btn = 0, dir = 0;
        if (!(replayMode))
        {
            btn = pad.getButtonState();
            dir = pad.getDirState();
            pad.record();
        }
        else
        {
            int ps = pad.replay();
            if (ps == RecordablePad.REPLAY_END)
            {
                ps = 0;
                isGameOver = true;
            }

            dir = ps & (PadDir.UP | PadDir.DOWN | PadDir.LEFT | PadDir.RIGHT);
            btn = ps & PadButton.ANY;
        }

        if (btnPressed)
        {
            if ((btn != 0))
                btn = 0;
            else
                btnPressed = false;
        }

        if (isGameOver)
        {
            {
                dir = 0;
                btn = dir;
            }

            _speed = _speed * SimulationTime.Decay(0.9f);
            clearVisibleBullets();
            if (cnt < -INVINCIBLE_CNT)
                cnt = -RESTART_CNT;
        }
        else if (cnt < -INVINCIBLE_CNT)
        {
            {
                dir = 0;
                btn = dir;
            }

            _relPos.y = _relPos.y * SimulationTime.Decay(0.99f);
            clearVisibleBullets();
        }

        float aimSpeed = targetSpeed;
        if (((btn & PadButton.B) != 0))
        {
            aimSpeed = aimSpeed * (0.5f);
        }
        else
        {
            float chargeAcceleration = regenerativeCharge * SimulationTime.Blend(0.1f);
            _speed = _speed + (chargeAcceleration);
            aimSpeed = aimSpeed + (chargeAcceleration);
            regenerativeCharge = regenerativeCharge - (chargeAcceleration);
        }

        if (_speed < aimSpeed)
        {
            _speed = _speed + ((aimSpeed - _speed) * SimulationTime.Blend(0.015f));
        }
        else
        {
            if (((btn & PadButton.B) != 0))
                regenerativeCharge = regenerativeCharge - ((aimSpeed - _speed) * SimulationTime.Blend(0.05f));
            _speed = _speed + ((aimSpeed - _speed) * SimulationTime.Blend(0.05f));
        }

        _pos.y = _pos.y + (_speed * SimulationTime.Step);
        tunnelOfs = tunnelOfs + (_speed * SimulationTime.Step);
        int tmv = GameMath.integer(tunnelOfs);
        tunnel.goToNextSlice(tmv);
        addScore(tmv);
        tunnelOfs = _pos.y - GameMath.integer(_pos.y);
        if (pos.y >= tunnel.getTorusLength())
        {
            pos.y = pos.y - (tunnel.getTorusLength());
            lap++;
        }

        tunnel.setShipPos(_relPos.x, tunnelOfs, _pos.y);
        tunnel.setSlices();
        tunnel.setSlicesBackward();
        Vector3 sp = tunnel.getPos_1_Vector(_relPos);
        pos3.x = sp.x;
        pos3.y = sp.y;
        pos3.z = sp.z;
        if (SimulationTime.Variable)
        {
            float targetBank = (dir & PadDir.LEFT) != 0 ? bankMax : (dir & PadDir.RIGHT) != 0 ? -bankMax : 0;
            bank = SimulationTime.FollowAndDecay(bank, targetBank, targetBank == 0 ? 0 : .1f, .9f);
        }
        else
        {
            if (((dir & PadDir.RIGHT) != 0)) bank = bank + ((-bankMax - bank) * 0.1f);
            if (((dir & PadDir.LEFT) != 0)) bank = bank + ((bankMax - bank) * 0.1f);
        }
        bool overAccel = false;
        if (((dir & PadDir.UP) != 0))
        {
            if (_relPos.y < RELPOS_MAX_Y)
            {
                _relPos.y = _relPos.y + (RELPOS_Y_MOVE * SimulationTime.Step);
            }
            else
            {
                targetSpeed = targetSpeed + (ACCEL_RATIO[grade] * SimulationTime.Step);
                if (((!(((btn & PadButton.B) != 0))) && (!(_inBossMode))) && (!(_isBossModeEnd)))
                    overAccel = true;
            }
        }

        if ((((dir & PadDir.DOWN) != 0)) && (_relPos.y > 0))
            _relPos.y = _relPos.y - (RELPOS_Y_MOVE * SimulationTime.Step);
        float acc = _relPos.y * (SPEED_MAX[grade] - SPEED_DEFAULT[grade]) / RELPOS_MAX_Y + SPEED_DEFAULT[grade];
        if (overAccel)
            targetSpeed = targetSpeed + ((acc - targetSpeed) * SimulationTime.Blend(0.001f));
        else if (targetSpeed < acc)
            targetSpeed = targetSpeed + ((acc - targetSpeed) * SimulationTime.Blend(0.005f));
        else
            targetSpeed = targetSpeed + ((acc - targetSpeed) * SimulationTime.Blend(0.03f));
        _inSightDepth = IN_SIGHT_DEPTH_DEFAULT * (1 + _relPos.y / RELPOS_MAX_Y);
        if (_speed > SPEED_MAX[grade])
            _inSightDepth = _inSightDepth + (IN_SIGHT_DEPTH_DEFAULT * (_speed - SPEED_MAX[grade]) / SPEED_MAX[grade] * 3.0f);
        if (!SimulationTime.Variable) bank = bank * 0.9f;
        _pos.x = _pos.x + (bank * 0.08f * (SliceState.DEFAULT_RAD / tunnel.getRadius(_relPos.y)) * SimulationTime.Step);
        if (_pos.x < 0)
            _pos.x = _pos.x + (PI * 2);
        else if (_pos.x >= PI * 2)
            _pos.x = _pos.x - (PI * 2);
        _relPos.x = _pos.x;
        float ox = _relPos.x - _eyePos.x;
        if (ox > PI)
            ox = ox - (PI * 2);
        else if (ox < -PI)
            ox = ox + (PI * 2);
        _eyePos.x = _eyePos.x + (ox * SimulationTime.Blend(0.1f));
        if (_eyePos.x < 0)
            _eyePos.x = _eyePos.x + (PI * 2);
        else if (_eyePos.x >= PI * 2)
            _eyePos.x = _eyePos.x - (PI * 2);
        Slice sl = tunnel.getSlice(_relPos.y);
        float co = tunnel.checkInCourse(_relPos);
        if (co != 0)
        {
            float bm = (-OUT_OF_COURSE_BANK * co - bank) * 0.075f;
            if (bm > 1)
                bm = 1;
            else if (bm < -1)
                bm = -1;
            _speed = _speed * SimulationTime.Decay(1 - fabs(bm));
            bank = bank + (bm * SimulationTime.Step);
            float lo = fabs(pos.x - sl.getLeftEdgeDeg());
            if (lo > PI)
                lo = PI * 2 - lo;
            float ro = fabs(pos.x - sl.getRightEdgeDeg());
            if (ro > PI)
                ro = PI * 2 - ro;
            if (lo > ro)
                pos.x = sl.getRightEdgeDeg();
            else
                pos.x = sl.getLeftEdgeDeg();
            _relPos.x = _pos.x;
        }

        d1 = d1 + ((sl.d1 - d1) * SimulationTime.Blend(0.05f));
        d2 = d2 + ((sl.d2 - d2) * SimulationTime.Blend(0.05f));
        if (((btn & PadButton.B) != 0))
        {
            if (!((chargingShot != null)))
            {
                chargingShot = shots.getInstanceForced();
                chargingShot.set_3(true);
            }
        }
        else
        {
            if ((chargingShot != null))
            {
                chargingShot.release();
                chargingShot = null;
            }

            if (((btn & PadButton.A) != 0))
            {
                if (fireCnt <= 0)
                {
                    fireCnt = SimulationTime.Repeat(fireCnt, FIRE_INTERVAL);
                    Shot shot = shots.getInstance();
                    if ((shot != null))
                    {
                        if ((fireShotCnt % STAR_SHELL_INTERVAL) == 0)
                            shot.set_3(false, true);
                        else
                            shot.set_3();
                        gunpointPos.x = _relPos.x + GUNPOINT_WIDTH * ((fireShotCnt % 2) * 2 - 1);
                        gunpointPos.y = _relPos.y;
                        shot.update(gunpointPos);
                        fireShotCnt++;
                    }
                }

                if (sideFireCnt <= 0)
                {
                    sideFireCnt = SimulationTime.Repeat(sideFireCnt, 99999);
                    Shot shot = shots.getInstance();
                    if ((shot != null))
                    {
                        float sideFireDeg = (speed - SPEED_DEFAULT[grade]) / (SPEED_MAX[grade] - SPEED_DEFAULT[grade]) * 0.1f;
                        if (sideFireDeg < 0.01f)
                            sideFireDeg = 0.01f;
                        float d = sideFireDeg * (sideFireShotCnt % 5) * 0.2f;
                        if ((sideFireShotCnt % 2) == 1)
                            d = -d;
                        if ((sideFireShotCnt % STAR_SHELL_INTERVAL) == 0)
                            shot.set_3(false, true, d);
                        else
                            shot.set_3(false, false, d);
                        gunpointPos.x = _relPos.x + GUNPOINT_WIDTH * ((fireShotCnt % 2) * 2 - 1);
                        gunpointPos.y = _relPos.y;
                        shot.update(gunpointPos);
                        sideFireShotCnt++;
                    }
                }
            }
        }

        if (fireCnt > 0)
            fireCnt -= SimulationTime.Step;
        int ssc = 99999;
        if (speed > SPEED_DEFAULT[grade] * 1.33f)
        {
            ssc = GameMath.integer((100000 / ((speed - SPEED_DEFAULT[grade] * 1.33f) * 99999 / (SPEED_MAX[grade] - SPEED_DEFAULT[grade]) + 1)));
        }

        if (sideFireCnt > ssc)
            sideFireCnt = ssc;
        if (sideFireCnt > 0)
            sideFireCnt -= SimulationTime.Step;
        rocketPos.x = _relPos.x - bank * 0.1f;
        rocketPos.y = _relPos.y;
        if ((chargingShot != null))
            chargingShot.update(rocketPos);
        if (cnt >= -INVINCIBLE_CNT)
            shape.addParticles(rocketPos, particles);
        if (SimulationTime.Emit) nextStarAppDist -= speed;
        if (nextStarAppDist <= 0)
        {
            for (int i = 0; i < 5; i++)
            {
                Particle pt = particles.getInstance();
                if (!((pt != null)))
                    break;
                starPos.x = relPos.x + rand.nextSignedFloat(PI / 2) + PI;
                starPos.y = 32;
                pt.set_12(starPos, -8 - rand.nextFloat(56), PI, 0, 0, 0.6f, 0.7f, 0.9f, 100, ParticlePType.STAR);
            }

            nextStarAppDist = 1;
        }

        if (screenShakeCnt > 0)
            screenShakeCnt -= SimulationTime.Step;
        if (replayMode)
            camera.move();
    }

    public void addBoostParticles(int n, float sp)
    {
        for (int i = 0; i < n; i++)
        {
            Particle pt = particles.getInstanceForced();
            pt.set_12(relPos, 1, rand.nextSignedFloat(PI * 0.01f), 0, sp + rand.nextSignedFloat(0.25f), 0.9f, 0.5f, 1.0f);
        }
    }

    public Vector getTargetPos()
    {
        return _relPos;
    }

    public float[] setEyepos()
    {
        float scale = 1;
        float ex = 0, ey = 0, ez = 0;
        float lx = 0, ly = 0, lz = 0;
        float deg = 0;
        if ((!(replayMode)) || (!(cameraMode)))
        {
            epos.x = _eyePos.x;
            epos.y = -1.1f;
            epos.y = epos.y + (_relPos.y * 0.3f);
            epos.z = 30.0f;
            Vector3 ep3 = tunnel.getPos_1_Vector3(epos);
            ex = ep3.x;
            ey = ep3.y;
            ez = ep3.z;
            epos.x = _eyePos.x;
            epos.y = epos.y + (6.0f);
            epos.y = epos.y + (_relPos.y * 0.3f);
            epos.z = 0;
            Vector3 lp3 = tunnel.getPos_1_Vector3(epos);
            lx = lp3.x;
            ly = lp3.y;
            lz = lp3.z;
            deg = _eyePos.x;
        }
        else
        {
            Vector3 ep3 = tunnel.getPos_1_Vector3(camera.cameraPos);
            ex = ep3.x;
            ey = ep3.y;
            ez = ep3.z;
            Vector3 lp3 = tunnel.getPos_1_Vector3(camera.lookAtPos);
            lx = lp3.x;
            ly = lp3.y;
            lz = lp3.z;
            deg = camera.deg;
            scale = 1 / camera.zoom;
        }

        if (screenShakeCnt > 0)
        {
            float mx = rand.nextSignedFloat(screenShakeIntense * (screenShakeCnt + 6));
            float my = rand.nextSignedFloat(screenShakeIntense * (screenShakeCnt + 6));
            float mz = rand.nextSignedFloat(screenShakeIntense * (screenShakeCnt + 6));
            ex = ex + (mx);
            ey = ey + (my);
            ez = ez + (mz);
            lx = lx + (mx);
            ly = ly + (my);
            lz = lz + (mz);
        }

        return Transform.LookAt(Transform.Perspective(10000, scale), ex, ey, ez, lx, ly, lz, sin(deg), -cos(deg), 0);
    }

    public void setScreenShake(int cnt, float its)
    {
        screenShakeCnt = cnt;
        screenShakeIntense = its;
    }

    public bool checkBulletHit(Vector p, Vector pp)
    {
        if (cnt <= 0)
            return false;
        float bmvx = 0, bmvy = 0, inaa = 0;
        bmvx = pp.x;
        bmvy = pp.y;
        bmvx = bmvx - (p.x);
        bmvy = bmvy - (p.y);
        if (bmvx > PI)
            bmvx = bmvx - (PI * 2);
        else if (bmvx < -PI)
            bmvx = bmvx + (PI * 2);
        inaa = bmvx * bmvx + bmvy * bmvy;
        if (inaa > 0.00001f)
        {
            float sofsx = 0, sofsy = 0, inab = 0, hd = 0;
            sofsx = _relPos.x;
            sofsy = _relPos.y;
            sofsx = sofsx - (p.x);
            sofsy = sofsy - (p.y);
            if (sofsx > PI)
                sofsx = sofsx - (PI * 2);
            else if (sofsx < -PI)
                sofsx = sofsx + (PI * 2);
            inab = bmvx * sofsx + bmvy * sofsy;
            if ((inab >= 0) && (inab <= inaa))
            {
                hd = sofsx * sofsx + sofsy * sofsy - inab * inab / inaa;
                if ((hd >= 0) && (hd <= HIT_WIDTH))
                {
                    destroyed();
                    return true;
                }
            }
        }

        return false;
    }

    public void destroyed()
    {
        if (cnt <= 0)
            return;
        for (int i = 0; i < 256; i++)
        {
            Particle pt = particles.getInstanceForced();
            pt.set_12(relPos, 1, rand.nextSignedFloat(PI / 8), rand.nextSignedFloat(2.5f), 0.5f + rand.nextFloat(1), 1, 0.2f + rand.nextFloat(0.8f), 0.2f, 32);
        }

        gameState.shipDestroyed();
        SoundManager.playSe("myship_dest.wav");
        setScreenShake(32, 0.05f);
        restart();
        cnt = -RESTART_CNT;
    }

    public bool hasCollision()
    {
        if (cnt < -INVINCIBLE_CNT)
            return false;
        else
            return true;
    }

    public void rankUp(bool isBoss)
    {
        if ((((_inBossMode) && (!(isBoss)))) || (isGameOver))
            return;
        if (_inBossMode)
        {
            bossAppNum--;
            if (bossAppNum <= 0)
            {
                rank++;
                gameState.gotoNextZone();
                _inBossMode = false;
                _isBossModeEnd = true;
                bossAppRank = 9999999;
                return;
            }
        }

        rank++;
        if (rank >= bossAppRank)
            _inBossMode = true;
    }

    public void gotoNextZoneForced()
    {
        bossAppNum = 0;
        _inBossMode = false;
        _isBossModeEnd = true;
        bossAppRank = 9999999;
    }

    public void startNextZone()
    {
        _isBossModeEnd = false;
    }

    public void rankDown()
    {
        if (_inBossMode)
            return;
        rank--;
    }

    public void setBossApp(int rank, int num, int zoneEndRank)
    {
        bossAppRank = rank;
        bossAppNum = num;
        this.zoneEndRank = zoneEndRank;
        _inBossMode = false;
    }

    public void addScore(int sc)
    {
        gameState.addScore(sc);
    }

    public void clearVisibleBullets()
    {
        gameState.clearVisibleBullets();
    }

    public void draw(float[] model, float[] tint, Gfx.Blend blend, Gfx.Cull cull, float lineWidth)
    {
        if ((cnt < -INVINCIBLE_CNT) || (((cnt < 0) && ((-cnt % 32) < 16))))
            return;
        float[] parent1 = model;
        model = Transform.Translate(model, pos3.x, pos3.y, pos3.z);
        model = Transform.Rotate(model, (pos.x - bank) * 180 / PI, 0, 0, 1);
        model = Transform.Rotate(model, d1 * 180 / PI, 0, 1, 0);
        model = Transform.Rotate(model, d2 * 180 / PI, 1, 0, 0);
        _shape.draw(model, tint, blend, cull, lineWidth);
        model = parent1;
    }

    public void drawFront(float[] model, float[] tint, Gfx.Blend blend, Gfx.Cull cull, float lineWidth)
    {
        Letter.drawNum(model, tint, blend, cull, lineWidth, GameMath.integer((speed * 2500)), 490, 420, 20);
        Letter.drawString(model, tint, blend, cull, lineWidth, "KM/H", 540, 445, 12);
        Letter.drawNum(model, tint, blend, cull, lineWidth, rank, 150, 432, 16);
        Letter.drawString(model, tint, blend, cull, lineWidth, "/", 185, 448, 10);
        Letter.drawNum(model, tint, blend, cull, lineWidth, zoneEndRank - rank, 250, 448, 10);
    }

    public Vector pos
    {
        get
        {
            return _pos;
        }
    }

    public Vector relPos
    {
        get
        {
            return _relPos;
        }
    }

    public Vector eyePos
    {
        get
        {
            return _eyePos;
        }
    }

    public float speed
    {
        get
        {
            return _speed;
        }
    }

    public ShipShape shape
    {
        get
        {
            return _shape;
        }
    }

    public float inSightDepth
    {
        get
        {
            return _inSightDepth;
        }
    }

    public bool inBossMode
    {
        get
        {
            return _inBossMode;
        }
    }

    public bool isBossModeEnd
    {
        get
        {
            return _isBossModeEnd;
        }
    }
}

public static class ShipGrade
{
    public const int NORMAL = 0, HARD = 1, EXTREME = 2;
}
