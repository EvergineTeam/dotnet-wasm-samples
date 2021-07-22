# WebXR wasm sample

The sample shows how to build a small webxr hello world wasm sample on .Net.
It does not render anything, just reads the device position, orientation and projection matrix.

The source code of the webxr native library is not published yet, but it will.

## Setup

1. Download and extract latest .Net SDK from <https://aka.ms/dotnet/6.0/daily/dotnet-sdk-win-x64.zip>
1. `[RUNTIME_PATH]\dotnet.exe workload install wasm-tools`

## Build

Go to _src\webxr_ folder and run

`[RUNTIME_PATH]\dotnet.exe publish` or

`[RUNTIME_PATH]\dotnet.exe publish -v diag > publish.log`

## Run

Execute a static web server from _src\webxr\bin\Debug\net6.0\publish\wwwroot_
