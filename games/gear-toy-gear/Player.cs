// Copyright 2009 Kenta Cho. Some rights reserved.
using System;

public class Player
{
    private const float speed = 3.0f;
    private const int invincibleDuration = 180;
    private const int restartDuration = 240;
    private const int fireInterval = 2;
    private const int homingLaserFireInterval = 10;
    public Vector3 Pos = new Vector3(), Vel = new Vector3();
    private Vector3 ppos = new Vector3();
    private GtgFrame frame;
    private GameState gameState;
    private Field field;
    private Pad pad;
    private Replay replay;
    private ParticlePool particles;
    private Sound sound;
    private ShotPool shots;
    private PlayerHomingLaserPool homingLasers;
    private EnemyPool enemies;
    private MiddleEnemyPool middleEnemies;
    private Quaternion shotOrientation = new Quaternion();
    private Shape shape;
    private Shape edgeShape;
    private int fireTicks, fireAngleTicks;
    private int homingLaserFireTicks;
    private int invincibleTicks;
    private PlayerData[] data;
    private float depthSpeed;
    private bool isInReplay;
    private float accel;
    public Player(GtgFrame frame, GameState gameState, Field field, Pad pad, Replay replay, ParticlePool particles, Sound sound)
    {
        this.frame = frame;
        this.gameState = gameState;
        this.field = field;
        this.pad = pad;
        this.replay = replay;
        this.particles = particles;
        this.sound = sound;
        shape = new GearShape(frame, 1, 0.8f, 0.2f, new Color(250, 100, 100, 240), new Color(250, 200, 200, 240), true);
        edgeShape = new GearEdgeShape(frame, 1, 0.8f, 0.5f, 0.1f);
        data = GtgArrays.Make(4, () => new PlayerData());
        for (int i = 0; i < 4; i++)
        {
            data[(i)].Pos.Z = 0;
            data[(i)].Scale = 2;
            data[(i)].Color.X = 1;
            data[(i)].Color.Y = 0;
            data[(i)].Color.Z = 0;
            data[(i)].Color.W = 1;
        }

        shotOrientation = (Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), 0)).Copy();
    }

    public void SetParams(ShotPool shots, PlayerHomingLaserPool homingLasers, EnemyPool enemies, MiddleEnemyPool middleEnemies)
    {
        this.shots = shots;
        this.homingLasers = homingLasers;
        this.enemies = enemies;
        this.middleEnemies = middleEnemies;
    }

    public void Initialize()
    {
        Pos.X = 0;
        Pos.Y = -Tube.Radius * 0.3f;
        Pos.Z = 0;
        ppos = (Pos).Copy();
        Vel = (Vector3.Zero).Copy();
        {
            fireAngleTicks = 0;
            fireTicks = fireAngleTicks;
        }

        homingLaserFireTicks = 0;
        invincibleTicks = -1;
        depthSpeed = 1;
        accel = 0;
    }

    public void StartRecord()
    {
        isInReplay = false;
        replay.StartRecord();
    }

    public void StartReplay()
    {
        isInReplay = true;
        replay.StartReplay();
    }

    public void SetLookAt()
    {
        frame.LookAt(new Vector3(Pos.X * 0.8f, Pos.Y * 0.8f, 36), new Vector3(Pos.X * 0.7f, Pos.Y * 0.7f, 0), new Vector3(0, 1, 0));
    }

    public void Update()
    {
        if (gameState.IsInGameOver)
        {
            Stage.GameSpeed += (1 - Stage.GameSpeed) * 0.1f;
            return;
        }

        if (invincibleTicks >= 0)
        {
            invincibleTicks--;
            if (invincibleTicks > invincibleDuration)
            {
                Stage.GameSpeed += (1 - Stage.GameSpeed) * 0.1f;
                return;
            }
        }

        ReplayData rd = new ReplayData();
        if (isInReplay && replay.HasNext())
            rd = (replay.Get()).Copy();
        Vector2 stick = new Vector2();
        if (isInReplay)
        {
            stick = (rd.Stick).Copy();
        }
        else
        {
            stick = (pad.ThumbStickLeft + pad.ThumbStickRight).Copy();
            rd.Stick = (stick).Copy();
        }

        float sa = (float)Math.Atan2(stick.X, stick.Y);
        float sl = stick.Length();
        if (sl > 1)
            sl = 1;
        sl *= speed * Stage.GameSpeedSqrt;
        Pos.X += (float)Math.Sin(sa) * sl;
        Pos.Y += (float)Math.Cos(sa) * sl;
        float td = Pos.X * Pos.X + Pos.Y * Pos.Y;
        if (td > Tube.Radius * 0.9f * Tube.Radius * 0.9f)
        {
            td = (float)Math.Sqrt(td);
            td = Tube.Radius * 0.9f / td;
            Pos.X *= td;
            Pos.Y *= td;
        }

        Vel = (Pos - ppos).Copy();
        ppos = (Pos).Copy();
        float lt;
        if (isInReplay)
        {
            lt = rd.LeftTrigger;
        }
        else
        {
            lt = pad.LeftTrigger;
            if (pad.ButtonL || pad.ButtonA)
                lt = 1;
            rd.LeftTrigger = lt;
        }

        if (lt > 0)
        {
            depthSpeed += (1 - pad.LeftTrigger - depthSpeed) * 0.2f;
            if (depthSpeed < 1)
                depthSpeed = 1;
        }

        float rt;
        if (isInReplay)
        {
            rt = rd.RightTrigger;
        }
        else
        {
            rt = pad.RightTrigger;
            if (pad.ButtonR || pad.ButtonB)
                rt = 1;
            rd.RightTrigger = rt;
        }

        if (rt > 0)
        {
            depthSpeed += rt * accel;
            if (depthSpeed > 1.25f)
                gameState.OnAccelPressed();
        }

        depthSpeed += (1 - depthSpeed) * 0.005f;
        Stage.GameSpeed += (depthSpeed - Stage.GameSpeed) * 0.05f;
        if (!isInReplay)
            replay.Add(rd);
        fireTicks--;
        fireAngleTicks++;
        if (fireTicks <= 0)
        {
            fireTicks = fireInterval;
            shots.Add((shotOrientation).Copy());
            float fa = fireAngleTicks * 0.02f;
            float foa = 0;
            for (int i = 0; i < 4; i++)
            {
                Quaternion fo = (Quaternion.CreateFromAxisAngle(new Vector3((float)Math.Sin(fa + foa), (float)Math.Cos(fa + foa), 1), (float)Math.Sin(fa * 5) * 0.15f)).Copy();
                shots.Add((shotOrientation * fo).Copy());
                foa += (float)Math.PI / 2;
            }
        }

        homingLaserFireTicks--;
        if (homingLaserFireTicks <= 0)
        {
            int ei;
            ei = enemies.GetNearest(Pos.X, Pos.Y, 50);
            if (ei >= 0)
            {
                homingLasers.Add(false, ei, 12.0f, fireAngleTicks * 1.0f);
                sound.PlaySe("PlayerLaser");
                homingLaserFireTicks = homingLaserFireInterval;
            }
            else
            {
                ei = middleEnemies.GetNearest(Pos.X, Pos.Y, 50);
                if (ei >= 0)
                {
                    homingLasers.Add(true, ei, 8.0f, fireAngleTicks * 1.0f);
                    sound.PlaySe("PlayerLaser");
                    homingLaserFireTicks = homingLaserFireInterval;
                }
            }
        }

        sound.ListenerPos = (Pos).Copy();
    }

    public void Destroy()
    {
        if (invincibleTicks >= 0)
            return;
        particles.Clear();
        particles.AddintVector3floatfloatfloatfloatfloatfloatfloat(128, (Pos).Copy(), 5, 0, 30, 0.5f, 0.5f, 1, 1);
        particles.AddintVector3floatfloatfloatfloatfloatfloatfloat(1024, (Pos).Copy(), 20, 1, 10, 1, 0.5f, 1, 1);
        invincibleTicks = restartDuration;
        depthSpeed = 1;
        sound.PlaySe("PlayerDestroyed");
        gameState.Miss();
    }

    public bool IsActive
    {
        get
        {
            return invincibleTicks <= invincibleDuration;
        }
    }

    public void Draw()
    {
        if (invincibleTicks > invincibleDuration || invincibleTicks % 30 > 15)
            return;
        Shape.BeginAddInstance();
        float a = fireAngleTicks * 0.05f;
        for (int i = 0; i < 4; i++)
        {
            data[(i)].Pos.X = Pos.X + (float)Math.Sin(a) * 2;
            data[(i)].Pos.Y = Pos.Y + (float)Math.Cos(a) * 2;
            data[(i)].Orientation = (Quaternion.CreateFromYawPitchRoll(a, a, a)).Copy();
            a += (float)Math.PI / 2;
        }

        for (int i = 0; i < 4; i++)
        {
            Shape.AddInstance((data[(i)].Pos).Copy(), data[(i)].Scale, (data[(i)].Orientation).Copy(), (data[(i)].Color).Copy());
        }

        shape.Draw();
    }

    public void IncrementAccel()
    {
        accel += 0.01f;
    }
}

public class PlayerData : ActorCopy
{
    public Vector3 Pos = new Vector3();
    public float Scale;
    public Quaternion Orientation = new Quaternion();
    public Vector4 Color = new Vector4();
    public PlayerData Copy()
    {
        return new PlayerData
        {
            Pos = Pos.Copy(),
            Scale = Scale,
            Orientation = Orientation.Copy(),
            Color = Color.Copy()
        };
    }

    public ActorCopy CopyActor()
    {
        return Copy();
    }
}
