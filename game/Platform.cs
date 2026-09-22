// Copyright 2004 Kenta Cho. All rights reserved.
using System;
using static Lub;

public class Pad
{
    public const int PAD_UP = 1, PAD_DOWN = 2, PAD_LEFT = 4, PAD_RIGHT = 8, PAD_BUTTON1 = 16, PAD_BUTTON2 = 32;
    public int directions, buttons;
    public bool pause, escape;
    public int getPadState() { return directions; }
    public int getButtonState() { return buttons; }
}

public class RankingItem
{
    public int score, stage;
    public RankingItem(int score, int stage) { this.score = score; this.stage = stage; }
}
public class PrefManager
{
    public const int RANKING_NUM = 10;
    public RankingItem[] ranking = new RankingItem[10];
    public PrefManager()
    {
        for (int i = 0; i < 10; i++) ranking[i] = new RankingItem((10 - i) * 10000, 0);
        if (Host.Available()) Host.Send("scores.load", "");
    }
    public void load(string data)
    {
        string[] values = data.Split(",");
        if (values.Length != 20) return;
        var items = new RankingItem[10];
        for (int i = 0; i < 10; i++)
        {
            int score = GameMath.parseNonnegative(values[i * 2]);
            int stage = GameMath.parseNonnegative(values[i * 2 + 1]);
            if (score < 0 || stage < 0 || stage > 5) return;
            items[i] = new RankingItem(score, stage);
        }
        ranking = items;
    }
    public void save()
    {
        string data = "";
        for (int i = 0; i < 10; i++) data += (i == 0 ? "" : ",") + ranking[i].score.ToString() + "," + ranking[i].stage.ToString();
        if (Host.Available()) Host.Send("scores.save", data);
    }
    public void setHiScore(int score, int stage)
    {
        int i = 0;
        for (; i < 10; i++) if (ranking[i].score < score) break;
        if (i >= 10) return;
        for (int j = 9; j > i; j--) ranking[j] = ranking[j - 1];
        ranking[i] = new RankingItem(score, stage);
        save();
    }
}

public static class SoundManager
{
    public const int STAGE_BGM_NUM = 3;
    static GameManager manager;
    public static void init(GameManager game) { manager = game; }
    public static void close() { Music.haltMusic(); }
    public static void playBgm(int index) { PlayMusic(index, "music.loop"); }
    public static void playBgmOnce(int index) { PlayMusic(index, "music.once"); }
    static void PlayMusic(int index, string topic)
    {
        if (manager.state == GameState.IN_GAME || manager.state == GameState.START_GAME || manager.state == GameState.END_GAME)
            if (Host.Available()) Host.Send(topic, index.ToString());
    }
    public static void playSe(int index)
    {
        if (manager.state == GameState.IN_GAME || manager.state == GameState.START_GAME)
            if (Host.Available()) Host.Send("sound.play", index.ToString());
    }
    public static void haltSe(int index) { if (Host.Available()) Host.Send("sound.stop", index.ToString()); }
}
public static class Music
{
    public static void haltMusic() { if (Host.Available()) Host.Send("music.stop", ""); }
    public static void fadeMusic() { if (Host.Available()) Host.Send("music.fade", ""); }
}
    public static class Bgm { public const int STG1 = 0, STG2 = 1, STG3 = 2, BOSS = 3, LAST_BOSS = 4, ENDING = 5; }
    public static class Se
    {
        public const int SHIP_SHOT = 0, STUCK = 1, STUCK_BONUS = 2, STUCK_DESTROYED = 3, SHIP_DESTROYED = 4,
            ENEMY_DAMAGED = 5, SMALL_ENEMY_DESTROYED = 6, ENEMY_DESTROYED = 7, BOSS_DESTROYED = 8,
            EXTEND = 9, WARNING = 10, PROPELLER = 11, STUCK_BONUS_PUSHIN = 12;
    }
