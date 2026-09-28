using System;

public static class MmTime
{
    public static float Follow(float ratio) => SimulationTime.Step == 1 ? ratio : SimulationTime.Blend(Math.Min(1, ratio));

    public static Quaternion Rotation(Quaternion rotation)
    {
        if (SimulationTime.Step == 1) return rotation;
        float sine = (float)Math.Sqrt(rotation.X * rotation.X + rotation.Y * rotation.Y + rotation.Z * rotation.Z);
        float angle = (float)Math.Atan2(sine, rotation.W);
        if (Math.Abs(sine) < .00001f) return Quaternion.Identity;
        float ratio = (float)Math.Sin(angle * SimulationTime.Step) / sine;
        return new Quaternion(rotation.X * ratio, rotation.Y * ratio, rotation.Z * ratio, (float)Math.Cos(angle * SimulationTime.Step));
    }
}
