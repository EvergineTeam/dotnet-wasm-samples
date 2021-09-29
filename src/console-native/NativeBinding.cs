using System;
using System.Runtime.InteropServices;

public static class NativeBinding
{
    private const string DllName = "libWasmNative";
    public delegate void callback();

    [DllImport(DllName)]
    public static extern int hello();

    [DllImport(DllName)]
    public static extern unsafe void callCbk(callback cbk);

    // [DllImport(DllName)]
    // public static extern unsafe void callCbk(delegate*<void> cbk);
}