public static class NrRandom {
    public static int visualSeed=1;
    public static int rand(){visualSeed=visualSeed*214013+2531011;return (visualSeed>>16)&32767;}
    public static int randN(int n){return rand()%n;}
    public static int randNS(int n){return rand()%(n<<1)-n;}
    public static int randNS2(int n){return randN(n)-(n>>1)+randN(n)-(n>>1);}
    public static int absN(int n){return n<0?-n:n;}
}
