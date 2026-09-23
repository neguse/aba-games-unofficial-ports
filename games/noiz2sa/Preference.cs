using static Lub;
public static class NrPreference {
    public static void loadPreference(string text){
        string[] words=text.Split(",");if(words.Length!=115)return;
        int[] values=new int[115];for(int i=0;i<115;i++){values[i]=GameMath.parseNonnegative(words[i]);if(values[i]<0)return;}
        if(values[114]>=14)return;
        for(int i=0;i<14;i++)NrAttract.hiScore.stageScore[i]=values[i];
        for(int i=0;i<10;i++)for(int j=0;j<10;j++)NrAttract.hiScore.sceneScore[i][j]=values[14+i*10+j];
        NrAttract.hiScore.stage=values[114];
    }
    public static void savePreference(){
        string text="";for(int i=0;i<14;i++)text+=NrAttract.hiScore.stageScore[i].ToString()+",";
        for(int i=0;i<10;i++)for(int j=0;j<10;j++)text+=NrAttract.hiScore.sceneScore[i][j].ToString()+",";
        text+=NrAttract.hiScore.stage.ToString();if(Host.Available())Host.Send("scores.save",text);
    }
}
