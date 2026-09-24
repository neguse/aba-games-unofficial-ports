// Copyright 2005 Kenta Cho. Some rights reserved.

public class GrScreen
{
    public static GunroarRand rand = new GunroarRand();
    public int screenShakeCnt;
    public float screenShakeIntense;
    public static void setRandSeed(int seed)
    {
        rand.setSeed(seed);
    }

    public float[] eye()
    {
        float x = 0, y = 0;
        if (screenShakeCnt > 0)
        {
            x = rand.nextSignedFloat(screenShakeIntense * (screenShakeCnt + 4));
            y = rand.nextSignedFloat(screenShakeIntense * (screenShakeCnt + 4));
        }

        return Transform.Translate(Transform.Perspective(), -x, -y, -13);
    }

    public void setScreenShake(int count, float intensity)
    {
        screenShakeCnt = count;
        screenShakeIntense = intensity;
    }

    public void move()
    {
        if (screenShakeCnt > 0)
            screenShakeCnt--;
    }
}
