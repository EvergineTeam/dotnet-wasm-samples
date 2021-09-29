# Console native wasm sample

The sample shows how to build a small hello world wasm sample that uses a native external library that makes a callback to .Net

The source code of the native library can be found at <https://github.com/emepetres/wasm-native-library-sandbox>.

## Prerequisites

- (Optional - Recommended) Visual Studio 2022
- (Required without VS2022) [Download](https://github.com/dotnet/installer#installers-and-binaries) latest dotnet nightly SDK release.
- Install wasm-tools (root terminal): `dotnet workload install wasm-tools --skip-manifest-update`

## Build

Use VS2022 _src/console-native.sln_ solution or VSCode/Terminal.

`dotnet build -c [Debug|Release]`

## Run

From VS2022 you can run the profile `Wasm.ConsoleNative.Sample`. Additionally you can publish the app

`dotnet publish -c [Debug|Release]`

and run the app by populating the folder _src\console-native\bin\[Debug|Release]\net6.0\publish\wwwroot_.

In this second case we do recommend to use VSCode Live Server, instead of Fenix, as the second has known issues with Web Assembly.

### Debug

Debug is in an experimental phase and currently some workarounds are needed to make it work.

1. Install Visual Studio 2022.
1. [Install latest rc2 sdk](https://aka.ms/dotnet/6.0.1XX-rc2/daily/dotnet-sdk-win-x64.exe).
1. Go to `C:\Program Files\dotnet\packs\Microsoft.NET.Runtime.MonoTargets.Sdk` and copy the folder `6.0.0-rc2.X.X/tasks/net472` to `6.0.0-rtm.X/tasks/net472`
1. Finally clean and rebuild the projects from VS2022. After that you will be able to put a break point and debug your Wasm .Net6 app.
