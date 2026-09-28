Directory.SetCurrentDirectory(AppContext.BaseDirectory);
return Lub.Run(FrameApp.OnInit, null, FrameApp.OnFrame, FrameApp.OnQuit, args);
