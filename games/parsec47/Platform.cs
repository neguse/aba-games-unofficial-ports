// Copyright 2003 Kenta Cho. All rights reserved.
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

public class P47PrefManager
{
    public const int MODE_NUM = 2, DIFFICULTY_NUM = 4, REACHED_PARSEC_SLOT_NUM = 10;
    public int[][][] hiScore = new int[2][][];
    public int[][] reachedParsec = new int[2][];
    public int selectedDifficulty = 1, selectedParsecSlot, selectedMode;
    public P47PrefManager()
    {
        for (int index0 = 0; index0 < 2; index0++)
        {
            hiScore[index0] = new int[4][]; reachedParsec[index0] = new int[4];
            for (int index1 = 0; index1 < 4; index1++)
            {
                reachedParsec[index0][index1] = 0; hiScore[index0][index1] = new int[10];
                for (int index2 = 0; index2 < 10; index2++) hiScore[index0][index1][index2] = 0;
            }
        }
        if (Host.Available()) Host.Send("scores.load", "");
    }
    public void load(string data)
    {
        string[] values = data.Split(",");
        if (values.Length != 91) return;
        var numbers = new int[91];
        for (int index3 = 0; index3 < 91; index3++)
        {
            numbers[index3] = GameMath.parseNonnegative(values[index3]);
            if (numbers[index3] < 0) return;
        }
        if (numbers[88] >= 4 || numbers[89] >= 10 || numbers[90] >= 2) return;
        int n = 0;
        for (int index4 = 0; index4 < 2; index4++) for (int index5 = 0; index5 < 4; index5++)
        {
            reachedParsec[index4][index5] = numbers[n]; n++;
            for (int index6 = 0; index6 < 10; index6++) { hiScore[index4][index5][index6] = numbers[n]; n++; }
        }
        selectedDifficulty = numbers[88]; selectedParsecSlot = numbers[89]; selectedMode = numbers[90];
    }
    public void save()
    {
        string data = "";
        for (int index7 = 0; index7 < 2; index7++) for (int index8 = 0; index8 < 4; index8++)
        {
            data += reachedParsec[index7][index8].ToString() + ",";
            for (int index9 = 0; index9 < 10; index9++) data += hiScore[index7][index8][index9].ToString() + ",";
        }
        data += selectedDifficulty.ToString() + "," + selectedParsecSlot.ToString() + "," + selectedMode.ToString();
        if (Host.Available()) Host.Send("scores.save", data);
    }
}

public static class SoundManager
{
    public const int SHOT = 0, ROLL_CHARGE = 1, ROLL_RELEASE = 2, SHIP_DESTROYED = 3, GET_BONUS = 4, EXTEND = 5,
        ENEMY_DESTROYED = 6, LARGE_ENEMY_DESTROYED = 7, BOSS_DESTROYED = 8, LOCK = 9, LASER = 10, BGM_NUM = 4;
    static P47GameManager manager;
    public static void init(P47GameManager game) { manager = game; }
    public static void close() { Sound.stopMusic(); }
    public static void playBgm(int n) { if (Host.Available() && manager.state == P47GameManager.IN_GAME) Host.Send("music.loop", n.ToString()); }
    public static void playSe(int n) { if (Host.Available() && manager.state == P47GameManager.IN_GAME) Host.Send("sound.play", n.ToString()); }
    public static void stopSe(int n) { if (Host.Available()) Host.Send("sound.stop", n.ToString()); }
}
public static class Sound
{
    public static void stopMusic() { if (Host.Available()) Host.Send("music.stop", ""); }
    public static void fadeMusic() { if (Host.Available()) Host.Send("music.fade", ""); }
}
