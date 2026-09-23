using static RrConstants;
using static RrArrays;
public static class RrBarrage {
    public static Barrage[][] barragePattern=Make(BARRAGE_TYPE_NUM,()=>Make(BARRAGE_PATTERN_MAX,()=>new Barrage()));
    public static int[] barragePatternNum=Make(BARRAGE_TYPE_NUM,()=>0);
    public static void initBarragemanager() {
        for(int i=0;i<BARRAGE_TYPE_NUM;i++) {
            var patterns=RrData.barrages[i];barragePatternNum[i]=patterns.Length;
            for(int j=0;j<patterns.Length;j++)barragePattern[i][j].bulletml=patterns[j];
        }
    }
}
