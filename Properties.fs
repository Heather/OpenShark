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
    let mutable hotkeyPlay  = int Keys.None
    let Save(ini : IniFile) =
        ini.IniWriteValue "General" "trayMinimize" "1" |> ignore
/// __________________________________________________________________________________________________________