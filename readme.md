OpenShark 
=========

 - light WinGrooves re-code on F#
 - Run rc.bat to generate res file for icon

FEATURES (Most of features are just WinGrooves but I list them anyways) :

 - Open GrooveShark in a separate window instead of a web browser
 - Minimize WinGrooves to the tray
 - Tray icon buttons [ Next Previous Play / Stop Like Dislike ]
 - Control WinGrooves using the media keys on your keyboard or using global customizable Hotkeys while in any application

TODO:

 - All the properties to xml or ini file (maybe conf for mono)
 - Options window to config all the stuff

![Screenshot](Resources/OpenShark.ico)
```fsharp
member f.InitializeForm =
    w.Dock                      <- DockStyle.Fill
    w.ScriptErrorsSuppressed    <- true
    w.Url                       <- new Uri("http://listen.grooveshark.com")e
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
