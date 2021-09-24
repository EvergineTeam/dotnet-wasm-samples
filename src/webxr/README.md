# WebXR wasm sample

The sample shows how to build a small webxr hello world wasm sample on .Net.
It does not render anything, just reads the device position, orientation and projection matrix.

The source code of the webxr native library is not published yet, but it will.

## Setup

1. Download and install latest .Net SDK from <https://dotnet.microsoft.com/download/dotnet/6.0>
1. `dotnet workload install wasm-tools --skip-manifest-update`

## Build

Go to _src\webxr_ folder and run

`dotnet publish -c [Debug|Release]` or

`dotnet publish -c [Debug|Release] -v diag > publish.log`

## Run

Execute a static web server from _src\webxr\bin\[Debug|Release]\net6.0\publish\wwwroot_
