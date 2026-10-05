using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
namespace Aba;

public sealed record GameInfo(string Id, string Title, bool Vr);

/// <summary>A fresh managed universe for each play session; Lub alone belongs to the host.</summary>
public sealed class GameSession : IDisposable
{
    sealed class GameContext(string path) : AssemblyLoadContext(isCollectible: true)
    {
        readonly AssemblyDependencyResolver resolver = new(path);
        protected override Assembly? Load(AssemblyName name)
        {
            if(name.Name==typeof(Lub).Assembly.GetName().Name) return typeof(Lub).Assembly;
            string? file=resolver.ResolveAssemblyToPath(name);
            return file==null?null:LoadFromAssemblyPath(file);
        }
        protected override IntPtr LoadUnmanagedDll(string name)
        {
            string? file=resolver.ResolveUnmanagedDllToPath(name);
            return file==null?IntPtr.Zero:LoadUnmanagedDllFromPath(file);
        }
    }
    GameContext? context;
    Action? initialize, quit, shutdown;
    Action<float>? frame;
    Action<EventData>? events;
    bool disposed;
    public GameInfo Info { get; }
    public string DirectoryPath { get; }
    public WeakReference Unloaded { get; }
    public GameSession(GameInfo info, string root)
    {
        Info=info; DirectoryPath=Path.Combine(root,"games",info.Id);
        string path=Path.Combine(DirectoryPath,"AbaGame.dll");
        context=new GameContext(path); Unloaded=new WeakReference(context);
        try
        {
            var assembly=context.LoadFromAssemblyPath(path);
            var type=assembly.GetType("Game",true)!;
            initialize=Bind<Action>(type,"OnInit"); quit=Bind<Action>(type,"OnQuit");
            frame=Bind<Action<float>>(type,"OnFrame")??throw new InvalidDataException("Missing Game.OnFrame");
            events=Bind<Action<EventData>>(type,"OnEvent");
            var physics=assembly.GetType("McdPhysics");
            if(physics!=null) shutdown=Bind<Action>(physics,"Shutdown");
        }
        catch
        {
            initialize=null;quit=null;frame=null;events=null;shutdown=null;
            context.Unload();context=null;throw;
        }
    }
    static T? Bind<T>(Type type,string method) where T:Delegate => type.GetMethod(method,BindingFlags.Public|BindingFlags.Static)?.CreateDelegate<T>();
    public void Initialize() => initialize?.Invoke();
    public void Frame(float dt) => frame?.Invoke(dt);
    public void Event(EventData e) => events?.Invoke(e);
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Dispose()
    {
        if(disposed) return; disposed=true;
        try { quit?.Invoke(); }
        finally
        {
            try { shutdown?.Invoke(); }
            finally
            {
                initialize=null; quit=null; shutdown=null; frame=null; events=null;
                var old=context; context=null; old?.Unload();
            }
        }
    }
}
