namespace OpenShark
/// __________________________________________________________________________________________________________
open System; open System.Windows.Forms
/// __________________________________________________________________________________________________________
[<AutoOpen>]
module Hotkeys =
    /// ______________________________________________________________________________________________________
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
    /// ______________________________________________________________________________________________________
    type KeyPressedEventArgs(modifier : ModifierKeys, key : Keys) =
        inherit System.EventArgs()
        let _modifier = modifier
        let _key = key

        member X.Modifier   with get() = _modifier
        member X.Key        with get() = _key
/// __________________________________________________________________________________________________________