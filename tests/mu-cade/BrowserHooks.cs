using static Lub;

public class IdleStageManager : StageManager
{
    public IdleStageManager(Field field, Ship ship, BulletPool bullets, World world, EnemyPool enemies) : base(field, ship, bullets, world, enemies)
    {
    }

    public override void move_0()
    {
    }
}

public static class BrowserHooks
{
    static string failure;
    static float[] reference = new float[]
    {
        59f, 5f, .571875f, 0f, -.0100158456f, 4.571875f, 0f, -.0100158457f, 0f, 0f, 9.99801936f,
        119f, 5f, 2.26875f, 0f, -.01f, 6.26875f, 0f, -.0100000001f, 0f, 0f, 10.0000001f,
        179f, 5f, 5.090625f, 0f, -.01f, 9.090625f, 0f, -.0100000001f, 0f, 0f, 10f,
        239f, 5f, 9.0375f, 0f, -.00999999966f, 13.0375f, 0f, -.0100000001f, 0f, 0f, 9.99999997f,
        299f, 5f, 14.109375f, 0f, -.00999999951f, 18.109375f, 0f, -.0100000001f, 0f, 0f, 9.99999998f,
        359f, 4f, 19.1625f, 0f, -.0099999996f, 23.1643651f, 0f, -3.75607769f, 0f, 0f, 7.99999995f,
        419f, 0f, 23.1046387f, 0f, -4.89290368f, 27.0953596f, 0f, -24.3692741f, 0f, 0f, 0f,
        479f, 0f, 25.9273244f, 0f, -27.134076f, 29.901354f, 0f, -62.9824706f, 0f, 0f, 0f,
        539f, 0f, 27.6250101f, 0f, -67.3752484f, 31.5823484f, 0f, -119.595667f, 0f, 0f, 0f,
        599f, 0f, 28.1976958f, 0f, -125.616421f, 32.1383429f, 0f, -194.208863f, 0f, 0f, 0f
    };
    static float[] chainReference = new float[]
    {
        599f, -0.980286337f, -6.36093847f, 1.14291272f, -0.0252237692f, -6.60355634f, 1.50103316f, 0.949540308f, -6.48335296f, 1.8859723f, 1.79248823f, -5.98902652f, 2.31634236f, 2.41385808f, -5.21811034f, 2.60966964f, 2.89063485f, -4.33696082f, 2.68079432f, 3.32608604f, -3.4332434f, 2.7032802f, 3.70564017f, -2.50675861f, 2.80146187f,
        1199f, 0.75208507f, -5.28502118f, 2.06679669f, 1.59299712f, -4.74858353f, 2.21047471f, 2.39299397f, -4.14850514f, 2.21822738f, 3.14603187f, -3.49380011f, 2.3546222f, 3.74371646f, -2.70343419f, 2.63334916f, 4.06937314f, -1.77104964f, 2.97639083f, 4.1001714f, -0.773927667f, -3.04185101f, 3.86567287f, 0.198476356f, -2.77326295f
    };

    public static void Command(int command, float value)
    {
        var g = Game.manager;
        if (command == 0)
        {
            g.enemies.clear();
            var old = g.stageManager;
            var idle = new IdleStageManager(g.field, g.ship, g.bullets, g.world, g.enemies);
            idle.rank = old.rank;
            idle.trgRank = old.trgRank;
            idle.cnt = old.cnt;
            idle.rankDownCnt = old.rankDownCnt;
            g.stageManager = idle;
        }
        if (command == 1)
        {
            for (int i = 0; i < 8; i++)
                g.ship.addTail_1(1);
        }
        if (command == 2)
        {
            var spec = new CentHeadChase(g.field, g.ship, g.bullets, g.world, 30, 2);
            spec.setJointedEnemies_5(g.enemies, 0, 10, 0, 0);
        }
        if (command == 3)
        {
            g.score = 7654321;
            g.startGameOver();
        }
        if (command == 4)
        {
            failure = null;
            RunPhysics(g);
            if (failure == null)
                Host.Send("test.physics.done", "1");
            else
                Host.Send("test.physics.fail", failure);
        }
    }

    public static void Report()
    {
        var g = Game.manager;
        var p = g.ship;
        int shots = 0;
        for (int i = 0; i < p.shots.actor.Length; i++)
        {
            if (p.shots.actor[i].exists)
                shots++;
        }

        Host.Send("test.state", g.state + "," + p._pos.x + "," + p._pos.y + "," + p.tailNum + "," + p.enhancedShotCnt + ","
            + (g.paused ? 1 : 0) + "," + g.time + "," + g.prefManager.prefData.highScore[0] + "," + TwinStickPad.input + "," + shots + ","
            + p.trgDeg + "," + (g._isGameOver ? 1 : 0) + "," + g.score + "," + p.restartCnt);
    }

    static void Near(float a, float b, float tolerance, string label)
    {
        if (failure == null && !(System.Math.Abs(a - b) <= tolerance))
            failure = label + ": " + a + " != " + b;
    }

    static void RunPhysics(GameManager g)
    {
        g.clearAll();
        McdPhysics.Seed(1);
        var w = McdPhysics.WorldCreate();
        McdPhysics.WorldConfigure(w);
        var group = McdPhysics.GroupCreate();
        var floor = McdPhysics.Box(null, 40, 40, 1);
        McdPhysics.GeomPosition(floor, 0, 0, -1);
        var bodies = new OdeHandle[2];
        var geoms = new OdeHandle[2];
        for (int i = 0; i < 2; i++)
        {
            var b = McdPhysics.BodyCreate(w);
            bodies[i] = b;
            McdPhysics.BodyGravity(b, 0);
            var m = McdPhysics.Mass();
            McdPhysics.MassBox(m, 1, 1, 1);
            McdPhysics.MassAdjust(m, 1);
            McdPhysics.BodyMass(b, m);
            McdPhysics.BodyPosition(b, i * 4, 0, 2);
            if (i == 0)
                geoms[i] = McdPhysics.Box(null, 1, 1, 1);
            else
                geoms[i] = McdPhysics.Sphere(null, 0.5f);
            McdPhysics.GeomBody(geoms[i], b);
        }

        var joints = new OdeHandle[64];
        var row = new float[16];
        for (int t = 0; t < 600; t++)
        {
            McdPhysics.ResetFeedback();
            int nc = 0;
            for (int i = 0; i < 2; i++)
            {
                McdPhysics.BodyForce(bodies[i], t < 300 ? 0.125f : -0.125f, 0, -2);
                var cs = McdPhysics.Contacts(w, group, geoms[i], floor, true);
                for (int k = 0; k < cs.Length; k++)
                {
                    if (nc < 64)
                    {
                        joints[nc] = cs[k].Joint;
                        nc++;
                    }
                }
            }

            McdPhysics.WorldStep(w);
            if (t % 60 == 59)
            {
                int n = 0;
                row[n] = t;
                n++;
                row[n] = nc;
                n++;
                for (int i = 0; i < 2; i++)
                {
                    var v = McdPhysics.BodyVector(bodies[i], 0);
                    for (int k = 0; k < v.Length; k++)
                    {
                        row[n] = v[k];
                        n++;
                    }
                }

                float s0 = 0;
                float s1 = 0;
                float s2 = 0;
                for (int k = 0; k < nc; k++)
                {
                    var f = McdPhysics.Feedback(joints[k], 1);
                    s0 += f[0];
                    s1 += f[1];
                    s2 += f[2];
                }

                row[n] = s0;
                n++;
                row[n] = s1;
                n++;
                row[n] = s2;
                n++;
                int r = (t + 1) / 60 - 1;
                if (n != 11 || row[1] != reference[r * 11 + 1])
                {
                    if (failure == null)
                        failure = "ODE contacts " + t;
                }
                else
                {
                    for (int i = 2; i < n; i++)
                        Near(row[i], reference[r * 11 + i], 0.00005f, "ODE " + t + "/" + (i + 1));
                }
            }

            McdPhysics.GroupEmpty(group);
        }

        McdPhysics.GroupDestroy(group);
        McdPhysics.GeomDestroy(floor);
        for (int i = 0; i < 2; i++)
            McdPhysics.GeomDestroy(geoms[i]);
        McdPhysics.WorldDestroy(w);

        McdPhysics.Seed(1);
        var world = McdPhysics.WorldCreate();
        McdPhysics.WorldConfigure(world);
        var chain = new OdeHandle[8];
        for (int i = 0; i < 8; i++)
        {
            var b = McdPhysics.BodyCreate(world);
            chain[i] = b;
            McdPhysics.BodyGravity(b, 0);
            var mass = McdPhysics.Mass();
            McdPhysics.MassBox(mass, 1, 0.5f, 1);
            McdPhysics.MassAdjust(mass, 1);
            McdPhysics.BodyMass(b, mass);
            McdPhysics.BodyPosition(b, 0, -i, 0);
            if (i > 0)
            {
                var joint = McdPhysics.Hinge(world);
                McdPhysics.JointAttach(joint, b, chain[i - 1]);
                McdPhysics.HingeAnchor(joint, 0, -i + 0.5f, 0);
                McdPhysics.HingeAxis(joint, 0, 0, 1);
                McdPhysics.HingeLimit(joint, 0, -1);
                McdPhysics.HingeLimit(joint, 1, 1);
            }
        }

        for (int i = 0; i < 1200; i++)
        {
            McdPhysics.BodyForce(chain[0], (float)System.Math.Sin(i * 0.031f) * 2, (float)System.Math.Cos(i * 0.019f) * 2, 0);
            McdPhysics.BodyForceAt(chain[0], 0.3f, 0, 0, 0, 0.5f, 0);
            for (int j = 0; j < 8; j++)
            {
                var v = McdPhysics.BodyVector(chain[j], 1);
                McdPhysics.BodyVelocity(chain[j], v[0] * 0.99f, v[1] * 0.99f, v[2] * 0.99f);
                v = McdPhysics.BodyVector(chain[j], 2);
                McdPhysics.BodyAngularVelocity(chain[j], v[0] * 0.9f, v[1] * 0.9f, v[2] * 0.9f);
            }

            McdPhysics.WorldStep(world);
            if (i == 599 || i == 1199)
            {
                int r = i == 599 ? 0 : 1;
                for (int j = 0; j < 8; j++)
                {
                    var p = McdPhysics.BodyVector(chain[j], 0);
                    var q = McdPhysics.BodyVector(chain[j], 4);
                    Near(p[0], chainReference[r * 25 + j * 3 + 1], 0.00005f, "hinge x");
                    Near(p[1], chainReference[r * 25 + j * 3 + 2], 0.00005f, "hinge y");
                    Near((float)System.Math.Atan2(q[4], q[5]), chainReference[r * 25 + j * 3 + 3], 0.00005f, "hinge angle");
                }
            }
        }

        McdPhysics.WorldDestroy(world);
        g.startTitle();
    }
}
