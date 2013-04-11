```fsharp
member f.InitializeForm =
    f.Width   <- 800
    f.Height  <- 750
    f.Text    <- project
    w.Dock                      <- DockStyle.Fill
    w.ScriptErrorsSuppressed    <- true
    w.Url                       <- new Uri("http://listen.grooveshark.com")
    w.DocumentCompleted.Add <| fun _ -> ()
    notifyIcon.ContextMenuStrip <- contextMenuStrip
    notifyIcon.Text             <- "OpenShark"
    notifyIcon.Icon             <- IconResource.OpenShark.Icon
    notifyIcon.Visible          <- true
    notifyIcon.MouseDoubleClick.Add <| fun _ ->
        f.Show(); f.WindowState <- FormWindowState.Normal
    contextMenuStrip.Items.AddRange(
        [|  about
            exit
        |]  |> Array.map(fun t -> t :> ToolStripItem))
    about.Text  <- "About"; about.Click.Add <| fun _ -> MessageBox.Show(project) |> ignore
    exit.Text   <- "Exit";  exit.Click.Add  <| fun _ -> f.Close()
    f.Load.Add <| fun _ ->
        hook.KeyPressed.Add <| fun (_, e) ->
            match e.Key.ToString() with
            | "MediaPlayPause"      -> playerExecute "play-pause"
            | "MediaNextTrack"      -> playerExecute "play-next"
            | "MediaPreviousTrack"  -> playerExecute "play-prev"
            | _ -> ()
            let KeyAsInt = int (e.Key ||| hook.keyToModifierKey(e.Modifier))
            if (KeyAsInt = Properties.hotkeyPlay) then
                htmlClickOn "#play-pause"
        w.ObjectForScripting <- f
        if Properties.startMinimized then showHideWindow()
    f.Activated.Add <| fun _ ->
        hook.unregisterAllHotkeys()
        try hook.RegisterHotKey(ModifierKeys.None, enum<Keys> VK_MEDIA_PLAY_PAUSE)
            hook.RegisterHotKey(ModifierKeys.None, enum<Keys> VK_MEDIA_NEXT_TRACK)
            hook.RegisterHotKey(ModifierKeys.None, enum<Keys> VK_MEDIA_PREV_TRACK)
            hook.RegisterHotKey(
                hook.Win32ModifiersFromKeys(enum<Keys> Properties.hotkeyPlay)
              , hook.getKeyWithoutModifier(enum<Keys> Properties.hotkeyPlay))
        with | :? InvalidOperationException -> ()
    f.Resize.Add <| fun _ ->
        if (Properties.trayMinimize) then 
            if (f.WindowState = FormWindowState.Minimized) then f.Hide()
```
