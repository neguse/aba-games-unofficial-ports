using static Lub;
using static RrAttract;
public static class RrPreference {
    public static void loadPreference(string text) {
        string[] words=text.Split(",");
        if(words.Length!=322)return;
        int[] values=new int[322];
        for(int i=0;i<322;i++){values[i]=GameMath.parseNonnegative(words[i]);if(values[i]<0)return;}
        if(values[320]>=40||values[321]>=4)return;
        for(int i=1;i<320;i+=2)if(values[i]>1)return;
        for(int mode=0;mode<4;mode++)for(int stage=0;stage<40;stage++){
            hiScore.score[mode][stage]=values[(mode*40+stage)*2];
            hiScore.cleard[mode][stage]=values[(mode*40+stage)*2+1];
        }
        hiScore.stage=values[320];hiScore.mode=values[321];
    }
    public static void savePreference() {
        string text="";
        for(int mode=0;mode<4;mode++)for(int stage=0;stage<40;stage++)text+=hiScore.score[mode][stage].ToString()+","+hiScore.cleard[mode][stage].ToString()+",";
        text+=hiScore.stage.ToString()+","+hiScore.mode.ToString();
        if(Host.Available())Host.Send("scores.save",text);
    }
}
