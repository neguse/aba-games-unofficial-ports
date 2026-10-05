// .NET replacement for games/mu-cade/OdeApi.cs; never compile both files.
// The game API and solver behavior match ode_host.c. The launcher calls Shutdown
// in its game-session finally block, after Game.OnQuit and before ALC unload.
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

public sealed class OdeHandle
{
    internal enum Resource { World, Space, Group, Body, Geom, Joint }
    internal readonly IntPtr Pointer;
    internal readonly Resource Kind;
    internal readonly OdeHandle Owner;
    internal readonly OdeHandle World;
    internal bool Alive = true;

    internal OdeHandle(IntPtr pointer, Resource kind, OdeHandle owner, OdeHandle world)
    {
        Pointer = pointer;
        Kind = kind;
        Owner = owner;
        World = world;
    }
}

public sealed class OdeMass
{
    // The pinned ODE build uses double precision: mass + c[4] + I[12].
    internal readonly double[] Values = new double[17];
}

public sealed class OdeContact
{
    public OdeHandle Joint;
    public float X, Y, Z;
}

public static class McdPhysics
{
    // Canonical wrappers make a body returned by GetBody the same Dictionary key
    // as the BodyCreate result, without aliasing a later body at a reused address.
    static readonly Dictionary<IntPtr, OdeHandle> handles = new();
    public static int LiveHandleCount => handles.Count;

    static McdPhysics()
    {
        if (Native.mcd_mass_size() != 17 * sizeof(double))
            throw new InvalidOperationException("Mu-cade requires the pinned double-precision ODE bridge.");
    }

    static IntPtr Ptr(OdeHandle handle)
    {
        if (handle == null) return IntPtr.Zero;
        if (!handle.Alive) throw new ObjectDisposedException(nameof(OdeHandle));
        return handle.Pointer;
    }

    static OdeHandle Own(IntPtr pointer, OdeHandle.Resource kind, OdeHandle owner = null, OdeHandle world = null)
    {
        if (pointer == IntPtr.Zero) throw new InvalidOperationException("ODE resource allocation failed.");
        var handle = new OdeHandle(pointer, kind, owner, world);
        handles.Add(pointer, handle);
        return handle;
    }

    static OdeHandle Find(IntPtr pointer) => pointer == IntPtr.Zero ? null : handles[pointer];
    static void Forget(OdeHandle handle)
    {
        handle.Alive = false;
        handles.Remove(handle.Pointer);
    }

    static void ForgetWhere(Predicate<OdeHandle> predicate)
    {
        foreach (var handle in new List<OdeHandle>(handles.Values))
            if (predicate(handle)) Forget(handle);
    }

    public static OdeHandle WorldCreate() => Own(Native.mcd_world_create(), OdeHandle.Resource.World);
    public static void WorldDestroy(OdeHandle a)
    {
        if (a == null || !a.Alive) return;
        Native.mcd_world_destroy(Ptr(a));
        ForgetWhere(h => ReferenceEquals(h.World, a));
        Forget(a);
    }
    public static void WorldConfigure(OdeHandle a) => Native.mcd_world_configure(Ptr(a));
    public static void WorldStep(OdeHandle a) => Native.mcd_world_step(Ptr(a));
    public static void WorldGravity(OdeHandle a, float x, float y, float z) => Native.mcd_world_gravity(Ptr(a), x, y, z);
    public static OdeHandle SpaceCreate() => Own(Native.mcd_space_create(), OdeHandle.Resource.Space);
    public static void SpaceDestroy(OdeHandle a)
    {
        if (a == null || !a.Alive) return;
        Native.mcd_space_destroy(Ptr(a));
        ForgetWhere(h => ReferenceEquals(h.Owner, a));
        Forget(a);
    }
    public static OdeHandle GroupCreate() => Own(Native.mcd_group_create(), OdeHandle.Resource.Group);
    public static void GroupDestroy(OdeHandle a)
    {
        if (a == null || !a.Alive) return;
        Native.mcd_group_destroy(Ptr(a));
        ForgetWhere(h => ReferenceEquals(h.Owner, a));
        Forget(a);
    }
    public static void GroupEmpty(OdeHandle a)
    {
        Native.mcd_group_empty(Ptr(a));
        ForgetWhere(h => ReferenceEquals(h.Owner, a));
    }
    public static void Seed(int seed) => Native.mcd_seed(seed);
    public static OdeHandle BodyCreate(OdeHandle a) => Own(Native.mcd_body_create(Ptr(a)), OdeHandle.Resource.Body, a, a);
    public static void BodyDestroy(OdeHandle a)
    {
        if (a == null || !a.Alive) return;
        Native.mcd_body_destroy(Ptr(a));
        Forget(a);
    }
    public static void BodyGravity(OdeHandle a, int enabled) => Native.mcd_body_gravity(Ptr(a), enabled);
    public static void BodyPosition(OdeHandle a, float x, float y, float z) => Native.mcd_body_position(Ptr(a), x, y, z);
    public static void BodyForce(OdeHandle a, float x, float y, float z) => Native.mcd_body_force(Ptr(a), x, y, z);
    public static void BodyRelForce(OdeHandle a, float x, float y, float z) => Native.mcd_body_rel_force(Ptr(a), x, y, z);
    public static void BodyVelocity(OdeHandle a, float x, float y, float z) => Native.mcd_body_velocity(Ptr(a), x, y, z);
    public static void BodyAngularVelocity(OdeHandle a, float x, float y, float z) => Native.mcd_body_angular_velocity(Ptr(a), x, y, z);
    public static void BodySetForce(OdeHandle a, float x, float y, float z) => Native.mcd_body_set_force(Ptr(a), x, y, z);
    public static void BodySetTorque(OdeHandle a, float x, float y, float z) => Native.mcd_body_set_torque(Ptr(a), x, y, z);
    public static void BodyForceAt(OdeHandle a, float x, float y, float z, float ox, float oy, float oz) => Native.mcd_body_force_at(Ptr(a), x, y, z, ox, oy, oz);
    public static void BodyEnable(OdeHandle a) => Native.mcd_body_enable(Ptr(a));
    public static void BodyDisable(OdeHandle a) => Native.mcd_body_disable(Ptr(a));
    public static void BodyMass(OdeHandle a, OdeMass m) => Native.mcd_body_mass(Ptr(a), m.Values);
    public static OdeHandle Box(OdeHandle a, float x, float y, float z) => Own(Native.mcd_box(Ptr(a), x, y, z), OdeHandle.Resource.Geom, a);
    public static OdeHandle Sphere(OdeHandle a, float r) => Own(Native.mcd_sphere(Ptr(a), r), OdeHandle.Resource.Geom, a);
    public static OdeHandle Plane(OdeHandle a, float x, float y, float z, float d) => Own(Native.mcd_plane(Ptr(a), x, y, z, d), OdeHandle.Resource.Geom, a);
    public static OdeHandle Transform(OdeHandle a) => Own(Native.mcd_transform(Ptr(a)), OdeHandle.Resource.Geom, a);
    public static void TransformGeom(OdeHandle a, OdeHandle b) => Native.mcd_transform_geom(Ptr(a), Ptr(b));
    public static void GeomDestroy(OdeHandle a)
    {
        if (a == null || !a.Alive) return;
        Native.mcd_geom_destroy(Ptr(a));
        Forget(a);
    }
    public static void GeomPosition(OdeHandle a, float x, float y, float z) => Native.mcd_geom_position(Ptr(a), x, y, z);
    public static void GeomBody(OdeHandle a, OdeHandle b) => Native.mcd_geom_body(Ptr(a), Ptr(b));
    public static OdeHandle GetBody(OdeHandle a) => Find(Native.mcd_get_body(Ptr(a)));
    public static bool Same(OdeHandle a, OdeHandle b) => ReferenceEquals(a, b);
    public static OdeHandle Hinge(OdeHandle a) => Own(Native.mcd_hinge(Ptr(a)), OdeHandle.Resource.Joint, a, a);
    public static void JointDestroy(OdeHandle a)
    {
        if (a == null || !a.Alive) return;
        // Group-owned contacts are freed by GroupEmpty/GroupDestroy, as in ODE.
        if (a.Owner != null && a.Owner.Kind == OdeHandle.Resource.Group) return;
        Native.mcd_joint_destroy(Ptr(a));
        Forget(a);
    }
    public static void JointAttach(OdeHandle a, OdeHandle b, OdeHandle c) => Native.mcd_joint_attach(Ptr(a), Ptr(b), Ptr(c));
    public static void HingeAnchor(OdeHandle a, float x, float y, float z) => Native.mcd_hinge_anchor(Ptr(a), x, y, z);
    public static void HingeAxis(OdeHandle a, float x, float y, float z) => Native.mcd_hinge_axis(Ptr(a), x, y, z);
    public static void HingeLimit(OdeHandle a, int param, float value) => Native.mcd_hinge_limit(Ptr(a), param, value);
    public static void MassBox(OdeMass m, float x, float y, float z) => Native.mcd_mass_box(m.Values, x, y, z);
    public static void MassTranslate(OdeMass m, float x, float y, float z) => Native.mcd_mass_translate(m.Values, x, y, z);
    public static void MassAdjust(OdeMass m, float value) => Native.mcd_mass_adjust(m.Values, value);
    public static void MassAdd(OdeMass m, OdeMass n) => Native.mcd_mass_add(m.Values, n.Values);
    public static OdeMass Mass() => new();
    public static float[] BodyVector(OdeHandle body, int kind)
    {
        var result = new float[kind == 4 ? 12 : 3];
        Native.mcd_body_vector(Ptr(body), kind, result);
        return result;
    }
    public static void BodyRotation(OdeHandle body, float[] rotation)
    {
        if (rotation == null || rotation.Length < 12)
            throw new ArgumentException("ODE rotations contain 12 values.", nameof(rotation));
        Native.mcd_body_rotation(Ptr(body), rotation);
    }

    static void WithCallback(Action<OdeHandle, OdeHandle> callback, Action<Native.NearCallback> invoke)
    {
        ArgumentNullException.ThrowIfNull(callback);
        ExceptionDispatchInfo failure = null;
        Native.NearCallback nativeCallback = (_, a, b) =>
        {
            // Managed exceptions must not unwind through the unmanaged solver.
            if (failure != null) return;
            try { callback(Find(a), Find(b)); }
            catch (Exception ex) { failure = ExceptionDispatchInfo.Capture(ex); }
        };
        invoke(nativeCallback);
        GC.KeepAlive(nativeCallback);
        failure?.Throw();
    }
    public static void Collide(OdeHandle space, Action<OdeHandle, OdeHandle> callback) =>
        WithCallback(callback, c => Native.mcd_collide(Ptr(space), c));
    public static void CollideWith(OdeHandle geom, OdeHandle space, Action<OdeHandle, OdeHandle> callback) =>
        WithCallback(callback, c => Native.mcd_collide_with(Ptr(geom), Ptr(space), c));
    public static void ResetFeedback() => Native.mcd_reset_feedback();
    public static OdeContact[] Contacts(OdeHandle world, OdeHandle group, OdeHandle a, OdeHandle b, bool feedback)
    {
        var joints = new IntPtr[4];
        var positions = new float[12];
        int count = Native.mcd_contacts(Ptr(world), Ptr(group), Ptr(a), Ptr(b), feedback ? 1 : 0, joints, positions);
        var result = new OdeContact[count];
        for (int i = 0; i < count; i++)
            result[i] = new OdeContact {
                Joint = Own(joints[i], OdeHandle.Resource.Joint, group, world),
                X = positions[i * 3], Y = positions[i * 3 + 1], Z = positions[i * 3 + 2]
            };
        return result;
    }
    public static float[] Feedback(OdeHandle joint, int body)
    {
        var result = new float[3];
        if (Native.mcd_feedback(Ptr(joint), body, result) == 0)
            throw new InvalidOperationException("This ODE joint has no feedback buffer.");
        return result;
    }

    public static void Shutdown()
    {
        // Groups must go before worlds; geoms must go before their bodies. This
        // also handles interrupted initialization and unowned transform children.
        foreach (var h in new List<OdeHandle>(handles.Values))
            if (h.Kind == OdeHandle.Resource.Group) GroupDestroy(h);
        foreach (var h in new List<OdeHandle>(handles.Values))
            if (h.Kind == OdeHandle.Resource.Space) SpaceDestroy(h);
        foreach (var h in new List<OdeHandle>(handles.Values))
            if (h.Kind == OdeHandle.Resource.Geom) GeomDestroy(h);
        foreach (var h in new List<OdeHandle>(handles.Values))
            if (h.Kind == OdeHandle.Resource.World) WorldDestroy(h);
        Native.mcd_shutdown();
    }

    static class Native
    {
        const string Library = "mucade_physics";
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void NearCallback(IntPtr data, IntPtr a, IntPtr b);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr mcd_world_create();
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_world_destroy(IntPtr a);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_world_configure(IntPtr a);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_world_step(IntPtr a);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_world_gravity(IntPtr a, float x, float y, float z);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr mcd_space_create();
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_space_destroy(IntPtr a);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr mcd_group_create();
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_group_destroy(IntPtr a);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_group_empty(IntPtr a);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_seed(int seed);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr mcd_body_create(IntPtr a);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_body_destroy(IntPtr a);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_body_gravity(IntPtr a, int enabled);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_body_position(IntPtr a, float x, float y, float z);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_body_force(IntPtr a, float x, float y, float z);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_body_rel_force(IntPtr a, float x, float y, float z);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_body_velocity(IntPtr a, float x, float y, float z);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_body_angular_velocity(IntPtr a, float x, float y, float z);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_body_set_force(IntPtr a, float x, float y, float z);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_body_set_torque(IntPtr a, float x, float y, float z);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_body_force_at(IntPtr a, float x, float y, float z, float ox, float oy, float oz);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_body_enable(IntPtr a);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_body_disable(IntPtr a);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_body_mass(IntPtr a, [In] double[] m);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr mcd_box(IntPtr a, float x, float y, float z);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr mcd_sphere(IntPtr a, float r);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr mcd_plane(IntPtr a, float x, float y, float z, float d);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr mcd_transform(IntPtr a);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_transform_geom(IntPtr a, IntPtr b);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_geom_destroy(IntPtr a);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_geom_position(IntPtr a, float x, float y, float z);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_geom_body(IntPtr a, IntPtr b);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr mcd_get_body(IntPtr a);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr mcd_hinge(IntPtr a);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_joint_destroy(IntPtr a);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_joint_attach(IntPtr a, IntPtr b, IntPtr c);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_hinge_anchor(IntPtr a, float x, float y, float z);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_hinge_axis(IntPtr a, float x, float y, float z);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_hinge_limit(IntPtr a, int param, float value);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int mcd_mass_size();
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_mass_box([In, Out] double[] m, float x, float y, float z);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_mass_translate([In, Out] double[] m, float x, float y, float z);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_mass_adjust([In, Out] double[] m, float value);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_mass_add([In, Out] double[] m, [In] double[] n);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_body_vector(IntPtr a, int kind, [Out] float[] result);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_body_rotation(IntPtr a, [In] float[] rotation);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_collide(IntPtr a, NearCallback callback);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_collide_with(IntPtr a, IntPtr b, NearCallback callback);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_reset_feedback();
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int mcd_contacts(IntPtr world, IntPtr group, IntPtr a, IntPtr b, int feedback, [Out] IntPtr[] joints, [Out] float[] positions);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int mcd_feedback(IntPtr a, int body, [Out] float[] result);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mcd_shutdown();
    }
}
