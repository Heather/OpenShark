﻿namespace OpenShark

open System;            open System.Text
open System.Drawing;    open System.Windows.Forms
open System.IO;         open System.ComponentModel

open MetroFramework
open MetroFramework.Forms
open MetroFramework.Controls

[<AutoOpen>]
module Core =
    let project = "OpenShark v.0.1.6"
    let hook = new KeyboardHook()

type main() as f =
    inherit MetroForm()

    let w           = new WebBrowser()
    let t           = new WebBrowser()
    let components  = new Container()

    let tabControl  = new MetroTabControl()
    let wpage       = new MetroTabPage()
    let opage       = new MetroTabPage()

    let notifyIcon = new NotifyIcon(components)
    let contextMenuStrip = new ContextMenuStrip(components)

    let current     = new ToolStripMenuItem()
    let about       = new ToolStripMenuItem()
    let emo         = new ToolStripMenuItem()
    let exit        = new ToolStripMenuItem()
    let previous    = new ToolStripMenuItem()
    let next        = new ToolStripMenuItem()
    let play        = new ToolStripMenuItem()
    let favorite    = new ToolStripMenuItem()
    let like        = new ToolStripMenuItem()
    let dislike     = new ToolStripMenuItem()

    let mutable windowInitialized = false

    let showHideWindow() =
        if f.WindowState = FormWindowState.Minimized then
                f.Show(); f.WindowState <- FormWindowState.Normal
        else    f.WindowState <- FormWindowState.Minimized
                if Properties.trayMinimize then f.Hide()
    let isVisibleOnAnyScreen(rect : Rectangle) =
        if (Screen.AllScreens
            |> Seq.filter(fun s -> s.WorkingArea.IntersectsWith(rect))
            |> Seq.length) > 0 then false else true
    let track() =
        if (f.WindowState = FormWindowState.Normal) then
            Properties.WindowPosition <- f.DesktopBounds

    let playerExecute action = w.Document.GetElementById(action).InvokeMember("click") |> ignore
    let htmlClickOn selector =
        if w.ReadyState = WebBrowserReadyState.Complete then
            w.Document.InvokeScript("clickElement", [|(selector : obj)|]) |> ignore

    let fn = Path.GetDirectoryName(Application.ExecutablePath) + "\\OpenShark.ini"
    let ini = new IniFile(fn)
    do  if File.Exists fn then Properties.Load ini (*  Load config  *)
        else Properties.Save ini                   (* Create config *)
        if (Properties.WindowPosition <> Rectangle.Empty) && isVisibleOnAnyScreen(Properties.WindowPosition) then
            f.StartPosition     <- FormStartPosition.Manual
            f.DesktopBounds     <- Properties.WindowPosition
            f.WindowState       <- Properties.WindowState
        else f.StartPosition    <- FormStartPosition.WindowsDefaultLocation
        f.InitializeForm

    member f.InitializeForm =
        f.Width   <- 1100
        f.Height  <- 650
        f.Text    <- project

        notifyIcon.ContextMenuStrip <- contextMenuStrip
        notifyIcon.Text             <- "OpenShark"
        notifyIcon.Icon             <- IconResource.OpenSharkIcon.Icon
        notifyIcon.Visible          <- true
        notifyIcon.MouseDoubleClick.Add <| fun _ ->
            f.Show(); f.WindowState <- FormWindowState.Normal

        contextMenuStrip.Items.AddRange(
            [|  current                 (* payer controls *)
                previous; next; play
                emo                     (* emotions *)
                favorite; like; dislike
                about; exit
            |]  |> Array.map(fun t -> t :> ToolStripItem))

        current.Text    <- "Current:";  current.Enabled     <- false
        previous.Text   <- "Previous";  previous.Click.Add  <| fun _ -> playerExecute "play-prev"
        next.Text       <- "Next";      next.Click.Add      <| fun _ -> playerExecute "play-next"
        play.Text       <- "Play/Pause";play.Click.Add      <| fun _ -> playerExecute "play-pause"
        play.Text       <- "Play/Pause";play.Click.Add      <| fun _ -> playerExecute "play-pause"
        favorite.Text   <- "Favorite";  favorite.Click.Add  <| fun _ -> htmlClickOn "#np-fav"
        like.Text       <- "Like";      like.Click.Add      <| fun _ -> htmlClickOn "#player-wrapper .queue-item-active .smile"
        dislike.Text    <- "Dislike";   dislike.Click.Add   <| fun _ -> htmlClickOn "##player-wrapper .queue-item-active .frown"
        emo.Text        <- "Emotions:"; emo.Enabled         <- false
        exit.Text       <- "Exit";      exit.Click.Add      <| fun _ -> f.Close()
        about.Text      <- "About";     about.Click.Add     <| fun _ ->
            MessageBox.Show <| String.Format("{0}, Engine version: {1}", project, w.Version) |> ignore

        f.Load.Add <| fun _ ->
            hook.KeyPressed.Add <| fun (_, e) ->
                match e.Key.ToString() with
                | "MediaPlayPause"      -> playerExecute "play-pause"
                | "MediaNextTrack"      -> playerExecute "play-next"
                | "MediaPreviousTrack"  -> playerExecute "play-prev"
                | _ -> ()
                let KeyAsInt = int (e.Key ||| hook.keyToModifierKey(e.Modifier))
                match KeyAsInt with
                    | k when k = Properties.hotkeyPlay      -> htmlClickOn "#play-pause"
                    | k when k = Properties.hotkeyNext      -> htmlClickOn "#play-next"
                    | k when k = Properties.hotkeyPrevious  -> htmlClickOn "#play-prev"
                    | k when k = Properties.hotkeyLike      -> htmlClickOn "#player-wrapper .queue-item-active .smile"
                    | k when k = Properties.hotkeyDislike   -> htmlClickOn "##player-wrapper .queue-item-active .frown"
                    | k when k = Properties.hotkeyFavorite  -> htmlClickOn "#np-fav"
                    | k when k = Properties.hotkeyMute      -> htmlClickOn "#volume"
                    | k when k = Properties.hotkeyShuffle   -> htmlClickOn "#shuffle"
                    | k when k = Properties.hotkeyShowHide  -> showHideWindow()
                    | _ -> ()
            w.ObjectForScripting <- f
            if Properties.startMinimized then showHideWindow()

        f.Activated.Add <| fun _ ->
            hook.unregisterAllHotkeys()
            try [   VK_MEDIA_PLAY_PAUSE
                    VK_MEDIA_NEXT_TRACK
                    VK_MEDIA_PREV_TRACK
                ] |> Seq.iter(fun k -> hook.RegisterHotKey(ModifierKeys.None, enum<Keys> k))
                [   Properties.hotkeyPlay
                    Properties.hotkeyNext
                    Properties.hotkeyPrevious
                    Properties.hotkeyLike
                    Properties.hotkeyDislike
                    Properties.hotkeyFavorite
                    Properties.hotkeyShowHide
                    Properties.hotkeyMute
                    Properties.hotkeyShuffle 
                ] |> Seq.iter(fun k ->
                    hook.RegisterHotKey(
                        hook.Win32ModifiersFromKeys(enum<Keys> k)
                        , hook.getKeyWithoutModifier(enum<Keys> k)))
            with | :? InvalidOperationException -> ()

        f.Resize.Add <| fun _ ->
            if (Properties.trayMinimize) then 
                if (f.WindowState = FormWindowState.Minimized) then f.Hide()

        f.Icon              <- IconResource.OpenSharkIcon.Icon
        f.CausesValidation  <- false;

        tabControl.Dock <- DockStyle.Fill

        wpage.Text <- "   Grooveshark     "
        opage.Text <- "    " + Properties.customUrl + "       "

        f.SizeGripStyle <- SizeGripStyle.Hide
        f.Padding       <- new Padding(4)

        wpage.Controls.Add w
        opage.Controls.Add t

        tabControl.Controls.Add wpage
        tabControl.Controls.Add opage
        tabControl.SelectedIndex <- 1

        f.Controls.Add tabControl

        tabControl.FontSize         <- MetroTabControlSize.Tall
        w.ScriptErrorsSuppressed    <- true
        w.Url                       <- new Uri("https://grooveshark.com")

        t.ScriptErrorsSuppressed    <- true
        t.Url                       <- new Uri(Properties.customUrl)

        windowInitialized <- true

        wpage.Validated.Add <| fun _ -> w.Dock <- DockStyle.Fill
        opage.Validated.Add <| fun _ -> t.Dock <- DockStyle.Fill
        wpage.Invalidate()
        opage.Invalidate()

    override f.OnLoad(e : EventArgs) =
        base.OnLoad(e);
        tabControl.SelectTab 0
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

[<AutoOpen>]
module Main = [<STAThread>] do 
    let mutable ok = ref true
    let m = new System.Threading.Mutex(true, "OpenShark", ok)
    if !ok then Application.Run(new main());   
    else MessageBox.Show("OpenShark is already running.") |> ignore
    GC.KeepAlive(m)
