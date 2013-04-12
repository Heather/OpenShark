namespace OpenShark
/// __________________________________________________________________________________________________________
open System;            open System.Text
open System.Drawing;    open System.Windows.Forms
open System.IO;         open System.ComponentModel
/// __________________________________________________________________________________________________________
[<AutoOpen>]
module Core =
    let project = "OpenShark v.0.0.7"
    #if __MonoCS__
    //No Win32 Keyboard hook for mono
    #else
    let hook = new KeyboardHook()
    #endif
type main() as f = 
    inherit Form()
    /// ______________________________________________________________________________________________________
    let w = new WebBrowser()
    /// ______________________________________________________________________________________________________
    let components = new Container()
    let notifyIcon = new NotifyIcon(components)
    let contextMenuStrip = new ContextMenuStrip(components)
    /// ______________________________________________________________________________________________________
    let current     = new ToolStripMenuItem()
    let about       = new ToolStripMenuItem()
    let options     = new ToolStripMenuItem()
    let exit        = new ToolStripMenuItem()
    let previous    = new ToolStripMenuItem()
    let next        = new ToolStripMenuItem()
    let play        = new ToolStripMenuItem()
    let like        = new ToolStripMenuItem()
    let dislike     = new ToolStripMenuItem()
    /// ______________________________________________________________________________________________________
    let mutable windowInitialized = false
    /// ______________________________________________________________________________________________________
    let showHideWindow() =
        if f.WindowState = FormWindowState.Minimized then
            f.Show(); f.WindowState <- FormWindowState.Normal;
        else
            f.WindowState <- FormWindowState.Minimized
            if Properties.trayMinimize then f.Hide()
    let isVisibleOnAnyScreen(rect : Rectangle) =
        if (Screen.AllScreens
            |> Seq.filter(fun s -> s.WorkingArea.IntersectsWith(rect))
            |> Seq.length) > 0 then false else true
    let track() =
        if (f.WindowState = FormWindowState.Normal) then
            Properties.WindowPosition <- f.DesktopBounds
    /// ______________________________________________________________________________________________________
    let playerExecute action = w.Document.GetElementById(action).InvokeMember("click") |> ignore
    let htmlClickOn selector =
        if w.ReadyState = WebBrowserReadyState.Complete then
            w.Document.InvokeScript("clickElement", [|(selector : obj)|]) |> ignore
    /// ______________________________________________________________________________________________________
    #if __MonoCS__
    //Load conf file
    #else
    let fn = Path.GetDirectoryName(Application.ExecutablePath) + "\\OpenShark.ini"
    let ini = new IniFile(fn)
    #endif
    do  // Read properties & Init Form
        #if __MonoCS__
        //Read conf file
        #else
        if File.Exists fn then
            if ini.IniReadValue "General" "trayMinimize" = "1" then
                Properties.trayMinimize <- true
        else Properties.Save ini (* Create Ini file *)
        #endif
        if (Properties.WindowPosition <> Rectangle.Empty) && isVisibleOnAnyScreen(Properties.WindowPosition) then
            f.StartPosition     <- FormStartPosition.Manual
            f.DesktopBounds     <- Properties.WindowPosition
            f.WindowState       <- Properties.WindowState
        else f.StartPosition    <- FormStartPosition.WindowsDefaultLocation
        f.InitializeForm
    /// ______________________________________________________________________________________________________
    member f.InitializeForm =
        f.Width   <- 1200
        f.Height  <- 650
        f.Text    <- project
        //  Web Browser
        w.Dock                      <- DockStyle.Fill
        w.ScriptErrorsSuppressed    <- true
        w.Url                       <- new Uri("http://listen.grooveshark.com")
        w.DocumentCompleted.Add <| fun _ -> ()
        //  Notify Icon
        notifyIcon.ContextMenuStrip <- contextMenuStrip
        notifyIcon.Text             <- "OpenShark"
        notifyIcon.Icon             <- IconResource.OpenSharkIcon.Icon
        notifyIcon.Visible          <- true
        notifyIcon.MouseDoubleClick.Add <| fun _ ->
            f.Show(); f.WindowState <- FormWindowState.Normal
        //  Context menu
        contextMenuStrip.Items.AddRange(
            [|  current
                previous
                next
                play
                like
                dislike
                about
                options
                exit
            |]  |> Array.map(fun t -> t :> ToolStripItem))
        current.Text    <- "Current:" // TODO: Song info
        current.Enabled <- false
        about.Text      <- project
        about.Enabled   <- false
        options.Text    <- "Options";   options.Click.Add   <| fun _ -> MessageBox.Show "Not supported yet" |> ignore
        exit.Text       <- "Exit";      exit.Click.Add      <| fun _ -> f.Close()
        previous.Text   <- "Previous";  previous.Click.Add  <| fun _ -> playerExecute "play-prev"
        next.Text       <- "Next";      next.Click.Add      <| fun _ -> playerExecute "play-next"
        play.Text       <- "Play/Pause";play.Click.Add      <| fun _ -> playerExecute "play-pause"
        play.Text       <- "Play/Pause";play.Click.Add      <| fun _ -> playerExecute "play-pause"
        like.Text       <- "Like";      like.Click.Add      <| fun _ -> htmlClickOn "#player-wrapper .queue-item-active .smile"
        dislike.Text    <- "Dislike";   dislike.Click.Add   <| fun _ -> htmlClickOn "##player-wrapper .queue-item-active .frown"
        /// ______________________________________________________________________________________________________
        f.Load.Add <| fun _ ->
            #if __MonoCS__
                //No Win32 Keyboard hook for mono
            #else
            hook.KeyPressed.Add <| fun (_, e) ->
                match e.Key.ToString() with
                | "MediaPlayPause"      -> playerExecute "play-pause"
                | "MediaNextTrack"      -> playerExecute "play-next"
                | "MediaPreviousTrack"  -> playerExecute "play-prev"
                | _ -> ()
                let KeyAsInt = int (e.Key ||| hook.keyToModifierKey(e.Modifier))
                if (KeyAsInt = Properties.hotkeyPlay) then
                    htmlClickOn "#play-pause"
            #endif
            w.ObjectForScripting <- f
            if Properties.startMinimized then showHideWindow()
        #if __MonoCS__
            //No Win32 Keyboard hook for mono
        #else
        f.Activated.Add <| fun _ ->
            hook.unregisterAllHotkeys()
            try hook.RegisterHotKey(ModifierKeys.None, enum<Keys> VK_MEDIA_PLAY_PAUSE)
                hook.RegisterHotKey(ModifierKeys.None, enum<Keys> VK_MEDIA_NEXT_TRACK)
                hook.RegisterHotKey(ModifierKeys.None, enum<Keys> VK_MEDIA_PREV_TRACK)
                hook.RegisterHotKey(
                    hook.Win32ModifiersFromKeys(enum<Keys> Properties.hotkeyPlay)
                  , hook.getKeyWithoutModifier(enum<Keys> Properties.hotkeyPlay))
            with | :? InvalidOperationException -> ()
        #endif
        f.Resize.Add <| fun _ ->
            if (Properties.trayMinimize) then 
                if (f.WindowState = FormWindowState.Minimized) then f.Hide()
        /// ______________________________________________________________________________________________________
        f.Icon              <- IconResource.OpenSharkIcon.Icon
        f.CausesValidation  <- false;
        /// ______________________________________________________________________________________________________
        f.Controls.AddRange [|w|]; windowInitialized <- true
        /// ______________________________________________________________________________________________________
        contextMenuStrip.ResumeLayout(false)
        f.ResumeLayout(false)
        f.PerformLayout()
    /// __________________________________________________________________________________________________________
    override f.OnLoad(e : EventArgs) =
        base.OnLoad(e);
    override f.OnClosed(e : EventArgs) =
        base.OnClosed(e)
        match f.WindowState with
        | FormWindowState.Normal | FormWindowState.Maximized ->
            Properties.WindowState      <- f.WindowState
        | _ -> Properties.WindowState   <- FormWindowState.Normal
        Properties.Save ini
    override f.OnResize(e : EventArgs) =
        base.OnResize(e); if (windowInitialized && Properties.trackWindowPosition) then track()
    override f.OnMove(e : EventArgs) =
        base.OnMove(e); if (windowInitialized && Properties.trackWindowPosition) then track()
    override f.Dispose(disposing : bool) =
        if not disposing then if components <> null then components.Dispose()
        base.Dispose(disposing)
/// __________________________________________________________________________________________________________
[<AutoOpen>]
module Main = [<STAThread>] do 
    let mutable ok = ref true
    let m = new System.Threading.Mutex(true, "WinGrooves", ok)
    if !ok then Application.Run(new main());   
    else MessageBox.Show("OpenShark is already running.") |> ignore
    GC.KeepAlive(m)
/// __________________________________________________________________________________________________________