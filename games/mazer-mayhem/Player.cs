// Copyright 2008 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;

public class Player
{
    private const float collisionWidth = 0.5f;
    private const int fireInterval = 2;
    private const float shotSpeed = 2.5f;
    private const int invincibleInterval = 240;
    private const int restartInterval = 60;
    private const int maxLeft = 3;
    private const int firstExtendScore = 1000000;
    private static Random random;
    private MmFrame frame;
    private Field field;
    private Pad pad;
    private Stage stage;
    private WallPool walls;
    private BallPool balls;
    private ShotPool shots;
    private GrenadePool grenades;
    private BoardPool playerBoards;
    private BoardPool ballBoards;
    private BulletPool bullets;
    private ParticlePool particles;
    private Record record;
    private Replay replay;
    private Sound sound;
    private Vector3 storedPos = new Vector3();
    private Vector2 storedPos2 = new Vector2();
    private float deg;
    private float tiresDeg;
    private Vector2 blurVel = new Vector2();
    private BlurNormalTextureCubeListShape shape, shapeTurret, shapeTires;
    private Vector4 bv = new Vector4();
    private int storedAreaX, storedAreaY;
    private Vector2 areaOfs = new Vector2();
    private int fireCnt, shotCnt;
    private int cnt;
    private QuadListShape shadowShape;
    private Vector2 dashVel = new Vector2();
    private int dashCnt;
    private bool aPressed;
    private float multiplier;
    private float baseMultiplier;
    private int bonusCnt;
    private int score;
    private int left;
    private int nextExtendScore;
    private int addedExtendScore;
    private bool storedIsInHyper;
    private int hyperStartCnt;
    private int bonusCntLength;
    private float baseRank;
    private float hyperRank;
    private QuadListShape stateShape;
    private int restartCnt;
    private int blinkCnt;
    private int blinkInterval;
    private bool hasShape;
    private float velZ;
    private int gameoverCnt;
    private int turnMessageCnt;
    private int hyperMessageCnt;
    private int dashMessageCnt;
    private int dashNumCnt;
    private bool isPlayedBonusGetSe;
    private bool isInReplay;
    private ReplayData replayData = new ReplayData();
    private ReplayCursor replayDataEnumerator;
    public static void SetRandomSeed(Int32 s)
    {
        random = new Random(s);
    }

    public Player(MmFrame frame, Field field, Pad pad, Stage stage, WallPool walls, BoardPool playerBoards, BoardPool ballBoards, ParticlePool particles, Record record, Replay replay, Sound sound)
    {
        this.frame = frame;
        this.field = field;
        this.pad = pad;
        this.stage = stage;
        this.walls = walls;
        this.playerBoards = playerBoards;
        this.ballBoards = ballBoards;
        this.particles = particles;
        this.record = record;
        this.replay = replay;
        this.sound = sound;
        shape = new BlurNormalTextureCubeListShape(frame);
        shape.BeginAdd(1);
        shape.AddVector3Vector3float((Vector3.Zero).Copy(), new Vector3(0, 0, -1), 0.75f);
        shape.EndAdd();
        shapeTurret = new BlurNormalTextureCubeListShape(frame);
        shapeTurret.BeginAdd(2);
        shapeTurret.AddVector3Vector3float(new Vector3(0, 1.25f, 0), new Vector3(0, 0, -1), 0.5f);
        shapeTurret.AddVector3Vector3float(new Vector3(0, 2.25f, 0), new Vector3(0, 0, -1), 0.5f);
        shapeTurret.EndAdd();
        shapeTires = new BlurNormalTextureCubeListShape(frame);
        shapeTires.BeginAdd(4);
        shapeTires.AddVector3Vector3float(new Vector3(1.5f, 1.5f, 0), (Vector3.Zero).Copy(), 0.5f);
        shapeTires.AddVector3Vector3float(new Vector3(-1.5f, 1.5f, 0), (Vector3.Zero).Copy(), 0.5f);
        shapeTires.AddVector3Vector3float(new Vector3(-1.5f, -1.5f, 0), (Vector3.Zero).Copy(), 0.5f);
        shapeTires.AddVector3Vector3float(new Vector3(1.5f, -1.5f, 0), (Vector3.Zero).Copy(), 0.5f);
        shapeTires.EndAdd();
        shadowShape = new QuadListShape(frame);
        shadowShape.Initializeintbytebytebytebyte(1, 0, 0, 0, 160);
        stateShape = new QuadListShape(frame);
        stateShape.Initializeintbytebytebytebyte(512, 10, 10, 10, 255);
    }

    public void SetParams(BallPool balls, BulletPool bullets, ShotPool shots, GrenadePool grenades)
    {
        this.balls = balls;
        this.bullets = bullets;
        this.shots = shots;
        this.grenades = grenades;
    }

    public void SetStartPos(int x, int y)
    {
        storedAreaX = x;
        storedAreaY = y;
        {
            storedPos2.X = Stage.AreaSize * storedAreaX * 2;
            storedPos.X = storedPos2.X;
        }

        {
            storedPos2.Y = Stage.AreaSize * storedAreaY * 2;
            storedPos.Y = storedPos2.Y;
        }
    }

    public void Start()
    {
        field.Offset = (storedPos2).Copy();
        {
            areaOfs.Y = 0;
            areaOfs.X = areaOfs.Y;
        }

        deg = 0;
        tiresDeg = 0;
        field.Deg = 0;
        cnt = 0;
        bonusCnt = 0;
        score = 0;
        {
            hyperRank = 0;
            baseRank = hyperRank;
        }

        Respawn();
        restartCnt = invincibleInterval;
        left = 2;
        {
            addedExtendScore = firstExtendScore;
            nextExtendScore = addedExtendScore;
        }

        bonusCntLength = 1;
        gameoverCnt = 0;
        turnMessageCnt = -300;
        hyperMessageCnt = -300;
        dashMessageCnt = -400;
        dashNumCnt = 5;
        stateShape.Clear();
        SetStartAreas();
        isInReplay = false;
    }

    private void SetStartAreas()
    {
        stage.AddAreas(storedAreaX - Stage.AreaCenterOffset, storedAreaY - Stage.AreaCenterOffset, Stage.AreaWidth, Stage.AreaWidth);
        stage.SetFloorShape(storedAreaX, storedAreaY);
    }

    public void StartReplay()
    {
        isInReplay = true;
        replayDataEnumerator = replay.GetEnumerator();
    }

    public void Respawn()
    {
        storedPos.Z = 24.0f;
        velZ = 0;
        {
            blurVel.Y = 0;
            blurVel.X = blurVel.Y;
        }

        {
            shotCnt = 0;
            fireCnt = shotCnt;
        }

        {
            dashVel.Y = 0;
            dashVel.X = dashVel.Y;
        }

        dashCnt = 0;
        aPressed = true;
        {
            baseMultiplier = 0;
            multiplier = baseMultiplier;
        }

        bonusCnt = 0;
        bonusCntLength = 1;
        storedIsInHyper = false;
        hyperStartCnt = 0;
        hyperRank = 0;
        {
            blinkInterval = 30;
            blinkCnt = blinkInterval;
        }

        hasShape = false;
    }

    public void Update()
    {
        isPlayedBonusGetSe = false;
        if (gameoverCnt > 0)
        {
            gameoverCnt++;
            if (gameoverCnt > 600)
            {
                RecordScore();
                frame.StartTitle();
                return;
            }

            if (pad.ButtonA || pad.ButtonStart)
            {
                if (!aPressed && gameoverCnt > 60 && !isInReplay)
                {
                    aPressed = true;
                    RecordScore();
                    frame.StartTitle();
                    return;
                }
            }
            else
            {
                aPressed = false;
            }

            return;
        }

        if (restartCnt >= 0)
        {
            restartCnt--;
            if (restartCnt > invincibleInterval)
                return;
            blinkCnt--;
            if (blinkCnt < 0)
            {
                blinkInterval = restartCnt / 8;
                blinkCnt = blinkInterval;
            }

            hasShape = (blinkCnt > blinkInterval / 2);
            if (restartCnt < 0)
                hasShape = true;
        }

        Vector2 left = new Vector2();
        if (!isInReplay)
        {
            replayData.Clear();
            left = (pad.ThumbStickLeft).Copy();
            replayData.Stick = (left).Copy();
        }
        else
        {
            if (!replayDataEnumerator.MoveNext())
            {
                frame.StartTitle();
                return;
            }

            replayData = (replayDataEnumerator.Current).Copy();
            left = (replayData.Stick).Copy();
        }

        float turnSpeed = 0.025f;
        if (dashCnt > 0)
            turnSpeed += dashCnt * 0.005f;
        float trl = 0, trr = 0;
        if (!isInReplay)
        {
            trl = pad.LeftTrigger;
            trr = pad.RightTrigger;
            if (pad.ButtonL)
                trl = 1.0f;
            if (pad.ButtonR)
                trr = 1.0f;
            replayData.LeftTrigger = trl;
            replayData.RightTrigger = trr;
        }
        else
        {
            trl = replayData.LeftTrigger;
            trr = replayData.RightTrigger;
        }

        float tr = trr - trl;
        if (tr != 0)
        {
            turnMessageCnt = 99999;
            deg += turnSpeed * tr;
        }

        tiresDeg += MathUtil.NormalizeDeg(deg - tiresDeg) * 0.1f;
        tiresDeg = MathUtil.NormalizeDeg(tiresDeg);
        Vector2 vel = (Vector2.Transform((left).Copy(), (Matrix.CreateFromYawPitchRoll(0, 0, -deg)).Copy())).Copy();
        vel *= 0.5f;
        velZ -= 0.03f;
        storedPos.Z += velZ;
        if (storedPos.Z < 1.0f && velZ < 0)
        {
            storedPos.Z = 1.0f;
            velZ *= -0.6f;
        }

        dashCnt--;
        if (!storedIsInHyper && multiplier >= 1.0f)
        {
            if (trl > 0.75f && trr > 0.75f)
            {
                hyperStartCnt++;
                if (hyperStartCnt >= 3)
                {
                    storedIsInHyper = true;
                    baseMultiplier = multiplier - (100.0f - multiplier);
                    Particle p = new Particle();
                    Quaternion qd = new Quaternion();
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 32; j++)
                        {
                            qd = (Quaternion.CreateFromAxisAngle(new Vector3(0, 0, -1), j * (float)Math.PI * 2 / 32)).Copy();
                            p.SetFixed((storedPos).Copy(), (qd).Copy(), 0.05f * multiplier * i, 60, 0, 0, 0, 150, 250, 50, 0.5f);
                            particles.Add(p);
                        }
                    }

                    hyperStartCnt = 0;
                    bonusCntLength = GameMath.integer(multiplier);
                    bullets.ChangeToBonusInRange((storedPos).Copy(), multiplier * 0.5f);
                    hyperMessageCnt = 99999;
                    sound.PlaySe("HyperStart");
                }
            }
            else
            {
                hyperStartCnt = 0;
            }
        }

        bool ba = false;
        if (!isInReplay)
        {
            ba = pad.ButtonA;
            replayData.ButtonA = ba;
            replay.Add(replayData);
        }
        else
        {
            ba = replayData.ButtonA;
        }

        if (ba)
        {
            if (!aPressed)
            {
                aPressed = true;
                if (dashCnt < 0)
                {
                    dashCnt = 10;
                    dashVel = (vel * 3).Copy();
                    Grenade g = new Grenade();
                    g.Set((storedPos).Copy(), (vel).Copy(), deg);
                    grenades.Add(g);
                    sound.PlaySe("Dash");
                    if (dashNumCnt > 0)
                    {
                        dashNumCnt--;
                        if (dashNumCnt <= 0)
                            dashMessageCnt = 99999;
                    }
                }
            }
        }
        else
        {
            aPressed = false;
        }

        dashMessageCnt++;
        vel += dashVel;
        dashVel *= 0.9f;
        if (walls.CheckHit((storedPos).Copy()))
        {
            vel.X += (storedPos.X - walls.HitPos.X) * walls.HitDistRatio * 2;
            vel.Y += (storedPos.Y - walls.HitPos.Y) * walls.HitDistRatio * 2;
            storedPos.Z += 0.1f;
        }

        Vector2 pp2 = (storedPos2).Copy();
        storedPos.X += vel.X;
        storedPos.Y += vel.Y;
        Vector3 v = Vector3.FromVector2((vel).Copy(), 0);
        stage.CheckWallHit(storedPos, v);
        storedPos2.X = storedPos.X;
        storedPos2.Y = storedPos.Y;
        areaOfs += (storedPos2 - pp2);
        bool isAreaChenged = false;
        if (areaOfs.X <= -Stage.AreaSize)
        {
            areaOfs.X += Stage.AreaSize * 2;
            storedAreaX--;
            stage.AddAreas(storedAreaX - Stage.AreaCenterOffset, storedAreaY - Stage.AreaCenterOffset, 1, Stage.AreaWidth);
            stage.RemoveAreas(storedAreaX + Stage.AreaCenterOffset + 1, storedAreaY - Stage.AreaCenterOffset, 1, Stage.AreaWidth);
            isAreaChenged = true;
        }
        else if (areaOfs.X >= Stage.AreaSize)
        {
            areaOfs.X -= Stage.AreaSize * 2;
            storedAreaX++;
            stage.AddAreas(storedAreaX + Stage.AreaCenterOffset, storedAreaY - Stage.AreaCenterOffset, 1, Stage.AreaWidth);
            stage.RemoveAreas(storedAreaX - Stage.AreaCenterOffset - 1, storedAreaY - Stage.AreaCenterOffset, 1, Stage.AreaWidth);
            isAreaChenged = true;
        }

        if (areaOfs.Y <= -Stage.AreaSize)
        {
            areaOfs.Y += Stage.AreaSize * 2;
            storedAreaY--;
            stage.AddAreas(storedAreaX - Stage.AreaCenterOffset, storedAreaY - Stage.AreaCenterOffset, Stage.AreaWidth, 1);
            stage.RemoveAreas(storedAreaX - Stage.AreaCenterOffset, storedAreaY + Stage.AreaCenterOffset + 1, Stage.AreaWidth, 1);
            isAreaChenged = true;
        }
        else if (areaOfs.Y >= Stage.AreaSize)
        {
            areaOfs.Y -= Stage.AreaSize * 2;
            storedAreaY++;
            stage.AddAreas(storedAreaX - Stage.AreaCenterOffset, storedAreaY + Stage.AreaCenterOffset, Stage.AreaWidth, 1);
            stage.RemoveAreas(storedAreaX - Stage.AreaCenterOffset, storedAreaY - Stage.AreaCenterOffset - 1, Stage.AreaWidth, 1);
            isAreaChenged = true;
        }

        if (isAreaChenged)
            stage.SetFloorShape(storedAreaX, storedAreaY);
        field.SetOffset((storedPos2).Copy(), deg);
        blurVel += (vel - blurVel) * 0.1f;
        fireCnt--;
        if (fireCnt <= 0 && ba && storedPos.Z < 10.0f)
        {
            float od = (shotCnt % 8) * 0.05f * ((shotCnt % 2) * 2 - 1);
            Fire(deg - od / 4);
            Fire(deg + od);
            shotCnt++;
            fireCnt += fireInterval;
            if (storedIsInHyper)
                sound.PlaySe("ShotHyper");
            else
                sound.PlaySe("Shot");
        }

        cnt++;
        turnMessageCnt++;
        baseRank += 0.001f;
        if (storedIsInHyper)
        {
            hyperRank += 0.00002f * multiplier * (1 + baseRank);
            baseRank += 0.00001f * multiplier;
            multiplier += (baseMultiplier - multiplier) * 0.02f * (1 - (float)bonusCnt / bonusCntLength);
            baseMultiplier -= 0.1f;
            if (bonusCnt > 0)
                bonusCnt--;
            if (multiplier <= 1.0f)
                EndHyperMode();
        }
        else
        {
            if (multiplier >= 100.0f)
                hyperMessageCnt++;
        }

        if (HasCollision)
        {
            if (bullets.CheckHit((storedPos2).Copy(), collisionWidth))
            {
                Destroy();
                return;
            }
        }
    }

    public void EndHyperMode()
    {
        multiplier = 0;
        storedIsInHyper = false;
        hyperRank = 0;
    }

    private void Destroy()
    {
        bullets.RemoveAll();
        Quaternion qd = new Quaternion();
        Particle p = new Particle();
        for (int i = 0; i < 32; i++)
        {
            qd = (Quaternion.CreateFromYawPitchRoll((float)(random.NextDouble() * Math.PI * 2), (float)(random.NextDouble() * Math.PI * 2), (float)(random.NextDouble() * Math.PI * 2))).Copy();
            for (int j = 0; j < 2; j++)
            {
                p.Set((storedPos).Copy(), (qd).Copy(), 1, 30, 200, 200, 200, 0, 0, 0, 1.0f);
                particles.AddForced(p);
            }

            for (int j = 0; j < 6; j++)
            {
                p.Set((storedPos).Copy(), (qd).Copy(), 3, 90, 250, 0, 0, 250, 200, 0, 0.33f);
                particles.AddForced(p);
            }
        }

        Respawn();
        left--;
        restartCnt = invincibleInterval + restartInterval;
        sound.PlaySe("PlayerDestroyed");
        if (left < 0)
            StartGameover();
    }

    private void StartGameover()
    {
        gameoverCnt = 1;
        frame.StartGameover();
    }

    private void RecordScore()
    {
        if (!isInReplay)
            record.RecordScore(score);
    }

    public void UpdateStateShape()
    {
        if (isInReplay)
            return;
        stateShape.BeginAdd();
        Letter.DrawQuadListShapeintintfloatfloatfloat(stateShape, score, 9, 1.75f, 1.5f, 0.1f);
        float sz = 1.0f + bonusCnt * 0.01f;
        if (!storedIsInHyper && multiplier < 100.0f)
            sz *= 0.5f;
        float x = Letter.DrawQuadListShapeintintfloatfloatfloat(stateShape, GameMath.integer(multiplier), 3, 1.7f, -1.7f, 0.15f * sz);
        if (storedIsInHyper)
            Letter.DrawQuadListShapestringfloatfloatfloat(stateShape, "x", x - sz * 0.12f, -1.7f, 0.07f * sz);
        Letter.DrawQuadListShapeintfloatfloatfloat(stateShape, GameMath.integer((Rank * 100)), 1.75f, 1.15f, 0.03f);
        Letter.DrawQuadListShapeintintfloatfloatfloat(stateShape, nextExtendScore, 9, 1.75f, 1.35f, 0.05f);
        if (gameoverCnt > 30)
        {
            Letter.DrawQuadListShapestringfloatfloatfloat(stateShape, "game over", -0.7f, 0, 0.08f);
        }
        else
        {
            if (turnMessageCnt >= 0 && turnMessageCnt < 240)
            {
                Letter.DrawQuadListShapestringfloatfloatfloat(stateShape, "push      or      to turn", -1.0f, 0, 0.04f);
                Letter.DrawQuadListShapestringfloatfloatfloat(stateShape, "ls", -0.64f, 0, 0.08f);
                Letter.DrawQuadListShapestringfloatfloatfloat(stateShape, "rs", 0, 0, 0.08f);
            }

            if (hyperMessageCnt >= 0 && hyperMessageCnt < 240)
            {
                Letter.DrawQuadListShapestringfloatfloatfloat(stateShape, "hold      and      to enter hyper mode", -1.35f, 0, 0.04f);
                Letter.DrawQuadListShapestringfloatfloatfloat(stateShape, "ls", -0.95f, 0, 0.08f);
                Letter.DrawQuadListShapestringfloatfloatfloat(stateShape, "rs", -0.22f, 0, 0.08f);
            }

            if (dashMessageCnt >= 0 && dashMessageCnt < 240)
            {
                Letter.DrawQuadListShapestringfloatfloatfloat(stateShape, "tap   to dash", -0.6f, 0.5f, 0.04f);
                Letter.DrawQuadListShapestringfloatfloatfloat(stateShape, "a", -0.34f, 0.5f, 0.08f);
            }
        }

        if (frame.PauseCnt >= 0 && (frame.PauseCnt % 60 < 30))
            Letter.DrawQuadListShapestringfloatfloatfloat(stateShape, "pause", -0.2f, -1.5f, 0.05f);
        stateShape.EndAdd();
    }

    private void Fire(float d)
    {
        Shot s = new Shot();
        s.Set((storedPos).Copy(), d, shotSpeed);
        shots.Add(s);
    }

    public void GetBonus(Vector3 p)
    {
        multiplier += 1.0f;
        if (!storedIsInHyper)
        {
            if (multiplier > 100.0f)
                multiplier = 100.0f;
        }
        else
        {
            if (multiplier > 999.0f)
                multiplier = 999.0f;
        }

        bonusCnt = bonusCntLength;
        Board b = new Board();
        b.Set((p).Copy(), (p.X - storedPos.X) * 0.1f, (p.Y - storedPos.Y) * 0.1f, 1.0f);
        QuadListShape s = playerBoards.GetShape(playerBoards.NextId);
        s.BeginAdd();
        float lx = Letter.DrawQuadListShapeintfloatfloatfloat(s, GameMath.integer(multiplier), 0, 0, 1.0f);
        s.EndAdd();
        b.Shape = s;
        playerBoards.AddForced(b);
        if (!isPlayedBonusGetSe)
        {
            isPlayedBonusGetSe = true;
            sound.PlaySe("Bonus");
        }
    }

    public void AddScoreint(int sc)
    {
        if (gameoverCnt > 0)
            return;
        if (storedIsInHyper)
            score += sc * GameMath.integer(multiplier);
        else
            score += sc;
        if (score >= nextExtendScore)
        {
            if (left < maxLeft)
            {
                left++;
                sound.PlaySe("Extend");
            }

            addedExtendScore += firstExtendScore;
            nextExtendScore += addedExtendScore;
        }
    }

    public void AddScoreintVector3(int sc, Vector3 p)
    {
        AddScoreint(sc);
        if (!storedIsInHyper)
            return;
        Board b = new Board();
        b.Set((p).Copy(), (storedPos.X - p.X) * 0.03f, (storedPos.Y - p.Y) * 0.03f, 0.3f);
        QuadListShape s = ballBoards.GetShape(ballBoards.NextId);
        s.BeginAdd();
        float lx = Letter.DrawQuadListShapeintfloatfloatfloat(s, GameMath.integer(multiplier), 0, 0, 2.0f);
        Letter.DrawQuadListShapestringfloatfloatfloat(s, "x", lx - 0.9f, 0, 1.0f);
        s.EndAdd();
        b.Shape = s;
        ballBoards.AddForced(b);
    }

    public void Draw()
    {
        bv.X = blurVel.X;
        bv.Y = blurVel.Y;
        int br = 5;
        if (dashCnt > 0)
            br += dashCnt;
        frame.Velocity = (bv * br).Copy();
        if (!hasShape)
            return;
        if (storedIsInHyper && (cnt % 2) == 0)
        {
            frame.EffectColor = new Vector4(1, 0.5f, 0.5f, 1);
            frame.BlurColor = new Vector4(1.0f, 0.5f, 0.5f, 0.2f);
        }
        else
        {
            frame.EffectColor = new Vector4(1, 1, 1, 1);
            frame.BlurColor = new Vector4(0, 0, 0, 0.2f);
        }

        frame.BlurThickness = 0.2f;
        shape.DrawDeg((storedPos).Copy(), deg);
        frame.EffectColor = new Vector4(1, 0.5f, 0.5f, 1);
        frame.BlurColor = new Vector4(0, 0, 0, 0.6f);
        frame.BlurThickness = 0.1f;
        shapeTurret.DrawDeg((storedPos).Copy(), deg);
        Vector3 tp = (storedPos).Copy();
        tp.X -= blurVel.X;
        tp.Y -= blurVel.Y;
        tp.Z -= 1.0f + (float)Math.Sin((cnt % 120) * (Math.PI * 2 / 60)) * 0.5f;
        shapeTires.DrawDeg((tp).Copy(), tiresDeg);
    }

    public void DrawState()
    {
        if (isInReplay)
            return;
        stateShape.DrawVector3((Vector3.Zero).Copy());
        frame.ChangeTechnique("BlurTech");
        frame.Velocity = (Vector4.Zero).Copy();
        Vector3 p = new Vector3();
        p.X = 3.2f;
        p.Y = -2.4f;
        float sz = 0.08f;
        for (int i = 0; i < left; i++)
        {
            frame.EffectColor = new Vector4(1, 0.5f, 0.5f, 1);
            frame.BlurColor = new Vector4(0, 0, 0, 0.6f);
            frame.BlurThickness = 0.1f;
            p.Z = -sz * 0.25f + 2.0f;
            shapeTires.DrawVector3Quaternionfloatfloatfloat((p).Copy(), (Quaternion.Identity).Copy(), sz, sz, sz * 0.25f);
            p.Z = 2.0f;
            shapeTurret.DrawVector3Quaternionfloatfloatfloat((p).Copy(), (Quaternion.Identity).Copy(), sz, sz, sz * 0.25f);
            frame.EffectColor = new Vector4(1, 1, 1, 1);
            frame.BlurColor = new Vector4(0, 0, 0, 0.2f);
            frame.BlurThickness = 0.2f;
            shape.DrawVector3Quaternionfloatfloatfloat((p).Copy(), (Quaternion.Identity).Copy(), sz, sz, sz * 0.25f);
            p.X -= 0.5f;
        }
    }

    public Vector3 Pos
    {
        get
        {
            return (storedPos).Copy();
        }
    }

    public Vector2 Pos2
    {
        get
        {
            return (storedPos2).Copy();
        }
    }

    public bool IsInHyper
    {
        get
        {
            return storedIsInHyper;
        }
    }

    public int AreaX
    {
        get
        {
            return storedAreaX;
        }
    }

    public int AreaY
    {
        get
        {
            return storedAreaY;
        }
    }

    public float Rank
    {
        get
        {
            return baseRank + hyperRank;
        }
    }

    public bool HasCollision
    {
        get
        {
            return (restartCnt < 0);
        }
    }

    public bool IsStarted
    {
        get
        {
            return (restartCnt <= invincibleInterval);
        }
    }

    public bool IsInGameover
    {
        get
        {
            return (gameoverCnt > 0);
        }
    }
}
