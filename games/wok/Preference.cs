using static Lub;
public static class WkPreference {
 public static void Load(string text){int value=GameMath.parseNonnegative(text);if(value>=0&&value<=999999999)WkCore.hiScore=value;}
 public static void Save(){if(Host.Available())Host.Send("scores.save",WkCore.hiScore.ToString());}
}
