// Copyright 2009 Kenta Cho. Some rights reserved.
using System;

public class Title
{
    private GtgFrame frame;
    private Pad pad;
    private Record record;
    private bool isButtonPressed;
    private int ticks;
    private GearShape shape;
    private GearData[] data;
    public Title(GtgFrame frame, Pad pad, Record record)
    {
        this.frame = frame;
        this.pad = pad;
        this.record = record;
        shape = new GearShape(frame, 1, 0.8f, 0.2f, new Color(255, 255, 255, 255), new Color(255, 255, 255, 255), false);
        data = GtgArrays.Make(1, () => new GearData());
        data[(0)].Pos.X = -12;
        data[(0)].Pos.Y = 11;
        data[(0)].Pos.Z = 0;
        {
            data[(0)].Color.W = 1;
            data[(0)].Color.Z = data[(0)].Color.W;
            data[(0)].Color.Y = data[(0)].Color.Z;
            data[(0)].Color.X = data[(0)].Color.Y;
        }

        data[(0)].Scale = 2;
        data[(0)].Orientation = (Quaternion.Identity).Copy();
    }

    public void Initialize()
    {
        isButtonPressed = true;
        ticks = 0;
    }

    public void Update()
    {
        int si = pad.CheckGameStartPressed();
        if (si >= 0)
        {
            if (!isButtonPressed)
            {
                if (si < 4)
                    pad.SetGamePadPlayerIndex(si);
                frame.StartGame();
            }
        }
        else
        {
            isButtonPressed = false;
        }

        ticks++;
    }

    public void Draw()
    {
        frame.LookAt(new Vector3(0, 0, 25), (Vector3.Zero).Copy(), new Vector3(0, 1, 0));
        Letter.AddstringVector3floatQuaternionfloat("GEAR", new Vector3(-20, 14, 0), 0.8f, (Quaternion.Identity).Copy(), 1);
        Letter.AddstringVector3floatQuaternionfloat("TOY", new Vector3(-20, 12, 0), 0.6f, (Quaternion.Identity).Copy(), 1);
        Letter.AddstringVector3floatQuaternionfloat("GEAR", new Vector3(-20, 10, 0), 0.8f, (Quaternion.Identity).Copy(), 1);
        Shape.BeginAddInstance();
        for (int i = 0; i < 1; i++)
        {
            Shape.AddInstance((data[(i)].Pos).Copy(), data[(i)].Scale, (data[(i)].Orientation).Copy(), (data[(i)].Color).Copy());
        }

        shape.Draw();
        if (ticks % 60 < 30)
            Letter.AddstringVector3floatQuaternionfloat("PRESS BUTTON", new Vector3(-20, -14, 0), 0.7f, (Quaternion.Identity).Copy(), 1);
        int rc = ticks / 30;
        if (rc > 10)
            rc = 10;
        float y = 14;
        for (int i = 0; i < rc; i++)
        {
            float ls = 0.5f;
            if (record.LastRank == i)
                ls = 1.0f;
            Letter.AddintVector3floatQuaternionfloat(i + 1, new Vector3(10, y, 0), ls, (Quaternion.Identity).Copy(), 1);
            Letter.AddintVector3floatQuaternionfloat(record.Scores[(i)], new Vector3(20, y, 0), 0.5f, (Quaternion.Identity).Copy(), 1);
            y -= 2;
        }
    }
}

public class GearData : ActorCopy
{
    public Vector3 Pos = new Vector3();
    public float Scale;
    public Quaternion Orientation = new Quaternion();
    public Vector4 Color = new Vector4();
    public GearData Copy()
    {
        return new GearData
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
