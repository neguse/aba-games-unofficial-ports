Directory.SetCurrentDirectory(AppContext.BaseDirectory);
return Lub.App.Run(Standalone.OnInit, null, Standalone.OnFrame, Standalone.OnQuit, args);
