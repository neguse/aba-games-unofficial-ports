using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

public static class MucadePhysicsTests
{
    static void Check(bool success, string message)
    {
        if (!success) throw new Exception(message);
    }

    static void Lifecycle()
    {
        for (int run = 0; run < 20; run++)
        {
            var world = McdPhysics.WorldCreate();
            McdPhysics.WorldConfigure(world);
            McdPhysics.WorldGravity(world, 0, 0, -2);
            var space = McdPhysics.SpaceCreate();
            var group = McdPhysics.GroupCreate();
            var body = McdPhysics.BodyCreate(world);
            var box = McdPhysics.Box(space, 1, 1, 1);
            var floor = McdPhysics.Plane(space, 0, 0, 1, 0);
            var mass = McdPhysics.Mass();
            McdPhysics.MassBox(mass, 1, 1, 1);
            McdPhysics.MassAdjust(mass, 1);
            McdPhysics.BodyMass(body, mass);
            McdPhysics.BodyPosition(body, 0, 0, .45f);
            McdPhysics.GeomBody(box, body);
            var bodies = new Dictionary<OdeHandle, string> { [body] = "body" };
            Check(bodies[McdPhysics.GetBody(box)] == "body", "GetBody did not preserve dictionary identity");
            Check(McdPhysics.Same(body, McdPhysics.GetBody(box)), "Same identity");
            Check(McdPhysics.Same(null, McdPhysics.GetBody(floor)), "Static body must be null");

            McdPhysics.BodyRotation(body, new float[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0 });
            Check(McdPhysics.BodyVector(body, 4).Length == 12, "Rotation ABI size");
            McdPhysics.BodyRelForce(body, 1, 0, 0);
            Check(McdPhysics.BodyVector(body, 3)[0] == 1, "Relative force");
            McdPhysics.BodySetForce(body, 0, 0, 0);
            McdPhysics.BodySetTorque(body, 0, 0, 0);
            McdPhysics.BodyVelocity(body, 0, 0, 0);
            McdPhysics.BodyAngularVelocity(body, 0, 0, 0);
            McdPhysics.BodyDisable(body);
            McdPhysics.BodyEnable(body);

            int callbacks = 0;
            McdPhysics.Collide(space, (a, b) => { callbacks++; Check(a != null && b != null, "Callback handles"); });
            Check(callbacks > 0, "Space collision callback did not run");
            callbacks = 0;
            McdPhysics.CollideWith(box, space, (a, b) => callbacks++);
            Check(callbacks > 0, "CollideWith callback did not run");
            bool caught = false;
            try { McdPhysics.Collide(space, (a, b) => throw new InvalidOperationException("callback sentinel")); }
            catch (InvalidOperationException ex) { caught = ex.Message == "callback sentinel"; }
            Check(caught, "Callback exception did not return safely through native code");

            McdPhysics.ResetFeedback();
            int feedbacks = 0;
            OdeHandle lastJoint = null;
            for (int i = 0; i < 35; i++)
                foreach (var c in McdPhysics.Contacts(world, group, box, floor, true))
                {
                    lastJoint = c.Joint;
                    feedbacks++;
                }
            Check(feedbacks == 100, "Feedback storage limit must remain exactly 100");
            McdPhysics.WorldStep(world);
            foreach (float f in McdPhysics.Feedback(lastJoint, 1)) Check(float.IsFinite(f), "Body 1 feedback");
            foreach (float f in McdPhysics.Feedback(lastJoint, 2)) Check(float.IsFinite(f), "Body 2 feedback");
            McdPhysics.GroupEmpty(group);
            caught = false;
            try { McdPhysics.Feedback(lastJoint, 1); }
            catch (ObjectDisposedException) { caught = true; }
            Check(caught, "Empty contact group retained a stale usable joint");
            McdPhysics.ResetFeedback();
            Check(McdPhysics.Contacts(world, group, box, floor, false).Length == 0, "Disabled feedback must not return contacts");
            McdPhysics.GroupEmpty(group);

            // A transformed shape owns no native child by default in the original
            // ODE API. Session cleanup must account for that standalone geometry.
            var transformed = McdPhysics.Transform(space);
            var child = McdPhysics.Box(null, 1, 1, 1);
            McdPhysics.TransformGeom(transformed, child);
            McdPhysics.GeomPosition(child, 2, 0, 0);
            McdPhysics.GeomBody(transformed, body);
            if (run % 2 == 0)
            {
                McdPhysics.GroupDestroy(group);
                McdPhysics.SpaceDestroy(space);
                McdPhysics.WorldDestroy(world);
                Check(McdPhysics.LiveHandleCount == 1, "Normal world close left unexpected resources");
            }
            // Odd iterations mimic an interrupted init/frame before OnQuit.
            McdPhysics.Shutdown();
            McdPhysics.Shutdown();
            Check(McdPhysics.LiveHandleCount == 0, "Session cleanup leaked handles");
        }
    }

    public static void RunIsolatedSession()
    {
        try { ReferencePhysics.Run(); Lifecycle(); }
        finally { McdPhysics.Shutdown(); }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static WeakReference LoadAndUnloadSession()
    {
        var context = new AssemblyLoadContext("mucade-physics-test", isCollectible: true);
        var reference = new WeakReference(context);
        var assembly = context.LoadFromAssemblyPath(Assembly.GetExecutingAssembly().Location);
        assembly.GetType(nameof(MucadePhysicsTests)).GetMethod(nameof(RunIsolatedSession)).Invoke(null, null);
        context.Unload();
        return reference;
    }

    static void CollectibleSessions()
    {
        for (int session = 0; session < 3; session++)
        {
            var reference = LoadAndUnloadSession();
            for (int attempt = 0; reference.IsAlive && attempt < 10; attempt++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            Check(!reference.IsAlive, "ODE bridge retained a collectible game assembly");
        }
    }

    public static int Main()
    {
        try
        {
            for (int i = 0; i < 3; i++)
            {
                ReferencePhysics.Run();
                Check(McdPhysics.LiveHandleCount == 0, "Reference fixture leaked handles");
                McdPhysics.Shutdown();
            }
            Lifecycle();
            CollectibleSessions();
            Console.WriteLine("PASS Mu-cade native ODE: original contact/force/hinge fixtures x3; lifecycle/callback/feedback-limit checks x20; collectible assembly unload x3");
            return 0;
        }
        finally { McdPhysics.Shutdown(); }
    }
}
