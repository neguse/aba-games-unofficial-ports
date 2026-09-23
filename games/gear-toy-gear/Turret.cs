// Copyright 2009 Kenta Cho. Some rights reserved.
using System;

public class TurretPool : ActorPool<Turret>
{
    private const float LaserBaseLength = 7.0f;
    private GtgFrame frame;
    private Field field;
    private LaserPool lasers;
    private PillarPool pillars;
    private Player player;
    private ParticlePool particles;
    private Sound sound;
    private MiddleEnemyPool enemies;
    public TurretPool(int n, GtgFrame frame, Field field, LaserPool lasers, PillarPool pillars, Player player, ParticlePool particles, Sound sound) : base(n, () => new Turret())
    {
        this.frame = frame;
        this.field = field;
        this.lasers = lasers;
        this.pillars = pillars;
        this.player = player;
        this.particles = particles;
        this.sound = sound;
    }

    public void SetParams(MiddleEnemyPool enemies)
    {
        this.enemies = enemies;
    }

    public override void Clear()
    {
        ForEach((Turret a) =>
        {
            if (a.Cue != null)
                a.Cue.Stop(AudioStopOptions.Immediate);
        });
        base.Clear();
    }

    public int Add(Vector3 pos, Vector3 vel, bool hasLaser)
    {
        Turret a = new Turret();
        a.Pos = (pos).Copy();
        a.TargetVel = (vel).Copy();
        a.Vel = (vel * 14).Copy();
        if (hasLaser)
        {
            a.HasLaser = true;
            a.LaserLength = 0;
            a.LaserSpeed = 5.0f;
        }

        return AddT(a);
    }

    public override bool UpdateT(Turret a)
    {
        float pl = a.Pos.Length();
        bool isInTube = (pl < Tube.Radius * 1.2f);
        if (isInTube)
        {
            a.IsInTube = true;
            a.Vel += (a.TargetVel - a.Vel) * 0.2f;
        }
        else
        {
            if (a.IsInTube)
                return false;
        }

        if (a.SourcePos.Z > Field.FireBoundaryDepth * 2)
            return false;
        a.Pos += a.Vel * Stage.GameSpeedSqrt;
        if (!a.HasLaser)
            return true;
        if (a.Cue == null)
        {
            a.Cue = sound.GetCue("Laser");
            a.Listener = new AudioListener();
            a.Emitter = new AudioEmitter();
            a.Cue.Apply3D(a.Listener, a.Emitter);
            a.Listener.Position = (sound.ListenerPos).Copy();
            a.Cue.Play();
        }

        a.LaserLength += a.LaserSpeed * Stage.GameSpeed;
        Vector3 v = (a.Pos - a.SourcePos).Copy();
        v.Normalize();
        Vector3 p = (a.SourcePos).Copy();
        float l = a.LaserLength % LaserBaseLength;
        p += v * l;
        v *= LaserBaseLength;
        int i = GameMath.integer((float)(a.LaserLength));
        for (;; l += LaserBaseLength)
        {
            if (p.X * p.X + p.Y * p.Y > Tube.Radius * Tube.Radius || pillars.CheckHit((p).Copy()))
            {
                a.LaserLength = l;
                particles.AddintVector3Vector3floatfloatfloatfloatfloatfloat(4, (p).Copy(), (v).Copy(), 0.2f, 15, 0.2f, 1, 0.4f, 0.8f);
                a.Emitter.Position = (p).Copy();
                break;
            }

            if (p.Z > -10 && Vector3.Distance((p).Copy(), (player.Pos).Copy()) < 5.0f)
            {
                player.Destroy();
            }

            lasers.AddVector3Vector3float((p).Copy(), (v).Copy(), LaserBaseLength * 0.8f);
            if (i % 12 == 0)
                particles.AddintVector3Vector3floatfloatfloatfloatfloatfloat(1, (p).Copy(), (v).Copy(), 0.1f, 7, 0.2f, 1, 0.4f, 0.8f);
            p += v;
            i++;
            if (l > a.LaserLength || p.Z > Field.FrontDepth)
            {
                a.Emitter.Position = (p).Copy();
                break;
            }
        }

        a.Listener.Position = (sound.ListenerPos).Copy();
        a.Cue.Apply3D(a.Listener, a.Emitter);
        return true;
    }

    public override void Remove(int i)
    {
        if (Actors[(i)].HasLaser)
        {
            Actors[(i)].Cue.Stop(AudioStopOptions.Immediate);
            Actors[(i)].Cue = null;
        }

        enemies.OnTurretRemovedint(i);
        base.Remove(i);
    }
}

public class Turret : ActorCopy
{
    public Vector3 Pos = new Vector3();
    public Vector3 TargetVel = new Vector3();
    public Vector3 Vel = new Vector3();
    public bool IsInTube;
    public bool HasLaser;
    public float LaserLength;
    public float LaserSpeed;
    public Vector3 SourcePos = new Vector3();
    public Cue Cue;
    public AudioListener Listener;
    public AudioEmitter Emitter;
    public Turret Copy()
    {
        return new Turret
        {
            Pos = Pos.Copy(),
            TargetVel = TargetVel.Copy(),
            Vel = Vel.Copy(),
            IsInTube = IsInTube,
            HasLaser = HasLaser,
            LaserLength = LaserLength,
            LaserSpeed = LaserSpeed,
            SourcePos = SourcePos.Copy(),
            Cue = Cue,
            Listener = Listener,
            Emitter = Emitter
        };
    }

    public ActorCopy CopyActor()
    {
        return Copy();
    }
}
