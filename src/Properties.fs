namespace OpenShark
/// __________________________________________________________________________________________________________
open System; open System.Drawing; open System.Windows.Forms
/// __________________________________________________________________________________________________________
[<AutoOpen>]
module Properties =
/// __________________________________________________________________________________________________________
    //very Static properties:
    let mutable trayMinimize        = true
    let mutable trackWindowPosition = false
    let mutable startMinimized      = false
    //very Dynamic properties:
    let mutable WindowPosition  = Rectangle.Empty
    let mutable WindowState     = FormWindowState.Normal
    //Hotkeys:
    let mutable hotkeyPlay      = int Keys.None
    let mutable hotkeyNext      = int Keys.None
    let mutable hotkeyPrevious  = int Keys.None
    let mutable hotkeyLike      = int Keys.None
    let mutable hotkeyDislike   = int Keys.None
    let mutable hotkeyFavorite  = int Keys.None
    let mutable hotkeyShowHide  = int Keys.None
    let mutable hotkeyMute      = int Keys.None
    let mutable hotkeyShuffle   = int Keys.None
    let Save(ini : IniFile) =
        ini.IniWriteValue "General" "trayMinimize" "1" |> ignore
/// __________________________________________________________________________________________________________