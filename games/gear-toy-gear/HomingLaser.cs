// Copyright 2009 Kenta Cho. Some rights reserved.
using System;

public class HomingLaserPool : ActorPool<HomingLaser>
{
    private GtgFrame frame;
    private Field field;
    private LaserPool lasers;
    private Player player;
    private PillarPool pillars;
    private ParticlePool particles;
    private Sound sound;
    public HomingLaserPool(int n, GtgFrame frame, Field field, LaserPool lasers, Player player, PillarPool pillars, ParticlePool particles, Sound sound) : base(n, () => new HomingLaser())
    {
        this.frame = frame;
        this.field = field;
        this.lasers = lasers;
        this.player = player;
        this.pillars = pillars;
        this.particles = particles;
        this.sound = sound;
    }

    public override void Clear()
    {
        ForEach((HomingLaser a) =>
        {
            if (a.Cue != null)
                a.Cue.Stop(AudioStopOptions.Immediate);
        });
        base.Clear();
    }

    public void Add(Vector3 from, Vector3 to, float speed)
    {
        HomingLaser a = new HomingLaser();
        a.Pos = (from).Copy();
        a.Vel = (to - from).Copy();
        a.Vel.Normalize();
        a.Vel *= speed;
        AddT(a);
    }

    public override bool UpdateT(HomingLaser a)
    {
        a.Pos += (a.Vel * Stage.GameSpeedSqrt) * SimulationTime.Step;
        if (pillars.CheckHit((a.Pos).Copy()))
        {
            particles.AddintVector3Vector3floatfloatfloatfloatfloatfloat(24, (a.Pos).Copy(), (a.Vel).Copy(), 0.5f, 10, 0.8f, 1.0f, 0.4f, 0.7f);
            return false;
        }

        float tt = -a.Pos.Z / (a.Vel.Z * Stage.GameSpeed);
        if (tt < 1)
            tt = 1;
        float tx = a.Pos.X + a.Vel.X * tt;
        float ty = a.Pos.Y + a.Vel.Y * tt;
        a.Vel.X += ((player.Pos.X + player.Vel.X * tt - tx) * (0.05f / tt)) * SimulationTime.Step;
        a.Vel.Y += ((player.Pos.Y + player.Vel.Y * tt - ty) * (0.05f / tt)) * SimulationTime.Step;
        if (SimulationTime.Emit) lasers.AddVector3Vector3floatint((a.Pos).Copy(), (a.Vel).Copy(), a.Vel.Length() * 0.8f, 25);
        if (SimulationTime.Emit) particles.AddintVector3Vector3floatfloatfloatfloatfloatfloat(6, (a.Pos).Copy(), (a.Vel).Copy(), 0.1f, 15, 0.4f, 1, 0.2f, 0.8f);
        if (a.Pos.Z > -10 && Vector3.Distance((a.Pos).Copy(), (player.Pos).Copy()) < 5.0f)
        {
            player.Destroy();
        }

        if (a.Cue == null)
        {
            a.Cue = sound.GetCue("HomingLaser");
            a.Listener = new AudioListener();
            a.Emitter = new AudioEmitter();
            a.Cue.Apply3D(a.Listener, a.Emitter);
            a.Cue.Play();
        }

        a.Listener.Position = (sound.ListenerPos).Copy();
        a.Emitter.Position = (a.Pos).Copy();
        a.Emitter.Velocity = (a.Vel).Copy();
        a.Cue.Apply3D(a.Listener, a.Emitter);
        return (a.Pos.Z < Field.FrontDepth * 2);
    }

    public override void Remove(int i)
    {
        if (Actors[(i)].Cue != null)
        {
            Actors[(i)].Cue.Stop(AudioStopOptions.Immediate);
            Actors[(i)].Cue = null;
        }

        base.Remove(i);
    }
}

public class HomingLaser : ActorCopy
{
    public Vector3 Pos = new Vector3();
    public Vector3 Vel = new Vector3();
    public Cue Cue;
    public AudioListener Listener;
    public AudioEmitter Emitter;
    public HomingLaser Copy()
    {
        return new HomingLaser
        {
            Pos = Pos.Copy(),
            Vel = Vel.Copy(),
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
