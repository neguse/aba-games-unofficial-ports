using System;
using Lub;

// gamepad と XR の controller を合わせた状態 (Y は上が +)。
public class PadInput
{
    public float LeftX, LeftY, RightX, RightY, LeftTrigger, RightTrigger;
    public bool Up, Down, Left, Right, Primary, Secondary, West, North, Start, Back, LeftShoulder, RightShoulder;

    public static int DirectionMask(float x, float y, bool up, bool down, bool left, bool right)
    {
        return (up || y > .35f ? 1 : 0) | (down || y < -.35f ? 2 : 0) | (left || x < -.35f ? 4 : 0) | (right || x > .35f ? 8 : 0);
    }

    public int LeftMask { get { return DirectionMask(LeftX, LeftY, Up, Down, Left, Right); } }
    public int RightMask { get { return DirectionMask(RightX, RightY, false, false, false, false); } }
    public bool A { get { return Primary || RightTrigger > .35f; } }
    public bool B { get { return Secondary || LeftTrigger > .35f; } }
}

// lub のキー名 (Lub.Input.KeyDown に渡す名前) と、押したときに立てる mask の bit。
public class KeyBinding
{
    public string Key;
    public int Mask;

    public KeyBinding(string key, int mask)
    {
        Key = key;
        Mask = mask;
    }
}

// キーと pad を、ゲームの入力の mask にする。
// キーの割り当ては GameProfile.Controls (lub のキー名と mask の組)、pad の割り当ては GameProfile.PadMask。
// 入力のフォーカスが無いときは何も押していないことにする。
public static class GameInput
{
    public static bool Focused()
    {
        return Lub.Xr.Active() ? Lub.Xr.Focused() : Lub.Input.Focused();
    }

    public static int Mask()
    {
        if (!Focused())
            return 0;
        int mask = 0;
        for (int i = 0; i < GameProfile.Controls.Length; i++)
            if (Lub.Input.KeyDown(GameProfile.Controls[i].Key))
                mask |= GameProfile.Controls[i].Mask;
        return mask | GameProfile.PadMask(Pad());
    }

    static float Stronger(float a, float b)
    {
        return Math.Abs(a) >= Math.Abs(b) ? a : b;
    }

    public static PadInput Pad()
    {
        var state = new PadInput();
        for (int player = 0; player < 4; player++)
        {
            if (!Lub.Input.GamepadConnected(player))
                continue;
            state.LeftX = Stronger(state.LeftX, Lub.Input.GamepadAxis(player, Lub.Input.PadAxis.LeftX));
            state.LeftY = Stronger(state.LeftY, -Lub.Input.GamepadAxis(player, Lub.Input.PadAxis.LeftY));
            state.RightX = Stronger(state.RightX, Lub.Input.GamepadAxis(player, Lub.Input.PadAxis.RightX));
            state.RightY = Stronger(state.RightY, -Lub.Input.GamepadAxis(player, Lub.Input.PadAxis.RightY));
            state.LeftTrigger = Math.Max(state.LeftTrigger, Lub.Input.GamepadAxis(player, Lub.Input.PadAxis.LeftTrigger));
            state.RightTrigger = Math.Max(state.RightTrigger, Lub.Input.GamepadAxis(player, Lub.Input.PadAxis.RightTrigger));
            state.Up = state.Up || Lub.Input.GamepadDown(player, Lub.Input.PadButton.DpadUp);
            state.Down = state.Down || Lub.Input.GamepadDown(player, Lub.Input.PadButton.DpadDown);
            state.Left = state.Left || Lub.Input.GamepadDown(player, Lub.Input.PadButton.DpadLeft);
            state.Right = state.Right || Lub.Input.GamepadDown(player, Lub.Input.PadButton.DpadRight);
            state.Primary = state.Primary || Lub.Input.GamepadDown(player, Lub.Input.PadButton.South);
            state.Secondary = state.Secondary || Lub.Input.GamepadDown(player, Lub.Input.PadButton.East);
            state.West = state.West || Lub.Input.GamepadDown(player, Lub.Input.PadButton.West);
            state.North = state.North || Lub.Input.GamepadDown(player, Lub.Input.PadButton.North);
            state.Start = state.Start || Lub.Input.GamepadDown(player, Lub.Input.PadButton.Start);
            state.Back = state.Back || Lub.Input.GamepadDown(player, Lub.Input.PadButton.Back);
            state.LeftShoulder = state.LeftShoulder || Lub.Input.GamepadDown(player, Lub.Input.PadButton.LeftShoulder);
            state.RightShoulder = state.RightShoulder || Lub.Input.GamepadDown(player, Lub.Input.PadButton.RightShoulder);
        }
        // 平面のゲームを XR の中で見せているときも、XR の controller で操作できる
        var l = Lub.Xr.Input(0);
        var r = Lub.Xr.Input(1);
        if (l != null && l.Active)
        {
            state.LeftX = Stronger(state.LeftX, l.StickX);
            state.LeftY = Stronger(state.LeftY, l.StickY);
            state.LeftTrigger = Math.Max(state.LeftTrigger, l.Trigger);
            state.Start = state.Start || l.Menu || l.Primary;
            state.Back = state.Back || l.Secondary;
        }
        if (r != null && r.Active)
        {
            state.RightX = Stronger(state.RightX, r.StickX);
            state.RightY = Stronger(state.RightY, r.StickY);
            state.RightTrigger = Math.Max(state.RightTrigger, r.Trigger);
            state.Primary = state.Primary || r.Primary;
            state.Secondary = state.Secondary || r.Secondary;
            state.Start = state.Start || r.Menu;
        }
        return state;
    }

    // マウスの位置を、ゲームの 640x480 の座標にする (Scene.View* は Target を映している window の矩形)。
    public static float PointerX()
    {
        Lub.Input.MousePos(out float mx, out _);
        return (mx - Scene.ViewX) * 640 / Math.Max(1, Scene.ViewWidth);
    }

    public static float PointerY()
    {
        Lub.Input.MousePos(out _, out float my);
        return (my - Scene.ViewY) * 480 / Math.Max(1, Scene.ViewHeight);
    }
}
