namespace OpenShark
/// __________________________________________________________________________________________________________
open System;            open System.Text
open System.Drawing;    open System.Windows.Forms
open System.IO;         open System.ComponentModel
/// __________________________________________________________________________________________________________
[<AutoOpen>]
module Core =
    let project = "OpenShark v.2.0.0"
    let hook = new KeyboardHook()

    let SetupGlobalHotkeys() =
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