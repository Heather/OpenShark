namespace OpenShark
/// __________________________________________________________________________________________________________
open System
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
/// __________________________________________________________________________________________________________
[<AutoOpen>]
module WPFParse =
    let parseColor o =
                match o with
                | Red -> "Red"
                | Green -> "Green"
                | Blue -> "Blue"
    /// ______________________________________________________________________________________________________
    let parseOrientation o =
                match o with
                | Horizontal -> "Horizontal"
                | Vertical -> "Vertical"
    /// ______________________________________________________________________________________________________
    let parseAttribute attr =
                match attr with
                | Width x -> sprintf @"Width = ""%i""" x
                | Height x -> sprintf @"Height = ""%i""" x
    /// ______________________________________________________________________________________________________
    let parseAttributes =
                List.fold (fun acc x -> sprintf "%s %s" acc (parseAttribute x)) ""
    /// ______________________________________________________________________________________________________
    let rec parseBinding (b:Expr) =
                match b with
                | Lambda (_, e) -> parseBinding e
                | PropertyGet (_, info, _) -> sprintf @"{Binding Path=%s}" (info.Name)
    /// ______________________________________________________________________________________________________
    let rec parseDataTemplate (DataTemplate x) =
                sprintf @"<DataTemplate>%s</DataTemplate>" 
                        (parseFrameworkElement x)                               
    /// ______________________________________________________________________________________________________
    and parseFrameworkElement x =
                match x with
                | Label (s, attrs) -> sprintf @"<Label Content=""%s"" %s/>" s
                                              (parseAttributes attrs)
                | Button s -> sprintf @"<Button Content=""%s""/>" s
                | TextBox (attrs, b) -> sprintf @"<TextBox %s Text=""%s""/>"
                                                (parseAttributes attrs)
                                                (parseBinding b)
                | StackPanel (xs, attrs, orient) ->
                         let (+) x y = sprintf "%s\n%s" x y
                         sprintf @"<StackPanel Orientation = ""%s"" %s>%s
                                   </StackPanel>"                                                                             
                                 (parseOrientation orient)
                                 (parseAttributes attrs)
                                 (List.fold (fun acc x -> acc + (parseFrameworkElement x)) "" xs)
                | Border (c, x) -> sprintf @"<Border BorderBrush=""%s""
                                                     BorderThickness=""2"">%s</Border>"
                                           (parseColor c)
                                           (parseFrameworkElement x)
                | ItemsControl t -> 
                         sprintf @"<ItemsControl ItemsSource=""{Binding .}""> 
                                   <ItemsControl.ItemTemplate>%s</ItemsControl.ItemTemplate>
                                   </ItemsControl>" (parseDataTemplate t)
                | WebControl args ->
                            sprintf @"<awe:WebControl 
                                        Name=""webControl"" 
                                        WebSession=""{Binding Source={StaticResource mySession}}"" 
                                        Source=""%s"" />" args
    /// ______________________________________________________________________________________________________
    let parseWindow (Window (c, attrs)) =
                sprintf @"<Window 
                        xmlns:core=""clr-namespace:Awesomium.Core;assembly=Awesomium.Core""
                        xmlns:data=""http://schemas.awesomium.com/winfx/data""
                        xmlns:awe=""http://schemas.awesomium.com/winfx""
                        WindowStyle=""None""
                        %s>%s</Window>"
                        (parseAttributes attrs)
                        (parseFrameworkElement c)
    /// ______________________________________________________________________________________________________
    let parseApplication c =
            sprintf @"<Application
                       xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                       xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                       <Application.MainWindow>%s</Application.MainWindow></Application>"
                    (parseWindow c)
/// __________________________________________________________________________________________________________