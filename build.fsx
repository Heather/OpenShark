#I @"packages/FAKE/tools/"
#r @"FakeLib.dll"

open Fake
open System
open System.Linq

let buildDir = "./bin"
let nugetDir = "./.nuget"
let packagesDir = "./packages"

Target "Clean" (fun _ -> CleanDirs [buildDir])

Target "RestorePackages" RestorePackages

Target "GenResources" (fun _ ->
    let programFiles = 
        if (IntPtr.Size = 4)
            then Environment.GetEnvironmentVariable("ProgramFiles")
            else Environment.GetEnvironmentVariable("ProgramFiles(x86)")
    let result =
        let SDKs = ["8.1A"; "8.0A"; "7.1A"; "7.0A"]
        let RCs = [for sdk in SDKs do
                    if System.IO.File.Exists <| programFiles + @"\Microsoft SDKs\Windows\v" + sdk + @"\bin\RC.Exe"
                        then yield programFiles + @"\Microsoft SDKs\Windows\v" + sdk + @"\bin\RC.Exe"]
        if RCs.Count() > 0 then
            let rc = RCs.[0]
            ExecProcess (fun info -> 
                info.FileName   <- rc
                info.Arguments  <- "OpenShark.rc"
            ) (TimeSpan.FromMinutes 1.0)     
        else failwith "No rc.exe found, check http://download.microsoft.com/download/F/1/3/F1300C9C-A120-4341-90DF-8A52509B23AC/standalonesdk/sdksetup.exe"
    if result <> 0 then failwith "Failed to generate icon resource"
)

Target "BuildSolution" (fun _ ->
    MSBuildWithDefaults "Build" ["./OpenShark.fsproj"]
    |> Log "AppBuild-Output: "
)

"BuildSolution" <== ["Clean"; "RestorePackages"; "GenResources"]

RunTargetOrDefault "BuildSolution"
