Directory.SetCurrentDirectory(AppContext.BaseDirectory);
return Lub.Run(Game.OnInit, null, Game.OnFrame, Game.OnQuit, args);
