namespace OpenShark
/// __________________________________________________________________________________________________________
open System
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
/// __________________________________________________________________________________________________________
[<AutoOpen>]
module WPFXaml =
    /// ______________________________________________________________________________________________________
    type Color = Red | Green | Blue

    type Orientation = Horizontal | Vertical

    type Attribute = Width of int
                   | Height of int

    type DataTemplate = DataTemplate of FrameworkElement

    and FrameworkElement
            = Label of string * Attribute list
            | Button of string
            | TextBox of Attribute list * Expr
            | StackPanel of FrameworkElement list * Attribute list * Orientation
            | Border of Color * FrameworkElement
            | ItemsControl of DataTemplate

    type Window = Window of FrameworkElement * Attribute list

    type Application = Application of Window
/// __________________________________________________________________________________________________________