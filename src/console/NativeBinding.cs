using System;
using System.Runtime.InteropServices;

public static class NativeBinding
{
    private const string DllName = "libWasmNative";

    [DllImport(DllName)]
    public static extern int hello();
}