// Copyright 2008 Kenta Cho. Some rights reserved.
using System;

public class Title
{
    private MmFrame frame;
    private Pad pad;
    private Record record;
    private float cnt;
    private bool sPressed;
    private QuadListShape shape;
    private BlurNormalTextureCubeListShape ballShape;
    private Vector3 ballPos = new Vector3();
    private Quaternion ballDir = new Quaternion();
    private Quaternion dirVel = new Quaternion();
    private int[] scores;
    public Title(MmFrame frame, Pad pad, Record record)
    {
        this.frame = frame;
        this.pad = pad;
        this.record = record;
        shape = new QuadListShape(frame);
        shape.Initializeintbytebytebytebyte(1024, 10, 10, 10, 255);
        ballShape = BallShape.CreateShape(frame);
        ballPos = new Vector3(0.95f, 0.5f, 0);
        ballDir = (Quaternion.Identity).Copy();
        dirVel = (Quaternion.CreateFromYawPitchRoll(0.01f, 0.005f, 0.0025f)).Copy();
    }

    public void Start()
    {
        cnt = 0;
        sPressed = true;
        scores = record.Scores;
    }

    public void Update()
    {
        cnt += SimulationTime.Step;
        ballDir *= MmTime.Rotation(dirVel);
        if (pad.ButtonStart || pad.ButtonA || pad.ButtonL || pad.ButtonR)
        {
            if (!sPressed)
            {
                frame.StartInGame();
                return;
            }
        }
        else
        {
            sPressed = false;
        }

        shape.BeginAdd();
        Letter.DrawQuadListShapestringfloatfloatfloat(shape, "mazer", -1.5f, 1.2f, 0.06f);
        Letter.DrawQuadListShapestringfloatfloatfloat(shape, "mayhem", -1.4f, 1.0f, 0.06f);
        if (cnt % 60 < 30)
            Letter.DrawQuadListShapestringfloatfloatfloat(shape, "press fire button to start", -0.19f, -1.79f, 0.04f);
        float y = 0.5f;
        for (int i = 0; i < Record.RankingNum; i++)
        {
            Letter.DrawQuadListShapeintfloatfloatfloat(shape, i + 1, 0.8f, y, 0.05f);
            Letter.DrawFixedSize(shape, scores[(i)], 9, 1.8f, y, 0.04f);
            y -= 0.175f;
        }

        y -= 0.175f;
        if (record.LastRank >= 0)
            Letter.DrawQuadListShapeintfloatfloatfloat(shape, record.LastRank + 1, 0.8f, y, 0.05f);
        if (record.LastScore > 0)
            Letter.DrawFixedSize(shape, record.LastScore, 9, 1.8f, y, 0.05f);
        shape.EndAdd();
    }

    public void Draw()
    {
        frame.ChangeTechnique("BlurAlphaLightingTech");
        frame.LookAt(new Vector3(0, 0, -1), (Vector3.Zero).Copy(), (Vector3.Up).Copy());
        frame.EffectColor = new Vector4(1, 1, 1, 1.0f);
        frame.BlurColor = new Vector4(0.2f, 0.2f, 0.2f, 0.5f);
        frame.BlurThickness = 0.1f;
        frame.Velocity = (Vector4.Zero).Copy();
        ballShape.DrawVector3Quaternionfloat((ballPos).Copy(), (ballDir).Copy(), 0.2f);
        frame.ChangeTechnique("BoardTech");
        frame.DepthEnabled = false;
        frame.WorldMatrix = (Matrix.Identity).Copy();
        shape.Draw();
        frame.DepthEnabled = true;
    }
}
