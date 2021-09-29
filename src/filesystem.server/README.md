# Production server sample

The sample shows how to publish a wasm app on production with file compression.

## Prerequisites

- (Optional - Recommended) Visual Studio 2022
- (Required without VS2022) [Download](https://github.com/dotnet/installer#installers-and-binaries) latest dotnet nightly SDK release.
- Install wasm-tools (root terminal): `dotnet workload install wasm-tools --skip-manifest-update`

## Build

Use VS2022 _src/filesystem-server.sln_ solution to build tasks project, then build the server project. You can also use VSCode/Terminal:

`dotnet build src\filesystem.tasks\Wasm.FileSystem.Tasks.csproj`
`dotnet build -c [Debug|Release] src\filesystem\Wasm.FileSystem.Server.csproj`

## Run

From VS2022 you can run the profile `Wasm.FileSystem.Server`. Additionally you can publish the app

`dotnet publish -c [Debug|Release] src\filesystem\Wasm.FileSystem.Sample.csproj`

and run the app by populating the folder _src\filesystem\bin\[Debug|Release]\net6.0\publish\wwwroot_.

In this second case we do recommend to use VSCode Live Server, instead of Fenix, as the second has known issues with Web Assembly.

## Publish

From VS2022 you can publish the Server project to an Azure Web App. You will need to publish it as a self-contained app, because .Net6 rc2 is not supported for now.
Alternatively you can publish the app using VSCode/Terminal:

`dotnet publish -c Release -r win-x86 --self-contained src\filesystem.server\Wasm.FileSystem.Server.csproj`

### Debug

Debug is in an experimental phase and currently some workarounds are needed to make it work.

1. Install Visual Studio 2022.
1. [Install latest rc2 sdk](https://aka.ms/dotnet/6.0.1XX-rc2/daily/dotnet-sdk-win-x64.exe).
1. Go to `C:\Program Files\dotnet\packs\Microsoft.NET.Runtime.MonoTargets.Sdk` and copy the folder `6.0.0-rc2.X.X/tasks/net472` to `6.0.0-rtm.X/tasks/net472`
1. Finally clean and rebuild the projects from VS2022. After that you will be able to put a break point and debug your Wasm .Net6 app.
