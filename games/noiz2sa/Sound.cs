using static Lub;
public static class NrSound {
    public static void playMusic(int index) { if(Host.Available()) Host.Send("music.loop",index.ToString()); }
    public static void stopMusic() { if(Host.Available()) Host.Send("music.stop",""); }
    public static void fadeMusic() { if(Host.Available()) Host.Send("music.fade",""); }
    public static void playChunk(int index) { if(Host.Available()) Host.Send("sound.play",index.ToString()); }
    public static void haltChunk(int index) { if(Host.Available()) Host.Send("sound.stop",index.ToString()); }
}
