using System;
using System.Collections.Generic;
public static class McdVerification
{
    static int failures;
    static GameManager g;
    static void Check(bool condition, string message) { if (!condition) { failures++; Console.WriteLine("FAIL " + message); } }
    static void Near(float a, float b, float t, string label) { if (!(Math.Abs(a - b) <= t)) { failures++; Console.WriteLine("FAIL " + label + ": " + a.ToString() + " != " + b.ToString()); } }
    static void Tick(int input, int n = 1)
    {
        TwinStickPad.input = input;
        for (int i = 0; i < n; i++) g.move_0();
    }
    static int Count<T>(T[] actor) where T : Actor
    {
        int n = 0;
        foreach (T a in actor) if (a.getExists()) n++;
        return n;
    }
    static void Fresh()
    {
        g.rand.setSeed(1); g.startInGame(); g.enemies.clear(); g.stageManager.idle = true; Tick(0, 73);
    }
    static CentHead CreateCentHead(int kind, float rank, int size)
    {
        if (kind == 0) return CentHeadToAndFrom.create(g.field, g.ship, g.bullets, g.world, rank, size);
        if (kind == 1) return CentHeadChase.create(g.field, g.ship, g.bullets, g.world, rank, size);
        return CentHeadRoll.create(g.field, g.ship, g.bullets, g.world, rank, size);
    }
    static void Controls()
    {
        Check(g.ship.tails.Length == 63 && g.stageManager.appearances.Length == 32, "tail and appearance slots");
        Check(g.state == 0, "starts in title"); Tick(0, 3); Tick(256); Check(g.state == 2, "shot button starts the game");
        Fresh(); Check(g.ship.restartCnt <= 0 && g.ship.exists, "ship ready");
        float x = g.ship._pos.x; Tick(8, 20); Check(g.ship._pos.x > x + .1f, "movement");
        float angle = g.ship.deg; Tick(256 | 1, 10); Near(g.ship.trgDeg, angle, .1f, "held aim"); Check(Count(g.ship.shots.actor) > 0, "shots fired");
        Tick(128, 10); Near(g.ship.trgDeg, -GameMath.PI / 2, .0001f, "second stick");
        int time = g.time; Tick(1024); Tick(0, 5); Check(g.paused && g.time == time, "pause freezes time");
        Tick(1024); Tick(0); Check(!g.paused && g.time > time, "pause resumes");
        Console.WriteLine("CONTROLS");
    }
    static void Tails()
    {
        Fresh(); for (int i = 0; i < 8; i++) g.ship.addTail_1(1);
        Check(g.ship.tailNum == 8 && g.ship.getMultiplier() > 1, "eight tail joints");
        Tick(0, 120); Check(g.ship.tailNum == 8, "tail joints hold");
        int mult = g.ship.getMultiplier(); Tick(512);
        Check(g.ship.tailNum == 0 && g.ship.enhancedShotCnt == mult * 5 - 1 && g.ship.bulletDisapCnt > 0, "cutting tails");
        Tick(256, 3); Check(Count(g.ship.enhancedShots.actor) > 0, "enhanced shots");
        Console.WriteLine("TAILS");
    }
    static void Scores()
    {
        Fresh(); g.score = 49999; g.addScore_1(1); Check(g.left == 1 && g.nextExtendScore == 200000, "first extend");
        g.score = 199999; g.addScore_1(1); Check(g.left == 2 && g.nextExtendScore == 400000, "second extend");
        g.score = 399999; g.addScore_1(1); Check(g.left == 2 && g.nextExtendScore == 600000, "extend cap");
        g.left = 0; g.ship.destroyed_0(); Check(g._isGameOver && g.left == -1, "loss");
        g.prefManager.prefData.recordResult(7654321, 123456);
        Check(g.prefManager.prefData.highScore[0] == 7654321 && g.prefManager.prefData.time[0] == 123456, "ranking record");
        int old = g.prefManager.prefData.highScore[0]; g.prefManager.loadText("bad");
        Check(g.prefManager.prefData.highScore[0] == old, "bad ranking text rejected");
        Console.WriteLine("SCORES");
    }
    static void Enemies()
    {
        for (int kind = 0; kind < 3; kind++)
        {
            for (int size = 0; size <= 2; size++)
            {
                string label = " kind " + kind.ToString() + " size " + size.ToString();
                Fresh(); CentHead spec = CreateCentHead(kind, 30, size);
                Enemy head = spec.setJointedEnemies_5(g.enemies, 0, 10, 0, 0);
                Check(head != null && Count(g.enemies.actor) == spec.bodyLength, "jointed enemies" + label);
                int most = 0;
                for (int i = 0; i < 240; i++) { Tick(0); most = Math.Max(most, Count(g.bullets.simpleBullets.actor)); }
                Check(most > 0, "enemy barrage" + label);
                int before = NullLub.DrawnVertices(); g.draw(); Check(NullLub.DrawnVertices() - before > 0, "enemy draw" + label);
            }
        }
        Fresh(); int score = g.score; Enemy block = g.enemies.getInstance();
        Check(block.set_10(g.stageManager._blockSpec, 0, 0, 0, 0, 1, 1, 1, 1), "block set");
        McdPhysics.BodyPosition(block.ode._bodyId, 30, 0, -11); Tick(0, 2); Check(!block.exists && g.score > score, "fallen block scores");
        int drawn = NullLub.DrawnVertices(); Letter.drawString(Transform.Ortho(), null, GlBlend.SrcAlphaOne, "test", "ABC 123", 10, 20, 3);
        Check(NullLub.DrawnVertices() - drawn > 100, "glyphs");
        var lp = new LinePoint(g.field); Check(lp.pos.Length == 8 && lp.posHist.Length == 40 && lp.posHist[0].Length == 8, "line point history");
        Fresh(); CentHead chase = CentHeadChase.create(g.field, g.ship, g.bullets, g.world, 30, 1);
        Enemy chaseHead = chase.setJointedEnemies_5(g.enemies, 0, 10, 0, 0); Check(chaseHead != null, "chase head");
        if (chaseHead != null)
        {
            McdPhysics.BodyPosition(chaseHead.ode._bodyId, 30, 0, -11); Tick(0, 2);
            Check(!chaseHead.exists && Count(g.tailParticles.actor) > 0, "fallen head drops tail particles");
            Tick(0, 180); Check(g.ship.tailNum > 0 && g.ship.getMultiplier() == g.ship.tailNum + 1, "tail particles join the ship");
        }
        Console.WriteLine("ENEMIES");
    }
    static void Stage()
    {
        Fresh(); g.stageManager.idle = false;
        int maxEnemies = 0, maxBullets = 0;
        for (int i = 1; i <= 4000; i++)
        {
            Tick(256 | (i % 240 < 120 ? 4 : 8));
            maxEnemies = Math.Max(maxEnemies, Count(g.enemies.actor)); maxBullets = Math.Max(maxBullets, Count(g.bullets.simpleBullets.actor));
            // The ship is destroyed only when it falls below z -10; putting it back before that keeps it alive for the whole run.
            if (i % 120 == 0 || McdPhysics.BodyVector(g.ship.ode._bodyId, 0)[2] < -9) { McdPhysics.BodyPosition(g.ship.ode._bodyId, 0, 0, 0); g.ship.ode.reset(); }
        }
        Check(maxEnemies > 10 && maxBullets > 0 && g.stageManager.rank > 0, "stage run: enemies " + maxEnemies.ToString() + " bullets " + maxBullets.ToString() + " rank " + g.stageManager.rank.ToString());
        Console.WriteLine("STAGE");
    }
    // Original bundled ODE DLL: Box/sphere contacts, feedback and falling off the floor.
    static float[][] reference = new float[][] {
        new float[] { 59, 5, .571875f, 0, -.0100158456f, 4.571875f, 0, -.0100158457f, 0, 0, 9.99801936f },
        new float[] { 119, 5, 2.26875f, 0, -.01f, 6.26875f, 0, -.0100000001f, 0, 0, 10.0000001f },
        new float[] { 179, 5, 5.090625f, 0, -.01f, 9.090625f, 0, -.0100000001f, 0, 0, 10f },
        new float[] { 239, 5, 9.0375f, 0, -.00999999966f, 13.0375f, 0, -.0100000001f, 0, 0, 9.99999997f },
        new float[] { 299, 5, 14.109375f, 0, -.00999999951f, 18.109375f, 0, -.0100000001f, 0, 0, 9.99999998f },
        new float[] { 359, 4, 19.1625f, 0, -.0099999996f, 23.1643651f, 0, -3.75607769f, 0, 0, 7.99999995f },
        new float[] { 419, 0, 23.1046387f, 0, -4.89290368f, 27.0953596f, 0, -24.3692741f, 0, 0, 0 },
        new float[] { 479, 0, 25.9273244f, 0, -27.134076f, 29.901354f, 0, -62.9824706f, 0, 0, 0 },
        new float[] { 539, 0, 27.6250101f, 0, -67.3752484f, 31.5823484f, 0, -119.595667f, 0, 0, 0 },
        new float[] { 599, 0, 28.1976958f, 0, -125.616421f, 32.1383429f, 0, -194.208863f, 0, 0, 0 } };
    static void Contacts()
    {
        McdPhysics.Seed(1); OdeHandle w = McdPhysics.WorldCreate(); McdPhysics.WorldConfigure(w); OdeHandle group = McdPhysics.GroupCreate();
        OdeHandle floor = McdPhysics.Box(null, 40, 40, 1); McdPhysics.GeomPosition(floor, 0, 0, -1);
        var bodies = new OdeHandle[2]; var geoms = new OdeHandle[2];
        for (int i = 0; i < 2; i++)
        {
            OdeHandle b = McdPhysics.BodyCreate(w); bodies[i] = b; McdPhysics.BodyGravity(b, 0);
            OdeMass m = McdPhysics.Mass(); McdPhysics.MassBox(m, 1, 1, 1); McdPhysics.MassAdjust(m, 1); McdPhysics.BodyMass(b, m); McdPhysics.BodyPosition(b, i * 4, 0, 2);
            geoms[i] = i == 0 ? McdPhysics.Box(null, 1, 1, 1) : McdPhysics.Sphere(null, .5f); McdPhysics.GeomBody(geoms[i], b);
        }
        for (int t = 0; t <= 599; t++)
        {
            McdPhysics.ResetFeedback(); var contacts = new List<OdeContact>();
            for (int i = 0; i < 2; i++)
            {
                McdPhysics.BodyForce(bodies[i], t < 300 ? .125f : -.125f, 0, -2);
                foreach (OdeContact c in McdPhysics.Contacts(w, group, geoms[i], floor, true)) contacts.Add(c);
            }
            McdPhysics.WorldStep(w);
            if (t % 60 == 59)
            {
                var row = new List<float>(); row.Add(t); row.Add(contacts.Count);
                foreach (OdeHandle b in bodies) foreach (float v in McdPhysics.BodyVector(b, 0)) row.Add(v);
                var sum = new float[] { 0, 0, 0 };
                foreach (OdeContact c in contacts) { float[] f = McdPhysics.Feedback(c.Joint, 1); for (int i = 0; i < 3; i++) sum[i] += f[i]; }
                for (int i = 0; i < 3; i++) row.Add(sum[i]);
                float[] expected = reference[(t + 1) / 60 - 1];
                Check(row[1] == expected[1], "ODE contacts " + t.ToString() + ": " + row[1].ToString() + " != " + expected[1].ToString());
                for (int i = 2; i < row.Count; i++) Near(row[i], expected[i], .00005f, "ODE " + t.ToString() + "/" + (i + 1).ToString());
            }
            McdPhysics.GroupEmpty(group);
        }
        McdPhysics.GroupDestroy(group); McdPhysics.GeomDestroy(floor); foreach (OdeHandle geom in geoms) McdPhysics.GeomDestroy(geom); McdPhysics.WorldDestroy(w);
        Console.WriteLine("ODE CONTACTS");
    }
    static float[][] chainReference = new float[][] {
        new float[] { 599, -0.980286337f, -6.36093847f, 1.14291272f, -0.0252237692f, -6.60355634f, 1.50103316f, 0.949540308f, -6.48335296f, 1.8859723f, 1.79248823f, -5.98902652f, 2.31634236f, 2.41385808f, -5.21811034f, 2.60966964f, 2.89063485f, -4.33696082f, 2.68079432f, 3.32608604f, -3.4332434f, 2.7032802f, 3.70564017f, -2.50675861f, 2.80146187f },
        new float[] { 1199, 0.75208507f, -5.28502118f, 2.06679669f, 1.59299712f, -4.74858353f, 2.21047471f, 2.39299397f, -4.14850514f, 2.21822738f, 3.14603187f, -3.49380011f, 2.3546222f, 3.74371646f, -2.70343419f, 2.63334916f, 4.06937314f, -1.77104964f, 2.97639083f, 4.1001714f, -0.773927667f, -3.04185101f, 3.86567287f, 0.198476356f, -2.77326295f } };
    static void Chain()
    {
        McdPhysics.Seed(1); OdeHandle world = McdPhysics.WorldCreate(); McdPhysics.WorldConfigure(world);
        var bodies = new OdeHandle[8];
        for (int i = 0; i <= 7; i++)
        {
            OdeHandle b = McdPhysics.BodyCreate(world); bodies[i] = b; McdPhysics.BodyGravity(b, 0);
            OdeMass mass = McdPhysics.Mass(); McdPhysics.MassBox(mass, 1, .5f, 1); McdPhysics.MassAdjust(mass, 1); McdPhysics.BodyMass(b, mass); McdPhysics.BodyPosition(b, 0, -i, 0);
            if (i > 0)
            {
                OdeHandle joint = McdPhysics.Hinge(world); McdPhysics.JointAttach(joint, b, bodies[i - 1]); McdPhysics.HingeAnchor(joint, 0, -i + .5f, 0);
                McdPhysics.HingeAxis(joint, 0, 0, 1); McdPhysics.HingeLimit(joint, 0, -1); McdPhysics.HingeLimit(joint, 1, 1);
            }
        }
        for (int i = 0; i <= 1199; i++)
        {
            McdPhysics.BodyForce(bodies[0], (float)Math.Sin(i * .031f) * 2, (float)Math.Cos(i * .019f) * 2, 0); McdPhysics.BodyForceAt(bodies[0], .3f, 0, 0, 0, .5f, 0);
            foreach (OdeHandle b in bodies)
            {
                float[] v = McdPhysics.BodyVector(b, 1); McdPhysics.BodyVelocity(b, v[0] * .99f, v[1] * .99f, v[2] * .99f);
                v = McdPhysics.BodyVector(b, 2); McdPhysics.BodyAngularVelocity(b, v[0] * .9f, v[1] * .9f, v[2] * .9f);
            }
            McdPhysics.WorldStep(world);
            if (i == 599 || i == 1199)
            {
                float[] row = chainReference[i == 599 ? 0 : 1];
                for (int j = 0; j < bodies.Length; j++)
                {
                    float[] p = McdPhysics.BodyVector(bodies[j], 0), r = McdPhysics.BodyVector(bodies[j], 4);
                    Near(p[0], row[j * 3 + 1], .00005f, "hinge x"); Near(p[1], row[j * 3 + 2], .00005f, "hinge y");
                    Near((float)Math.Atan2(r[4], r[5]), row[j * 3 + 3], .00005f, "hinge angle");
                }
            }
        }
        McdPhysics.WorldDestroy(world);
        Console.WriteLine("ODE CHAIN");
    }
    public static void Main()
    {
        Game.OnInit(); g = Game.manager;
        Controls();
        Tails();
        Scores();
        Enemies();
        Stage();
        g.clearAll();
        Contacts();
        Chain();
        Console.WriteLine("RESULT " + failures.ToString());
    }
}
