#r @"./packages/tools/FAKE/tools/FakeLib.dll"
open System
open System.IO
open Fake
open Fake.AssemblyInfoFile
open Fake.FixieHelper
open Fake.ArchiveHelper.Tar

let version = "0.1"

let artifactsDir = "artifacts"
let appArtifactsDir = artifactsDir @@ "app"
let testsArtifactsDir = artifactsDir @@ "tests"
let runtimeArtifactsDir = artifactsDir @@ "runtime"
let executableName = "App.exe"
let runtimeExecutableName = "app"
let runtimeTarName = "runtime.tar"

Target "Clean" (fun _ ->
    CleanDirs [ artifactsDir ]
)

Target "UpdateAssemblyInfo" (fun _ ->
    UpdateAttributes "./src/Properties/AssemblyInfo.cs"
        [ Attribute.Version version
          Attribute.FileVersion version ]
)

Target "Build" (fun _ ->
    let buildProps = [ "Configuration", "Release"; "DefineConstants", "TRACE" ]

    let buildProject projectPath outputPath =
        MSBuild outputPath "Build" buildProps [ projectPath ]
        |> Log "Build Artifact: "

    [ "./src/App.csproj", appArtifactsDir
      "./tests/App.UnitTests.csproj", testsArtifactsDir ]
    |> Map.ofList
    |> Map.iter buildProject
)

Target "MakeAppBundle" (fun _ ->
    let dlls =
        filesInDirMatching "*.dll" (DirectoryInfo(appArtifactsDir))
        |> Array.map (fun fi -> fi.Name)
        |> String.concat " "

    let args =
        [ "--deps"
          "--cross default"
          "-o " + runtimeExecutableName
          executableName
          dlls ]
        |> String.concat " "

    let result =
        ExecProcess (fun info ->
            info.FileName <- "mkbundle"
            info.WorkingDirectory <- appArtifactsDir
            info.Arguments <- args) (TimeSpan.FromMinutes 1.0)

    if result <> 0 then failwithf "`mkbundle` returned with a non-zero exit code"
)

Target "PackRuntime" (fun _ ->
    let runtimeDeps = [
            "/lib/ld-musl-x86_64.so.1"
            "/usr/lib/libgcc_s.so.1"
            "/usr/lib/libMonoPosixHelper.so"
            "/lib/libz.so.1"
            "/etc/mono/4.5/machine.config" ]

    runtimeDeps
        |> Seq.iter (fun file ->
            CopyFileWithSubfolder "/" runtimeArtifactsDir file)

    CopyFile runtimeArtifactsDir (appArtifactsDir @@ runtimeExecutableName)

    // It produces a .tar archive that is not supported by Docker :(
    // 
    // CompressDirWithDefaults
        // (directoryInfo runtimeArtifactsDir)
        // (fileInfo (artifactsDir @@ runtimeTarName))

    let args =
        [ "cvp"
          "-f " + ".." @@ runtimeTarName
          "." ]
        |> String.concat " "

    let result =
        ExecProcess (fun info ->
            info.FileName <- "tar"
            info.WorkingDirectory <- runtimeArtifactsDir
            info.Arguments <- args) (TimeSpan.FromMinutes 1.0)

    if result <> 0 then failwithf "`tar` returned with a non-zero exit code"
)

Target "Test" (fun _ ->
    !! (testsArtifactsDir @@ "*Tests.dll") |> Fixie id
)

Target "Default" DoNothing

"Clean"
    ==> "UpdateAssemblyInfo"
    ==> "Build"
    ==> "Test"
    ==> "MakeAppBundle"
    ==> "PackRuntime"
    ==> "Default"

RunTargetOrDefault "Default"
