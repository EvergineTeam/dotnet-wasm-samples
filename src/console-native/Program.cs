using System;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

[AttributeUsage(AttributeTargets.Method)]
sealed class MonoPInvokeCallbackAttribute : Attribute
{
    public MonoPInvokeCallbackAttribute(Type t) { }
}

public class Test
{
    public static int Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.Build().RunAsync();

        Console.WriteLine($"Hello from .Net6!");
        for (int i = 0; i < args.Length; i++)
        {
            Console.WriteLine($"args[{i}] = {args[i]}");
        }

        NativeBinding.callCbk(cbk);

        return args.Length;
    }

    [MonoPInvokeCallback(typeof(NativeBinding.callback))]
    public static void cbk()
    {
        Console.WriteLine($"Hello from .Net6.0 callback!");
    }
}