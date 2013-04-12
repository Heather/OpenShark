namespace OpenShark
/// __________________________________________________________________________________________________________
open System.Windows
open System.Windows.Controls
open System.Windows.Markup
/// __________________________________________________________________________________________________________
[<AutoOpen>]
module WPFRun =
    /// ______________________________________________________________________________________________________
    let run app data =  app |> parseApplication                    
                            |> XamlReader.Parse
                            |> fun x -> (x :?> System.Windows.Application)
                            |> fun x -> x.MainWindow.DataContext <- data
                                        x.MainWindow.Show()
                                        x.Run()
                            |> ignore
/// __________________________________________________________________________________________________________