namespace Aba;

/// <summary>Consume a transition's held buttons before either screen accepts a new action.</summary>
public sealed class SelectionGate
{
    public bool Armed { get; private set; }
    public void Reset() => Armed=false;
    public void Update(bool select, bool returnToList, bool exitMenu)
    {
        if(!select&&!returnToList&&!exitMenu) Armed=true;
    }
}
