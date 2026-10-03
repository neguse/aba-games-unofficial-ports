// Copyright 2008 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;

public class Stage
{
    public static Random Random;
    public const float AreaSize = 90.0f;
    public const int AreaWidth = 3;
    public const int AreaCenterOffset = 1;
    private MmFrame frame;
    private WallPool walls;
    private Sound sound;
    private Player player;
    private BallPool balls;
    private TurretPool turrets;
    private BulletPool bullets;
    private Area[][] areas;
    private int stageAreaWidth, stageAreaHeight;
    private int playerStartAreaX, playerStartAreaY;
    private QuadListShape floorShape;
    private float appearanceCnt;
    public float appearanceCntDec;
    public float appearanceWaitCnt;
    private int bgmIndex;
    private int bgmIndexOfs;
    public static void SetRandomSeed(Int32 s)
    {
        Random = new Random(s);
    }

    public Stage(MmFrame frame, WallPool walls, Sound sound)
    {
        this.frame = frame;
        this.walls = walls;
        this.sound = sound;
        floorShape = new QuadListShape(frame);
        floorShape.Initializeintbytebytebytebyte(AreaWidth * AreaWidth * 9, 100, 100, 0, 100);
    }

    public void SetParams(Player player, BallPool balls, TurretPool turrets, BulletPool bullets)
    {
        this.player = player;
        this.balls = balls;
        this.turrets = turrets;
        this.bullets = bullets;
    }

    public void SetFloorShape(int x, int y)
    {
        floorShape.BeginAdd();
        for (int bx = x - AreaCenterOffset; bx <= x + AreaCenterOffset; bx++)
            for (int by = y - AreaCenterOffset; by <= y + AreaCenterOffset; by++)
                if (areas[(bx)][(by)].HasFloor)
                    areas[(bx)][(by)].AddFloorQuadListShape(floorShape);
        floorShape.EndAdd();
    }

    private string[] bgmNames = new string[]
    {
        "Mm1",
        "Mm2",
        "Mm3"
    };
    public void Update()
    {
        if (appearanceWaitCnt > 0)
        {
            appearanceWaitCnt -= SimulationTime.Step;
            if (appearanceWaitCnt <= 0)
            {
                if (!player.IsInGameover && frame.IsInGame)
                    sound.PlayBgm(bgmNames[(bgmIndex)]);
                bgmIndex += bgmIndexOfs;
                if (bgmIndex >= bgmNames.Length)
                    bgmIndex = 0;
                else if (bgmIndex < 0)
                    bgmIndex = bgmNames.Length - 1;
            }

            return;
        }

        appearanceCnt -= appearanceCntDec / (balls.Length() + 0.1f) * SimulationTime.Step;
        appearanceCntDec += 0.0075f * SimulationTime.Step;
        if (appearanceCntDec > 30.0f)
        {
            appearanceCntDec = -10000.0f;
            areas[(1)][(1)].AddBossBall();
        }

        if (appearanceCnt <= 0)
        {
            areas[(1)][(1)].AddBalls();
            appearanceCnt = 200.0f;
        }
    }

    public void GoToNextStage()
    {
        balls.DestroyAll();
        bullets.ChangeToBonusAll();
        player.EndHyperMode();
        sound.FadeoutBgm();
        appearanceWaitCnt = 180;
        appearanceCnt = 0.0f;
        appearanceCntDec = 10.0f;
    }

    public void Draw()
    {
        floorShape.Draw();
    }

    public void AddAreas(int sx, int sy, int w, int h)
    {
        for (int x = sx; x < sx + w; x++)
            for (int y = sy; y < sy + h; y++)
                areas[(x)][(y)].Add();
    }

    public void RemoveAreas(int sx, int sy, int w, int h)
    {
        for (int x = sx; x < sx + w; x++)
            for (int y = sy; y < sy + h; y++)
                areas[(x)][(y)].Remove();
    }

    private int[][] degOfs = new int[][]
    {
        new int[]
        {
            0,
            -1
        },
        new int[]
        {
            1,
            0
        },
        new int[]
        {
            0,
            1
        },
        new int[]
        {
            -1,
            0
        },
        new int[]
        {
            1,
            -1
        },
        new int[]
        {
            1,
            1
        },
        new int[]
        {
            -1,
            1
        },
        new int[]
        {
            -1,
            -1
        }
    };
    public void CreateMaze(List<string> data)
    {
        int w, h;
        w = (data[(0)].Length - 1) / 2 + 2;
        h = (data.Count - 1) / 2 + 2;
        stageAreaWidth = w;
        stageAreaHeight = h;
        areas = MmArrays.Make(w, () => MmArrays.Make(h, () => (Area)null));
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                areas[(x)][(y)] = new Area(this, balls, turrets, bullets, walls, player, x * AreaSize * 2, y * AreaSize * 2);
        int[][] mazeBlocks = MmArrays.Make(w * 2 + 1, () => MmArrays.Make(h * 2 + 1, () => 0));
        for (int y = 0; y < h * 2 - 3; y++)
        {
            string l = data[(y)];
            for (int x = 0; x < w * 2 - 3; x++)
            {
                switch (l[(x)])
                {
                    case '0':
                        mazeBlocks[(x)][(y)] = 0;
                        break;
                    case '1':
                        mazeBlocks[(x)][(y)] = 1;
                        break;
                    case '2':
                        mazeBlocks[(x)][(y)] = 2;
                        break;
                }
            }
        }

        for (int x = 1; x < w - 1; x++)
        {
            for (int y = 1; y < h - 1; y++)
            {
                int mb = mazeBlocks[(x * 2 - 1)][(y * 2 - 1)];
                if (mb != 1)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (mazeBlocks[(x * 2 - 1 + degOfs[(i)][(0)])][(y * 2 - 1 + degOfs[(i)][(1)])] == 1)
                        {
                            areas[(x)][(y)].HasWalls[(i)] = true;
                            areas[(x)][(y)].SetWalls(i);
                        }
                    }

                    for (int i = 4; i < 8; i++)
                    {
                        if (mazeBlocks[(x * 2 - 1 + degOfs[(i)][(0)])][(y * 2 - 1 + degOfs[(i)][(1)])] == 1)
                            areas[(x)][(y)].HasWalls[(i)] = true;
                    }

                    if (mb != 2)
                    {
                        areas[(x)][(y)].SetWallBalls();
                    }
                    else
                    {
                        playerStartAreaX = x;
                        playerStartAreaY = y;
                    }

                    areas[(x)][(y)].HasFloor = true;
                }
            }
        }

        player.SetStartPos(playerStartAreaX, playerStartAreaY);
        bgmIndex = Random.Nextint(bgmNames.Length);
        bgmIndexOfs = Random.Nextint(2) * 2 - 1;
        appearanceWaitCnt = 1;
        appearanceCnt = 0.0f;
        appearanceCntDec = 10.0f;
    }

    public void CheckWallHit(Vector3 p, Vector3 v)
    {
        areas[(GameMath.integer(((p.X + AreaSize) / (AreaSize * 2))))][(GameMath.integer(((p.Y + AreaSize) / (AreaSize * 2))))].CheckWallHit(p, v);
    }
}

public class Area
{
    private static BallAppearance noBallAppearance;
    public const float WallWidth = 0.15f;
    public bool[] HasWalls = MmArrays.Make(8, () => false);
    public bool HasFloor;
    public AreaAreaType Type;
    public float Rank;
    private Stage stage;
    private BallPool balls;
    private TurretPool turrets;
    private BulletPool bullets;
    private WallPool walls;
    private Player player;
    private Vector2 pos = new Vector2();
    private Vector3 pos3 = new Vector3();
    private Appearance[] appearances = MmArrays.Make(16, () => new Appearance());
    private int appearancesNum;
    private AppearanceWall[] appearanceWalls = MmArrays.Make(32, () => new AppearanceWall());
    private int appearanceWallsNum;
    public Area(Stage stage, BallPool balls, TurretPool turrets, BulletPool bullets, WallPool walls, Player player, float x, float y)
    {
if(noBallAppearance==null)noBallAppearance=new BallAppearance();
        this.stage = stage;
        this.balls = balls;
        this.turrets = turrets;
        this.bullets = bullets;
        this.walls = walls;
        this.player = player;
        Clear();
        {
            pos3.X = x;
            pos.X = pos3.X;
        }

        {
            pos3.Y = y;
            pos.Y = pos3.Y;
        }
    }

    private void Clear()
    {
        for (int i = 0; i < HasWalls.Length; i++)
            HasWalls[(i)] = false;
        HasFloor = false;
        Type = AreaAreaType.Small;
        Rank = 0;
        {
            pos.Y = 0;
            pos.X = pos.Y;
        }

        {
            pos3.Z = 0;
            pos3.Y = pos3.Z;
            pos3.X = pos3.Y;
        }

        for (int i = 0; i < appearances.Length; i++)
            appearances[(i)].Clear();
        appearancesNum = 0;
        for (int i = 0; i < appearanceWalls.Length; i++)
            appearanceWalls[(i)].Clear();
        appearanceWallsNum = 0;
    }

    private float[] floorPoints = new float[]
    {
        -1,
        -1 + WallWidth * 2.5f,
        1 - WallWidth * 2.5f,
        1
    };
    private int[][] floorWallParams = new int[][]
    {
        new int[]
        {
            1,
            2
        },
        new int[]
        {
            7,
            11
        },
        new int[]
        {
            13,
            14
        },
        new int[]
        {
            4,
            8
        },
        new int[]
        {
            3,
            3
        },
        new int[]
        {
            15,
            15
        },
        new int[]
        {
            12,
            12
        },
        new int[]
        {
            0,
            0
        }
    };
    private float[] pointsAlpha = MmArrays.Make(16, () => 0f);
    public void AddFloorQuadListShape(QuadListShape shape)
    {
        for (int i = 0; i < 16; i++)
            pointsAlpha[(i)] = 0.3f;
        for (int i = 0; i < 8; i++)
        {
            if (HasWalls[(i)])
            {
                pointsAlpha[(floorWallParams[(i)][(0)])] = 0;
                pointsAlpha[(floorWallParams[(i)][(1)])] = 0;
            }
        }

        for (int i = 0; i < 9; i++)
            AddFloorQuadListShapeint(shape, (i % 3) + GameMath.integer((i / 3)) * 4);
    }

    private void AddFloorQuadListShapeint(QuadListShape shape, int idx)
    {
        AddPoint(shape, idx);
        AddPoint(shape, idx + 1);
        AddPoint(shape, idx + 5);
        AddPoint(shape, idx + 4);
    }

    private void AddPoint(QuadListShape shape, int idx)
    {
        int xi = idx % 4;
        int yi = GameMath.integer((idx / 4));
        shape.Addfloatfloatfloatfloat(pos.X + Stage.AreaSize * floorPoints[(xi)], pos.Y + Stage.AreaSize * floorPoints[(yi)], -0.1f, pointsAlpha[(idx)]);
    }

    private float[][] wallParams = new float[][]
    {
        new float[]
        {
            -1.0f,
            -1.0f,
            2,
            0
        },
        new float[]
        {
            1.0f,
            -1.0f,
            0,
            2
        },
        new float[]
        {
            -1.0f,
            1.0f,
            2,
            0
        },
        new float[]
        {
            -1.0f,
            -1.0f,
            0,
            2
        }
    };
    public void SetWalls(int d)
    {
        int bn = 3 + Stage.Random.Nextint(3);
        float x = wallParams[(d)][(0)];
        float y = wallParams[(d)][(1)];
        float vx = wallParams[(d)][(2)] / bn;
        float vy = wallParams[(d)][(3)] / bn;
        float br = 1.0f / (bn + 1);
        x += vx / 2;
        y += vy / 2;
        for (int i = 0; i < bn; i++)
        {
            AppearanceWall aw = new AppearanceWall();
            aw.Radius = br * Stage.AreaSize * ((float)Stage.Random.NextDouble() * 0.5f + 0.75f);
            aw.Pos = new Vector3((x + ((float)Stage.Random.NextDouble() - 0.5f) * br) * Stage.AreaSize, (y + ((float)Stage.Random.NextDouble() - 0.5f) * br) * Stage.AreaSize, -(float)Stage.Random.NextDouble() * aw.Radius * 0.5f);
            aw.Dir = (Quaternion.CreateFromYawPitchRoll((float)(Stage.Random.NextDouble() * Math.PI), (float)(Stage.Random.NextDouble() * Math.PI), (float)(Stage.Random.NextDouble() * Math.PI))).Copy();
            appearanceWalls[(appearanceWallsNum)] = (aw).Copy();
            appearanceWallsNum++;
            x += vx;
            y += vy;
        }
    }

    public void SetWallBalls()
    {
        for (int i = 0; i < Stage.Random.Nextint(10); i++)
        {
            AppearanceWall aw = new AppearanceWall();
            aw.Radius = (float)((Stage.Random.NextDouble() + 0.5f) * 6);
            aw.Pos = new Vector3((float)((Stage.Random.NextDouble() - 0.5f) * Stage.AreaSize), (float)((Stage.Random.NextDouble() - 0.5f) * Stage.AreaSize), -(float)Stage.Random.NextDouble() * aw.Radius * 0.5f);
            aw.Dir = (Quaternion.CreateFromYawPitchRoll((float)(Stage.Random.NextDouble() * Math.PI), (float)(Stage.Random.NextDouble() * Math.PI), (float)(Stage.Random.NextDouble() * Math.PI))).Copy();
            appearanceWalls[(appearanceWallsNum)] = (aw).Copy();
            appearanceWallsNum++;
        }
    }

    public void CheckWallHit(Vector3 p, Vector3 v)
    {
        float x = (p.X - pos.X) / Stage.AreaSize;
        float y = (p.Y - pos.Y) / Stage.AreaSize;
        if ((HasWalls[(0)] && y <= -1 + WallWidth && v.Y < 0) || (HasWalls[(2)] && y >= 1 - WallWidth && v.Y > 0))
        {
            v.Y *= -1;
            p.Y += v.Y * SimulationTime.Step;
        }

        if ((HasWalls[(3)] && x <= -1 + WallWidth && v.X < 0) || (HasWalls[(1)] && x >= 1 - WallWidth && v.X > 0))
        {
            v.X *= -1;
            p.X += v.X * SimulationTime.Step;
        }
    }

    public void Add()
    {
        for (int i = 0; i < appearanceWallsNum; i++)
            appearanceWalls[(i)].Add((pos).Copy(), walls);
    }

    public void AddBalls()
    {
        if (HasFloor)
        {
            SetRank();
            SetAppearances();
        }

        for (int i = 0; i < appearancesNum; i++)
            appearances[(i)].Add((pos3).Copy(), balls, turrets, bullets, -1);
        appearancesNum = 0;
    }

    public void AddBossBall()
    {
        if (HasFloor)
        {
            SetBossRank();
            SetAppearances();
        }

        for (int i = 0; i < appearancesNum; i++)
            appearances[(i)].Add((pos3).Copy(), balls, turrets, bullets, -1);
        appearancesNum = 0;
    }

    public void Remove()
    {
        balls.ForEach((Ball a) =>
        {
            if (MathUtil.ContainsVector2Vector2float((a.Pos2).Copy(), (pos).Copy(), Stage.AreaSize))
            {
                balls.RemoveT(a);
            }
        });
        walls.ForEach((Wall a) =>
        {
            if (MathUtil.ContainsVector2Vector2float((a.Pos2).Copy(), (pos).Copy(), Stage.AreaSize))
                walls.RemoveT(a);
        });
    }

    private void SetRank()
    {
        Rank = player.Rank;
        switch (Stage.Random.Nextint(4))
        {
            case 0:
            case 1:
                Type = AreaAreaType.Small;
                break;
            case 2:
                Type = AreaAreaType.Middle1;
                break;
            case 3:
                Type = AreaAreaType.ChainedMiddle;
                break;
        }
    }

    private void SetBossRank()
    {
        Rank = player.Rank;
        Type = AreaAreaType.Big;
    }

    private void SetAppearances()
    {
        appearancesNum = 0;
        int sn = 0;
        switch (Type)
        {
            case AreaAreaType.Small:
                sn = 4 + Stage.Random.Nextint(2);
                AddSmallAppearances(Rank, sn);
                break;
            case AreaAreaType.Middle1:
                sn = 2 + Stage.Random.Nextint(2);
                AddSmallAppearances(Rank, sn);
                AddMiddleAppearances(Rank, 1);
                break;
            case AreaAreaType.Middle2:
                AddMiddleAppearances(Rank, 2);
                break;
            case AreaAreaType.ChainedMiddle:
                sn = 2 + Stage.Random.Nextint(2);
                AddSmallAppearances(Rank, sn);
                AddChainedMiddleAppearances(Rank, 1);
                break;
            case AreaAreaType.Big:
                AddBigAppearances(Rank);
                break;
        }
    }

    private void AddSmallAppearances(float rank, int n)
    {
        Firing firing = new Firing();
        Firing nextFiring = new Firing();
        Firing subFiring = new Firing();
        Firing subNextFiring = new Firing();
        BallMovement bm = new BallMovement();
        firing.Setfloatfloatintintfloatfloatfloat(rank / 3, 1.0f, 2, Stage.Random.Nextint(2) - 1, 0.75f, 1.5f, 0.3f);
        firing.IsValid = true;
        firing.IsMorphed = false;
        firing.IsFiringNear = false;
        nextFiring.IsValid = false;
        bm.Mode = BallMovementMovementMode.Approach;
        bm.Speed = 0.5f + (float)Stage.Random.NextDouble() * (float)Stage.Random.NextDouble() * 2.5f;
        bm.Range = (float)Stage.Random.NextDouble() * 36.0f;
        bm.StayDuration = Stage.Random.Nextint(180);
        BallAppearance ba = (CreateBallAppearance(1.5f, 0.25f, BallAppearanceTurretsType.Aim, 1, (firing).Copy(), (nextFiring).Copy(), (subFiring).Copy(), (subNextFiring).Copy(), (bm).Copy(), 100)).Copy();
        for (int i = 0; i < n; i++)
            SetAppearance(ba);
    }

    private void AddMiddleAppearances(float rank, int n)
    {
        BallAppearance ba = (CreateMiddleBallAppearances(rank, true, 500, 2.5f, 5.0f, 1)).Copy();
        for (int i = 0; i < n; i++)
            SetAppearance(ba);
    }

    private void AddChainedMiddleAppearances(float rank, int n)
    {
        BallAppearance ba1 = (CreateMiddleBallAppearances(rank * 1.2f, true, 500, 1.5f, 10.0f, 4)).Copy();
        BallAppearance ba2 = (CreateMiddleBallAppearances(rank * 0.7f, false, 0, 1.5f, 10.0f, 0)).Copy();
        for (int i = 0; i < n; i++)
            SetAppearanceChained(ba1, ba2);
    }

    private BallAppearance CreateMiddleBallAppearances(float rank, bool isMoving, int score, float radius, float hardness, float speedRatio)
    {
        Firing firing = new Firing();
        Firing nextFiring = new Firing();
        Firing subFiring = new Firing();
        Firing subNextFiring = new Firing();
        BallMovement bm = new BallMovement();
        if (isMoving)
            bm.Mode = BallMovementMovementMode.Approach;
        else
            bm.Mode = BallMovementMovementMode.None;
        bm.Speed = (0.1f + (float)Stage.Random.NextDouble() * 0.5f) * speedRatio;
        bm.Range = (float)Stage.Random.NextDouble() * 36.0f;
        bm.StayDuration = 180 + Stage.Random.Nextint(360);
        BallAppearanceTurretsType type = BallAppearanceTurretsType.Aim;
        int turretNum = 0;
        switch (Stage.Random.Nextint(6))
        {
            case 0:
                firing.Setfloatfloatintintfloatfloatfloat(rank, 0.5f, 0, Stage.Random.Nextint(2) - 1, 1.25f, 1.0f, 0.4f);
                firing.IsValid = true;
                firing.IsMorphed = false;
                nextFiring.Setfloatfloatintintfloatfloatfloat(rank / 2, 0.5f, 1, Stage.Random.Nextint(2) - 1, 1.25f, 1.0f, 0.4f);
                nextFiring.IsValid = true;
                nextFiring.IsMorphed = true;
                type = BallAppearanceTurretsType.Aim;
                turretNum = 1;
                break;
            case 1:
                firing.Setfloatfloatintintfloatfloatfloat(rank / 2, 0.5f, 0, Stage.Random.Nextint(2) - 1, 1.25f, 1.0f, 0.4f);
                firing.IsValid = true;
                firing.IsMorphed = false;
                nextFiring.Setfloatfloatintintfloatfloatfloat(rank / 3, 0.5f, 0, Stage.Random.Nextint(2) - 1, 1.25f, 1.0f, 0.4f);
                nextFiring.IsValid = true;
                nextFiring.IsMorphed = true;
                subFiring.Setfloatfloatintintfloatfloatfloat(rank / 4, 0.5f, 1, Stage.Random.Nextint(2) - 1, 0.9f, 1.25f, 0.2f);
                subFiring.SetIntervalRatio(0.5f);
                subFiring.IsValid = true;
                subFiring.IsMorphed = false;
                subNextFiring.Setfloatfloatintintfloatfloatfloat(rank / 4, 0.5f, 2, Stage.Random.Nextint(2) - 1, 0.9f, 1.25f, 0.2f);
                subNextFiring.IsValid = true;
                subNextFiring.IsMorphed = true;
                type = BallAppearanceTurretsType.Aim;
                turretNum = 3;
                break;
            case 2:
                firing.Setfloatfloatintintfloatfloatfloat(rank / 2, 0.5f, 0, Stage.Random.Nextint(2) - 1, 1.25f, 1.0f, 0.4f);
                firing.IsValid = true;
                firing.IsMorphed = false;
                nextFiring.Setfloatfloatintintfloatfloatfloat(rank / 3, 0.5f, 1, Stage.Random.Nextint(2) - 1, 1.25f, 1.0f, 0.4f);
                nextFiring.IsValid = true;
                nextFiring.IsMorphed = true;
                subFiring.Setfloatfloatintintfloatfloatfloat(rank / 2, 0.5f, 0, Stage.Random.Nextint(2) - 1, 1.0f, 1.2f, 0.3f);
                subFiring.SetIntervalRatio(0.5f);
                subFiring.IsValid = true;
                subFiring.IsMorphed = false;
                subNextFiring.Setfloatfloatintintfloatfloatfloat(rank / 3, 0.5f, 1, Stage.Random.Nextint(2) - 1, 1.0f, 1.2f, 0.3f);
                subNextFiring.IsValid = true;
                subNextFiring.IsMorphed = true;
                type = BallAppearanceTurretsType.Aim;
                turretNum = 2;
                break;
            case 3:
                turretNum = 3 + Stage.Random.Nextint(3);
                firing.Setfloatfloatintintfloatfloatfloat(rank / turretNum, 0.5f, 3, Stage.Random.Nextint(2) - 1, 1.0f, 0.5f, 0.5f);
                firing.IsValid = true;
                firing.IsMorphed = false;
                nextFiring.Setfloatfloatintintfloatfloatfloat(rank / turretNum, 0.5f, 1, Stage.Random.Nextint(2) - 1, 1.0f, 0.5f, 0.5f);
                nextFiring.IsValid = true;
                nextFiring.IsMorphed = true;
                type = BallAppearanceTurretsType.Roll;
                break;
            case 4:
                turretNum = 2 + Stage.Random.Nextint(2);
                firing.Setfloatfloatintintfloatfloatfloat(rank / turretNum / 2, 0.5f, 3, Stage.Random.Nextint(2) - 1, 1.0f, 0.5f, 0.5f);
                firing.IsValid = true;
                firing.IsMorphed = false;
                nextFiring.Setfloatfloatintintfloatfloatfloat(rank / turretNum / 2, 0.5f, 1, Stage.Random.Nextint(2) - 1, 1.0f, 0.5f, 0.5f);
                nextFiring.IsValid = true;
                nextFiring.IsMorphed = true;
                subFiring = (firing).Copy();
                subFiring.IntervalCnt++;
                subNextFiring = (nextFiring).Copy();
                subNextFiring.IntervalCnt++;
                type = BallAppearanceTurretsType.RollCross;
                break;
            case 5:
                turretNum = 2 + Stage.Random.Nextint(2);
                firing.Setfloatfloatintintfloatfloatfloat(rank / turretNum / 2, 0.5f, 3, Stage.Random.Nextint(2) - 1, 1.0f, 0.5f, 0.5f);
                firing.IsValid = true;
                firing.IsMorphed = false;
                nextFiring.Setfloatfloatintintfloatfloatfloat(rank / turretNum / 2, 0.5f, 1, Stage.Random.Nextint(2) - 1, 1.0f, 0.5f, 0.5f);
                nextFiring.IsValid = true;
                nextFiring.IsMorphed = true;
                subFiring = (firing).Copy();
                subNextFiring = (nextFiring).Copy();
                type = BallAppearanceTurretsType.RollTwin;
                break;
        }

        return (CreateBallAppearance(radius, hardness, type, turretNum, (firing).Copy(), (nextFiring).Copy(), (subFiring).Copy(), (subNextFiring).Copy(), (bm).Copy(), score)).Copy();
    }

    private void AddBigAppearances(float rank)
    {
        Firing firing = new Firing();
        Firing nextFiring = new Firing();
        Firing subFiring = new Firing();
        Firing subNextFiring = new Firing();
        BallMovement bm = new BallMovement();
        bm.Mode = BallMovementMovementMode.Approach;
        bm.Speed = 0.05f + (float)Stage.Random.NextDouble() * 0.2f;
        bm.Range = (float)Stage.Random.NextDouble() * 36.0f;
        bm.StayDuration = 180 + Stage.Random.Nextint(360);
        bm.StayCnt = 0;
        BallAppearanceTurretsType type = BallAppearanceTurretsType.Aim;
        int turretNum = 0;
        BallAppearance ba = new BallAppearance();
        BallAppearance sba = new BallAppearance();
        int ballFireingType = Stage.Random.Nextint(3);
        int ballNum = 1;
        float ir = (float)Stage.Random.NextDouble();
        switch (ballFireingType)
        {
            case 0:
                firing.Setintfloat(GameMath.integer((120.0f / (1.0f + ir))), 10.0f);
                firing.IsValid = true;
                firing.IsMorphed = false;
                nextFiring.IsValid = false;
                type = BallAppearanceTurretsType.BallTwin;
            {
                turretNum = 2;
                ballNum = turretNum;
            }

                break;
            case 1:
                firing.Setintfloat(GameMath.integer((120.0f / (1.0f + ir))), 5.0f);
                firing.IsValid = true;
                firing.IsMorphed = false;
                nextFiring.IsValid = false;
                type = BallAppearanceTurretsType.BallRoll;
            {
                turretNum = 3;
                ballNum = turretNum;
            }

                break;
            case 2:
                firing.Setintfloat(GameMath.integer((120.0f / (1.0f + ir))), 100.0f);
                firing.IsValid = true;
                firing.IsMorphed = false;
                nextFiring.IsValid = false;
                type = BallAppearanceTurretsType.BallAim;
            {
                turretNum = 1;
                ballNum = turretNum;
            }

                break;
        }

        subFiring.Setfloatfloatintintfloatfloatfloat(rank / 3, 0.5f, 0, Stage.Random.Nextint(2) - 1, 1.25f, 1.2f, 0.3f);
        subFiring.IsValid = true;
        subFiring.IsMorphed = false;
        subNextFiring.Setfloatfloatintintfloatfloatfloat(rank / 3, 0.5f, 1, Stage.Random.Nextint(2) - 1, 1.25f, 1.2f, 0.3f);
        subNextFiring.IsValid = true;
        subNextFiring.IsMorphed = true;
        ba = (CreateBallAppearance(8.0f, 50.0f, type, turretNum, (firing).Copy(), (nextFiring).Copy(), (subFiring).Copy(), (subNextFiring).Copy(), (bm).Copy(), 1000)).Copy();
        switch (Stage.Random.Nextint(2))
        {
            case 0:
                turretNum = 2 + Stage.Random.Nextint(2);
                firing.Setfloatfloatintintfloatfloatfloat(rank / turretNum / 2 / ballNum / (1 + ir), 0.5f, 3, Stage.Random.Nextint(2) - 1, 1.0f, 0.7f, 0.4f);
                firing.IsValid = true;
                firing.IsMorphed = false;
                nextFiring.Setfloatfloatintintfloatfloatfloat(rank / turretNum / 2 / ballNum / (1 + ir), 0.5f, 2, Stage.Random.Nextint(2) - 1, 1.0f, 0.7f, 0.4f);
                nextFiring.IsValid = true;
                nextFiring.IsMorphed = true;
                type = BallAppearanceTurretsType.Roll;
                break;
            case 1:
                turretNum = 1;
                firing.Setfloatfloatintintfloatfloatfloat(rank / turretNum / ballNum / (1 + ir), 0.5f, 4, Stage.Random.Nextint(2) - 1, 1.0f, 1.0f, 0.4f);
                firing.IsValid = true;
                firing.IsMorphed = false;
                nextFiring.Setfloatfloatintintfloatfloatfloat(rank / turretNum / ballNum / (1 + ir), 0.5f, 2, Stage.Random.Nextint(2) - 1, 1.0f, 1.0f, 0.4f);
                nextFiring.IsValid = true;
                nextFiring.IsMorphed = true;
                type = BallAppearanceTurretsType.Aim;
                break;
        }

        bm.Mode = BallMovementMovementMode.StayBullet;
        bm.Speed = 1.0f;
        switch (ballFireingType)
        {
            case 0:
                bm.Range = 24.0f + (float)Stage.Random.NextDouble() * 8.0f;
                break;
            case 1:
                bm.Range = 8.0f + (float)Stage.Random.NextDouble() * 3.0f;
                break;
            case 2:
                bm.Range = -24.0f - (float)Stage.Random.NextDouble() * 8.0f;
                break;
        }

        bm.StayDuration = 99999;
        sba = (CreateBallAppearance(2.0f, 0.01f, type, turretNum, (firing).Copy(), (nextFiring).Copy(), (subFiring).Copy(), (subNextFiring).Copy(), (bm).Copy(), 0)).Copy();
        SetAppearanceBoss(ba, sba);
    }

    private void SetAppearance(BallAppearance ba)
    {
        Appearance a = new Appearance();
        a.Pos = (GetAppearancePos()).Copy();
        a.BallAppearance = (ba).Copy();
        a.SubBallAppearance = (noBallAppearance).Copy();
        a.Type = AppearanceAppearanceType.Normal;
        appearances[(appearancesNum)] = (a).Copy();
        appearancesNum++;
    }

    private void SetAppearanceBoss(BallAppearance ba, BallAppearance sba)
    {
        Appearance a = new Appearance();
        a.Pos = new Vector3(0, 0, 24.0f);
        a.BallAppearance = (ba).Copy();
        a.SubBallAppearance = (sba).Copy();
        a.Type = AppearanceAppearanceType.Normal;
        appearances[(appearancesNum)] = (a).Copy();
        appearancesNum++;
    }

    private void SetAppearanceChained(BallAppearance ba, BallAppearance cba)
    {
        Appearance a = new Appearance();
        a.Pos = (GetAppearancePos()).Copy();
        a.BallAppearance = (ba).Copy();
        a.SubBallAppearance = (noBallAppearance).Copy();
        a.ChainedBallAppearance = (cba).Copy();
        a.Type = AppearanceAppearanceType.Chained;
        appearances[(appearancesNum)] = (a).Copy();
        appearancesNum++;
    }

    private Vector3 GetAppearancePos()
    {
        return new Vector3((-0.8f + (float)Stage.Random.NextDouble() * 1.6f) * Stage.AreaSize, (-0.8f + (float)Stage.Random.NextDouble() * 1.6f) * Stage.AreaSize, 24.0f + (float)Stage.Random.NextDouble() * 16.0f);
    }

    private BallAppearance CreateBallAppearance(float r, float h, BallAppearanceTurretsType type, int turretNum, Firing firing, Firing nextFiring, Firing subFiring, Firing subNextFiring, BallMovement movement, int score)
    {
        BallAppearance ba = new BallAppearance();
        ba.Radius = r;
        ba.Hardness = h;
        ba.Type = type;
        ba.TurretNum = turretNum;
        ba.Firing = (firing).Copy();
        ba.NextFiring = (nextFiring).Copy();
        ba.SubFiring = (subFiring).Copy();
        ba.SubNextFiring = (subNextFiring).Copy();
        ba.Movement = (movement).Copy();
        ba.Score = score;
        return (ba).Copy();
    }
}

public class Appearance
{
    public AppearanceAppearanceType Type;
    public Vector3 Pos = new Vector3();
    public BallAppearance BallAppearance = new BallAppearance();
    public BallAppearance SubBallAppearance = new BallAppearance();
    public BallAppearance ChainedBallAppearance = new BallAppearance();
    public void Clear()
    {
        Type = AppearanceAppearanceType.Normal;
        {
            Pos.Z = 0;
            Pos.Y = Pos.Z;
            Pos.X = Pos.Y;
        }

        BallAppearance.Clear();
        SubBallAppearance.Clear();
        ChainedBallAppearance.Clear();
    }

    public void Add(Vector3 p, BallPool balls, TurretPool turrets, BulletPool bullets, int bid)
    {
        switch (Type)
        {
            case AppearanceAppearanceType.Normal:
            {
                if (!balls.IsAbleToAdd)
                    return;
                Ball b = (BallAppearance.CreateBall((p + Pos).Copy(), balls, turrets, bullets, bid, SubBallAppearance)).Copy();
                b.Appearance = (this).Copy();
                balls.Add(b);
            }

                break;
            case AppearanceAppearanceType.Chained:
            {
                if (balls.RemainingActorNum < 3)
                    return;
                Ball b1 = (BallAppearance.CreateBall((p + Pos).Copy(), balls, turrets, bullets, bid, SubBallAppearance)).Copy();
                b1.Appearance = (this).Copy();
                p.X += b1.State.Radius * 2;
                Ball b2 = (ChainedBallAppearance.CreateBall((p + Pos).Copy(), balls, turrets, bullets, bid, SubBallAppearance)).Copy();
                p.X += b1.State.Radius * 2;
                Ball b3 = (ChainedBallAppearance.CreateBall((p + Pos).Copy(), balls, turrets, bullets, bid, SubBallAppearance)).Copy();
                {
                    b3.TurretActivateInterval = 60;
                    b2.TurretActivateInterval = b3.TurretActivateInterval;
                    b1.TurretActivateInterval = b2.TurretActivateInterval;
                }

                b1.TurretActivateCnt = -1;
                b2.TurretActivateCnt = -1;
                b3.TurretActivateCnt = 0;
                int id1 = balls.Add(b1);
                int id2 = balls.Add(b2);
                int id3 = balls.Add(b3);
                balls.SetNextBallId(id1, id2);
                balls.SetNextBallId(id2, id3);
                balls.SetRootBallId(id2, id1);
                balls.SetRootBallId(id3, id1);
            }

                break;
        }
    }

    public Appearance Copy()
    {
        return new Appearance
        {
            Type = Type,
            Pos = Pos.Copy(),
            BallAppearance = BallAppearance.Copy(),
            SubBallAppearance = SubBallAppearance.Copy(),
            ChainedBallAppearance = ChainedBallAppearance.Copy()
        };
    }
}

public class BallAppearance
{
    public float Radius;
    public float Hardness;
    public BallAppearanceTurretsType Type;
    public int TurretNum;
    public Firing Firing = new Firing();
    public Firing NextFiring = new Firing();
    public Firing SubFiring = new Firing();
    public Firing SubNextFiring = new Firing();
    public BallMovement Movement = new BallMovement();
    public int Score;
    public void Clear()
    {
        Radius = 1.0f;
        Hardness = 1.0f;
        Type = BallAppearanceTurretsType.Aim;
        TurretNum = 0;
        Firing.Clear();
        NextFiring.Clear();
        SubFiring.Clear();
        SubNextFiring.Clear();
        Movement.Clear();
        Score = 0;
    }

    public Ball CreateBall(Vector3 p, BallPool balls, TurretPool turrets, BulletPool bullets, int bid, BallAppearance subBallAppearance)
    {
        Turret t = new Turret();
        t.NextBallId = -1;
        int tid = -1;
        float d = 0;
        float od = (float)Math.PI * 2 / TurretNum;
        float dv = ((float)Stage.Random.NextDouble() + 0.5f) * 0.01f;
        if (Stage.Random.Nextint(2) == 1)
            dv *= -1;
        float fd = (float)(Stage.Random.NextDouble() * Math.PI * 2);
        int bnid = balls.NextId;
        if (bid >= 0)
            bnid = bid;
        switch (Type)
        {
            case BallAppearanceTurretsType.Aim:
                for (int i = 0; i < TurretNum; i++)
                {
                    if (!turrets.IsAbleToAdd)
                        break;
                    if (i == 0)
                        t.BulletId = AddBullet(bullets, bnid, subBallAppearance);
                    else
                        t.BulletId = AddBulletSub(bullets, bnid, subBallAppearance);
                    {
                        t.DegOfs = d;
                        t.Deg = t.DegOfs;
                    }

                    t.FiringDeg = 0;
                    t.NextId = tid;
                    t.Type = TurretTurretType.Aim;
                    tid = turrets.Add(t);
                    d += od;
                }

                break;
            case BallAppearanceTurretsType.BallAim:
                for (int i = 0; i < TurretNum; i++)
                {
                    if (!turrets.IsAbleToAdd)
                        break;
                    t.BulletId = AddBullet(bullets, bnid, subBallAppearance);
                    {
                        t.DegOfs = d;
                        t.Deg = t.DegOfs;
                    }

                    t.FiringDeg = 0;
                    t.NextId = tid;
                    t.Type = TurretTurretType.Aim;
                    tid = turrets.Add(t);
                    d += od;
                }

                break;
            case BallAppearanceTurretsType.Roll:
            case BallAppearanceTurretsType.BallRoll:
                for (int i = 0; i < TurretNum; i++)
                {
                    if (!turrets.IsAbleToAdd)
                        break;
                    t.BulletId = AddBullet(bullets, bnid, subBallAppearance);
                    t.Deg = d;
                    t.DegVel = dv;
                    if (Type == BallAppearanceTurretsType.BallRoll)
                        t.FiringDeg = 0;
                    else
                        t.FiringDeg = fd;
                    t.NextId = tid;
                    t.Type = TurretTurretType.Roll;
                    tid = turrets.Add(t);
                    d += od;
                }

                break;
            case BallAppearanceTurretsType.BallTwin:
                od /= (1.5f + (float)Stage.Random.NextDouble());
                d = -od / 2;
                for (int i = 0; i < TurretNum; i++)
                {
                    if (!turrets.IsAbleToAdd)
                        break;
                    Firing.SetIntervalRatio((i % 2) * 0.5f);
                    t.BulletId = AddBullet(bullets, bnid, subBallAppearance);
                    {
                        t.DegOfs = d;
                        t.Deg = t.DegOfs;
                    }

                    t.FiringDeg = 0;
                    t.NextId = tid;
                    t.Type = TurretTurretType.AimRoll;
                    tid = turrets.Add(t);
                    d += od;
                }

                break;
            case BallAppearanceTurretsType.RollCross:
                for (int j = 0; j < 2; j++)
                {
                    d = 0;
                    for (int i = 0; i < TurretNum; i++)
                    {
                        if (!turrets.IsAbleToAdd)
                            break;
                        if (j == 0)
                            t.BulletId = AddBullet(bullets, bnid, subBallAppearance);
                        else
                            t.BulletId = AddBulletSub(bullets, bnid, subBallAppearance);
                        t.Deg = d;
                        t.DegVel = dv;
                        t.FiringDeg = fd;
                        t.NextId = tid;
                        t.Type = TurretTurretType.Roll;
                        tid = turrets.Add(t);
                        d += od;
                    }

                    if (!turrets.IsAbleToAdd)
                        break;
                    od *= -1;
                    fd *= -1;
                    dv *= -1;
                }

                break;
            case BallAppearanceTurretsType.RollTwin:
                for (int j = 0; j < 2; j++)
                {
                    d = 0;
                    for (int i = 0; i < TurretNum; i++)
                    {
                        if (!turrets.IsAbleToAdd)
                            break;
                        if (j == 0)
                            t.BulletId = AddBullet(bullets, bnid, subBallAppearance);
                        else
                            t.BulletId = AddBulletSub(bullets, bnid, subBallAppearance);
                        t.Deg = d;
                        t.DegVel = dv;
                        t.FiringDeg = fd;
                        t.NextId = tid;
                        t.Type = TurretTurretType.Roll;
                        tid = turrets.Add(t);
                        d += od;
                    }

                    if (!turrets.IsAbleToAdd)
                        break;
                    dv *= 2;
                }

                break;
        }

        if (turrets.IsAbleToAdd && (Type == BallAppearanceTurretsType.BallAim || Type == BallAppearanceTurretsType.BallRoll || Type == BallAppearanceTurretsType.BallTwin))
        {
            t.BulletId = AddBulletSub(bullets, bnid, subBallAppearance);
            {
                t.DegOfs = 0;
                t.Deg = t.DegOfs;
            }

            t.FiringDeg = 0;
            t.NextId = tid;
            t.Type = TurretTurretType.Aim;
            tid = turrets.Add(t);
        }

        Ball b = new Ball();
        b.Set((p).Copy(), Radius, Radius * 2.0f, Hardness, bid);
        b.TurretId = tid;
        b.Movement = (Movement).Copy();
        b.Score = Score;
        return (b).Copy();
    }

    private int AddBullet(BulletPool bullets, int bid, BallAppearance subBallAppearance)
    {
        Bullet b = new Bullet();
        b.Set((Vector3.Zero).Copy(), 0, 0, 0, bid, 0, 0, 0);
        b.Firing = (Firing).Copy();
        b.NextFiring = (NextFiring).Copy();
        b.SubBallAppearance = (subBallAppearance).Copy();
        b.IsTop = true;
        return bullets.Add(b);
    }

    private int AddBulletSub(BulletPool bullets, int bid, BallAppearance subBallAppearance)
    {
        Bullet b = new Bullet();
        b.Set((Vector3.Zero).Copy(), 0, 0, 0, bid, 0, 0, 0);
        b.Firing = (SubFiring).Copy();
        b.NextFiring = (SubNextFiring).Copy();
        SubFiring.IntervalCnt++;
        SubNextFiring.IntervalCnt++;
        b.SubBallAppearance = (subBallAppearance).Copy();
        b.IsTop = true;
        return bullets.Add(b);
    }

    public BallAppearance Copy()
    {
        return new BallAppearance
        {
            Radius = Radius,
            Hardness = Hardness,
            Type = Type,
            TurretNum = TurretNum,
            Firing = Firing.Copy(),
            NextFiring = NextFiring.Copy(),
            SubFiring = SubFiring.Copy(),
            SubNextFiring = SubNextFiring.Copy(),
            Movement = Movement.Copy(),
            Score = Score
        };
    }
}

public class AppearanceWall
{
    public Vector3 Pos = new Vector3();
    public float Radius;
    public Quaternion Dir = new Quaternion();
    public void Clear()
    {
        {
            Pos.Z = 0;
            Pos.Y = Pos.Z;
            Pos.X = Pos.Y;
        }

        Radius = 1.0f;
        Dir = (Quaternion.Identity).Copy();
    }

    public void Add(Vector2 p, WallPool walls)
    {
        Wall w = new Wall();
        w.Set(new Vector3(p.X + Pos.X, p.Y + Pos.Y, Pos.Z), Radius, (Dir).Copy());
        walls.Add(w);
    }

    public AppearanceWall Copy()
    {
        return new AppearanceWall
        {
            Pos = Pos.Copy(),
            Radius = Radius,
            Dir = Dir.Copy()
        };
    }
}

public enum AreaAreaType
{
    Small,
    Middle1,
    Middle2,
    ChainedMiddle,
    Big,
}

public enum AppearanceAppearanceType
{
    Normal,
    Chained,
}

public enum BallAppearanceTurretsType
{
    Aim,
    Roll,
    RollCross,
    RollTwin,
    BallTwin,
    BallRoll,
    BallAim,
}
