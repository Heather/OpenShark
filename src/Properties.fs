namespace OpenShark
/// __________________________________________________________________________________________________________
open System; open System.Drawing; open System.Windows.Forms
/// __________________________________________________________________________________________________________
/// NOTE: I know that serialization will be better but so far I'm just lazy weirdo.
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

    let Load(ini : IniFile) =
        let (=->) s v = if ini.IniReadValue s v = "1" then true else false
        let (=|>) s v = ini.IniReadValue s v
        let (==>) s v = 
            let tryref  = ref 0
            let str     = ini.IniReadValue s v
            if Int32.TryParse(str, tryref) then !tryref else int Keys.None

        trayMinimize        <- "General" =-> "trayMinimize"
        trackWindowPosition <- "General" =-> "trackWindowPosition"
        startMinimized      <- "General" =-> "startMinimized"

        WindowPosition <-   match "Window" =|> "WindowPosition" with
                            | "Empty"       -> Rectangle.Empty
                            //TODO:         -> Process Other
                            | _             -> Rectangle.Empty
        WindowState <-      match "Window" =|> "WindowState" with
                            | "Normal"      -> FormWindowState.Normal
                            | "Maximized"   -> FormWindowState.Maximized
                            | "Minimized"   -> FormWindowState.Minimized
                            | _             -> FormWindowState.Normal

        hotkeyPlay          <- "HotKeys" ==> "hotkeyPlay"
        hotkeyNext          <- "HotKeys" ==> "hotkeyNext"
        hotkeyPrevious      <- "HotKeys" ==> "hotkeyPrevious"
        hotkeyLike          <- "HotKeys" ==> "hotkeyLike"
        hotkeyDislike       <- "HotKeys" ==> "hotkeyDislike"
        hotkeyFavorite      <- "HotKeys" ==> "hotkeyFavorite"
        hotkeyShowHide      <- "HotKeys" ==> "hotkeyShowHide"
        hotkeyMute          <- "HotKeys" ==> "hotkeyMute"
        hotkeyShuffle       <- "HotKeys" ==> "hotkeyShuffle"

    let Save(ini : IniFile) =
        let (=->) s v p = 
            ini.IniWriteValue s v 
                <|  match p with    | false -> "0"
                                    | true  -> "1"
                |> ignore
        let (==>) s v p = ini.IniWriteValue s v <| p.ToString() |> ignore

        "General" =-> "trayMinimize"        <| trayMinimize
        "General" =-> "trackWindowPosition" <| trackWindowPosition
        "General" =-> "startMinimized"      <| startMinimized

        "Window" ==> "WindowPosition"   
            <|  match WindowPosition with
                | w when w = Rectangle.Empty -> "Empty"
                | _ -> "Empty"

        "Window" ==> "WindowState"   
            <|  match WindowState with
                | w when w = FormWindowState.Normal     -> "Normal"
                | w when w = FormWindowState.Maximized  -> "Maximized"
                | w when w = FormWindowState.Minimized  -> "Minimized"
                | _ -> "Normal"

        "HotKeys" ==> "hotkeyPlay"      <| hotkeyPlay
        "HotKeys" ==> "hotkeyNext"      <| hotkeyNext
        "HotKeys" ==> "hotkeyPrevious"  <| hotkeyPrevious
        "HotKeys" ==> "hotkeyLike"      <| hotkeyLike
        "HotKeys" ==> "hotkeyDislike"   <| hotkeyDislike
        "HotKeys" ==> "hotkeyFavorite"  <| hotkeyFavorite
        "HotKeys" ==> "hotkeyShowHide"  <| hotkeyShowHide
        "HotKeys" ==> "hotkeyMute"      <| hotkeyMute
        "HotKeys" ==> "hotkeyShuffle"   <| hotkeyShuffle
/// __________________________________________________________________________________________________________