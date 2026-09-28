// Copyright 2008 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;

public class Field
{
    private const float baseDepth = 32.0f;
    public Vector2 Offset = new Vector2();
    public float Deg;
    private MmFrame frame;
    private Pad pad;
    private Vector2 screenOffset = new Vector2();
    private Vector2 storedSize = new Vector2();
    private Vector2 storedInnerSize = new Vector2();
    private float viewPitch;
    private float viewRoll;
    public Field(MmFrame frame, Pad pad)
    {
        this.frame = frame;
        this.pad = pad;
        storedSize = new Vector2(64.0f, 48.0f);
        storedInnerSize = new Vector2(storedSize.X * 0.8f, storedSize.Y * 0.8f);
        Offset = new Vector2();
        screenOffset = new Vector2();
    }

    public void Initiazlize()
    {
        {
            Offset.Y = 0;
            Offset.X = Offset.Y;
        }

        Deg = 0;
        {
            screenOffset.Y = 0;
            screenOffset.X = screenOffset.Y;
        }

        {
            viewRoll = 0;
            viewPitch = viewRoll;
        }
    }

    public void UpdateScreenOffset()
    {
        screenOffset.X = Offset.X + (float)Math.Sin(Deg) * 8.0f;
        screenOffset.Y = Offset.Y + (float)Math.Cos(Deg) * 8.0f;
    }

    public void SetEyePosition()
    {
        UpdateScreenOffset();
        Vector3 to = new Vector3(screenOffset.X, screenOffset.Y, 0);
        Vector2 rs = (pad.ThumbStickRight).Copy();
        viewPitch += (rs.Y - viewPitch) * 0.1f;
        float pitch = 0.3f + viewPitch * 0.9f;
        viewRoll += (rs.X - viewRoll) * 0.1f;
        float vd = -Deg - viewRoll * (float)Math.PI / 2;
        Vector3 from = (Vector3.TransformVector3Matrix((Vector3.Backward).Copy(), (Matrix.CreateFromYawPitchRoll(0, pitch, 0)).Copy())).Copy();
        from = (to + Vector3.TransformVector3Matrix((from).Copy(), (Matrix.CreateFromYawPitchRoll(0, 0, vd)).Copy()) * baseDepth).Copy();
        frame.LookAt((from).Copy(), (to).Copy(), (Vector3.TransformVector3Matrix((Vector3.Up).Copy(), (Matrix.CreateFromYawPitchRoll(0, 0, -Deg)).Copy())).Copy());
    }

    public void SetOffset(Vector2 p, float d)
    {
        Vector2 o = (p - Offset).Copy();
        float r = o.Length() - 5;
        if (r > 0)
        {
            Offset.X += (p.X - Offset.X) * MmTime.Follow(r * 0.01f);
            Offset.Y += (p.Y - Offset.Y) * MmTime.Follow(r * 0.01f);
        }

        Deg += MathUtil.NormalizeDeg(d - Deg) * SimulationTime.Blend(0.1f);
        Deg = MathUtil.NormalizeDeg(Deg);
    }

    public bool Contains(Vector2 p)
    {
        p -= screenOffset;
        return (p.Length() < 96.0f);
    }

    public bool ContainsInner(Vector2 p)
    {
        p -= screenOffset;
        return (p.Length() < 80.0f);
    }

    public Vector2 Size
    {
        get
        {
            return (storedSize).Copy();
        }
    }

    public Vector2 InnerSize
    {
        get
        {
            return (storedInnerSize).Copy();
        }
    }
}
