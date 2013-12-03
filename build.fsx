#I @"packages/FAKE/tools/"
#r @"FakeLib.dll"

open Fake
open System

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
        ExecProcess (fun info -> 
            info.FileName   <- programFiles + @"\Microsoft SDKs\Windows\v7.0A\bin\RC.Exe"
            info.Arguments  <- "OpenShark.rc"
        ) (TimeSpan.FromMinutes 1.0)     
    if result <> 0 then failwith "Failed to register FSProtocol.dll"
)

Target "BuildSolution" (fun _ ->
    MSBuildWithDefaults "Build" ["./OpenShark.fsproj"]
    |> Log "AppBuild-Output: "
)

"BuildSolution" <== ["Clean"; "RestorePackages"; "GenResources"]

RunTargetOrDefault "BuildSolution"