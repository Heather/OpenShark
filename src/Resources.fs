namespace OpenShark

open System.Reflection; open System.Drawing

[<AutoOpen>]
module Resources =    

    type IconResource (name : string) =
        let loadIconResource name =
            let assm = Assembly.GetExecutingAssembly()
            let res = assm.GetManifestResourceStream(name)
            new Icon(res)

        let image = loadIconResource name

        member __.Name = name
        member __.Icon = image

    [<RequireQualifiedAccess>]
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>] 
    module IconResource =
        let OpenSharkIcon = IconResource "OpenShark.ico"
