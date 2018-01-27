#r @"packages/FAKE/tools/FakeLib.dll"

open System.IO
open Fake
open Fake.Core
open Fake.Core.Trace
open Fake.DotNet
open Fake.Core.TargetOperators
open Fake.DotNet.Cli

let sourcePath = Path.GetFullPath "./src/"
let solutionPath = Path.GetFullPath "./src/Project.sln"
let dependenciesPath = Path.GetFullPath "./src/packages/"
let testsPath = Path.GetFullPath "./src/UnitTests/UnitTests.fsproj"

Target.Create "InstallDotnetCore" (fun _ -> 
    Cli.DotnetCliInstall Cli.Release_2_0_0
)

Target.Create "CheckVersion" (fun _ ->
    let dotnetVersion = "2.1.4"
    let dotnetCoreIsNotInstalled = 
        DotNetCli.isInstalled () 
        |> not

    printfn "installed: %A" dotnetCoreIsNotInstalled
    let needInstall =
        if dotnetCoreIsNotInstalled then 
            true
        elif DotNetCli.getVersion () <> dotnetVersion then 
            true
        else 
            false

    let result = 
        if needInstall then 
            DotNetCli.InstallDotNetSDK dotnetVersion
            |> (+) "dotnet core installed on: "
        else
            "dotnet core already installed"

    trace result
)

Target.Create "Test" (fun _ -> 
    DotNetCli.Test 
        (fun p ->
            { p with 
                Configuration = "Release"
                AdditionalArgs = [ testsPath ; "--no-restore"; "--no-build" ]})
)

Target.Create "Restore" (fun _ ->
    DotNetCli.Restore 
        (fun p -> 
            { p with 
                NoCache = true;
                AdditionalArgs = [ solutionPath ; " --packages " + dependenciesPath ] })
)

Target.Create "Build" (fun _ ->
    DotNetCli.Build
        (fun p -> 
            { p with 
                Configuration = "Release";
                Project = solutionPath;
                AdditionalArgs = [ "--no-restore" ]})
)

Target.Create "Default" (fun _ -> 
    trace "It's done, go get a coffee and enjoy your life"
)

"CheckVersion"
==> "Restore"
==> "Build"
==> "Test"
==> "Default" 

Target.RunOrDefault "Default"
