// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;

[AttributeUsage(AttributeTargets.Method)]
sealed class MonoPInvokeCallbackAttribute : Attribute
{
    public MonoPInvokeCallbackAttribute(Type t) { }
}

public class Test
{
    public static unsafe int Main(string[] args)
    {
        //await Task.Delay(1);
        Console.WriteLine("Hello World!");
        for (int i = 0; i < args.Length; i++)
        {
            Console.WriteLine($"args[{i}] = {args[i]}");
        }

        Console.WriteLine($"The answer to everything is {NativeBinding.hello()}");

        NativeBinding.callCbk(cbk);
        //NativeBinding.callCbk(&cbk);

        return args.Length;
    }

    [MonoPInvokeCallback(typeof(NativeBinding.callback))]
    public static void cbk()
    {
        Console.WriteLine($"Hello from .Net6.0 callback!");
    }
}