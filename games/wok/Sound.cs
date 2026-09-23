using static Lub;
public static class WkSound {
 public static int playingMusicIdx=-1,nextMusicIdx=-1;
 static float elapsed;
 static int flags;
 public static void playMusic(int index){playingMusicIdx=index;nextMusicIdx=index;elapsed=0;if(Host.Available())Host.Send("music.once",index.ToString());}
 public static void stopMusic(){playingMusicIdx=-1;nextMusicIdx=-1;elapsed=0;if(Host.Available())Host.Send("music.stop","");}
 public static void nextMusic(){nextMusicIdx=(playingMusicIdx+1)%2;}
 public static void playChunk(int index){flags|=1<<index;}
 public static void Frame(float dt){
  if(playingMusicIdx>=0){elapsed+=dt;if(elapsed>=WkData.musicDuration[playingMusicIdx]){float remainder=elapsed-WkData.musicDuration[playingMusicIdx];playMusic(nextMusicIdx);elapsed=remainder;}}
  for(int i=0;i<3;i++)if((flags&(1<<i))!=0&&Host.Available())Host.Send("sound.play",i.ToString());flags=0;
 }
}
