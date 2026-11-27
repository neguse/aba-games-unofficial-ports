using System;
using System.Collections.Generic;

public static class MmVerification
{
    static int failures, sectionFailures;
    static void Check(bool condition, string message) { if (!condition) { failures++; Console.WriteLine("FAIL " + message); } }
    static void Near(float a, float b, float tolerance, string label) { Check(Math.Abs(a - b) <= tolerance, label + ": " + a.ToString() + " != " + b.ToString()); }
    static void Section(string name)
    {
        Console.WriteLine((failures == sectionFailures ? "PASS " : "FAILED ") + name);
        sectionFailures = failures;
    }
    static int Input(int i) => 16 | (i % 240 < 120 ? 1 : 8) | (i % 360 < 90 ? 32 : 0);
    static void Tick(MmFrame f, int input, int n = 1)
    {
        Pad.input = input;
        for (int i = 0; i < n; i++) { f.Update(); f.field.SetEyePosition(); }
    }
    static float[] Snapshot(MmFrame f)
    {
        var p = f.player;
        return new float[] { f.cnt, p.storedPos.X, p.storedPos.Y, p.storedPos.Z, p.deg, f.balls.Length(), f.bullets.Length(), p.score, f.shots.Length(), p.shotCnt, p.left, p.bonusCnt };
    }
    // Random.Next advances both indices and returns seed[storedNext] - seed[nextp]; this state makes its next raw sample `sample`.
    static void SetNextSample(Random random, int sample)
    {
        random.storedNext = 0;
        random.nextp = 1;
        random.seed[1] = sample;
        random.seed[2] = 0;
    }
    // Original C# 0.14, seed 1, with SetEyePosition after each update.
    static readonly float[][] reference = new float[][]
    {
        new float[] { 100, 143.26845f, 192.12083f, 3.5020006f, -2.2249994f, 10, 19, 0, 31, 51, 2, 0 },
        new float[] { 200, 111.084946f, 218.47737f, 1, -2.2249994f, 10, 22, 0, 19, 101, 2, 0 },
        new float[] { 300, 103.7827f, 215.3911f, 1, -2.2249994f, 15, 31, 0, 4, 151, 2, 0 },
        new float[] { 400, 103.7827f, 204.92595f, 1, -3.2500033f, 15, 35, 0, 12, 201, 2, 0 },
        new float[] { 500, 113.71113f, 172.48282f, 1, -4.475008f, 18, 37, 206, 38, 251, 2, 1 },
        new float[] { 600, 161.70529f, 160.356f, 1, -4.475008f, 16, 37, 415, 40, 301, 2, 1 },
        new float[] { 700, 149.94687f, 111.758286f, 1, -4.475008f, 13, 33, 721, 34, 351, 2, 1 },
        new float[] { 800, 171.6495f, 128.05103f, 1, -6.5000157f, 17, 43, 827, 36, 401, 2, 1 },
        new float[] { 900, 191.30128f, 158.88249f, 1, -6.7250166f, 15, 47, 1036, 36, 451, 2, 1 },
        new float[] { 1000, 209.20271f, 190.02736f, 1, -6.7250166f, 15, 58, 1448, 41, 501, 2, 1 },
        new float[] { 1100, 200.1387f, 232.55354f, 1, -7.2500186f, 21, 79, 1448, 22, 551, 2, 1 },
        new float[] { 1200, 178.2797f, 255.88625f, 1, -8.975006f, 18, 86, 1754, 38, 601, 2, 1 },
        new float[] { 1300, 156.54202f, 210.85909f, 1, -8.975006f, 22, 123, 1768, 38, 651, 2, 1 },
        new float[] { 1400, 115.939865f, 219.91144f, 1, -8.975006f, 20, 116, 1995, 27, 701, 2, 1 },
        new float[] { 1500, 114.21045f, 202.2074f, 1, -10.499983f, 20, 119, 1995, 39, 751, 2, 1 },
        new float[] { 1600, 146.08298f, 190.27826f, 24, -11.224972f, 23, 52, 2204, 20, 0, 1, 0 },
        new float[] { 1700, 160.05992f, 176.59785f, 7.9299965f, -11.224972f, 23, 63, 2205, 48, 24, 1, 0 },
        new float[] { 1800, 208.38963f, 187.36958f, 1, -11.249971f, 25, 102, 2308, 35, 78, 1, 0 },
        new float[] { 1900, 246.98978f, 184.88219f, 1, -13.474937f, 24, 111, 2413, 36, 128, 1, 1 },
        new float[] { 2000, 220.8913f, 217.27638f, 1, -13.474937f, 30, 158, 2439, 33, 178, 1, 1 },
        new float[] { 2100, 224.26639f, 253.31969f, 1, -13.474937f, 29, 160, 2952, 15, 228, 1, 1 },
        new float[] { 2200, 216.56831f, 255.10211f, 1, -14.499922f, 32, 193, 3055, 20, 278, 1, 1 },
        new float[] { 2300, 204.0845f, 225.4059f, 24, -15.724903f, 28, 86, 3502, 1, 0, 0, 0 },
        new float[] { 2400, 169.09831f, 224.31317f, 7.6239934f, -15.724903f, 30, 94, 3706, 61, 36, 0, 1 },
        new float[] { 2500, 169.94518f, 174.3208f, 1, -15.724903f, 28, 104, 3912, 39, 86, 0, 1 },
        new float[] { 2600, 152.9877f, 135.53014f, 1, -17.749872f, 32, 150, 4016, 32, 136, 0, 1 },
        new float[] { 2700, 188.53108f, 139.86154f, 1, -17.974869f, 32, 191, 4230, 38, 186, 0, 1 },
        new float[] { 2800, 224.31276f, 143.04764f, 1, -17.974869f, 36, 235, 4239, 22, 236, 0, 1 },
        new float[] { 2900, 252.95137f, 122.47241f, 1.0172501f, -18.49986f, 37, 243, 4249, 4, 286, 0, 1 },
        new float[] { 3000, 228.8884f, 150.50484f, 24, -20.224834f, 36, 90, 4380, 12, 0, -1, 0 },
    };
    // Original CircleParticle and SpringConstraint: Collision, forces, rotation and spring correction.
    static readonly float[][] physics = new float[][]
    {
        new float[] { 59, -9.70379f, -0.24060991f, 3.3077238f, -11.504731f, 0.64389104f, 2.6885693f, -0.11075115f, 0.05916795f, -0.03711915f, -0.2252779f, -0.03068763f, 0.02578044f, -0.56138927f, -0.09331862f, 0.068037175f, 0.81945693f },
        new float[] { 119, -1.7595336f, 1.1566838f, 2.6725006f, -3.3111994f, 0.104988456f, 3.408686f, 0.5143833f, -0.0008018017f, 0.007396698f, 0.505316f, 0.0053265765f, 0.0031068325f, -0.85738677f, -0.14135994f, 0.10280143f, 0.4840947f },
        new float[] { 179, 43.892094f, 0.3552808f, 3.6900806f, 45.82479f, 0.8540938f, 3.3409224f, 0.9187355f, -0.001404196f, 0.009205818f, 0.9230652f, 0.0035582185f, 0.0057325363f, -0.94796234f, -0.15586708f, 0.113254376f, 0.25349975f },
        new float[] { 239, 85.08359f, 1.7163062f, 3.2562048f, 84.91555f, 0.06428392f, 4.4126053f, 0.2645111f, 0.0034601688f, 0.006560564f, 0.2575531f, -0.0008723363f, 0.00959301f, -0.97297984f, -0.15976794f, 0.1160403f, 0.11974842f },
        new float[] { 299, 68.9749f, 1.7490801f, 3.7861f, 69.262764f, 0.10674848f, 4.935716f, -0.7035141f, 0.002919197f, 0.0073537827f, -0.69277954f, -0.0011610761f, 0.01020956f, -0.97905034f, -0.16065128f, 0.11665595f, 0.045464925f },
        new float[] { 359, 20.170341f, 1.0067157f, 4.877218f, 22.169682f, 0.63685524f, 5.136127f, -0.69311714f, -0.0024558306f, 0.011344433f, -0.6790104f, 0.002066791f, 0.008177757f, -0.9800714f, -0.16075704f, 0.116718546f, 0.004686686f },
        new float[] { 419, 5.2509966f, 1.4626456f, 5.139726f, 6.7618084f, 0.3521582f, 5.9170647f, 0.21545458f, 0.005661249f, 0.0057868958f, 0.21440935f, -0.0035392344f, 0.012226582f, -0.9799417f, -0.16070202f, 0.116670825f, -0.017633151f },
        new float[] { 479, 40.813927f, 0.49773285f, 6.4024367f, 38.839962f, 1.0059594f, 6.046669f, 0.73337555f, -0.020887345f, 0.024438381f, 0.7303581f, 0.014053345f, -1.9550323E-05f, -0.9796634f, -0.16063797f, 0.116620034f, -0.029842649f },
        new float[] { 539, 74.50911f, 0.74538076f, 6.8194156f, 72.417046f, 0.8466188f, 6.7485447f, 0.28385925f, -0.00014859438f, 0.009958744f, 0.2609253f, 0.00016927719f, 0.009736061f, -0.9794499f, -0.16059269f, 0.11658485f, -0.036521383f },
        new float[] { 599, 68.44751f, 1.7341483f, 6.71927f, 67.7007f, 0.1905825f, 7.799744f, -0.4167099f, 0.022436857f, -0.0058293343f, -0.3319931f, -0.014919609f, 0.020318508f, -0.97931314f, -0.16056485f, 0.116563395f, -0.040175036f },
        new float[] { 659, 42.30795f, 1.3933883f, 7.550689f, 44.075584f, 0.41946584f, 8.232438f, -0.3455391f, 0.013512731f, 0.00042963028f, -0.32712555f, -0.008987755f, 0.016179085f, -0.97923255f, -0.1605487f, 0.116551f, -0.042173926f },
        new float[] { 719, 34.445374f, 1.2052419f, 8.275909f, 36.367935f, 0.5458203f, 8.737462f, 0.04021454f, -0.00801599f, 0.015507698f, 0.06833649f, 0.00535506f, 0.0061454773f, -0.97918665f, -0.16053963f, 0.11654401f, -0.043267567f },
        new float[] { 779, 43.917484f, 0.8494606f, 9.118772f, 41.71587f, 0.7835085f, 9.164908f, 0.13272476f, -0.10999167f, 0.08689499f, 0.066833496f, 0.0733338f, -0.041438103f, -0.97916114f, -0.16053456f, 0.116540134f, -0.043865923f },
        new float[] { 839, 42.67248f, 0.605633f, 9.883384f, 40.57377f, 0.9463336f, 9.644907f, -0.096637726f, -0.009631693f, 0.01663971f, -0.12559128f, 0.006424427f, 0.005405426f, -0.9791471f, -0.16053173f, 0.11653798f, -0.04419331f },
        new float[] { 899, 37.71628f, 1.0793399f, 10.145787f, 35.7729f, 0.6306735f, 10.459915f, 0.0270195f, 0.016034126f, -0.0013256073f, 0.032794952f, -0.01068759f, 0.017382622f, -0.97913903f, -0.1605305f, 0.116536744f, -0.04437244f },
        new float[] { 959, 48.93958f, 0.6503458f, 11.040024f, 51.081474f, 0.916759f, 10.853513f, 0.34489822f, 0.07138455f, -0.04007435f, 0.42485428f, -0.047588527f, 0.04321289f, -0.9791351f, -0.1605296f, 0.11653628f, -0.044470463f },
        new float[] { 1019, 71.51786f, 1.1505529f, 11.283874f, 73.505455f, 0.5833345f, 11.680922f, 0.2326355f, 0.04054284f, -0.018481255f, 0.23085785f, -0.027028084f, 0.028820992f, -0.9791318f, -0.16052942f, 0.116535895f, -0.04452408f },
        new float[] { 1079, 66.148926f, 1.8196335f, 11.409567f, 66.32341f, 0.13730167f, 12.58721f, -0.3651886f, 0.01919794f, -0.003537178f, -0.5130844f, -0.012798265f, 0.01886177f, -0.9791318f, -0.16052942f, 0.116535895f, -0.04455342f },
        new float[] { 1139, 28.402369f, 1.5264907f, 12.20888f, 26.923124f, 0.33274764f, 13.044493f, -0.68639946f, 0.023112297f, -0.0062761307f, -0.66332436f, -0.0154079795f, 0.020687103f, -0.9791318f, -0.16052942f, 0.116535895f, -0.04456948f },
        new float[] { 1199, 7.172419f, 1.354056f, 12.923655f, 5.456725f, 0.44771773f, 13.558081f, 0.045547485f, 0.037426353f, -0.016296387f, 0.08024502f, -0.024950534f, 0.027365685f, -0.9791318f, -0.16052942f, 0.116535895f, -0.044578277f },
    };
    public static void Main()
    {
        // Saving waits for the record to load (FrameHost.Begin reads it).
        FrameHost.loaded = true;
        var f = new MmFrame(); f.LoadContent(); f.Seed(1); f.StartInGame();
        for (int i = 1; i <= 3000; i++)
        {
            Tick(f, Input(i));
            if (i % 100 == 0)
            {
                float[] actual = Snapshot(f), expected = reference[i / 100 - 1];
                for (int j = 0; j < expected.Length; j++) Near(actual[j], expected[j], j >= 1 && j <= 4 ? .0001f : 0, "game " + i.ToString() + " field " + (j + 1).ToString());
            }
        }
        Section("original C# game: 3000 updates");

        var a = new CircleParticle(); var b = new CircleParticle(); a.Clear(); b.Clear();
        a.Set(new Vector3(-1, 0, 3)); b.Set(new Vector3(1, 0, 3)); a.Mass = 2; b.Mass = 3;
        for (int i = 0; i <= 1199; i++)
        {
            a.AddMasslessForce(new Vector3((float)Math.Sin(i * .017f) * 2, .3f, -.2f)); b.AddMasslessForce(new Vector3((float)Math.Cos(i * .023f) * -2, -.2f, .15f));
            a.CheckCollision(b); a.Update(); b.Update();
            if (i % 3 == 0) SpringConstraint.Resolve(a.Pos, b.Pos, a.InvMass, b.InvMass, .5f, 2);
            if (i % 60 == 59)
            {
                Vector3 av = a.Velocity, bv = b.Velocity;
                float[] actual = new float[] { i, a.Pos.X, a.Pos.Y, a.Pos.Z, b.Pos.X, b.Pos.Y, b.Pos.Z, av.X, av.Y, av.Z, bv.X, bv.Y, bv.Z, a.Rotation.X, a.Rotation.Y, a.Rotation.Z, a.Rotation.W };
                float[] expected = physics[(i + 1) / 60 - 1];
                for (int j = 0; j < expected.Length; j++) Near(actual[j], expected[j], .003f, "physics " + i.ToString() + " field " + (j + 1).ToString());
            }
        }
        var random = new Random(1);
        foreach (int expected in new int[] { 534011718, 237820880, 1002897798, 1657007234, 1412011072, 929393559, 760389092, 2026928803, 217468053, 1379662799 })
            Check(random.Next() == expected, "Random(1).Next " + expected.ToString());
        foreach (int sample in new int[] { 0, 1073741823, 2147483646 })
            foreach (int max in new int[] { 2, 3, 6, 10, 180, 360 })
            {
                SetNextSample(random, sample);
                Check(random.Nextint(max) == (sample == 0 ? 0 : sample == 2147483646 ? max - 1 : (max - 1) / 2), "Nextint " + max.ToString() + " of sample " + sample.ToString());
            }
        var pool = new ParticlePool(3, null); var particle = new Particle(); particle.Pos.X = 10;
        pool.Add(particle); particle.Pos.X = 20; pool.Add(particle); particle.Pos.X = 30; pool.Add(particle);
        Check(pool.Get(0).Pos.X == 10 && pool.Get(1).Pos.X == 20, "pool keeps copies of added actors");
        var copy = pool.Get(0); copy.Pos.X = 99; Check(pool.Get(0).Pos.X == 10, "pool Get returns a copy");
        pool.Removeint(0); pool.Gc(); Check(pool.Length() == 2 && pool.Get(0).GetId() == 0 && pool.Get(0).Pos.X == 30, "pool compaction moves the last actor");
        particle.Pos.X = 40; pool.Add(particle); Check(pool.Get(0).Pos.X == 30 && pool.Get(2).Pos.X == 40, "pool adds after compaction");
        Section("physics, RNG boundaries, actor value copies and compaction");

        f.Seed(1); f.StartInGame(); var recording = new List<float[]>();
        for (int i = 1; i <= 600; i++) { Tick(f, Input(i)); recording.Add(Snapshot(f)); }
        Check(f.replay.data.Count == 600, "replay records 600 updates");
        f.StartTitle();
        for (int i = 1; i <= 600; i++)
        {
            Tick(f, 0);
            float[] actual = Snapshot(f), expected = recording[i - 1];
            for (int j = 0; j < expected.Length; j++) Near(actual[j], expected[j], .0001f, "replay " + i.ToString() + " field " + (j + 1).ToString());
        }
        f.StartInGame(); Tick(f, 0, 2);
        var p = f.player;
        for (int i = 0; i < 110; i++) p.GetBonus(p.storedPos);
        Check(p.multiplier == 100, "multiplier caps at 100");
        Tick(f, 96, 3); Check(p.storedIsInHyper, "hyper mode starts");
        for (int i = 0; i < 1000; i++) p.GetBonus(p.storedPos);
        Check(p.multiplier == 999, "hyper multiplier caps at 999");
        int score = p.score; p.AddScoreint(100); Check(p.score == score + 99900, "hyper multiplies the score");
        p.EndHyperMode(); Check(!p.storedIsInHyper && p.multiplier == 0, "hyper mode ends");
        p.score = 999999; p.AddScoreint(1); Check(p.left == 3 && p.nextExtendScore == 3000000, "first extend");
        p.AddScoreint(2000000); Check(p.left == 3 && p.nextExtendScore == 6000000, "extend keeps three ships");
        float x = p.storedPos.X, y = p.storedPos.Y;
        Tick(f, 128); Check(f.storedPauseCnt >= 0, "pause");
        Tick(f, 8, 10); Check(p.storedPos.X == x && p.storedPos.Y == y, "pause stops the player");
        Tick(f, 128); Check(f.storedPauseCnt < 0, "resume");
        Tick(f, 0); Tick(f, 17); Check(p.dashCnt > 0 && f.grenades.Length() > 0, "dash throws a grenade");
        f.stage.appearanceWaitCnt = 0; f.stage.appearanceCntDec = 31; Tick(f, 0);
        int boss = -1;
        for (int i = 0; i < f.balls.Length(); i++) if (f.balls.actors[i].BaseRadius > 5) { boss = i; break; }
        Check(boss >= 0, "boss appearance");
        if (boss >= 0)
        {
            f.balls.actors[boss].Hardness = 1e9f;
            f.balls.actors[boss].BurstRadius = f.balls.actors[boss].State.Radius;
            f.balls.AddDamageintfloat(boss, 1);
        }
        Check(f.stage.appearanceWaitCnt == 180 && f.stage.appearanceCntDec == 10, "boss burst advances the stage");
        p.score = 7654321; p.left = 0; p.Destroy(); Check(p.gameoverCnt == 1 && p.left == -1, "game over");
        Tick(f, 0, 601);
        Check(f.state == MmFrameGameState.Title && f.record.Scores[0] == 7654321 && p.isInReplay, "game over records the score and shows the replay");
        // The null lub saves nothing; MmPreference.Save sends the record's scores joined by commas to FrameHost.savePath.
        Check(FrameHost.savePath == "save/mazer-mayhem/scores.txt" && string.Join(",", f.record.Scores).StartsWith("7654321,"), "saved scores");
        string scores = "7654321,1000000,900000,800000,700000,600000,500000,400000,300000,200000";
        var record = new Record(); record.Load(); MmPreference.Load(record, scores);
        Check(record.Scores[0] == 7654321 && record.Scores[9] == 200000, "scores load");
        MmPreference.Load(record, "1,2,3,4,5,6,7,8,9,10"); Check(record.Scores[0] == 7654321, "ascending scores are rejected");
        var sound = new Sound(); sound.PlayBgm("Mm1"); Check(FrameHost.music == 11 && FrameHost.musicVolume == 1, "bgm starts");
        sound.FadeoutBgm();
        for (int i = 0; i < 60; i++) sound.Update();
        Near(FrameHost.musicVolume, .5f, .0001f, "fade midpoint"); Check(FrameHost.music == 11, "bgm plays while fading");
        for (int i = 0; i < 60; i++) sound.Update();
        Check(FrameHost.music == -1, "bgm stops after the fade");
        Section("replay, hyper, extends, pause, dash, boss progression, game over, scores and 120-update music fade");
        Console.WriteLine("RESULT " + failures.ToString());
    }
}
