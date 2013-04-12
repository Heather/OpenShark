namespace OpenShark
/// __________________________________________________________________________________________________________
open System.Reflection; open System.Drawing
/// __________________________________________________________________________________________________________
[<AutoOpen>]
module Resources =    
    /// ______________________________________________________________________________________________________
    type IconResource (name : string) =
        let loadIconResource name =
            #if __MonoCS__
            let pths = [
                "../../";
                "../"
                ]
            let rec tryLoad(x) =
                let p = 
                    if x = -1 then  name                        
                    else pths.[x] + name
                if File.Exists p then
                    new Icon(p : string)
                else 
                    if x < pths.Length  then tryLoad(x + 1)
                    else null
            tryLoad -1
            #else
            let assm = Assembly.GetExecutingAssembly()
            let res = assm.GetManifestResourceStream(name)
            new Icon(res)
            #endif

        let image = loadIconResource name

        member __.Name = name
        member __.Icon = image
    /// ______________________________________________________________________________________________________
    [<RequireQualifiedAccess>]
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>] 
    module IconResource =
        let OpenSharkIcon = IconResource "OpenShark.ico"
/// __________________________________________________________________________________________________________