namespace OpenShark
/// __________________________________________________________________________________________________________
open System
/// __________________________________________________________________________________________________________
[<AutoOpen>]
module WPFHelp =
    /// ______________________________________________________________________________________________________
    let width = Width

    let height = Height

    let stackpanel attrs orient xs = StackPanel (xs, attrs, orient)

    let border c x = Border(c, x)

    let label attrs s = Label(s, attrs)

    let button = Button

    let textbox attrs b = TextBox(attrs, b)

    let itemscontrol = ItemsControl

    let datatemplate = DataTemplate

    let application = Application

    let window attrs c = Window (c, attrs)
/// __________________________________________________________________________________________________________