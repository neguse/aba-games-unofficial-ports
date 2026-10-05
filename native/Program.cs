using System.Reflection;
using System.Text.Json;
using static Lub;
using Aba;
using Aba.Native;

return Collection.Run(args);

namespace Aba
{
public static class Collection
{
    static readonly string root=AppContext.BaseDirectory;
    static readonly GameInfo[] games=JsonSerializer.Deserialize<GameInfo[]>(
        Assembly.GetExecutingAssembly().GetManifestResourceStream("Aba.Catalog.json")!,
        new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
    static GameSession? game;
    static HostBridge? bridge;
    static Canvas? canvas;
    static int selected, gameWidth=640,gameHeight=480;
    static int pending=-2; // -2 none, -1 list, >=0 game
    static readonly SelectionGate releaseGate=new();
    static bool armed=>releaseGate.Armed;
    static bool previousConfirm, previousReturn, previousExit;
    static int previousDirection;
    static float repeat;
    static string? error;
    static int smokeFrames=-1,smokeTicks,smokeIndex;
    static bool smokeFailed;
    static readonly List<WeakReference> unloaded=new();
    public static int Run(string[] args)
    {
        var runtime=new List<string>();
        for(int i=0;i<args.Length;i++)
        {
            if(args[i]=="--smoke-all" && i+1<args.Length) smokeFrames=int.Parse(args[++i]);
            else if(args[i]=="--game" && i+1<args.Length)
            {
                string id=args[++i]; pending=Array.FindIndex(games,x=>x.Id==id);
                if(pending<0) { Console.Error.WriteLine($"Unknown game: {id}"); return 2; }
            }
            else runtime.Add(args[i]);
        }
        Directory.SetCurrentDirectory(root);
        int result=Lub.Run(Init,OnEvent,Frame,Quit,runtime.ToArray());
        return result!=0?result:smokeFailed?1:0;
    }
    static void Init()
    {
        Config(new ConfigOpts{Width=960,Height=720});
        Session.AfterFrame=Transition;
        if(smokeFrames>=0) pending=0;
    }
    static void OnEvent(EventData e) => game?.Event(e);
    static void Frame(float dt)
    {
        canvas??=new Canvas();
        bool xr=Xr.Active();
        var left=Xr.Input(0); var right=Xr.Input(1);
        bool goBack=Input.KeyDown("F12") || (Input.GamepadDown(0,Input.PadButton.Back)&&Input.GamepadDown(0,Input.PadButton.Start))
            || (left?.Active==true&&right?.Active==true&&left.StickClick&&right.StickClick);
        bool confirm=Input.KeyDown("return")||Input.KeyDown("space")||Input.GamepadDown(0,Input.PadButton.South)
            || right?.Active==true&&(right.Primary||right.Trigger>.65f);
        bool exitMenu=Input.KeyDown("escape")||Input.GamepadDown(0,Input.PadButton.East)||right?.Active==true&&right.Secondary;
        releaseGate.Update(confirm||Input.MouseDown(1),goBack,exitMenu);
        if(game!=null && goBack&&!previousReturn && armed) pending=-1;
        previousReturn=goBack;
        if(game==null) Menu(dt,xr,left,right,confirm);
        else if(!armed)
        {
            var title=canvas.Target("collection-release",Canvas.Width,Canvas.Height);
            canvas.Begin(title);canvas.Text(game.Info.Title,60,250,36);
            canvas.Text("Release the select button to begin",60,315,24);canvas.End();
            canvas.Present(title,Canvas.Width,Canvas.Height,xr);
        }
        else if(pending==-2)
        {
            try
            {
                bool flat=!game.Info.Vr;
                TextureRef? target=flat?canvas.Target("collection-game",gameWidth,gameHeight):null;
                Gfx.Size(out int windowWidth,out int windowHeight);
                float scale=Math.Min(windowWidth/(float)gameWidth,windowHeight/(float)gameHeight);
                if(bridge?.Masashikun!=null)
                {
                    Input.MousePos(out float mx,out float my);
                    float uiScale=Math.Min(windowWidth/(float)Canvas.Width,windowHeight/(float)Canvas.Height);
                    bridge.Masashikun.HandlePointer((mx-(windowWidth-Canvas.Width*uiScale)/2)/uiScale,
                        (my-(windowHeight-Canvas.Height*uiScale)/2)/uiScale,Input.MousePressed(1));
                }
                bridge?.BeforeFrame(dt,xr?Xr.Focused():Input.Focused(),(windowWidth-gameWidth*scale)/2,(windowHeight-gameHeight*scale)/2,gameWidth*scale,gameHeight*scale);
                if(flat) { Session.MainTarget=target; Session.MainSize=(gameWidth,gameHeight); Session.XrActiveOverride=false; }
                try { game.Frame(dt); }
                finally { Session.MainTarget=null; Session.MainSize=null; Session.XrActiveOverride=null; }
                bridge?.AfterFrame(dt,xr?Xr.Focused():Input.Focused());
                if(flat&&target!=null)
                {
                    if(bridge?.Masashikun!=null)
                    {
                        var ui=canvas.Target("collection-mas-ui",Canvas.Width,Canvas.Height);
                        canvas.Begin(ui);canvas.Image(target,0,0,960,720);bridge.Masashikun.Draw(canvas);canvas.End();
                        canvas.Present(ui,Canvas.Width,Canvas.Height,xr);
                    }
                    else canvas.Present(target,gameWidth,gameHeight,xr);
                }
                if(bridge?.QuitRequested==true) pending=-1;
                if(smokeFrames>=0&&++smokeTicks>=smokeFrames) pending=-1;
            }
            catch(Exception ex) { error=ex.GetBaseException().Message; Console.Error.WriteLine(ex); pending=-1; }
        }
        previousConfirm=confirm;
    }
    static void Menu(float dt,bool xr,XrInput? left,XrInput? right,bool confirm)
    {
        int direction=0;
        float ax=Input.GamepadAxis(0,Input.PadAxis.LeftX),ay=Input.GamepadAxis(0,Input.PadAxis.LeftY);
        if(Input.KeyDown("up")||Input.GamepadDown(0,Input.PadButton.DpadUp)||ay<-.5f||left?.StickY>.5f) direction=-2;
        if(Input.KeyDown("down")||Input.GamepadDown(0,Input.PadButton.DpadDown)||ay>.5f||left?.StickY<-.5f) direction=2;
        if(Input.KeyDown("left")||Input.GamepadDown(0,Input.PadButton.DpadLeft)||ax<-.5f||left?.StickX<-.5f) direction=-1;
        if(Input.KeyDown("right")||Input.GamepadDown(0,Input.PadButton.DpadRight)||ax>.5f||left?.StickX>.5f) direction=1;
        repeat-=dt;
        if(direction!=0&&(direction!=previousDirection||repeat<=0))
        {
            selected=Math.Clamp(selected+direction,0,games.Length-1);
            repeat=direction!=previousDirection?.35f:.12f;
        }
        previousDirection=direction;
        if(armed&&confirm&&!previousConfirm) pending=selected;
        if(Input.KeyPressed("r")) canvas!.Recenter();
        bool leave=Input.KeyDown("escape")||Input.GamepadDown(0,Input.PadButton.East)||right?.Active==true&&right.Secondary;
        if(armed&&leave&&!previousExit) Lub.Quit();
        previousExit=leave;
        var screen=canvas!.Target("collection-menu",Canvas.Width,Canvas.Height);
        canvas.Begin(screen);
        canvas.Text("ABA GAMES",36,22,38,0x83d9ffff);
        canvas.Text("13 games / one collection",38,72,20);
        for(int i=0;i<games.Length;i++)
        {
            float x=36+(i%2)*448,y=122+(i/2)*68;
            bool active=i==selected;
            canvas.Rect(x,y,436,58,active?0x16496affu:0x0d1d32ffu);
            canvas.Text(games[i].Title,x+16,y+6,24,active?0xffffffff:0xd3e0f1ff);
            canvas.Text(games[i].Vr?"VR + Desktop":"2D screen + Desktop",x+17,y+35,13,0x84acc7ff);
            if(!xr)
            {
                Input.MousePos(out float mx,out float my); Gfx.Size(out int w,out int h);
                float scale=Math.Min(w/(float)Canvas.Width,h/(float)Canvas.Height);
                mx=(mx-(w-Canvas.Width*scale)/2)/scale; my=(my-(h-Canvas.Height*scale)/2)/scale;
                if(armed&&mx>=x&&mx<x+436&&my>=y&&my<y+58&&Input.MousePressed()) {selected=i;pending=i;}
            }
        }
        canvas.Text("Select: arrows / stick    Play: Enter / A / trigger",38,610,18);
        canvas.Text("Return: F12 / Back + Start / both VR stick clicks",38,639,17);
        canvas.Text("R: recenter panel    Esc / B: quit from this list",38,666,16,0x84acc7ff);
        if(error!=null) {canvas.Rect(22,96,916,28,0x762c35ff);canvas.Text(error.Length>95?error[..95]:error,28,98,16);}
        canvas.End(); canvas.Present(screen,Canvas.Width,Canvas.Height,xr);
    }
    static void Transition()
    {
        if(pending==-2) return;
        int next=pending; pending=-2;
        CloseGame(); Session.Reset(); canvas=new Canvas();
        if(next<0)
        {
            if(smokeFrames>=0)
            {
                if(error!=null) {smokeFailed=true;Console.Error.WriteLine("ABA_SMOKE_FAILED "+error);Lub.Quit();return;}
                if(++smokeIndex<games.Length) pending=smokeIndex;
                else {Console.WriteLine("ABA_SMOKE_ALL_PASSED");Lub.Quit();}
            }
            return;
        }
        try
        {
            error=null; selected=next;
            game=new GameSession(games[next],root);
            Directory.SetCurrentDirectory(game.DirectoryPath);
            Session.Configuring=opts=>{gameWidth=opts.Width??640;gameHeight=opts.Height??480;};
            Session.Quitting=()=>pending=-1;
            if(!game.Info.Vr)
            {
                bridge=new HostBridge(game.Info.Id,game.DirectoryPath);
                bridge.Error+=message=>{error=message;pending=-1;};
                bridge.Install();
            }
            game.Initialize(); smokeTicks=0;releaseGate.Reset();
            Console.WriteLine("ABA_GAME_STARTED "+game.Info.Id);
        }
        catch(Exception ex)
        {
            error=ex.GetBaseException().Message;Console.Error.WriteLine(ex);pending=-1;
        }
    }
    static void CloseGame()
    {
        try {game?.Dispose();}
        catch(Exception ex) {error??=ex.GetBaseException().Message;Console.Error.WriteLine(ex);}
        finally
        {
            if(game!=null) {unloaded.Add(game.Unloaded);Console.WriteLine("ABA_GAME_STOPPED "+game.Info.Id);}
            game=null;
            try {bridge?.Dispose();}
            finally {bridge=null;Session.ClearHooks();Directory.SetCurrentDirectory(root);}
            releaseGate.Reset();gameWidth=640;gameHeight=480;
        }
    }
    static void Quit() {CloseGame();Session.AfterFrame=null;}
}
}
