using Lub;
public static class NrSound {
    public static void playMusic(int index) { GameAudio.PlayMusic(index, true); }
    public static void stopMusic() { GameAudio.StopMusic(); }
    public static void fadeMusic() { GameAudio.FadeMusic(); }
    public static void playChunk(int index) { GameAudio.Play(index); }
    public static void haltChunk(int index) { GameAudio.Stop(index); }
}
