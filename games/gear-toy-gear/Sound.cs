using static Lub;
using System;

public class Sound
{
    public Cue GetCue(string name)
    {
        return new Cue(name);
    }

    public static string[] names = new string[]
    {
        "EnemyDestroyed",
        "Extend",
        "HomingLaser",
        "Laser",
        "MiddleEnemyDestroyed",
        "PlayerDestroyed",
        "PlayerLaser",
        "Shot"
    };
    public Vector3 ListenerPos = new Vector3();
    public void PlaySe3D(string name, Vector3 pos, Vector3 vel = null)
    {
        var cue = new Cue(name);
        cue.Apply3D(new AudioListener { Position = ListenerPos.Copy() }, new AudioEmitter { Position = pos.Copy(), Velocity = vel == null ? new Vector3() : vel.Copy() });
        cue.Play();
    }

    int fadeoutCnt = -1;
    public void Initialize()
    {
    }

    public void Update()
    {
        if (fadeoutCnt > 0)
        {
            fadeoutCnt--;
            if (Host.Available())
                Host.Send("music.volume", (fadeoutCnt / 120f).ToString());
            if (fadeoutCnt == 0)
                StopBgm();
        }
    }

    public void PlayBgm(string name)
    {
        StopBgm();
        fadeoutCnt = -1;
        int index = name == "Gtg1" ? 0 : name == "Gtg2" ? 1 : 2;
        if (Host.Available())
            Host.Send("music.loop", index.ToString());
    }

    public void StopBgm()
    {
        if (Host.Available())
            Host.Send("music.stop", "");
    }

    public void FadeoutBgm()
    {
        fadeoutCnt = 120;
    }

    public void PlaySe(string name)
    {
        for (int i = 0; i < names.Length; i++)
            if (names[(i)] == name && Host.Available())
                Host.Send("sound.play", i.ToString());
    }
}

public class AudioListener
{
    public Vector3 Position = new Vector3();
}

public class AudioEmitter
{
    public Vector3 Position = new Vector3(), Velocity = new Vector3();
}

public enum AudioStopOptions
{
    Immediate
}

public class Cue
{
    static int nextId;
    int id, index;
    bool playing;
    Vector3 position = new Vector3();
    public Cue(string name)
    {
        id = nextId;
        nextId++;
        index = -1;
        for (int i = 0; i < Sound.names.Length; i++)
            if (Sound.names[i] == name)
                index = i;
    }

    public void Apply3D(AudioListener listener, AudioEmitter emitter)
    {
        position = emitter.Position - listener.Position;
        if (playing && Host.Available())
            Host.Send("spatial.update", id.ToString() + "," + PositionText());
    }

    string PositionText()
    {
        return position.X.ToString() + "," + position.Y.ToString() + "," + position.Z.ToString();
    }

    public void Play()
    {
        if (index < 0)
            return;
        playing = true;
        if (Host.Available())
            Host.Send("spatial.play", id.ToString() + "," + index.ToString() + "," + (index == 2 || index == 3 ? "1" : "0") + "," + PositionText());
    }

    public void Stop(AudioStopOptions options)
    {
        playing = false;
        if (Host.Available())
            Host.Send("spatial.stop", id.ToString());
    }
}
