using System;
using TinySystem.Text;
public static class GameMath {
 public static int integer(float value){return (int)(value<0?Math.Ceiling(value):Math.Floor(value));}
 public static float sin(float value){return (float)Math.Sin(value);}
 public static float cos(float value){return (float)Math.Cos(value);}
 public static float sqrt(float value){return (float)Math.Sqrt(value);}
 public static int parseNonnegative(string text){
  var bytes = new Utf8Bytes(text);
  if(bytes.Length==0||bytes.Length>10)return -1;
  int value=0;
  for(int i=0;i<bytes.Length;i++){int digit=bytes[i] >= 48 && bytes[i] <= 57 ? bytes[i] - 48 : -1;if(digit<0||value>214748364||(value==214748364&&digit>7))return -1;value=value*10+digit;}
  return value;
 }
}
