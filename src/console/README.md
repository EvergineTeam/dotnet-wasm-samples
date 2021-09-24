# Console wasm sample

The sample shows how to build a small hello world wasm sample that uses a native external library that makes a callback to .Net

The source code of the native library can be found at <https://github.com/emepetres/wasm-native-library-sandbox>.

## Setup

1. Download and install latest .Net SDK from <https://dotnet.microsoft.com/download/dotnet/6.0>
1. `dotnet workload install wasm-tools --skip-manifest-update`

## Build

Go to _src\console_ folder and run

`dotnet publish -c [Debug|Release]` or

`dotnet publish -c [Debug|Release] -v diag > publish.log`

## Run

Execute a static web server from _src\console\bin\[Debug|Release]\net6.0\publish\wwwroot_
