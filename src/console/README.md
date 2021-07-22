# Console wasm sample

The sample shows how to build a small hello world wasm sample that links to a native external library.

The source code of the native library can be found at <https://github.com/emepetres/wasm-native-library-sandbox>.

## Setup

1. Download and extract latest .Net SDK from <https://aka.ms/dotnet/6.0/daily/dotnet-sdk-win-x64.zip>
1. `[RUNTIME_PATH]\dotnet.exe workload install wasm-tools`

## Build

Go to _src\console_ folder and run

`[RUNTIME_PATH]\dotnet.exe publish` or

`[RUNTIME_PATH]\dotnet.exe publish -v diag > publish.log`

## Run

Execute a static web server from _src\console\bin\Debug\net6.0\publish\wwwroot_
