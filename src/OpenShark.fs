
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