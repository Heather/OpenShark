OpenShark 2.0 (Beta)
====================

NOTE: For now I suggest to use 1.0 WinForms based version with some reasons: https://github.com/Heather/OpenShark/tree/1.0

 - Light WinGrooves re-code on WPF / Awesomium / F# / ... maybe some other messy stuff
 - For now there are a lot of things TODO to make it semi-good

FEATURES (Most of features are just WinGrooves but I list them anyways) :

 - Open GrooveShark in a separate window instead of a web browser
 - Minimize WinGrooves to the tray
 - Tray icon buttons [ Next Previous Play / Stop Like Dislike ]
 - Control WinGrooves using the media keys on your keyboard or using global customizable Hotkeys while in any application
 - other cool stuff maybe

TODO:

 - Repair C# -> js code
 - Clean up the code
 - I am lazy to change ini file with options window & xml

```fsharp
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
```
![Screenshot](Resources/OpenShark.ico)
