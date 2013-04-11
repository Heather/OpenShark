#light (* exec fsharpi --exec $0 $@ *)
/// __________________________________________________________________________________________________________
open System;            open System.Text
open System.Drawing;    open System.Windows.Forms
open System.IO;         open System.ComponentModel
/// __________________________________________________________________________________________________________
#if __MonoCS__
let isLinux =
    int Environment.OSVersion.Platform |> fun p ->
        (p = 4) || (p = 6) || (p = 128)
#else
open System.Runtime.InteropServices
/// __________________________________________________________________________________________________________
[<AutoOpen>]
module Hotkeys =
    let WM_HOTKEY           = 0x0312;
    let VK_MEDIA_NEXT_TRACK = 0xB0;
    let VK_MEDIA_PREV_TRACK = 0xB1;
    let VK_MEDIA_STOP       = 0xB2;
    let VK_MEDIA_PLAY_PAUSE = 0xB3;
type ModifierKeys =
    | None      = 0
    | Alt       = 1
    | Control   = 2
    | Shift     = 4
    | Win       = 8
/// __________________________________________________________________________________________________________
type KeyPressedEventArgs(modifier, key) =
    inherit EventArgs()
    let _modifier = modifier
    let _key = key

    member X.Modifier   with get() = _modifier
    member X.Key        with get() = _key
/// __________________________________________________________________________________________________________
[<AutoOpen>]
module Win32 =
    [<DllImport(@"User32", CharSet = CharSet.Ansi, SetLastError = false, ExactSpelling = true)>]
    extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
    [<DllImport(@"User32", CharSet = CharSet.Ansi, SetLastError = false, ExactSpelling = true)>]
    extern bool UnregisterHotKey(IntPtr hWnd, int id)
/// __________________________________________________________________________________________________________
type KeyboardHook() as X =
    inherit NativeWindow()
    let mutable _currentId = 0
    let _keyPressed = Event< _ >()
    do  X.CreateHandle(new CreateParams())
    [<CLIEvent>]
    member X.KeyPressed = _keyPressed.Publish
    member X.RegisterHotKey(modifier : ModifierKeys, key : Keys) =
        _currentId <- _currentId + 1 
        if (not <| RegisterHotKey(X.Handle, _currentId, int modifier, int key)) then
            raise <| new InvalidOperationException(
                "Unable to register the hotkey " 
                + modifier.ToString() + " "
                + key.ToString() 
                + ". Please choose a different one (or find the key or program that has registered it already).");
    member X.unregisterAllHotkeys() =
            for i in [_currentId..0] do
                UnregisterHotKey(X.Handle, i) |> ignore
    member X.Win32ModifiersFromKeys(k : Keys) : ModifierKeys =
        let ModAlt          = 1uy
        let ModControl      = 2uy
        let ModShift        = 4uy
        let ModWin          = 8uy
        [   if int k &&& int Keys.Shift = int Keys.Shift then
                yield ModShift
            if int k &&& int Keys.Control = int Keys.Control then
                yield ModControl
            if int k &&& int Keys.Alt = int Keys.Alt then
                yield ModAlt
            if int k &&& int Keys.LWin = int Keys.LWin then
                yield ModWin
        ] |> Seq.sumBy(fun f -> int f)
          |> enum<ModifierKeys>
    member X.getModifierKey(k : Keys) = 
        [   if int k &&& int Keys.Shift = int Keys.Shift then
                yield Keys.Shift
            if int k &&& int Keys.Control = int Keys.Control then
                yield Keys.Control
            if int k &&& int Keys.Alt = int Keys.Alt then
                yield Keys.Alt
            if int k &&& int Keys.LWin = int Keys.LWin then
                yield Keys.LWin
        ]   |> Seq.sumBy(fun f -> int f)
            |> enum<Keys>
    member X.keyToModifierKey(k : ModifierKeys) =
        [   if int k &&& int ModifierKeys.Shift = int ModifierKeys.Shift then
                yield Keys.Shift
            if int k &&& int ModifierKeys.Control = int ModifierKeys.Control then
                yield Keys.Control
            if int k &&& int ModifierKeys.Alt = int ModifierKeys.Alt then
                yield Keys.Alt
            if int k &&& int ModifierKeys.Win = int ModifierKeys.Win then
                yield Keys.LWin
        ]   |> Seq.sumBy(fun f -> int f)
            |> enum<Keys>
    member X.getKeyWithoutModifier(k : Keys) = enum<Keys> (int k &&& int Keys.KeyCode)
    override X.WndProc(msg : Message byref) =
        base.WndProc(&msg)
        if msg.Msg = WM_HOTKEY then
            _keyPressed.Trigger(X, 
                new KeyPressedEventArgs(
                    enum<ModifierKeys> (msg.LParam.ToInt32() &&& 0xFFFF)
                ,   enum<Keys> ((msg.LParam.ToInt32() >>> 16) &&& 0xFFFF)))
    interface IDisposable with
        member D.Dispose() = 
            X.unregisterAllHotkeys()
            X.DestroyHandle()
#endif
/// __________________________________________________________________________________________________________
type IconResource (name:string) =
    let loadIconResource name =
        let pths = [
            "../../";
            "../"
            ]
        let rec tryLoad(x) =
            let p = 
                if x = -1 then  name                        
                else pths.[x] + name
            if File.Exists p then
                new System.Drawing.Icon(p : string)
            else 
                if x < pths.Length  then tryLoad(x + 1)
                else null
        tryLoad -1

    let image = loadIconResource name

    member __.Name = name
    member __.Icon = image

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>] 
module IconResource =
    let OpenShark = IconResource "OpenShark.ico"
/// __________________________________________________________________________________________________________
#if __MonoCS__
    // TODO: conf file
#else
module kernel =
    [<DllImport(@"kernel32")>]
    extern int64 WritePrivateProfileString(string section, string key, string v, string filePath);
    [<DllImport("kernel32")>]
    extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
/// __________________________________________________________________________________________________________
type public IniFile(iNIPath : string) =
    let mutable path = iNIPath
    member X.Path
        with get() = path
        and set v = path <- v
    member X.IniWriteValue section key value =
        kernel.WritePrivateProfileString(section, key, value, X.Path)
    member X.IniReadValue section key =
        let temp = new StringBuilder(255)
        let i = kernel.GetPrivateProfileString(section, key, "", temp, 255, X.Path)
        temp.ToString()
#endif
/// __________________________________________________________________________________________________________
module Properties =
    //very Static properties:
    let mutable trayMinimize        = true
    let mutable trackWindowPosition = false
    let mutable startMinimized      = false
    //very Dynamic properties:
    let mutable WindowPosition  = Rectangle.Empty
    let mutable WindowState     = FormWindowState.Normal
    //Hotkeys:
    let mutable hotkeyPlay  = int Keys.None
    let Save(ini : IniFile) =
        ini.IniWriteValue "General" "trayMinimize" "1" |> ignore
/// __________________________________________________________________________________________________________
let project = "OpenShark v.0.0.2"
let hook = new KeyboardHook()
type main() as f = 
    inherit Form()
    /// ______________________________________________________________________________________________________
    let w = new WebBrowser()
    /// ______________________________________________________________________________________________________
    let components = new Container()
    let notifyIcon = new NotifyIcon(components)
    let contextMenuStrip = new ContextMenuStrip(components)
    /// ______________________________________________________________________________________________________
    let about   = new ToolStripMenuItem();
    let exit    = new ToolStripMenuItem();
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
        f.Width   <- 800
        f.Height  <- 750
        f.Text    <- project
        //  Web Browser
        w.Dock                      <- DockStyle.Fill
        w.ScriptErrorsSuppressed    <- true
        w.Url                       <- new Uri("http://listen.grooveshark.com")
        w.DocumentCompleted.Add <| fun _ -> ()
        //  Notify Icon
        notifyIcon.ContextMenuStrip <- contextMenuStrip
        notifyIcon.Text             <- "OpenShark"
        notifyIcon.Icon             <- IconResource.OpenShark.Icon
        notifyIcon.Visible          <- true
        notifyIcon.MouseDoubleClick.Add <| fun _ ->
            f.Show(); f.WindowState <- FormWindowState.Normal
        //  Context menu
        contextMenuStrip.Items.AddRange(
            [|  about
                exit
            |]  |> Array.map(fun t -> t :> ToolStripItem))
        about.Text  <- "About"; about.Click.Add <| fun _ -> MessageBox.Show(project) |> ignore
        exit.Text   <- "Exit";  exit.Click.Add  <| fun _ -> f.Close()
        /// ______________________________________________________________________________________________________
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
        /// ______________________________________________________________________________________________________
        f.Icon              <- IconResource.OpenShark.Icon
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
[<STAThread>] do 
    let mutable ok = ref true
    let m = new System.Threading.Mutex(true, "WinGrooves", ok)
    if !ok then Application.Run(new main());   
    else MessageBox.Show("OpenShark is already running.") |> ignore
    GC.KeepAlive(m)
/// __________________________________________________________________________________________________________