using Lub;
public static class WkPreference {
 public static void Load(string text){int value=GameMath.parseNonnegative(text);if(value>=0&&value<=999999999)WkCore.hiScore=value;}
 public static void Save(){GameSave.Save("scores", WkCore.hiScore.ToString());}
}
