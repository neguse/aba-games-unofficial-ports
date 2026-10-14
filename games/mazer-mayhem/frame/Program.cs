Directory.SetCurrentDirectory(AppContext.BaseDirectory);
return Lub.App.Run(Game.OnInit, null, Game.OnFrame, Game.OnQuit, args);
