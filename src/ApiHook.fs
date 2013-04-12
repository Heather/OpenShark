namespace OpenShark
/// __________________________________________________________________________________________________________
open System; open System.Text; open System.Windows.Forms
/// __________________________________________________________________________________________________________
[<AutoOpen>]
module ApiHook =
    /// ______________________________________________________________________________________________________
    open System.Runtime.InteropServices
    /// ______________________________________________________________________________________________________
    [<AutoOpen>]
    module User32 =
        [<DllImport(@"User32", CharSet = CharSet.Ansi, SetLastError = false, ExactSpelling = true)>]
        extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [<DllImport(@"User32", CharSet = CharSet.Ansi, SetLastError = false, ExactSpelling = true)>]
        extern bool UnregisterHotKey(IntPtr hWnd, int id)
    [<AutoOpen>]
    module Kernel32 =
        [<DllImport(@"kernel32")>]
        extern int64 WritePrivateProfileString(string section, string key, string v, string filePath);
        [<DllImport("kernel32")>]
        extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
    /// ______________________________________________________________________________________________________
    type public IniFile(iNIPath : string) =
        let mutable path = iNIPath
        member X.Path
            with get() = path
            and set v = path <- v
        member X.IniWriteValue section key value =
            WritePrivateProfileString(section, key, value, X.Path)
        member X.IniReadValue section key =
            let temp = new StringBuilder(255)
            let i = GetPrivateProfileString(section, key, "", temp, 255, X.Path)
            temp.ToString()
    /// ______________________________________________________________________________________________________
    type public KeyboardHook() as X =
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
/// __________________________________________________________________________________________________________