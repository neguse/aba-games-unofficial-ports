// Copyright 2003 Kenta Cho. All rights reserved.
public class P47Rand
{
    static Rand generator = new Rand();
    public void setSeed(int seed) { generator.setSeed(seed); }
    public int nextInt(int n) { return generator.nextInt(n); }
    public int nextSignedInt(int n) { return generator.nextSignedInt(n); }
    public float nextFloat(float n) { return generator.nextFloat(n); }
    public float nextSignedFloat(float n) { return generator.nextSignedFloat(n); }
}
