// Copyright 2009 Kenta Cho. Some rights reserved.
using System;
using System.Collections.Generic;

public class Stage
{
    private Action<EnemyType, EnemyMotionType>[] initializeEnemyFuncs;
    private const float playerDepthSpeedBase = 5;
    private static float[] basePillarInterval = new float[]
    {
        50,
        40,
        15,
        50,
        50,
        150,
        150,
        100,
        7
    };
    private const int stageTicksBase = 300;
    public static Random Random;
    public static float PlayerDepthSpeed;
    public static float GameSpeed, GameSpeedSqrt;
    public static float BackgroundR, BackgroundG, BackgroundB;
    public static float Rank;
    private EnemyPool enemies;
    private MiddleEnemyPool middleEnemies;
    private PillarPool pillars;
    private Player player;
    private GameState gameState;
    private int ticks;
    private float enemyTicks, enemyTicksBase, enemyFormCount, enemyFormCountBase, enemyFormTicks, enemyFormTicksBase, middleEnemyTicks, middleEnemyTicksBase;
    private float pillarTicks, pillarTicksRate;
    private EnemyType enemyType = new EnemyType();
    private EnemyMotionType enemyMotionType = new EnemyMotionType();
    private MiddleEnemyWeaponType middleEnemyWeaponType;
    private int middleEnemyFireInterval;
    private StagePillarType pillarType;
    private int stageTicks;
    private float targetBackgroundR, targetBackgroundG, targetBackgroundB;
    private float targetRank;
    private int stageCount;
    private bool isBossStage;
    public Stage(EnemyPool enemies, MiddleEnemyPool middleEnemies, PillarPool pillars, Player player, GameState gameState)
    {
        this.enemies = enemies;
        this.middleEnemies = middleEnemies;
        this.pillars = pillars;
        this.player = player;
        this.gameState = gameState;
        initializeEnemyFuncs = GtgArrays.Make<Action<EnemyType, EnemyMotionType>>(4, () => null);
        initializeEnemyFuncs[(0)] = (EnemyType et, EnemyMotionType mt) =>
        {
            InitializeForEnemy1(et, mt);
        };
        initializeEnemyFuncs[(1)] = (EnemyType et, EnemyMotionType mt) =>
        {
            InitializeForEnemy2(et, mt);
        };
        initializeEnemyFuncs[(2)] = (EnemyType et, EnemyMotionType mt) =>
        {
            InitializeForEnemy3(et, mt);
        };
        initializeEnemyFuncs[(3)] = (EnemyType et, EnemyMotionType mt) =>
        {
            InitializeForEnemy4(et, mt);
        };
    }

    public void Start(int randomSeed)
    {
        Random = new Random(randomSeed);
        PlayerDepthSpeed = playerDepthSpeedBase;
        {
            GameSpeedSqrt = 1;
            GameSpeed = GameSpeedSqrt;
        }

        ticks = 0;
        stageTicks = 0;
        {
            BackgroundB = 0;
            BackgroundG = BackgroundB;
            BackgroundR = BackgroundG;
        }

        {
            targetBackgroundB = 0;
            targetBackgroundG = targetBackgroundB;
            targetBackgroundR = targetBackgroundG;
        }

        Rank = 0.5f;
        targetRank = 1;
        stageCount = 0;
        isBossStage = false;
        {
            middleEnemyTicksBase = 0;
            middleEnemyTicks = middleEnemyTicksBase;
            enemyFormTicksBase = middleEnemyTicks;
            enemyFormTicks = enemyFormTicksBase;
            enemyFormCountBase = enemyFormTicks;
            enemyFormCount = enemyFormCountBase;
            enemyTicksBase = enemyFormCount;
            enemyTicks = enemyTicksBase;
        }

        {
            pillarTicksRate = 0;
            pillarTicks = pillarTicksRate;
        }

        enemyType = new EnemyType();
        enemyMotionType = new EnemyMotionType();
        middleEnemyWeaponType = MiddleEnemyWeaponType.Laser;
        middleEnemyFireInterval = 0;
        pillarType = StagePillarType.Bar;
    }

    public void Update()
    {
        stageTicks--;
        if (isBossStage)
        {
            if (stageTicks >= 420)
            {
                if (stageTicks == 420)
                    AddBosses();
            }
            else if (middleEnemies.Count <= 0 || stageTicks <= 120)
            {
                middleEnemies.FinishBossStage();
                stageTicks = 120;
                Rank /= 2;
                targetRank += 0.7f;
                isBossStage = false;
                if ((stageCount / 7) % 2 == 0)
                    gameState.FadeoutBgm();
            }
        }

        if (stageTicks <= 0)
            GoToNextStage();
        if (stageTicks == 30)
        {
            targetBackgroundR = CreateBackgroundColor();
            targetBackgroundG = CreateBackgroundColor();
            targetBackgroundB = CreateBackgroundColor();
        }

        BackgroundR += (targetBackgroundR - BackgroundR) * 0.1f;
        BackgroundG += (targetBackgroundG - BackgroundG) * 0.1f;
        BackgroundB += (targetBackgroundB - BackgroundB) * 0.1f;
        enemyTicks -= GameSpeed;
        if (enemyTicks <= 0 || (enemyTicks < 9999 && enemies.Count <= 0))
        {
            enemyMotionType.AppearingAngle = (float)(Stage.Random.NextDouble() * Math.PI * 2);
            enemyFormCount = enemyFormCountBase;
            enemyTicks = enemyTicksBase * ((float)Random.NextDouble() + 3);
            enemyFormTicks = 0;
        }

        if (enemyFormCount > 0)
        {
            enemyFormTicks -= GameSpeed;
            if (enemyFormTicks <= 0)
            {
                enemies.Add((enemyType).Copy(), (enemyMotionType).Copy());
                enemyFormCount--;
                enemyFormTicks = enemyFormTicksBase;
            }
        }

        middleEnemyTicks -= GameSpeed;
        if (middleEnemyTicks <= 0)
        {
            float ea = (float)(Random.NextDouble() * Math.PI * 2);
            float ar = 0;
            if (Random.NextDouble() < 0.3f)
            {
                ar = (0.005f + (float)Random.NextDouble() * 0.005f) * (Random.Nextint(2) * 2 - 1);
            }

            middleEnemies.Add(Tube.Radius * 0.8f, ea, ar, middleEnemyWeaponType, middleEnemyFireInterval);
            middleEnemyTicks = middleEnemyTicksBase * ((float)Random.NextDouble() + 1);
        }

        pillarTicks -= GameSpeed;
        if (pillarTicks <= 0)
        {
            AddPillars();
            pillarTicks = basePillarInterval[((int)pillarType)] * ((float)Random.NextDouble() * 3 + 1) * pillarTicksRate;
        }

        ticks++;
        PlayerDepthSpeed = playerDepthSpeedBase * GameSpeed;
        gameState.Multiplier = GameMath.integer((float)(((GameSpeed - 1) * 32))) + 1;
        gameState.AddScoreint(1);
    }

    public void Draw()
    {
        if (stageCount % 7 == 1 && stageTicks > stageTicksBase - 60)
            Letter.AddstringVector3floatQuaternionfloat("STAGE " + (stageCount / 7 + 1), new Vector3(-10, 0, 0), 1 + stageTicks * 0.002f, (Quaternion.Identity).Copy(), 0.7f);
    }

    private void GoToNextStage()
    {
        stageCount++;
        if (stageCount % 7 == 1)
        {
            player.IncrementAccel();
            if ((stageCount / 7) % 2 == 0)
                gameState.StartBgm();
        }

        if (stageCount % 7 == 0)
        {
            isBossStage = true;
            {
                pillarTicks = 999999;
                middleEnemyTicks = pillarTicks;
                enemyTicks = middleEnemyTicks;
            }

            stageTicks = 300 + 120 + GameMath.integer((float)((320 / GameSpeed)));
            return;
        }

        isBossStage = false;
        if (Random.Nextint(4) == 0)
            pillarTicks = 999999;
        else
            pillarType = (StagePillarType)Random.Nextint(basePillarInterval.Length);
        if (Random.Nextint(3) == 0)
        {
            middleEnemyTicks = Random.Nextint(60);
            middleEnemyTicksBase = 300;
            middleEnemyFireInterval = GameMath.integer((float)((60.0f / Rank)));
            middleEnemyWeaponType = (MiddleEnemyWeaponType)(Random.Nextint(2));
        }
        else
        {
            middleEnemyTicks = 999999;
        }

        if ((pillarTicks >= 9999 && middleEnemyTicks >= 9999) || Random.Nextint(3) > 0)
        {
            enemyType.FireInterval = (75 + Random.Nextint(50)) / Rank;
            enemyType.FireSpeed = (3 + (float)Random.NextDouble() * 4) * Rank;
            initializeEnemyFuncs[(Random.Nextint(initializeEnemyFuncs.Length))](enemyType, enemyMotionType);
            if (Random.Nextint(3) == 0)
                enemyType.IsFiringStraight = false;
            if (Random.Nextint(4) == 0)
            {
                enemyMotionType.IsAppearingFromFront = true;
                if (enemyMotionType.TargetZ > 0)
                    enemyMotionType.TargetZ = Field.BackDepth * 2;
            }

            enemyTicks = enemyTicksBase;
        }
        else
        {
            enemyTicks = 999999;
        }

        if (middleEnemyTicks >= 9999 && enemyTicks >= 9999)
            pillarTicksRate = 0.5f;
        else if (middleEnemyTicks < 9999 && enemyTicks < 9999)
            pillarTicksRate = 2;
        else
            pillarTicksRate = 1;
        pillarTicksRate /= Rank;
        pillarTicks = basePillarInterval[((int)pillarType)] * ((float)Random.NextDouble() * 3 + 1) * pillarTicksRate;
        Rank += (targetRank - Rank) * 0.1f;
        stageTicks = stageTicksBase;
    }

    private void AddBosses()
    {
        int bc = 3 + Random.Nextint(3);
        float a = (float)Random.NextDouble() * (float)Math.PI * 2;
        float ar = (0.005f + (float)Random.NextDouble() * 0.005f) * (Random.Nextint(2) * 2 - 1);
        MiddleEnemyWeaponType wt = (MiddleEnemyWeaponType)(Random.Nextint(2));
        float fi = 30.0f / Rank;
        float tz = Field.FireBoundaryDepth * (2.5f + (float)Random.NextDouble());
        float ftr = 1;
        for (int i = 0; i < bc; i++)
        {
            middleEnemies.AddBoss(Tube.Radius * 0.75f, a, ar, wt, fi, tz, ftr);
            a += (float)Math.PI * 2 / bc;
            ftr += 1;
        }
    }

    private void AddPillars()
    {
        int pc;
        float r, a, x, y;
        float yr;
        int t, td;
        switch (pillarType)
        {
            case StagePillarType.Bar:
            case StagePillarType.Single:
            case StagePillarType.ShortBar:
                float pa = (float)(Random.NextDouble() * Math.PI * 2);
                float ar = 0;
                if (pillarType != StagePillarType.ShortBar && Random.NextDouble() < 0.3f)
                {
                    ar = (0.01f + (float)Random.NextDouble() * 0.01f) * (Random.Nextint(2) * 2 - 1);
                }

                float rr;
                if (pillarType == StagePillarType.Single)
                {
                    pc = 1;
                    rr = Tube.Radius * ((float)Random.NextDouble() * 0.5f + 0.4f);
                    r = Tube.Radius - rr / 2;
                }
                else if (pillarType == StagePillarType.ShortBar)
                {
                    pc = 8 + Random.Nextint(3) * 2;
                    rr = Tube.Radius * 2 / pc;
                    r = rr * ((float)pc / 2 - 0.5f);
                    pc = 2 + Random.Nextint(3);
                }
                else
                {
                    pc = 6 + Random.Nextint(4) * 2;
                    rr = Tube.Radius * 2 / pc;
                    r = rr * ((float)pc / 2 - 0.5f);
                }

                for (int i = 0; i < pc; i++)
                {
                    pillars.Addfloatfloatfloatfloatfloat(r, pa, rr / 2, ar, -((i % 2) * 2 - 1) * pc);
                    r -= rr;
                }

                break;
            case StagePillarType.Horizontal:
            case StagePillarType.Vertical:
                y = (float)Random.NextDouble() * Tube.Radius * 0.9f * (Random.Nextint(2) * 2 - 1);
                r = 10 + (float)Random.NextDouble() * 10;
                SetLinePillars(y, r, (pillarType == StagePillarType.Horizontal), -1);
                break;
            case StagePillarType.HorizontalWall:
            case StagePillarType.VerticalWall:
                yr = Random.Nextint(2) * 2 - 1;
                r = 10 + (float)Random.NextDouble() * 10;
                t = 0;
                td = 30 + Random.Nextint(20);
                y = Tube.Radius * 0.9f;
                for (; y >= -(float)Random.NextDouble() * Tube.Radius * 0.3f; y -= r * 2)
                {
                    SetLinePillars(y * yr, r, (pillarType == StagePillarType.HorizontalWall), t);
                    t += td;
                }

                break;
            case StagePillarType.Circle:
                r = 6 + (float)Random.NextDouble() * 6;
                float cir = 30 + (float)Random.NextDouble() * 50;
                float cer = (float)Random.NextDouble() * (Tube.Radius * 0.9f - cir);
                a = (float)(Random.NextDouble() * Math.PI * 2);
                x = (float)Math.Sin(a) * cer;
                y = (float)Math.Cos(a) * cer;
                pc = GameMath.integer((float)((cir * Math.PI / r)));
                a = (float)(Random.NextDouble() * Math.PI * 2);
                for (int i = 0; i < pc; i++)
                {
                    pillars.Addfloatfloatfloatint(x + (float)Math.Sin(a) * cir, y + (float)Math.Cos(a) * cir, r, -1);
                    a += (float)Math.PI * 2 / pc;
                }

                break;
            case StagePillarType.SmallSingle:
                r = 5 + (float)Random.NextDouble() * 10;
                a = (float)(Random.NextDouble() * Math.PI * 2);
                pillars.Addfloatfloatfloatfloatfloat((float)Random.NextDouble() * (Tube.Radius - r), a, r, 0, 0);
                break;
        }
    }

    private void SetLinePillars(float y, float r, bool isHorizonal, int apparanceTicks)
    {
        float l = (float)Math.Sqrt(Tube.Radius * Tube.Radius - y * y);
        int c = GameMath.integer((float)((l / r))) + 2;
        float x = -r * (c - 1);
        for (int i = 0; i < c; i++)
        {
            if (isHorizonal)
                pillars.Addfloatfloatfloatint(x, y, r, apparanceTicks);
            else
                pillars.Addfloatfloatfloatint(y, x, r, apparanceTicks);
            x += r * 2;
        }
    }

    private float CreateBackgroundColor()
    {
        if (Random.Nextint(3) == 0)
            return 0.3f + (float)Random.NextDouble() * 0.1f;
        else
            return (float)Random.NextDouble() * 0.2f;
    }

    private void InitializeForEnemy1(EnemyType et, EnemyMotionType mt)
    {
        float asr = (float)(Random.NextDouble() * 0.5f + 0.5f) * (Random.Nextint(2) * 2 - 1);
        mt.AngleSpeed.Center = 0.05f * asr;
        mt.AngleSpeed.Amplitude = 0.025f * asr;
        mt.AngleSpeed.CycleSpeed = 0.1f;
        mt.Radius.Center = 0.6f * Tube.Radius;
        mt.Radius.Amplitude = 0.3f * Tube.Radius;
        mt.Radius.CycleSpeed = 0.1f;
        mt.AppearingAngle = (float)(Random.NextDouble() * Math.PI * 2);
        mt.TargetVelRatio = 0.2f;
        mt.IsAppearingFromFront = false;
        mt.TargetZ = Field.FireBoundaryDepth * (1.5f + (float)Random.NextDouble() * 2);
        mt.VelZSpeed = 1.0f;
        mt.VelZSpeedDecayRate = 0.9f;
        et.IsFiringStraight = false;
        et.Scale = 8;
        et.Color.X = 1;
        et.Color.Y = 0.7f;
        et.Color.Z = 0.3f;
        et.Color.W = 0;
        enemyFormCountBase = 10;
        enemyFormTicksBase = 20;
        enemyTicksBase = 100;
    }

    private void InitializeForEnemy2(EnemyType et, EnemyMotionType mt)
    {
        {
            mt.AngleSpeed.CycleSpeed = 0;
            mt.AngleSpeed.Amplitude = mt.AngleSpeed.CycleSpeed;
            mt.AngleSpeed.Center = mt.AngleSpeed.Amplitude;
        }

        mt.Radius.Center = ((float)Stage.Random.NextDouble() * 0.9f) * Tube.Radius;
        {
            mt.Radius.CycleSpeed = 0;
            mt.Radius.Amplitude = mt.Radius.CycleSpeed;
        }

        mt.AppearingAngle = (float)(Stage.Random.NextDouble() * Math.PI * 2);
        mt.TargetVelRatio = 1;
        mt.IsAppearingFromFront = false;
        mt.TargetZ = Field.FrontDepth * 2;
        mt.VelZSpeed = 0.6f;
        mt.VelZSpeedDecayRate = 0.9f;
        et.IsFiringStraight = true;
        et.Scale = 7;
        et.Color.X = 0.7f;
        et.Color.Y = 1;
        et.Color.Z = 0.3f;
        et.Color.W = 0;
        et.FireInterval /= 2;
        enemyFormCountBase = 1;
        enemyTicksBase = 5;
    }

    private void InitializeForEnemy3(EnemyType et, EnemyMotionType mt)
    {
        float asr = (Stage.Random.Nextint(2) * 2 - 1);
        mt.AngleSpeed.Center = 0.025f * asr;
        {
            mt.AngleSpeed.CycleSpeed = 0;
            mt.AngleSpeed.Amplitude = mt.AngleSpeed.CycleSpeed;
        }

        mt.Radius.Center = ((float)Stage.Random.NextDouble() * 0.3f + 0.6f) * Tube.Radius;
        {
            mt.Radius.CycleSpeed = 0;
            mt.Radius.Amplitude = mt.Radius.CycleSpeed;
        }

        mt.AppearingAngle = (float)(Stage.Random.NextDouble() * Math.PI * 2);
        mt.TargetVelRatio = 0.1f;
        mt.IsAppearingFromFront = false;
        mt.TargetZ = Field.FrontDepth * 2;
        mt.VelZSpeed = 0.2f;
        mt.VelZSpeedDecayRate = 0.9f;
        et.IsFiringStraight = false;
        et.Scale = 6;
        et.Color.X = 0.9f;
        et.Color.Y = 0.3f;
        et.Color.Z = 0.3f;
        et.Color.W = 0;
        enemyFormCountBase = 10;
        enemyFormTicksBase = 6;
        enemyTicksBase = 100;
    }

    private void InitializeForEnemy4(EnemyType et, EnemyMotionType mt)
    {
        float rr = (Stage.Random.Nextint(2) * 2 - 1);
        {
            mt.AngleSpeed.CycleSpeed = 0;
            mt.AngleSpeed.Amplitude = mt.AngleSpeed.CycleSpeed;
            mt.AngleSpeed.Center = mt.AngleSpeed.Amplitude;
        }

        mt.Radius.Center = 0.5f * Tube.Radius;
        mt.Radius.Amplitude = 0.4f * rr * Tube.Radius;
        mt.Radius.CycleSpeed = (float)(Stage.Random.NextDouble() * 0.5f);
        mt.AppearingAngle = (float)(Stage.Random.NextDouble() * Math.PI * 2);
        mt.TargetVelRatio = 0.04f;
        mt.IsAppearingFromFront = false;
        mt.TargetZ = Field.FrontDepth * 2;
        mt.VelZSpeed = 0.3f + (float)(Stage.Random.NextDouble() * 0.4f);
        mt.VelZSpeedDecayRate = 0.9f;
        et.IsFiringStraight = true;
        et.Scale = 8;
        et.Color.X = 0.7f;
        et.Color.Y = 0.7f;
        et.Color.Z = 0.5f;
        et.Color.W = 0;
        enemyFormCountBase = 10;
        enemyFormTicksBase = 10;
        enemyTicksBase = 100;
    }
}

public enum StagePillarType
{
    Bar,
    Single,
    ShortBar,
    Horizontal,
    Vertical,
    HorizontalWall,
    VerticalWall,
    Circle,
    SmallSingle
};
