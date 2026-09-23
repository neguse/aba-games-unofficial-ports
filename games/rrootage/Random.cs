public static class RrRandom {
    public static Rand mt = new Rand();
    public static int visualSeed = 1;
    public static void setSeed(int seed) { mt.setSeed(seed); }
    public static int nextRand() { return mt.nextBits(); }
    public static int randN(int n) { return ((nextRand() >> 5) & 134217727) % n; }
    public static int randNS(int n) { return ((nextRand() >> 5) & 134217727) % (n << 1) - n; }
    public static int randNS2(int n) { return randN(n) - (n >> 1) + randN(n) - (n >> 1); }
    public static int absN(int n) { return n < 0 ? -n : n; }
    public static int rand() { visualSeed = visualSeed * 214013 + 2531011; return (visualSeed >> 16) & 32767; }
}
