using System;
public static class GameMath {
 public static int integer(float value){return (int)(value<0?Math.Ceiling(value):Math.Floor(value));}
 public static float sin(float value){return (float)Math.Sin(value);}
 public static float cos(float value){return (float)Math.Cos(value);}
 public static float sqrt(float value){return (float)Math.Sqrt(value);}
 public static int parseNonnegative(string text){
  if(text.Length==0||text.Length>10)return -1;
  int value=0;
  for(int i=0;i<text.Length;i++){int digit="0123456789".IndexOf(text.Substring(i,1));if(digit<0||value>214748364||(value==214748364&&digit>7))return -1;value=value*10+digit;}
  return value;
 }
}
